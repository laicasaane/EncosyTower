#if UNITY_ENTITIES && LATIOS_FRAMEWORK

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Entities
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfInvalidCollider([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static System.ArgumentException CreateException()
                => new("Collider is null or invalid.", "collider");
        }
    }
}

#endif
