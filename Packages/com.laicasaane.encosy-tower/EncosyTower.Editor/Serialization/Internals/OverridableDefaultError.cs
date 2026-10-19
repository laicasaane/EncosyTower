#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using EncosyTower.PolyEnumStructs;

namespace EncosyTower.Editor.Serialization.Internals
{
    [PolyEnumStruct]
    internal readonly partial struct OverridableDefaultError
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

        public readonly partial record struct MemberNotFound(
              System.Type SourceType
            , string MemberName
            , bool IsInstanceMember
        )
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => $"Overridable default member '{SourceType?.FullName}.{MemberName}': no readable "
                    + $"{(IsInstanceMember ? "instance" : "static")} field or property with this name was found.";
        }

        public readonly partial record struct ReadFailure(
              System.Type SourceType
            , string MemberName
            , System.Exception Exception
        )
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToMessage()
                => $"Overridable default member '{SourceType?.FullName}.{MemberName}': "
                    + $"reading the default value failed: {Exception?.Message}.";
        }
    }
}

#endif
