using System;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>A text configuration entry that pairs its type with a string value.</summary>
    [Serializable]
    internal struct ConfigText
    {
        /// <summary>The type of the text.</summary>
        [Tooltip("The type of the text.")]
        [FormerlySerializedAs("_name")]
        [SerializeField] private ConfigTextType _type;

        /// <summary>The value of the text.</summary>
        [Tooltip("The value of the text.")]
        [SerializeField] private string _value;

        /// <summary>Gets the type of the text.</summary>
        internal readonly ConfigTextType Type => _type;

        /// <summary>Gets the value of the text.</summary>
        internal readonly string Value => _value;
    }
}
