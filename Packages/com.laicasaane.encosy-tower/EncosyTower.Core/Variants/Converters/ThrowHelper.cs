using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Variants.Converters
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfSizeInvalid<T>([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateSizeException<T>();
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfStringInvalidCast([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateStringInvalidCastException();
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfObjectInvalidCast([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateObjectInvalidCastException();
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfUndefinedInvalidCast<T>([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateUndefinedException<T>();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NotSupportedException CreateSizeException<T>()
            => new(
                $"The size of {typeof(T)} is {UnsafeUtility.SizeOf(typeof(T))} bytes, " +
                $"while a Variant can only store {VariantData.BYTE_COUNT} bytes of custom data. " +
                $"To enable the automatic conversion between {typeof(T)} and {typeof(Variant)}, " +
                $"please {GetDefineSymbolMessage(UnsafeUtility.SizeOf(typeof(T)))}"
            );

        private static string GetDefineSymbolMessage(int size)
        {
            var longCount = (int)Math.Ceiling((double)size / VariantData.SIZE_OF_LONG);
            var nextSize = longCount * VariantData.SIZE_OF_LONG;

            if (size > VariantData.MAX_BYTE_COUNT)
            {
                return $"contact the author to increase the maximum size of Variant type to {nextSize} bytes " +
                    $"(currently it is capped at {VariantData.MAX_BYTE_COUNT} bytes).";
            }
            else
            {
                return $"define VARIANT_{longCount}_LONGS, or use the menu " +
                    "'Encosy Tower/Project Settings/Variant Type'.";
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidCastException CreateStringInvalidCastException()
            => new("Cannot get value of string from the input variant.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidCastException CreateObjectInvalidCastException()
            => new("Cannot get value of object from the input variant.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidCastException CreateUndefinedException<T>()
            => new($"Cannot get value of {typeof(T)} from the input variant.");
    }
}
