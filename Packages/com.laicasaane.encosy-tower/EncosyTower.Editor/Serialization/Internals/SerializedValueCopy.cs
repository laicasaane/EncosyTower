#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal static class SerializedValueCopy
    {
        private const string ARRAY_SIZE_PATH = "Array.size";

        private static readonly Dictionary<Type, bool> s_requiresPropertyCopy = new();

        public static object Read(SerializedProperty property, Type valueType)
        {
            if (valueType.IsEnum)
            {
                return Enum.ToObject(valueType, property.longValue);
            }

            if (property.propertyType is SerializedPropertyType.ManagedReference
                or SerializedPropertyType.ExposedReference
                || RequiresPropertyCopy(valueType))
            {
                return ReadThroughHolder(property, valueType);
            }

            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                var count = property.arraySize;
                var elementType = GetElementType(valueType);

                var result = valueType.IsArray
                    ? (IList)Array.CreateInstance(elementType, count)
                    : (IList)Activator.CreateInstance(valueType);

                for (var i = 0; i < count; i++)
                {
                    using var element = property.GetArrayElementAtIndex(i);
                    var value = Read(element, elementType);

                    if (valueType.IsArray)
                    {
                        result[i] = value;
                    }
                    else
                    {
                        result.Add(value);
                    }
                }

                return result;
            }

            return valueType == typeof(char) ? (char)property.uintValue : property.boxedValue;
        }

        public static void Write(SerializedProperty property, object value, Type valueType = null)
        {
            var recordUndo = property.serializedObject.targetObject is OverridableDefaultHolder == false;
            var undoGroup = recordUndo ? Undo.GetCurrentGroup() : default;

            try
            {
                WriteCore(property, value, valueType);
            }
            finally
            {
                if (recordUndo)
                {
                    Undo.CollapseUndoOperations(undoGroup);
                }
            }
        }

        private static void WriteCore(SerializedProperty property, object value, Type valueType)
        {
            valueType ??= value?.GetType();
            var isCollection = property.isArray && property.propertyType != SerializedPropertyType.String;

            if (isCollection && value is IList == false)
            {
                property.arraySize = 0;
                return;
            }

            if (property.propertyType is SerializedPropertyType.ManagedReference
                or SerializedPropertyType.ExposedReference
                || RequiresPropertyCopy(valueType))
            {
                WriteThroughHolder(property, value, valueType);
                return;
            }

            if (isCollection)
            {
                var list = (IList)value;
                var count = list.Count;
                property.arraySize = count;

                for (var i = 0; i < count; i++)
                {
                    using var element = property.GetArrayElementAtIndex(i);
                    Write(element, list[i]);
                }
            }
            else if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = value as UnityEngine.Object;
            }
            else
            {
                property.boxedValue = value;
            }
        }

        private static object ReadThroughHolder(SerializedProperty property, Type valueType)
        {
            var holder = OverridableDefaultHolder.Create(valueType);

            try
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    holder.Box = new ManagedReferenceBox();
                }

                using var destination = holder.GetValueProperty();
                var context = new CopyContext(destination.serializedObject, property.serializedObject);
                Copy(property, destination.propertyPath, context);
                destination.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                var box = holder.Box;
                return box.GetType().GetField(OverridableEditorAPI.VALUE).GetValue(box);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        private static void WriteThroughHolder(SerializedProperty property, object value, Type valueType)
        {
            var source = OverridableDefaultHolder.Create(valueType ?? typeof(object));
            OverridableDefaultHolder copy = null;

            try
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    source.Box = new ManagedReferenceBox { value = value };
                }
                else
                {
                    var box = source.Box;
                    box.GetType().GetField(OverridableEditorAPI.VALUE).SetValue(box, value);
                }

                copy = OverridableDefaultHolder.Create(valueType ?? typeof(object));

                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    copy.Box = new ManagedReferenceBox();
                }

                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(source), copy);

                using var originalValue = source.GetValueProperty();
                using var copiedValue = copy.GetValueProperty();
                var context = new CopyContext(property.serializedObject, originalValue.serializedObject);
                Copy(copiedValue, property.propertyPath, context);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(copy);
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        private static void Copy(SerializedProperty source, string destinationPath, CopyContext context)
        {
            if (source.isFixedBuffer)
            {
                using var destination = context.Destination.FindProperty(destinationPath);
                var count = source.fixedBufferSize;

                for (var i = 0; i < count; i++)
                {
                    using var sourceElement = source.GetFixedBufferElementAtIndex(i);
                    using var destinationElement = destination.GetFixedBufferElementAtIndex(i);
                    CopyFixedBufferElement(sourceElement, destinationElement);
                }

                return;
            }

            if (source.isArray && source.propertyType != SerializedPropertyType.String)
            {
                var count = source.arraySize;
                bool resize;

                using (var destination = context.Destination.FindProperty(destinationPath))
                {
                    using var size = destination.FindPropertyRelative(ARRAY_SIZE_PATH);
                    resize = destination.arraySize != count || size.hasMultipleDifferentValues;
                }

                if (resize)
                {
                    context.Materialize();

                    using (var destination = context.Destination.FindProperty(destinationPath))
                    {
                        destination.arraySize = count;
                    }

                    context.Materialize();
                }

                for (var i = 0; i < count; i++)
                {
                    using var sourceElement = source.GetArrayElementAtIndex(i);
                    using var destination = context.Destination.FindProperty(destinationPath);
                    using var destinationElement = destination.GetArrayElementAtIndex(i);
                    Copy(sourceElement, destinationElement.propertyPath, context);
                }

                return;
            }

            switch (source.propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ExposedReference:
                {
                    CopyChildren(source, destinationPath, context);
                    return;
                }

                case SerializedPropertyType.ManagedReference:
                {
                    CopyManagedReference(source, destinationPath, context);
                    return;
                }

                case SerializedPropertyType.ObjectReference:
                {
                    using var destination = context.Destination.FindProperty(destinationPath);
                    using var original = context.ObjectReferenceSource.FindProperty(source.propertyPath);
                    destination.objectReferenceValue = original.objectReferenceValue;
                    return;
                }

                default:
                {
                    using var destination = context.Destination.FindProperty(destinationPath);
                    destination.boxedValue = source.boxedValue;
                    return;
                }
            }
        }

        private static void CopyFixedBufferElement(SerializedProperty source, SerializedProperty destination)
        {
            switch (source.numericType)
            {
                case SerializedPropertyNumericType.UInt64:
                {
                    destination.ulongValue = source.ulongValue;
                    return;
                }

                case SerializedPropertyNumericType.Int64:
                {
                    destination.longValue = source.longValue;
                    return;
                }

                case SerializedPropertyNumericType.UInt8:
                case SerializedPropertyNumericType.UInt16:
                case SerializedPropertyNumericType.UInt32:
                {
                    destination.uintValue = source.uintValue;
                    return;
                }

                case SerializedPropertyNumericType.Float:
                {
                    destination.floatValue = source.floatValue;
                    return;
                }

                case SerializedPropertyNumericType.Double:
                {
                    destination.doubleValue = source.doubleValue;
                    return;
                }

                default:
                {
                    switch (source.propertyType)
                    {
                        case SerializedPropertyType.Boolean:
                        {
                            destination.boolValue = source.boolValue;
                            return;
                        }

                        case SerializedPropertyType.Character:
                        {
                            destination.uintValue = source.uintValue;
                            return;
                        }

                        case SerializedPropertyType.Float:
                        {
                            destination.floatValue = source.floatValue;
                            return;
                        }

                        default:
                        {
                            destination.intValue = source.intValue;
                            return;
                        }
                    }
                }
            }
        }

        private static void CopyChildren(SerializedProperty source, string destinationPath, CopyContext context)
        {
            using var child = source.Copy();
            var depth = source.depth;
            var prefixLength = source.propertyPath.Length;
            var enterChildren = true;

            while (child.Next(enterChildren) && child.depth > depth)
            {
                var childPath = destinationPath + child.propertyPath[prefixLength..];
                Copy(child, childPath, context);
                enterChildren = false;
            }
        }

        private static void CopyManagedReference(SerializedProperty source, string destinationPath, CopyContext context)
        {
            var value = source.managedReferenceValue;
            context.Materialize();

            if (value == null)
            {
                using var destination = context.Destination.FindProperty(destinationPath);
                destination.managedReferenceValue = null;
                return;
            }

            var id = source.managedReferenceId;

            if (context.References.TryGetValue(id, out var existingPath))
            {
                using var existing = context.Destination.FindProperty(existingPath);
                using var destination = context.Destination.FindProperty(destinationPath);
                destination.managedReferenceValue = existing.managedReferenceValue;
                return;
            }

            using (var destination = context.Destination.FindProperty(destinationPath))
            {
                destination.managedReferenceValue = FormatterServices.GetUninitializedObject(value.GetType());
            }

            context.References.Add(id, destinationPath);
            context.Materialize();
            CopyChildren(source, destinationPath, context);
        }

        private static Type GetElementType(Type type)
            => type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

        private static bool RequiresPropertyCopy(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (s_requiresPropertyCopy.TryGetValue(type, out var result) == false)
            {
                result = RequiresPropertyCopy(type, new HashSet<Type>());
                s_requiresPropertyCopy.Add(type, result);
            }

            return result;
        }

        private static bool RequiresPropertyCopy(Type type, HashSet<Type> visited)
        {
            if (type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || typeof(UnityEngine.Object).IsAssignableFrom(type)
                || visited.Add(type) == false
            )
            {
                return false;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ExposedReference<>))
            {
                return true;
            }

            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                return RequiresPropertyCopy(GetElementType(type), visited);
            }

            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                var fields = current.GetFields(FLAGS);
                var count = fields.Length;

                for (var i = 0; i < count; i++)
                {
                    var field = fields[i];

                    if (field.IsInitOnly || field.IsDefined(typeof(NonSerializedAttribute), inherit: false))
                    {
                        continue;
                    }

                    var managedReference = field.IsDefined(typeof(SerializeReference), inherit: false);

                    if (field.IsPublic == false
                        && managedReference == false
                        && field.IsDefined(typeof(SerializeField), inherit: false) == false
                    )
                    {
                        continue;
                    }

                    if (managedReference
                        || field.IsDefined(typeof(FixedBufferAttribute), inherit: false)
                        || RequiresPropertyCopy(field.FieldType, visited)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private sealed class CopyContext
        {
            private readonly bool _recordUndo;
            private bool _hasUndoSnapshot;

            public CopyContext(SerializedObject destination, SerializedObject objectReferenceSource)
            {
                Destination = destination;
                ObjectReferenceSource = objectReferenceSource;
                _recordUndo = destination.targetObject is OverridableDefaultHolder == false;
            }

            public SerializedObject Destination { get; }

            public SerializedObject ObjectReferenceSource { get; }

            public Dictionary<long, string> References { get; } = new();

            public void Materialize()
            {
                if (Destination.hasModifiedProperties == false)
                {
                    return;
                }

                if (_recordUndo && _hasUndoSnapshot == false)
                {
                    Undo.RegisterCompleteObjectUndo(Destination.targetObjects, Undo.GetCurrentGroupName());
                    _hasUndoSnapshot = true;
                }

                Destination.ApplyModifiedPropertiesWithoutUndo();
                Destination.Update();
            }
        }

        [Serializable]
        private sealed class ManagedReferenceBox
        {
            [SerializeReference]
            public object value;
        }
    }
}

#endif
