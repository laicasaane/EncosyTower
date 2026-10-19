using System;
using System.Collections.Generic;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Codex;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Overlays;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Tasks;
using UnityEngine.UIElements;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Attachments
{
    /// <summary>Creates, lends and takes back the attachment sets of the screens.</summary>
    /// <remarks>
    /// The navigation bar, the tabs and the cards are pages in flows of their own, attached to their screen and shown
    /// with it. At most two sets exist at a time, one for the screen that leaves and one for the screen that arrives.
    /// The flows are created with <see cref="UitkPageFlow.TryCreate"/> and a copy of the codex flow context.
    /// </remarks>
    internal sealed class PageAttachments : IDisposable
    {
        public const string NAVIGATION_BAR_FLOW = "NavigationBar";
        public const string TABS_FLOW = "Tabs";
        public const string CARDS_FLOW = "Cards";
        public const int MAX_SETS = 2;

        private readonly List<AttachmentSet> _sets = new(MAX_SETS);
        private readonly AttachmentSetFactory _factory;
        private readonly AppState _state;

        private NavigationBarStatus _navigationBarStatus;

        public PageAttachments(UitkPageCodex codex, AppState state)
        {
            _state = state;
            _factory = new AttachmentSetFactory(
                  codex
                , state
                , (scope, kind) => FlowCreated?.Invoke(scope, kind)
                , scope => FlowDropped?.Invoke(scope)
            );
        }

        public event Action<PageFlowScope, PageFlowKind> FlowCreated;

        public event Action<PageFlowScope> FlowDropped;

        public FloatingContextPage FloatingContext { get; set; }

        public ToastPage Toast { get; set; }

        private static PageContext CreateContext(object userData)
            => new() {
                AsyncOperation = PageAsyncOperation.Sequential,
                DefaultIndex = -1,
                ShowOptions = PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.ZeroDuration,
                UserData = Option.Some(userData),
            };

        private static bool IsShown(IUitkPage page)
            => page is UitkPageElement element
            && element.Slot != null
            && element.Slot.style.display.value != DisplayStyle.None;

        public bool TryGetActive(BookLocation location, out AttachmentSet set)
        {
            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                var candidate = _sets[i];

                if (candidate.InUse && candidate.Location == location)
                {
                    set = candidate;
                    return true;
                }
            }

            set = null;
            return false;
        }

        public async UnityTask<AttachmentSet> RentAsync(
              BookLocation location
            , VisualElement navigationBarHost
            , VisualElement detailHost
            , VisualElement cardsHost
            , CancellationToken token
        )
        {
            var set = TakeSet();

            set.InUse = true;
            set.Location = location;

            try
            {
                navigationBarHost.Add(set.NavigationBarFlow.FlowElement);

                if (detailHost != null)
                {
                    detailHost.Add(set.TabsFlow.FlowElement);
                }

                if (cardsHost != null)
                {
                    cardsHost.Add(set.CardsFlow.FlowElement);
                }

                if (set.IsPopulated == false && await PopulateAsync(set, token) == false)
                {
                    token.ThrowIfCancellationRequested();
                    ThrowHelper.ThrowAttachmentSetPopulateFailed(location);
                }

                if (detailHost != null && cardsHost != null)
                {
                    await ApplyViewAsync(set, token);
                }

                set.Notify();

                if (set.Bar != null)
                {
                    set.Bar.Render(in _navigationBarStatus);
                }
            }
            catch
            {
                if (set.IsPopulated)
                {
                    Return(set);
                }
                else
                {
                    Discard(set);
                }

                throw;
            }

            return set;
        }

        public void Return(AttachmentSet set)
        {
            if (set == null || set.InUse == false)
            {
                return;
            }

            set.InUse = false;

            set.NavigationBarFlow.FlowElement.RemoveFromHierarchy();
            set.TabsFlow.FlowElement.RemoveFromHierarchy();
            set.CardsFlow.FlowElement.RemoveFromHierarchy();
        }

        public void RenderNavigationBars(in NavigationBarStatus status)
        {
            _navigationBarStatus = status;

            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                var set = _sets[i];

                if (set.InUse && set.Bar != null)
                {
                    set.Bar.Render(in status);
                }
            }
        }

        public void Dispose()
        {
            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                _sets[i].Dispose();
            }

            _sets.Clear();

            FloatingContext = null;
            Toast = null;
            FlowCreated = null;
            FlowDropped = null;
        }

        private AttachmentSet TakeSet()
        {
            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                if (_sets[i].InUse == false)
                {
                    return _sets[i];
                }
            }

            if (count < MAX_SETS)
            {
                var set = _factory.Create();

                _sets.Add(set);
                return set;
            }

            ThrowHelper.ThrowAttachmentSetsExhausted();
            return null;
        }

        private void Discard(AttachmentSet set)
        {
            _sets.Remove(set);
            _factory.Release(set);
        }

        private async UnityTask<bool> PopulateAsync(AttachmentSet set, CancellationToken token)
        {
            var barKey = AssetKeyAPI.Key(AttachmentPage.NavigationBar);
            var chronicleKey = AssetKeyAPI.Key(TabPage.Chronicle);
            var inscriptionKey = AssetKeyAPI.Key(TabPage.Inscription);
            var conceptKey = AssetKeyAPI.Key(CardPage.Concept);
            var inPracticeKey = AssetKeyAPI.Key(CardPage.InPractice);

            var bar = await set.NavigationBarFlow.ShowPageAsync(barKey, CreateContext(set), token);

            if (bar.IsApplied() == false)
            {
                return false;
            }

            if ((await set.TabsFlow.AddPageAsync(chronicleKey, CreateContext(set), token)).IsApplied() == false)
            {
                return false;
            }

            if ((await set.TabsFlow.AddPageAsync(inscriptionKey, CreateContext(set), token)).IsApplied() == false)
            {
                return false;
            }

            if ((await set.CardsFlow.AddPageAsync(conceptKey, CreateContext(set), token)).IsApplied() == false)
            {
                return false;
            }

            if ((await set.CardsFlow.AddPageAsync(inPracticeKey, CreateContext(set), token)).IsApplied() == false)
            {
                return false;
            }

            set.IsPopulated = true;
            return true;
        }

        private async UnityTask ApplyViewAsync(AttachmentSet set, CancellationToken token)
        {
            var tab = _state.ViewOf(set.Location.Dimension).Tab;
            var tabs = set.TabsFlow.Pages;

            if ((uint)tab < (uint)tabs.Count && IsShown(tabs[tab]) == false)
            {
                var context = CreateContext(new DimensionPageData(_state, set.Location));

                await set.TabsFlow.ShowPageAtIndexAsync(tab, context, token);
            }

            var conceptVisible = _state.ViewOf(set.Location.Dimension).ConceptVisible;
            await ApplyCardAsync(set: set, index: 0, visible: conceptVisible, token: token);

            var inPracticeVisible = _state.ViewOf(set.Location.Dimension).InPracticeVisible;
            await ApplyCardAsync(set: set, index: 1, visible: inPracticeVisible, token: token);
        }

        private async UnityTask ApplyCardAsync(AttachmentSet set, int index, bool visible, CancellationToken token)
        {
            var cards = set.CardsFlow.Pages;

            if ((uint)index >= (uint)cards.Count || IsShown(cards[index]) == visible)
            {
                return;
            }

            var context = CreateContext(new DimensionPageData(_state, set.Location));

            if (visible)
            {
                await set.CardsFlow.ShowPageAtIndexAsync(index, context, token);
            }
            else
            {
                await set.CardsFlow.HidePageAtIndexAsync(index, context, token);
            }
        }
    }
}
