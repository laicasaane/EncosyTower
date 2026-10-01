using System;
using UnityEngine.Scripting.APIUpdating;

namespace EncosyTower.PageFlows.MonoPages
{
    [Serializable]
    [MovedFrom(true, sourceNamespace: null, sourceAssembly: "EncosyTower.Core", sourceClassName: null)]
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
