using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.UnityExtensions
{
    public static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("RUNTIME_CHECKS")]
        public static void ThrowIfComponentTypeInvalid([DoesNotReturnIf(false)] bool isValid, Type type)
        {
            if (isValid == false)
            {
                throw CreateComponentTypeException(type);
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("RUNTIME_CHECKS")]
        public static void ThrowIfSceneInvalid([DoesNotReturnIf(false)] bool isValid, Scene scene)
        {
            if (isValid == false)
            {
                throw CreateSceneException(scene);
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("RUNTIME_CHECKS")]
        public static void ThrowIfObjectInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateSelfException();
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("RUNTIME_CHECKS")]
        public static void ThrowIfEntityIdInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateObjectException();
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("RUNTIME_CHECKS")]
        public static void ThrowIfEntityIdNotCreated([DoesNotReturnIf(false)] bool value)
        {
            if (value == false)
            {
                throw CreateNotCreatedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateComponentTypeException(Type componentType)
            => new($"Type {componentType} is not a 'UnityEngine.Component'.", nameof(componentType));

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException CreateSceneException(Scene scene) => new($"Scene {scene.handle} is invalid");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateSelfException() => new("UnityEngine.Object is null or invalid.", "self");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateObjectException() => new("UnityEngine.Object is null or invalid.", "obj");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException CreateNotCreatedException()
            => new("UnityEntityId must be created using the constructor that takes a UnityEngine.Object.");
    }
}
