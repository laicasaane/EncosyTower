#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using EncosyTower.PolyEnumStructs;

namespace EncosyTower.Editor.Internals
{
    /// <summary>
    /// Describes why the choices of a <see cref="ValueChoicesAttribute"/> could not be resolved.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <c>Undefined</c>, which means the choices were resolved.
    /// <see cref="ValueChoicesEditorAPI.TryGetChoices"/> returns the other cases, and
    /// <see cref="ValueChoicesPropertyDrawer"/> logs the message of a failure as a warning.
    /// </para>
    /// <para>
    /// <see cref="ValueChoicesEditorAPI.TryGetChoices"/> reports the first failure it finds, in the order of the
    /// table below, from the member name checks to the member read and the collection checks.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>When it occurs, and the fix</description>
    /// </listheader>
    /// <item>
    /// <term><c>Undefined</c></term>
    /// <description>No failure. The message is empty.</description>
    /// </item>
    /// <item>
    /// <term><c>MissingMemberName</c></term>
    /// <description>The attribute is missing or its member name is null or empty. Pass a member name.</description>
    /// </item>
    /// <item>
    /// <term><c>SourceTypeNotFound</c></term>
    /// <description>
    /// A source type name matches no loaded type, or resolving it throws.
    /// Use the full name or the assembly-qualified name of a loaded type.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>AmbiguousSourceType</c></term>
    /// <description>
    /// A source type name matches more than one loaded type. Use the assembly-qualified name.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>MissingType</c></term>
    /// <description>The source type or the value type is null. Name the source type in the attribute.</description>
    /// </item>
    /// <item>
    /// <term><c>TargetTypeMismatch</c></term>
    /// <description>
    /// <c>applyToCollection</c> is true but the field value type is not a one-dimensional array or a
    /// <c>List&lt;T&gt;</c>. Remove <c>applyToCollection</c> or use an array or list field.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>ExclusiveCollection</c></term>
    /// <description>
    /// <c>applyToCollection</c> is true and <c>IsExclusive</c> is true. Set <c>IsExclusive</c> to false.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InvalidProperty</c></term>
    /// <description>
    /// The named property has no static getter or has index parameters. Use a static property with a getter.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InvalidMethod</c></term>
    /// <description>
    /// Static methods with the name exist, but none is parameterless and non-generic. Add such a method.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InstanceMember</c></term>
    /// <description>Only instance members have the name. Make the member static.</description>
    /// </item>
    /// <item>
    /// <term><c>MissingMember</c></term>
    /// <description>
    /// The source type has no member with the name. Check the name and the source type.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>UnsupportedMemberType</c></term>
    /// <description>
    /// The member type is not an enumerable of the field value type, of <see cref="ValueChoice{T}"/> or of
    /// <see cref="ValueChoice"/>. Change the member type.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InvalidCollection</c></term>
    /// <description>The member returned null or a value that is not enumerable. Return an enumerable.</description>
    /// </item>
    /// <item>
    /// <term><c>IncompatibleValue</c></term>
    /// <description>
    /// A choice value is not assignable to the field value type; null counts for a non-nullable value type.
    /// Change the value or the type.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>ChoiceTargetTypeMismatch</c></term>
    /// <description>
    /// The choices do not have the shape of the target. With <c>applyToCollection</c>, each choice must be a whole
    /// array or list assignable to the field value type. Without it, each choice must be one element value, and a
    /// member of collection choices is reported. Match the choices to the target or change <c>applyToCollection</c>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>ReadFailure</c></term>
    /// <description>
    /// Reading the member or its choices threw; the case holds the base exception. Fix the member so that it does
    /// not throw.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumFactoryFor(typeof(Error))]
    internal readonly partial struct ValueChoicesError
    {
        private readonly Error _error;

        private ValueChoicesError(in Error error)
        {
            _error = error;
        }

        /// <summary>
        /// Gets the message of the wrapped case.
        /// </summary>
        /// <returns>A message for the Console, or an empty string for <c>Undefined</c>.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public readonly string ToMessage()
            => _error.ToMessage();

        /// <inheritdoc cref="ToMessage"/>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public readonly override string ToString()
            => ToMessage();

        /// <summary>
        /// The failure cases. The error type documents when each case occurs.
        /// </summary>
        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            /// <summary>
            /// The contract of every failure case.
            /// </summary>
            partial interface IEnumCase
            {
                /// <summary>
                /// Gets the message of the case.
                /// </summary>
                /// <returns>A message for the Console, or an empty string for <c>Undefined</c>.</returns>
                string ToMessage();
            }

            /// <summary>
            /// The choices were resolved. This is the default value.
            /// </summary>
            public readonly partial struct Undefined
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => string.Empty;
            }

            /// <summary>
            /// A source type name matches no loaded type, or resolving it throws.
            /// </summary>
            public readonly partial record struct SourceTypeNotFound(string SourceTypeName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices source type '{SourceTypeName}': the type was not found.";
            }

            /// <summary>
            /// A source type name matches more than one loaded type.
            /// </summary>
            public readonly partial record struct AmbiguousSourceType(string SourceTypeName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices source type '{SourceTypeName}': "
                        + "the name matches multiple loaded types; use an assembly-qualified name.";
            }

            /// <summary>
            /// The attribute is missing, or its member name is null or empty.
            /// </summary>
            public readonly partial record struct MissingMemberName(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "a choices attribute with a member name is required.";
            }

            /// <summary>
            /// The source type or the value type is null.
            /// </summary>
            public readonly partial record struct MissingType(
                  System.Type SourceType
                , string MemberName
                , System.Type ValueType
            )
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the source type and value type are required.";
            }

            /// <summary>
            /// The named property has no static getter, or has index parameters.
            /// </summary>
            public readonly partial record struct InvalidProperty(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the property must have a static getter and no index parameters.";
            }

            /// <summary>
            /// Static methods have the name, but none is parameterless and non-generic.
            /// </summary>
            public readonly partial record struct InvalidMethod(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the method must be parameterless and have no unbound generic parameters.";
            }

            /// <summary>
            /// Only instance members have the name.
            /// </summary>
            public readonly partial record struct InstanceMember(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the member is an instance member; a static member is required.";
            }

            /// <summary>
            /// The source type has no member with the name.
            /// </summary>
            public readonly partial record struct MissingMember(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': the member was not found.";
            }

            /// <summary>
            /// The member type is not an enumerable of the field value type, of <see cref="ValueChoice{T}"/> or of
            /// <see cref="ValueChoice"/>.
            /// </summary>
            public readonly partial record struct UnsupportedMemberType(
                  System.Type SourceType
                , string MemberName
                , System.Type MemberType
            )
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"unsupported member type '{MemberType}'.";
            }

            /// <summary>
            /// The member returned null or a value that is not enumerable.
            /// </summary>
            public readonly partial record struct InvalidCollection(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the member returned a null or unsupported collection.";
            }

            /// <summary>
            /// A choice value is not assignable to the field value type.
            /// </summary>
            public readonly partial record struct IncompatibleValue(
                  System.Type SourceType
                , string MemberName
                , object Value
                , System.Type ValueType
            )
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"value '{Value}' is not assignable to '{ValueType}'.";
            }

            /// <summary>
            /// <c>applyToCollection</c> is true but the field value type is not an array or a list.
            /// </summary>
            public readonly partial record struct TargetTypeMismatch(
                  System.Type SourceType
                , string MemberName
                , System.Type ValueType
            )
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"collection choices require an array or list value type, not '{ValueType}'.";
            }

            /// <summary>
            /// The choices do not have the shape of the target: whole collections, or single element values.
            /// </summary>
            public readonly partial record struct ChoiceTargetTypeMismatch(
                  System.Type SourceType
                , string MemberName
                , bool ApplyToCollection
                , System.Type ValueType
                , System.Type ChoiceType
            )
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"{(ApplyToCollection ? "Collection" : "Element")} choices require values "
                        + $"assignable to '{ValueType}', not '{ChoiceType}'.";
            }

            /// <summary>
            /// <c>applyToCollection</c> is combined with <c>IsExclusive</c>.
            /// </summary>
            public readonly partial record struct ExclusiveCollection(System.Type SourceType, string MemberName)
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "collection choices must be non-exclusive.";
            }

            /// <summary>
            /// Reading the member or its choices threw. The case holds the base exception.
            /// </summary>
            public readonly partial record struct ReadFailure(
                  System.Type SourceType
                , string MemberName
                , System.Exception Exception
            )
            {
                /// <inheritdoc/>
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"reading choices failed: {Exception.Message}.";
            }
        }
    }
}

#endif
