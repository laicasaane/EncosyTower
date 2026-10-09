#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Editor.Serialization.Internals;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Internals
{
    internal static class ValueChoicesEditorAPI
    {
        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private static readonly Dictionary<string, (Type type, ValueChoicesError error)> s_sourceTypes
            = new(StringComparer.Ordinal);

        public static bool TryGetChoices(
              ValueChoicesAttribute attribute
            , Type declaringType
            , Type valueType
            , out IReadOnlyList<(string label, object value)> choices
            , out ValueChoicesError error
        )
        {
            var memberName = attribute?.MemberName ?? "<unspecified>";
            var sourceType = attribute?.SourceType ?? declaringType;

            if (attribute == null || string.IsNullOrEmpty(attribute.MemberName))
            {
                error = ValueChoicesError.MissingMemberName(sourceType, memberName);
                goto FAILED;
            }

            if (attribute.SourceTypeName != null
                && TryResolveSourceType(attribute.SourceTypeName, out sourceType, out error) == false
            )
            {
                goto FAILED;
            }

            if (sourceType == null || valueType == null)
            {
                error = ValueChoicesError.MissingType(sourceType, memberName, valueType);
                goto FAILED;
            }

            var targetsCollection = attribute.applyToCollection;

            if (targetsCollection && IsCollectionType(valueType) == false)
            {
                error = ValueChoicesError.TargetTypeMismatch(sourceType, memberName, valueType);
                goto FAILED;
            }

            if (targetsCollection && attribute.IsExclusive)
            {
                error = ValueChoicesError.ExclusiveCollection(sourceType, memberName);
                goto FAILED;
            }

            try
            {
                if (TryReadMember(sourceType, memberName, out var memberType, out var source, out error) == false)
                {
                    goto FAILED;
                }

                if (TryCollectChoices(
                      sourceType
                    , memberName
                    , memberType
                    , source
                    , valueType
                    , targetsCollection
                    , out choices
                    , out error
                ) == false
                )
                {
                    goto FAILED;
                }

                error = default;
                return true;
            }
            catch (Exception exception)
            {
                error = ValueChoicesError.ReadFailure(sourceType, memberName, exception.GetBaseException());
            }

        FAILED:
            choices = Array.Empty<(string label, object value)>();
            return false;
        }

        public static bool Matches(object a, object b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a is UnityEngine.Object || b is UnityEngine.Object)
            {
                return false;
            }

            if (a is IList left && b is IList right)
            {
                var count = left.Count;

                if (count != right.Count)
                {
                    return false;
                }

                for (var i = 0; i < count; i++)
                {
                    if (Matches(left[i], right[i]) == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            if (a is IList || b is IList)
            {
                return false;
            }

            if (a.GetType().IsValueType || b.GetType().IsValueType || a is string || b is string)
            {
                return Equals(a, b);
            }

            return string.Equals(EditorJsonUtility.ToJson(a), EditorJsonUtility.ToJson(b), StringComparison.Ordinal);
        }

        public static bool IsCollectionType(Type valueType)
            => valueType != null
                && ((valueType.IsArray && valueType.GetArrayRank() == 1)
                    || (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
                );

        public static bool UsesDropdown(ValueChoicesAttribute attribute, Type valueType)
            => attribute?.applyToCollection != true
                && (OverridableEditorAPI.GetMode(valueType) == OverridableMode.Dropdown
                    || attribute?.IsExclusive == true
                );

        public static bool ListsOnlyChoices(ValueChoicesAttribute attribute, Type valueType)
            => attribute?.IsExclusive == true;

        private static bool TryResolveSourceType(
              string sourceTypeName
            , out Type sourceType
            , out ValueChoicesError error
        )
        {
            if (s_sourceTypes.TryGetValue(sourceTypeName, out var result) == false)
            {
                result = ResolveSourceType(sourceTypeName);
                s_sourceTypes.Add(sourceTypeName, result);
            }

            sourceType = result.type;
            error = result.error;
            return sourceType != null;
        }

        private static (Type type, ValueChoicesError error) ResolveSourceType(string sourceTypeName)
        {
            try
            {
                var typeName = sourceTypeName;
                string assemblyName = null;
                var assemblySeparator = GetAssemblySeparatorIndex(sourceTypeName);

                if (assemblySeparator >= 0)
                {
                    Type type;

                    try
                    {
                        type = Type.GetType(typeName: sourceTypeName, throwOnError: false, ignoreCase: false);
                    }
                    catch (Exception)
                    {
                        type = null;
                    }

                    if (type != null)
                    {
                        return (type, default);
                    }

                    typeName = sourceTypeName[..assemblySeparator].Trim();
                    assemblyName = sourceTypeName[(assemblySeparator + 1)..].Trim();
                }

                Type sourceType = null;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var count = assemblies.Length;

                for (var i = 0; i < count; i++)
                {
                    var assembly = assemblies[i];

                    if (assemblyName != null && MatchesAssemblyName(assembly, assemblyName) == false)
                    {
                        continue;
                    }

                    var candidate = assembly.GetType(name: typeName, throwOnError: false, ignoreCase: false);

                    if (candidate == null)
                    {
                        continue;
                    }

                    if (sourceType != null && sourceType != candidate)
                    {
                        return (null, ValueChoicesError.AmbiguousSourceType(sourceTypeName));
                    }

                    sourceType = candidate;
                }

                return sourceType != null
                    ? (sourceType, default)
                    : (null, ValueChoicesError.SourceTypeNotFound(sourceTypeName));
            }
            catch (Exception)
            {
                return (null, ValueChoicesError.SourceTypeNotFound(sourceTypeName));
            }

            static bool MatchesAssemblyName(Assembly assembly, string name)
                => string.Equals(assembly.FullName, name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase)
                ;
        }

        private static int GetAssemblySeparatorIndex(string sourceTypeName)
        {
            var depth = 0;
            var length = sourceTypeName.Length;

            for (var i = 0; i < length; i++)
            {
                var character = sourceTypeName[i];

                if (character == '\\')
                {
                    i++;
                }
                else if (character == '[')
                {
                    depth++;
                }
                else if (character == ']')
                {
                    depth--;
                }
                else if (character == ',' && depth == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool HasCollectionChoiceType(Type memberType)
        {
            if (memberType.IsArray)
            {
                return IsCollectionChoiceType(memberType.GetElementType());
            }

            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return IsCollectionChoiceType(memberType.GetGenericArguments()[0]);
            }

            var interfaces = memberType.GetInterfaces();
            var count = interfaces.Length;

            for (var i = 0; i < count; i++)
            {
                var type = interfaces[i];

                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && IsCollectionChoiceType(type.GetGenericArguments()[0])
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCollectionChoiceType(Type choiceType)
        {
            if (choiceType.IsGenericType && choiceType.GetGenericTypeDefinition() == typeof(ValueChoice<>))
            {
                choiceType = choiceType.GetGenericArguments()[0];
            }

            return IsCollectionType(choiceType);
        }

        private static bool TryReadMember(
              Type sourceType
            , string memberName
            , out Type memberType
            , out object source
            , out ValueChoicesError error
        )
        {
            var field = sourceType.GetField(memberName, STATIC_FLAGS);

            if (field != null)
            {
                memberType = field.FieldType;
                source = field.GetValue(obj: null);
                error = default;
                return true;
            }

            var property = sourceType.GetProperty(memberName, STATIC_FLAGS);

            if (property != null)
            {
                var getter = property.GetGetMethod(nonPublic: true);

                if (getter == null || getter.IsStatic == false || property.GetIndexParameters().Length != 0)
                {
                    error = ValueChoicesError.InvalidProperty(sourceType, memberName);
                    goto FAILED;
                }

                memberType = property.PropertyType;
                source = property.GetValue(obj: null);
                error = default;
                return true;
            }

            var methods = sourceType.GetMethods(STATIC_FLAGS);
            var count = methods.Length;
            var hasMethod = false;

            for (var i = 0; i < count; i++)
            {
                var method = methods[i];

                if (method.Name != memberName)
                {
                    continue;
                }

                hasMethod = true;

                if (method.ContainsGenericParameters || method.GetParameters().Length != 0)
                {
                    continue;
                }

                memberType = method.ReturnType;
                source = method.Invoke(obj: null, parameters: null);
                error = default;
                return true;
            }

            if (hasMethod)
            {
                error = ValueChoicesError.InvalidMethod(sourceType, memberName);
            }
            else if (sourceType.GetMember(memberName, INSTANCE_FLAGS).Length != 0)
            {
                error = ValueChoicesError.InstanceMember(sourceType, memberName);
            }
            else
            {
                error = ValueChoicesError.MissingMember(sourceType, memberName);
            }

        FAILED:
            memberType = null;
            source = null;
            return false;
        }

        private static bool TryCollectChoices(
              Type sourceType
            , string memberName
            , Type memberType
            , object source
            , Type valueType
            , bool applyToCollection
            , out IReadOnlyList<(string label, object value)> choices
            , out ValueChoicesError error
        )
        {
            var choiceType = typeof(ValueChoice<>).MakeGenericType(valueType);
            var isLabelled = typeof(IEnumerable<>).MakeGenericType(choiceType).IsAssignableFrom(memberType);
            var isErased = typeof(IEnumerable<ValueChoice>).IsAssignableFrom(memberType);
            var isValues = typeof(IEnumerable<>).MakeGenericType(valueType).IsAssignableFrom(memberType);

            if (isLabelled == false && isErased == false && isValues == false)
            {
                error = applyToCollection || HasCollectionChoiceType(memberType)
                    ? ValueChoicesError.ChoiceTargetTypeMismatch(
                          sourceType
                        , memberName
                        , applyToCollection
                        , valueType
                        , memberType
                    )
                    : ValueChoicesError.UnsupportedMemberType(sourceType, memberName, memberType);

                goto FAILED;
            }

            var enumerable = source as IEnumerable;

            if (enumerable == null)
            {
                error = ValueChoicesError.InvalidCollection(sourceType, memberName);
                goto FAILED;
            }

            var labelProperty = isLabelled ? choiceType.GetProperty(nameof(ValueChoice<int>.Label)) : null;
            var valueProperty = isLabelled ? choiceType.GetProperty(nameof(ValueChoice<int>.Value)) : null;
            var acceptsNull = valueType.IsValueType == false || Nullable.GetUnderlyingType(valueType) != null;
            var result = new List<(string label, object value)>();

            foreach (var item in enumerable)
            {
                string label = null;
                var value = item;

                if (isErased && item is ValueChoice erasedChoice)
                {
                    label = erasedChoice.Label;
                    value = erasedChoice.Value;
                }
                else if (isLabelled)
                {
                    label = (string)labelProperty.GetValue(item);
                    value = valueProperty.GetValue(item);
                }

                if (value == null ? acceptsNull == false : valueType.IsInstanceOfType(value) == false)
                {
                    error = applyToCollection || IsCollectionType(value?.GetType())
                        ? ValueChoicesError.ChoiceTargetTypeMismatch(
                              sourceType
                            , memberName
                            , applyToCollection
                            , valueType
                            , value?.GetType()
                        )
                        : ValueChoicesError.IncompatibleValue(sourceType, memberName, value, valueType);

                    goto FAILED;
                }

                if (string.IsNullOrEmpty(label))
                {
                    var name = valueType.IsEnum && value != null ? Enum.GetName(valueType, value) : null;

                    label = name != null
                        ? valueType.GetField(name)?.GetCustomAttribute<InspectorNameAttribute>()?.displayName
                        : null;

                    label ??= OverridableEditorAPI.GetDisplayText(valueType, value);
                }

                result.Add((label, value));
            }

            choices = result;
            error = default;
            return true;

        FAILED:
            choices = Array.Empty<(string label, object value)>();
            return false;
        }
    }
}

#endif
