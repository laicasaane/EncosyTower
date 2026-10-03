#pragma warning disable 0219

using EncosyTower.UnionIds;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SC = global::System.Collections;
using g__SCG = global::System.Collections.Generic;
using g__SCM = global::System.ComponentModel;
using g__SD = global::System.Diagnostics;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ET = global::EncosyTower.Common;
using g__ETCol = global::EncosyTower.Collections;
using g__ETColE = global::EncosyTower.Collections.Extensions;
using g__ETCon = global::EncosyTower.Conversion;
using g__ETDBG = global::EncosyTower.Debugging;
using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;
using g__ETEE = global::EncosyTower.EnumExtensions;
using g__ETEESG = global::EncosyTower.EnumExtensions.SourceGen;
using g__ETUI = global::EncosyTower.UnionIds;
using g__ETUIT = global::EncosyTower.UnionIds.Types;
using g__ETS = global::EncosyTower.Serialization;
using g__ETSE = global::EncosyTower.SystemExtensions;
using g__UE = global::UnityEngine;
using g__UC = global::Unity.Collections;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace




#pragma warning disable

[g__SRCS.Union]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit, Size = 4)]
[g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
[g__SCM.TypeConverter(typeof(TypeConverter))]
partial struct Id : g__ETUI.IUnionId<uint, Id>, g__SRCS.IUnion, g__ET.IHasValue
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.ITryParse<Id>
    , g__ETCon.ITryParseSpan<Id>
{
    public const char SEPARATOR = '-';

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly uint _raw;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly byte IdUnsigned;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly sbyte IdSigned;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly global::Kind Id_Kind;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(3)]
    public readonly IdKind Kind;

    private Id(uint value) : this()
    {
        _raw = value;
    }

    public Id(IdKind kind, byte id) : this()
    {
        IdUnsigned = id;
        Kind = kind;
    }

    public Id(IdKind kind, sbyte id) : this()
    {
        IdSigned = id;
        Kind = kind;
    }

    public Id(global::Kind id) : this()
    {
        Id_Kind = id;
        Kind = IdKind.Kind;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator Id(global::Kind id)
    {
        return new(id);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id(IdKind kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(kind, g__S.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)
    {
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id(string kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), g__S.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)
    {
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id(string kind, byte id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)
    {
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id(string kind, sbyte id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)
    {
    }

    public Id(IdKind kind, g__S.ReadOnlySpan<char> id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                KindExtensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                Id_Kind = idValue;
                break;
            }

        }

        Kind = kind;
    }

    public Id(g__S.ReadOnlySpan<char> kind, g__S.ReadOnlySpan<char> id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
    {
        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            kindValue = default;
        }

        switch (kindValue)
        {
            case IdKind.Kind:
            {
                KindExtensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                Id_Kind = idValue;
                break;
            }

        }

        Kind = kindValue;
    }

    public Id(g__S.ReadOnlySpan<char> kind, byte id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
    {
        IdUnsigned = id;

        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            kindValue = default;
        }

        Kind = kindValue;
    }

    public Id(g__S.ReadOnlySpan<char> kind, sbyte id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
    {
        IdSigned = id;

        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            kindValue = default;
        }

        Kind = kindValue;
    }

    public object Value
    {
        get => Kind switch
        {
            IdKind.Kind => Id_Kind,
            _ => null,
        };
    }

    public bool HasValue
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => Id_IdKindExtensions.IsDefined(Kind);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static explicit operator global::Kind(Id value)
    {
        return value.GetValueOrThrow(g__ET.GenericT.T<global::Kind>());
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly global::Kind GetValueOrThrow(g__ET.T<global::Kind> _)
    {
        ThrowIfUncastable(Kind, IdKind.Kind);

        return Id_Kind;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly global::Kind GetValueOrDefault(g__ET.T<global::Kind> _ = default, global::Kind @default = default)
    {
        if (IsCastable(Kind, IdKind.Kind))
        {
            return Id_Kind;
        }

        return @default;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out global::Kind value)
    {
        if (Kind == IdKind.Kind)
        {
            value = Id_Kind;
            return true;
        }

        value = default;
        return false;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string str, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        return TryParse(g__S.MemoryExtensions.AsSpan(str), out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    public bool TryParse(g__S.ReadOnlySpan<char> str, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        if (str.IsEmpty)
        {
            result = default;
            return false;
        }

        var ranges = g__ETSE.SpanAPI.Split(str, SEPARATOR, 2);

        g__S.Range? kindRange = default;
        g__S.Range? idRange = default;

        foreach (var range in ranges)
        {
            if (kindRange.HasValue == false)
            {
                kindRange = range;
                continue;
            }

            if (idRange.HasValue == false)
            {
                idRange = range;
                continue;
            }

            if (kindRange.HasValue && idRange.HasValue)
            {
                break;
            }
        }

        if (kindRange.HasValue == false || idRange.HasValue == false)
        {
            result = default;
            return false;
        }

        var kindSpan = str[kindRange.Value];
        var idSpan = str[idRange.Value];

        return TryParse(kindSpan, idSpan, out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(IdKind kind, string id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        return TryParse(kind, g__S.MemoryExtensions.AsSpan(id), out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string kind, string id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        return TryParse(g__S.MemoryExtensions.AsSpan(kind), g__S.MemoryExtensions.AsSpan(id), out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string kind, byte id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        return TryParse(g__S.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string kind, sbyte id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        return TryParse(g__S.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    public bool TryParse(IdKind kind, g__S.ReadOnlySpan<char> id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                if (KindExtensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute))
                {
                    result = new(idValue);
                    return true;
                }

                goto FAILED;
            }

        }

        FAILED:
        result = default;
        return false;
    }

    public bool TryParse(g__S.ReadOnlySpan<char> kind, g__S.ReadOnlySpan<char> id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            result = default;
            return false;
        }

        switch (kindValue)
        {
            case IdKind.Kind:
            {
                if (KindExtensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute))
                {
                    result = new(idValue);
                    return true;
                }

                goto FAILED;
            }

        }

        FAILED:
        result = default;
        return false;
    }

    public bool TryParse(g__S.ReadOnlySpan<char> kind, byte id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            result = default;
            return false;
        }

        result = new(kindValue, id);
        return true;
    }

    public bool TryParse(g__S.ReadOnlySpan<char> kind, sbyte id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            result = default;
            return false;
        }

        result = new(kindValue, id);
        return true;
    }

    public bool TryFormat(g__S.Span<char> destination, out int charsWritten)
    {
        if (g__ETCol.EncosyFixedStringExtensions.TryFormat(Kind.ToFixedString(), destination, out var kindCharsWritten) == false)
        {
            charsWritten = 0;
            return false;
        }

        if (g__ETCol.EncosyFixedStringExtensions.TryFormat(GetIdFixedString(), destination[..kindCharsWritten], out var idCharsWritten) == false)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = kindCharsWritten + idCharsWritten;
        return true;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Id other)
    {
        return _raw == other._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object obj)
    {
        return obj is Id other && _raw == other._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly int CompareTo(Id other)
    {
        return _raw.CompareTo(other._raw);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode()
    {
        return _raw.GetHashCode();
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(Id value)
    {
        return value._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator Id(uint value)
    {
        return new(value);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Id left, Id right)
    {
        return left._raw == right._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Id left, Id right)
    {
        return left._raw != right._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Id left, Id right)
    {
        return left._raw < right._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Id left, Id right)
    {
        return left._raw <= right._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Id left, Id right)
    {
        return left._raw > right._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Id left, Id right)
    {
        return left._raw >= right._raw;
    }

    public readonly override string ToString()
    {
        return Kind switch
        {
            IdKind.Kind => $"{Id_IdKindExtensions.Names.Kind}{SEPARATOR}{KindExtensions.ToStringFast(Id_Kind)}",
            _ => $"{Kind.ToUnderlyingValue().ToString()}{SEPARATOR}{IdUnsigned}",
        };

    }

    public readonly string ToDisplayString()
    {
        return Kind switch
        {
            IdKind.Kind => $"{Id_IdKindExtensions.DisplayNames.Kind}{SEPARATOR}{KindExtensions.ToDisplayStringFast(Id_Kind)}",
            _ => $"{Kind.ToUnderlyingValue().ToString()}{SEPARATOR}{IdUnsigned}",
        };

    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly TFixedString ToFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

    public readonly g__UC.FixedString32Bytes ToFixedString()
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, Kind.ToFixedString(false));
        g__UC.FixedStringMethods.Append(ref fs, '-');

        switch (Kind)
        {
            case IdKind.Kind:
            {
                g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToFixedString(Id_Kind, false));
                break;
            }

            default:
            {
                g__UC.FixedStringMethods.Append(ref fs, IdUnsigned);
                break;
            }
        }

        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public TFixedString ToDisplayFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToDisplayFixedString());

    public readonly g__UC.FixedString32Bytes ToDisplayFixedString()
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, Kind.ToDisplayFixedString(false));
        g__UC.FixedStringMethods.Append(ref fs, '-');

        switch (Kind)
        {
            case IdKind.Kind:
            {
                g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToDisplayFixedString(Id_Kind, false));
                break;
            }

            default:
            {
                g__UC.FixedStringMethods.Append(ref fs, IdUnsigned);
                break;
            }
        }

        return fs;
    }

    public readonly string GetIdStringFast()
    {
        return Kind switch
        {
            IdKind.Kind => KindExtensions.ToStringFast(Id_Kind, false),
            _ => IdUnsigned.ToString(),
        };
    }

    public readonly string GetIdDisplayStringFast()
    {
        return Kind switch
        {
            IdKind.Kind => KindExtensions.ToDisplayStringFast(Id_Kind, false),
            _ => IdUnsigned.ToString(),
        };
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly TFixedString GetIdFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
    {
        TFixedString result = default;
        g__UC.FixedStringMethods.Append(ref result, GetIdFixedString());
        return result;
    }

    public readonly g__UC.FixedString32Bytes GetIdFixedString()
    {
        var fs = new g__UC.FixedString32Bytes();

        switch (Kind)
        {
            case IdKind.Kind:
            {
                g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToFixedString(Id_Kind, false));
                break;
            }

            default:
            {
                g__UC.FixedStringMethods.Append(ref fs, IdUnsigned);
                break;
            }
        }

        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly TFixedString GetIdDisplayFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
    {
        TFixedString result = default;
        g__UC.FixedStringMethods.Append(ref result, GetIdDisplayFixedString());
        return result;
    }

    public readonly g__UC.FixedString32Bytes GetIdDisplayFixedString()
    {
        var fs = new g__UC.FixedString32Bytes();

        switch (Kind)
        {
            case IdKind.Kind:
            {
                g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToDisplayFixedString(Id_Kind, false));
                break;
            }

            default:
            {
                g__UC.FixedStringMethods.Append(ref fs, IdUnsigned);
                break;
            }
        }

        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNames(IdKind kind, g__SCG.ICollection<string> result)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                g__ETColE.EncosyICollectionExtensions.AddRangeFast(result, KindExtensions.Names.AsSpan());
                return true;
            }

            default:
            {
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetDisplayNames(IdKind kind, g__SCG.ICollection<string> result)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                g__ETColE.EncosyICollectionExtensions.AddRangeFast(result, KindExtensions.DisplayNames.AsSpan());
                return true;
            }

            default:
            {
                result = g__S.Array.Empty<string>();
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetNames(IdKind kind, out g__S.ReadOnlyMemory<string> result)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                result = KindExtensions.Names.AsMemory();
                return true;
            }

            default:
            {
                result = g__S.Array.Empty<string>();
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetDisplayNames(IdKind kind, out g__S.ReadOnlyMemory<string> result)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                result = KindExtensions.DisplayNames.AsMemory();
                return true;
            }

            default:
            {
                result = g__S.Array.Empty<string>();
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetFixedNames(IdKind kind, g__UC.AllocatorManager.AllocatorHandle allocator, out g__UC.NativeArray<g__UC.FixedString32Bytes> result)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                result = KindExtensions.FixedNames.ToNativeArray<g__UC.FixedString32Bytes>(allocator);
                return true;
            }

            default:
            {
                result = g__UC.CollectionHelper.CreateNativeArray<g__UC.FixedString32Bytes>(0, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryGetFixedDisplayNames(IdKind kind, g__UC.AllocatorManager.AllocatorHandle allocator, out g__UC.NativeArray<g__UC.FixedString32Bytes> result)
    {
        switch (kind)
        {
            case IdKind.Kind:
            {
                result = KindExtensions.FixedDisplayNames.ToNativeArray<g__UC.FixedString32Bytes>(allocator);
                return true;
            }

            default:
            {
                result = g__UC.CollectionHelper.CreateNativeArray<g__UC.FixedString32Bytes>(0, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    private static bool IsCastable(IdKind a, IdKind b)
    {
        return a == b;
    }

    [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
    private static void ThrowIfUncastable(IdKind source, IdKind target)
    {
        if (IsCastable(source, target) == false)
        {
            throw new g__S.InvalidCastException(
                $"Cannot cast 'Id' into '{target}' because it currently stores a '{source}'."
            );
        }

    }

}

partial struct Id // IdKind
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    public enum IdKind : byte
    {
        /// <see cref="global::Kind"/>
        Kind = 0,

    }

}

partial struct Id // Serializable
{
    [g__SRCS.Union]
    [g__S.Serializable]
    [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    public partial struct Serializable : g__SRCS.IUnion, g__ETUI.ISerializableUnionId<uint, Id, Serializable>
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
    {
        [g__UE.SerializeField]
        public IdKind Kind;

        [g__UE.SerializeField]
        public long Id;

        public Serializable(IdKind kind, long id) : this()
        {
            Kind = kind;
            Id = id;
        }

        public Serializable(global::Kind id) : this()
        {
            Kind = IdKind.Kind;
            Id = (long)new Id(id).IdUnsigned;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Serializable(IdKind kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(kind, g__S.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)
        {
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Serializable(string kind, string id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), g__S.MemoryExtensions.AsSpan(id), ignoreCase, allowMatchingMetadataAttribute)
        {
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Serializable(string kind, long id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)
        {
        }

        public Serializable(IdKind kind, g__S.ReadOnlySpan<char> id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
        {
            switch (kind)
            {
                case IdKind.Kind:
                {
                    KindExtensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                    Id_Kind = idValue;
                    break;
                }

            }

            Kind = kind;
        }

        public Serializable(g__S.ReadOnlySpan<char> kind, g__S.ReadOnlySpan<char> id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
        {
            if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
            {
                kindValue = default;
            }

            switch (kindValue)
            {
                case IdKind.Kind:
                {
                    KindExtensions.TryParse(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                    Id_Kind = idValue;
                    break;
                }

            }

            Kind = kindValue;
        }

        public Serializable(g__S.ReadOnlySpan<char> kind, long id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
        {
            Id = id;

            if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
            {
                kindValue = default;
            }

            Kind = kindValue;
        }

        [field: g__S.NonSerialized]
        public global::Kind Id_Kind
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => new Id(Kind, (byte)Id).Id_Kind;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set => Id = (long)new Id(value).IdUnsigned;
        }

        public object Value
        {
            get => Kind switch
            {
                IdKind.Kind => Id_Kind,
                _ => null,
            };
        }

        public bool HasValue
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => Id_IdKindExtensions.IsDefined(Kind);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator global::Kind(Serializable value)
        {
            return value.GetValueOrThrow(g__ET.GenericT.T<global::Kind>());
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly global::Kind GetValueOrThrow(g__ET.T<global::Kind> _)
        {
            ThrowIfUncastable(Kind, IdKind.Kind);

            return Id_Kind;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly global::Kind GetValueOrDefault(g__ET.T<global::Kind> _ = default, global::Kind @default = default)
        {
            if (IsCastable(Kind, IdKind.Kind))
            {
                return Id_Kind;
            }

            return @default;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out global::Kind value)
        {
            if (Kind == IdKind.Kind)
            {
                value = Id_Kind;
                return true;
            }

            value = default;
            return false;
        }

        public readonly bool TryConvert(out Id result)
        {
            switch (Kind)
            {
                case IdKind.Kind:
                {
                    result = new(Id_Kind);
                    return true;
                }

                default:
                {
                    result = default;
                    return false;
                }

            }
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Serializable other)
        {
            return Kind == other.Kind && Id == other.Id;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            return obj is Serializable other && Kind == other.Kind && Id == other.Id;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly int CompareTo(Serializable other)
        {
            return ((uint)this).CompareTo((uint)other);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            return ((uint)this).GetHashCode();
        }

        public readonly override string ToString()
        {
            return Kind switch
            {
                IdKind.Kind => $"{Id_IdKindExtensions.Names.Kind}{SEPARATOR}{KindExtensions.ToStringFast(Id_Kind)}",
                _ => $"{Kind.ToUnderlyingValue().ToString()}{SEPARATOR}{Id}",
            };

        }

        public readonly string ToDisplayString()
        {
            return Kind switch
            {
                IdKind.Kind => $"{Id_IdKindExtensions.DisplayNames.Kind}{SEPARATOR}{KindExtensions.ToDisplayStringFast(Id_Kind)}",
                _ => $"{Kind.ToUnderlyingValue().ToString()}{SEPARATOR}{Id}",
            };

        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly TFixedString ToFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

        public readonly g__UC.FixedString32Bytes ToFixedString()
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, Kind.ToFixedString(false));
            g__UC.FixedStringMethods.Append(ref fs, '-');

            switch (Kind)
            {
                case IdKind.Kind:
                {
                    g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToFixedString(Id_Kind, false));
                    break;
                }

                default:
                {
                    g__UC.FixedStringMethods.Append(ref fs, Id);
                    break;
                }
            }

            return fs;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public TFixedString ToDisplayFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToDisplayFixedString());

        public readonly g__UC.FixedString32Bytes ToDisplayFixedString()
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, Kind.ToDisplayFixedString(false));
            g__UC.FixedStringMethods.Append(ref fs, '-');

            switch (Kind)
            {
                case IdKind.Kind:
                {
                    g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToDisplayFixedString(Id_Kind, false));
                    break;
                }

                default:
                {
                    g__UC.FixedStringMethods.Append(ref fs, Id);
                    break;
                }
            }

            return fs;
        }

        public readonly string GetIdStringFast()
        {
            return Kind switch
            {
                IdKind.Kind => KindExtensions.ToStringFast(Id_Kind, false),
                _ => Id.ToString(),
            };
        }

        public readonly string GetIdDisplayStringFast()
        {
            return Kind switch
            {
                IdKind.Kind => KindExtensions.ToDisplayStringFast(Id_Kind, false),
                _ => Id.ToString(),
            };
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly TFixedString GetIdFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            TFixedString result = default;
            g__UC.FixedStringMethods.Append(ref result, GetIdFixedString());
            return result;
        }

        public readonly g__UC.FixedString32Bytes GetIdFixedString()
        {
            var fs = new g__UC.FixedString32Bytes();

            switch (Kind)
            {
                case IdKind.Kind:
                {
                    g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToFixedString(Id_Kind, false));
                    break;
                }

                default:
                {
                    g__UC.FixedStringMethods.Append(ref fs, Id);
                    break;
                }
            }

            return fs;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly TFixedString GetIdDisplayFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            TFixedString result = default;
            g__UC.FixedStringMethods.Append(ref result, GetIdDisplayFixedString());
            return result;
        }

        public readonly g__UC.FixedString32Bytes GetIdDisplayFixedString()
        {
            var fs = new g__UC.FixedString32Bytes();

            switch (Kind)
            {
                case IdKind.Kind:
                {
                    g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToDisplayFixedString(Id_Kind, false));
                    break;
                }

                default:
                {
                    g__UC.FixedStringMethods.Append(ref fs, Id);
                    break;
                }
            }

            return fs;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(Serializable value)
        {
            return new Union { id = (byte)value.Id, kind = value.Kind }.raw;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Serializable left, Serializable right)
        {
            return left.Kind == right.Kind && left.Id == right.Id;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Serializable left, Serializable right)
        {
            return left.Kind != right.Kind || left.Id != right.Id;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Serializable left, Serializable right)
        {
            return ((uint)left) < ((uint)right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Serializable left, Serializable right)
        {
            return ((uint)left) <= ((uint)right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Serializable left, Serializable right)
        {
            return ((uint)left) > ((uint)right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Serializable left, Serializable right)
        {
            return ((uint)left) >= ((uint)right);
        }

        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        private struct Union
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public uint raw;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public byte id;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(3)]
            public IdKind kind;

        }

    }

}

partial struct Id // TypeConverter
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    public sealed class TypeConverter : g__ETS.ParsableStructConverter<Id>
    {
        public override bool IgnoreCase => false;

        public override bool AllowMatchingMetadataAttribute => false;
    }

}

partial struct Id // KindExtensions
{
#region    EXTENSIONS
#endregion ==========

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    private static partial class KindExtensions // KindExtensions
    {
        public const int Length = 1;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToStringFast(global::Kind value)
            => ToStringFast(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToStringFast(global::Kind value, bool emptyIfUndefined)
            => Names.Get(value, emptyIfUndefined);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToDisplayStringFast(global::Kind value)
            => ToDisplayStringFast(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToDisplayStringFast(global::Kind value, bool emptyIfUndefined)
            => DisplayNames.Get(value, emptyIfUndefined);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToFixedString(global::Kind value)
            => ToFixedString(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToFixedString(global::Kind value, bool emptyIfUndefined)
            => FixedNames.Get(value, emptyIfUndefined);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(global::Kind value)
            => ToDisplayFixedString(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(global::Kind value, bool emptyIfUndefined)
            => FixedDisplayNames.Get(value, emptyIfUndefined);

        private static g__UC.FixedString32Bytes ToFixedString(byte value)
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, value);
            return fs;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static byte ToUnderlyingValue(global::Kind value)
            => (byte)value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string name, out global::Kind value)
            => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string name, out global::Kind value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        public static bool TryParse(string name, out global::Kind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                value = default;
                return false;
            }

            var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

            switch (name)
            {
                case string s when s.Equals(Names.None, stringComparison):
                {
                    value = global::Kind.None;
                    return true;
                }

                case string s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (global::Kind)underlyingValue;
                    return true;
                }

                default:
                {
                    value = default;
                    return false;
                }
            }
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::Kind value)
            => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::Kind value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::Kind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
        {
            if (name.IsEmpty)
            {
                value = default;
                return false;
            }

            var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

            switch (name)
            {
                case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.None), stringComparison):
                {
                    value = global::Kind.None;
                    return true;
                }

                case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (global::Kind)underlyingValue;
                    return true;
                }

                default:
                {
                    value = default;
                    return false;
                }
            }
        }

        public static bool IsDefined(global::Kind value)
            => value switch
            {
                global::Kind.None => true,
                _ => false,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool IsNameDefined(string name, global::Kind _)
            => IsNameDefined(name, default(global::Kind), allowMatchingMetadataAttribute: false);

        public static bool IsNameDefined(string name, global::Kind _, bool allowMatchingMetadataAttribute)
        {
            return name switch
            {
                Names.None => true,
                _ => false,
            };
        }

    }

#region    NAMES
#endregion =====

    static partial class KindExtensions// Names
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Names
        {
            public const string None = nameof(global::Kind.None);

            private static readonly string[] s_names = new string[]
            {
                None,
            };

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static string Get(global::Kind value) => Get(value, true);

            public static string Get(global::Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::Kind.None => None,
                    _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
                };
        }

    }

#region    DISPLAY NAMES
#endregion =============

    static partial class KindExtensions// DisplayNames
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class DisplayNames
        {
            public const string None = Names.None;

            private static readonly string[] s_names = new string[]
            {
                None,
            };

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static string Get(global::Kind value) => Get(value, true);

            public static string Get(global::Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::Kind.None => None,
                    _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
                };
        }

    }

#region    FIXED NAMES
#endregion ===========

    static partial class KindExtensions// FixedNames
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class FixedNames
        {
            public static g__UC.FixedString32Bytes None
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => (g__UC.FixedString32Bytes)Names.None;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

            public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            {
                var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(KindExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(None);
                return names;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__UC.FixedString32Bytes Get(global::Kind value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(global::Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::Kind.None => None,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }

#region    FIXED DISPLAY NAMES
#endregion ===================

    static partial class KindExtensions// FixedDisplayNames
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public static partial class FixedDisplayNames
        {
            public static g__UC.FixedString32Bytes None
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => (g__UC.FixedString32Bytes)DisplayNames.None;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
                => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

            public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            {
                var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(KindExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(None);
                return names;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__UC.FixedString32Bytes Get(global::Kind value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(global::Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::Kind.None => None,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }

}

partial struct Id { } // IdKindExtensions

#region    INTERFACE
#endregion =========

static partial class Id_IdKindExtensions { } // IId_IdKindExtensions

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::Id.IdKind), typeof(IId_IdKindExtensions), typeof(Id_IdKindExtensions), typeof(Id_IdKindExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
public partial interface IId_IdKindExtensions
    : g__ETEE.IEnumExtensions<Id_IdKindExtended, global::Id.IdKind, byte>
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
{
}

#region    EXTENDED STRUCT
#endregion ===============

static partial class Id_IdKindExtensions { } // Id_IdKindExtended

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::Id.IdKind), typeof(IId_IdKindExtensions), typeof(Id_IdKindExtensions), typeof(Id_IdKindExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
public readonly partial struct Id_IdKindExtended : IId_IdKindExtensions
    , g__S.IEquatable<Id_IdKindExtended>
    , g__S.IComparable<Id_IdKindExtended>
{
    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly global::Id.IdKind _value;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly byte _underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id_IdKindExtended(global::Id.IdKind value) : this()
    {
        _value = value;
    }

    public global::Id.IdKind Value
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    public byte UnderlyingValue
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => _underlyingValue;
    }

    public int Length
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => Id_IdKindExtensions.Length;
    }

    public bool IsDefined
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => Id_IdKindExtensions.IsDefined(_value);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id_IdKindExtended Create(global::Id.IdKind value) => new Id_IdKindExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id_IdKindExtended CreateFromUnderlyingValue(byte value) => new Id_IdKindExtended((global::Id.IdKind)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToStringFast(bool emptyIfUndefined) => Id_IdKindExtensions.ToStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString() => ToDisplayString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayString(bool emptyIfUndefined) => Id_IdKindExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast() => ToDisplayStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToDisplayStringFast(bool emptyIfUndefined) => Id_IdKindExtensions.ToDisplayStringFast(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out Id_IdKindExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out Id_IdKindExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string name, out Id_IdKindExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = Id_IdKindExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new Id_IdKindExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out Id_IdKindExtended value) => TryParse(name, out value, false, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out Id_IdKindExtended value, bool ignoreCase) => TryParse(name, out value, ignoreCase, false);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(g__S.ReadOnlySpan<char> name, out Id_IdKindExtended value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        var result = Id_IdKindExtensions.TryParse(name, out var enumValue, ignoreCase, allowMatchingMetadataAttribute);
        value = new Id_IdKindExtended(enumValue);
        return result;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString() => ToFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToFixedString(bool emptyIfUndefined) => Id_IdKindExtensions.ToFixedString(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString() => ToDisplayFixedString(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public g__UC.FixedString32Bytes ToDisplayFixedString(bool emptyIfUndefined) => Id_IdKindExtensions.ToDisplayFixedString(_value, emptyIfUndefined);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public TFixedString ToFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public TFixedString ToDisplayFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToDisplayFixedString());

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
          g__S.Span<char> destination
        , out int charsWritten
    ) => Id_IdKindExtensions.TryFormat(_value, destination, out charsWritten);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(
          g__S.Span<char> destination
        , out int charsWritten
        , g__S.ReadOnlySpan<char> format
        , g__S.IFormatProvider provider = null
    ) => Id_IdKindExtensions.TryFormat(_value, destination, out charsWritten, format, provider);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name) => Id_IdKindExtensions.IsNameDefined(name, default(global::Id.IdKind));

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => Id_IdKindExtensions.IsNameDefined(name, default(global::Id.IdKind), allowMatchingMetadataAttribute);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int ToIndex() => Id_IdKindExtensions.FindIndex(_value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override string ToString() => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public string ToString(string format, g__S.IFormatProvider formatProvider) => ToStringFast(true);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => _underlyingValue.GetHashCode();

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Id_IdKindExtended other) => this._underlyingValue.CompareTo(other._underlyingValue);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool Equals(Id_IdKindExtended other) => this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj) => obj is Id_IdKindExtended other && this._underlyingValue == other._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator Id_IdKindExtended(global::Id.IdKind value) => new Id_IdKindExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Id_IdKindExtended left, Id_IdKindExtended right) => left._underlyingValue == right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Id_IdKindExtended left, Id_IdKindExtended right) => left._underlyingValue != right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Id_IdKindExtended left, Id_IdKindExtended right) => left._underlyingValue < right._underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Id_IdKindExtended left, Id_IdKindExtended right) => left._underlyingValue > right._underlyingValue;

}

#region    EXTENSIONS
#endregion ==========

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::Id.IdKind), typeof(IId_IdKindExtensions), typeof(Id_IdKindExtensions), typeof(Id_IdKindExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
public static partial class Id_IdKindExtensions // Id_IdKindExtensions
{
    /// <summary>
    /// The number of members in the enum.
    /// This is a non-distinct count of defined names.
    /// </summary>
    public const int Length = 1;

    /// <summary>
    /// Returns the string representation of the <see cref="global::Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::Id.IdKind value)
        => ToStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(this global::Id.IdKind value, bool emptyIfUndefined)
        => Names.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the string representation of the <see cref="global::Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::Id.IdKind value)
        => ToDisplayStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(this global::Id.IdKind value, bool emptyIfUndefined)
        => DisplayNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::Id.IdKind value)
        => ToFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(this global::Id.IdKind value, bool emptyIfUndefined)
        => FixedNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::Id.IdKind value)
        => ToDisplayFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(this global::Id.IdKind value, bool emptyIfUndefined)
        => FixedDisplayNames.Get(value, emptyIfUndefined);

    private static g__UC.FixedString32Bytes ToFixedString(byte value)
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, value);
        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsExtended(this global::Id.IdKind value)
        => new Id_IdKindExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this byte value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this sbyte value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this short value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this ushort value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this int value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this uint value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this long value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(this ulong value)
        => new Id_IdKindExtended((global::Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static byte ToUnderlyingValue(this global::Id.IdKind value)
        => (byte)value;

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::Id.IdKind value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this string name, out global::Id.IdKind value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this string name, out global::Id.IdKind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case string s when s.Equals(Names.Kind, stringComparison):
            {
                value = global::Id.IdKind.Kind;
                return true;
            }

            case string s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::Id.IdKind)underlyingValue;
                return true;
            }

            default:
            {
                value = default;
                return false;
            }
        }
    }

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::Id.IdKind value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::Id.IdKind value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(this g__S.ReadOnlySpan<char> name, out global::Id.IdKind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
    {
        if (name.IsEmpty)
        {
            value = default;
            return false;
        }

        var stringComparison = ignoreCase ? g__S.StringComparison.OrdinalIgnoreCase : g__S.StringComparison.Ordinal;

        switch (name)
        {
            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Kind), stringComparison):
            {
                value = global::Id.IdKind.Kind;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::Id.IdKind)underlyingValue;
                return true;
            }

            default:
            {
                value = default;
                return false;
            }
        }
    }

    /// <summary>
    /// Returns a boolean telling whether the given enum value exists in the enumeration.
    /// </summary>
    /// <param name="value">The value to check if it's defined</param>
    /// <returns><c>true</c> if the value exists in the enumeration, <c>false</c> otherwise</returns>
    public static bool IsDefined(this global::Id.IdKind value)
        => value switch
        {
            global::Id.IdKind.Kind => true,
            _ => false,
        };

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool IsNameDefined(this string name, global::Id.IdKind _)
        => IsNameDefined(name, default(global::Id.IdKind), allowMatchingMetadataAttribute: false);

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
    /// or if a member decorated with a <c>[Display]</c> attribute
    /// with the required name exists.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
    /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
    public static bool IsNameDefined(this string name, global::Id.IdKind _, bool allowMatchingMetadataAttribute)
    {
        return name switch
        {
            Names.Kind => true,
            _ => false,
        };
    }

    public static bool TryFormat(
          this global::Id.IdKind value
        , g__S.Span<char> destination
        , out int charsWritten
    )
    {
        if (IsDefined(value))
        {
            return g__ETCol.EncosyFixedStringExtensions.TryFormat(ToFixedString(value), destination, out charsWritten);
        }

        return g__ETCol.EncosyFixedStringExtensions.TryFormat(g__ETCol.EncosyFixedStringExtensions.ToFixedString(ToUnderlyingValue(value)), destination, out charsWritten);
    }

    public static bool TryFormat(
          this global::Id.IdKind value
        , g__S.Span<char> destination
        , out int charsWritten
        , g__S.ReadOnlySpan<char> format
        , g__S.IFormatProvider provider = null
    )
    {
        if (IsDefined(value))
        {
            return g__ETCol.EncosyFixedStringExtensions.TryFormat(ToFixedString(value), destination, out charsWritten);
        }

        if (ToUnderlyingValue(value).TryFormat(destination, out var chars, format, provider))
        {
            charsWritten = chars;
             return true;
        }

        charsWritten = 0;
         return false;
    }

    /// <summary>
    /// Finds the index for a given enum value in the enumeration.
    /// </summary>
    /// <param name="value">The value to find the index for</param>
    /// <returns>The zero-based index if the enum value exists in the enumeration, otherwise -1.</returns>
    public static int FindIndex(this global::Id.IdKind value)
        => value switch
        {
            global::Id.IdKind.Kind => 0,
            _ => -1,
        };

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Values
    {
        private static readonly global::Id.IdKind[] s_values = new global::Id.IdKind[]
        {
            global::Id.IdKind.Kind,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<global::Id.IdKind> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<global::Id.IdKind> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<global::Id.IdKind> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<global::Id.IdKind>(s_values, allocator);
    }

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class UnderlyingValues
    {
        private static readonly byte[] s_values = new byte[]
        {
            ToUnderlyingValue(global::Id.IdKind.Kind),
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<byte> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<byte> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<byte> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<byte>(s_values, allocator);
    }

}

#region    NAMES
#endregion =====

static partial class Id_IdKindExtensions// Names
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Names
    {
        public const string Kind = nameof(global::Id.IdKind.Kind);

        private static readonly string[] s_names = new string[]
        {
            Kind,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::Id.IdKind value) => Get(value, true);

        public static string Get(global::Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::Id.IdKind.Kind => Kind,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    DISPLAY NAMES
#endregion =============

static partial class Id_IdKindExtensions// DisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class DisplayNames
    {
        public const string Kind = Names.Kind;

        private static readonly string[] s_names = new string[]
        {
            Kind,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::Id.IdKind value) => Get(value, true);

        public static string Get(global::Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::Id.IdKind.Kind => Kind,
                _ => emptyIfUndefined ? string.Empty : ToUnderlyingValue(value).ToString(),
            };
    }

}

#region    FIXED NAMES
#endregion ===========

static partial class Id_IdKindExtensions// FixedNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedNames
    {
        public static g__UC.FixedString32Bytes Kind
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Kind;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(Id_IdKindExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Kind);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::Id.IdKind value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::Id.IdKind.Kind => Kind,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}

#region    FIXED DISPLAY NAMES
#endregion ===================

static partial class Id_IdKindExtensions// FixedDisplayNames
{
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class FixedDisplayNames
    {
        public static g__UC.FixedString32Bytes Kind
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Kind;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(Id_IdKindExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Kind);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::Id.IdKind value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::Id.IdKind.Kind => Kind,
                _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
            };
    }

}

partial struct Id { } // IdEnumeration

[g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
public static partial class IdEnumeration
{
    /// <summary>
    /// Total member count of the following enums
    /// <list type="bullet">
    /// <item><see cref="global::Kind"/> (1)</item>
    /// </list>
    /// </summary>
    public const int Length = 1;

    public static void CopyTo(g__S.Span<Id> dest)
    {
        if (dest.Length < Length) throw new g__S.ArgumentOutOfRangeException(nameof(dest));

        dest[0] = global::Kind.None;
    }

    public static bool TryCopyTo(g__S.Span<Id> dest)
    {
        if (dest.Length < Length) return false;

        dest[0] = global::Kind.None;

        return true;
    }

    public static void AddTo<TCollection>([g__SDCA.NotNull] TCollection dest)
        where TCollection : g__SCG.ICollection<Id>
    {
        g__ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(dest);

        dest.Add(global::Kind.None);
    }

}



