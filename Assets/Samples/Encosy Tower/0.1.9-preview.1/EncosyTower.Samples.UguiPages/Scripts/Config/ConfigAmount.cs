using System;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;
using UnityEngine.Serialization;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>An amount configuration entry that pairs its type with a floating-point value.</summary>
    [Serializable]
    internal struct ConfigAmount
    {
        /// <summary>The type of the amount.</summary>
        [Tooltip("The type of the amount.")]
        [FormerlySerializedAs("_name")]
        [SerializeField] private ConfigAmountType _type;

        /// <summary>The value of the amount.</summary>
        [Tooltip("The value of the amount.")]
        [SerializeField] private float _value;

        /// <summary>Gets the type of the amount.</summary>
        internal readonly ConfigAmountType Type => _type;

        /// <summary>Gets the value of the amount.</summary>
        internal readonly float Value => _value;
    }
}
