#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Editor.CodeGen;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Editor.CodeGen
{
    [Category("Editor.CodeGen")]
    public sealed class DotnetWorkspaceTests
    {
        private static readonly string[] s_requiredTemplatePaths = {
            "Directory.Build.props",
            "Directory.Build.targets",
            "Directory.Packages.props",
            "global.json",
            "EncosyCodeGen.slnx",
            Path.Combine("Runner", "EncosyCodeGenRunner.csproj"),
            Path.Combine("Runner", "Program.cs"),
            Path.Combine("Runner", "ThrowHelper.cs"),
            Path.Combine("Island", "Island.csproj"),
            Path.Combine("Island", "GeneratedIslands.props"),
        };

        private static readonly string[] s_runtimeDirectoryNames = {
            "artifacts",
            "dependencies",
            "islands",
            "logs",
            "manifests",
            "results",
            "sources",
        };

        private static readonly string[] s_runnerOwnedTokens = {
            "{{ENCOSY_LANGUAGE_VERSION}}",
            "{{ENCOSY_NULLABLE_MODE}}",
            "{{ENCOSY_ALLOW_UNSAFE_BLOCKS}}",
            "{{ENCOSY_DEFINE_CONSTANTS}}",
            "{{ENCOSY_PATH_MAP}}",
            "{{ENCOSY_ADDITIONAL_OPTIONS_PROPERTY}}",
            "{{ENCOSY_CODE_ANALYSIS_RULE_SET_PROPERTY}}",
            "{{ENCOSY_PROJECT_ROOT}}",
            "{{ENCOSY_UNITY_VERSION_ROOT}}",
            "{{ENCOSY_ISLAND_ID}}",
            "{{ENCOSY_SOURCE_ITEMS}}",
            "{{ENCOSY_REFERENCE_ITEMS}}",
            "{{ENCOSY_ANALYZER_ITEMS}}",
            "{{ENCOSY_ADDITIONAL_FILE_ITEMS}}",
            "{{ENCOSY_GLOBAL_ANALYZER_CONFIG_ITEM}}",
            "{{ENCOSY_ISLAND_PROJECT_REFERENCES}}",
            "{{ENCOSY_ISLAND_SOLUTION_ENTRIES}}",
        };

        private static readonly string s_testRoot = Path.GetFullPath("Library/EncosyTower/CodeGenWorkspaceTests");

        private string _testDirectory = string.Empty;

        [SetUp]
        public void SetUp()
        {
            _testDirectory = Path.Combine(s_testRoot, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Test]
        public void ParseSdkVersions_StableOnly_ReturnsAscendingVersions()
        {
            var result = DotnetWorkspaceTemplateWriter.ParseSdkVersions(
                "10.0.302 [D:\\dotnet\\sdk]\n" +
                "11.0.100-preview.1 [D:\\dotnet\\sdk]\n" +
                "10.0.103 [D:\\dotnet\\sdk]\n"
            );

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].text, Is.EqualTo("10.0.103"));
            Assert.That(result[1].text, Is.EqualTo("10.0.302"));
        }

        [Test]
        public void TemplateMaterialization_HostValuesAndRuntimeTokens_AreExact()
        {
            var templateRoot = DotnetWorkspaceTemplateWriter.ResolveTemplateRoot();
            var workspaceRoot = Path.Combine(_testDirectory, "Workspace");

            DotnetWorkspaceTemplateWriter.MaterializeTemplateTree(
                templateRoot,
                workspaceRoot,
                "10.0.302",
                CancellationToken.None
            );

            var globalJson = File.ReadAllText(Path.Combine(workspaceRoot, "global.json"));
            var islandProject = File.ReadAllText(Path.Combine(workspaceRoot, "Island", "Island.csproj"));
            var generatedProps = File.ReadAllText(
                Path.Combine(workspaceRoot, "Island", "GeneratedIslands.props")
            );
            var solution = File.ReadAllText(Path.Combine(workspaceRoot, "EncosyCodeGen.slnx"));

            Assert.That(globalJson, Does.Contain("\"version\": \"10.0.302\""));
            Assert.That(islandProject, Does.Not.Contain("{{ENCOSY_UNITY3D_VERSION}}"));

            var runnerTemplates = islandProject + generatedProps + solution;

            for (var i = 0; i < s_runnerOwnedTokens.Length; i++)
            {
                Assert.That(runnerTemplates, Does.Contain(s_runnerOwnedTokens[i]), s_runnerOwnedTokens[i]);
            }

            for (var i = 0; i < s_requiredTemplatePaths.Length; i++)
            {
                var path = Path.Combine(workspaceRoot, s_requiredTemplatePaths[i]);
                var bytes = File.ReadAllBytes(path);

                Assert.That(HasUtf8Bom(bytes), Is.False, path);
            }

            for (var i = 0; i < s_runtimeDirectoryNames.Length; i++)
            {
                Assert.That(
                    Directory.Exists(Path.Combine(workspaceRoot, s_runtimeDirectoryNames[i])),
                    Is.True,
                    s_runtimeDirectoryNames[i]
                );
            }
        }

        [Test]
        public void TemplateMaterialization_MissingRequiredFile_ThrowsBeforeProcessRequest()
        {
            var sourceRoot = DotnetWorkspaceTemplateWriter.ResolveTemplateRoot();
            var templateRoot = Path.Combine(_testDirectory, "Templates");
            var workspaceRoot = Path.Combine(_testDirectory, "Workspace");
            CopyTemplateTree(sourceRoot, templateRoot);
            var missingPath = Path.Combine(templateRoot, "global.json");
            File.Delete(missingPath);

            var exception = Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceTemplateWriter.MaterializeTemplateTree(
                    templateRoot,
                    workspaceRoot,
                    "10.0.302",
                    CancellationToken.None
                )
            );

            Assert.That(exception.Code, Is.EqualTo("ECG4213"));
            Assert.That(exception.Message, Does.Contain(missingPath));
        }

        [Test]
        public void TemplateComposition_DuplicateReplacementKey_Throws()
        {
            AssertCompositionFailure(
                new[] {
                    new KeyValuePair<string, string>("{{ENCOSY_TEST}}", "first"),
                    new KeyValuePair<string, string>("{{ENCOSY_TEST}}", "second"),
                },
                "{{ENCOSY_TEST}}",
                new[] { "{{ENCOSY_TEST}}" }
            );
        }

        [Test]
        public void TemplateComposition_MissingRequiredToken_Throws()
        {
            AssertCompositionFailure(
                new[] { new KeyValuePair<string, string>("{{ENCOSY_TEST}}", "value") },
                "plain content",
                new[] { "{{ENCOSY_TEST}}" }
            );
        }

        [Test]
        public void TemplateComposition_UnresolvedOwnedToken_Throws()
        {
            AssertCompositionFailure(
                new[] { new KeyValuePair<string, string>("{{ENCOSY_FIRST}}", "value") },
                "{{ENCOSY_FIRST}} {{ENCOSY_SECOND}}",
                new[] { "{{ENCOSY_FIRST}}", "{{ENCOSY_SECOND}}" }
            );
        }

        [Test]
        public void InputManifest_WriteTwice_IsByteDeterministic()
        {
            var projectRoot = Path.GetFullPath(".");
            var dotnetPath = Path.Combine(_testDirectory, "dotnet");
            File.WriteAllText(dotnetPath, string.Empty);
            var manifest = new DotnetWorkspaceProtocol.InputManifest {
                schemaVersion = DotnetWorkspaceProtocol.SCHEMA_VERSION,
                projectRoot = projectRoot,
                unityVersion = Application.unityVersion,
                unityVersionRoot = projectRoot,
                dotnetExecutablePath = dotnetPath,
                selectedSdkVersion = "10.0.302",
                assemblies = Array.Empty<DotnetWorkspaceProtocol.AssemblyInput>(),
            };
            var firstPath = Path.Combine(_testDirectory, "first.json");
            var secondPath = Path.Combine(_testDirectory, "second.json");

            DotnetWorkspaceProtocol.WriteInput(firstPath, manifest);
            DotnetWorkspaceProtocol.WriteInput(secondPath, manifest);

            var firstBytes = File.ReadAllBytes(firstPath);
            CollectionAssert.AreEqual(firstBytes, File.ReadAllBytes(secondPath));
            Assert.That(
                firstBytes.Length < 3
                || firstBytes[0] != 0xEF
                || firstBytes[1] != 0xBB
                || firstBytes[2] != 0xBF,
                Is.True
            );
        }

        [Test]
        public void InputManifest_PredefinedAssembliesWithoutAsmdefs_UsesExactNamesAndAssetsBoundary()
        {
            var projectRoot = Path.Combine(_testDirectory, "Project");
            var assetsRoot = Path.Combine(projectRoot, "Assets");
            var packagesRoot = Path.Combine(projectRoot, "Packages");
            var outputsRoot = Path.Combine(projectRoot, "Library", "Outputs");
            var unityVersionRoot = Path.Combine(_testDirectory, "Unity");
            var dotnetPath = Path.Combine(_testDirectory, "dotnet");
            var asmdefPath = Path.Combine(assetsRoot, "Owner.asmdef");
            var packageSourcePath = Path.Combine(packagesRoot, "Outside.cs");
            var assemblyNames = new[] {
                  "Assembly-CSharp-firstpass"
                , "Assembly-CSharp-Editor-firstpass"
                , "Assembly-CSharp"
                , "Assembly-CSharp-Editor"
            };
            var sourcePaths = new[] {
                  Path.Combine(assetsRoot, "Firstpass.cs")
                , Path.Combine(assetsRoot, "FirstpassEditor.cs")
                , Path.Combine(assetsRoot, "Runtime.cs")
                , Path.Combine(assetsRoot, "Editor.cs")
            };
            var outputPaths = new[] {
                  Path.Combine(outputsRoot, "Firstpass.dll")
                , Path.Combine(outputsRoot, "FirstpassEditor.dll")
                , Path.Combine(outputsRoot, "Runtime.dll")
                , Path.Combine(outputsRoot, "Editor.dll")
            };

            Directory.CreateDirectory(assetsRoot);
            Directory.CreateDirectory(packagesRoot);
            Directory.CreateDirectory(outputsRoot);
            Directory.CreateDirectory(unityVersionRoot);
            File.WriteAllText(dotnetPath, string.Empty);
            File.WriteAllText(asmdefPath, string.Empty);
            File.WriteAllText(packageSourcePath, string.Empty);

            var assemblies = new DotnetWorkspaceProtocol.AssemblyInput[assemblyNames.Length];

            for (var i = 0; i < assemblyNames.Length; i++)
            {
                File.WriteAllText(sourcePaths[i], string.Empty);
                File.WriteAllText(outputPaths[i], string.Empty);
                assemblies[i] = new DotnetWorkspaceProtocol.AssemblyInput {
                    name = assemblyNames[i],
                    asmdefPath = string.Empty,
                    outputPath = outputPaths[i],
                    sourceFiles = new[] { sourcePaths[i] },
                    defines = Array.Empty<string>(),
                    references = Array.Empty<DotnetWorkspaceProtocol.ReferenceInput>(),
                    compilerOptions = new DotnetWorkspaceProtocol.CompilerOptionsInput {
                        languageVersion = "10.0",
                        nullable = "disable",
                        responseArguments = Array.Empty<string>(),
                        responseFiles = Array.Empty<string>(),
                        analyzerConfigPath = string.Empty,
                        analyzerRulesetPath = string.Empty,
                    },
                    analyzers = Array.Empty<string>(),
                    additionalFiles = Array.Empty<string>(),
                };
            }

            var manifest = new DotnetWorkspaceProtocol.InputManifest {
                schemaVersion = DotnetWorkspaceProtocol.SCHEMA_VERSION,
                projectRoot = projectRoot,
                unityVersion = Application.unityVersion,
                unityVersionRoot = unityVersionRoot,
                dotnetExecutablePath = dotnetPath,
                selectedSdkVersion = "10.0.302",
                assemblies = assemblies,
            };

            Assert.DoesNotThrow(() => DotnetWorkspaceProtocol.ValidateInput(manifest));

            assemblies[0].name = "assembly-CSharp-firstpass";
            var caseVariantException = Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceProtocol.ValidateInput(manifest)
            );

            Assert.That(caseVariantException.Code, Is.EqualTo("ECG4026"));

            assemblies[0].name = assemblyNames[0];
            assemblies[0].sourceFiles[0] = packageSourcePath;
            var packageSourceException = Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceProtocol.ValidateInput(manifest)
            );

            Assert.That(packageSourceException.Code, Is.EqualTo("ECG4035"));

            assemblies[0].sourceFiles[0] = sourcePaths[0];
            assemblies[0].asmdefPath = asmdefPath;
            var predefinedAsmdefException = Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceProtocol.ValidateInput(manifest)
            );

            Assert.That(predefinedAsmdefException.Code, Is.EqualTo("ECG4034"));
        }

        [Test]
        public void ReadResult_ValidResult_ReturnsCompleteBatch()
        {
            var projectRoot = Path.GetFullPath(".");
            var outputPath = Path.Combine(projectRoot, "Assets", "Temp", "Output.gen.cs");
            var manifest = new DotnetWorkspaceProtocol.AggregateResultManifest {
                schemaVersion = DotnetWorkspaceProtocol.SCHEMA_VERSION,
                projectRoot = projectRoot,
                generatedCodes = new[] {
                    new DotnetWorkspaceProtocol.GeneratedCodeResult {
                        filePath = outputPath,
                        content = "content",
                    },
                },
                diagnostics = new[] {
                    new DotnetWorkspaceProtocol.DiagnosticResult {
                        severity = 1,
                        code = "ECG4999",
                        message = "warning",
                        filePath = string.Empty,
                    },
                },
            };
            var path = Path.Combine(_testDirectory, "result.json");
            DotnetWorkspaceProtocol.WriteJsonAtomic(path, manifest);

            var batch = DotnetWorkspaceProtocol.ReadResult(path, projectRoot);

            Assert.That(batch.GeneratedCodes.Length, Is.EqualTo(1));
            Assert.That(batch.GeneratedCodes[0].filePath, Is.EqualTo(outputPath));
            Assert.That(batch.GeneratedCodes[0].content, Is.EqualTo("content"));
            Assert.That(batch.Diagnostics.Length, Is.EqualTo(1));
            Assert.That(batch.AllCandidatesSkipped, Is.False);
        }

        [Test]
        public void ReadResult_DuplicateCanonicalDestination_Throws()
        {
            var projectRoot = Path.GetFullPath(".");
            var outputPath = Path.Combine(projectRoot, "Assets", "Temp", "Output.gen.cs");
            var manifest = new DotnetWorkspaceProtocol.AggregateResultManifest {
                schemaVersion = DotnetWorkspaceProtocol.SCHEMA_VERSION,
                projectRoot = projectRoot,
                generatedCodes = new[] {
                    new DotnetWorkspaceProtocol.GeneratedCodeResult {
                        filePath = outputPath,
                        content = "first",
                    },
                    new DotnetWorkspaceProtocol.GeneratedCodeResult {
                        filePath = Path.Combine(projectRoot, "Assets", "Temp", ".", "Output.gen.cs"),
                        content = "second",
                    },
                },
                diagnostics = Array.Empty<DotnetWorkspaceProtocol.DiagnosticResult>(),
            };
            var path = Path.Combine(_testDirectory, "duplicate-result.json");
            DotnetWorkspaceProtocol.WriteJsonAtomic(path, manifest);

            Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceProtocol.ReadResult(path, projectRoot)
            );
        }

        [Test]
        public void ReadResult_UnknownSchema_Throws()
        {
            var projectRoot = Path.GetFullPath(".");
            var manifest = new DotnetWorkspaceProtocol.AggregateResultManifest {
                schemaVersion = 2,
                projectRoot = projectRoot,
                generatedCodes = Array.Empty<DotnetWorkspaceProtocol.GeneratedCodeResult>(),
                diagnostics = Array.Empty<DotnetWorkspaceProtocol.DiagnosticResult>(),
            };
            var path = Path.Combine(_testDirectory, "unknown-result.json");
            DotnetWorkspaceProtocol.WriteJsonAtomic(path, manifest);

            Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceProtocol.ReadResult(path, projectRoot)
            );
        }

        [Test]
        public void WorkspaceRoot_RetentionSelectsCurrentOrTimestampChild()
        {
            var writer = new DotnetWorkspaceTemplateWriter(_testDirectory);
            var current = writer.GetWorkspaceRoot(retainDotnetSolutions: false, invocationTimestamp: 1234L);
            var retained = writer.GetWorkspaceRoot(retainDotnetSolutions: true, invocationTimestamp: 1234L);

            Assert.That(current, Is.EqualTo(Path.Combine(_testDirectory, "Current")));
            Assert.That(retained, Is.EqualTo(Path.Combine(_testDirectory, "1234")));
        }

        [Test]
        public async Task CreateAndRunAsync_RetentionControlsWorkspaceRecreationAndPreservation()
        {
            var currentRoot = Path.Combine(_testDirectory, "Current");
            var retainedSibling = Path.Combine(_testDirectory, "0");
            var currentMarker = Path.Combine(currentRoot, "preexisting.txt");
            var retainedMarker = Path.Combine(retainedSibling, "retained.txt");
            Directory.CreateDirectory(currentRoot);
            Directory.CreateDirectory(retainedSibling);
            File.WriteAllText(currentMarker, "replace");
            File.WriteAllText(retainedMarker, "preserve");

            var writer = new DotnetWorkspaceTemplateWriter(_testDirectory);
            var runner = new DotnetProcessRunner();
            var output = new List<DotnetProcessOutput>();

            await writer.StopRecordedProcessTreesAsync(runner);
            var currentWorkspace = await writer.CreateAsync(retainDotnetSolutions: false);
            var currentResult = await runner.RunAsync(currentWorkspace.PrepareRequest, output.Add);

            Assert.That(currentResult.ExitCode, Is.Zero, string.Join(Environment.NewLine, output));
            var currentBatch = writer.ReadResult(currentWorkspace);

            Assert.That(currentWorkspace.RootPath, Is.EqualTo(currentRoot));
            Assert.That(File.Exists(currentMarker), Is.False);
            Assert.That(File.Exists(retainedMarker), Is.True);
            Assert.That(currentBatch.GeneratedCodes, Is.Not.Null);
            Assert.That(currentBatch.Diagnostics, Is.Not.Null);

            await writer.StopRecordedProcessTreesAsync(runner);
            output.Clear();
            var retainedWorkspace = await writer.CreateAsync(retainDotnetSolutions: true);
            var retainedResult = await runner.RunAsync(retainedWorkspace.PrepareRequest, output.Add);

            Assert.That(retainedResult.ExitCode, Is.Zero, string.Join(Environment.NewLine, output));
            var retainedBatch = writer.ReadResult(retainedWorkspace);

            Assert.That(Path.GetDirectoryName(retainedWorkspace.RootPath), Is.EqualTo(_testDirectory));
            Assert.That(retainedWorkspace.RootPath, Is.Not.EqualTo(currentRoot));
            Assert.That(retainedWorkspace.RootPath, Is.Not.EqualTo(retainedSibling));
            Assert.That(Directory.Exists(currentRoot), Is.True);
            Assert.That(File.Exists(retainedMarker), Is.True);
            Assert.That(retainedBatch.GeneratedCodes, Is.Not.Null);
            Assert.That(retainedBatch.Diagnostics, Is.Not.Null);
        }

        private static void AssertCompositionFailure(
              IReadOnlyList<KeyValuePair<string, string>> replacements
            , string content
            , IReadOnlyCollection<string> ownedTokens
        )
        {
            var templatePath = "test.template";
            var exception = Assert.Throws<CodeGenRunException>(
                () => DotnetWorkspaceTemplateWriter.ComposeTemplate(
                    templatePath,
                    content,
                    replacements,
                    ownedTokens
                )
            );

            Assert.That(exception.Code, Is.EqualTo("ECG4214"));
            Assert.That(exception.Message, Does.Contain(templatePath));
        }

        private static void CopyTemplateTree(string sourceRoot, string destinationRoot)
        {
            Directory.CreateDirectory(destinationRoot);
            var directories = Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories);

            for (var i = 0; i < directories.Length; i++)
            {
                var relativePath = directories[i][sourceRoot.Length..]
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Directory.CreateDirectory(Path.Combine(destinationRoot, relativePath));
            }

            var files = Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories);

            for (var i = 0; i < files.Length; i++)
            {
                var relativePath = files[i][sourceRoot.Length..]
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                File.Copy(files[i], Path.Combine(destinationRoot, relativePath));
            }
        }

        private static bool HasUtf8Bom(byte[] bytes)
            => bytes.Length >= 3
            && bytes[0] == 0xEF
            && bytes[1] == 0xBB
            && bytes[2] == 0xBF
            ;

    }
}

#endif
