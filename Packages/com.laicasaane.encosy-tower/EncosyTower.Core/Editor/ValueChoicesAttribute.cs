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
    /// <para>
    /// The member must be a static field, property or parameterless method. To offer Project Settings values,
    /// write a static member that reads <c>Settings&lt;T&gt;.Instance</c>.
    /// The member may be non-public. A property or method is evaluated when the Editor resolves the choices.
    /// </para>
    /// <para>
    /// The member type must be an enumerable of the field value type, of <see cref="ValueChoice{T}"/> of the
    /// field value type, or of <see cref="ValueChoice"/>. A choice without a label gets one from the Editor;
    /// see <see cref="ValueChoice{T}"/>.
    /// </para>
    /// <para>
    /// <c>IsExclusive = false</c> keeps free input and adds the choices to a menu;
    /// <c>IsExclusive = true</c> restricts the field to the choices and draws a dropdown.
    /// An exclusive field whose stored value is not a choice shows a warning icon.
    /// Non-flags enums are drawn as a dropdown: <c>IsExclusive = false</c> lists every enum value,
    /// while <c>IsExclusive = true</c> lists only the choices.
    /// </para>
    /// <para>
    /// <c>applyToCollection = true</c> selects non-exclusive presets for the whole collection;
    /// the default <c>false</c> applies choices to each element. With the default, an array or
    /// <c>List&lt;T&gt;</c> field is drawn by Unity and this attribute is applied to each element.
    /// </para>
    /// <para>
    /// A source type name may be a full name or an assembly-qualified name and is resolved in the Editor.
    /// </para>
    /// <para>
    /// On an <see cref="EncosyTower.Serialization.Overridable{T}"/> field, the field value type is the
    /// <c>T</c> of the <see cref="EncosyTower.Serialization.Overridable{T}"/>. Picking a choice sets the value and
    /// overrides the default source.
    /// </para>
    /// <para>
    /// A configuration the Editor cannot resolve, such as a missing member or a choice of the wrong type,
    /// is logged as a warning in the Console, and the field is drawn normally.
    /// Uses of this attribute are omitted from player builds.
    /// </para>
    /// </remarks>
    /// <example>
    /// A string field with free input and a menu of presets:
    /// <code>
    /// [ValueChoices(nameof(Names))]
    /// public string displayName;
    ///
    /// private static string[] Names =&gt; new[] { "Grunt", "Boss" };
    /// </code>
    /// An enum field restricted to a subset, with labels:
    /// <code>
    /// [ValueChoices(nameof(Allowed), IsExclusive = true)]
    /// public Difficulty difficulty;
    ///
    /// private static ValueChoice&lt;Difficulty&gt;[] Allowed =&gt; new ValueChoice&lt;Difficulty&gt;[] {
    ///     new("Easy", Difficulty.Easy),
    ///     new("Hard", Difficulty.Hard),
    /// };
    /// </code>
    /// A list field with presets that replace the whole list:
    /// <code>
    /// [ValueChoices(nameof(WavePresets), applyToCollection: true)]
    /// public List&lt;int&gt; waves;
    ///
    /// private static ValueChoice&lt;List&lt;int&gt;&gt;[] WavePresets =&gt; new ValueChoice&lt;List&lt;int&gt;&gt;[] {
    ///     new("Rush", new() { 2, 4, 8 }),
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="ValueChoice{T}"/>
    /// <seealso cref="ValueChoice"/>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ValueChoicesAttribute : PropertyAttribute
    {
        /// <summary>
        /// Uses choices declared on the type containing the annotated field.
        /// </summary>
        /// <param name="memberName">The static member providing the choices.</param>
        /// <param name="applyToCollection">
        /// <c>true</c>: the drawer applies to the whole array or <c>List&lt;T&gt;</c> field; each choice is a complete
        /// collection and picking one replaces the whole collection (requires <c>IsExclusive = false</c>).
        /// <c>false</c>: the drawer applies to each element and each choice is one element value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="memberName"/> is null or empty.
        /// </exception>
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
        /// <param name="applyToCollection">
        /// <c>true</c>: the drawer applies to the whole array or <c>List&lt;T&gt;</c> field; each choice is a complete
        /// collection and picking one replaces the whole collection (requires <c>IsExclusive = false</c>).
        /// <c>false</c>: the drawer applies to each element and each choice is one element value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="sourceType"/> is null, or <paramref name="memberName"/> is null or empty.
        /// </exception>
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
        /// <remarks>
        /// A call with two strings binds to this overload, not to the one that takes a member name and
        /// <paramref name="applyToCollection"/>.
        /// </remarks>
        /// <param name="sourceTypeName">The full or assembly-qualified name of the type providing the choices.</param>
        /// <param name="memberName">The static member providing the choices.</param>
        /// <param name="applyToCollection">
        /// <c>true</c>: the drawer applies to the whole array or <c>List&lt;T&gt;</c> field; each choice is a complete
        /// collection and picking one replaces the whole collection (requires <c>IsExclusive = false</c>).
        /// <c>false</c>: the drawer applies to each element and each choice is one element value.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="sourceTypeName"/> or <paramref name="memberName"/> is null or empty.
        /// </exception>
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
        /// <remarks>
        /// When it is <c>null</c> and no source type name is set, the type that declares the field is the source.
        /// </remarks>
        public Type SourceType { get; }

        /// <summary>
        /// The source type name to resolve, or <c>null</c> when no string source was supplied.
        /// </summary>
        /// <remarks>
        /// The Editor scans the loaded assemblies for the name; a name that matches two different types is an error.
        /// An assembly-qualified name limits the scan to that assembly.
        /// </remarks>
        public string SourceTypeName { get; }

        /// <summary>
        /// The static field, property or parameterless method providing the choices.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Whether the field accepts only the supplied choices. The default allows custom values.
        /// </summary>
        /// <remarks>
        /// <c>true</c> with <c>applyToCollection</c> set is a configuration error.
        /// </remarks>
        public bool IsExclusive { get; set; }
    }
}
