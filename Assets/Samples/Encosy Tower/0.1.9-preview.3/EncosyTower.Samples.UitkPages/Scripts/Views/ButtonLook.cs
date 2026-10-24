using UnityEngine;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>The fill, border and label colors of a button in one state.</summary>
    internal readonly struct ButtonLook
    {
        /// <summary>The color of the button's background.</summary>
        public readonly Color Fill;

        /// <summary>The color of the button's four borders.</summary>
        public readonly Color Border;

        /// <summary>The color of the button's label.</summary>
        public readonly Color Ink;

        /// <summary>Creates the look.</summary>
        /// <param name="fill">The color of the button's background.</param>
        /// <param name="border">The color of the button's four borders.</param>
        /// <param name="ink">The color of the button's label.</param>
        public ButtonLook(Color fill, Color border, Color ink)
        {
            Fill = fill;
            Border = border;
            Ink = ink;
        }
    }
}
