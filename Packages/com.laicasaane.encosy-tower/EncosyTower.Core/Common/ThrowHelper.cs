using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_COLLECTIONS
using Unity.Collections;
#endif

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Common
{
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfErrorHasNoValue<T>([DoesNotReturnIf(false)] bool hasValue)
        {
            if (hasValue == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"The instance of Error<{typeof(T)}> has no value");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfOptionHasNoValue<T>([DoesNotReturnIf(false)] bool hasValue)
        {
            if (hasValue == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"The instance of Option<{typeof(T)}> has no value");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfResultWithDefaultErrorHasSameType<TValue>(
            [DoesNotReturnIf(false)] bool isDifferentType
        )
        {
            if (isDifferentType == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"{typeof(Result<TValue>)} is not allowed. " +
                $"Value type must be different from {typeof(Error<StringOrException>)}.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfResultValueAndErrorHaveSameType<TValue, TError>(
            [DoesNotReturnIf(false)] bool isDifferentType
        )
        {
            if (isDifferentType == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"{typeof(Result<TValue, TError>)} is not allowed. " +
                $"Value type must be different from  error type.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfSuccessHasNoValue<TFailure>([DoesNotReturnIf(false)] bool hasValue)
        {
            if (hasValue == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"The instance of Option<{typeof(TFailure)}> has no value");
        }

#if UNITY_COLLECTIONS
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowCannotParse(in FixedString128Bytes guidString)
        {
            throw new ArgumentException($"Cannot parse '{guidString}' into a Guid.", nameof(guidString));
        }
#endif

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfPrimeIsNegative([DoesNotReturnIf(false)] bool isZeroOrPositive)
        {
            if (isZeroOrPositive == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("Cannot get the next prime from a negative number.");
        }
    }
}
