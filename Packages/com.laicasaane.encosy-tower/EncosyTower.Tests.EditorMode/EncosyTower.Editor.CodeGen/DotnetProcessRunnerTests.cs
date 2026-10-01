using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.CodeGen;
using EncosyTower.Editor.CodeGen;
using NUnit.Framework;

namespace EncosyTower.Tests.Editor.CodeGen;

[TestFixture]
[Category("Editor.CodeGen")]
public sealed class DotnetProcessRunnerTests
{
    private string _testRoot;
    private string _workspaceRoot;

    [SetUp]
    public void SetUp()
    {
        _testRoot = Path.GetFullPath(
            Path.Combine("Library", "EncosyTower.CodeGen.ProcessTests", Guid.NewGuid().ToString("N"))
        );
        _workspaceRoot = _testRoot;
        Directory.CreateDirectory(_testRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, recursive: true);
        }
    }

    [Test]
    public void CreateStartInfo_UsesArgumentListWithoutShell()
    {
        var request = CreateRequest(
              FindDotnetExecutable()
            , new[] { "--info", _workspaceRoot }
        );

        var startInfo = DotnetProcessRunner.CreateStartInfo(request);

        Assert.That(startInfo.UseShellExecute, Is.False);
        Assert.That(startInfo.Arguments, Is.Empty);
        Assert.That(startInfo.ArgumentList, Is.EqualTo(request.Arguments));
        Assert.That(startInfo.WorkingDirectory, Is.EqualTo(_workspaceRoot));
    }

    [Test]
    public void ProcessManifest_RoundTripsExactVersionedIdentity()
    {
        var manifestPath = GetManifestPath();
        var rootProcess = new DotnetProcessManifest.ProcessEntry(
              processId: 42
            , executablePath: FindDotnetExecutable()
            , commandLine: $"dotnet --project \"{_workspaceRoot}\""
            , startTimeUtcTicks: DateTime.UtcNow.Ticks
        );
        var document = DotnetProcessManifest.Create(_workspaceRoot, "Prepare", _workspaceRoot, rootProcess);

        DotnetProcessManifest.WriteAtomic(manifestPath, document, _workspaceRoot);

        var result = DotnetProcessManifest.Read(manifestPath, _workspaceRoot);
        var json = File.ReadAllText(manifestPath);

        Assert.That(result.schemaVersion, Is.EqualTo(1));
        Assert.That(result.workspaceRoot, Is.EqualTo(_workspaceRoot));
        Assert.That(result.operationName, Is.EqualTo("Prepare"));
        Assert.That(result.processes, Has.Length.EqualTo(1));
        Assert.That(json, Does.Contain("\"schemaVersion\": 1"));
        Assert.That(json, Does.Contain("\"rootProcessId\": 42"));
    }

    [Test]
    public void ProcessManifest_RejectsMalformedAndUnknownVersions()
    {
        var manifestPath = GetManifestPath();
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
        File.WriteAllText(manifestPath, "{", Encoding.UTF8);

        var malformed = Assert.Throws<CodeGenRunException>(
            () => DotnetProcessManifest.Read(manifestPath, _workspaceRoot)
        );

        Assert.That(malformed.Code, Is.EqualTo("ENCOSY_CODEGEN_PROCESS_MANIFEST_MALFORMED"));

        File.WriteAllText(
              manifestPath
            , "{"
                + "\"schemaVersion\":2,"
                + $"\"workspaceRoot\":\"{EscapeJson(_workspaceRoot)}\","
                + "\"operationName\":\"Prepare\","
                + $"\"workingDirectory\":\"{EscapeJson(_workspaceRoot)}\","
                + "\"rootProcessId\":42,"
                + "\"processes\":[{"
                    + "\"processId\":42,"
                    + $"\"executablePath\":\"{EscapeJson(FindDotnetExecutable())}\","
                    + $"\"commandLine\":\"dotnet {EscapeJson(_workspaceRoot)}\","
                    + $"\"startTimeUtcTicks\":{DateTime.UtcNow.Ticks}"
                + "}]}"
            , Encoding.UTF8
        );

        var unknownVersion = Assert.Throws<CodeGenRunException>(
            () => DotnetProcessManifest.Read(manifestPath, _workspaceRoot)
        );

        Assert.That(unknownVersion.Code, Is.EqualTo("ENCOSY_CODEGEN_PROCESS_MANIFEST_VERSION"));
    }

    [Test]
    public async Task RunAsync_LaunchesWithSpacesAndUnicodeAndCleansManifest()
    {
        var nestedRoot = Path.Combine(_workspaceRoot, "space name", "unicode-台");
        Directory.CreateDirectory(nestedRoot);
        _workspaceRoot = nestedRoot;

        var request = CreateRequest(
              FindDotnetExecutable()
            , new[] { "--info", _workspaceRoot }
        );
        var outputs = new List<DotnetProcessOutput>();
        var runner = new DotnetProcessRunner();

        var result = await runner.RunAsync(request, outputs.Add);

        Assert.That(result.ProcessId, Is.GreaterThan(0));
        Assert.That(outputs, Is.Not.Empty);
        Assert.That(File.Exists(request.StandardOutputLogPath), Is.True);
        Assert.That(File.Exists(request.StandardErrorLogPath), Is.True);
        Assert.That(File.Exists(request.ProcessManifestPath), Is.False);
    }

    [Test]
    public async Task RunAsync_PreservesStdoutStderrFloodAndNonzeroExit()
    {
        RequireWindows();

        var powershellPath = FindPowerShellExecutable();
        var escapedRoot = EscapePowerShellLiteral(_workspaceRoot);
        var script = $"$null = '{escapedRoot}'; "
            + "for ($i = 0; $i -lt 2048; $i++) { "
            + "[Console]::Out.WriteLine(\"OUT-$i\"); "
            + "[Console]::Error.WriteLine(\"ERR-$i\") }; exit 23";
        var request = CreateRequest(powershellPath, CreatePowerShellArguments(script));
        var outputs = new List<DotnetProcessOutput>();
        var runner = new DotnetProcessRunner();

        var result = await runner.RunAsync(request, outputs.Add);
        var standardOutput = File.ReadAllText(request.StandardOutputLogPath);
        var standardError = File.ReadAllText(request.StandardErrorLogPath);

        Assert.That(result.ExitCode, Is.EqualTo(23));
        Assert.That(outputs, Has.Count.EqualTo(4096));
        Assert.That(standardOutput, Does.Contain("OUT-0"));
        Assert.That(standardOutput, Does.Contain("OUT-2047"));
        Assert.That(standardError, Does.Contain("ERR-0"));
        Assert.That(standardError, Does.Contain("ERR-2047"));
        Assert.That(File.Exists(request.ProcessManifestPath), Is.False);
    }

    [Test]
    public async Task RunAsync_CancellationKillsRecordedChildAfterRootExited()
    {
        RequireWindows();

        var powershellPath = FindPowerShellExecutable();
        var childScriptPath = Path.Combine(_workspaceRoot, "child.ps1");
        var parentScriptPath = Path.Combine(_workspaceRoot, "parent.ps1");
        File.WriteAllText(
              childScriptPath
            , $"Set-Location -LiteralPath '{EscapePowerShellLiteral(_workspaceRoot)}'\n" + "Start-Sleep -Seconds 120\n"
            , Encoding.UTF8
        );
        File.WriteAllText(
              parentScriptPath
            , $"$child = Start-Process -FilePath '{EscapePowerShellLiteral(powershellPath)}' "
                + "-ArgumentList '-NoLogo','-NoProfile','-NonInteractive','-File',"
                + $"'{EscapePowerShellLiteral(childScriptPath)}' -WindowStyle Hidden -PassThru\n"
                + "Write-Output \"CHILD=$($child.Id)\"\n"
                + "Start-Sleep -Milliseconds 500\n"
                + "exit 0\n"
            , Encoding.UTF8
        );

        var request = CreateRequest(
              powershellPath
            , new[] {
                  "-NoLogo"
                , "-NoProfile"
                , "-NonInteractive"
                , "-File"
                , parentScriptPath
            }
        );
        var runner = new DotnetProcessRunner();
        using var cancellation = new CancellationTokenSource();
        var childProcessId = 0;
        var callbackCount = 0;
        var runTask = runner.RunAsync(
              request
            , output =>
            {
                callbackCount++;

                if (output.Text.StartsWith("CHILD=", StringComparison.Ordinal))
                {
                    childProcessId = int.Parse(
                          output.Text["CHILD=".Length..]
                        , System.Globalization.CultureInfo.InvariantCulture
                    );
                }
            }
            , cancellation.Token
        );

        try
        {
            await WaitUntilAsync(
                  () => childProcessId > 0
                    && IsManifestTrackingDescendant(request.ProcessManifestPath, _workspaceRoot)
                    && IsProcessAlive(request.ProcessManifestPath, _workspaceRoot) == false
                , TimeSpan.FromSeconds(10)
            );
            cancellation.Cancel();

            try
            {
                await runTask;
                Assert.Fail("The canceled process task completed successfully.");
            }
            catch (OperationCanceledException)
            {
                // Expected cancellation path.
            }

            var completedCallbackCount = callbackCount;
            await Task.Delay(100);

            Assert.That(IsProcessAlive(childProcessId), Is.False);
            Assert.That(File.Exists(request.ProcessManifestPath), Is.False);
            Assert.That(callbackCount, Is.EqualTo(completedCallbackCount));
        }
        finally
        {
            cancellation.Cancel();

            try
            {
                await runTask;
            }
            catch (OperationCanceledException)
            {
                // Expected cleanup path.
            }
            catch (CodeGenRunException)
            {
                // The assertion above retains the original failure evidence.
            }

            if (File.Exists(request.ProcessManifestPath))
            {
                await runner.StopRecordedProcessTreeAsync(_workspaceRoot, request.ProcessManifestPath);
            }
        }
    }

    [Test]
    public void StopRecordedProcessTreeAsync_RejectsUnrelatedLiveProcess()
    {
        using var currentProcess = Process.GetCurrentProcess();
        var manifestPath = GetManifestPath();
        var unrelated = new DotnetProcessManifest.ProcessEntry(
              currentProcess.Id
            , FindDotnetExecutable()
            , $"unrelated {_workspaceRoot}"
            , currentProcess.StartTime.ToUniversalTime().Ticks
        );
        var document = DotnetProcessManifest.Create(_workspaceRoot, "Prepare", _workspaceRoot, unrelated);
        DotnetProcessManifest.WriteAtomic(manifestPath, document, _workspaceRoot);
        var runner = new DotnetProcessRunner();

        var exception = Assert.ThrowsAsync<CodeGenRunException>(
            async () => await runner.StopRecordedProcessTreeAsync(_workspaceRoot, manifestPath)
        );

        Assert.That(
              exception.Code == "ENCOSY_CODEGEN_PROCESS_IDENTITY"
                || exception.Code == "ENCOSY_CODEGEN_WORKSPACE_LOCKED"
            , Is.True
        );
        Assert.That(currentProcess.HasExited, Is.False);
        Assert.That(File.Exists(manifestPath), Is.True);
    }

    [Test]
    public async Task StopRecordedProcessTreeAsync_RemovesFullyExitedManifest()
    {
        var manifestPath = GetManifestPath();
        var exited = new DotnetProcessManifest.ProcessEntry(
              int.MaxValue
            , FindDotnetExecutable()
            , $"dotnet {_workspaceRoot}"
            , DateTime.UtcNow.Ticks
        );
        var document = DotnetProcessManifest.Create(_workspaceRoot, "Prepare", _workspaceRoot, exited);
        DotnetProcessManifest.WriteAtomic(manifestPath, document, _workspaceRoot);
        var runner = new DotnetProcessRunner();

        await runner.StopRecordedProcessTreeAsync(_workspaceRoot, manifestPath);

        Assert.That(File.Exists(manifestPath), Is.False);
    }

    [Test]
    public void Progress_FinishAlwaysRemovesRunningStateAndQueuedOutput()
    {
        var progress = new DotnetCodeGenProgress();
        progress.Start();

        try
        {
            progress.Enqueue(new DotnetProcessOutput(false, "stdout"));
            progress.Enqueue(new DotnetProcessOutput(true, "stderr"));
            progress.Drain();
        }
        finally
        {
            progress.Finish();
        }

        Assert.That(progress.IsRunning, Is.False);
        Assert.That(progress.PendingOutputCount, Is.Zero);
    }

    [Test]
    public async Task Backend_StopsBeforeCreateRunsExactPrepareReadsResultAndCleansProgress()
    {
        var sequence = new List<string>();
        var request = CreateRequest(
              FindDotnetExecutable()
            , new[] { "--info", _workspaceRoot }
        );
        var expectedBatch = new GeneratedCodeBatch(
              Array.Empty<GeneratedCode>()
            , Array.Empty<CodeGenDiagnostic>()
            , AllCandidatesSkipped: false
        );
        var workspace = CreateWorkspace(request);
        DotnetProcessRequest runRequest = null;
        var progress = new DotnetCodeGenProgress();
        var backend = new DotnetCodeGenBackend(
              token => Stop(token, sequence)
            , token => CreateWorkspaceAsync(token, workspace, sequence)
            , (actualRequest, onOutput, token) => RunProcessAsync(
                  actualRequest
                , onOutput
                , token
                , new DotnetProcessResult(42, 0)
                , sequence
                , request => runRequest = request
              )
            , actualWorkspace => ReadResult(actualWorkspace, workspace, expectedBatch, sequence)
            , progress
        );

        var actualBatch = await backend.GenerateAsync();

        Assert.That(sequence, Is.EqualTo(new[] { "stop", "create", "run", "read" }));
        Assert.That(runRequest, Is.SameAs(request));
        Assert.That(actualBatch.GeneratedCodes, Is.SameAs(expectedBatch.GeneratedCodes));
        Assert.That(actualBatch.Diagnostics, Is.SameAs(expectedBatch.Diagnostics));
        Assert.That(progress.IsRunning, Is.False);
        Assert.That(progress.PendingOutputCount, Is.Zero);
    }

    [Test]
    public async Task Backend_NonzeroExitIsActionableAndCleansProgress()
    {
        var sequence = new List<string>();
        var request = CreateRequest(
              FindDotnetExecutable()
            , new[] { "--info", _workspaceRoot }
        );
        var workspace = CreateWorkspace(request);
        var expectedBatch = new GeneratedCodeBatch(
              Array.Empty<GeneratedCode>()
            , Array.Empty<CodeGenDiagnostic>()
            , AllCandidatesSkipped: false
        );
        var progress = new DotnetCodeGenProgress();
        var backend = new DotnetCodeGenBackend(
              token => Stop(token, sequence)
            , token => CreateWorkspaceAsync(token, workspace, sequence)
            , (actualRequest, onOutput, token) => RunProcessAsync(
                  actualRequest
                , onOutput
                , token
                , new DotnetProcessResult(42, 17)
                , sequence
                , static _ => { }
              )
            , actualWorkspace => ReadResult(actualWorkspace, workspace, expectedBatch, sequence)
            , progress
        );

        CodeGenRunException exception;

        try
        {
            await backend.GenerateAsync();
            Assert.Fail("The nonzero process result completed successfully.");
            return;
        }
        catch (CodeGenRunException caught)
        {
            exception = caught;
        }

        Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_DOTNET_PROCESS_FAILED"));
        Assert.That(exception.Message, Does.Contain("exited with code 17"));
        Assert.That(exception.Message, Does.Contain(workspace.SelectedSdkVersion));
        Assert.That(exception.Message, Does.Contain(request.StandardOutputLogPath));
        Assert.That(exception.Message, Does.Contain(request.StandardErrorLogPath));
        Assert.That(sequence, Is.EqualTo(new[] { "stop", "create", "run" }));
        Assert.That(progress.IsRunning, Is.False);
        Assert.That(progress.PendingOutputCount, Is.Zero);
    }

    private static Task Stop(CancellationToken token, List<string> sequence)
    {
        token.ThrowIfCancellationRequested();
        sequence.Add("stop");
        return Task.CompletedTask;
    }

    private static Task<DotnetWorkspace> CreateWorkspaceAsync(
          CancellationToken token
        , DotnetWorkspace workspace
        , List<string> sequence
    )
    {
        token.ThrowIfCancellationRequested();
        sequence.Add("create");
        return Task.FromResult(workspace);
    }

    private static Task<DotnetProcessResult> RunProcessAsync(
          DotnetProcessRequest request
        , Action<DotnetProcessOutput> onOutput
        , CancellationToken token
        , DotnetProcessResult result
        , List<string> sequence
        , Action<DotnetProcessRequest> captureRequest
    )
    {
        token.ThrowIfCancellationRequested();
        captureRequest(request);
        sequence.Add("run");
        onOutput(new DotnetProcessOutput(false, "process output"));
        return Task.FromResult(result);
    }

    private static GeneratedCodeBatch ReadResult(
          DotnetWorkspace workspace
        , DotnetWorkspace expectedWorkspace
        , GeneratedCodeBatch batch
        , List<string> sequence
    )
    {
        Assert.That(workspace, Is.SameAs(expectedWorkspace));
        sequence.Add("read");
        return batch;
    }

    private DotnetProcessRequest CreateRequest(string executablePath, string[] arguments)
        => new(
              OperationName: "Prepare"
            , executablePath
            , WorkingDirectory: _workspaceRoot
            , arguments
            , StandardOutputLogPath: Path.Combine(_workspaceRoot, "logs", "stdout.log")
            , StandardErrorLogPath: Path.Combine(_workspaceRoot, "logs", "stderr.log")
            , ProcessManifestPath: GetManifestPath()
        );

    private string GetManifestPath()
        => Path.Combine(_workspaceRoot, "manifests", "process.json");

    private DotnetWorkspace CreateWorkspace(DotnetProcessRequest request)
        => new(
              RootPath: _workspaceRoot
            , SelectedSdkVersion: "10.0.302"
            , PrepareRequest: request
            , ResultManifestPath: Path.Combine(_workspaceRoot, "manifests", "result.json")
            , ProcessManifestPath: request.ProcessManifestPath
        );

    private static string[] CreatePowerShellArguments(string script)
        => new[] {
              "-NoLogo"
            , "-NoProfile"
            , "-NonInteractive"
            , "-Command"
            , script
        };

    private static string FindDotnetExecutable()
        => FindExecutable(Path.DirectorySeparatorChar == '\\' ? "dotnet.exe" : "dotnet");

    private static string FindPowerShellExecutable()
        => FindExecutable("pwsh.exe", "powershell.exe");

    private static string FindExecutable(params string[] executableNames)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var paths = pathValue.Split(Path.PathSeparator);
        var pathCount = paths.Length;
        var executableCount = executableNames.Length;

        for (var pathIndex = 0; pathIndex < pathCount; pathIndex++)
        {
            for (var executableIndex = 0; executableIndex < executableCount; executableIndex++)
            {
                var candidate = Path.Combine(paths[pathIndex], executableNames[executableIndex]);

                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }
        }

        Assert.Fail($"Executable is unavailable: {string.Join(", ", executableNames)}.");
        return string.Empty;
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (predicate() == false)
        {
            if (DateTime.UtcNow >= deadline)
            {
                Assert.Fail($"Condition did not pass within {timeout}.");
            }

            await Task.Delay(25);
        }
    }

    private static bool IsProcessAlive(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.HasExited == false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool IsManifestTrackingDescendant(string manifestPath, string workspaceRoot)
    {
        try
        {
            return DotnetProcessManifest.Read(manifestPath, workspaceRoot).processes.Length >= 2;
        }
        catch (CodeGenRunException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool IsProcessAlive(string manifestPath, string workspaceRoot)
    {
        try
        {
            var rootProcessId = DotnetProcessManifest.Read(manifestPath, workspaceRoot).rootProcessId;
            return IsProcessAlive(rootProcessId);
        }
        catch (CodeGenRunException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private static string EscapeJson(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string EscapePowerShellLiteral(string value)
        => value.Replace("'", "''");

    private static void RequireWindows()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        {
            Assert.Ignore("Windows process-tree fixture.");
        }
    }

}
