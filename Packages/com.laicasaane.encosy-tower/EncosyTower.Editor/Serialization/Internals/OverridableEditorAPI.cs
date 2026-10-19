#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using EncosyTower.Common;
using static EncosyTower.Editor.ValueDisplayAPI;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EncosyTower.Editor.Serialization.Internals
{
    /// <summary>
    /// Holds the rules that the Overridable rows share: default resolution, shown text, locking and the override
    /// transitions.
    /// </summary>
    /// <remarks>
    /// Both row implementations, <see cref="OverridableRowBuilder"/> for UI Toolkit and
    /// <see cref="OverridableImguiRow"/> for IMGUI, call these members, so the two behave the same.
    /// </remarks>
    internal static class OverridableEditorAPI
    {
        /// <summary>
        /// The serialized property name of <see cref="Overridable{T}.value"/>.
        /// </summary>
        public const string VALUE = nameof(Overridable<int>.value);

        /// <summary>
        /// The serialized property name of <see cref="Overridable{T}.isOverridden"/>.
        /// </summary>
        public const string IS_OVERRIDDEN = nameof(Overridable<int>.isOverridden);
        public const string PROJECT_SETTINGS_LABEL = "Project Settings";
        public const string DEFAULT_LABEL = ValueDisplayAPI.DEFAULT_TEXT;

        private const string INSTANCE = "Instance";
        private const string LOCATE_DEFAULT_SOURCE = "Locate Default Source";

        private static readonly Dictionary<Type, MonoScript> s_sourceScripts = new();

        private static readonly HashSet<(Type SourceType, string MemberName)> s_reportedDefaultErrors = new();

        private static readonly MethodInfo s_fillPropertyContextMenu = typeof(EditorGUI).GetMethod(
              name: "FillPropertyContextMenu"
            , bindingAttr: BindingFlags.Static | BindingFlags.NonPublic
            , binder: null
            , types: new[] {
                  typeof(SerializedProperty),
                  typeof(bool),
                  typeof(bool),
                  typeof(SerializedProperty),
                  typeof(GenericMenu),
                  typeof(UnityEngine.UIElements.VisualElement),
              }
            , modifiers: null
        ) ?? typeof(EditorGUI).GetMethod(
              name: "FillPropertyContextMenu"
            , bindingAttr: BindingFlags.Static | BindingFlags.NonPublic
            , binder: null
            , types: new[] { typeof(SerializedProperty), typeof(SerializedProperty), typeof(GenericMenu) }
            , modifiers: null
        );

        /// <summary>
        /// Chooses how a value type is drawn.
        /// </summary>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>
        /// <see cref="OverridableMode.Dropdown"/> for a non-flags enum; otherwise <see cref="OverridableMode.Toggle"/>.
        /// </returns>
        public static OverridableMode GetMode(Type valueType)
            => IsDropdownEnum(valueType)
                ? OverridableMode.Dropdown
                : OverridableMode.Toggle;

        /// <summary>
        /// Gets the label of the default item in an enum dropdown.
        /// </summary>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <returns>
        /// The <see cref="OverridableDefaultAttribute.Label"/> when it is set; otherwise "Project Settings" for a
        /// source type that derives from <c>Settings&lt;T&gt;</c>, and "Default" for any other case.
        /// </returns>
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

        /// <summary>
        /// Gets the text of the default item in an enum dropdown.
        /// </summary>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>
        /// The label and the display text of the default in parentheses when the default resolves; otherwise only the
        /// label; see <see cref="GetDefaultLabel"/>.
        /// </returns>
        public static string GetDefaultChoiceLabel(OverridableDefaultAttribute attribute, Type valueType)
        {
            var label = GetDefaultLabel(attribute);

            return TryGetDefaultValue(attribute, out var value)
                ? $"{label} ({GetDisplayText(valueType, value)})"
                : label;
        }

        /// <summary>
        /// Gets the default that a row shows while it is not overridden.
        /// </summary>
        /// <remarks>
        /// This value is also the seed copied into the value when the override starts. It allocates a new instance
        /// on every call, which is acceptable because only the Inspector calls it.
        /// </remarks>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>
        /// The resolved default; see <see cref="TryGetDefaultValue(OverridableDefaultAttribute, out object)"/>.
        /// When it does not resolve, the type default: an empty string for <see cref="string"/>,
        /// <see langword="null"/> for a Unity object, an empty array for an array, a new instance for a value type or
        /// for a non-abstract class with a public parameterless constructor, otherwise <see langword="null"/>.
        /// </returns>
        public static object GetShownDefault(OverridableDefaultAttribute attribute, Type valueType)
        {
            if (TryGetDefaultValue(attribute, out var value))
            {
                return value;
            }

            if (valueType == typeof(string))
            {
                return string.Empty;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
            {
                return null;
            }

            if (valueType.IsArray)
            {
                return Array.CreateInstance(elementType: valueType.GetElementType(), length: 0);
            }

            if (valueType.IsValueType
                || (valueType.IsAbstract == false && valueType.GetConstructor(Type.EmptyTypes) != null)
            )
            {
                return Activator.CreateInstance(valueType);
            }

            return null;
        }

        /// <summary>
        /// Gets the tooltip of a row that shows the default.
        /// </summary>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>
        /// "Default value:" and the shown default, then a source line: "From: Project Settings > ..." for a settings
        /// source, "From: Type ▸ Member" for another source, or "No [OverridableDefault]: uses the type's default
        /// value." without an attribute.
        /// </returns>
        public static string GetDefaultTooltip(OverridableDefaultAttribute attribute, Type valueType)
        {
            var value = GetShownDefault(attribute, valueType);
            var text = GetDefaultDisplayText(valueType, value);
            string source;

            if (attribute == null)
            {
                source = "No [OverridableDefault]: uses the type's default value.";
            }
            else if (IsSettingsType(attribute.SourceType))
            {
                var settings = attribute.SourceType.GetCustomAttribute<EncosyTower.Settings.SettingsAttribute>();
                var displayPath = settings?.DisplayPath ?? attribute.SourceType.Name;
                var memberName = ObjectNames.NicifyVariableName(attribute.MemberName);
                source = $"From: Project Settings > {displayPath.Replace("/", " > ")} > {memberName}";
            }
            else
            {
                source = $"From: {attribute.SourceType.Name} ▸ {attribute.MemberName}";
            }

            return $"Default value: {text}\n{source}";
        }

        /// <summary>
        /// Builds the context menu of a row.
        /// </summary>
        /// <remarks>
        /// The menu is Unity's standard property menu, reached by reflection, then a separator and "Locate Default
        /// Source". When Unity's method is missing or throws, the menu has only the last item.
        /// </remarks>
        /// <param name="property">The property of the row.</param>
        /// <param name="locate">The action of "Locate Default Source".</param>
        /// <param name="shiftPressed">Whether Shift was held when the menu opened.</param>
        /// <returns>The menu.</returns>
        public static GenericMenu BuildPropertyContextMenu(
              SerializedProperty property
            , GenericMenu.MenuFunction locate
            , bool shiftPressed = false
        )
            => BuildPropertyContextMenu(property, locate, s_fillPropertyContextMenu, shiftPressed);

        /// <summary>
        /// Shows where the default of a row comes from.
        /// </summary>
        /// <remarks>
        /// Without an attribute, a dialog explains that the row uses the type default. A settings source opens its
        /// Project Settings page and puts the member name in the search field; failures of that step are ignored.
        /// Another source opens the script that declares the member, at its line, or shows a dialog with the type and
        /// member name when no script is found.
        /// </remarks>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <param name="label">The label of the field, used in the dialog.</param>
        public static void Locate(OverridableDefaultAttribute attribute, Type valueType, string label)
        {
            if (attribute == null)
            {
                EditorUtility.DisplayDialog("Default Value", GetNoDefaultMessage(label, valueType), "OK");
                return;
            }

            if (IsSettingsType(attribute.SourceType))
            {
                LocateSettings(attribute);
                return;
            }

            if (TryFindSourceScript(attribute.SourceType, attribute.MemberName, out var script, out var line))
            {
                AssetDatabase.OpenAsset(script, line);
                return;
            }

            EditorUtility.DisplayDialog("Default Value", $"{attribute.SourceType.Name} ▸ {attribute.MemberName}", "OK");
        }

        /// <summary>
        /// Finds the script that declares a static member of a type.
        /// </summary>
        /// <remarks>
        /// The search takes the first of these: a <see cref="MonoScript"/> asset that is named like the type and
        /// whose class is the type, a script cached for the type, or a source file of the editor assembly with the
        /// assembly name of the type (every <see cref="MonoScript"/> when there is no such assembly) whose text
        /// declares the member; see <see cref="TryGetDeclaredMemberLine"/>.
        /// </remarks>
        /// <param name="sourceType">The type that declares the member.</param>
        /// <param name="memberName">The name of the static member.</param>
        /// <param name="script">Receives the script, or <see langword="null"/>.</param>
        /// <param name="line">
        /// Receives the line of the member, 1 when a script found by name does not declare it, or 0.
        /// </param>
        /// <returns><see langword="true"/> when a script was found.</returns>
        public static bool TryFindSourceScript(Type sourceType, string memberName, out MonoScript script, out int line)
        {
            var guids = AssetDatabase.FindAssets($"t:MonoScript {sourceType.Name}");
            var count = guids.Length;

            for (var i = 0; i < count; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var candidate = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (candidate.IsInvalid() || candidate.GetClass() != sourceType)
                {
                    continue;
                }

                script = candidate;
                line = GetMemberLine(path, memberName);
                return true;
            }

            if (s_sourceScripts.TryGetValue(sourceType, out var cached)
                && cached.IsValid()
                && TryGetDeclaredMemberLine(cached.text, sourceType, memberName, out line)
            )
            {
                script = cached;
                return true;
            }

            var paths = GetSourceScriptPaths(sourceType);
            count = paths.Length;

            for (var i = 0; i < count; i++)
            {
                var path = paths[i].Replace('\\', '/');

                if (Path.IsPathRooted(path))
                {
                    path = FileUtil.GetProjectRelativePath(path);
                }

                var candidate = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (candidate.IsInvalid()
                    || TryGetDeclaredMemberLine(candidate.text, sourceType, memberName, out line) == false
                )
                {
                    continue;
                }

                s_sourceScripts[sourceType] = candidate;
                script = candidate;
                return true;
            }

            script = null;
            line = 0;
            return false;
        }

        /// <summary>
        /// Finds the line where a source text declares a static member of a type.
        /// </summary>
        /// <remarks>
        /// The scan skips comments and string and character literals. It follows namespaces and nested types, so
        /// the member must be declared in exactly the type. A constructed generic type uses its definition.
        /// </remarks>
        /// <param name="text">The source text.</param>
        /// <param name="sourceType">The type that declares the member.</param>
        /// <param name="memberName">The name of the static member.</param>
        /// <param name="line">Receives the one-based line of the declaration, or 0.</param>
        /// <returns><see langword="true"/> when the declaration was found.</returns>
        public static bool TryGetDeclaredMemberLine(string text, Type sourceType, string memberName, out int line)
        {
            if (string.IsNullOrEmpty(text))
            {
                goto FAILED;
            }

            if (sourceType.IsConstructedGenericType)
            {
                sourceType = sourceType.GetGenericTypeDefinition();
            }

            var sourceNamespace = sourceType.Namespace ?? string.Empty;
            var typeName = Regex.Replace(sourceType.FullName ?? sourceType.Name, @"`\d+", string.Empty);

            if (sourceNamespace.Length > 0)
            {
                typeName = typeName[(sourceNamespace.Length + 1)..];
            }

            var simpleName = sourceType.Name;
            var arityIndex = simpleName.IndexOf('`');

            if (arityIndex >= 0)
            {
                simpleName = simpleName[..arityIndex];
            }

            if (text.IndexOf(simpleName, StringComparison.Ordinal) < 0
                || text.IndexOf(memberName, StringComparison.Ordinal) < 0
            )
            {
                goto FAILED;
            }

            var tokens = GetDeclarationTokens(text);

            if (TryFindDeclaredMember(tokens, sourceNamespace, typeName, memberName, out var position))
            {
                line = 1;

                for (var i = 0; i < position; i++)
                {
                    if (text[i] == '\n')
                    {
                        line++;
                    }
                }

                return true;
            }

        FAILED:
            line = 0;
            return false;
        }

        public static string GetNoDefaultMessage(string label, Type valueType)
        {
            var value = GetFriendlyDefaultText(valueType);

            return $"{label}: defaults to {value} (type default)\n"
                + $"To set a custom default, annotate {label} with [OverridableDefault].";
        }

        /// <summary>
        /// Turns the override on and seeds the value with the shown default.
        /// </summary>
        /// <remarks>
        /// The copy, the flag and the apply are collapsed into one Undo step.
        /// </remarks>
        /// <param name="property">The <see cref="Overridable{T}"/> property.</param>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="valueType">The type of the value.</param>
        public static void BeginOverride(
              SerializedProperty property
            , OverridableDefaultAttribute attribute
            , Type valueType
        )
        {
            var undoGroup = Undo.GetCurrentGroup();

            try
            {
                var value = property.FindPropertyRelative(VALUE);
                var shownDefault = GetShownDefault(attribute, valueType);
                SerializedValueAPI.Write(value, shownDefault, valueType);

                property.FindPropertyRelative(IS_OVERRIDDEN).boolValue = true;
                property.serializedObject.ApplyModifiedProperties();
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }
        }

        /// <summary>
        /// Reads the default that an attribute names, and reports a failure once.
        /// </summary>
        /// <remarks>
        /// A member that cannot be found or read is logged as a warning once for each source type and member name.
        /// Use the overload with an error when the failure must not be logged.
        /// </remarks>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="value">Receives the default, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when the default was read.</returns>
        public static bool TryGetDefaultValue(OverridableDefaultAttribute attribute, out object value)
        {
            var result = TryGetDefaultValue(attribute, out value, out var error);

            if (result == false && error.TryGetValue(out var failure))
            {
                ReportDefaultError(attribute, failure);
            }

            return result;
        }

        /// <summary>
        /// Reads the default that an attribute names, and returns the failure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A source type that derives from <c>Settings&lt;T&gt;</c> is read as an instance member of its settings
        /// asset; any other source type is read as a static member. The member is a field or a readable property
        /// without index parameters, public or non-public.
        /// </para>
        /// <para>
        /// Nothing is logged. An exception thrown by the member is returned as a failure, except an
        /// <see cref="OperationCanceledException"/>, which is not caught.
        /// </para>
        /// </remarks>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <param name="value">Receives the default, or <see langword="null"/>.</param>
        /// <param name="error">
        /// Receives the failure. It stays empty when the attribute is missing or incomplete, or when the settings
        /// asset is not valid.
        /// </param>
        /// <returns><see langword="true"/> when the default was read.</returns>
        public static bool TryGetDefaultValue(
              OverridableDefaultAttribute attribute
            , out object value
            , out Option<OverridableDefaultError> error
        )
        {
            value = null;
            error = default;

            if (attribute == null || attribute.SourceType == null || string.IsNullOrEmpty(attribute.MemberName))
            {
                return false;
            }

            var sourceType = attribute.SourceType;

            if (IsSettingsType(sourceType))
            {
                var instance = GetSettingsAsset(attribute);

                return instance.IsValid()
                    && TryReadMember(
                          sourceType
                        , instance
                        , attribute.MemberName
                        , BindingFlags.Instance
                        , out value
                        , out error
                    );
            }

            return TryReadMember(
                  type: sourceType
                , instance: null
                , memberName: attribute.MemberName
                , kind: BindingFlags.Static
                , value: out value
                , error: out error
            );
        }

        /// <summary>
        /// Gets the settings asset that supplies the default.
        /// </summary>
        /// <param name="attribute">The attribute of the field, or <see langword="null"/>.</param>
        /// <returns>
        /// The <c>Instance</c> of the source type when it derives from <c>Settings&lt;T&gt;</c>; otherwise
        /// <see langword="null"/>.
        /// </returns>
        public static UnityEngine.Object GetSettingsAsset(OverridableDefaultAttribute attribute)
        {
            if (attribute == null || attribute.SourceType == null || IsSettingsType(attribute.SourceType) == false)
            {
                return null;
            }

            return TryReadMember(
                  type: attribute.SourceType
                , instance: null
                , memberName: INSTANCE
                , kind: BindingFlags.Static
                , value: out var instance
                , error: out _
            ) ? instance as UnityEngine.Object : null;
        }

        /// <summary>
        /// Applies the selection of an enum dropdown.
        /// </summary>
        /// <remarks>
        /// Item 0 turns the override off and keeps the value. Item n turns it on with the nth enum value. For a type
        /// that is not an enum, only the flag changes.
        /// </remarks>
        /// <param name="property">The <see cref="Overridable{T}"/> property.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <param name="choiceIndex">The selected item, where 0 is the default item.</param>
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

            if (valueType.IsEnum)
            {
                var values = Enum.GetValues(valueType);
                value.longValue = Convert.ToInt64(values.GetValue(valueIndex));
            }

            isOverridden.boolValue = true;
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Turns the override off and keeps the stored value.
        /// </summary>
        /// <param name="property">The <see cref="Overridable{T}"/> property.</param>
        public static void EndOverride(SerializedProperty property)
        {
            property.FindPropertyRelative(IS_OVERRIDDEN).boolValue = false;
            property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Gets the selected item of an enum dropdown.
        /// </summary>
        /// <param name="property">The <see cref="Overridable{T}"/> property.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <returns>
        /// -1 for a mixed state or an overridden value that is not an enum value, 0 for the default item, and n + 1
        /// for the nth enum value.
        /// </returns>
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

            if (valueType.IsEnum)
            {
                var index = Array.IndexOf(Enum.GetValues(valueType), ReadValue(value, valueType));
                return index < 0 ? -1 : index + 1;
            }

            return 0;
        }

        /// <summary>
        /// Checks whether the selected objects differ.
        /// </summary>
        /// <param name="property">The <see cref="Overridable{T}"/> property.</param>
        /// <returns>
        /// <see langword="true"/> when the override flag differs, or when it is on and the value differs.
        /// </returns>
        public static bool IsMixed(SerializedProperty property)
            => IsMixed(property.FindPropertyRelative(IS_OVERRIDDEN), property.FindPropertyRelative(VALUE));

        /// <summary>
        /// Gets the value type of an <see cref="Overridable{T}"/> field type.
        /// </summary>
        /// <param name="fieldType">The type of the field, or of its array or list.</param>
        /// <returns>
        /// The <c>T</c> of <see cref="Overridable{T}"/> after the array or list is unwrapped; <c>object</c> when the
        /// type is not an <see cref="Overridable{T}"/>.
        /// </returns>
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

        /// <summary>
        /// Checks whether a property is an element of a collection that an <see cref="Overridable{T}"/> holds.
        /// </summary>
        /// <remarks>
        /// The collection is the value of an <see cref="Overridable{T}"/> and its elements are
        /// <see cref="Overridable{T}"/> too. These elements are drawn without an
        /// <see cref="OverridableDefaultAttribute"/>.
        /// </remarks>
        /// <param name="property">The property to check.</param>
        /// <param name="fieldInfo">The field that holds the outer <see cref="Overridable{T}"/>.</param>
        /// <returns><see langword="true"/> when the property is such an element.</returns>
        public static bool IsOverridableListElement(SerializedProperty property, FieldInfo fieldInfo)
        {
            if (fieldInfo == null || property.propertyPath.EndsWith("]", StringComparison.Ordinal) == false)
            {
                return false;
            }

            var fieldType = fieldInfo.FieldType;
            Type valueType;

            if (IsGenericOf(fieldType, typeof(Overridable<>)))
            {
                valueType = fieldType.GetGenericArguments()[0];
            }
            else if (fieldInfo.Name == VALUE && IsGenericOf(fieldInfo.DeclaringType, typeof(Overridable<>)))
            {
                valueType = fieldType;
            }
            else
            {
                return false;
            }

            Type elementType;

            if (valueType.IsArray)
            {
                elementType = valueType.GetElementType();
            }
            else if (IsGenericOf(valueType, typeof(List<>)))
            {
                elementType = valueType.GetGenericArguments()[0];
            }
            else
            {
                return false;
            }

            return IsGenericOf(elementType, typeof(Overridable<>));
        }

        /// <summary>
        /// Gets the value type that a row edits.
        /// </summary>
        /// <param name="fieldInfo">The field that holds the outer <see cref="Overridable{T}"/>.</param>
        /// <param name="property">The property of the row.</param>
        /// <returns>
        /// The <c>T</c> of the element for an element of a collection that an <see cref="Overridable{T}"/> holds;
        /// otherwise the result of <see cref="GetValueType(Type)"/> for the field type.
        /// </returns>
        public static Type GetValueType(FieldInfo fieldInfo, SerializedProperty property)
        {
            var fieldType = fieldInfo.FieldType;

            if (IsOverridableListElement(property, fieldInfo) && IsGenericOf(fieldType, typeof(Overridable<>)))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            return GetValueType(fieldType);
        }

        public static string GetElementNoDefaultMessage(string fieldLabel, Type valueType, int elementIndex)
        {
            var value = GetFriendlyDefaultText(valueType);

            return $"{fieldLabel} Element {elementIndex}: defaults to {value} (type default)\n"
                + $"Elements have no default of their own; [OverridableDefault] on {fieldLabel} sets the whole array.";
        }

        private static GenericMenu BuildPropertyContextMenu(
              SerializedProperty property
            , GenericMenu.MenuFunction locate
            , MethodInfo fillPropertyContextMenu
            , bool shiftPressed
        )
        {
            var menu = new GenericMenu();

            if (fillPropertyContextMenu != null)
            {
                try
                {
                    var arguments = fillPropertyContextMenu.GetParameters().Length == 6
                        ? new object[] { property, shiftPressed, true, null, menu, null }
                        : new object[] { property, null, menu };

                    menu = fillPropertyContextMenu.Invoke(obj: null, parameters: arguments) as GenericMenu ?? menu;
                }
                catch (Exception)
                {
                    menu = new GenericMenu();
                }
            }

            if (menu.GetItemCount() > 0)
            {
                menu.AddSeparator(string.Empty);
            }

            menu.AddItem(content: new GUIContent(LOCATE_DEFAULT_SOURCE), on: false, func: locate);
            return menu;
        }

        private static List<Group> GetDeclarationTokens(string text)
        {
            const string PATTERN = @"//[^\r\n]*|/\*[\s\S]*?\*/|@""(?:""""|[^""])*""|""(?:\\.|[^""\\])*"""
                + @"|'(?:\\.|[^'\\])*'|(?<token>@?[A-Za-z_]\w*|[{};.=(),])";

            var matches = Regex.Matches(text, PATTERN);
            var count = matches.Count;
            var tokens = new List<Group>(count);

            for (var i = 0; i < count; i++)
            {
                var token = matches[i].Groups["token"];

                if (token.Success)
                {
                    tokens.Add(token);
                }
            }

            return tokens;
        }

        private static bool TryFindDeclaredMember(
              List<Group> tokens
            , string sourceNamespace
            , string sourceType
            , string memberName
            , out int position
        )
        {
            var scopes = new Stack<(string Namespace, string Type, bool IsDeclaration)>();
            var currentNamespace = string.Empty;
            var currentType = string.Empty;
            var isDeclaration = true;
            var count = tokens.Count;

            for (var i = 0; i < count; i++)
            {
                var token = tokens[i].Value;

                if (token == "{")
                {
                    scopes.Push((currentNamespace, currentType, isDeclaration));
                    isDeclaration = false;
                    continue;
                }

                if (token == "}")
                {
                    if (scopes.Count > 0)
                    {
                        (currentNamespace, currentType, isDeclaration) = scopes.Pop();
                    }

                    continue;
                }

                if (isDeclaration == false)
                {
                    continue;
                }

                if (token == "namespace")
                {
                    var name = string.Empty;
                    var next = i + 1;

                    for (; next < count && tokens[next].Value is not ("{" or ";"); next++)
                    {
                        name += tokens[next].Value.TrimStart('@');
                    }

                    if (next >= count)
                    {
                        break;
                    }

                    if (tokens[next].Value == "{")
                    {
                        scopes.Push((currentNamespace, currentType, isDeclaration));
                    }

                    currentNamespace = currentNamespace.Length == 0 ? name : $"{currentNamespace}.{name}";
                    i = next;
                    continue;
                }

                if (token is "class" or "struct" or "interface" or "record")
                {
                    var next = i + 1;

                    if (token == "record" && next < count && tokens[next].Value is "class" or "struct")
                    {
                        next++;
                    }

                    if (next >= count)
                    {
                        break;
                    }

                    var name = tokens[next].Value.TrimStart('@');

                    next++;

                    while (next < count && tokens[next].Value is not ("{" or ";"))
                    {
                        next++;
                    }

                    if (next >= count || tokens[next].Value == ";")
                    {
                        i = next;
                        continue;
                    }

                    scopes.Push((currentNamespace, currentType, isDeclaration));
                    currentType = currentType.Length == 0 ? name : $"{currentType}+{name}";
                    i = next;
                    continue;
                }

                if (token != "static" || currentNamespace != sourceNamespace || currentType != sourceType)
                {
                    continue;
                }

                for (var next = i + 1; next < count; next++)
                {
                    var member = tokens[next].Value;

                    if (member is ";" or "{" or "}" or "(" or "=")
                    {
                        break;
                    }

                    if (member.TrimStart('@') == memberName
                        && next + 1 < count
                        && tokens[next + 1].Value is ";" or "{" or "(" or "=" or ","
                    )
                    {
                        position = tokens[i].Index;
                        return true;
                    }
                }
            }

            position = 0;
            return false;
        }

        private static void LocateSettings(OverridableDefaultAttribute attribute)
        {
            try
            {
                var settings = attribute.SourceType.GetCustomAttribute<EncosyTower.Settings.SettingsAttribute>();
                var displayPath = settings?.DisplayPath ?? attribute.SourceType.Name;
                var window = SettingsService.OpenProjectSettings($"Project/{displayPath}");
                var windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SettingsWindow");

                if (window.IsInvalid() || windowType == null || windowType.IsInstanceOfType(window) == false)
                {
                    return;
                }

                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var searchField = windowType.GetField("m_SearchText", flags);

                if (searchField == null)
                {
                    return;
                }

                searchField.SetValue(window, ObjectNames.NicifyVariableName(attribute.MemberName));
                windowType.GetMethod("HandleSearchFiltering", flags)?.Invoke(obj: window, parameters: null);
                window.Repaint();
            }
            catch
            {
                return;
            }
        }

        private static string[] GetSourceScriptPaths(Type sourceType)
        {
            var assemblyName = sourceType.Assembly.GetName().Name;
            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            var count = assemblies.Length;

            for (var i = 0; i < count; i++)
            {
                var assembly = assemblies[i];

                if (string.Equals(assembly.name, assemblyName, StringComparison.Ordinal))
                {
                    return assembly.sourceFiles;
                }
            }

            var guids = AssetDatabase.FindAssets("t:MonoScript");
            return Array.ConvertAll(guids, static guid => AssetDatabase.GUIDToAssetPath(guid));
        }

        private static int GetMemberLine(string path, string memberName)
        {
            try
            {
                var pattern = $@"\bstatic\b.*\b{Regex.Escape(memberName)}\b";
                var line = 0;

                foreach (var text in File.ReadLines(path))
                {
                    line++;

                    if (Regex.IsMatch(text, pattern))
                    {
                        return line;
                    }
                }
            }
            catch (IOException)
            {
                return 1;
            }
            catch (UnauthorizedAccessException)
            {
                return 1;
            }

            return 1;
        }

        private static bool IsMixed(SerializedProperty isOverridden, SerializedProperty value)
            => isOverridden.hasMultipleDifferentValues || (isOverridden.boolValue && value.hasMultipleDifferentValues);

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

        /// <summary>
        /// Logs a warning for a default that cannot be read, once for each source type and member name.
        /// </summary>
        private static void ReportDefaultError(OverridableDefaultAttribute attribute, in OverridableDefaultError error)
        {
            if (s_reportedDefaultErrors.Add((attribute.SourceType, attribute.MemberName)))
            {
                ThrowHelper.LogWarningOverridableDefault(error);
            }
        }

        /// <summary>
        /// Reads a field or a property of a default source.
        /// </summary>
        /// <remarks>
        /// A field is tried first, then a readable property without index parameters. The returned failure is
        /// <c>MemberNotFound</c> when neither exists, or <c>ReadFailure</c> when reading throws. An
        /// <see cref="OperationCanceledException"/>, also as an inner exception, is not caught.
        /// </remarks>
        private static bool TryReadMember(
              Type type
            , object instance
            , string memberName
            , BindingFlags kind
            , out object value
            , out Option<OverridableDefaultError> error
        )
        {
            try
            {
                var flags = kind | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                var field = type.GetField(memberName, flags);

                if (field != null)
                {
                    value = field.GetValue(instance);
                    error = default;
                    return true;
                }

                var property = type.GetProperty(memberName, flags);

                if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
                {
                    value = property.GetValue(instance);
                    error = default;
                    return true;
                }
            }
            catch (Exception exception) when (exception is OperationCanceledException == false
                && exception.InnerException is OperationCanceledException == false
            )
            {
                value = null;

                error = Option.Some<OverridableDefaultError>(
                    new OverridableDefaultError.ReadFailure(type, memberName, exception.GetBaseException())
                );

                return false;
            }

            value = null;

            error = Option.Some<OverridableDefaultError>(
                new OverridableDefaultError.MemberNotFound(type, memberName, (kind & BindingFlags.Instance) != 0)
            );

            return false;
        }

        private static object ReadValue(SerializedProperty value, Type valueType)
            => SerializedValueAPI.Read(value, valueType);

        private static string GetFriendlyDefaultText(Type valueType)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType)
                || (valueType.IsEnum && valueType.IsDefined(typeof(FlagsAttribute), inherit: false))
            )
            {
                return "None";
            }

            if (valueType == typeof(string))
            {
                return "empty text";
            }

            if (valueType == typeof(Vector3))
            {
                return "(0, 0, 0)";
            }

            if (valueType == typeof(Color))
            {
                return "transparent black";
            }

            if (IsDropdownEnum(valueType))
            {
                var values = Enum.GetValues(valueType);

                if (values.Length > 0)
                {
                    return GetDisplayText(valueType, values.GetValue(0));
                }
            }

            return GetDisplayText(valueType, GetShownDefault(attribute: null, valueType: valueType));
        }

        private static string GetDefaultDisplayText(Type valueType, object value)
        {
            if (valueType == typeof(string))
            {
                return $"\"{value}\"";
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
            {
                return value is UnityEngine.Object unityObject && unityObject.IsValid()
                    ? $"{unityObject.name} ({unityObject.GetType().Name})"
                    : "None";
            }

            if (value is AnimationCurve or Gradient)
            {
                return GetDisplayText(valueType, value);
            }

            if (IsGenericOf(valueType, typeof(ExposedReference<>)))
            {
                var reference = valueType.GetField(nameof(ExposedReference<UnityEngine.Object>.defaultValue));
                return GetDefaultDisplayText(valueType.GetGenericArguments()[0], reference.GetValue(value));
            }

            if (valueType.IsClass && valueType.IsArray == false && typeof(IList).IsAssignableFrom(valueType) == false)
            {
                return valueType.Name;
            }

            return GetDisplayText(valueType, value);
        }

    }
}

#endif
