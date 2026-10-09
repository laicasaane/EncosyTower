#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal sealed class OverridableChoiceField : BaseField<int>
    {
        private readonly VisualElement _input;
        private readonly TextElement _text;
        private string[] _choices = Array.Empty<string>();
        private string _unlistedValueText = string.Empty;

        public OverridableChoiceField(string label) : this(label, new VisualElement()) { }

        private OverridableChoiceField(string label, VisualElement visualInput) : base(label, visualInput)
        {
            _input = visualInput;

            AddToClassList(BasePopupField<string, string>.ussClassName);
            AddToClassList(PopupField<string>.ussClassName);
            labelElement.AddToClassList(BasePopupField<string, string>.labelUssClassName);
            labelElement.AddToClassList(PopupField<string>.labelUssClassName);
            visualInput.AddToClassList(BasePopupField<string, string>.inputUssClassName);
            visualInput.AddToClassList(PopupField<string>.inputUssClassName);

            _text = new TextElement { pickingMode = PickingMode.Ignore };
            _text.AddToClassList(BasePopupField<string, string>.textUssClassName);
            visualInput.Add(_text);

            var arrow = new VisualElement { pickingMode = PickingMode.Ignore };
            arrow.AddToClassList(BasePopupField<string, string>.arrowUssClassName);
            visualInput.Add(arrow);

            visualInput.RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
        }

        public string[] Choices
        {
            get => _choices;
            set
            {
                _choices = value ?? Array.Empty<string>();
                UpdateText();
            }
        }

        public string InputTooltip
        {
            get => _input.tooltip;
            set
            {
                tooltip = value;
                _input.tooltip = value;
            }
        }

        public bool HasDefaultChoice { get; set; } = true;

        public string UnlistedValueText
        {
            get => _unlistedValueText;
            set
            {
                _unlistedValueText = value ?? string.Empty;
                UpdateText();
            }
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateText();
        }

        protected override void UpdateMixedValueContent()
        {
            _text.text = showMixedValue ? mixedValueString : GetChoiceText(value);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0 || enabledInHierarchy == false)
            {
                return;
            }

            ShowMenu();
            evt.StopPropagation();
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            if (enabledInHierarchy == false)
            {
                return;
            }

            ShowMenu();
            evt.StopPropagation();
        }

        private void ShowMenu()
        {
            var menu = new GenericMenu();
            var choices = _choices;
            var current = showMixedValue ? -1 : value;
            var count = choices.Length;

            for (var i = 0; i < count; i++)
            {
                if (i == 1 && HasDefaultChoice)
                {
                    menu.AddSeparator(string.Empty);
                }

                menu.AddItem(new GUIContent(choices[i]), i == current, OnChoiceSelected, i);
            }

            menu.DropDown(_input.worldBound);
        }

        private void OnChoiceSelected(object userData)
        {
            value = (int)userData;
        }

        private void UpdateText()
        {
            if (showMixedValue == false)
            {
                _text.text = GetChoiceText(value);
            }
        }

        private string GetChoiceText(int index)
            => (uint)index < (uint)_choices.Length ? _choices[index] : _unlistedValueText;
    }
}

#endif
