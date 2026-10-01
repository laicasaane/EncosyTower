#if UNITY_EDITOR

using System;
using System.Globalization;
using System.IO;
using System.Text;
using EncosyTower.Editor.SourceGen;
using NUnit.Framework;

namespace EncosyTower.Tests.Editor.SourceGen
{
    [Category("Editor.SourceGen")]
    public sealed class SourceGenCaptureTests
    {
        private const int BUILD_TARGET_GROUP = 1;
        private const string BUILD_TARGET_NAME = "Standalone";

        private static readonly UTF8Encoding s_utf8WithoutBom = new(false, true);
        private static readonly string s_testRoot = Path.GetFullPath(
            "Library/EncosyTower/SourceGenCaptureTests"
        );

        private string[] _compilerArguments = Array.Empty<string>();
        private string _projectRoot = string.Empty;

        [SetUp]
        public void SetUp()
        {
            _projectRoot = Path.Combine(s_testRoot, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_projectRoot);
            _compilerArguments = Array.Empty<string>();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteOwnedProjectRoot(_projectRoot);
        }

        [Test]
        public void Prepare_AppendsGlobalArgumentAndRestoresExactOriginalArguments()
        {
            var originalArguments = new[] { "-langversion:10", "/unsafe" };
            _compilerArguments = Clone(originalArguments);
            var operation = CreateOperation();

            Assert.That(operation.Prepare(retainOutput: false, out var prepareError), Is.True, prepareError);
            Assert.That(_compilerArguments, Has.Length.EqualTo(3));
            Assert.That(_compilerArguments[0], Is.EqualTo(originalArguments[0]));
            Assert.That(_compilerArguments[1], Is.EqualTo(originalArguments[1]));
            Assert.That(
                  _compilerArguments[2]
                , Is.EqualTo($"-generatedfilesout:\"{operation.CaptureDirectoryPath}\"")
            );

            _compilerArguments = new[] { "changed after prepare" };
            Assert.That(
                  operation.Restore(out var captureDirectoryPath, out var restoreError)
                , Is.True
                , restoreError
            );
            Assert.That(captureDirectoryPath, Is.EqualTo(operation.CaptureDirectoryPath));
            CollectionAssert.AreEqual(originalArguments, _compilerArguments);
            Assert.That(operation.HasRecoveryState, Is.False);
            Assert.That(Directory.Exists(captureDirectoryPath), Is.True);
        }

        [Test]
        public void Prepare_DoesNotCreateProjectResponseFiles()
        {
            var operation = CreateOperation();
            Assert.That(operation.Prepare(retainOutput: false, out var prepareError), Is.True, prepareError);

            try
            {
                Assert.That(File.Exists(Path.Combine(_projectRoot, "Assets", "csc.rsp")), Is.False);
                Assert.That(File.Exists(Path.Combine(_projectRoot, "Assets", "csc.rsp.meta")), Is.False);
            }
            finally
            {
                Assert.That(operation.Restore(out _, out var restoreError), Is.True, restoreError);
            }
        }

        [Test]
        public void Prepare_UsesSourceGenLibraryDirectories()
        {
            var operation = CreateOperation();
            Assert.That(operation.Prepare(retainOutput: false, out var prepareError), Is.True, prepareError);

            try
            {
                Assert.That(
                      operation.GeneratedCodeRootPath
                    , Is.EqualTo(
                        Path.Combine(_projectRoot, "Library", "EncosyTower", "SourceGen")
                    )
                );
                Assert.That(File.Exists(RecoveryStatePath), Is.True);
            }
            finally
            {
                Assert.That(operation.Restore(out _, out var restoreError), Is.True, restoreError);
            }
        }

        [TestCase("-generatedfilesout:\"C:/Other\"")]
        [TestCase("   /GENERATEDFILESOUT:C:/Other")]
        public void ExistingGeneratedFilesOption_PrepareRejectsWithoutMutation(string option)
        {
            _compilerArguments = new[] { "-noconfig", option, "/unsafe" };
            var originalArguments = Clone(_compilerArguments);
            var operation = CreateOperation();

            Assert.That(operation.Prepare(retainOutput: false, out var error), Is.False);
            Assert.That(error, Is.Not.Empty);
            CollectionAssert.AreEqual(originalArguments, _compilerArguments);
            Assert.That(operation.HasRecoveryState, Is.False);
            Assert.That(Directory.Exists(operation.GeneratedCodeRootPath), Is.False);
        }

        [Test]
        public void CaptureArgumentWriteFailure_RestoresOriginalArguments()
        {
            _compilerArguments = new[] { "-langversion:10" };
            var setCallCount = 0;
            var operation = CreateOperation((_, arguments) => {
                setCallCount++;

                if (setCallCount == 1)
                {
                    throw new InvalidOperationException("simulated write failure");
                }

                _compilerArguments = Clone(arguments);
            });

            Assert.That(operation.Prepare(retainOutput: false, out var error), Is.False);
            Assert.That(error, Does.Contain("simulated write failure"));
            CollectionAssert.AreEqual(new[] { "-langversion:10" }, _compilerArguments);
            Assert.That(operation.HasRecoveryState, Is.False);
        }

        [Test]
        public void CorruptRecoveryState_RestoreRefusesWithoutChangingArguments()
        {
            _compilerArguments = new[] { "-langversion:10" };
            var operation = CreateOperation();
            Assert.That(operation.Prepare(retainOutput: false, out var prepareError), Is.True, prepareError);
            var currentArguments = Clone(_compilerArguments);
            File.WriteAllBytes(RecoveryStatePath, s_utf8WithoutBom.GetBytes("corrupt"));

            Assert.That(operation.Restore(out _, out var error), Is.False);
            Assert.That(error, Does.Contain(RecoveryStatePath));
            CollectionAssert.AreEqual(currentArguments, _compilerArguments);
            Assert.That(operation.HasRecoveryState, Is.True);
        }

        [Test]
        public void InterruptedOperation_RestoresThroughNewInstance()
        {
            var originalArguments = new[] { "-langversion:10", "/unsafe" };
            _compilerArguments = Clone(originalArguments);
            var firstOperation = CreateOperation();
            Assert.That(firstOperation.Prepare(retainOutput: false, out var prepareError), Is.True, prepareError);
            var captureDirectoryPath = firstOperation.CaptureDirectoryPath;
            var recoveredOperation = CreateOperation();

            Assert.That(
                  recoveredOperation.Restore(out var recoveredCapturePath, out var restoreError)
                , Is.True
                , restoreError
            );
            Assert.That(recoveredCapturePath, Is.EqualTo(captureDirectoryPath));
            CollectionAssert.AreEqual(originalArguments, _compilerArguments);
            Assert.That(recoveredOperation.HasRecoveryState, Is.False);
        }

        [Test]
        public void ConsecutivePreparations_CreateDistinctRetainedCaptureDirectories()
        {
            var firstOperation = CreateOperation();
            Assert.That(
                  firstOperation.Prepare(retainOutput: true, out var firstPrepareError)
                , Is.True
                , firstPrepareError
            );
            var firstCaptureDirectoryPath = firstOperation.CaptureDirectoryPath;
            Assert.That(firstOperation.Restore(out _, out var firstRestoreError), Is.True, firstRestoreError);

            var secondOperation = CreateOperation();
            Assert.That(
                  secondOperation.Prepare(retainOutput: true, out var secondPrepareError)
                , Is.True
                , secondPrepareError
            );
            var secondCaptureDirectoryPath = secondOperation.CaptureDirectoryPath;
            Assert.That(secondOperation.Restore(out _, out var secondRestoreError), Is.True, secondRestoreError);

            Assert.That(secondCaptureDirectoryPath, Is.Not.EqualTo(firstCaptureDirectoryPath));
            Assert.That(Directory.Exists(firstCaptureDirectoryPath), Is.True);
            Assert.That(Directory.Exists(secondCaptureDirectoryPath), Is.True);
        }

        [Test]
        public void ConsecutivePreparations_RenewCurrentAndPreserveRetainedDirectories()
        {
            var sourceGenRoot = Path.Combine(_projectRoot, "Library", "EncosyTower", "SourceGen");
            var currentRoot = Path.Combine(sourceGenRoot, "Current");
            var retainedRoot = Path.Combine(sourceGenRoot, "0");
            var currentMarker = Path.Combine(currentRoot, "replace.txt");
            var retainedMarker = Path.Combine(retainedRoot, "preserve.txt");
            Directory.CreateDirectory(currentRoot);
            Directory.CreateDirectory(retainedRoot);
            File.WriteAllText(currentMarker, "replace");
            File.WriteAllText(retainedMarker, "preserve");

            var firstOperation = CreateOperation();
            Assert.That(
                  firstOperation.Prepare(retainOutput: false, out var firstPrepareError)
                , Is.True
                , firstPrepareError
            );

            try
            {
                Assert.That(firstOperation.CaptureDirectoryPath, Is.EqualTo(currentRoot));
                Assert.That(File.Exists(currentMarker), Is.False);
                Assert.That(File.Exists(retainedMarker), Is.True);
            }
            finally
            {
                Assert.That(firstOperation.Restore(out _, out var restoreError), Is.True, restoreError);
            }

            File.WriteAllText(currentMarker, "replace again");
            var secondOperation = CreateOperation();
            Assert.That(
                  secondOperation.Prepare(retainOutput: false, out var secondPrepareError)
                , Is.True
                , secondPrepareError
            );

            try
            {
                Assert.That(secondOperation.CaptureDirectoryPath, Is.EqualTo(currentRoot));
                Assert.That(File.Exists(currentMarker), Is.False);
                Assert.That(File.Exists(retainedMarker), Is.True);
            }
            finally
            {
                Assert.That(secondOperation.Restore(out _, out var restoreError), Is.True, restoreError);
            }
        }

        [Test]
        public void Prepare_UsesUnixMillisecondsForCaptureDirectoryName()
        {
            var beforePrepare = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var operation = CreateOperation();

            Assert.That(operation.Prepare(retainOutput: true, out var prepareError), Is.True, prepareError);

            var afterPrepare = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var directoryName = Path.GetFileName(operation.CaptureDirectoryPath);
            Assert.That(
                  long.TryParse(
                      directoryName
                    , NumberStyles.None
                    , CultureInfo.InvariantCulture
                    , out var unixMilliseconds
                )
                , Is.True
            );
            Assert.That(unixMilliseconds, Is.InRange(beforePrepare, afterPrepare));
            Assert.That(operation.Restore(out _, out var restoreError), Is.True, restoreError);
        }

        private string RecoveryStatePath
            => Path.Combine(_projectRoot, "Library", "EncosyTower", "SourceGen", "Recovery", "state.json");

        private SourceGenCaptureOperation CreateOperation(Action<int, string[]> setCompilerArguments = null)
        {
            return new SourceGenCaptureOperation(
                  _projectRoot
                , BUILD_TARGET_GROUP
                , BUILD_TARGET_NAME
                , _ => Clone(_compilerArguments)
                , setCompilerArguments ?? ((_, arguments) => _compilerArguments = Clone(arguments))
            );
        }

        private static string[] Clone(string[] values)
        {
            var result = new string[values.Length];
            Array.Copy(values, result, values.Length);
            return result;
        }

        private static void DeleteOwnedProjectRoot(string projectRoot)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                return;
            }

            var fullParent = Path.GetFullPath(s_testRoot);
            var fullProjectRoot = Path.GetFullPath(projectRoot);
            var parent = Directory.GetParent(fullProjectRoot);
            var childName = Path.GetFileName(fullProjectRoot);

            if (parent is null
                || PathsEqual(parent.FullName, fullParent) == false
                || Guid.TryParseExact(childName, "N", out var guid) == false
                || string.Equals(guid.ToString("N"), childName, StringComparison.Ordinal) == false
            )
            {
                throw new InvalidOperationException($"Refusing to delete unowned test directory: {fullProjectRoot}");
            }

            if (Directory.Exists(fullProjectRoot))
            {
                Directory.Delete(fullProjectRoot, true);
            }
        }

        private static bool PathsEqual(string left, string right)
            => string.Equals(
                  Path.GetFullPath(left)
                , Path.GetFullPath(right)
                , Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            );
    }
}

#endif
