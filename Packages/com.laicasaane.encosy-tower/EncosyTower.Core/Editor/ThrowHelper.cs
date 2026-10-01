#if UNITY_EDITOR

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Core;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    public static class ThrowHelper
    {
        [ApiForEditor]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        public static void ThrowIfAssetNotFound([DoesNotReturnIf(false)] bool isFound, string path)
        {
            if (isFound == false)
            {
                throw CreateException(path);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(string path)
                => new($"Editor asset was not found at '{path}'.");
        }
    }
}

#endif
