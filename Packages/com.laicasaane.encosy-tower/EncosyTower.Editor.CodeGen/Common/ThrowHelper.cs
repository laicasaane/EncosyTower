using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Owns reusable exception contracts for Editor CodeGen.
    /// </summary>
    internal static class ThrowHelper
    {
        internal const string WRITE_STAGING_FAILED = "ENCOSY_CODEGEN_WRITE_STAGING_FAILED";
        internal const string WRITE_COMMIT_FAILED = "ENCOSY_CODEGEN_WRITE_COMMIT_FAILED";
        internal const string WRITE_REFRESH_FAILED = "ENCOSY_CODEGEN_WRITE_REFRESH_FAILED";
        internal const string WRITE_COMMIT_FAILED_MESSAGE = "Generated-code batch commit failed.";
        internal const string WRITE_REFRESH_FAILED_MESSAGE =
            "Unity asset refresh failed after generated-code commit.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentOutOfRangeException CreateProgressStageOutOfRangeException()
            => new("stage");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentException CreateProcessOutputTextNullException()
            => new("Process output text cannot be null.", "output");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException CreateProgressAlreadyRunningException()
            => new("The .NET CodeGen progress item is already running.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException CreateProgressNotRunningException()
            => new("The .NET CodeGen progress item is not running.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException CreateProgressMainThreadRequiredException()
            => new(".NET CodeGen progress must be updated on Unity's main thread.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateInvalidBatchException()
            => new(
                  "ENCOSY_CODEGEN_INVALID_BATCH"
                , "The code generation backend returned an invalid result batch."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateResultManifestMalformedException(string resultManifestPath)
            => new(
                  "ENCOSY_CODEGEN_RESULT_MALFORMED"
                , $"The result manifest '{resultManifestPath}' contains null arrays."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceFactoryReturnedNullException()
            => new(
                  "ENCOSY_CODEGEN_WORKSPACE_MALFORMED"
                , "The .NET workspace factory returned no workspace."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceContractMalformedException()
            => new(
                  "ENCOSY_CODEGEN_WORKSPACE_MALFORMED"
                , "The .NET workspace does not match the factory or Prepare process contract."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDotnetProcessFailedException(
              DotnetWorkspace workspace
            , DotnetProcessResult result
        )
        {
            var request = workspace.PrepareRequest;

            return new CodeGenRunException(
                  "ENCOSY_CODEGEN_DOTNET_PROCESS_FAILED"
                , $"The .NET CodeGen operation '{request.OperationName}' exited with code "
                    + $"{result.ExitCode} using SDK {workspace.SelectedSdkVersion}. "
                    + $"Command: {request.ExecutablePath}. "
                    + $"Inspect '{request.StandardOutputLogPath}' and "
                    + $"'{request.StandardErrorLogPath}'. Verify the installed SDK, runtimes, "
                    + "targeting packs, NuGet feeds, and offline cache."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestReadException(
              string processManifestPath
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_READ"
                , $"Cannot read the CodeGen process manifest '{processManifestPath}'."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestMalformedException(
              string processManifestPath
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_MALFORMED"
                , $"The CodeGen process manifest '{processManifestPath}' is malformed."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestPathException(string processManifestPath)
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_PATH"
                , $"The CodeGen process manifest path '{processManifestPath}' has no directory."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestWriteException(
              string processManifestPath
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_WRITE"
                , $"Cannot atomically write the CodeGen process manifest '{processManifestPath}'."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestDeleteException(
              string processManifestPath
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_DELETE"
                , $"Cannot remove the CodeGen process manifest '{processManifestPath}'."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestVersionException(int schemaVersion)
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_VERSION"
                , $"Unsupported CodeGen process manifest schema version '{schemaVersion}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestWorkspaceLockedException(
              string workspaceRoot
            , string expectedRoot
        )
            => new(
                  "ENCOSY_CODEGEN_WORKSPACE_LOCKED"
                , $"The process manifest belongs to '{workspaceRoot}', not '{expectedRoot}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestIdentityMalformedException()
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_MALFORMED"
                , "The CodeGen process manifest is missing required identity data."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateSelectedSdkVersionRequiredException()
            => new("ECG4101", "The selected .NET SDK version is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateUnityVersionRootMissingException(string applicationPath)
            => new("ECG4102", $"Cannot resolve the Unity version root from: {applicationPath}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateManifestPathRequiredException()
            => new("ECG4001", "A manifest path is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateManifestValueRequiredException()
            => new("ECG4002", "A manifest value is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateManifestParentMissingException(string fullPath)
            => new("ECG4003", $"The manifest path has no parent directory: {fullPath}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateResultManifestMissingException(string path)
            => new("ECG4004", $"The .NET result manifest does not exist: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateResultManifestMalformedException(
              string path
            , Exception innerException
        )
            => new("ECG4005", $"The .NET result manifest is malformed: {path}", innerException);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDuplicateResultPathException(string path)
            => new("ECG4006", $"The .NET result contains a duplicate output path: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateInvalidDiagnosticSeverityException(int severity)
            => new("ECG4007", $"The .NET result contains an invalid diagnostic severity: {severity}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProjectRootRequiredException()
            => new("ECG4008", "The Unity project root is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProjectPathRequiredException()
            => new("ECG4009", "A project path is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProjectPathOutsideRootException(string path)
            => new("ECG4010", $"The path is outside the Unity project: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProjectPathOutsideOwnedDirectoriesException(string path)
            => new("ECG4011", $"The path is not inside the project Assets or Packages directory: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateRequiredFileMissingException(string path)
            => new("ECG4012", $"The required file does not exist: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateInputManifestRequiredException()
            => new("ECG4013", "The compiler-input manifest is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateInputEnvironmentUnavailableException()
            => new("ECG4028", "The compiler-input environment paths are not available.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateInputAssemblyNullException(int index)
            => new("ECG4014", $"The compiler-input assembly at index {index} is null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreatePredefinedAssemblyHasAsmdefException(string assemblyName)
            => new("ECG4034", $"Predefined assembly '{assemblyName}' must have an empty asmdef path.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateAssemblyCompilerOptionsMissingException(string assemblyName)
            => new("ECG4015", $"Assembly '{assemblyName}' has no compiler options.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDuplicateInputAssemblyNameException(string assemblyName)
            => new("ECG4016", $"Duplicate compiler-input assembly name: {assemblyName}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateLastGoodAssemblyMissingException(string path)
            => new("ECG4029", $"The last-good assembly output does not exist: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDuplicateInputAssemblyOutputException(string path)
            => new("ECG4017", $"Duplicate compiler-input assembly output: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreatePredefinedAssemblySourceOutsideAssetsException(
              string assemblyName
            , string path
        )
            => new("ECG4035", $"Predefined assembly '{assemblyName}' has a source outside Assets: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateSourceOwnedByMultipleAssembliesException(string path)
            => new("ECG4018", $"A source file belongs to more than one assembly: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateMalformedReferenceException(string assemblyName, int index)
            => new("ECG4030", $"Assembly '{assemblyName}' has a malformed reference at index {index}.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateMissingOrDuplicateReferenceException(
              string assemblyName
            , string path
        )
            => new("ECG4031", $"Assembly '{assemblyName}' has a missing or duplicate reference: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateMissingOrDuplicateInputPathException(
              string kind
            , string path
        )
            => new("ECG4032", $"The compiler-input {kind} path is missing or duplicated: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateOptionalInputPathMissingException(string kind, string path)
            => new("ECG4033", $"The compiler-input {kind} path does not exist: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateResultManifestEmptyException()
            => new("ECG4019", "The .NET result manifest is empty.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateResultProjectRootMismatchException(string projectRoot)
            => new("ECG4020", $"The result project root does not match the current project: {projectRoot}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedResultNullException(int index)
            => new("ECG4021", $"The generated result at index {index} is null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedResultContentNullException(string path)
            => new("ECG4022", $"The generated result has null content: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDiagnosticMalformedException(int index)
            => new("ECG4023", $"The diagnostic at index {index} is malformed.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProjectFileReparsePointException(string path)
            => new("ECG4024", $"A project file is a reparse point: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProjectPathReparsePointException(string path)
            => new("ECG4024", $"A project path crosses a reparse point: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateUnsupportedManifestSchemaException(int value, string manifestName)
            => new(
                  "ECG4025"
                , $"Unsupported {manifestName} schema version {value}; "
                    + $"expected {DotnetWorkspaceProtocol.SCHEMA_VERSION}."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateManifestFieldRequiredException(string fieldName)
            => new("ECG4026", $"The manifest field '{fieldName}' is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateManifestArrayRequiredException(string fieldName)
            => new("ECG4027", $"The manifest array '{fieldName}' is required.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceResultRootUnexpectedException(string rootPath)
            => new("ECG4201", $"The workspace result belongs to an unexpected root: {rootPath}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDotnetExecutableMissingException()
            => new(
                  "ECG4202"
                , "The dotnet executable was not found on PATH. Install the .NET SDK from "
                    + "https://dotnet.microsoft.com/download and restart Unity."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDotnetSdkListStartException(string dotnetPath)
            => new("ECG4203", $"Cannot start '{dotnetPath} --list-sdks'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDotnetSdkListFailedException(
              string dotnetPath
            , int exitCode
            , string standardError
        )
            => new("ECG4204", $"'{dotnetPath} --list-sdks' failed with exit code {exitCode}: {standardError}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateStableDotnetSdkMissingException()
            => new(
                  "ECG4205"
                , "No stable .NET SDK is installed. Install an SDK from https://dotnet.microsoft.com/download."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplatePackagePathMissingException()
            => new(
                  "ECG4212"
                , "Cannot resolve the EncosyTower package path for .NET workspace templates."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplateRootMissingException(string templateRoot)
            => new("ECG4213", $"The .NET workspace template path does not exist: {templateRoot}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateRequiredTemplateMissingException(string path)
            => new("ECG4213", $"A required .NET workspace template is missing: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplateMaterializationException(
              string path
            , string message
        )
            => new("ECG4213", $"Cannot materialize .NET workspace template '{path}': {message}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDuplicateTemplateTokenException(
              string templatePath
            , string token
        )
            => new("ECG4214", $"Cannot compose template '{templatePath}': duplicate token '{token}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateMissingTemplateTokenException(
              string templatePath
            , string token
        )
            => new("ECG4214", $"Cannot compose template '{templatePath}': missing token '{token}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateUnresolvedTemplateTokenException(
              string templatePath
            , string token
        )
            => new("ECG4214", $"Cannot compose template '{templatePath}': unresolved token '{token}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceRecreationPathUnexpectedException(string workspaceRoot)
            => new("ECG4206", $"Refusing to recreate an unexpected workspace path: {workspaceRoot}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceProcessManifestLockedException()
            => new(
                  "ECG4207"
                , "The recorded .NET process manifest still exists. The workspace is locked "
                    + "until the process owner validates and stops the recorded tree."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateRetainedWorkspaceExistsException(string workspaceRoot)
            => new("ECG4211", $"The retained .NET workspace already exists: {workspaceRoot}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspacePathOccupiedException(string workspaceRoot)
            => new("ECG4211", $"The .NET workspace path is already occupied: {workspaceRoot}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplateDirectoryReadException(
              string sourceDirectory
            , string message
        )
            => new("ECG4213", $"Cannot read .NET workspace template directory '{sourceDirectory}': {message}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplateCopyException(string sourcePath, string message)
            => new("ECG4213", $"Cannot copy .NET workspace template '{sourcePath}': {message}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplateReparsePointException(string path)
            => new("ECG4213", $"A .NET workspace template path is a reparse point: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateTemplatePathEscapeException(string templatePath)
            => new("ECG4213", $"A .NET workspace template escaped the workspace root: {templatePath}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceReparsePointException(string path)
            => new("ECG4210", $"The workspace contains a reparse point: {path}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceRootReparsePointException(string root)
            => new("ECG4209", $"The workspace root is a reparse point: {root}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateWorkspaceLockedByManifestException(string path)
            => new("ENCOSY_CODEGEN_WORKSPACE_LOCKED", $"The workspace is locked by '{path}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessStartException(string executablePath)
            => new("ENCOSY_CODEGEN_PROCESS_START", $"Cannot start '{executablePath}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateRecordedProcessIdentityMismatchException(int processId)
            => new(
                  "ENCOSY_CODEGEN_WORKSPACE_LOCKED"
                , $"Recorded process {processId} no longer matches its live identity."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessIdentityCaptureException(int processId)
            => new("ENCOSY_CODEGEN_PROCESS_IDENTITY", $"Cannot capture the identity of CodeGen process {processId}.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessIdentityInspectionException(
              int processId
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_IDENTITY"
                , $"Cannot inspect CodeGen process {processId}."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessWorkspaceIdentityMissingException(int processId)
            => new(
                  "ENCOSY_CODEGEN_PROCESS_IDENTITY"
                , $"Process {processId} does not contain the expected workspace identity."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessRequestMissingValuesException()
            => new(
                  "ENCOSY_CODEGEN_PROCESS_REQUEST"
                , "The CodeGen process request is missing required values."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDotnetExecutableMissingException(string executablePath)
            => new(
                  "ENCOSY_CODEGEN_DOTNET_MISSING"
                , $"The configured dotnet executable '{executablePath}' does not exist. "
                    + "Install a supported .NET SDK from https://dotnet.microsoft.com/download."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessArtifactPathsNotDistinctException()
            => new(
                  "ENCOSY_CODEGEN_PROCESS_REQUEST"
                , "The process manifest, stdout log, and stderr log paths must be distinct."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessArgumentNullException(int index, string operationName)
            => new(
                  "ENCOSY_CODEGEN_PROCESS_REQUEST"
                , $"Argument {index} for '{operationName}' is null."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessManifestPathsRequiredException()
            => new(
                  "ENCOSY_CODEGEN_PROCESS_MANIFEST_PATH"
                , "The workspace root and process manifest path are required."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessPathEscapeException(
              string role
            , string path
            , string workspaceRoot
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_PATH_ESCAPE"
                , $"The {role} path '{path}' is outside '{workspaceRoot}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PlatformNotSupportedException CreateProcessIdentityPlatformUnsupportedException()
            => new("Process identity is not supported on this platform.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Win32Exception CreateProcessCommandLineLengthQueryException(int errorCode)
            => new(errorCode, "Cannot query the process command-line length.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Win32Exception CreateProcessCommandLineQueryException(int errorCode)
            => new(errorCode, "Cannot query the process command line.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Win32Exception CreateMacProcessCommandLineQueryException(int errorCode)
            => new(errorCode);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateProcessTerminateException(
              int processId
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_PROCESS_TERMINATE"
                , $"Cannot terminate verified CodeGen process {processId}."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedCodesNullException()
            => new("ENCOSY_CODEGEN_WRITE_INVALID_BATCH", "GeneratedCodes cannot be null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDiagnosticsNullException()
            => new("ENCOSY_CODEGEN_WRITE_INVALID_BATCH", "Diagnostics cannot be null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateAllSkippedBatchContainsCodeException()
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_BATCH"
                , "An all-skipped batch cannot contain generated code."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDiagnosticCodeNullException(int index)
            => new("ENCOSY_CODEGEN_WRITE_INVALID_BATCH", $"Diagnostics[{index}].Code cannot be null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDiagnosticMessageNullException(int index)
            => new("ENCOSY_CODEGEN_WRITE_INVALID_BATCH", $"Diagnostics[{index}].Message cannot be null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDiagnosticFilePathNullException(int index)
            => new("ENCOSY_CODEGEN_WRITE_INVALID_BATCH", $"Diagnostics[{index}].FilePath cannot be null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateUnityProjectRootResolutionException(string assetsPath)
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_PATH"
                , $"Cannot resolve the Unity project root from '{assetsPath}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedCodeStagingDirectoryException(
              string stagingRoot
            , Exception innerException
        )
            => new(
                  WRITE_STAGING_FAILED
                , $"Cannot create generated-code staging directory '{stagingRoot}'."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedContentNullException(int index)
            => new("ENCOSY_CODEGEN_WRITE_INVALID_BATCH", $"GeneratedCodes[{index}].content cannot be null.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateDuplicateGeneratedDestinationException(int index, string path)
            => new(
                  "ENCOSY_CODEGEN_WRITE_DUPLICATE_PATH"
                , $"GeneratedCodes[{index}] duplicates destination '{path}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedFilePathBlankException(int index)
            => new("ENCOSY_CODEGEN_WRITE_INVALID_PATH", $"GeneratedCodes[{index}].filePath cannot be blank.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedFilePathInvalidException(
              int index
            , string filePath
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_PATH"
                , $"GeneratedCodes[{index}].filePath '{filePath}' is invalid."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedContentUnicodeException(
              string destinationPath
            , Exception innerException
        )
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_BATCH"
                , $"Generated content for '{destinationPath}' is not valid Unicode."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedDestinationOutsideOwnedDirectoriesException(string path)
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_PATH"
                , $"Generated destination '{path}' must be beneath project Assets or Packages."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedCodeRootMissingException(string path)
            => new("ENCOSY_CODEGEN_WRITE_INVALID_PATH", $"Allowed generated-code root '{path}' does not exist.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedDestinationDirectoryException(string path)
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_PATH"
                , $"Generated destination '{path}' is an existing directory."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedDestinationFileParentException(
              string destinationPath
            , string parentPath
        )
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_PATH"
                , $"Generated destination '{destinationPath}' has file parent '{parentPath}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedDestinationReparsePointException(string path)
            => new(
                  "ENCOSY_CODEGEN_WRITE_INVALID_PATH"
                , $"Generated destination traverses reparse entry '{path}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedCodeReadException(
              string errorCode
            , string path
            , Exception innerException
        )
            => new(errorCode, $"Cannot read generated-code file '{path}'.", innerException);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedCodeStagingWriteException(
              string stagingPath
            , Exception innerException
        )
            => new(
                  WRITE_STAGING_FAILED
                , $"Cannot stage generated content at '{stagingPath}'."
                , innerException
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedDestinationChangedException(string path)
            => new(WRITE_COMMIT_FAILED, $"Generated destination changed before commit: '{path}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedDestinationAppearedException(string path)
            => new(WRITE_COMMIT_FAILED, $"Generated destination appeared before commit: '{path}'.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateAssetRefreshException(Exception innerException)
            => new(WRITE_REFRESH_FAILED, WRITE_REFRESH_FAILED_MESSAGE, innerException);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static CodeGenRunException CreateGeneratedCodeCommitException(
              string errorCode
            , string message
            , Exception innerException
        )
            => new(errorCode, message, innerException);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static IOException CreateGeneratedCodeDirectoryCollisionException(string path)
            => new($"Cannot create generated-code directory over file '{path}'.");

        internal static void ThrowIfInvalidBatch(GeneratedCodeBatch batch)
        {
            var generatedCodes = batch.GeneratedCodes;
            var diagnostics = batch.Diagnostics;
            var valid = generatedCodes != null && diagnostics != null;
            var diagnosticCount = diagnostics?.Length ?? 0;

            for (var i = 0; valid && i < diagnosticCount; i++)
            {
                var diagnostic = diagnostics[i];
                valid = diagnostic.Code != null
                    && diagnostic.Message != null
                    && diagnostic.FilePath != null;
            }

            if (valid == false)
            {
                ThrowInvalidBatch();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowInvalidBatch()
            => throw CreateInvalidBatchException();

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void LogDiagnostics(CodeGenDiagnostic[] diagnostics)
        {
            var count = diagnostics.Length;

            for (var i = 0; i < count; i++)
            {
                var diagnostic = diagnostics[i];
                var message = diagnostic.FilePath.Length == 0
                    ? $"[{diagnostic.Code}] {diagnostic.Message}"
                    : $"[{diagnostic.Code}] {diagnostic.Message} " +
                      $"({diagnostic.FilePath}:{diagnostic.Line}:{diagnostic.Column})";

                switch (diagnostic.Severity)
                {
                    case CodeGenDiagnosticSeverity.Warning:
                    {
                        StaticDevLogger.LogWarning(message);
                        break;
                    }

                    case CodeGenDiagnosticSeverity.Error:
                    {
                        StaticDevLogger.LogError(message);
                        break;
                    }

                    default:
                    {
                        StaticDevLogger.LogInfo(message);
                        break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void LogWarning_StaleUnityGenerators()
        {
            StaticDevLogger.LogWarning(
                "Unity compilation failed. Generator code may be stale; use " +
                "'Encosy Tower/CodeGen/Generate in .NET' to run clean generator sources."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void LogError(string code, string message)
        {
            StaticDevLogger.LogError($"[{code}] {message}");
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfReparsePoint(
              [DoesNotReturnIf(false)] bool valid
            , string path
        )
        {
            if (valid == false)
            {
                throw CreateGeneratedDestinationReparsePointException(path);
            }
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfTemplateDestinationEscapes(
              string workspaceRoot
            , string destination
            , string templatePath
        )
        {
            if (DotnetWorkspaceProtocol.IsContained(workspaceRoot, destination) == false)
            {
                throw CreateTemplatePathEscapeException(templatePath);
            }
        }
    }
}
