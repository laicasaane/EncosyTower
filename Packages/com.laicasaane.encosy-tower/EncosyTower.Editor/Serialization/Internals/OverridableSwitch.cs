#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal sealed class OverridableSwitch : VisualElement, INotifyValueChanged<bool>
    {
        private const string ON_CLASS = "encosy-overridable__switch--on";
        private const string MIXED_CLASS = "encosy-overridable__switch--mixed";

        private bool _value;
        private bool _showMixedValue;

        public OverridableSwitch()
        {
            focusable = true;
            tooltip = "Override";
            AddToClassList("encosy-overridable__switch");

            var track = new VisualElement { name = "track", pickingMode = PickingMode.Ignore };
            track.AddToClassList("encosy-overridable__switch-track");
            Add(track);

            var outline = new VisualElement { name = "outline", pickingMode = PickingMode.Ignore };
            outline.AddToClassList("encosy-overridable__switch-outline");
            track.Add(outline);

            var knob = new VisualElement { name = "knob", pickingMode = PickingMode.Ignore };
            knob.AddToClassList("encosy-overridable__switch-knob");
            track.Add(knob);

            var focusRing = new VisualElement { name = "focus-ring", pickingMode = PickingMode.Ignore };
            focusRing.AddToClassList("encosy-overridable__switch-focus-ring");
            track.Add(focusRing);

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        public bool value
        {
            get => _value;
            set
            {
                if (_value == value && _showMixedValue == false)
                {
                    return;
                }

                using var evt = ChangeEvent<bool>.GetPooled(_value, value);
                ShowMixedValue = false;
                SetValueWithoutNotify(value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        public bool ShowMixedValue
        {
            get => _showMixedValue;
            set
            {
                _showMixedValue = value;
                EnableInClassList(MIXED_CLASS, value);
            }
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            _value = newValue;
            EnableInClassList(ON_CLASS, newValue);
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.button != 0 || enabledInHierarchy == false)
            {
                return;
            }

            Focus();
            value = _showMixedValue || _value == false;
            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (enabledInHierarchy == false)
            {
                return;
            }

            switch (evt.keyCode)
            {
                case KeyCode.Space:
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                {
                    value = _showMixedValue || _value == false;
                    evt.StopPropagation();
                    break;
                }
            }
        }
    }
}

#endif
