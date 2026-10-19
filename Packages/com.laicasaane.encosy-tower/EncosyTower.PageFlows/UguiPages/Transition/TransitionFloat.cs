#if UNITY_UGUI

using System;

namespace EncosyTower.PageFlows.UguiPages.Transitions
{
    /// <summary>A pair of values that a transition moves from the start to the end.</summary>
    [Serializable]
    public struct TransitionFloat
    {
        /// <summary>The value the transition starts with.</summary>
        public float start;

        /// <summary>The value the transition ends with.</summary>
        public float end;

        /// <summary>Creates a pair.</summary>
        /// <param name="start">The value the transition starts with.</param>
        /// <param name="end">The value the transition ends with.</param>
        public TransitionFloat(float start, float end)
        {
            this.start = start;
            this.end = end;
        }
    }
}

#endif
