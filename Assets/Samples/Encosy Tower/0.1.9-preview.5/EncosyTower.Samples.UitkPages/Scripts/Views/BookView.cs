using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Samples.UitkPages.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>Helpers that color, outline and show the elements of the pages.</summary>
    internal static class BookView
    {
        private static readonly ConditionalWeakTable<Button, ButtonStates> s_states = new();
        private static readonly List<Button> s_buttons = new();

        /// <summary>
        /// Gives every button under a root its hover and press states, once, and makes its colors snap: a page
        /// calls this in its render path, so a render never shows a color transition.
        /// </summary>
        /// <param name="root">The root of the page.</param>
        /// <param name="palette">The page's palette, which holds the amounts of the states.</param>
        /// <remarks>The buttons of the theme switch are left out: their look comes from the switch.</remarks>
        public static void AttachStates(VisualElement root, PaletteStyle palette)
        {
            root.Query<Button>().Build().ToList(s_buttons);

            var count = s_buttons.Count;

            for (var i = 0; i < count; i++)
            {
                var button = s_buttons[i];

                if (button.ClassListContains(ThemeSwitch.SEGMENT_CLASS))
                {
                    continue;
                }

                GetOrAttach(button, palette).Snap();
            }

            s_buttons.Clear();
        }

        /// <summary>
        /// Gives every button under a root its hover and press states, once, and makes its colors snap; the
        /// amounts of the states come from each button's resolved USS custom properties.
        /// </summary>
        /// <param name="root">The root of the page, for a page that has no palette.</param>
        public static void AttachStates(VisualElement root)
        {
            AttachStates(root, palette: null);
        }

        /// <summary>Makes the colors of every button with states under a root snap, as at a theme change.</summary>
        /// <param name="root">The root whose buttons snap.</param>
        public static void SnapAll(VisualElement root)
        {
            root.Query<Button>().Build().ToList(s_buttons);

            var count = s_buttons.Count;

            for (var i = 0; i < count; i++)
            {
                if (s_states.TryGetValue(s_buttons[i], out var states))
                {
                    states.Snap();
                }
            }

            s_buttons.Clear();
        }

        /// <summary>Sets the normal colors of a button; the border mixes the background and the ink.</summary>
        /// <param name="button">The button.</param>
        /// <param name="background">The background color.</param>
        /// <param name="ink">The label color.</param>
        /// <param name="borderMix">How far the border goes from the background toward the ink, from 0 to 1.</param>
        public static void SetColors(Button button, Color background, Color ink, float borderMix)
        {
            SetColors(button, background, ink, Color.Lerp(background, ink, borderMix));
        }

        /// <summary>Sets and stores the complete normal look with an explicit border color.</summary>
        /// <param name="button">The button.</param>
        /// <param name="background">The background color.</param>
        /// <param name="ink">The label color.</param>
        /// <param name="border">The border color.</param>
        public static void SetColors(Button button, Color background, Color ink, Color border)
        {
            button.style.backgroundColor = background;
            button.style.color = ink;

            button.style.borderTopColor = button.style.borderRightColor = border;
            button.style.borderBottomColor = button.style.borderLeftColor = border;

            if (s_states.TryGetValue(button, out var states))
            {
                states.SetNormal(new ButtonLook(background, border, ink));
            }
        }

        /// <summary>
        /// Gives a button the looks of its hover and press states, in place of the USS rules. Call it after the
        /// last write of the button's colors in a render.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="hover">The look while the pointer is over the button.</param>
        /// <param name="press">The look while the button is pressed.</param>
        /// <param name="inset">The color of the inset shown while the button is pressed.</param>
        public static void SetStateLooks(Button button, in ButtonLook hover, in ButtonLook press, Color inset)
        {
            GetOrAttach(button, palette: null).SetLooks(in hover, in press, inset);
        }

        /// <summary>
        /// Takes the looks of the hover and press states away from a button, so the USS rules own them again.
        /// </summary>
        /// <param name="button">The button.</param>
        public static void ClearStateLooks(Button button)
        {
            if (s_states.TryGetValue(button, out var states))
            {
                states.ClearLooks();
            }
        }

        /// <summary>Sets the color of the four borders of an element.</summary>
        /// <param name="element">The element.</param>
        /// <param name="color">The border color.</param>
        /// <remarks>On a button with states, the color is also kept in its normal look.</remarks>
        public static void SetBorder(VisualElement element, Color color)
        {
            element.style.borderTopColor = element.style.borderRightColor = color;
            element.style.borderBottomColor = element.style.borderLeftColor = color;

            if (element is Button button && s_states.TryGetValue(button, out var states))
            {
                states.SetNormalBorder(color);
            }
        }

        /// <summary>
        /// Enables or disables a button. A disabled button drops its hover and press state at once, because
        /// UI Toolkit sends no event when a button is disabled.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="value"><c>true</c> to enable the button; <c>false</c> to disable it.</param>
        public static void SetEnabled(Button button, bool value)
        {
            button.SetEnabled(value);

            if (value == false && s_states.TryGetValue(button, out var states))
            {
                states.ResetState();
            }
        }

        /// <summary>Shows or hides an element.</summary>
        /// <param name="element">The element.</param>
        /// <param name="visible"><c>true</c> to show the element; <c>false</c> to hide it.</param>
        public static void SetDisplay(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static ButtonStates GetOrAttach(Button button, PaletteStyle palette)
        {
            if (s_states.TryGetValue(button, out var states))
            {
                return states;
            }

            states = new ButtonStates(palette);

            s_states.Add(button, states);
            button.AddManipulator(states);

            return states;
        }
    }
}
