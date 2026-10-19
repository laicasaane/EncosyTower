using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.UitkPages;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed partial class UitkPageRevealTests
    {
        [TestCase(PageTransitionOptions.Default, true, false, 1, 1)]
        [TestCase(PageTransitionOptions.NoTransition, true, false, 0, 0)]
        [TestCase(PageTransitionOptions.NoTransition, true, true, 0, 1)]
        [TestCase(PageTransitionOptions.ZeroDuration, true, false, 1, 1)]
        [TestCase(PageTransitionOptions.Default, false, false, 1, 0)]
        [TestCase(PageTransitionOptions.NoTransition, false, false, 0, 0)]
        public async Task ShowOptions_RevealWithoutChangingOpacityOrCallbackPolicyAsync(
              PageTransitionOptions options
            , bool hasTransition
            , bool forceRunShow
            , int expectedPageCalls
            , int expectedTransitionCalls
        )
        {
            var flow = await CreateFlowAsync(PageFlowKind.PageStackSingle);
            var probe = CreateProbe();
            probe.HasTransition = hasTransition;
            probe.ForceRunShow = forceRunShow;
            var context = ContextFor(probe) with { ShowOptions = options };

            Assert.IsTrue((await ShowAsync(flow, context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue(probe.BeforeShowEntered);
            Assert.IsFalse(probe.DisplayedDuringBeforeShow);
            Assert.AreEqual(hasTransition, probe.BeforeTransitionEntered);
            Assert.AreEqual(expectedPageCalls, probe.ShowCalls);
            Assert.AreEqual(expectedTransitionCalls, probe.TransitionShowCalls);
            Assert.IsTrue(IsDisplayed(probe.Page.Slot));
            Assert.AreEqual(UitkRevealProbe.SENTINEL_OPACITY, probe.Page.Slot.style.opacity.value);

            if (expectedPageCalls > 0)
            {
                Assert.IsTrue(probe.DisplayedDuringShow);
            }

            if (expectedTransitionCalls > 0)
            {
                Assert.IsTrue(probe.DisplayedDuringTransitionShow);
                Assert.AreEqual(options, probe.TransitionShowOptions);
            }

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task ShowAll_KeepsEveryPageHiddenUntilEveryBeforeHookCompletesAsync()
        {
            var list = (UitkPageListMulti)await CreateFlowAsync(PageFlowKind.PageListMulti);
            var first = CreateProbe();
            var second = CreateProbe();
            first.SuspendBeforeShow = true;
            first.SuspendBeforeTransition = true;
            second.SuspendBeforeShow = true;
            second.SuspendBeforeTransition = true;

            var addedFirst = await list.AddPageAsync(ELEMENT_PAGE, ContextFor(first), CancellationToken.None);
            var addedSecond = await list.AddPageAsync(ELEMENT_PAGE, ContextFor(second), CancellationToken.None);
            Assert.IsTrue(addedFirst.IsSuccess);
            Assert.IsTrue(addedSecond.IsSuccess);
            Assert.AreSame(first.Page, list.Pages[0]);
            Assert.AreSame(second.Page, list.Pages[1]);
            var show = ShowAllAsync(list);

            try
            {
                await WaitUntilAsync(HasEnteredAllBeforeHooksOrCompleted);
                Assert.IsTrue(first.BeforeShowEntered && first.BeforeTransitionEntered);
                Assert.IsTrue(second.BeforeShowEntered && second.BeforeTransitionEntered);
                Assert.IsFalse(first.DisplayedDuringBeforeShow);
                Assert.IsFalse(first.DisplayedDuringBeforeTransition);
                Assert.IsFalse(second.DisplayedDuringBeforeShow);
                Assert.IsFalse(second.DisplayedDuringBeforeTransition);
                Assert.IsFalse(IsDisplayed(first.Page.Slot));
                Assert.IsFalse(IsDisplayed(second.Page.Slot));
                first.ReleaseAll();
                second.ReleaseBeforeShow();

                await WaitUntilAsync(HasCompletedAllPageBeforeHooksOrCompleted);
                Assert.IsTrue(first.BeforeTransitionCompleted);
                Assert.IsFalse(second.BeforeTransitionCompleted);
                Assert.AreEqual(expected: 0, actual: first.ShowCalls);
                Assert.AreEqual(expected: 0, actual: second.ShowCalls);
                Assert.IsFalse(IsDisplayed(first.Page.Slot));
                Assert.IsFalse(IsDisplayed(second.Page.Slot));
                second.ReleaseBeforeTransition();

                Assert.IsTrue((await show).IsSuccess);
                Assert.IsTrue(first.DisplayedDuringShow && first.DisplayedDuringTransitionShow);
                Assert.IsTrue(second.DisplayedDuringShow && second.DisplayedDuringTransitionShow);
                Assert.IsTrue(IsDisplayed(first.Page.Slot));
                Assert.IsTrue(IsDisplayed(second.Page.Slot));
                Assert.AreEqual(UitkRevealProbe.SENTINEL_OPACITY, first.Page.Slot.style.opacity.value);
                Assert.AreEqual(UitkRevealProbe.SENTINEL_OPACITY, second.Page.Slot.style.opacity.value);
                Assert.AreEqual(expected: 2, actual: list.Pages.Count);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                first.ReleaseAll();
                second.ReleaseAll();
                await show;
            }

            bool HasEnteredAllBeforeHooksOrCompleted()
                => show.IsCompleted || HaveAllBeforeHooksEntered();

            bool HaveAllBeforeHooksEntered()
                => first.BeforeShowEntered
                && first.BeforeTransitionEntered
                && second.BeforeShowEntered
                && second.BeforeTransitionEntered;

            bool HasCompletedAllPageBeforeHooksOrCompleted()
                => show.IsCompleted || (first.BeforeShowCompleted && second.BeforeShowCompleted);
        }

        private static async Task<Success<PageFlowError>> ShowAllAsync(UitkPageListMulti list)
            => await list.ShowAllPagesAsync(ZeroDuration, CancellationToken.None);
    }
}
