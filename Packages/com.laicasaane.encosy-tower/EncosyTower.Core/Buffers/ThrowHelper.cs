using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Buffers
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfInvalidAllocatorStrategy([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    "Allocator strategy must be either Unity.Collections.Allocator " +
                    "or Unity.Collections.AllocatorManager.AllocatorHandle"
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfBufferAlreadyAllocated([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot allocate an already allocated buffer.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfResizeUninitializedBuffer([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot resize an uninitialized buffer.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfBufferShiftIndexIsOutOfRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Out of bounds index");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfBufferShiftCountIsOutOfRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Out of bounds count");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfBufferShiftCountIsBelowIndex([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Count is lesser than index");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfPointerMisaligned([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Pinned storage address does not satisfy native element alignment.");
        }
    }
}
