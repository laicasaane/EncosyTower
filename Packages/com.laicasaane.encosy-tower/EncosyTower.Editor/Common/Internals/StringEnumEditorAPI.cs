#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Common;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEditor;

namespace EncosyTower.Editor.Internals
{
    internal static class StringEnumEditorAPI
    {
        public const string VALUE = nameof(StringEnum<DayOfWeek>._value);
        public const string NOT_SET_TEXT = "(Not set)";

        private static readonly Dictionary<Type, ChoicesResult> s_results = new();

        public static void ClearCache()
        {
            s_results.Clear();
        }

        public static bool TryGetStructType(FieldInfo fieldInfo, out Type structType)
        {
            structType = null;

            var type = fieldInfo?.FieldType;

            if (type == null)
            {
                return false;
            }

            if (type.IsArray && type.GetArrayRank() == 1)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type.IsGenericType == false)
            {
                return false;
            }

            var definition = type.GetGenericTypeDefinition();

            if (definition != typeof(StringEnum<>) && definition != typeof(StringEnum<,>))
            {
                return false;
            }

            structType = type;
            return true;
        }

        public static bool TryGetChoices(Type structType, out IReadOnlyList<ValueChoice> choices)
            => TryGetChoices(structType, out choices, out _);

        public static bool TryGetChoices(Type structType, out IReadOnlyList<ValueChoice> choices, out string error)
        {
            if (s_results.TryGetValue(structType, out var result) == false)
            {
                result = ResolveChoices(structType);
                s_results.Add(structType, result);
            }

            choices = result.Choices;
            error = result.Error;
            return result.Resolved;
        }

        public static int GetChoiceIndex(IReadOnlyList<ValueChoice> choices, string stored)
            => string.IsNullOrEmpty(stored) ? -1 : ValueChoicesEditorAPI.GetChoiceIndex(choices, stored);

        public static string GetShownText(IReadOnlyList<ValueChoice> choices, string stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return NOT_SET_TEXT;
            }

            var index = GetChoiceIndex(choices, stored);
            return index >= 0 ? choices[index].Label : stored;
        }

        public static bool IsUnlisted(IReadOnlyList<ValueChoice> choices, string stored)
            => string.IsNullOrEmpty(stored) == false && GetChoiceIndex(choices, stored) < 0;

        public static bool TryWrite(SerializedObject serializedObject, string structPath, string stored)
        {
            if (serializedObject.targetObject.IsInvalid())
            {
                return false;
            }

            serializedObject.UpdateIfRequiredOrScript();

            using var structProperty = serializedObject.FindProperty(structPath);

            if (structProperty == null || PropertyLockAPI.IsLocked(structProperty))
            {
                return false;
            }

            using var valueProperty = structProperty.FindPropertyRelative(VALUE);

            if (valueProperty == null)
            {
                return false;
            }

            valueProperty.stringValue = stored;
            serializedObject.ApplyModifiedProperties();
            return true;
        }

        public static ValueChoiceMenuItem[] CreateMenuItems(
              SerializedObject serializedObject
            , string structPath
            , IReadOnlyList<ValueChoice> choices
            , string stored
            , bool isMixed
        )
        {
            return ValueChoicesEditorAPI.CreateChoiceMenuItems(choices, stored, isMixed, Apply);

            void Apply(object value)
            {
                TryWrite(serializedObject, structPath, (string)value);
            }
        }

        private static ChoicesResult ResolveChoices(Type structType)
        {
            var arguments = structType.GetGenericArguments();
            var enumType = arguments[0];
            var labels = ValueDisplayAPI.GetValueChoices(enumType);
            var values = Enum.GetValues(enumType);
            var choices = new List<ValueChoice>(values.Length);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            try
            {
                object converter = null;
                var transform = arguments.Length > 1 ? GetTransform(enumType, arguments[1], out converter) : null;

                for (var i = 0; i < values.Length; i++)
                {
                    var value = values.GetValue(i);

                    var text = transform == null
                        ? value.ToString()
                        : (string)transform.Invoke(converter, new[] { value });

                    if (string.IsNullOrEmpty(text) || seen.Add(text) == false)
                    {
                        continue;
                    }

                    choices.Add(new ValueChoice(labels[i], text));
                }
            }
            catch (Exception exception)
            {
                var cause = exception is TargetInvocationException { InnerException: { } inner } ? inner : exception;

                StaticDevLogger.LogWarning(
                    $"StringEnum drawer cannot read the choices of {structType.Name}: {cause.Message}"
                );

                return new ChoicesResult(
                      false
                    , Array.Empty<ValueChoice>()
                    , $"Cannot read the choices of {structType.Name}: {cause.Message}"
                );
            }

            return new ChoicesResult(true, choices, null);

            static MethodInfo GetTransform(Type enumType, Type converterType, out object converter)
            {
                converter = Activator.CreateInstance(converterType);

                return typeof(IStringEnumConverter<>)
                    .MakeGenericType(enumType)
                    .GetMethod(nameof(IStringEnumConverter<DayOfWeek>.Transform), new[] { enumType });
            }
        }

        private readonly record struct ChoicesResult(bool Resolved, IReadOnlyList<ValueChoice> Choices, string Error);
    }
}

#endif
