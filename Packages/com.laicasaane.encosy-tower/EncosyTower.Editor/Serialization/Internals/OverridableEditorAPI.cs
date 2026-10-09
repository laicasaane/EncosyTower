#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal static class OverridableEditorAPI
    {
        public const string VALUE = nameof(Overridable<int>.value);
        public const string IS_OVERRIDDEN = nameof(Overridable<int>.isOverridden);
        public const string PROJECT_SETTINGS_LABEL = "Project Settings";
        public const string DEFAULT_LABEL = "Default";
        public const string MIXED_TEXT = "Mixed";

        private const string INSTANCE = "Instance";
        private const string LOCATE_DEFAULT_SOURCE = "Locate Default Source";

        private static readonly Dictionary<Type, MonoScript> s_sourceScripts = new();

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

        public static OverridableMode GetMode(Type valueType)
            => IsDropdownEnum(valueType)
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

        public static string GetDefaultChoiceLabel(OverridableDefaultAttribute attribute, Type valueType)
        {
            var label = GetDefaultLabel(attribute);

            return TryGetDefaultValue(attribute, out var value)
                ? $"{label} ({GetDisplayText(valueType, value)})"
                : label;
        }

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

        public static GenericMenu BuildPropertyContextMenu(
              SerializedProperty property
            , GenericMenu.MenuFunction locate
            , bool shiftPressed = false
        )
            => BuildPropertyContextMenu(property, locate, s_fillPropertyContextMenu, shiftPressed);

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
                SerializedValueCopy.Write(value, shownDefault, valueType);

                property.FindPropertyRelative(IS_OVERRIDDEN).boolValue = true;
                property.serializedObject.ApplyModifiedProperties();
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }
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

            return TryReadMember(
                  type: sourceType
                , instance: null
                , memberName: attribute.MemberName
                , kind: BindingFlags.Static
                , value: out value
            );
        }

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
            ) ? instance as UnityEngine.Object : null;
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

            if (valueType.IsEnum)
            {
                var values = Enum.GetValues(valueType);
                value.longValue = Convert.ToInt64(values.GetValue(valueIndex));
            }

            isOverridden.boolValue = true;
            property.serializedObject.ApplyModifiedProperties();
        }

        public static void EndOverride(SerializedProperty property)
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
                    : DEFAULT_LABEL;
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
            try
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
            }
            catch (Exception exception) when (exception is OperationCanceledException == false
                && exception.InnerException is OperationCanceledException == false
            )
            {
                value = null;
                return false;
            }

            value = null;
            return false;
        }

        private static object ReadValue(SerializedProperty value, Type valueType)
            => SerializedValueCopy.Read(value, valueType);

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetCurveDisplayText(AnimationCurve curve)
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
        private static string GetGradientDisplayText(Gradient gradient)
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
