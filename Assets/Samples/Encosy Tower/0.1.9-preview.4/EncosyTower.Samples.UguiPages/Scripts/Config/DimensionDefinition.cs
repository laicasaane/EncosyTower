using System;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>The definition of one dimension.</summary>
    [Serializable]
    public struct DimensionDefinition
    {
        /// <summary>The dimension that this definition describes.</summary>
        [Tooltip("The dimension that this definition describes.")]
        [SerializeField] private Dimension _id;

        /// <summary>The title of the dimension.</summary>
        [Tooltip("The title of the dimension.")]
        public string title;

        /// <summary>The lore text of the Echo dialog.</summary>
        [Tooltip("The lore text of the Echo dialog.")]
        public string lore;

        /// <summary>The motif text of the dimension.</summary>
        [Tooltip("The motif text of the dimension.")]
        public string motif;

        /// <summary>The object that the inscription names.</summary>
        [Tooltip("The object that the inscription names.")]
        public string inscriptionObject;

        /// <summary>The body text of the concept card.</summary>
        [Tooltip("The body text of the concept card.")]
        public string concept;

        /// <summary>The body text of the in-practice card.</summary>
        [Tooltip("The body text of the in-practice card.")]
        public string practice;

        /// <summary>The text that the in-practice card names as the thing to watch for.</summary>
        [Tooltip("The text that the in-practice card names as the thing to watch for.")]
        public string watch;

        /// <summary>Whether the dimension has a knowledge entry. The sample does not read it.</summary>
        [Tooltip("Whether the dimension has a knowledge entry. The sample does not read it.")]
        public bool hasKnowledge;

        /// <summary>The surface color of the dimension screen.</summary>
        [Tooltip("The surface color of the dimension screen.")]
        public ThemedColor surface;

        /// <summary>The color of the navigation bar on the dimension screen.</summary>
        [Tooltip("The color of the navigation bar on the dimension screen.")]
        public ThemedColor navigation;

        /// <summary>Gets the dimension that this definition describes.</summary>
        internal readonly Dimension Id => _id;
    }
}
