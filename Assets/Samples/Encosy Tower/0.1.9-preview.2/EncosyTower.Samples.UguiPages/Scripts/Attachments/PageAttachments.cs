using System;
using System.Collections.Generic;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Overlays;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.Attachments
{
    /// <summary>Creates, lends and takes back the attachment sets of the screens.</summary>
    /// <remarks>
    /// The navigation bar, the tabs and the cards are pages in flows of their own, attached to their screen and shown
    /// with it. At most two sets exist at a time, one for the screen that leaves and one for the screen that arrives.
    /// The flows are created with <see cref="UguiPageFlow.Create{T}"/> and a copy of the codex flow context.
    /// </remarks>
    internal sealed class PageAttachments
    {
        public const string NAVIGATION_BAR_FLOW = "NavigationBar";
        public const string TABS_FLOW = "Tabs";
        public const string CARDS_FLOW = "Cards";

        private const int MAX_SETS = 2;

        private readonly List<AttachmentSet> _sets = new(MAX_SETS);
        private readonly AttachmentSetFactory _factory;
        private readonly AppState _state;

        private NavigationBarStatus _status;

        public PageAttachments(UguiPageCodex codex, AppState state)
        {
            _state = state;
            _factory = new AttachmentSetFactory(
                  codex
                , (scope, kind) => FlowCreated?.Invoke(scope, kind)
                , scope => FlowDropped?.Invoke(scope)
            );
        }

        public event Action<PageFlowScope, PageFlowKind> FlowCreated;

        public event Action<PageFlowScope> FlowDropped;

        public FloatingContextPage FloatingContext { get; set; }

        public ToastPage Toast { get; set; }

        private static PageContext CreateContext(object userData, int defaultIndex)
            => new() {
                AsyncOperation = PageAsyncOperation.Sequential,
                ShowOptions = PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.ZeroDuration,
                DefaultIndex = defaultIndex,
                UserData = Option.Some(userData),
            };

        public async UnityTask<AttachmentSet> RentAsync(
              BookLocation location
            , RectTransform navigationBarHost
            , RectTransform detailHost
            , RectTransform cardsHost
            , CancellationToken token
        )
        {
            var set = Take(location);

            try
            {
                if (set.IsCreated == false)
                {
                    _factory.CreateFlows(set);
                    set.IsCreated = true;
                }

                _factory.Place(set, navigationBarHost, detailHost, cardsHost);

                if (set.IsReady == false)
                {
                    var isAdded = await AddPagesAsync(set, token);

                    if (isAdded == false)
                    {
                        token.ThrowIfCancellationRequested();
                        ThrowHelper.ThrowInvalidOperation_AttachmentPagesFailed(location);
                    }

                    set.IsReady = true;
                }

                set.RaiseRebound();
                RenderNavigationBar(set);

                if (detailHost.IsValid() && cardsHost.IsValid())
                {
                    await ApplyViewAsync(set, token);

                    set.RaiseRebound();
                }

                return set;
            }
            catch
            {
                if (set.IsReady)
                {
                    Return(set);
                }
                else
                {
                    _sets.Remove(set);
                    _factory.Release(set);
                }

                throw;
            }
        }

        public void Return(AttachmentSet set)
        {
            set.InUse = false;

            _factory.Park(set);
        }

        public bool TryGetActive(BookLocation location, out AttachmentSet result)
        {
            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                var set = _sets[i];

                if (set.InUse && set.Location == location)
                {
                    result = set;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public void RenderNavigationBars(in NavigationBarStatus status)
        {
            _status = status;

            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                var set = _sets[i];

                if (set.InUse)
                {
                    RenderNavigationBar(set);
                }
            }
        }

        private AttachmentSet Take(BookLocation location)
        {
            DropDestroyedSets();

            AttachmentSet set = null;
            var count = _sets.Count;

            for (var i = 0; i < count; i++)
            {
                if (_sets[i].InUse == false)
                {
                    set = _sets[i];
                    break;
                }
            }

            if (set == null)
            {
                if (count >= MAX_SETS)
                {
                    ThrowHelper.ThrowInvalidOperation_AttachmentSetsExhausted();
                }

                set = new AttachmentSet(_state);
                _sets.Add(set);
            }

            set.InUse = true;
            set.Location = location;

            return set;
        }

        private void DropDestroyedSets()
        {
            for (var i = _sets.Count - 1; i >= 0; i--)
            {
                var set = _sets[i];

                if (set.IsDestroyed)
                {
                    _sets.RemoveAt(i);
                    _factory.RaiseFlowsDropped(set);
                }
            }
        }

        private async UnityTask<bool> AddPagesAsync(AttachmentSet set, CancellationToken token)
        {
            var barContext = CreateContext(userData: set, defaultIndex: 0);
            var barKey = AssetKeyAPI.Key(AttachmentPage.NavigationBar);
            var bar = await set.NavigationBarFlow.ShowPageAsync(barKey, barContext, token);

            if (bar.IsFailure)
            {
                return false;
            }

            var context = CreateContext(userData: set, defaultIndex: -1);
            var chronicleKey = AssetKeyAPI.Key(TabPage.Chronicle);
            var chronicle = await set.TabsFlow.AddPageAsync(chronicleKey, context, token);

            if (chronicle.IsFailure)
            {
                return false;
            }

            var inscriptionKey = AssetKeyAPI.Key(TabPage.Inscription);
            var inscription = await set.TabsFlow.AddPageAsync(inscriptionKey, context, token);

            if (inscription.IsFailure)
            {
                return false;
            }

            var conceptKey = AssetKeyAPI.Key(CardPage.Concept);
            var concept = await set.CardsFlow.AddPageAsync(conceptKey, context, token);

            if (concept.IsFailure)
            {
                return false;
            }

            var practiceKey = AssetKeyAPI.Key(CardPage.InPractice);
            var practice = await set.CardsFlow.AddPageAsync(practiceKey, context, token);

            return practice.IsSuccess;
        }

        private async UnityTask ApplyViewAsync(AttachmentSet set, CancellationToken token)
        {
            var context = CreateContext(userData: new DimensionPageData(_state, set.Location), defaultIndex: -1);
            var tabIndex = _state.ViewOf(set.Location.Dimension).Tab;

            if (set.TabIndex != tabIndex)
            {
                var tab = await set.TabsFlow.ShowPageAtIndexAsync(tabIndex, context, token);

                if (tab.IsApplied())
                {
                    set.NoteTab(tabIndex);
                }
            }

            var conceptVisible = _state.ViewOf(set.Location.Dimension).ConceptVisible;

            if (set.ConceptShown != conceptVisible)
            {
                await SetCardAsync(set: set, index: 0, visible: conceptVisible, context: context, token: token);
            }

            var practiceVisible = _state.ViewOf(set.Location.Dimension).InPracticeVisible;

            if (set.InPracticeShown != practiceVisible)
            {
                await SetCardAsync(set: set, index: 1, visible: practiceVisible, context: context, token: token);
            }
        }

        private async UnityTask SetCardAsync(
              AttachmentSet set
            , int index
            , bool visible
            , PageContext context
            , CancellationToken token
        )
        {
            var result = visible
                ? await set.CardsFlow.ShowPageAtIndexAsync(index, context, token)
                : await set.CardsFlow.HidePageAtIndexAsync(index, context, token);

            if (result.IsApplied())
            {
                set.NoteCard(index, visible);
            }
        }

        private void RenderNavigationBar(AttachmentSet set)
        {
            if (set.NavigationBar.IsValid())
            {
                set.NavigationBar.Render(in _status);
            }
        }

    }
}
