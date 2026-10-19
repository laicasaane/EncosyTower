#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor
{
    /// <summary>
    /// A hidden, temporary object that hosts a value so that a <see cref="SerializedProperty"/> can show it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A row uses it to show the default read-only. When the target object of a property is a holder,
    /// a row that reads it treats the property as locked and
    /// <see cref="SerializedValueAPI"/> records no Undo for it.
    /// </para>
    /// <para>
    /// The object has <see cref="HideFlags.DontSave"/>. Its creator owns it and must call
    /// <see cref="UnityEngine.Object.DestroyImmediate(UnityEngine.Object)"/>; the rows do so when they detach or
    /// are cleared.
    /// </para>
    /// </remarks>
    public sealed class SerializedValueHolder : ScriptableObject
    {
        [SerializeReference]
        private object _box;

        public const string VALUE = nameof(SerializedValueBox<int>.value);

        private const string VALUE_PATH = nameof(_box) + "." + nameof(SerializedValueBox<int>.value);

        private Type _valueType;
        private SerializedObject _serializedObject;

        /// <summary>
        /// The <see cref="SerializedValueBox{T}"/> held by the serialized field.
        /// </summary>
        /// <remarks>
        /// <see cref="SerializedValueAPI"/> replaces it with a managed-reference box for a managed reference.
        /// </remarks>
        public object Box
        {
            get => _box;
            set => _box = value;
        }

        /// <summary>
        /// Creates a holder for a value type.
        /// </summary>
        /// <param name="valueType">The type of the hosted value.</param>
        /// <returns>
        /// A new holder with a default-constructed <see cref="SerializedValueBox{T}"/>. The caller owns it.
        /// </returns>
        public static SerializedValueHolder Create(Type valueType)
        {
            var boxType = typeof(SerializedValueBox<>).MakeGenericType(valueType);
            var box = Activator.CreateInstance(boxType);
            var holder = CreateInstance<SerializedValueHolder>();
            holder.hideFlags = HideFlags.DontSave;
            holder._box = box;
            holder._valueType = valueType;
            return holder;
        }

        /// <summary>
        /// Writes a value into the holder.
        /// </summary>
        /// <remarks>
        /// The write is applied without Undo.
        /// </remarks>
        /// <param name="value">The value to host.</param>
        /// <param name="expandedSource">
        /// A property whose expanded state is copied to the result, or <see langword="null"/>.
        /// </param>
        /// <returns>The value property of the holder.</returns>
        public SerializedProperty Write(object value, SerializedProperty expandedSource)
        {
            var property = GetValueProperty();
            SerializedValueAPI.Write(property, value, _valueType);

            if (expandedSource != null)
            {
                property.isExpanded = expandedSource.isExpanded;
            }

            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return property;
        }

        /// <summary>
        /// Gets the value property of the holder.
        /// </summary>
        /// <returns>
        /// The property of <see cref="SerializedValueBox{T}.value"/>, read from a <see cref="SerializedObject"/>
        /// that the holder creates on first use and updates on every call.
        /// </returns>
        public SerializedProperty GetValueProperty()
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

    /// <summary>
    /// The serialized wrapper of a hosted value.
    /// </summary>
    /// <typeparam name="T">The type of the hosted value.</typeparam>
    [Serializable]
    public sealed class SerializedValueBox<T>
    {
        /// <summary>
        /// The hosted value. A holder property path ends with this field name.
        /// </summary>
        public T value = default;
    }
}

#endif
