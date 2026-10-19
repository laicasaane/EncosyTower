#if UNITY_EDITOR

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Serialization;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
{
    /// <summary>
    /// Turns serialized values and enum members into the text that editor rows show.
    /// </summary>
    public static class ValueDisplayAPI
    {
        public const string DEFAULT_TEXT = "Default";

        private const string VALUE = nameof(Overridable<int>.value);
        private const string IS_OVERRIDDEN = nameof(Overridable<int>.isOverridden);

        /// <summary>
        /// Gets the display name of every value of an enum.
        /// </summary>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>
        /// The <c>[InspectorName]</c> or the nicified name of each enum value; an empty array for a type that is not
        /// an enum.
        /// </returns>
        public static string[] GetValueChoices(Type valueType)
        {
            if (valueType.IsEnum == false)
            {
                return Array.Empty<string>();
            }

            var values = Enum.GetValues(valueType);
            var choices = new string[values.Length];

            for (var i = 0; i < choices.Length; i++)
            {
                choices[i] = GetEnumDisplayName(valueType, values.GetValue(i));
            }

            return choices;
        }

        /// <summary>
        /// Gets the text that a row shows for a value.
        /// </summary>
        /// <remarks>
        /// The text is empty for <see langword="null"/>, "True" or "False" for a bool, <c>[a, b]</c> for a list, and
        /// the display name for a non-flags enum value. An <see cref="Overridable{T}"/> shows its value, or "Default"
        /// when it is not overridden. <see cref="AnimationCurve"/>, <see cref="Gradient"/>, <see cref="LayerMask"/>,
        /// <see cref="char"/> and <see cref="Quaternion"/> have their own forms; any other value uses
        /// <see cref="object.ToString"/>.
        /// </remarks>
        /// <param name="valueType">The type of the value.</param>
        /// <param name="value">The value to show.</param>
        /// <returns>The text.</returns>
        public static string GetDisplayText(Type valueType, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (valueType == typeof(bool) && value is bool boolValue)
            {
                return boolValue ? bool.TrueString : bool.FalseString;
            }

            if (value is IList list)
            {
                var count = list.Count;
                var elements = new string[count];

                for (var i = 0; i < count; i++)
                {
                    var element = list[i];
                    elements[i] = GetDisplayText(element?.GetType() ?? typeof(object), element);
                }

                return $"[{string.Join(", ", elements)}]";
            }

            if (IsGenericOf(valueType, typeof(Overridable<>)))
            {
                var isOverridden = (bool)valueType.GetField(IS_OVERRIDDEN).GetValue(value);

                return isOverridden
                    ? GetDisplayText(valueType.GetGenericArguments()[0], valueType.GetField(VALUE).GetValue(value))
                    : DEFAULT_TEXT;
            }

            if (IsDropdownEnum(valueType) && Enum.IsDefined(valueType, value))
            {
                return GetEnumDisplayName(valueType, value);
            }

            return value switch {
                AnimationCurve curve => GetCurveDisplayText(curve),
                Gradient gradient => GetGradientDisplayText(gradient),
                LayerMask mask => mask.value.ToString(CultureInfo.InvariantCulture),
                char character => character switch {
                    '\0' => "'\\0'",
                    '\n' => "'\\n'",
                    '\r' => "'\\r'",
                    '\t' => "'\\t'",
                    _ => $"'{character}'",
                },
                Quaternion rotation => $"Euler {rotation.eulerAngles.ToString("F2", CultureInfo.InvariantCulture)}",
                _ => value.ToString(),
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurveDisplayText(AnimationCurve curve)
        {
            var keys = curve.keys;
            var count = keys.Length;
            var values = new string[count];

            for (var i = 0; i < count; i++)
            {
                var key = keys[i];
                values[i] = FormattableString.Invariant($"({key.time}, {key.value})");
            }

            return $"Curve [{string.Join(", ", values)}] ({curve.preWrapMode} / {curve.postWrapMode})";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetGradientDisplayText(Gradient gradient)
        {
            var colorKeys = gradient.colorKeys;
            var alphaKeys = gradient.alphaKeys;
            var colors = new string[colorKeys.Length];
            var alphas = new string[alphaKeys.Length];

            for (var i = 0; i < colors.Length; i++)
            {
                var key = colorKeys[i];
                var color = key.color.ToString("F3", CultureInfo.InvariantCulture);
                colors[i] = FormattableString.Invariant($"{key.time}: {color}");
            }

            for (var i = 0; i < alphas.Length; i++)
            {
                var key = alphaKeys[i];
                alphas[i] = FormattableString.Invariant($"{key.time}: {key.alpha}");
            }

            return $"Gradient ({gradient.mode}): colors [{string.Join(", ", colors)}]; "
                + $"alpha [{string.Join(", ", alphas)}]";
        }

        public static string GetEnumDisplayName(Type enumType, object value)
        {
            var name = Enum.GetName(enumType, value);
            var field = name == null ? null : enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            var inspectorName = field?.GetCustomAttribute<InspectorNameAttribute>();

            if (inspectorName != null)
            {
                return inspectorName.displayName;
            }

            return name == null ? value.ToString() : ObjectNames.NicifyVariableName(name);
        }

        public static bool IsGenericOf(Type type, Type definition)
            => type.IsGenericType && type.GetGenericTypeDefinition() == definition;

        public static bool IsDropdownEnum(Type valueType)
            => valueType.IsEnum && valueType.IsDefined(typeof(FlagsAttribute), inherit: false) == false;
    }
}

#endif
