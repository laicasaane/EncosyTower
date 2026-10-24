using System;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    internal sealed class DialogBackdrop
    {
        private VisualElement _backdrop;
        private Action _dismiss;

        public void Bind(VisualElement root, Action dismiss)
        {
            if (_backdrop == null)
            {
                _backdrop = root.Q("backdrop");
                _backdrop.pickingMode = PickingMode.Position;
                _backdrop.RegisterCallback<ClickEvent>(OnClick);
            }

            _dismiss = dismiss;
        }

        public void Unbind()
        {
            _dismiss = null;
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.button != 0 || ReferenceEquals(evt.target, _backdrop) == false)
            {
                return;
            }

            evt.StopPropagation();
            _dismiss?.Invoke();
        }
    }
}
