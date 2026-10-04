using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Types.Internals
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfRuntimeTypeCacheIsNull([DoesNotReturnIf(false)] bool isNotNull)
        {
            if (isNotNull == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("RuntimeTypeCache is not initialized correctly.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowGlobalObjectNotFound<TOwner, TObject>()
            => throw new InvalidOperationException(
                $"No global object of type '{Type<TObject>.FriendlyName}' is stored for '{Type<TOwner>.FriendlyName}'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowGlobalValueNotFound<TOwner, TValue>()
            => throw new InvalidOperationException(
                $"No global value of type '{Type<TValue>.FriendlyName}' is stored for '{Type<TOwner>.FriendlyName}'."
            );
    }
}
