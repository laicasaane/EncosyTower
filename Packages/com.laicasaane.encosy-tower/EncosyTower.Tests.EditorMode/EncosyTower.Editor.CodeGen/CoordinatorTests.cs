#if UNITY_EDITOR

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.CodeGen;
using EncosyTower.Editor.CodeGen;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Editor.CodeGen
{
    [Category("Editor.CodeGen")]
    public sealed class CoordinatorTests
    {
        private static readonly GeneratedCodeBatch s_emptyBatch = new(
              Array.Empty<GeneratedCode>()
            , Array.Empty<CodeGenDiagnostic>()
            , AllCandidatesSkipped: false
        );
        private static readonly string[] s_sessionKeys = {
            CodeGenAutoTrigger.INITIAL_LOAD_HANDLED_KEY,
            CodeGenAutoTrigger.PENDING_AUTOMATIC_KEY,
            CodeGenAutoTrigger.SUPPRESS_NEXT_COMPILATION_KEY,
        };

        private SessionValue[] _sessionValues;

        [SetUp]
        public void SetUp()
        {
            _sessionValues = CaptureSessionValues();

            for (var i = 0; i < s_sessionKeys.Length; i++)
            {
                SessionState.EraseString(s_sessionKeys[i]);
            }
        }

        [TearDown]
        public void TearDown()
        {
            RestoreSessionValues(_sessionValues);
        }

        [Test]
        public void MenuValidation_DisablesOnlyForEditorBusyOrActiveRun()
        {
            Assert.That(CodeGenMenu.CanExecute(isEditorBusy: false, isRunning: false), Is.True);
            Assert.That(CodeGenMenu.CanExecute(isEditorBusy: true, isRunning: false), Is.False);
            Assert.That(CodeGenMenu.CanExecute(isEditorBusy: false, isRunning: true), Is.False);
        }

        [Test]
        public async Task ManualRequests_ForceNamedBackendAndIgnoreAutomaticSettings()
        {
            var unityCallCount = 0;
            var dotnetCallCount = 0;
            var writerCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => Generate(token, () => unityCallCount++)
                , token => Generate(token, () => dotnetCallCount++)
                , (batch, token) => Write(batch, token, () => writerCallCount++)
            );

            Assert.That(coordinator.RequestUnity(), Is.True);
            await coordinator.ActiveTask;
            Assert.That(coordinator.RequestDotnet(), Is.True);
            await coordinator.ActiveTask;

            Assert.That(unityCallCount, Is.EqualTo(1));
            Assert.That(dotnetCallCount, Is.EqualTo(1));
            Assert.That(writerCallCount, Is.EqualTo(2));
        }

        [Test]
        public async Task ManualRequest_ReplacesQueuedAutomaticRequest()
        {
            var unityCallCount = 0;
            var dotnetCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => Generate(token, () => unityCallCount++)
                , token => Generate(token, () => dotnetCallCount++)
                , WriteNothing
            );

            coordinator.QueueAutomatic(failedCompilation: false);
            Assert.That(coordinator.RequestDotnet(), Is.True);
            await coordinator.ActiveTask;

            Assert.That(
                  coordinator.TryDispatchAutomatic(new CodeGenSettings.Snapshot(true, false), isEditorBusy: false)
                , Is.False
            );
            Assert.That(unityCallCount, Is.Zero);
            Assert.That(dotnetCallCount, Is.EqualTo(1));
            AssertPendingAutomatic(isPresent: false);
        }

        [TestCase(false, false, 0, 0)]
        [TestCase(false, true, 0, 0)]
        [TestCase(true, false, 1, 0)]
        [TestCase(true, true, 0, 1)]
        public async Task AutomaticRequest_UsesExactSettingsMatrix(
              bool automaticallyGenerateCode
            , bool useDotnet
            , int expectedUnityCalls
            , int expectedDotnetCalls
        )
        {
            var unityCallCount = 0;
            var dotnetCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => Generate(token, () => unityCallCount++)
                , token => Generate(token, () => dotnetCallCount++)
                , WriteNothing
            );

            coordinator.QueueAutomatic(failedCompilation: false);
            var dispatched = coordinator.TryDispatchAutomatic(
                  new CodeGenSettings.Snapshot(automaticallyGenerateCode, useDotnet)
                , isEditorBusy: false
            );

            if (dispatched)
            {
                await coordinator.ActiveTask;
            }

            Assert.That(unityCallCount, Is.EqualTo(expectedUnityCalls));
            Assert.That(dotnetCallCount, Is.EqualTo(expectedDotnetCalls));
            AssertPendingAutomatic(isPresent: false);
        }

        [Test]
        public async Task FailedCompilationAutomatic_DispatchesOnlyDotnet()
        {
            var unityCallCount = 0;
            var dotnetCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => Generate(token, () => unityCallCount++)
                , token => Generate(token, () => dotnetCallCount++)
                , WriteNothing
            );

            coordinator.QueueAutomatic(failedCompilation: true);
            Assert.That(
                  coordinator.TryDispatchAutomatic(new CodeGenSettings.Snapshot(true, false), isEditorBusy: false)
                , Is.False
            );

            coordinator.QueueAutomatic(failedCompilation: true);
            Assert.That(
                  coordinator.TryDispatchAutomatic(new CodeGenSettings.Snapshot(true, true), isEditorBusy: false)
                , Is.True
            );
            await coordinator.ActiveTask;

            Assert.That(unityCallCount, Is.Zero);
            Assert.That(dotnetCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task AutomaticEvents_CoalesceToOnePendingRequest()
        {
            var unityCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => Generate(token, () => unityCallCount++)
                , token => Generate(token, static () => { })
                , WriteNothing
            );

            coordinator.QueueAutomatic(failedCompilation: false);
            coordinator.QueueAutomatic(failedCompilation: false);
            coordinator.QueueAutomatic(failedCompilation: false);

            Assert.That(
                  coordinator.TryDispatchAutomatic(new CodeGenSettings.Snapshot(true, false), isEditorBusy: false)
                , Is.True
            );
            await coordinator.ActiveTask;

            Assert.That(
                  coordinator.TryDispatchAutomatic(new CodeGenSettings.Snapshot(true, false), isEditorBusy: false)
                , Is.False
            );
            Assert.That(unityCallCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ActiveRun_RejectsManualRequestAndCancellationWritesNothing()
        {
            var source = new TaskCompletionSource<GeneratedCodeBatch>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            var writerCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => GenerateUntilCanceled(source, token)
                , token => Generate(token, static () => { })
                , (batch, token) => Write(batch, token, () => writerCallCount++)
            );

            Assert.That(coordinator.RequestUnity(), Is.True);
            Assert.That(coordinator.IsRunning, Is.True);
            Assert.That(coordinator.RequestDotnet(), Is.False);
            coordinator.Cancel();
            await coordinator.ActiveTask;

            Assert.That(coordinator.IsRunning, Is.False);
            Assert.That(writerCallCount, Is.Zero);
            AssertPendingAutomatic(isPresent: false);
        }

        [Test]
        public async Task AllCandidatesSkipped_WritesNothingAndClearsState()
        {
            var skippedBatch = new GeneratedCodeBatch(
                  Array.Empty<GeneratedCode>()
                , Array.Empty<CodeGenDiagnostic>()
                , AllCandidatesSkipped: true
            );
            var writerCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  token => Generate(token, skippedBatch)
                , token => Generate(token, s_emptyBatch)
                , (batch, token) => Write(batch, token, () => writerCallCount++)
            );

            Assert.That(coordinator.RequestUnity(), Is.True);
            await coordinator.ActiveTask;

            Assert.That(writerCallCount, Is.Zero);
            AssertPendingAutomatic(isPresent: false);
            AssertSuppression(isPresent: false);
        }

        [Test]
        public async Task WriterResult_LeavesOneSuppressionOnlyWhenContentChanged()
        {
            var changedCoordinator = new CodeGenCoordinator(
                  token => Generate(token, s_emptyBatch)
                , token => Generate(token, s_emptyBatch)
                , static (batch, token) => Write(batch, token, changedFileCount: 1)
            );

            Assert.That(changedCoordinator.RequestUnity(), Is.True);
            await changedCoordinator.ActiveTask;
            AssertSuppression(isPresent: true);

            CodeGenAutoTrigger.ClearOutputCompilationSuppression();
            var noOpCoordinator = new CodeGenCoordinator(
                  token => Generate(token, s_emptyBatch)
                , token => Generate(token, s_emptyBatch)
                , WriteNothing
            );

            Assert.That(noOpCoordinator.RequestUnity(), Is.True);
            await noOpCoordinator.ActiveTask;
            AssertSuppression(isPresent: false);
        }

        [Test]
        public async Task ExpectedFailure_WritesNothingAndClearsState()
        {
            var writerCallCount = 0;
            var coordinator = new CodeGenCoordinator(
                  static _ => Task.FromException<GeneratedCodeBatch>(
                      new CodeGenRunException("TEST_CODE", "Expected test failure.")
                  )
                , token => Generate(token, s_emptyBatch)
                , (batch, token) => Write(batch, token, () => writerCallCount++)
            );
            LogAssert.Expect(LogType.Error, new Regex(@"\[TEST_CODE\] Expected test failure\."));

            Assert.That(coordinator.RequestUnity(), Is.True);
            await coordinator.ActiveTask;

            Assert.That(writerCallCount, Is.Zero);
            Assert.That(coordinator.IsRunning, Is.False);
            AssertPendingAutomatic(isPresent: false);
            AssertSuppression(isPresent: false);
        }

        private static SessionValue[] CaptureSessionValues()
        {
            var values = new SessionValue[s_sessionKeys.Length];
            var sentinel = Guid.NewGuid().ToString("N");

            for (var i = 0; i < s_sessionKeys.Length; i++)
            {
                var value = SessionState.GetString(s_sessionKeys[i], sentinel);
                values[i] = new SessionValue(
                      exists: string.Equals(value, sentinel, StringComparison.Ordinal) == false
                    , value
                );
            }

            return values;
        }

        private static void RestoreSessionValues(SessionValue[] values)
        {
            for (var i = 0; i < s_sessionKeys.Length; i++)
            {
                if (values[i].Exists)
                {
                    SessionState.SetString(s_sessionKeys[i], values[i].Value);
                }
                else
                {
                    SessionState.EraseString(s_sessionKeys[i]);
                }
            }
        }

        private static void AssertPendingAutomatic(bool isPresent)
        {
            var value = SessionState.GetString(CodeGenAutoTrigger.PENDING_AUTOMATIC_KEY, string.Empty);
            Assert.That(value.Length > 0, Is.EqualTo(isPresent));
        }

        private static void AssertSuppression(bool isPresent)
        {
            var value = SessionState.GetString(CodeGenAutoTrigger.SUPPRESS_NEXT_COMPILATION_KEY, string.Empty);
            Assert.That(value.Length > 0, Is.EqualTo(isPresent));
        }

        private static Task<GeneratedCodeBatch> Generate(CancellationToken token, GeneratedCodeBatch batch)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(batch);
        }

        private static Task<GeneratedCodeBatch> Generate(CancellationToken token, Action countCall)
        {
            token.ThrowIfCancellationRequested();
            countCall();
            return Task.FromResult(s_emptyBatch);
        }

        private static Task<GeneratedCodeBatch> GenerateUntilCanceled(
              TaskCompletionSource<GeneratedCodeBatch> source
            , CancellationToken token
        )
        {
            token.Register(static state =>
            {
                ((TaskCompletionSource<GeneratedCodeBatch>)state).TrySetCanceled();
            }, source);
            return source.Task;
        }

        private static int WriteNothing(GeneratedCodeBatch batch, CancellationToken token)
            => Write(batch, token, changedFileCount: 0);

        private static int Write(GeneratedCodeBatch batch, CancellationToken token, int changedFileCount)
        {
            token.ThrowIfCancellationRequested();
            return changedFileCount;
        }

        private static int Write(GeneratedCodeBatch batch, CancellationToken token, Action countCall)
        {
            token.ThrowIfCancellationRequested();
            countCall();
            return 0;
        }

        private readonly struct SessionValue
        {
            internal SessionValue(bool exists, string value)
            {
                Exists = exists;
                Value = value;
            }

            internal bool Exists { get; }

            internal string Value { get; }
        }
    }
}

#endif
