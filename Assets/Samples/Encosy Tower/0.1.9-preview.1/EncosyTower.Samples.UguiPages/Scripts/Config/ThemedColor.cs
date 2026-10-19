using System;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>A color with a light and a dark value.</summary>
    [Serializable]
    public struct ThemedColor
    {
        /// <summary>The color in the light theme.</summary>
        [Tooltip("The color in the light theme.")]
        public Color light;

        /// <summary>The color in the dark theme.</summary>
        [Tooltip("The color in the dark theme.")]
        public Color dark;
    }
}
