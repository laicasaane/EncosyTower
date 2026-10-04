using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Types;
using UnityEngine;

namespace EncosyTower.TypeFlags.Internals
{
    static class ThrowHelper
    {
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
