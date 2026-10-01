using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.UnityExtensions
{
    public static partial class EncosyGameObjectExtensions
    {
        private const string CLONE_SUFFIX = "(Clone)";

        public static Component GetOrAddComponent([NotNull] this GameObject self, [NotNull] Type componentType)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);
            DebuggingThrowHelper.ThrowIfNull(componentType);
            ThrowHelper.ThrowIfComponentTypeInvalid(
                  IsComponentType(componentType)
                , componentType
            );

            if (self.TryGetComponent(componentType, out var component) == false)
            {
                component = self.AddComponent(componentType);
            }

            return component;
        }

        public static T GetOrAddComponent<T>([NotNull] this GameObject self) where T : Component
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);

            if (self.TryGetComponent(out T component) == false)
            {
                component = self.AddComponent<T>();
            }

            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToScene([NotNull] this GameObject self, Scene scene)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);
            ThrowHelper.ThrowIfSceneInvalid(scene.IsValid(), scene);
            SceneManager.MoveGameObjectToScene(self, scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveToSceneWithoutParent([NotNull] this GameObject self, Scene scene)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);
            ThrowHelper.ThrowIfSceneInvalid(scene.IsValid(), scene);
            self.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(self, scene);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DetachParent([NotNull] this GameObject self, bool worldPositionStays = true)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);
            self.transform.SetParent(null, worldPositionStays);
        }

        /// <summary>
        /// Trim the "(Clone)" suffix usually added to the name of a GameObject when Instantiate is called.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static GameObject TrimCloneSuffix([NotNull] this GameObject self)
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(self);

            var name = self.name.AsSpan();

            if (name.Length >= CLONE_SUFFIX.Length)
            {
                self.name = name[..^CLONE_SUFFIX.Length].ToString();
            }

            return self;
        }

        private static bool IsComponentType(Type type)
            => typeof(Component).IsAssignableFrom(type);

    }
}
