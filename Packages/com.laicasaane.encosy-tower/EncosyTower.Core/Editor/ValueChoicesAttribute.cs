using System;
using System.Diagnostics;
using UnityEngine;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Editor
{
    /// <summary>
    /// Provides choices for fields, including auto-properties annotated with <c>[field: ValueChoices(...)]</c>.
    /// </summary>
    /// <remarks>
    /// The member must be a static field, property or parameterless method. To offer Project Settings values,
    /// write a static member that reads <c>Settings&lt;T&gt;.Instance</c>.
    /// <c>IsExclusive = false</c> keeps free input and adds the choices to a menu;
    /// <c>IsExclusive = true</c> restricts the field to the choices.
    /// Non-flags enums are drawn as a dropdown: <c>IsExclusive = false</c> lists every enum value,
    /// while <c>IsExclusive = true</c> lists only the choices.
    /// <c>applyToCollection = true</c> selects non-exclusive presets for the whole collection;
    /// the default <c>false</c> applies choices to each element.
    /// A source type name may be a full name or an assembly-qualified name and is resolved in the Editor.
    /// </remarks>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ValueChoicesAttribute : PropertyAttribute
    {
        /// <summary>
        /// Uses choices declared on the type containing the annotated field.
        /// </summary>
        /// <param name="memberName">The static member providing the choices.</param>
        /// <param name="applyToCollection">Whether choices replace the collection or individual elements.</param>
        public ValueChoicesAttribute(string memberName, bool applyToCollection = false) : base(applyToCollection)
        {
            DebuggingThrowHelper.ThrowIfNullOrEmpty(memberName);
            MemberName = memberName;
        }

        /// <summary>
        /// Uses choices declared on another type.
        /// </summary>
        /// <param name="sourceType">The type providing the choices.</param>
        /// <param name="memberName">The static member providing the choices.</param>
        /// <param name="applyToCollection">Whether choices replace the collection or individual elements.</param>
        public ValueChoicesAttribute(Type sourceType, string memberName, bool applyToCollection = false)
            : base(applyToCollection)
        {
            DebuggingThrowHelper.ThrowIfNull(sourceType);
            DebuggingThrowHelper.ThrowIfNullOrEmpty(memberName);
            SourceType = sourceType;
            MemberName = memberName;
        }

        /// <summary>
        /// Uses choices declared on a type resolved by name in the Editor.
        /// </summary>
        /// <param name="sourceTypeName">The full or assembly-qualified name of the type providing the choices.</param>
        /// <param name="memberName">The static member providing the choices.</param>
        /// <param name="applyToCollection">Whether choices replace the collection or individual elements.</param>
        public ValueChoicesAttribute(string sourceTypeName, string memberName, bool applyToCollection = false)
            : base(applyToCollection)
        {
            DebuggingThrowHelper.ThrowIfNullOrEmpty(sourceTypeName);
            DebuggingThrowHelper.ThrowIfNullOrEmpty(memberName);
            SourceTypeName = sourceTypeName;
            MemberName = memberName;
        }

        /// <summary>
        /// The explicit source type, or <c>null</c> when the source is implicit or named by string.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// The source type name to resolve, or <c>null</c> when no string source was supplied.
        /// </summary>
        public string SourceTypeName { get; }

        /// <summary>
        /// The static field, property or parameterless method providing the choices.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Whether the field accepts only the supplied choices. The default allows custom values.
        /// </summary>
        public bool IsExclusive { get; set; }
    }
}
