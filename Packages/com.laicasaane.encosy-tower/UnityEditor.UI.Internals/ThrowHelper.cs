using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEditor.UI.Internals
{
    internal static class ThrowHelper
    {
        [HideInCallstack]
        internal static void ThrowIfNull([NotNull] object argument, string paramName)
        {
            if (argument == null)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentNullException CreateException(string paramName)
                => new(paramName);
        }
    }
}
