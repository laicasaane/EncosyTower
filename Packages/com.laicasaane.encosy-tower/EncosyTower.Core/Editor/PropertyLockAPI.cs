#if UNITY_EDITOR

using System;
using EncosyTower.Serialization;
using UnityEditor;

namespace EncosyTower.Editor
{
    /// <summary>
    /// Tells whether a serialized property is read-only because it only shows a default.
    /// </summary>
    public static class PropertyLockAPI
    {
        private const string VALUE = nameof(Overridable<int>.value);
        private const string IS_OVERRIDDEN = nameof(Overridable<int>.isOverridden);

        /// <summary>
        /// Checks whether a property is read-only because it only shows a default.
        /// </summary>
        /// <remarks>
        /// A locked row has no tooltip or menu. A property is locked when its object is an
        /// <see cref="SerializedValueHolder"/>, or when an <see cref="Overridable{T}"/> that contains it is not
        /// overridden and has no mixed state.
        /// </remarks>
        /// <param name="property">The property to check.</param>
        /// <returns><see langword="true"/> when the property is locked.</returns>
        public static bool IsLocked(SerializedProperty property)
        {
            if (property.serializedObject.targetObject is SerializedValueHolder)
            {
                return true;
            }

            const string VALUE_SEGMENT = "." + VALUE + ".";
            var path = property.propertyPath;
            var index = path.LastIndexOf(VALUE_SEGMENT, StringComparison.Ordinal);

            while (index >= 0)
            {
                path = path[..index];
                using var parent = property.serializedObject.FindProperty(path);
                using var overridden = parent?.FindPropertyRelative(IS_OVERRIDDEN);

                if (overridden != null
                    && overridden.propertyType == SerializedPropertyType.Boolean
                    && overridden.hasMultipleDifferentValues == false
                    && overridden.boolValue == false
                )
                {
                    return true;
                }

                index = path.LastIndexOf(VALUE_SEGMENT, StringComparison.Ordinal);
            }

            return false;
        }
    }
}

#endif
