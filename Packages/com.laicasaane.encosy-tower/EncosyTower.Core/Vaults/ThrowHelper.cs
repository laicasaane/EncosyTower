using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Vaults
{
    static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception CreateArgumentNullExceptionInstance()
            => new ArgumentNullException("instance");

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorInstanceAlreadyExists<T>()
            => StaticDevLogger.LogError($"An instance of {typeof(T)} has already been existing");

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowCannotCastEvenRegistered<T>(object obj)
            => throw new InvalidCastException(
                $"Cannot cast an instance of type {obj.GetType()} to {typeof(T)}" +
                $"even though it is registered for {typeof(T)}"
            );

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void ThrowCannotCast<T>(object obj)
            => throw new InvalidCastException($"Cannot cast an instance of type {obj.GetType()} to {typeof(T)}");

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorTypeMismatch<T, TId>(TId id, object obj, UnityEngine.Object context)
        {
            var message = "Id \"{0}\" is mapped to an object of type \"{1}\". " +
                "However an object of type \"{2}\" is being requested from it. " +
                "It might be a bug at the time of registering.";

            if (context)
            {
                StaticDevLogger.LogErrorFormat(context, message, id, obj?.GetType(), typeof(T));
            }
            else
            {
                StaticDevLogger.LogErrorFormat(message, id, obj?.GetType(), typeof(T));
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorRegisteredObjectIsNull<TId>(TId id, UnityEngine.Object context)
        {
            var message = "The object registered with id \"{0}\" is null.";

            if (context)
            {
                StaticDevLogger.LogErrorFormat(context, message, id);
            }
            else
            {
                StaticDevLogger.LogErrorFormat(message, id);
            }
        }
    }
}
