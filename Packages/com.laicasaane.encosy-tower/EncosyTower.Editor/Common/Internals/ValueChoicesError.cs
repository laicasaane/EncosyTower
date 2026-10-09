#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using EncosyTower.PolyEnumStructs;

namespace EncosyTower.Editor.Internals
{
    [PolyEnumFactoryFor(typeof(Error))]
    internal readonly partial struct ValueChoicesError
    {
        private readonly Error _error;

        private ValueChoicesError(in Error error)
        {
            _error = error;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public readonly string ToMessage()
            => _error.ToMessage();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public readonly override string ToString()
            => ToMessage();

        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            partial interface IEnumCase
            {
                string ToMessage();
            }

            public readonly partial struct Undefined
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => string.Empty;
            }

            public readonly partial record struct SourceTypeNotFound(string SourceTypeName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices source type '{SourceTypeName}': the type was not found.";
            }

            public readonly partial record struct AmbiguousSourceType(string SourceTypeName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices source type '{SourceTypeName}': "
                        + "the name matches multiple loaded types; use an assembly-qualified name.";
            }

            public readonly partial record struct MissingMemberName(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "a choices attribute with a member name is required.";
            }

            public readonly partial record struct MissingType(
                  System.Type SourceType
                , string MemberName
                , System.Type ValueType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the source type and value type are required.";
            }

            public readonly partial record struct InvalidProperty(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the property must have a static getter and no index parameters.";
            }

            public readonly partial record struct InvalidMethod(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the method must be parameterless and have no unbound generic parameters.";
            }

            public readonly partial record struct InstanceMember(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the member is an instance member; a static member is required.";
            }

            public readonly partial record struct MissingMember(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': the member was not found.";
            }

            public readonly partial record struct UnsupportedMemberType(
                  System.Type SourceType
                , string MemberName
                , System.Type MemberType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"unsupported member type '{MemberType}'.";
            }

            public readonly partial record struct InvalidCollection(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "the member returned a null or unsupported collection.";
            }

            public readonly partial record struct IncompatibleValue(
                  System.Type SourceType
                , string MemberName
                , object Value
                , System.Type ValueType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"value '{Value}' is not assignable to '{ValueType}'.";
            }

            public readonly partial record struct TargetTypeMismatch(
                  System.Type SourceType
                , string MemberName
                , System.Type ValueType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"collection choices require an array or list value type, not '{ValueType}'.";
            }

            public readonly partial record struct ChoiceTargetTypeMismatch(
                  System.Type SourceType
                , string MemberName
                , bool ApplyToCollection
                , System.Type ValueType
                , System.Type ChoiceType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"{(ApplyToCollection ? "Collection" : "Element")} choices require values "
                        + $"assignable to '{ValueType}', not '{ChoiceType}'.";
            }

            public readonly partial record struct ExclusiveCollection(System.Type SourceType, string MemberName)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + "collection choices must be non-exclusive.";
            }

            public readonly partial record struct ReadFailure(
                  System.Type SourceType
                , string MemberName
                , System.Exception Exception
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public readonly string ToMessage()
                    => $"Value choices member '{SourceType?.FullName}.{MemberName}': "
                        + $"reading choices failed: {Exception.Message}.";
            }
        }
    }
}

#endif
