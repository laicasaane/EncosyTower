using System;

namespace EncosyTower.TypeWraps
{
    /// <remarks>
    /// Generated members forward to the wrapped value through its storage field. When that field is
    /// <see langword="readonly"/> (always in a class or a <see langword="readonly"/> struct), a mutable struct value
    /// cannot be changed through it: forwarded setters are not generated, and mutating methods and event
    /// subscriptions act on a copy and are lost. Reference types, handle types such as <c>NativeList&lt;T&gt;</c>,
    /// and immutable types are unaffected. For a mutable struct with value semantics, use a non-readonly struct.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WrapTypeAttribute : Attribute
    {
        public const string DEFAULT_MEMBER_NAME = "value";

        public Type Type { get; }

        public string MemberName { get; }

        public bool ExcludeConverter { get; set; }

        public WrapTypeAttribute(Type type) : this(type, DEFAULT_MEMBER_NAME)
        { }

        public WrapTypeAttribute(Type type, string memberName)
        {
            Type = type;
            MemberName = memberName;
        }
    }

    /// <remarks>
    /// Generated members forward to the wrapped value through the record's positional property, which returns a
    /// copy, so a mutable struct value cannot be changed through it: forwarded setters are not generated, and
    /// mutating methods and event subscriptions act on that copy and are lost. Reference types, handle types such as
    /// <c>NativeList&lt;T&gt;</c>, and immutable types are unaffected. For a mutable struct with value semantics, use
    /// <see cref="WrapTypeAttribute"/> on a non-readonly struct.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class WrapRecordAttribute : Attribute
    {
        public bool ExcludeConverter { get; set; }
    }

    public interface IWrapper { }

    public interface IWrap<T> : IWrapper { }
}
