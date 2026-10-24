using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>The color theme of the Book.</summary>
    internal enum Theme
    {
        Light,
        Dark,
    }

    /// <summary>Finds the theme of an element from the dark theme class on it or on a parent.</summary>
    internal static class ThemeAPI
    {
        public const string DARK_CLASS = "tower-book-app--dark";

        public static Theme Of(VisualElement element)
            => IsDark(element) ? Theme.Dark : Theme.Light;

        public static bool IsDark(VisualElement element)
        {
            while (element != null)
            {
                if (element.ClassListContains(DARK_CLASS))
                {
                    return true;
                }

                element = element.parent;
            }

            return false;
        }
    }
}
