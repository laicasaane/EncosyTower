#if UNITY_EDITOR

using System.Diagnostics.CodeAnalysis;
using System.IO;
using EncosyTower.Core;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor
{
    [ApiForEditor]
    public static class EditorAPI
    {
        [ApiForEditor]
        public static string ProjectPath => Path.Combine(Application.dataPath, "..");

        [ApiForEditor]
        public static bool IsDark => EditorGUIUtility.isProSkin;

        [ApiForEditor]
        public static Color GetColor(in Color dark, in Color light)
        {
            return IsDark ? dark : light;
        }

        /// <summary>
        /// Returns a <see cref="Color"/> based on the current editor skin, using the provided hex color strings
        /// for dark and light modes.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="hexDark"/> or <paramref name="hexLight"/> is null or empty.
        /// </exception>
        /// <returns>A <see cref="Color"/> based on the current editor skin.</returns>
        [ApiForEditor]
        public static Color GetColor(string hexDark, string hexLight)
        {
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(hexDark, nameof(hexDark));
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(hexLight, nameof(hexLight));

            if (IsDark)
            {
                ColorUtility.TryParseHtmlString(hexDark, out var color);
                return color;
            }
            else
            {
                ColorUtility.TryParseHtmlString(hexLight, out var color);
                return color;
            }
        }

        /// <summary>
        /// Returns a <see cref="GUIContent"/> with the appropriate icon for the current editor skin.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="dark"/> or <paramref name="light"/> is null or empty.
        /// </exception>
        /// <returns>A <see cref="GUIContent"/> with the appropriate icon for the current editor skin.</returns>
        [ApiForEditor]
        public static GUIContent GetIcon(string dark, string light)
        {
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(dark, nameof(dark));
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(light, nameof(light));
            return IsDark ? EditorGUIUtility.IconContent(dark) : EditorGUIUtility.IconContent(light);
        }

        /// <summary>
        /// Returns a <see cref="StyleSheet"/> based on the current editor skin,
        /// or loading it from the specified paths for dark and light modes if it hasn't been loaded yet.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="darkPath"/> or <paramref name="lightPath"/> is null or empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the asset is not found at the specified path.
        /// </exception>
        /// <returns>
        /// A <see cref="StyleSheet"/> loaded from the appropriate path based on the current editor skin.
        /// </returns>
        [ApiForEditor]
        public static StyleSheet GetOrLoadStyleSheet(
              ref StyleSheet darkField, string darkPath
            , ref StyleSheet lightField, string lightPath
        )
        {
            ref var field = ref IsDark ? ref darkField : ref lightField;
            var path = IsDark ? darkPath : lightPath;

            if (field.IsInvalid())
            {
                field = LoadAsset<StyleSheet>(path);
            }

            return field;
        }

        /// <summary>
        /// Returns an asset of type <typeparamref name="T"/> based on the current editor skin,
        /// or loading it from the specified path
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the asset is not found at the specified path.
        /// </exception>
        /// <returns>The loaded asset of type <typeparamref name="T"/>.</returns>
        [ApiForEditor]
        public static T GetOrLoadAsset<T>(ref T field, string path)
            where T : Object
        {
            if (field.IsInvalid())
            {
                field = LoadAsset<T>(path);
            }

            return field;
        }

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from the specified paths for dark and light modes,
        /// based on the current editor skin.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="darkPath"/> or <paramref name="lightPath"/> is null or empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the asset is not found at the specified path.
        /// </exception>
        /// <returns>
        /// A <see cref="StyleSheet"/> loaded from the appropriate path based on the current editor skin.
        /// </returns>
        [ApiForEditor]
        public static StyleSheet LoadStyleSheet(string darkPath, string lightPath)
        {
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(darkPath, nameof(darkPath));
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(lightPath, nameof(lightPath));
            var path = IsDark ? darkPath : lightPath;
            return LoadAsset<StyleSheet>(path);
        }

        /// <summary>
        /// Loads an asset of type <typeparamref name="T"/> from the specified <paramref name="path"/>.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="path"/> is null or empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the asset is not found at the specified path.
        /// </exception>
        /// <returns>The loaded asset of type <typeparamref name="T"/>.</returns>
        [ApiForEditor]
        [return: NotNull]
        public static T LoadAsset<T>([NotNull] string path)
            where T : Object
        {
            Debugging.ThrowHelper.ThrowIfNullOrEmpty(path, nameof(path));

            var asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset.IsInvalid())
            {
                ThrowHelper.ThrowIfAssetNotFound(false, path);
            }

            return asset;
        }
    }
}

#endif
