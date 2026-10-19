#if UNITY_UGUI

using System;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageCodexInitializerTests
    {
        private const string SCREEN = nameof(InitializerTestScopes.Screen);
        private const string POPUP = nameof(InitializerTestScopes.Popup);
        private const string INITIALIZER_FAILURE = "The codex initializer threw an exception: boom";

        private GameObject _gameObject;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject.IsValid())
            {
                UnityEngine.Object.Destroy(_gameObject);
            }
        }

        [Test]
        public async Task MatchingDefinitions_CallTheHookOnceWithTheScopesAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            initializer.Mode = ScriptedInitializer.InitializerMode.Complete;

            var result = codex.Initialize();
            await UnityTask.NextFrameAsync();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expected: 1, actual: initializer.CallCount);
            Assert.AreSame(codex, initializer.Codex);
            Assert.IsTrue(codex.TryGetFlowScope(SCREEN, out var screen));
            Assert.IsTrue(codex.TryGetFlowScope(POPUP, out var popup));
            Assert.AreEqual(screen, initializer.Scopes.Screen);
            Assert.AreEqual(popup, initializer.Scopes.Popup);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task MissingDefinition_ReportsTheMismatchAndSkipsTheHookAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN });

            var result = codex.Initialize();
            await UnityTask.NextFrameAsync();

            Assert.IsTrue(result.TryGetFailure(out var error));
            StringAssert.Contains("do not match the properties", error.ToMessage());
            StringAssert.Contains("Properties without a definition: Popup.", error.ToMessage());
            Assert.IsFalse(codex.GetFlowAt(0).TryGetValue(out _));
            Assert.IsFalse(codex.TryGetFlowScope(SCREEN, out _));
            Assert.AreEqual(expected: 0, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task MissingInitializer_ReportsFailureWithoutLoggingAsync()
        {
            _gameObject = new GameObject(nameof(UguiPageCodexInitializerTests), typeof(RectTransform));
            var codex = _gameObject.AddComponent<UguiPageCodex>();

            var result = codex.Initialize();
            await UnityTask.NextFrameAsync();

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(PageFlowCodexError.InitializerMissing().ToMessage(), error.ToMessage());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task FailedReinitialization_KeepsTheExistingFlowsAndScopesAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            Assert.IsTrue(codex.Initialize().IsSuccess);
            Assert.IsTrue(codex.GetFlowAt(0).TryGetValue(out var screen));
            Assert.IsTrue(codex.GetFlowAt(1).TryGetValue(out var popup));
            Assert.IsTrue(codex.TryGetFlowScope(POPUP, out var popupScope));

            codex._flows = CreateDefinitions(SCREEN);
            var result = codex.Initialize();
            await UnityTask.NextFrameAsync();

            Assert.IsTrue(result.IsFailure);
            Assert.IsTrue(codex.GetFlowAt(0).TryGetValue(out var currentScreen));
            Assert.IsTrue(codex.GetFlowAt(1).TryGetValue(out var currentPopup));
            Assert.AreSame(screen, currentScreen);
            Assert.AreSame(popup, currentPopup);
            Assert.IsTrue(codex.TryGetFlowScope(POPUP, out var currentPopupScope));
            Assert.AreEqual(popupScope, currentPopupScope);
            Assert.AreEqual(expected: 1, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task SuccessfulReinitialization_DestroysTheOldFlowsAndTheirPoolRootsAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            Assert.IsTrue(codex.Initialize().IsSuccess);
            Assert.IsTrue(codex.GetFlowAt(0).TryGetValue(out var oldScreen));
            Assert.IsTrue(codex.GetFlowAt(1).TryGetValue(out var oldPopup));
            var oldScreenPool = oldScreen.PoolTransform;
            var oldPopupPool = oldPopup.PoolTransform;
            Assert.IsTrue(oldScreenPool.IsValid());
            Assert.IsTrue(oldPopupPool.IsValid());

            var result = codex.Initialize();
            await UnityTask.NextFrameAsync();
            await UnityTask.NextFrameAsync();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(oldScreen.IsValid());
            Assert.IsFalse(oldPopup.IsValid());
            Assert.IsFalse(oldScreenPool.IsValid());
            Assert.IsFalse(oldPopupPool.IsValid());
            Assert.IsTrue(codex.GetFlowAt(0).TryGetValue(out var newScreen));
            Assert.IsTrue(codex.GetFlowAt(1).TryGetValue(out var newPopup));
            Assert.IsTrue(newScreen.IsValid());
            Assert.IsTrue(newPopup.IsValid());
            Assert.AreNotSame(oldScreen, newScreen);
            Assert.AreNotSame(oldPopup, newPopup);
            Assert.AreEqual(expected: 2, actual: codex.FlowCount);
            Assert.AreEqual(expected: 2, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task InitializeAsync_AwaitsTheInitializerCompletionAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            initializer.Mode = ScriptedInitializer.InitializerMode.AwaitTwoFrames;

            var task = codex.InitializeAsync();
            Assert.IsFalse(initializer.IsCompleted);
            var result = await task;

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(initializer.IsCompleted);
            Assert.AreEqual(expected: 1, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task InitializeAsync_ReportsInitializerExceptionWithoutLoggingAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            initializer.Mode = ScriptedInitializer.InitializerMode.Throw;

            var result = await codex.InitializeAsync();

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(INITIALIZER_FAILURE, error.ToMessage());
            Assert.AreEqual(expected: 1, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Awake_LogsInitializerExceptionOnceAsync()
        {
            var (codex, initializer) = CreateCodex(identifiers: new[] { SCREEN, POPUP }, activate: false);
            codex._flowContext.autoInitializeOnAwake = true;
            initializer.Mode = ScriptedInitializer.InitializerMode.Throw;
            var exceptionCount = 0;
            string exceptionText = null;

            LogAssert.Expect(LogType.Exception, "InvalidOperationException: boom");
            Application.logMessageReceived += CaptureException;

            try
            {
                _gameObject.SetActive(true);
                await UnityTask.NextFrameAsync();
                await UnityTask.NextFrameAsync();
                await UnityTask.NextFrameAsync();
            }
            finally
            {
                Application.logMessageReceived -= CaptureException;
            }

            Assert.AreEqual(expected: 1, actual: exceptionCount);
            StringAssert.Contains(INITIALIZER_FAILURE, exceptionText);
            Assert.AreEqual(expected: 1, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();

            void CaptureException(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Exception)
                {
                    exceptionCount++;
                    exceptionText = $"{condition}\n{stackTrace}";
                }
            }
        }

        [Test]
        public async Task Destroy_CancelsThePendingInitializerAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            initializer.Mode = ScriptedInitializer.InitializerMode.WaitForCancellationOnFirstCall;
            var task = codex.InitializeAsync();

            UnityEngine.Object.Destroy(_gameObject);
            await UnityTask.NextFrameAsync();
            await UnityTask.NextFrameAsync();

            Exception exception = null;

            try
            {
                await task;
            }
            catch (OperationCanceledException ex)
            {
                exception = ex;
            }

            Assert.IsInstanceOf<OperationCanceledException>(exception);
            Assert.IsTrue(codex.InitializationToken.IsCancellationRequested);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task Reinitialize_CancelsThePreviousInitializerAndCompletesTheNewOneAsync()
        {
            var (codex, initializer) = CreateCodex(new[] { SCREEN, POPUP });
            initializer.Mode = ScriptedInitializer.InitializerMode.WaitForCancellationOnFirstCall;
            var first = codex.InitializeAsync();
            var firstToken = codex.InitializationToken;

            var second = await codex.InitializeAsync();
            Exception exception = null;

            try
            {
                await first;
            }
            catch (OperationCanceledException ex)
            {
                exception = ex;
            }

            Assert.IsTrue(second.IsSuccess);
            Assert.IsInstanceOf<OperationCanceledException>(exception);
            Assert.IsTrue(firstToken.IsCancellationRequested);
            Assert.IsFalse(codex.InitializationToken.IsCancellationRequested);
            Assert.AreEqual(expected: 2, actual: initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        private static UguiPageCodex.FlowDefinition[] CreateDefinitions(params string[] identifiers)
        {
            var flows = new UguiPageCodex.FlowDefinition[identifiers.Length];

            for (var i = 0; i < identifiers.Length; i++)
            {
                flows[i] = new UguiPageCodex.FlowDefinition {
                    identifier = identifiers[i],
                    kind = PageFlowKind.PageStackSingle,
                };
            }

            return flows;
        }

        private (UguiPageCodex, ScriptedInitializer) CreateCodex(string[] identifiers, bool activate = true)
        {
            _gameObject = new GameObject(nameof(UguiPageCodexInitializerTests), typeof(RectTransform));
            _gameObject.SetActive(false);

            var codex = _gameObject.AddComponent<UguiPageCodex>();
            var initializer = _gameObject.AddComponent<ScriptedInitializer>();
            codex._flows = CreateDefinitions(identifiers);
            _gameObject.SetActive(activate);
            return (codex, initializer);
        }

        private sealed class ScriptedInitializer : UguiPageCodexInitializer<InitializerTestScopes>
        {
            public int CallCount { get; private set; }

            public UguiPageCodex Codex { get; private set; }

            public InitializerTestScopes Scopes { get; private set; }

            public bool IsCompleted { get; private set; }

            public InitializerMode Mode { get; set; }

            protected override async UnityTask OnInitializeAsync(UguiPageCodex codex, InitializerTestScopes scopes)
            {
                CallCount++;
                Codex = codex;
                Scopes = scopes;

                switch (Mode)
                {
                    case InitializerMode.AwaitTwoFrames:
                    {
                        await UnityTask.NextFrameAsync();
                        await UnityTask.NextFrameAsync();
                        IsCompleted = true;
                        return;
                    }

                    case InitializerMode.Throw:
                    {
                        await UnityTask.NextFrameAsync();
                        throw new InvalidOperationException("boom");
                    }

                    case InitializerMode.WaitForCancellationOnFirstCall when CallCount == 1:
                    {
                        while (true)
                        {
                            await UnityTask.NextFrameAsync(codex.InitializationToken);
                        }
                    }

                    default:
                    {
                        IsCompleted = true;
                        return;
                    }
                }
            }

            public enum InitializerMode
            {
                Complete,
                AwaitTwoFrames,
                Throw,
                WaitForCancellationOnFirstCall,
            }
        }
    }

    [PageFlowScopeCollection]
    internal partial struct InitializerTestScopes
    {
        public PageFlowScope Screen { get; private set; }

        public PageFlowScope Popup { get; private set; }
    }
}

#endif
