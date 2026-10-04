#if UNITY_UGUI

using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    public enum UguiMessageScope
    {
        [InspectorName("GameObject")] GameObject,
        [InspectorName("Component")] Component,
    }
}

#endif
