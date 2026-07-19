using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    [UxmlElement(libraryPath = "Encosy Tower")]
    public partial class VisualSeparator : VisualElement
    {
        public static readonly string UssClassName = "visual-separator";

        public VisualSeparator() : this(string.Empty)
        {
        }

        public VisualSeparator(string additionalUssClassName) : base()
        {
            AddToClassList(UssClassName);

            if (string.IsNullOrWhiteSpace(additionalUssClassName) == false)
            {
                AddToClassList(additionalUssClassName);
            }
        }
    }
}
