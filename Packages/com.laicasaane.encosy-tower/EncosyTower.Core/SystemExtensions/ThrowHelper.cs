using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EncosyTower.SystemExtensions
{
    static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowBadGuidFormatSpecification()
            => throw new FormatException(
                "Format string can be only \"D\", \"d\", \"N\", \"n\", \"P\", \"p\", \"B\", \"b\", \"X\" or \"x\"."
            );

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfNegative([DoesNotReturnIf(true)] bool check, long value, string paramName)
        {
            if (check)
            {
                throw CreateException(paramName, value);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName, long value)
                => new(paramName, value, $"{paramName} ('{value}') must be a non-negative value.");
        }
    }
}
