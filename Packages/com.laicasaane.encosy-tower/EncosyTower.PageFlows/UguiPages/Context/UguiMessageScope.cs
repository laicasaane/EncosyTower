#if UNITY_UGUI

using UnityEngine;

namespace EncosyTower.PageFlows
{
    /// <summary>Selects what a flow scope identifies.</summary>
    public enum UguiMessageScope
    {
        /// <summary>The scope identifies the GameObject of the owner.</summary>
        [InspectorName("GameObject")] GameObject,

        /// <summary>The scope identifies the owner component.</summary>
        [InspectorName("Component")] Component,
    }
}

#endif
