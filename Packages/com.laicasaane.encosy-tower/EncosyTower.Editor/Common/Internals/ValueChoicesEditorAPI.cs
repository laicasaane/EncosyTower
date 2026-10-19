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
    /// <summary>
    /// Resolves a <see cref="ValueChoicesAttribute"/> into <see cref="ValueChoice"/> lists and holds the choice
    /// helpers shared by both drawers and the Overridable rows.
    /// </summary>
    /// <remarks>
    /// Resolution reads the member by reflection. Non-public static members are accepted, and a property or method
    /// is evaluated on every resolution. Failures are returned as <see cref="ValueChoicesError"/>, not thrown.
    /// </remarks>
    internal static class ValueChoicesEditorAPI
    {
        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private const BindingFlags INSTANCE_FLAGS = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private static readonly Dictionary<string, SourceTypeResult> s_sourceTypes
            = new(StringComparer.Ordinal);

        /// <summary>
        /// Resolves the choices that an attribute names.
        /// </summary>
        /// <remarks>
        /// The checks run in this order, and the first failure is reported: member name, string source type name,
        /// source and value type, <c>applyToCollection</c> with the value type, <c>applyToCollection</c> with
        /// <c>IsExclusive</c>, then the member read and the choice checks. An exception thrown while reading the
        /// member is returned as <c>ReadFailure</c> with its base exception.
        /// </remarks>
        /// <param name="attribute">The attribute that names the member.</param>
        /// <param name="declaringType">The type that declares the field. It is the source type when the attribute
        /// names none.</param>
        /// <param name="valueType">The element type, or the array or list type when the attribute sets
        /// <c>applyToCollection</c>.</param>
        /// <param name="choices">Receives the choices, or an empty list on failure.</param>
        /// <param name="error">Receives the failure, or the default value on success.</param>
        /// <returns>
        /// <see langword="true"/> when the choices were resolved; otherwise a failure; see
        /// <see cref="ValueChoicesError"/>.
        /// </returns>
        public static bool TryGetChoices(
              ValueChoicesAttribute attribute
            , Type declaringType
            , Type valueType
            , out IReadOnlyList<ValueChoice> choices
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
            choices = Array.Empty<ValueChoice>();
            return false;
        }

        /// <summary>
        /// Compares two choice values. The result decides which menu item is checked and which dropdown item is
        /// selected.
        /// </summary>
        /// <remarks>
        /// Two references to the same object match. A null value, or a Unity object that is not the same object,
        /// does not match. Two lists match when their elements match in order; a list never matches a non-list.
        /// Value types and strings compare with <see cref="object.Equals(object, object)"/>. Other objects match when
        /// their <see cref="EditorJsonUtility.ToJson(object)"/> text is equal.
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns><see langword="true"/> when the values match.</returns>
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

        /// <summary>
        /// Checks whether a value type accepts <c>applyToCollection</c>.
        /// </summary>
        /// <param name="valueType">The field value type.</param>
        /// <returns><see langword="true"/> for a one-dimensional array or a <c>List&lt;T&gt;</c>.</returns>
        public static bool IsCollectionType(Type valueType)
            => valueType != null
                && ((valueType.IsArray && valueType.GetArrayRank() == 1)
                    || (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
                );

        /// <summary>
        /// Checks whether a field is drawn as a dropdown instead of its normal input with a menu button.
        /// </summary>
        /// <param name="attribute">The attribute of the field.</param>
        /// <param name="valueType">The field value type.</param>
        /// <returns>
        /// <see langword="true"/> when the attribute does not set <c>applyToCollection</c> and the value type is a
        /// non-flags enum or the attribute is exclusive.
        /// </returns>
        public static bool UsesDropdown(ValueChoicesAttribute attribute, Type valueType)
            => attribute?.applyToCollection != true
                && (OverridableEditorAPI.GetMode(valueType) == OverridableMode.Dropdown
                    || attribute?.IsExclusive == true
                );

        public static bool IsExclusive(ValueChoicesAttribute attribute)
            => attribute?.IsExclusive == true;

        /// <summary>
        /// Gets the choices that a dropdown lists.
        /// </summary>
        /// <remarks>
        /// An exclusive attribute lists only its own choices. Otherwise the dropdown lists every value of the enum
        /// and ignores <paramref name="choices"/>; those choices then feed only the menu button.
        /// </remarks>
        /// <param name="attribute">The attribute of the field.</param>
        /// <param name="valueType">The field value type.</param>
        /// <param name="choices">The resolved choices.</param>
        /// <returns>The choices to list.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the attribute is not exclusive and <paramref name="valueType"/> is not an enum.
        /// </exception>
        public static IReadOnlyList<ValueChoice> GetSelectableChoices(
              ValueChoicesAttribute attribute
            , Type valueType
            , IReadOnlyList<ValueChoice> choices
        )
        {
            if (IsExclusive(attribute))
            {
                return choices;
            }

            var values = Enum.GetValues(valueType);
            var labels = ValueDisplayAPI.GetValueChoices(valueType);
            var count = values.Length;
            var result = new ValueChoice[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = new ValueChoice(labels[i], values.GetValue(i));
            }

            return result;
        }

        /// <summary>
        /// Finds the first choice whose value matches a value.
        /// </summary>
        /// <param name="choices">The choices to search.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>The index of the choice, or -1 when no choice matches; see <see cref="Matches"/>.</returns>
        public static int GetChoiceIndex(IReadOnlyList<ValueChoice> choices, object value)
        {
            var count = choices.Count;

            for (var i = 0; i < count; i++)
            {
                if (Matches(choices[i].Value, value))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Creates one menu item for each choice.
        /// </summary>
        /// <remarks>
        /// The caller renders the items, as a UI Toolkit or an IMGUI menu.
        /// </remarks>
        /// <param name="choices">The choices to list.</param>
        /// <param name="currentValue">The value of the field, used to check an item.</param>
        /// <param name="isMixed">Whether the selected objects differ; no item is checked then.</param>
        /// <param name="onSelected">Called with the value of the choice that the user picks.</param>
        /// <returns>The items, in the order of <paramref name="choices"/>.</returns>
        public static ValueChoiceMenuItem[] CreateChoiceMenuItems(
              IReadOnlyList<ValueChoice> choices
            , object currentValue
            , bool isMixed
            , Action<object> onSelected
        )
        {
            var count = choices.Count;
            var result = new ValueChoiceMenuItem[count];

            for (var i = 0; i < count; i++)
            {
                var choice = choices[i];
                var isChecked = isMixed == false && Matches(choice.Value, currentValue);
                result[i] = new ValueChoiceMenuItem(new GUIContent(choice.Label), isChecked, SelectChoice);

                void SelectChoice()
                {
                    onSelected(choice.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves a source type name once for each name.
        /// </summary>
        /// <remarks>
        /// The result of <see cref="ResolveSourceType"/> is cached by name, failures included, and the cache is
        /// never invalidated while the domain lives.
        /// </remarks>
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

            sourceType = result.Type;
            error = result.Error;
            return sourceType != null;
        }

        /// <summary>
        /// Finds the type that a source type name denotes.
        /// </summary>
        /// <remarks>
        /// A name with a comma outside brackets is assembly-qualified and is first tried with
        /// <see cref="Type.GetType(string, bool, bool)"/>. Then the loaded assemblies are scanned, only the assembly
        /// that matches the given assembly name when there is one. Two different types are
        /// <c>AmbiguousSourceType</c>, none is <c>SourceTypeNotFound</c>, and so is any exception.
        /// </remarks>
        private static SourceTypeResult ResolveSourceType(string sourceTypeName)
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
                        return new SourceTypeResult(type, default);
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
                        return new SourceTypeResult(null, ValueChoicesError.AmbiguousSourceType(sourceTypeName));
                    }

                    sourceType = candidate;
                }

                return sourceType != null
                    ? new SourceTypeResult(sourceType, default)
                    : new SourceTypeResult(null, ValueChoicesError.SourceTypeNotFound(sourceTypeName));
            }
            catch (Exception)
            {
                return new SourceTypeResult(null, ValueChoicesError.SourceTypeNotFound(sourceTypeName));
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

        /// <summary>
        /// Reads the member that supplies the choices.
        /// </summary>
        /// <remarks>
        /// The lookup order is a static field, a static property, the first parameterless non-generic static method,
        /// which is invoked, then instance members, which are an error. Non-public and inherited static members are
        /// found. A property needs a static getter and no index parameters.
        /// </remarks>
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

        /// <summary>
        /// Reads the choices from the value of the member.
        /// </summary>
        /// <remarks>
        /// The member type must be an enumerable of <c>ValueChoice&lt;TValue&gt;</c>, of <see cref="ValueChoice"/> or
        /// of <c>TValue</c>, where <c>TValue</c> is the value type. A <see langword="null"/> value is accepted only
        /// for a reference type or a nullable type. A choice without a label gets the <c>[InspectorName]</c> of an
        /// enum member, otherwise the display text of the value.
        /// </remarks>
        private static bool TryCollectChoices(
              Type sourceType
            , string memberName
            , Type memberType
            , object source
            , Type valueType
            , bool applyToCollection
            , out IReadOnlyList<ValueChoice> choices
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

            if (source is not IEnumerable enumerable)
            {
                error = ValueChoicesError.InvalidCollection(sourceType, memberName);
                goto FAILED;
            }

            var labelProperty = isLabelled ? choiceType.GetProperty(nameof(ValueChoice<int>.Label)) : null;
            var valueProperty = isLabelled ? choiceType.GetProperty(nameof(ValueChoice<int>.Value)) : null;
            var acceptsNull = valueType.IsValueType == false || Nullable.GetUnderlyingType(valueType) != null;
            var result = new List<ValueChoice>();

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

                    label ??= ValueDisplayAPI.GetDisplayText(valueType, value);
                }

                result.Add(new ValueChoice(label, value));
            }

            choices = result;
            error = default;
            return true;

        FAILED:
            choices = Array.Empty<ValueChoice>();
            return false;
        }

        private readonly record struct SourceTypeResult(Type Type, ValueChoicesError Error);
    }
}

#endif
