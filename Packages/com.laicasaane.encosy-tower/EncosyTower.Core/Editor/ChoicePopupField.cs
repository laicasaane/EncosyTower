#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor
{
    /// <summary>
    /// A popup-styled field over a list of labels. Its value is the index of the selected label.
    /// </summary>
    /// <remarks>
    /// <para>
    /// With <see cref="HasDefaultChoice"/>, item 0 is the default item and a separator follows it in the menu.
    /// An index outside the list shows <see cref="UnlistedValueText"/>, and a mixed state shows the mixed-value
    /// text.
    /// </para>
    /// <para>
    /// A left click or a navigation submit opens a <see cref="GenericMenu"/> while the field is enabled.
    /// </para>
    /// </remarks>
    public sealed class ChoicePopupField : BaseField<int>
    {
        private readonly VisualElement _input;
        private readonly TextElement _text;
        private string[] _choices = Array.Empty<string>();
        private string _unlistedValueText = string.Empty;

        /// <summary>
        /// Creates the field with a label.
        /// </summary>
        /// <param name="label">The label text.</param>
        public ChoicePopupField(string label) : this(label, new VisualElement()) { }

        private ChoicePopupField(string label, VisualElement visualInput) : base(label, visualInput)
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

        /// <summary>
        /// The labels of the items.
        /// </summary>
        /// <value>
        /// The labels in item order. Setting <see langword="null"/> stores an empty array. The setter updates the
        /// shown text.
        /// </value>
        public string[] Choices
        {
            get => _choices;
            set
            {
                _choices = value ?? Array.Empty<string>();
                UpdateText();
            }
        }

        /// <summary>
        /// The tooltip of the field and of its input element.
        /// </summary>
        public string InputTooltip
        {
            get => _input.tooltip;
            set
            {
                tooltip = value;
                _input.tooltip = value;
            }
        }

        /// <summary>
        /// Whether item 0 is the default item, which a separator follows. The default is <see langword="true"/>.
        /// </summary>
        public bool HasDefaultChoice { get; set; } = true;

        /// <summary>
        /// The text shown when the value is not an index of <see cref="Choices"/>.
        /// </summary>
        /// <value>
        /// The text. Setting <see langword="null"/> stores an empty string. The setter updates the shown text.
        /// </value>
        public string UnlistedValueText
        {
            get => _unlistedValueText;
            set
            {
                _unlistedValueText = value ?? string.Empty;
                UpdateText();
            }
        }

        /// <inheritdoc/>
        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateText();
        }

        /// <inheritdoc/>
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
