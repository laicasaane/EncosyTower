#if UNITY_MATHEMATICS

using System.Diagnostics;
using EncosyTower.Debugging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Pooling
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void AssertInitialization(bool isPoolInitialized, bool isContextInitialized)
        {
            const string MESSAGE = "SceneObjectPoolBehaviour must be initialized first!";

            Checks.IsTrue(isPoolInitialized, MESSAGE);
            Checks.IsTrue(isContextInitialized, MESSAGE);
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void AssertInitialization(
              bool isTransformArrayCreated
            , bool isGameObjectInfoMapCreated
            , bool isPositionsCreated
            , bool isScalesCreated
            , bool isRotationsCreated
        )
        {
            const string MESSAGE = "SceneObjectPoolBehaviour must be initialized first!";

            Checks.IsTrue(isTransformArrayCreated, MESSAGE);
            Checks.IsTrue(isGameObjectInfoMapCreated, MESSAGE);
            Checks.IsTrue(isPositionsCreated, MESSAGE);
            Checks.IsTrue(isScalesCreated, MESSAGE);
            Checks.IsTrue(isRotationsCreated, MESSAGE);
        }
    }
}

#endif
