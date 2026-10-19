using System;
using System.Collections.Generic;
using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    [UxmlElement]
    public partial class TestElementPage
        : UitkPageElement
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnBeforeShowAsync
        , IPageOnShowAsync
        , IPageOnHideAsync
        , IPageOnAfterHide
        , IPageOnReturnToPool
    {
        private UitkRevealProbe _revealProbe;

        public TestElementPage()
        {
            TestFocusMode = DefaultFocusMode;
            TestFocusElementName = DefaultFocusElementName;
        }

        public static PageFocusMode DefaultFocusMode { get; set; }

        public static string DefaultFocusElementName { get; set; }

        public static PageAbortMode? AbortOnCreate { get; set; }

        public static PageAbortMode? AbortOnAttach { get; set; }

        public static Exception ThrowOnAttach { get; set; }

        public static CancellationTokenSource CancelOnAttach { get; set; }

        public static int AttachDelayFrames { get; set; }

        public static CancellationTokenSource CancelOnShow { get; set; }

        public static Exception ThrowOnReturnToPool { get; set; }

        public static List<string> Events { get; } = new();

        public int Counter { get; set; }

        public int HideDelayFrames { get; set; }

        public PageFocusMode TestFocusMode { get; set; }

        public string TestFocusElementName { get; set; }

        public override PageFocusMode FocusMode => TestFocusMode;

        public override string FocusElementName => TestFocusElementName;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            _revealProbe = null;

            if (context.UserData.TryGetValue(out var userData) && userData is UitkRevealProbe probe)
            {
                _revealProbe = probe;
                probe.OnCreated(this);
            }

            if (AbortOnCreate.HasValue)
            {
                _ = context.Abort.Request(AbortOnCreate.Value);
            }

            return UnityTask.FromResult(true);
        }

        public async UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
        {
            if (_revealProbe != null)
            {
                return await _revealProbe.OnAttachAsync(context, token);
            }

            for (var i = 0; i < AttachDelayFrames; i++)
            {
                await UnityTask.NextFrameAsync();
            }

            if (AbortOnAttach.HasValue)
            {
                _ = context.Abort.Request(AbortOnAttach.Value);
            }

            if (ThrowOnAttach != null)
            {
                throw ThrowOnAttach;
            }

            CancelOnAttach?.Cancel();
            return true;
        }

        public UnityTask OnBeforeShowAsync(PageContext context, CancellationToken token)
            => _revealProbe?.OnBeforeShowAsync() ?? UnityTask.CompletedTask;

        public UnityTask OnShowAsync(PageContext context, CancellationToken token)
        {
            if (_revealProbe != null)
            {
                return _revealProbe.OnShowAsync(context, token);
            }

            if (CancelOnShow == null)
            {
                return UnityTask.CompletedTask;
            }

            CancelOnShow.Cancel();
            return UnityTask.FromCanceled(token);
        }

        public async UnityTask OnHideAsync(PageContext context, CancellationToken token)
        {
            Events.Add($"Hide:{Counter}");

            for (var i = 0; i < HideDelayFrames; i++)
            {
                await UnityTask.NextFrameAsync();

                if (token.IsCancellationRequested)
                {
                    Events.Add($"HideCancelled:{Counter}");
                    token.ThrowIfCancellationRequested();
                }
            }
        }

        public void OnAfterHide(PageContext context)
        {
            Events.Add($"AfterHide:{Counter}");
        }

        public void OnReturnToPool(PageContext context)
        {
            _revealProbe?.OnReturned();
            Events.Add($"Return:{Counter}");

            if (ThrowOnReturnToPool != null)
            {
                throw ThrowOnReturnToPool;
            }
        }

        protected override IPageTransition CreateTransition(VisualElement slot)
            => _revealProbe != null ? _revealProbe.CreateTransition() : base.CreateTransition(slot);
    }

    [PageFlowScopeCollection]
    public partial struct UitkTestScopes
    {
        public PageFlowScope Screen { get; private set; }

        public PageFlowScope Popup { get; private set; }
    }

    public sealed class UitkTestInitializer : UitkPageCodexInitializer<UitkTestScopes>
    {
        public int CallCount { get; private set; }

        public UitkTestScopes Scopes { get; private set; }

        protected override UnityTask OnInitializeAsync(UitkPageCodex codex, UitkTestScopes scopes)
        {
            CallCount++;
            Scopes = scopes;
            return UnityTask.CompletedTask;
        }
    }
}
