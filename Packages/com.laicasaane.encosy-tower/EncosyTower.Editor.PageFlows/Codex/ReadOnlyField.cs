#if UNITY_EDITOR

using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    internal sealed class ReadOnlyField : BaseField<string>
    {
        private const string READONLY_USS_CLASS_NAME = PageFlowContextSection.ROOT_USS_CLASS_NAME + "__readonly";
        private const string VALUE_USS_CLASS_NAME = PageFlowContextSection.ROOT_USS_CLASS_NAME + "__readonly-value";

        private readonly VisualElement _input;
        private readonly Label _text;

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

        public void SetText(string text, VisualElement icon = null)
        {
            if (icon != null)
            {
                _input.Insert(0, icon);
            }

            _text.text = text;
        }
    }
}

#endif
