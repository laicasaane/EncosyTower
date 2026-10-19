using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.UitkPages;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed partial class UitkPageRevealTests
    {
        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private readonly List<UitkRevealProbe> _probes = new();
        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < _probes.Count; i++)
            {
                _probes[i].ReleaseAll();
            }

            _probes.Clear();
            _harness?.Dispose();
            _harness = null;
            TestElementPage.Events.Clear();
        }

        [TestCase(PageFlowKind.PageStackSingle)]
        [TestCase(PageFlowKind.PageStackMulti)]
        [TestCase(PageFlowKind.PageListSingle)]
        [TestCase(PageFlowKind.PageListMulti)]
        public async Task SuspendedPreparation_StaysHiddenUntilEveryBeforeHookCompletesAsync(PageFlowKind kind)
        {
            var flow = await CreateFlowAsync(kind);
            var probe = CreateProbe();
            probe.SuspendAttach = true;
            probe.SuspendBeforeShow = true;
            probe.SuspendBeforeTransition = true;
            var context = ContextFor(probe);
            var attach = AttachAsync(flow, context, CancellationToken.None);
            Task<Success<PageFlowError>> show = null;

            try
            {
                await WaitUntilAsync(HasAttachedOrCompleted);
                Assert.IsTrue(probe.AttachEntered);
                Assert.IsFalse(IsDisplayed(probe.Page.Slot), "The page became displayed during attachment.");
                Assert.AreEqual(UitkRevealProbe.SENTINEL_OPACITY, probe.Page.Slot.style.opacity.value);
                probe.ReleaseAttach();

                if (flow is UitkPageListSingle or UitkPageListMulti)
                {
                    Assert.IsTrue((await attach).IsSuccess);
                    show = ShowAsync(flow, context, CancellationToken.None);
                }
                else
                {
                    show = attach;
                }

                await WaitUntilAsync(HasBeforeHooksOrCompleted);
                Assert.IsTrue(probe.BeforeShowEntered);
                Assert.IsTrue(probe.BeforeTransitionEntered);
                Assert.IsFalse(probe.DisplayedDuringBeforeShow);
                Assert.IsFalse(probe.DisplayedDuringBeforeTransition);
                Assert.IsFalse(IsDisplayed(probe.Page.Slot));
                probe.ReleaseBeforeShow();

                await WaitUntilAsync(HasPageBeforeCompleted);
                Assert.IsTrue(probe.BeforeShowCompleted);
                Assert.IsFalse(probe.BeforeTransitionCompleted);
                Assert.IsFalse(IsDisplayed(probe.Page.Slot), "One completed before hook must not reveal the page.");
                probe.ReleaseBeforeTransition();

                Assert.IsTrue((await show).IsSuccess);
                Assert.IsTrue(probe.DisplayedDuringShow);
                Assert.IsTrue(probe.DisplayedDuringTransitionShow);
                Assert.IsTrue(IsDisplayed(probe.Page.Slot));
                Assert.AreEqual(UitkRevealProbe.SENTINEL_OPACITY, probe.Page.Slot.style.opacity.value);
                Assert.AreEqual(expected: 1, actual: probe.ShowCalls);
                Assert.AreEqual(expected: 1, actual: probe.TransitionShowCalls);
                Assert.AreSame(_harness.Codex, flow.Context.Owner);
                Assert.IsTrue(flow.TryGetPooledCount(ELEMENT_PAGE, out var pooled));
                Assert.AreEqual(expected: 0, actual: pooled);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                probe.ReleaseAll();
                await attach;

                if (show != null && ReferenceEquals(show, attach) == false)
                {
                    await show;
                }
            }

            bool HasAttachedOrCompleted()
                => probe.AttachEntered || attach.IsCompleted;

            bool HasBeforeHooksOrCompleted()
                => (probe.BeforeShowEntered && probe.BeforeTransitionEntered) || show.IsCompleted;

            bool HasPageBeforeCompleted()
                => probe.BeforeShowCompleted || show.IsCompleted;
        }

        private async Task<UitkPageFlow> CreateFlowAsync(PageFlowKind kind)
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);

            _harness = await CreateAsync(
                  ("Screen", kind, "")
                , ("Popup", PageFlowKind.PageStackSingle, "")
            );

            var flow = _harness.GetFlow<UitkPageFlow>("Screen");

            Assert.IsTrue((await flow.PrepoolPageAsync(
                  pageAssetKey: ELEMENT_PAGE
                , amount: 1
                , token: CancellationToken.None
            )).IsSuccess);

            return flow;
        }

        private UitkRevealProbe CreateProbe()
        {
            var probe = new UitkRevealProbe();
            _probes.Add(probe);
            return probe;
        }

        private static PageContext ContextFor(UitkRevealProbe probe)
            => ZeroDuration with { UserData = Option.Some<object>(probe) };

        private static async Task<Success<PageFlowError>> AttachAsync(
              UitkPageFlow flow
            , PageContext context
            , CancellationToken token
        )
        {
            return flow switch {
                UitkPageListSingle list => await list.AddPageAsync(ELEMENT_PAGE, context, token),
                UitkPageListMulti list => await list.AddPageAsync(ELEMENT_PAGE, context, token),
                _ => await ShowAsync(flow, context, token),
            };
        }

        private static async Task<Success<PageFlowError>> ShowAsync(
              UitkPageFlow flow
            , PageContext context
            , CancellationToken token
        )
        {
            return flow switch {
                UitkPageStackSingle stack => await stack.ShowPageAsync(ELEMENT_PAGE, context, token),
                UitkPageStackMulti stack => await stack.ShowPageAsync(ELEMENT_PAGE, context, token),
                UitkPageListSingle list => await list.ShowPageAtIndexAsync(index: 0, context: context, token: token),
                UitkPageListMulti list => await list.ShowPageAtIndexAsync(index: 0, context: context, token: token),
                _ => PageFlowError.Undefined(),
            };
        }
    }
}
