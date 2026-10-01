using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.StringIds
{
    static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfFailedRegistering(
              [DoesNotReturnIf(false)] bool check
            , in UnmanagedString str
            , StringId id
        )
        {
            if (check == false)
            {
                throw CreateException(str, id);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(in UnmanagedString str, StringId id)
                => new($"Cannot register a StringId by the same value \"{str}\" with different id \"{id}\".");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfFailedRegistering(
              [DoesNotReturnIf(false)] bool check
            , string str
            , StringId id
        )
        {
            if (check == false)
            {
                throw CreateException(str, id);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(string str, StringId id)
                => new($"Cannot register a StringId by the same value \"{str}\" with different id \"{id}\".");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfAmountIsNotValid([DoesNotReturnIf(false)] bool isValid, int amount)
        {
            if (isValid == false)
            {
                throw CreateAmountException(amount);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateAmountException(int amount)
                => new(nameof(amount), amount, "amount must be greater than 0");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfNotDefined([DoesNotReturnIf(false)] bool check, StringId key)
        {
            if (check == false)
            {
                throw CreateNotDefinedException(key);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateNotDefinedException(StringId key)
                => new(
                    $"No StringId has not been globally defined with id \"{key}\". " +
                    "To define one, use StringToId.Get() API."
                );
        }
    }
}
