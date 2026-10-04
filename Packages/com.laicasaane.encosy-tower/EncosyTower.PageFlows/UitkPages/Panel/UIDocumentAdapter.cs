using System;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    internal sealed class UIDocumentAdapter : IUitkPanelComponentAdapter
    {
        private readonly UIDocument _document;

        private Action<VisualElement, int> _onRoot;
        private VisualElement _root;
        private int _version;
        private bool _rootLost;

        public UIDocumentAdapter(UIDocument document)
        {
            _document = document;
        }

        public string DisplayName => "UI Document";

        public UIDocument Document => _document;

        public int Version => _version;

        public void Attach(Action<VisualElement, int> onRoot)
        {
            _onRoot = onRoot;
            _rootLost = false;
            Report(_document.rootVisualElement);
        }

        public void Detach()
        {
            _onRoot = null;
            _rootLost = false;
        }

        public void NotifyRootLost()
        {
            _rootLost = true;
        }

        public void Poll()
        {
            if (_onRoot == null || (_rootLost == false && _root != null))
            {
                return;
            }

            var root = _document.rootVisualElement;

            if (root == null)
            {
                return;
            }

            _rootLost = false;
            Report(root);
        }

        private void Report(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            if (ReferenceEquals(root, _root) == false)
            {
                _root = root;
                _version++;
            }

            _onRoot?.Invoke(_root, _version);
        }
    }
}
