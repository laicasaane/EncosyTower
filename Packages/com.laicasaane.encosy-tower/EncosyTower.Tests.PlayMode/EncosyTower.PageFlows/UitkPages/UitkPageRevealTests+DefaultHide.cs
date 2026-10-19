using System;
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
        [TestCase(RejectedHideOutcome.Dropped)]
        [TestCase(RejectedHideOutcome.Withdrawn)]
        [TestCase(RejectedHideOutcome.Cancelled)]
        public async Task RejectedHide_KeepsAnotherOperationsIncomingDefaultDisplayedAsync(RejectedHideOutcome outcome)
        {
            var defaultProbe = CreateProbe();
            var activeProbe = CreateProbe();
            var list = await CreateDefaultListAsync(defaultProbe, activeProbe);
            defaultProbe.SuspendShow = true;
            var show = ShowIndexAsync(list, index: 0, context: ZeroDuration, token: CancellationToken.None);
            Task<Success<PageFlowError>> hide = null;
            using var cancellation = new CancellationTokenSource();
            var source = new PageAbortSource();

            try
            {
                await WaitUntilAsync(HasEnteredShowOrCompleted);
                Assert.AreEqual(expected: 1, actual: defaultProbe.ShowCalls);
                Assert.IsTrue(IsDisplayed(defaultProbe.Page.Slot));
                Assert.IsTrue(list.CurrentPage.TryGetValue(out var previous));
                Assert.AreSame(activeProbe.Page, previous);

                var context = ZeroDuration with {
                    DefaultIndex = 0,
                    AsyncOperation = outcome == RejectedHideOutcome.Dropped
                        ? PageAsyncOperation.Drop
                        : PageAsyncOperation.Sequential,
                    Abort = source.Handle,
                };

                hide = HideAsync(list, context, cancellation.Token);

                if (outcome == RejectedHideOutcome.Withdrawn)
                {
                    Assert.IsTrue(source.Abort(PageAbortMode.Reverse).IsSuccess);
                }
                else if (outcome == RejectedHideOutcome.Cancelled)
                {
                    cancellation.Cancel();
                }

                var rejected = await hide;
                Assert.IsTrue(rejected.TryGetFailure(out var error));
                Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);

                if (outcome == RejectedHideOutcome.Dropped)
                {
                    Assert.IsTrue(error._error.TryGetValue(out PageFlowError.Error.InTransition _));
                }
                else if (outcome == RejectedHideOutcome.Withdrawn)
                {
                    Assert.IsTrue(error.IsWithdrawn);
                }
                else
                {
                    Assert.IsTrue(error.IsCancelled);
                }

                Assert.IsTrue(
                      IsDisplayed(defaultProbe.Page.Slot)
                    , "The rejected hide hid another call's incoming page."
                );
                Assert.IsTrue(list.IsInTransition);
                defaultProbe.ReleaseAll();
                Assert.IsTrue((await show).IsSuccess);
                Assert.IsTrue(list.CurrentPage.TryGetValue(out var current));
                Assert.AreSame(defaultProbe.Page, current);
                Assert.IsTrue(IsDisplayed(defaultProbe.Page.Slot));
                Assert.IsFalse(IsDisplayed(activeProbe.Page.Slot));
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                defaultProbe.ReleaseAll();
                await show;

                if (hide != null)
                {
                    await hide;
                }
            }

            bool HasEnteredShowOrCompleted()
                => defaultProbe.ShowCalls > 0 || show.IsCompleted;
        }

        [Test]
        public async Task DefaultHide_RevealsDefaultBeforeShowAndKeepsBothPagesOwnedAsync()
        {
            var defaultProbe = CreateProbe();
            var activeProbe = CreateProbe();
            var list = await CreateDefaultListAsync(defaultProbe, activeProbe);
            var hidden = await HideAsync(list, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(hidden.IsSuccess);
            Assert.IsTrue(list.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(defaultProbe.Page, current);
            Assert.IsFalse(defaultProbe.DisplayedDuringBeforeShow);
            Assert.IsFalse(defaultProbe.DisplayedDuringBeforeTransition);
            Assert.IsTrue(defaultProbe.DisplayedDuringShow);
            Assert.IsTrue(defaultProbe.DisplayedDuringTransitionShow);
            Assert.IsTrue(IsDisplayed(defaultProbe.Page.Slot));
            Assert.IsFalse(IsDisplayed(activeProbe.Page.Slot));
            Assert.AreEqual(expected: 2, actual: list.Pages.Count);
            Assert.AreEqual(expected: 0, actual: defaultProbe.ReturnCalls);
            Assert.AreEqual(expected: 0, actual: activeProbe.ReturnCalls);
            Assert.IsTrue(list.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
            Assert.AreEqual(expected: 0, actual: pooled);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(-1)]
        [TestCase(99)]
        public async Task DefaultHide_WithoutAValidDefaultHidesOnlyTheCurrentPageAsync(int defaultIndex)
        {
            var defaultProbe = CreateProbe();
            var activeProbe = CreateProbe();
            var list = await CreateDefaultListAsync(defaultProbe, activeProbe);
            var context = ZeroDuration with { DefaultIndex = defaultIndex };
            var hidden = await HideAsync(list, context, CancellationToken.None);

            Assert.IsTrue(hidden.IsSuccess);
            Assert.IsFalse(list.CurrentPage.HasValue);
            Assert.IsFalse(IsDisplayed(defaultProbe.Page.Slot));
            Assert.IsFalse(IsDisplayed(activeProbe.Page.Slot));
            Assert.AreEqual(expected: 0, actual: defaultProbe.ShowCalls);
            Assert.AreEqual(expected: 2, actual: list.Pages.Count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task DefaultHide_AbortAfterRevealRehidesTheUncommittedDefaultAsync()
        {
            var defaultProbe = CreateProbe();
            var activeProbe = CreateProbe();
            var list = await CreateDefaultListAsync(defaultProbe, activeProbe);
            var source = new PageAbortSource();
            defaultProbe.ShowAction = AbortDefault;
            var result = await HideAsync(list, ZeroDuration with { Abort = source.Handle }, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.AreEqual(PageTransitionState.Undone, error.TransitionState);
            Assert.IsTrue(defaultProbe.DisplayedDuringShow);
            Assert.IsTrue(list.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(activeProbe.Page, current);
            Assert.IsFalse(IsDisplayed(defaultProbe.Page.Slot));
            Assert.IsTrue(IsDisplayed(activeProbe.Page.Slot));
            Assert.IsFalse(list.IsInTransition);
            Assert.AreEqual(expected: 0, actual: defaultProbe.ReturnCalls);
            Assert.AreEqual(expected: 0, actual: activeProbe.ReturnCalls);
            LogAssert.NoUnexpectedReceived();

            static void AbortDefault(TestElementPage page, PageContext context)
            {
                Assert.IsTrue(context.Abort.Request(PageAbortMode.Reverse).IsSuccess);
            }
        }

        [Test]
        public async Task DefaultHide_CancellationAfterAbortRehidesTheUncommittedDefaultAsync()
        {
            var defaultProbe = CreateProbe();
            var activeProbe = CreateProbe();
            var list = await CreateDefaultListAsync(defaultProbe, activeProbe);
            using var cancellation = new CancellationTokenSource();
            var source = new PageAbortSource();
            defaultProbe.ThrowCancellationOnShow = true;
            defaultProbe.ShowAction = CancelDefault;
            var hide = HideAsync(list, ZeroDuration with { Abort = source.Handle }, cancellation.Token);

            await AssertCancelledAsync(hide);
            Assert.IsTrue(defaultProbe.DisplayedDuringShow);
            Assert.IsTrue(list.CurrentPage.TryGetValue(out var current));
            Assert.AreSame(activeProbe.Page, current);
            Assert.IsFalse(IsDisplayed(defaultProbe.Page.Slot));
            Assert.IsTrue(IsDisplayed(activeProbe.Page.Slot));
            Assert.IsFalse(list.IsInTransition);
            Assert.AreEqual(expected: 2, actual: list.Pages.Count);
            LogAssert.NoUnexpectedReceived();

            void CancelDefault(TestElementPage page, PageContext context)
            {
                Assert.IsTrue(context.Abort.Request(PageAbortMode.Reverse).IsSuccess);
                cancellation.Cancel();
            }
        }

        [Test]
        public async Task DefaultHide_CancelledCallDoesNotHideTheQueuedCallsCurrentDefaultAsync()
        {
            var defaultProbe = CreateProbe();
            var activeProbe = CreateProbe();
            var list = await CreateDefaultListAsync(defaultProbe, activeProbe);
            using var cancellation = new CancellationTokenSource();
            var source = new PageAbortSource();
            Task<Success<PageFlowError>> queued = null;
            defaultProbe.ThrowCancellationOnShow = true;
            defaultProbe.ShowAction = QueueAndCancel;

            try
            {
                var hide = HideAsync(list, ZeroDuration with { Abort = source.Handle }, cancellation.Token);
                await AssertCancelledAsync(hide);
                defaultProbe.ThrowCancellationOnShow = false;
                Assert.IsNotNull(queued);
                Assert.IsTrue((await queued).IsSuccess);
                Assert.IsTrue(list.CurrentPage.TryGetValue(out var current));
                Assert.AreSame(defaultProbe.Page, current);
                Assert.IsTrue(IsDisplayed(defaultProbe.Page.Slot));
                Assert.IsFalse(IsDisplayed(activeProbe.Page.Slot));
                Assert.AreEqual(expected: 2, actual: defaultProbe.ShowCalls);
                Assert.IsFalse(list.IsInTransition);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                defaultProbe.ShowAction = null;
                defaultProbe.ThrowCancellationOnShow = false;
                defaultProbe.ReleaseAll();

                if (queued != null)
                {
                    await queued;
                }
            }

            void QueueAndCancel(TestElementPage page, PageContext context)
            {
                defaultProbe.ShowAction = null;
                var queuedContext = ZeroDuration with { AsyncOperation = PageAsyncOperation.Sequential };
                queued = ShowIndexAsync(list, index: 0, context: queuedContext, token: CancellationToken.None);
                Assert.IsTrue(context.Abort.Request(PageAbortMode.Reverse).IsSuccess);
                cancellation.Cancel();
            }
        }

        private static async Task AssertCancelledAsync(Task operation)
        {
            OperationCanceledException cancelled = null;

            try
            {
                await operation;
            }
            catch (OperationCanceledException exception)
            {
                cancelled = exception;
            }

            Assert.IsNotNull(cancelled);
        }

        private async Task<UitkPageListSingle> CreateDefaultListAsync(
              UitkRevealProbe defaultProbe
            , UitkRevealProbe activeProbe
        )
        {
            var list = (UitkPageListSingle)await CreateFlowAsync(PageFlowKind.PageListSingle);
            var addedDefault = await list.AddPageAsync(ELEMENT_PAGE, ContextFor(defaultProbe), CancellationToken.None);
            Assert.IsTrue(addedDefault.IsSuccess);

            var addedActive = await list.AddPageAsync(ELEMENT_PAGE, ContextFor(activeProbe), CancellationToken.None);
            Assert.IsTrue(addedActive.IsSuccess);

            var shown = await ShowIndexAsync(list, index: 1, context: ZeroDuration, token: CancellationToken.None);
            Assert.IsTrue(shown.IsSuccess);
            Assert.IsFalse(IsDisplayed(defaultProbe.Page.Slot));
            return list;
        }

        private static async Task<Success<PageFlowError>> ShowIndexAsync(
              UitkPageListSingle list
            , int index
            , PageContext context
            , CancellationToken token
        )
            => await list.ShowPageAtIndexAsync(index, context, token);

        private static async Task<Success<PageFlowError>> HideAsync(
              UitkPageListSingle list
            , PageContext context
            , CancellationToken token
        )
            => await list.HideActivePageAsync(context, token);

        public enum RejectedHideOutcome
        {
            Dropped,
            Withdrawn,
            Cancelled,
        }
    }
}
