#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using System.Threading;
using EncosyTower.CodeGen;
using EncosyTower.Editor.CodeGen;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Editor.CodeGen
{
    [Category("Editor.CodeGen")]
    public sealed class GeneratedCodeBatchWriterTests
    {
        private static readonly Encoding s_utf8WithoutBom = new UTF8Encoding(false, true);

        private Action _refreshAssets;
        private Action<string, string> _moveFile;
        private Action<string, string, string> _replaceFile;
        private Func<string, FileAttributes> _getAttributes;
        private string _projectRoot;
        private string _assetsTempRoot;
        private string _testSuiteRoot;
        private string _relativeTestRoot;
        private string _testRoot;
        private bool _assetsTempRootExisted;
        private bool _testSuiteRootExisted;
        private int _refreshCount;

        [SetUp]
        public void SetUp()
        {
            _refreshCount = 0;
            _refreshAssets = CountRefresh;
            _moveFile = File.Move;
            _replaceFile = File.Replace;
            _getAttributes = File.GetAttributes;
            _projectRoot = Directory.GetParent(Path.GetFullPath(Application.dataPath)).FullName;
            _assetsTempRoot = Path.Combine(Application.dataPath, "Temp");
            _testSuiteRoot = Path.Combine(_assetsTempRoot, nameof(GeneratedCodeBatchWriterTests));
            _relativeTestRoot = Path.Combine(
                  "Assets"
                , "Temp"
                , nameof(GeneratedCodeBatchWriterTests)
                , Guid.NewGuid().ToString("N")
            );
            _testRoot = Path.Combine(_projectRoot, _relativeTestRoot);
            _assetsTempRootExisted = Directory.Exists(_assetsTempRoot);
            _testSuiteRootExisted = Directory.Exists(_testSuiteRoot);
            Directory.CreateDirectory(_testRoot);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, true);
            }

            DeleteCreatedEmptyDirectory(_testSuiteRoot, _testSuiteRootExisted);
            DeleteCreatedEmptyDirectory(_assetsTempRoot, _assetsTempRootExisted);
        }

        [Test]
        public void Write_AbsoluteAndRelativeDestinations_WriteUtf8WithoutBomAndRefreshOnce()
        {
            var relativePath = RelativePath("Relative.gen.cs");
            var absolutePath = AbsolutePath("Absolute.gen.cs");
            var relativeContent = "first\r\nλ";
            var absoluteContent = "second\n漢字";
            var writer = CreateWriter();

            var result = writer.Write(
                  Batch(Code(relativePath, relativeContent), Code(absolutePath, absoluteContent))
                , default
            );

            Assert.That(result, Is.EqualTo(2));
            CollectionAssert.AreEqual(
                  s_utf8WithoutBom.GetBytes(relativeContent)
                , File.ReadAllBytes(AbsolutePath("Relative.gen.cs"))
            );
            CollectionAssert.AreEqual(s_utf8WithoutBom.GetBytes(absoluteContent), File.ReadAllBytes(absolutePath));
            Assert.That(HasUtf8Bom(File.ReadAllBytes(absolutePath)), Is.False);
            Assert.That(_refreshCount, Is.EqualTo(1));
        }

        [Test]
        public void Write_Utf8BomDestination_RewritesWithoutBomAndPreservesMeta()
        {
            var destinationPath = AbsolutePath("Bom.gen.cs");
            var metaPath = destinationPath + ".meta";
            var content = "same text";
            var metaBytes = s_utf8WithoutBom.GetBytes("fileFormatVersion: 2\nguid: fedcba9876543210fedcba9876543210\n");
            File.WriteAllBytes(destinationPath, WithUtf8Bom(s_utf8WithoutBom.GetBytes(content)));
            File.WriteAllBytes(metaPath, metaBytes);
            SetLastWriteTimeUtc(metaPath, DateTime.UtcNow.AddHours(-2));
            var metaTimestamp = File.GetLastWriteTimeUtc(metaPath);
            var writer = CreateWriter();

            var result = writer.Write(Batch(Code(destinationPath, content)), default);

            Assert.That(result, Is.EqualTo(1));
            CollectionAssert.AreEqual(s_utf8WithoutBom.GetBytes(content), File.ReadAllBytes(destinationPath));
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaPath));
            Assert.That(File.GetLastWriteTimeUtc(metaPath), Is.EqualTo(metaTimestamp));
            Assert.That(_refreshCount, Is.EqualTo(1));
        }

        [Test]
        public void Write_UnchangedDestination_PreservesTimestampAndMeta()
        {
            var destinationPath = AbsolutePath("Unchanged.gen.cs");
            var metaPath = destinationPath + ".meta";
            var content = "same\r\ncontent";
            var metaBytes = s_utf8WithoutBom.GetBytes("fileFormatVersion: 2\nguid: 0123456789abcdef0123456789abcdef\n");
            File.WriteAllBytes(destinationPath, s_utf8WithoutBom.GetBytes(content));
            File.WriteAllBytes(metaPath, metaBytes);
            SetLastWriteTimeUtc(destinationPath, DateTime.UtcNow.AddHours(-2));
            SetLastWriteTimeUtc(metaPath, DateTime.UtcNow.AddHours(-2));
            var destinationTimestamp = File.GetLastWriteTimeUtc(destinationPath);
            var metaTimestamp = File.GetLastWriteTimeUtc(metaPath);
            var writer = CreateWriter();

            var result = writer.Write(Batch(Code(destinationPath, content)), default);

            Assert.That(result, Is.Zero);
            Assert.That(File.GetLastWriteTimeUtc(destinationPath), Is.EqualTo(destinationTimestamp));
            Assert.That(File.GetLastWriteTimeUtc(metaPath), Is.EqualTo(metaTimestamp));
            CollectionAssert.AreEqual(metaBytes, File.ReadAllBytes(metaPath));
            Assert.That(_refreshCount, Is.Zero);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("../Outside.gen.cs")]
        [TestCase("Library/PackageCache/Outside.gen.cs")]
        [TestCase("Packages/../Library/Outside.gen.cs")]
        public void Write_InvalidDestination_RejectsCompleteBatchBeforeOutput(string invalidPath)
        {
            var validPath = AbsolutePath("Valid.gen.cs");
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(validPath, "valid"), Code(invalidPath, "invalid")), default)
            );

            Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_WRITE_INVALID_PATH"));
            Assert.That(File.Exists(validPath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_ExternalAbsoluteDestination_RejectsCompleteBatchBeforeOutput()
        {
            var validPath = AbsolutePath("Valid.gen.cs");
            var externalPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gen.cs");
            var writer = CreateWriter();

            try
            {
                Assert.Throws<CodeGenRunException>(() =>
                    writer.Write(Batch(Code(validPath, "valid"), Code(externalPath, "external")), default)
                );
                Assert.That(File.Exists(validPath), Is.False);
                Assert.That(File.Exists(externalPath), Is.False);
                Assert.That(_refreshCount, Is.Zero);
            }
            finally
            {
                if (File.Exists(externalPath))
                {
                    File.Delete(externalPath);
                }
            }
        }

        [Test]
        public void Write_DuplicateCanonicalDestination_RejectsEvenWhenContentMatches()
        {
            var absolutePath = AbsolutePath("Duplicate.gen.cs");
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(RelativePath("Duplicate.gen.cs"), "same"), Code(absolutePath, "same")), default)
            );

            Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_WRITE_DUPLICATE_PATH"));
            Assert.That(File.Exists(absolutePath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_NullBatchContracts_RejectBeforeOutput()
        {
            var destinationPath = AbsolutePath("Valid.gen.cs");
            var writer = CreateWriter();

            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(new GeneratedCodeBatch(null, Array.Empty<CodeGenDiagnostic>(), false), default)
            );
            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(new GeneratedCodeBatch(Array.Empty<GeneratedCode>(), null, false), default)
            );
            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(destinationPath, null)), default)
            );
            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(
                      new GeneratedCodeBatch(
                            Array.Empty<GeneratedCode>()
                          , new[] {
                              new CodeGenDiagnostic(default, null, "message", string.Empty, 0, 0),
                          }
                          , false
                      )
                    , default
                )
            );

            Assert.That(File.Exists(destinationPath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_InvalidUnicodeContent_RejectsBeforeOutput()
        {
            var destinationPath = AbsolutePath("InvalidUnicode.gen.cs");
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(destinationPath, "\uD800")), default)
            );

            Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_WRITE_INVALID_BATCH"));
            Assert.That(File.Exists(destinationPath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_AllCandidatesSkippedWithOutput_RejectsBeforeOutput()
        {
            var destinationPath = AbsolutePath("Skipped.gen.cs");
            var writer = CreateWriter();

            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(
                      new GeneratedCodeBatch(
                            new[] { Code(destinationPath, "invalid") }
                          , Array.Empty<CodeGenDiagnostic>()
                          , true
                      )
                    , default
                )
            );

            Assert.That(File.Exists(destinationPath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_AllCandidatesSkippedWithoutOutput_DoesNotRefresh()
        {
            var writer = CreateWriter();

            var result = writer.Write(
                  new GeneratedCodeBatch(Array.Empty<GeneratedCode>(), Array.Empty<CodeGenDiagnostic>(), true)
                , default
            );

            Assert.That(result, Is.Zero);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_ReparseAncestor_RejectsBeforeOutput()
        {
            var destinationPath = AbsolutePath("Reparse", "Output.gen.cs");
            var reparsePath = AbsolutePath("Reparse");
            Directory.CreateDirectory(reparsePath);
            _getAttributes = path => PathsEqual(path, reparsePath)
                ? FileAttributes.Directory | FileAttributes.ReparsePoint
                : File.GetAttributes(path);
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(destinationPath, "content")), default)
            );

            Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_WRITE_INVALID_PATH"));
            Assert.That(File.Exists(destinationPath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_ExistingDirectoryDestination_RejectsBeforeOutput()
        {
            var destinationPath = AbsolutePath("Directory.gen.cs");
            Directory.CreateDirectory(destinationPath);
            var writer = CreateWriter();

            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(destinationPath, "content")), default)
            );

            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_SecondCommitFailure_RollsBackFirstDestination()
        {
            var firstPath = AbsolutePath("A.gen.cs");
            var secondPath = AbsolutePath("B.gen.cs");
            WriteText(firstPath, "old-a");
            WriteText(secondPath, "old-b");
            var replaceCount = 0;
            _replaceFile = (source, destination, backup) =>
            {
                replaceCount++;

                if (replaceCount == 2)
                {
                    throw new IOException("Injected commit failure.");
                }

                File.Replace(source, destination, backup);
            };
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(firstPath, "new-a"), Code(secondPath, "new-b")), default)
            );

            Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_WRITE_COMMIT_FAILED"));
            Assert.That(ReadText(firstPath), Is.EqualTo("old-a"));
            Assert.That(ReadText(secondPath), Is.EqualTo("old-b"));
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_CommitFailure_RollsBackNewDestinationAndCreatedDirectories()
        {
            var newDirectory = AbsolutePath("A", "Nested");
            var newPath = Path.Combine(newDirectory, "New.gen.cs");
            var existingPath = AbsolutePath("Z.gen.cs");
            WriteText(existingPath, "old");
            _replaceFile = (_, _, _) => throw new IOException("Injected commit failure.");
            var writer = CreateWriter();

            Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(newPath, "new"), Code(existingPath, "changed")), default)
            );

            Assert.That(File.Exists(newPath), Is.False);
            Assert.That(Directory.Exists(newDirectory), Is.False);
            Assert.That(ReadText(existingPath), Is.EqualTo("old"));
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_RollbackFailure_ReportsAffectedDestination()
        {
            var firstPath = AbsolutePath("A.gen.cs");
            var secondPath = AbsolutePath("B.gen.cs");
            WriteText(firstPath, "old-a");
            WriteText(secondPath, "old-b");
            var replaceCount = 0;
            _replaceFile = (source, destination, backup) =>
            {
                replaceCount++;

                if (replaceCount == 2)
                {
                    throw new IOException("Injected commit failure.");
                }

                if (replaceCount == 3)
                {
                    throw new IOException("Injected rollback failure.");
                }

                File.Replace(source, destination, backup);
            };
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(firstPath, "new-a"), Code(secondPath, "new-b")), default)
            );

            Assert.That(exception.Message, Does.Contain(firstPath));
            Assert.That(ReadText(firstPath), Is.EqualTo("new-a"));
            Assert.That(ReadText(secondPath), Is.EqualTo("old-b"));
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_CanceledBeforeCommit_AppliesNoOutput()
        {
            var destinationPath = AbsolutePath("Canceled.gen.cs");
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var writer = CreateWriter();

            Assert.Throws<OperationCanceledException>(() =>
                writer.Write(Batch(Code(destinationPath, "content")), cancellation.Token)
            );

            Assert.That(File.Exists(destinationPath), Is.False);
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_CanceledAfterStagingBeforeCommit_DiscardsStagingAndOutput()
        {
            var destinationPath = AbsolutePath("CanceledAfterStaging.gen.cs");
            var stagingRoot = Path.Combine(_projectRoot, "Library", "EncosyTower", "CodeGen", "Staging");
            var stagingDirectoryCount = GetDirectoryCount(stagingRoot);
            using var cancellation = new CancellationTokenSource();
            var projectRootReadCount = 0;
            _getAttributes = path =>
            {
                if (PathsEqual(path, _projectRoot))
                {
                    projectRootReadCount++;

                    if (projectRootReadCount == 2)
                    {
                        cancellation.Cancel();
                    }
                }

                return File.GetAttributes(path);
            };
            var writer = CreateWriter();

            Assert.Throws<OperationCanceledException>(() =>
                writer.Write(Batch(Code(destinationPath, "content")), cancellation.Token)
            );

            Assert.That(File.Exists(destinationPath), Is.False);
            Assert.That(GetDirectoryCount(stagingRoot), Is.EqualTo(stagingDirectoryCount));
            Assert.That(_refreshCount, Is.Zero);
        }

        [Test]
        public void Write_CanceledAfterCommitStarts_CompletesWholeCommit()
        {
            var firstPath = AbsolutePath("A.gen.cs");
            var secondPath = AbsolutePath("B.gen.cs");
            WriteText(firstPath, "old-a");
            WriteText(secondPath, "old-b");
            using var cancellation = new CancellationTokenSource();
            var replaceCount = 0;
            _replaceFile = (source, destination, backup) =>
            {
                replaceCount++;

                if (replaceCount == 1)
                {
                    cancellation.Cancel();
                }

                File.Replace(source, destination, backup);
            };
            var writer = CreateWriter();

            var result = writer.Write(Batch(Code(firstPath, "new-a"), Code(secondPath, "new-b")), cancellation.Token);

            Assert.That(result, Is.EqualTo(2));
            Assert.That(ReadText(firstPath), Is.EqualTo("new-a"));
            Assert.That(ReadText(secondPath), Is.EqualTo("new-b"));
            Assert.That(_refreshCount, Is.EqualTo(1));
        }

        [Test]
        public void Write_ExistingAndNewDestinations_UseAtomicReplaceAndMove()
        {
            var existingPath = AbsolutePath("Existing.gen.cs");
            var newPath = AbsolutePath("New.gen.cs");
            WriteText(existingPath, "old");
            var replaceCount = 0;
            var moveCount = 0;
            _replaceFile = (source, destination, backup) =>
            {
                replaceCount++;
                File.Replace(source, destination, backup);
            };
            _moveFile = (source, destination) =>
            {
                moveCount++;
                File.Move(source, destination);
            };
            var writer = CreateWriter();

            var result = writer.Write(Batch(Code(existingPath, "changed"), Code(newPath, "created")), default);

            Assert.That(result, Is.EqualTo(2));
            Assert.That(replaceCount, Is.EqualTo(1));
            Assert.That(moveCount, Is.EqualTo(1));
            Assert.That(_refreshCount, Is.EqualTo(1));
        }

        [Test]
        public void Write_RefreshFailure_RollsBackCommittedDestinations()
        {
            var firstPath = AbsolutePath("A.gen.cs");
            var secondPath = AbsolutePath("B.gen.cs");
            WriteText(firstPath, "old-a");
            WriteText(secondPath, "old-b");
            _refreshAssets = () =>
            {
                _refreshCount++;
                throw new InvalidOperationException("Injected refresh failure.");
            };
            var writer = CreateWriter();

            var exception = Assert.Throws<CodeGenRunException>(() =>
                writer.Write(Batch(Code(firstPath, "new-a"), Code(secondPath, "new-b")), default)
            );

            Assert.That(exception.Code, Is.EqualTo("ENCOSY_CODEGEN_WRITE_REFRESH_FAILED"));
            Assert.That(ReadText(firstPath), Is.EqualTo("old-a"));
            Assert.That(ReadText(secondPath), Is.EqualTo("old-b"));
            Assert.That(_refreshCount, Is.EqualTo(1));
        }

        private GeneratedCodeBatchWriter CreateWriter()
            => new(_refreshAssets, _moveFile, _replaceFile, _getAttributes);

        private void CountRefresh()
        {
            _refreshCount++;
        }

        private string AbsolutePath(params string[] components)
        {
            var path = _testRoot;

            for (var i = 0; i < components.Length; i++)
            {
                path = Path.Combine(path, components[i]);
            }

            return path;
        }

        private string RelativePath(params string[] components)
        {
            var path = _relativeTestRoot;

            for (var i = 0; i < components.Length; i++)
            {
                path = Path.Combine(path, components[i]);
            }

            return path;
        }

        private static GeneratedCodeBatch Batch(params GeneratedCode[] generatedCodes)
            => new(generatedCodes, Array.Empty<CodeGenDiagnostic>(), false);

        private static GeneratedCode Code(string filePath, string content)
            => new() {
                filePath = filePath,
                content = content,
            };

        private static void WriteText(string path, string content)
        {
            File.WriteAllBytes(path, s_utf8WithoutBom.GetBytes(content));
        }

        private static string ReadText(string path)
            => s_utf8WithoutBom.GetString(File.ReadAllBytes(path));

        private static void SetLastWriteTimeUtc(string path, DateTime timestamp)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    File.SetLastWriteTimeUtc(path, timestamp);
                    return;
                }
                catch (IOException) when (attempt < 99)
                {
                    Thread.Sleep(10);
                }
            }
        }

        private static bool HasUtf8Bom(byte[] bytes)
            => bytes.Length >= 3
                && bytes[0] == 0xEF
                && bytes[1] == 0xBB
                && bytes[2] == 0xBF;

        private static byte[] WithUtf8Bom(byte[] bytes)
        {
            var result = new byte[bytes.Length + 3];
            result[0] = 0xEF;
            result[1] = 0xBB;
            result[2] = 0xBF;
            Buffer.BlockCopy(bytes, 0, result, 3, bytes.Length);
            return result;
        }

        private static int GetDirectoryCount(string path)
            => Directory.Exists(path) ? Directory.GetDirectories(path).Length : 0;

        private static void DeleteCreatedEmptyDirectory(string path, bool existed)
        {
            if (existed == false
                && Directory.Exists(path)
                && Directory.GetFileSystemEntries(path).Length == 0
            )
            {
                Directory.Delete(path);
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
