using System;
using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    internal sealed class UitkRevealProbe
    {
        public const float SENTINEL_OPACITY = 0.37f;

        private readonly UnityTaskCompletionSource _attach = new();
        private readonly UnityTaskCompletionSource _beforeShow = new();
        private readonly UnityTaskCompletionSource _beforeTransition = new();
        private readonly UnityTaskCompletionSource _show = new();

        public TestElementPage Page { get; private set; }

        public bool SuspendAttach { get; set; }

        public bool SuspendBeforeShow { get; set; }

        public bool SuspendBeforeTransition { get; set; }

        public bool SuspendShow { get; set; }

        public bool AcceptAttach { get; set; } = true;

        public bool HasTransition { get; set; } = true;

        public bool ForceRunShow { get; set; }

        public bool ThrowCancellationOnShow { get; set; }

        public PageAbortMode? AbortOnAttach { get; set; }

        public CancellationTokenSource CancelOnAttach { get; set; }

        public Action<TestElementPage, PageContext> ShowAction { get; set; }

        public bool AttachEntered { get; private set; }

        public bool BeforeShowEntered { get; private set; }

        public bool BeforeShowCompleted { get; private set; }

        public bool BeforeTransitionEntered { get; private set; }

        public bool BeforeTransitionCompleted { get; private set; }

        public bool DisplayedDuringBeforeShow { get; private set; }

        public bool DisplayedDuringBeforeTransition { get; private set; }

        public bool DisplayedDuringShow { get; private set; }

        public bool DisplayedDuringTransitionShow { get; private set; }

        public int ShowCalls { get; private set; }

        public int TransitionShowCalls { get; private set; }

        public int ReturnCalls { get; private set; }

        public PageTransitionOptions TransitionShowOptions { get; private set; }

        public void OnCreated(TestElementPage page)
        {
            Page = page;
            page.Slot.style.opacity = SENTINEL_OPACITY;
        }

        public void OnReturned()
        {
            ReturnCalls++;
        }

        public void ReleaseAttach()
        {
            _attach.TrySetResult();
        }

        public void ReleaseBeforeShow()
        {
            _beforeShow.TrySetResult();
        }

        public void ReleaseBeforeTransition()
        {
            _beforeTransition.TrySetResult();
        }

        public void ReleaseAll()
        {
            ReleaseAttach();
            ReleaseBeforeShow();
            ReleaseBeforeTransition();
            _show.TrySetResult();
        }

        public async UnityTask<bool> OnAttachAsync(PageContext context, CancellationToken token)
        {
            AttachEntered = true;

            if (SuspendAttach)
            {
                await _attach.Task;
            }

            if (AbortOnAttach.HasValue)
            {
                _ = context.Abort.Request(AbortOnAttach.Value);
            }

            CancelOnAttach?.Cancel();
            return AcceptAttach;
        }

        public async UnityTask OnBeforeShowAsync()
        {
            BeforeShowEntered = true;
            DisplayedDuringBeforeShow = UitkPagesHarness.IsDisplayed(Page.Slot);

            if (SuspendBeforeShow)
            {
                await _beforeShow.Task;
            }

            BeforeShowCompleted = true;
        }

        public async UnityTask OnShowAsync(PageContext context, CancellationToken token)
        {
            ShowCalls++;
            DisplayedDuringShow = UitkPagesHarness.IsDisplayed(Page.Slot);
            ShowAction?.Invoke(Page, context);

            if (SuspendShow)
            {
                await _show.Task;
            }

            if (ThrowCancellationOnShow)
            {
                await UnityTask.FromCanceled(token);
            }
        }

        public IPageTransition CreateTransition()
            => HasTransition ? new RevealTransition(this) : null;

        private sealed class RevealTransition : IPageTransition
        {
            private readonly UitkRevealProbe _probe;

            public RevealTransition(UitkRevealProbe probe)
            {
                _probe = probe;
            }

            public bool ForceRunHide => false;

            public bool ForceRunShow => _probe.ForceRunShow;

            public async UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
            {
                if (context.Transition != PageTransition.Show)
                {
                    return;
                }

                _probe.BeforeTransitionEntered = true;
                _probe.DisplayedDuringBeforeTransition = UitkPagesHarness.IsDisplayed(_probe.Page.Slot);

                if (_probe.SuspendBeforeTransition)
                {
                    await _probe._beforeTransition.Task;
                }

                _probe.BeforeTransitionCompleted = true;
            }

            public void OnAfterTransition(PageTransitionContext context)
            {
            }

            public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
            {
                _probe.TransitionShowCalls++;
                _probe.TransitionShowOptions = context.ShowOptions;
                _probe.DisplayedDuringTransitionShow = UitkPagesHarness.IsDisplayed(_probe.Page.Slot);
                return UnityTask.CompletedTask;
            }

            public UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
                => UnityTask.CompletedTask;
        }
    }
}
