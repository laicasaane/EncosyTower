#if UNITY_6000_5_OR_NEWER

using System;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    internal sealed class PanelRendererAdapter : IUitkPanelComponentAdapter
    {
        private readonly PanelRenderer _renderer;

        private Action<VisualElement, int> _onRoot;

        public PanelRendererAdapter(PanelRenderer renderer)
        {
            _renderer = renderer;
        }

        public string DisplayName => "Panel Renderer";

        public void Attach(Action<VisualElement, int> onRoot)
        {
            _onRoot = onRoot;
            _renderer.RegisterUIReloadCallback(OnReload);
        }

        public void Detach()
        {
            _renderer.UnregisterUIReloadCallback(OnReload);
            _onRoot = null;
        }

        public void NotifyRootLost() { }

        public void Poll() { }

        private void OnReload(VisualElement root, int version)
        {
            _onRoot?.Invoke(root, version);
        }
    }
}

#endif
