using System.Diagnostics.CodeAnalysis;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    public static class EncosyGUIStyleExtensions
    {
        public static GUIStyle WithNormalBackground([NotNull] this GUIStyle style, Color color)
        {
            DebuggingThrowHelper.ThrowIfNull(style);
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, color);
            background.Apply();

            style.normal.background = background;

            return style;
        }

    }
}
