using System.Threading;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed class UitkTransitionAndFocusTests
    {
        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            TestElementPage.DefaultFocusMode = PageFocusMode.None;
            TestElementPage.DefaultFocusElementName = null;
            _harness?.Dispose();
            _harness = null;
        }

        [Test]
        public async Task UssClassTransition_CompletesAfterTheUssTransition()
        {
            var slot = await CreateSlotAsync();
            var transition = new UitkUssClassTransition(slot) {
                EnterFromClass = "test-enter-from",
                EnterToClass = "test-enter-to",
            };

            var start = Time.realtimeSinceStartup;
            await transition.OnShowAsync(PageTransitionOptions.Default, CancellationToken.None);
            var elapsed = Time.realtimeSinceStartup - start;

            Assert.GreaterOrEqual(elapsed, 0.08f);
            Assert.IsTrue(slot.ClassListContains("test-enter-to"));
            Assert.IsFalse(slot.ClassListContains("test-enter-from"));
            Assert.IsFalse(transition.IsRunning);
        }

        [Test]
        public async Task UssClassTransition_WithoutTransitionProperty_CompletesWithinAFewFrames()
        {
            var slot = await CreateSlotAsync();
            var transition = new UitkUssClassTransition(slot) {
                EnterFromClass = "test-plain-from",
                EnterToClass = "test-plain-to",
            };

            var startFrame = Time.frameCount;
            await transition.OnShowAsync(PageTransitionOptions.Default, CancellationToken.None);

            Assert.Less(Time.frameCount - startFrame, 10);
            Assert.IsTrue(slot.ClassListContains("test-plain-to"));
        }

        [Test]
        public async Task UssClassTransition_Cancellation_AppliesTheEndClass()
        {
            var slot = await CreateSlotAsync();
            var transition = new UitkUssClassTransition(slot) {
                EnterFromClass = "test-slow-from",
                EnterToClass = "test-slow-to",
            };

            using var cancellation = new CancellationTokenSource();
            var start = Time.realtimeSinceStartup;
            var task = transition.OnShowAsync(PageTransitionOptions.Default, cancellation.Token);

            await WaitFramesAsync(3);
            cancellation.Cancel();
            await task;

            Assert.Less(Time.realtimeSinceStartup - start, 2f);
            Assert.IsTrue(slot.ClassListContains("test-slow-to"));
            Assert.IsFalse(slot.ClassListContains("test-slow-from"));
            Assert.IsFalse(transition.IsRunning);
        }

        [Test]
        public async Task UssClassTransition_Preset_EndsFullyVisible()
        {
            var slot = await CreateSlotAsync();
            var transition = new UitkUssClassTransition(slot);

            await transition.OnShowAsync(PageTransitionOptions.Default, CancellationToken.None);

            Assert.AreEqual(1f, slot.style.opacity.value, 0.0001f);
            Assert.IsFalse(transition.IsRunning);
        }

        [Test]
        public async Task ValueAnimationTransition_AppliesEndValues()
        {
            var slot = await CreateSlotAsync();
            var transition = new UitkValueAnimationTransition(slot) {
                ShowDurationMs = 50,
                HideDurationMs = 50,
            };

            await transition.OnShowAsync(PageTransitionOptions.Default, CancellationToken.None);
            Assert.AreEqual(1f, slot.style.opacity.value, 0.0001f);

            await transition.OnHideAsync(PageTransitionOptions.Default, CancellationToken.None);
            Assert.AreEqual(0f, slot.style.opacity.value, 0.0001f);
        }

        [Test]
        public async Task ValueAnimationTransition_Cancellation_AppliesTheEndValue()
        {
            var slot = await CreateSlotAsync();
            var transition = new UitkValueAnimationTransition(slot, UitkValueAnimationPreset.SlideFromBottom) {
                ShowDurationMs = 5000,
            };

            using var cancellation = new CancellationTokenSource();
            var start = Time.realtimeSinceStartup;
            var task = transition.OnShowAsync(PageTransitionOptions.Default, cancellation.Token);

            await WaitFramesAsync(3);
            cancellation.Cancel();
            await task;

            Assert.Less(Time.realtimeSinceStartup - start, 2f);
            Assert.AreEqual(1f, slot.style.opacity.value, 0.0001f);
            Assert.IsFalse(transition.IsRunning);
        }

        [Test]
        public async Task FirstFocusable_FocusesTheFirstFocusableElementOnShow()
        {
            TestElementPage.DefaultFocusMode = PageFocusMode.FirstFocusable;
            var stack = await CreateStackAsync();

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            Assert.AreEqual("first-button", GetFocusedName(stack));
        }

        [Test]
        public async Task NamedFocus_FocusesTheNamedElementOnShow()
        {
            TestElementPage.DefaultFocusMode = PageFocusMode.Named;
            TestElementPage.DefaultFocusElementName = "named-button";
            var stack = await CreateStackAsync();

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            Assert.AreEqual("named-button", GetFocusedName(stack));
        }

        [Test]
        public async Task Hide_BlursTheFocusedElementInsideThePage()
        {
            TestElementPage.DefaultFocusMode = PageFocusMode.FirstFocusable;
            var stack = await CreateStackAsync();

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.AreEqual("first-button", GetFocusedName(stack));

            await stack.HideActivePageAsync(ZeroDuration, CancellationToken.None);

            Assert.IsNull(GetFocusedName(stack));
        }

        [Test]
        public async Task Pop_ReturnsFocusToThePreviousElement()
        {
            TestElementPage.DefaultFocusMode = PageFocusMode.FirstFocusable;
            var stack = await CreateStackAsync();

            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            var firstFocused = stack.FlowElement.panel.focusController.focusedElement;

            TestElementPage.DefaultFocusMode = PageFocusMode.Named;
            TestElementPage.DefaultFocusElementName = "named-button";
            await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);
            Assert.AreNotSame(firstFocused, stack.FlowElement.panel.focusController.focusedElement);

            await stack.HideActivePageAsync(ZeroDuration, CancellationToken.None);

            Assert.AreSame(firstFocused, stack.FlowElement.panel.focusController.focusedElement);
        }

        private async Task<VisualElement> CreateSlotAsync()
        {
            _harness = await CreateAsync(
                  ("Screen", PageFlowKind.SinglePageStack, "")
                , ("Popup", PageFlowKind.SinglePageStack, "")
            );

            _harness.Codex.PanelRoot.styleSheets.Add(Resources.Load<StyleSheet>(TRANSITIONS));

            var slot = new VisualElement();
            _harness.Codex.CodexRoot.Add(slot);
            await WaitFramesAsync(2);
            return slot;
        }

        private async Task<UitkMultiPageStack> CreateStackAsync()
        {
            _harness = await CreateAsync(
                  ("Screen", PageFlowKind.MultiPageStack, "")
                , ("Popup", PageFlowKind.SinglePageStack, "")
            );

            return _harness.GetFlow<UitkMultiPageStack>("Screen");
        }

        private static string GetFocusedName(UitkPageFlow flow)
            => (flow.FlowElement.panel.focusController.focusedElement as VisualElement)?.name;
    }
}
