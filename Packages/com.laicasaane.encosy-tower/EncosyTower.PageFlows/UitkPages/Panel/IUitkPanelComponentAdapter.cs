using System;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages.Internals
{
    internal interface IUitkPanelComponentAdapter
    {
        string DisplayName { get; }

        void Attach(Action<VisualElement, int> onRoot);

        void Detach();

        void NotifyRootLost();

        void Poll();
    }
}
