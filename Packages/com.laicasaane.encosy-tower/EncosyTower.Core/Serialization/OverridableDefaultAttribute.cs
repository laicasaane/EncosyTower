using System;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Serialization
{
    /// <summary>
    /// Declares where an <see cref="Overridable{T}"/> field reads its value when it is not overridden.
    /// </summary>
    /// <remarks>
    /// When <see cref="SourceType"/> derives from <c>EncosyTower.Settings.Settings&lt;T&gt;</c>, the member is read
    /// from its <c>Instance</c>; otherwise the member must be static.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class OverridableDefaultAttribute : Attribute
    {
        /// <summary>
        /// Selects the member supplying the value while the override is disabled.
        /// </summary>
        /// <param name="sourceType">The type holding the default value.</param>
        /// <param name="memberName">The field or property holding the default value.</param>
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
        public Type SourceType { get; }

        /// <summary>
        /// The name of the field or property that holds the default value.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Names only the default choice of enum dropdowns. <c>null</c> uses the drawer's default label.
        /// </summary>
        public string Label { get; set; }
    }
}
