using System;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Serialization
{
    /// <summary>
    /// Declares where an <see cref="Overridable{T}"/> field reads its value when it is not overridden.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <see cref="SourceType"/> derives from <see cref="EncosyTower.Settings.Settings{T}"/>, the member is an
    /// instance field or property read from its <c>Instance</c>; otherwise it is a static field or property.
    /// Public and non-public members are accepted. The Editor reports a member it cannot find or read once, as a
    /// Console warning, and shows the type default.
    /// </para>
    /// <para>
    /// Only the Editor reads this attribute, to show and seed the default. The runtime never resolves it:
    /// pass the same default to <see cref="Overridable{T}.GetValueOrDefault"/>.
    /// The Editor reads the member on every Inspector refresh. It follows a settings asset when its values
    /// change, and polls any other member about every 100 ms while the row is not overridden.
    /// </para>
    /// <para>
    /// When the value of an <see cref="Overridable{T}"/> is itself an array or list of <see cref="Overridable{T}"/>,
    /// the attribute on the outer field supplies the whole collection. The elements ignore the attribute
    /// and show the type default.
    /// </para>
    /// </remarks>
    /// <example>
    /// <c>SpawnerSettings</c> derives from <c>Settings&lt;SpawnerSettings&gt;</c> and has a public
    /// <c>waveSize</c> field. <c>DefaultDelay</c> is a non-public static property.
    /// <code>
    /// public sealed class Spawner : MonoBehaviour
    /// {
    ///     [OverridableDefault(typeof(SpawnerSettings), nameof(SpawnerSettings.waveSize))]
    ///     public Overridable&lt;int&gt; waveSize;
    ///
    ///     [OverridableDefault(typeof(Spawner), nameof(DefaultDelay))]
    ///     public Overridable&lt;float&gt; delay;
    ///
    ///     private static float DefaultDelay =&gt; 0.5f;
    ///
    ///     public float GetDelay()
    ///         =&gt; delay.GetValueOrDefault(DefaultDelay);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Overridable{T}"/>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class OverridableDefaultAttribute : Attribute
    {
        /// <summary>
        /// Selects the member supplying the value while the override is disabled.
        /// </summary>
        /// <param name="sourceType">The type holding the default value.</param>
        /// <param name="memberName">The field or property holding the default value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="sourceType"/> is null, or <paramref name="memberName"/> is null or empty.
        /// </exception>
        public OverridableDefaultAttribute(Type sourceType, string memberName)
        {
            DebuggingThrowHelper.ThrowIfNull(sourceType);
            DebuggingThrowHelper.ThrowIfNullOrEmpty(memberName);

            SourceType = sourceType;
            MemberName = memberName;
        }

        /// <summary>
        /// The type that holds the default value.
        /// </summary>
        /// <remarks>
        /// A type that derives from <see cref="EncosyTower.Settings.Settings{T}"/> supplies the member from its
        /// <c>Instance</c>; see the type remarks.
        /// </remarks>
        public Type SourceType { get; }

        /// <summary>
        /// The name of the field or property that holds the default value.
        /// </summary>
        /// <remarks>
        /// Indexers and write-only properties are not read; the Editor reports the member as not found.
        /// </remarks>
        public string MemberName { get; }

        /// <summary>
        /// Names only the default choice of enum dropdowns. <c>null</c> uses the drawer's default label.
        /// </summary>
        /// <value>
        /// The text before the default value in the first dropdown item. When <c>null</c> or empty, the label is
        /// "Project Settings" if <see cref="SourceType"/> derives from <see cref="EncosyTower.Settings.Settings{T}"/>,
        /// otherwise "Default". It has no effect on switch rows.
        /// </value>
        public string Label { get; set; }
    }
}
