using System;
using System.Collections.Generic;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.UnityExtensions;

namespace EncosyTower.Samples.UguiPages.Attachments
{
    /// <summary>The three flows attached to one screen: the navigation bar, the tabs and the cards.</summary>
    internal sealed class AttachmentSet
    {
        public AttachmentSet(AppState state)
        {
            State = state;
        }

        public event Action Rebound;

        public AppState State { get; }

        public List<PageFlowScope> FlowScopes { get; } = new(3);

        public BookLocation Location { get; set; }

        public bool InUse { get; set; }

        public bool IsReady { get; set; }

        public bool IsCreated { get; set; }

        public UguiPageStackSingle NavigationBarFlow { get; set; }

        public UguiPageListSingle TabsFlow { get; set; }

        public UguiPageListMulti CardsFlow { get; set; }

        public NavigationBarPage NavigationBar { get; set; }

        public PageFlowScope TabsScope => TabsFlow.Context.FlowScope;

        public PageFlowScope CardsScope => CardsFlow.Context.FlowScope;

        public int TabIndex { get; private set; } = -1;

        public bool ConceptShown { get; private set; }

        public bool InPracticeShown { get; private set; }

        public bool IsDestroyed
            => IsCreated && (NavigationBarFlow.IsInvalid() || TabsFlow.IsInvalid() || CardsFlow.IsInvalid());

        public void NoteTab(int index)
        {
            TabIndex = index;
        }

        public void NoteCard(int index, bool visible)
        {
            if (index == 0)
            {
                ConceptShown = visible;
            }
            else
            {
                InPracticeShown = visible;
            }
        }

        public void RaiseRebound()
        {
            Rebound?.Invoke();
        }
    }
}
