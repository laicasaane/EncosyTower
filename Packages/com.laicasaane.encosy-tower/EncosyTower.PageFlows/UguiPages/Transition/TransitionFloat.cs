#if UNITY_UGUI

using System;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.PageFlows.UguiPages
{
    [Serializable]
    [MovedFrom(
          true
        , sourceNamespace: "EncosyTower.PageFlows.MonoPages"
        , sourceAssembly: "EncosyTower.PageFlows.MonoPages"
        , sourceClassName: "TransitionFloat"
    )]
    public struct TransitionFloat
    {
        public float start;
        public float end;

        public TransitionFloat(float start, float end)
        {
            this.start = start;
            this.end = end;
        }
    }
}

#endif
