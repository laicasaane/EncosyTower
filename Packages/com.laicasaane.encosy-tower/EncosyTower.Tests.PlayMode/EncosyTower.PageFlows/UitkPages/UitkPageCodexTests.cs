using System;
using System.Threading.Tasks;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Codex;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    public sealed class UitkPageCodexTests
    {
        private const string SCREEN = nameof(UitkTestScopes.Screen);
        private const string POPUP = nameof(UitkTestScopes.Popup);

        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            _harness?.Dispose();
            _harness = null;
        }

        [Test]
        public async Task FailedReinitialization_KeepsTheExistingFlowsInitializedAsync()
        {
            await CreateCodexAsync();
            var codex = _harness.Codex;
            var oldScreen = _harness.GetFlow<UitkPageStackSingle>(SCREEN);
            var oldPopup = _harness.GetFlow<UitkPageStackMulti>(POPUP);
            Assert.IsTrue(codex.TryGetFlowScope(POPUP, out var oldPopupScope));

            codex._flows = new[] {
                new UitkPageCodex.FlowDefinition {
                    identifier = SCREEN,
                    kind = PageFlowKind.PageStackSingle,
                    containerName = string.Empty,
                },
            };

            var result = codex.Initialize();

            Assert.IsTrue(result.TryGetFailure(out var error));
            StringAssert.Contains("Properties without a definition: Popup.", error.ToMessage());
            Assert.AreEqual(expected: 2, actual: codex.FlowCount);
            Assert.IsTrue(codex.GetFlow(SCREEN).TryGetValue(out var screen));
            Assert.IsTrue(codex.GetFlow(POPUP).TryGetValue(out var popup));
            Assert.AreSame(oldScreen, screen);
            Assert.AreSame(oldPopup, popup);
            Assert.IsTrue(screen.IsInitialized);
            Assert.IsTrue(popup.IsInitialized);
            Assert.IsTrue(codex.TryGetFlowScope(POPUP, out var popupScope));
            Assert.AreEqual(oldPopupScope, popupScope);
            Assert.AreEqual(expected: 1, actual: _harness.Initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task SuccessfulReinitialization_DisposesTheOldFlowsAndCreatesNewOnesAsync()
        {
            await CreateCodexAsync();
            var codex = _harness.Codex;
            var oldScreen = _harness.GetFlow<UitkPageStackSingle>(SCREEN);
            var oldPopup = _harness.GetFlow<UitkPageStackMulti>(POPUP);

            var result = codex.Initialize();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expected: 2, actual: codex.FlowCount);
            Assert.IsFalse(oldScreen.IsInitialized);
            Assert.IsFalse(oldPopup.IsInitialized);
            Assert.IsTrue(codex.GetFlow(SCREEN).TryGetValue(out var screen));
            Assert.IsTrue(codex.GetFlow(POPUP).TryGetValue(out var popup));
            Assert.AreNotSame(oldScreen, screen);
            Assert.AreNotSame(oldPopup, popup);
            Assert.IsTrue(screen.IsInitialized);
            Assert.IsTrue(popup.IsInitialized);
            Assert.AreEqual(expected: 2, actual: _harness.Initializer.CallCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task FlowInitializedTwice_ThrowsAndKeepsItsElementAndScopeAsync()
        {
            await CreateCodexAsync();
            var flow = _harness.GetFlow<UitkPageStackSingle>(SCREEN);
            var element = flow.FlowElement;
            var scope = flow.Context.FlowScope;
            InvalidOperationException thrown = null;

            try
            {
                flow.Initialize(
                      identifier: flow.Identifier
                    , containerName: flow.ContainerName
                    , codex: flow.Codex
                    , context: flow.Context
                    , behaviourParent: null
                );
            }
            catch (InvalidOperationException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.AreSame(expected: element, actual: flow.FlowElement);
            Assert.AreEqual(expected: scope, actual: flow.Context.FlowScope);
            Assert.IsTrue(flow.IsInitialized);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task InitializeAsync_CompletesTheSecondInitializationAsync()
        {
            await CreateCodexAsync();

            var result = await _harness.Codex.InitializeAsync();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expected: 2, actual: _harness.Initializer.CallCount);
            Assert.AreEqual(expected: 2, actual: _harness.Codex.FlowCount);
            Assert.IsTrue(_harness.Codex.TryGetFlowScope(SCREEN, out var screen));
            Assert.IsTrue(_harness.Codex.TryGetFlowScope(POPUP, out var popup));
            Assert.AreEqual(screen, _harness.Initializer.Scopes.Screen);
            Assert.AreEqual(popup, _harness.Initializer.Scopes.Popup);
            LogAssert.NoUnexpectedReceived();
        }

        private async Task CreateCodexAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);

            _harness = await UitkPagesHarness.CreateAsync(
                  (SCREEN, PageFlowKind.PageStackSingle, string.Empty)
                , (POPUP, PageFlowKind.PageStackMulti, string.Empty)
            );

            Assert.AreEqual(expected: 1, actual: _harness.Initializer.CallCount);
            Assert.AreEqual(expected: 2, actual: _harness.Codex.FlowCount);
        }
    }
}
