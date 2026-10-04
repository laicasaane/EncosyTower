using System;

namespace EncosyTower.TypeFlags
{
    /// <summary>
    /// Generates a type flag API in the annotated partial type: a public <c>TypeFlag</c> field and a nested
    /// <c>TypeFlagAPI</c> type. Every name it generates must be free in the type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class TypeFlagAttribute : Attribute
    {
        /// <summary>
        /// Who may call the generated write members.
        /// </summary>
        /// <remarks>
        /// With <see cref="UseExtensions"/>, this option only sets the visibility of the generated fields. They are
        /// typed <c>TypeFlag&lt;T&gt;</c>, which is public, so any code can still write through
        /// <c>default(TypeFlag&lt;T&gt;)</c>.
        /// </remarks>
        public TypeFlagAccess WriteAccess { get; set; }

        /// <summary>
        /// Member groups to generate beyond the flag state.
        /// </summary>
        public TypeFlagApi Api { get; set; } = TypeFlagApi.Default;

        /// <summary>
        /// Generates only the fields, typed <c>TypeFlag&lt;T&gt;</c> and <c>TypeFlag&lt;T&gt;.ReadOnly</c>,
        /// and relies on the predefined extension methods. <see cref="WriteAccess"/> sets the field visibility;
        /// <see cref="Api"/> has no effect.
        /// </summary>
        public bool UseExtensions { get; set; }
    }

    /// <summary>
    /// Who may call the write members that <see cref="TypeFlagAttribute"/> generates.
    /// </summary>
    public enum TypeFlagAccess : byte
    {
        /// <summary>
        /// The owner and its nested types write through the private <c>s_typeFlag</c> field; other code reads
        /// through the public <c>TypeFlag</c> field.
        /// </summary>
        Private,

        /// <summary>
        /// <c>TypeFlagAPI</c> has public read members and <see langword="internal"/> write members, so the owner's
        /// assembly can write.
        /// </summary>
        Internal,

        /// <summary>
        /// <c>TypeFlagAPI</c> has public read and write members, so any assembly can write.
        /// </summary>
        Public,
    }

    /// <summary>
    /// Member groups that <see cref="TypeFlagAttribute"/> generates beyond the flag state.
    /// </summary>
    [Flags]
    public enum TypeFlagApi : byte
    {
        /// <summary>
        /// Only the flag state: <c>TypeId</c>, <c>IsEnabled</c>, <c>WaitUntilEnabledAsync</c>, <c>Enable</c>, and
        /// <c>Disable</c>.
        /// </summary>
        State = 0,

        /// <summary>
        /// Storage of the owner itself. A class owner gets <c>TryGetInstance</c>, <c>GetInstanceOrThrow</c>,
        /// <c>TryRegister</c>, and <c>TryUnregister</c>; a struct owner gets <c>TryGetValue</c>,
        /// <c>GetValueOrThrow</c>, <c>SetValue</c>, and <c>TryRemoveValue</c> for its own value.
        /// </summary>
        Self = 1 << 0,

        /// <summary>
        /// Storage of other types for the owner: <c>TryGetObject&lt;T&gt;</c>, <c>GetObjectOrThrow&lt;T&gt;</c>,
        /// <c>TryGetValue&lt;T&gt;</c>, <c>GetValueOrThrow&lt;T&gt;</c>, <c>TryAddObject&lt;T&gt;</c>,
        /// <c>TryRemoveObject&lt;T&gt;</c>, <c>SetValue&lt;T&gt;</c>, and <c>TryRemoveValue&lt;T&gt;</c>.
        /// </summary>
        Related = 1 << 1,

        /// <summary>
        /// With <see cref="Self"/>, adds <c>GetInstanceAsync</c> or <c>GetValueAsync</c>. Adds nothing alone.
        /// </summary>
        Async = 1 << 2,

        /// <summary>
        /// <see cref="Self"/>, <see cref="Related"/>, and <see cref="Async"/>.
        /// </summary>
        Default = Self | Related | Async,
    }
}
