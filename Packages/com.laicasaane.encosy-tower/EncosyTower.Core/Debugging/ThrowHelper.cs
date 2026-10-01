// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Debugging
{
    /// <summary>
    /// Provides generic validation guards and exception factories for Encosy Tower.
    /// </summary>
    /// <remarks>
    /// Conditional guard symbols are declared by <see cref="ValidationDefines"/>. Area-specific
    /// guards are provided by their corresponding ThrowHelper classes.
    /// </remarks>
    public static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfNull(
              [NotNull] object argument
            , [CallerArgumentExpression("argument")] string paramName = null
        )
        {
            if (argument == null)
            {
                throw CreateArgumentNullException(paramName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfNullOrEmpty(
              [NotNull] string argument
            , [CallerArgumentExpression("argument")] string paramName = null
        )
        {
            if (argument.IsEmpty())
            {
                throw CreateArgumentNullException_String(paramName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfUnityObjectInvalid(
              [NotNull] UnityEngine.Object argument
            , [CallerArgumentExpression("argument")] string paramName = null
        )
        {
            if (argument.IsInvalid())
            {
                throw CreateArgumentNullException_UnityObject(paramName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfNullOrUnityObjectInvalid<T>(
              [NotNull] T argument
            , [CallerArgumentExpression("argument")] string paramName = null
        )
        {
            if (argument is null)
            {
                throw CreateArgumentNullException(paramName);
            }

            if (argument is UnityEngine.Object unityObject && unityObject.IsInvalid())
            {
                throw CreateArgumentNullException_UnityObject(paramName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfNotCreated<T>(
              T argument
            , [CallerArgumentExpression("argument")] string paramName = null
        )
            where T : IIsCreated
        {
            if (argument.IsCreated == false)
            {
                throw CreateArgumentException_NotCreatedCorrectly(paramName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        public static void ThrowIfNotInitialized<T>(
              T argument
            , [CallerArgumentExpression("argument")] string paramName = null
        )
            where T : IIsInitialized
        {
            if (argument.IsInitialized == false)
            {
                throw CreateArgumentException_NotInitialized(paramName);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_NotCreatedCorrectly(string paramName)
            => new("Value must be created correctly.", paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_NotInitialized(string paramName)
            => new("Value must be initialized.", paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException CreateInvalidOperationException_TypeNotCreatedCorrectly(string name)
            => new($"Type '{name}' was not created correctly.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentNullException CreateArgumentNullException(string paramName)
            => new(paramName);

        public static ArgumentNullException CreateArgumentNullException_String(string paramName)
            => new(paramName, "The string argument cannot be null or empty.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentNullException CreateArgumentNullException_UnityObject(string paramName)
            => new(paramName, "The Unity object cannot be null or invalid.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_LengthNegative()
            => new("length", "The value must be non-negative.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException_IndexNegative()
            => new("index", "The value must be non-negative.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_ArrayPlusOffTooSmall()
            => new("Destination array is not long enough to copy all the items in the collection. Check array index and length.");
    }
}
