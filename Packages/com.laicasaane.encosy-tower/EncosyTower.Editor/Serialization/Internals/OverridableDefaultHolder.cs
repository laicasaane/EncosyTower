#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal sealed class OverridableDefaultHolder : ScriptableObject
    {
        [SerializeReference]
        private object _overridableDefault;

        private const string VALUE_PATH = nameof(_overridableDefault) + "." + nameof(OverridableDefaultBox<int>.value);

        private Type _valueType;
        private SerializedObject _serializedObject;

        internal object Box
        {
            get => _overridableDefault;
            set => _overridableDefault = value;
        }

        public static OverridableDefaultHolder Create(Type valueType)
        {
            var boxType = typeof(OverridableDefaultBox<>).MakeGenericType(valueType);
            var box = Activator.CreateInstance(boxType);
            var holder = CreateInstance<OverridableDefaultHolder>();
            holder.hideFlags = HideFlags.DontSave;
            holder._overridableDefault = box;
            holder._valueType = valueType;
            return holder;
        }

        public SerializedProperty Write(object value, SerializedProperty expandedSource)
        {
            var property = GetValueProperty();
            SerializedValueCopy.Write(property, value, _valueType);

            if (expandedSource != null)
            {
                property.isExpanded = expandedSource.isExpanded;
            }

            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return property;
        }

        internal SerializedProperty GetValueProperty()
        {
            _serializedObject ??= new SerializedObject(this);
            _serializedObject.Update();
            return _serializedObject.FindProperty(VALUE_PATH);
        }

        private void OnDisable()
        {
            _serializedObject?.Dispose();
            _serializedObject = null;
        }
    }

    [Serializable]
    internal sealed class OverridableDefaultBox<T>
    {
        public T value = default;
    }
}

#endif
