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
        [TestCase(PageFlowKind.PageStackSingle, AttachmentOutcome.Rejected)]
        [TestCase(PageFlowKind.PageStackMulti, AttachmentOutcome.Aborted)]
        [TestCase(PageFlowKind.PageListMulti, AttachmentOutcome.Cancelled)]
        public async Task FailedAttachment_StaysHiddenAndReturnsItsUnownedPageAsync(
              PageFlowKind kind
            , AttachmentOutcome outcome
        )
        {
            var flow = await CreateFlowAsync(kind);
            var probe = CreateProbe();
            probe.SuspendAttach = true;
            probe.AcceptAttach = outcome != AttachmentOutcome.Rejected;
            using var cancellation = new CancellationTokenSource();
            var source = new PageAbortSource();

            if (outcome == AttachmentOutcome.Aborted)
            {
                probe.AbortOnAttach = PageAbortMode.Reverse;
            }
            else if (outcome == AttachmentOutcome.Cancelled)
            {
                probe.CancelOnAttach = cancellation;
            }

            var context = ContextFor(probe) with { Abort = source.Handle };
            var operation = AttachAsync(flow, context, cancellation.Token);

            try
            {
                await WaitUntilAsync(HasAttachedOrCompleted);
                Assert.IsTrue(probe.AttachEntered);
                Assert.IsFalse(IsDisplayed(probe.Page.Slot));
                probe.ReleaseAttach();
                var result = await operation;
                Assert.IsTrue(result.TryGetFailure(out var error));
                Assert.IsFalse(result.IsApplied());

                if (outcome == AttachmentOutcome.Rejected)
                {
                    Assert.AreEqual(PageFlowError.AttachRejected(probe.Page).ToMessage(), error.ToMessage());
                }
                else if (outcome == AttachmentOutcome.Aborted)
                {
                    Assert.IsTrue(error.IsAborted);
                    Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
                    Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Page page));
                    Assert.AreSame(probe.Page, page.Source);
                    Assert.AreEqual(PageAbortMode.Reverse, mode);
                }
                else
                {
                    Assert.IsTrue(error.IsCancelled);
                    Assert.IsTrue(cancellation.IsCancellationRequested);
                }

                Assert.IsFalse(IsDisplayed(probe.Page.Slot));
                Assert.AreEqual(expected: 0, actual: probe.ShowCalls);
                Assert.AreEqual(expected: 0, actual: probe.TransitionShowCalls);
                Assert.AreEqual(expected: 1, actual: probe.ReturnCalls);
                Assert.IsTrue(flow.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
                Assert.AreEqual(expected: 1, actual: pooled);
                Assert.IsFalse(flow.IsInTransition);
                AssertHasNoPage(flow);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                probe.ReleaseAll();
                await operation;
            }

            bool HasAttachedOrCompleted()
                => probe.AttachEntered || operation.IsCompleted;
        }

        [Test]
        public async Task WithdrawnQueuedShow_NeverAttachesOrRevealsAndReturnsItsPageAsync()
        {
            var stack = (UitkPageStackSingle)await CreateFlowAsync(PageFlowKind.PageStackSingle);
            var activeProbe = CreateProbe();
            activeProbe.SuspendBeforeShow = true;
            var queuedProbe = CreateProbe();
            var source = new PageAbortSource();
            var active = ShowAsync(stack, ContextFor(activeProbe), CancellationToken.None);
            Task<Success<PageFlowError>> queued = null;

            try
            {
                await WaitUntilAsync(HasStartedActiveOrCompleted);
                Assert.IsTrue(activeProbe.BeforeShowEntered);

                var context = ContextFor(queuedProbe) with {
                    AsyncOperation = PageAsyncOperation.Sequential,
                    Abort = source.Handle,
                };

                queued = ShowAsync(stack, context, CancellationToken.None);
                await WaitUntilAsync(HasCreatedQueuedOrCompleted);
                Assert.IsNotNull(queuedProbe.Page);
                Assert.IsFalse(queuedProbe.AttachEntered);
                Assert.IsFalse(IsDisplayed(queuedProbe.Page.Slot));
                Assert.IsTrue(source.Abort(PageAbortMode.Reverse).IsSuccess);
                var withdrawn = await queued;
                Assert.IsTrue(withdrawn.TryGetFailure(out var error));
                Assert.IsTrue(error.IsWithdrawn);
                Assert.AreEqual(PageTransitionState.NotStarted, error.TransitionState);
                Assert.IsFalse(queuedProbe.AttachEntered);
                Assert.AreEqual(expected: 0, actual: queuedProbe.ShowCalls);
                Assert.AreEqual(expected: 1, actual: queuedProbe.ReturnCalls);
                Assert.IsFalse(IsDisplayed(queuedProbe.Page.Slot));
                activeProbe.ReleaseAll();
                Assert.IsTrue((await active).IsSuccess);
                Assert.IsTrue(stack.CurrentPage.TryGetValue(out var current));
                Assert.AreSame(activeProbe.Page, current);
                Assert.IsTrue(IsDisplayed(activeProbe.Page.Slot));
                Assert.IsTrue(stack.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
                Assert.AreEqual(expected: 1, actual: pooled);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                activeProbe.ReleaseAll();
                queuedProbe.ReleaseAll();
                await active;

                if (queued != null)
                {
                    await queued;
                }
            }

            bool HasStartedActiveOrCompleted()
                => activeProbe.BeforeShowEntered || active.IsCompleted;

            bool HasCreatedQueuedOrCompleted()
                => queuedProbe.Page != null || queued.IsCompleted;
        }

        private static void AssertHasNoPage(UitkPageFlow flow)
        {
            switch (flow)
            {
                case UitkPageStackSingle stack:
                {
                    Assert.IsFalse(stack.CurrentPage.HasValue);
                    break;
                }

                case UitkPageStackMulti stack:
                {
                    Assert.IsFalse(stack.CurrentPage.HasValue);
                    Assert.AreEqual(expected: 0, actual: stack.PageCollection.Count);
                    break;
                }

                case UitkPageListMulti list:
                {
                    Assert.AreEqual(expected: 0, actual: list.Pages.Count);
                    break;
                }
            }
        }

        public enum AttachmentOutcome
        {
            Rejected,
            Aborted,
            Cancelled,
        }
    }
}
