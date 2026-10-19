using System;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Persistence
{
    /// <summary>The saved view of one dimension screen.</summary>
    [Serializable]
    internal struct ViewStateData
    {
        /// <summary>The dimension that the view belongs to.</summary>
        [field: SerializeField]
        public Dimension Dimension { get; set; }

        /// <summary>The index of the tab that is shown.</summary>
        [field: SerializeField]
        public int Tab { get; set; }

        /// <summary>Whether the concept card is shown.</summary>
        [field: SerializeField]
        public bool ConceptVisible { get; set; }

        /// <summary>Whether the in-practice card is shown.</summary>
        [field: SerializeField]
        public bool InPracticeVisible { get; set; }
    }
}
