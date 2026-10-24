using System;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>A color configuration entry that pairs its type with light and dark values.</summary>
    [Serializable]
    internal struct ConfigColor
    {
        /// <summary>The type of the color.</summary>
        [Tooltip("The type of the color.")]
        [FormerlySerializedAs("_name")]
        [SerializeField] private ConfigColorType _type;

        /// <summary>The light and dark values of the color.</summary>
        [Tooltip("The light and dark values.")]
        [SerializeField] private ThemedColor _value;

        /// <summary>Gets the type of the color.</summary>
        internal readonly ConfigColorType Type => _type;

        /// <summary>Gets the light and dark values of the color.</summary>
        internal readonly ThemedColor Value => _value;
    }
}
