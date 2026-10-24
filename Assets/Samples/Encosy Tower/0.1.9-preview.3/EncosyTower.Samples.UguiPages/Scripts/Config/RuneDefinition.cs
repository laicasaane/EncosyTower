using System;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>The definition of one rune.</summary>
    [Serializable]
    public struct RuneDefinition
    {
        /// <summary>The rune that this definition describes.</summary>
        [Tooltip("The rune that this definition describes.")]
        [SerializeField] private Rune _id;

        /// <summary>The Latin letter that the rune decodes to.</summary>
        [Tooltip("The Latin letter that the rune decodes to.")]
        public string letter;

        /// <summary>Gets the rune that this definition describes.</summary>
        internal readonly Rune Id => _id;
    }
}
