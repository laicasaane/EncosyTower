using System.Collections.Immutable;

namespace EncosyTower.Entities.Stats.Generators
{
    internal struct StatTypeInfo : IEquatable<StatTypeInfo>
    {
        public string type;
        public string typeName;
        public string namespaceName;
        public int size;

        public StatTypeInfo(string type, string typeName, string namespaceName, int size)
        {
            this.type = type;
            this.typeName = typeName;
            this.namespaceName = namespaceName;
            this.size = size;
        }

        public const string NAMESPACE = "EncosyTower.Entities.Stats";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public static ImmutableArray<StatTypeInfo> All => StatTypeInfoTable.All;

        private static class StatTypeInfoTable
        {
            public static readonly ImmutableArray<StatTypeInfo> All = ImmutableArray.Create<StatTypeInfo>(
                new("None", "None", "g__ETES", 1),
            new("bool", "Bool", "", 1),
            new("bool2", "Bool2", "g__UM", 2),
            new("bool2x2", "Bool2x2", "g__UM", 4),
            new("bool2x3", "Bool2x3", "g__UM", 6),
            new("bool2x4", "Bool2x4", "g__UM", 8),
            new("bool3", "Bool3", "g__UM", 3),
            new("bool3x2", "Bool3x2", "g__UM", 6),
            new("bool3x3", "Bool3x3", "g__UM", 9),
            new("bool3x4", "Bool3x4", "g__UM", 12),
            new("bool4", "Bool4", "g__UM", 4),
            new("bool4x2", "Bool4x2", "g__UM", 8),
            new("bool4x3", "Bool4x3", "g__UM", 12),
            new("bool4x4", "Bool4x4", "g__UM", 16),
            new("byte", "Byte", "", 1),
            new("double", "Double", "", 8),
            new("double2", "Double2", "g__UM", 16),
            new("double2x2", "Double2x2", "g__UM", 32),
            new("double2x3", "Double2x3", "g__UM", 48),
            new("double2x4", "Double2x4", "g__UM", 64),
            new("double3", "Double3", "g__UM", 24),
            new("double3x2", "Double3x2", "g__UM", 48),
            new("double3x3", "Double3x3", "g__UM", 72),
            new("double3x4", "Double3x4", "g__UM", 96),
            new("double4", "Double4", "g__UM", 32),
            new("double4x2", "Double4x2", "g__UM", 64),
            new("double4x3", "Double4x3", "g__UM", 96),
            new("double4x4", "Double4x4", "g__UM", 128),
            new("float", "Float", "", 4),
            new("float2", "Float2", "g__UM", 8),
            new("float2x2", "Float2x2", "g__UM", 16),
            new("float2x3", "Float2x3", "g__UM", 24),
            new("float2x4", "Float2x4", "g__UM", 32),
            new("float3", "Float3", "g__UM", 12),
            new("float3x2", "Float3x2", "g__UM", 24),
            new("float3x3", "Float3x3", "g__UM", 36),
            new("float3x4", "Float3x4", "g__UM", 48),
            new("float4", "Float4", "g__UM", 16),
            new("float4x2", "Float4x2", "g__UM", 32),
            new("float4x3", "Float4x3", "g__UM", 48),
            new("float4x4", "Float4x4", "g__UM", 64),
            new("half", "Half", "g__UM", 2),
            new("half2", "Half2", "g__UM", 4),
            new("half3", "Half3", "g__UM", 6),
            new("half4", "Half4", "g__UM", 8),
            new("int", "Int", "", 4),
            new("int2", "Int2", "g__UM", 8),
            new("int2x2", "Int2x2", "g__UM", 16),
            new("int2x3", "Int2x3", "g__UM", 24),
            new("int2x4", "Int2x4", "g__UM", 32),
            new("int3", "Int3", "g__UM", 12),
            new("int3x2", "Int3x2", "g__UM", 24),
            new("int3x3", "Int3x3", "g__UM", 36),
            new("int3x4", "Int3x4", "g__UM", 48),
            new("int4", "Int4", "g__UM", 16),
            new("int4x2", "Int4x2", "g__UM", 32),
            new("int4x3", "Int4x3", "g__UM", 48),
            new("int4x4", "Int4x4", "g__UM", 64),
            new("long", "Long", "", 8),
            new("sbyte", "SByte", "", 1),
            new("short", "Short", "", 2),
            new("uint", "UInt", "", 4),
            new("uint2", "UInt2", "g__UM", 8),
            new("uint2x2", "UInt2x2", "g__UM", 16),
            new("uint2x3", "UInt2x3", "g__UM", 24),
            new("uint2x4", "UInt2x4", "g__UM", 32),
            new("uint3", "UInt3", "g__UM", 12),
            new("uint3x2", "UInt3x2", "g__UM", 24),
            new("uint3x3", "UInt3x3", "g__UM", 36),
            new("uint3x4", "UInt3x4", "g__UM", 48),
            new("uint4", "UInt4", "g__UM", 16),
            new("uint4x2", "UInt4x2", "g__UM", 32),
            new("uint4x3", "UInt4x3", "g__UM", 48),
            new("uint4x4", "UInt4x4", "g__UM", 64),
            new("ulong", "ULong", "", 8),
                new("ushort", "UShort", "", 2)
            );
        }

        public static bool TryGet(int index, out StatTypeInfo info)
        {
            if ((uint)index < (uint)All.Length)
            {
                info = All[index];
                return true;
            }

            info = default;
            return false;
        }

        public static bool TryGetEnumTypeName(string underlyingType, out string typeName)
        {
            typeName = underlyingType switch {
                "sbyte" => "SByte",
                "byte" => "Byte",
                "short" => "Short",
                "ushort" => "UShort",
                "int" => "Int",
                "uint" => "UInt",
                "long" => "Long",
                "ulong" => "ULong",
                _ => null,
            };
            return typeName != null;
        }

        public readonly bool Equals(StatTypeInfo other)
            => string.Equals(type, other.type, StringComparison.Ordinal)
            && string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && size == other.size;

        public override readonly bool Equals(object obj)
            => obj is StatTypeInfo other && Equals(other);

        public override readonly int GetHashCode()
            => HashValue.Combine(type, typeName, namespaceName, size);
    }
}
