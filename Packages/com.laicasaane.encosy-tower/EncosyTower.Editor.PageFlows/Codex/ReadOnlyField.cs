#if UNITY_EDITOR

using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// A label with a text that the user cannot edit, aligned with the other inspector fields.
    /// </summary>
    /// <remarks>
    /// The inherited <c>value</c> is not connected to the text. Use <see cref="SetText"/> to change the text.
    /// </remarks>
    internal sealed class ReadOnlyField : BaseField<string>
    {
        private const string READONLY_USS_CLASS_NAME = PageFlowContextSection.ROOT_USS_CLASS_NAME + "__readonly";
        private const string VALUE_USS_CLASS_NAME = PageFlowContextSection.ROOT_USS_CLASS_NAME + "__readonly-value";

        private readonly VisualElement _input;
        private readonly Label _text;

        private VisualElement _icon;

        public ReadOnlyField(string label) : this(label, new VisualElement()) { }

        private ReadOnlyField(string label, VisualElement input) : base(label, input)
        {
            _input = input;
            _input.AddToClassList(READONLY_USS_CLASS_NAME);
            AddToClassList(alignedFieldUssClassName);

            _text = new Label();
            _text.AddToClassList(VALUE_USS_CLASS_NAME);
            _input.Add(_text);
        }

        /// <summary>
        /// Sets the text and the icon.
        /// </summary>
        /// <param name="text">The text to show.</param>
        /// <param name="icon">The element to show before the text, or <see langword="null"/> for none.</param>
        /// <remarks>
        /// Removes the icon of a previous call.
        /// </remarks>
        public void SetText(string text, VisualElement icon = null)
        {
            _icon?.RemoveFromHierarchy();
            _icon = icon;

            if (icon != null)
            {
                _input.Insert(0, icon);
            }

            _text.text = text;
        }
    }
}

#endif
