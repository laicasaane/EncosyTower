#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Serialization
{
    internal static class OverridableDrawerModel
    {
        public const string VALUE = nameof(Overridable<int>.value);
        public const string IS_OVERRIDDEN = nameof(Overridable<int>.isOverridden);
        public const string PROJECT_SETTINGS_LABEL = "Project Settings";
        public const string DEFAULT_LABEL = "Default";
        public const string MIXED_TEXT = "Mixed";

        private const string OFF = "Off";
        private const string ON = "On";
        private const string INSTANCE = "Instance";

        private static readonly string[] s_boolChoices = { OFF, ON };

        public static OverridableMode GetMode(Type valueType)
            => valueType == typeof(bool) || IsDropdownEnum(valueType)
                ? OverridableMode.Dropdown
                : OverridableMode.Toggle;

        public static string GetDefaultLabel(OverridableDefaultAttribute attribute)
        {
            if (attribute == null)
            {
                return DEFAULT_LABEL;
            }

            if (string.IsNullOrEmpty(attribute.Label) == false)
            {
                return attribute.Label;
            }

            return IsSettingsType(attribute.SourceType) ? PROJECT_SETTINGS_LABEL : DEFAULT_LABEL;
        }

        public static string[] GetValueChoices(Type valueType)
        {
            if (valueType == typeof(bool))
            {
                return (string[])s_boolChoices.Clone();
            }

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

        public static bool TryGetDefaultValue(OverridableDefaultAttribute attribute, out object value)
        {
            value = null;

            if (attribute == null || attribute.SourceType == null || string.IsNullOrEmpty(attribute.MemberName))
            {
                return false;
            }

            var sourceType = attribute.SourceType;

            if (IsSettingsType(sourceType))
            {
                var instance = GetSettingsAsset(attribute);

                return instance.IsValid()
                    && TryReadMember(sourceType, instance, attribute.MemberName, BindingFlags.Instance, out value);
            }

            return TryReadMember(sourceType, null, attribute.MemberName, BindingFlags.Static, out value);
        }

        public static UnityEngine.Object GetSettingsAsset(OverridableDefaultAttribute attribute)
        {
            if (attribute == null || attribute.SourceType == null || IsSettingsType(attribute.SourceType) == false)
            {
                return null;
            }

            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            return attribute.SourceType.GetProperty(INSTANCE, flags)?.GetValue(null) as UnityEngine.Object;
        }

        public static string GetEffectiveText(
              SerializedProperty property
            , OverridableDefaultAttribute attribute
            , Type valueType
        )
        {
            var isOverridden = property.FindPropertyRelative(IS_OVERRIDDEN);
            var value = property.FindPropertyRelative(VALUE);

            if (IsMixed(isOverridden, value))
            {
                return MIXED_TEXT;
            }

            if (isOverridden.boolValue)
            {
                return GetDisplayText(valueType, ReadValue(value, valueType));
            }

            return TryGetDefaultValue(attribute, out var defaultValue)
                ? GetDisplayText(valueType, defaultValue)
                : string.Empty;
        }

        public static void ApplyChoice(SerializedProperty property, Type valueType, int choiceIndex)
        {
            var isOverridden = property.FindPropertyRelative(IS_OVERRIDDEN);

            if (choiceIndex <= 0)
            {
                isOverridden.boolValue = false;
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            var value = property.FindPropertyRelative(VALUE);
            var valueIndex = choiceIndex - 1;

            if (valueType == typeof(bool))
            {
                value.boolValue = valueIndex == 1;
            }
            else if (valueType.IsEnum)
            {
                var values = Enum.GetValues(valueType);
                value.longValue = Convert.ToInt64(values.GetValue(valueIndex));
            }

            isOverridden.boolValue = true;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static void Reset(SerializedProperty property)
        {
            property.FindPropertyRelative(IS_OVERRIDDEN).boolValue = false;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static int GetSelectedIndex(SerializedProperty property, Type valueType)
        {
            var isOverridden = property.FindPropertyRelative(IS_OVERRIDDEN);
            var value = property.FindPropertyRelative(VALUE);

            if (IsMixed(isOverridden, value))
            {
                return -1;
            }

            if (isOverridden.boolValue == false)
            {
                return 0;
            }

            if (valueType == typeof(bool))
            {
                return value.boolValue ? 2 : 1;
            }

            if (valueType.IsEnum)
            {
                var index = Array.IndexOf(Enum.GetValues(valueType), ReadValue(value, valueType));
                return index < 0 ? -1 : index + 1;
            }

            return 0;
        }

        public static bool IsMixed(SerializedProperty property)
            => IsMixed(property.FindPropertyRelative(IS_OVERRIDDEN), property.FindPropertyRelative(VALUE));

        public static bool CanReset(SerializedProperty property)
        {
            var isOverridden = property.FindPropertyRelative(IS_OVERRIDDEN);
            return isOverridden.hasMultipleDifferentValues || isOverridden.boolValue;
        }

        public static Type GetValueType(Type fieldType)
        {
            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            else if (IsGenericOf(fieldType, typeof(List<>)))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            return IsGenericOf(fieldType, typeof(Overridable<>)) ? fieldType.GetGenericArguments()[0] : typeof(object);
        }

        private static bool IsGenericOf(Type type, Type definition)
            => type.IsGenericType && type.GetGenericTypeDefinition() == definition;

        private static bool IsMixed(SerializedProperty isOverridden, SerializedProperty value)
            => isOverridden.hasMultipleDifferentValues || (isOverridden.boolValue && value.hasMultipleDifferentValues);

        private static bool IsDropdownEnum(Type valueType)
            => valueType.IsEnum && valueType.IsDefined(typeof(FlagsAttribute), inherit: false) == false;

        private static bool IsSettingsType(Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (IsGenericOf(current, typeof(EncosyTower.Settings.Settings<>)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadMember(
              Type type
            , object instance
            , string memberName
            , BindingFlags kind
            , out object value
        )
        {
            var flags = kind | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            var field = type.GetField(memberName, flags);

            if (field != null)
            {
                value = field.GetValue(instance);
                return true;
            }

            var property = type.GetProperty(memberName, flags);

            if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
            {
                value = property.GetValue(instance);
                return true;
            }

            value = null;
            return false;
        }

        private static object ReadValue(SerializedProperty value, Type valueType)
        {
            if (valueType == typeof(bool))
            {
                return value.boolValue;
            }

            if (valueType.IsEnum)
            {
                return Enum.ToObject(valueType, value.longValue);
            }

            return value.boxedValue;
        }

        private static string GetDisplayText(Type valueType, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (valueType == typeof(bool) && value is bool boolValue)
            {
                return boolValue ? ON : OFF;
            }

            if (IsDropdownEnum(valueType) && Enum.IsDefined(valueType, value))
            {
                return GetEnumDisplayName(valueType, value);
            }

            return value.ToString();
        }

        private static string GetEnumDisplayName(Type enumType, object value)
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
    }
}

#endif
