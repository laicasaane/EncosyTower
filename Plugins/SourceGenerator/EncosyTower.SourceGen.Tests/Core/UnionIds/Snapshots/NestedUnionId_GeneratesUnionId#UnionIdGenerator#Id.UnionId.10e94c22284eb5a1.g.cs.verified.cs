#pragma warning disable 0219

using System;
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


namespace TestProject
{
    partial class Outer 
    {



#pragma warning disable

[g__SRCS.Union]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit, Size = 8)]
[g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
[g__SCM.TypeConverter(typeof(TypeConverter))]
partial struct Id : g__ETUI.IUnionId<ulong, Id>, g__SRCS.IUnion, g__ET.IHasValue
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.ITryParse<Id>
    , g__ETCon.ITryParseSpan<Id>
    , g__S.IEquatable<global::TestProject.Code>
{
    public const char SEPARATOR = '-';

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly ulong _raw;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly uint IdUnsigned;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly int IdSigned;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly global::TestProject.Kind Id_Kind;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    public readonly global::TestProject.Code Id_Code;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(7)]
    public readonly IdKind Kind;

    private Id(ulong value) : this()
    {
        _raw = value;
    }

    public Id(IdKind kind, uint id) : this()
    {
        IdUnsigned = id;
        Kind = kind;
    }

    public Id(IdKind kind, int id) : this()
    {
        IdSigned = id;
        Kind = kind;
    }

    public Id(global::TestProject.Kind id) : this()
    {
        Id_Kind = id;
        Kind = IdKind.Kind;
    }

    public Id(global::TestProject.Code id) : this()
    {
        Id_Code = id;
        Kind = IdKind.Code;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator Id(global::TestProject.Kind id)
    {
        return new(id);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator Id(global::TestProject.Code id)
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
    public Id(string kind, uint id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)
    {
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id(string kind, int id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this(g__S.MemoryExtensions.AsSpan(kind), id, ignoreCase, allowMatchingMetadataAttribute)
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

            case IdKind.Code:
            {
                var idResult = TryParse_Code(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                Id_Code = idValue;
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

            case IdKind.Code:
            {
                var idResult = TryParse_Code(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                Id_Code = idValue;
                break;
            }

        }

        Kind = kindValue;
    }

    public Id(g__S.ReadOnlySpan<char> kind, uint id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
    {
        IdUnsigned = id;

        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            kindValue = default;
        }

        Kind = kindValue;
    }

    public Id(g__S.ReadOnlySpan<char> kind, int id, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true) : this()
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
            IdKind.Code => Id_Code,
            _ => null,
        };
    }

    public bool HasValue
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        get => Id_IdKindExtensions.IsDefined(Kind);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static explicit operator global::TestProject.Kind(Id value)
    {
        return value.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.Kind>());
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static explicit operator global::TestProject.Code(Id value)
    {
        return value.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.Code>());
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly global::TestProject.Kind GetValueOrThrow(g__ET.T<global::TestProject.Kind> _)
    {
        ThrowIfUncastable(Kind, IdKind.Kind);

        return Id_Kind;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly global::TestProject.Kind GetValueOrDefault(g__ET.T<global::TestProject.Kind> _ = default, global::TestProject.Kind @default = default)
    {
        if (IsCastable(Kind, IdKind.Kind))
        {
            return Id_Kind;
        }

        return @default;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out global::TestProject.Kind value)
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
    public readonly global::TestProject.Code GetValueOrThrow(g__ET.T<global::TestProject.Code> _)
    {
        ThrowIfUncastable(Kind, IdKind.Code);

        return Id_Code;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly global::TestProject.Code GetValueOrDefault(g__ET.T<global::TestProject.Code> _ = default, global::TestProject.Code @default = default)
    {
        if (IsCastable(Kind, IdKind.Code))
        {
            return Id_Code;
        }

        return @default;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(out global::TestProject.Code value)
    {
        if (Kind == IdKind.Code)
        {
            value = Id_Code;
            return true;
        }

        value = default;
        return false;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    private static partial bool TryParse_Code(g__S.ReadOnlySpan<char> str, out global::TestProject.Code value, bool ignoreCase, bool allowMatchingMetadataAttribute);

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
    public bool TryParse(string kind, uint id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        return TryParse(g__S.MemoryExtensions.AsSpan(kind), id, out result, ignoreCase, allowMatchingMetadataAttribute);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool TryParse(string kind, int id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
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

            case IdKind.Code:
            {
                var idResult = TryParse_Code(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);

                if (idResult)
                {
                    result = new((global::TestProject.Code)idValue);
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

            case IdKind.Code:
            {
                var idResult = TryParse_Code(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);

                if (idResult)
                {
                    result = new((global::TestProject.Code)idValue);
                    return true;
                }

                goto FAILED;
            }

        }

        FAILED:
        result = default;
        return false;
    }

    public bool TryParse(g__S.ReadOnlySpan<char> kind, uint id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
    {
        if (Id_IdKindExtensions.TryParse(kind, out var kindValue, ignoreCase, allowMatchingMetadataAttribute) == false)
        {
            result = default;
            return false;
        }

        result = new(kindValue, id);
        return true;
    }

    public bool TryParse(g__S.ReadOnlySpan<char> kind, int id, out Id result, bool ignoreCase = true, bool allowMatchingMetadataAttribute = true)
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
        if (g__ETCol.EncosyFixedStringExtensions.TryFormat(Id_IdKindExtensions.ToFixedString(Kind), destination, out var kindCharsWritten) == false)
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
    public static explicit operator ulong(Id value)
    {
        return value._raw;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static implicit operator Id(ulong value)
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

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(global::TestProject.Code other)
    {
        return Kind == IdKind.Code && Id_Code.Equals(other);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Id left, global::TestProject.Code right)
    {
        return left.Equals(right);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(global::TestProject.Code left, Id right)
    {
        return right.Equals(left);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Id left, global::TestProject.Code right)
    {
        return !left.Equals(right);
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(global::TestProject.Code left, Id right)
    {
        return !right.Equals(left);
    }

    public readonly override string ToString()
    {
        return Kind switch
        {
            IdKind.Kind => $"{Id_IdKindExtensions.Names.Kind}{SEPARATOR}{KindExtensions.ToStringFast(Id_Kind)}",
            IdKind.Code => $"{Id_IdKindExtensions.Names.Code}{SEPARATOR}{Id_Code}",
            _ => $"{Id_IdKindExtensions.ToUnderlyingValue(Kind).ToString()}{SEPARATOR}{IdUnsigned}",
        };

    }

    public readonly string ToDisplayString()
    {
        return Kind switch
        {
            IdKind.Kind => $"{Id_IdKindExtensions.DisplayNames.Kind}{SEPARATOR}{KindExtensions.ToDisplayStringFast(Id_Kind)}",
            IdKind.Code => $"{Id_IdKindExtensions.ToDisplayStringFast(Kind)}{SEPARATOR}{Id_Code}",
            _ => $"{Id_IdKindExtensions.ToUnderlyingValue(Kind).ToString()}{SEPARATOR}{IdUnsigned}",
        };

    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public readonly TFixedString ToFixedString<TFixedString>()
        where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

    public readonly g__UC.FixedString32Bytes ToFixedString()
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, Id_IdKindExtensions.ToFixedString(Kind, false));
        g__UC.FixedStringMethods.Append(ref fs, '-');

        switch (Kind)
        {
            case IdKind.Kind:
            {
                g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToFixedString(Id_Kind, false));
                break;
            }

            case IdKind.Code:
            {
                Append_Code(ref fs, Id_Code, false);
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
        g__UC.FixedStringMethods.Append(ref fs, Id_IdKindExtensions.ToDisplayFixedString(Kind, false));
        g__UC.FixedStringMethods.Append(ref fs, '-');

        switch (Kind)
        {
            case IdKind.Kind:
            {
                g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToDisplayFixedString(Id_Kind, false));
                break;
            }

            case IdKind.Code:
            {
                Append_Code(ref fs, Id_Code, true);
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
            IdKind.Code => Id_Code.ToString(),
            _ => IdUnsigned.ToString(),
        };
    }

    public readonly string GetIdDisplayStringFast()
    {
        return Kind switch
        {
            IdKind.Kind => KindExtensions.ToDisplayStringFast(Id_Kind, false),
            IdKind.Code => Id_Code.ToString(),
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

            case IdKind.Code:
            {
                Append_Code(ref fs, Id_Code, false);
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

            case IdKind.Code:
            {
                Append_Code(ref fs, Id_Code, true);
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

            case IdKind.Code:
            {
                goto default;
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

            case IdKind.Code:
            {
                goto default;
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

            case IdKind.Code:
            {
                goto default;
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

            case IdKind.Code:
            {
                goto default;
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

            case IdKind.Code:
            {
                goto default;
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

            case IdKind.Code:
            {
                goto default;
            }

            default:
            {
                result = g__UC.CollectionHelper.CreateNativeArray<g__UC.FixedString32Bytes>(0, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
                return false;
            }

        }
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    private static partial void Append_Code(ref g__UC.FixedString32Bytes fs, global::TestProject.Code value, bool isDisplay);

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
        /// <see cref="global::TestProject.Kind"/>
        Kind = 0,

        /// <see cref="global::TestProject.Code"/>
        Code = 1,

    }

}

partial struct Id // Serializable
{
    [g__SRCS.Union]
    [g__S.Serializable]
    [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    public partial struct Serializable : g__SRCS.IUnion, g__ETUI.ISerializableUnionId<ulong, Id, Serializable>
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

        public Serializable(global::TestProject.Kind id) : this()
        {
            Kind = IdKind.Kind;
            Id = (long)new Id(id).IdUnsigned;
        }

        public Serializable(global::TestProject.Code id) : this()
        {
            Kind = IdKind.Code;
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

                case IdKind.Code:
                {
                    var idResult = TryParse_Code(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                    Id_Code = idValue;
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

                case IdKind.Code:
                {
                    var idResult = TryParse_Code(id, out var idValue, ignoreCase, allowMatchingMetadataAttribute);
                    Id_Code = idValue;
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
        public global::TestProject.Kind Id_Kind
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => new Id(Kind, (uint)Id).Id_Kind;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set => Id = (long)new Id(value).IdUnsigned;
        }

        [field: g__S.NonSerialized]
        public global::TestProject.Code Id_Code
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => new Id(Kind, (uint)Id).Id_Code;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set => Id = (long)new Id(value).IdUnsigned;
        }

        public object Value
        {
            get => Kind switch
            {
                IdKind.Kind => Id_Kind,
                IdKind.Code => Id_Code,
                _ => null,
            };
        }

        public bool HasValue
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => Id_IdKindExtensions.IsDefined(Kind);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator global::TestProject.Kind(Serializable value)
        {
            return value.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.Kind>());
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static explicit operator global::TestProject.Code(Serializable value)
        {
            return value.GetValueOrThrow(g__ET.GenericT.T<global::TestProject.Code>());
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly global::TestProject.Kind GetValueOrThrow(g__ET.T<global::TestProject.Kind> _)
        {
            ThrowIfUncastable(Kind, IdKind.Kind);

            return Id_Kind;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly global::TestProject.Kind GetValueOrDefault(g__ET.T<global::TestProject.Kind> _ = default, global::TestProject.Kind @default = default)
        {
            if (IsCastable(Kind, IdKind.Kind))
            {
                return Id_Kind;
            }

            return @default;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out global::TestProject.Kind value)
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
        public readonly global::TestProject.Code GetValueOrThrow(g__ET.T<global::TestProject.Code> _)
        {
            ThrowIfUncastable(Kind, IdKind.Code);

            return Id_Code;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly global::TestProject.Code GetValueOrDefault(g__ET.T<global::TestProject.Code> _ = default, global::TestProject.Code @default = default)
        {
            if (IsCastable(Kind, IdKind.Code))
            {
                return Id_Code;
            }

            return @default;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(out global::TestProject.Code value)
        {
            if (Kind == IdKind.Code)
            {
                value = Id_Code;
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

                case IdKind.Code:
                {
                    result = new(Id_Code);
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
            return ((ulong)this).CompareTo((ulong)other);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            return ((ulong)this).GetHashCode();
        }

        public readonly override string ToString()
        {
            return Kind switch
            {
                IdKind.Kind => $"{Id_IdKindExtensions.Names.Kind}{SEPARATOR}{KindExtensions.ToStringFast(Id_Kind)}",
                IdKind.Code => $"{Id_IdKindExtensions.Names.Code}{SEPARATOR}{Id_Code}",
                _ => $"{Id_IdKindExtensions.ToUnderlyingValue(Kind).ToString()}{SEPARATOR}{Id}",
            };

        }

        public readonly string ToDisplayString()
        {
            return Kind switch
            {
                IdKind.Kind => $"{Id_IdKindExtensions.DisplayNames.Kind}{SEPARATOR}{KindExtensions.ToDisplayStringFast(Id_Kind)}",
                IdKind.Code => $"{Id_IdKindExtensions.ToDisplayStringFast(Kind)}{SEPARATOR}{Id_Code}",
                _ => $"{Id_IdKindExtensions.ToUnderlyingValue(Kind).ToString()}{SEPARATOR}{Id}",
            };

        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public readonly TFixedString ToFixedString<TFixedString>()
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
            => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

        public readonly g__UC.FixedString32Bytes ToFixedString()
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, Id_IdKindExtensions.ToFixedString(Kind, false));
            g__UC.FixedStringMethods.Append(ref fs, '-');

            switch (Kind)
            {
                case IdKind.Kind:
                {
                    g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToFixedString(Id_Kind, false));
                    break;
                }

                case IdKind.Code:
                {
                    Append_Code(ref fs, Id_Code, false);
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
            g__UC.FixedStringMethods.Append(ref fs, Id_IdKindExtensions.ToDisplayFixedString(Kind, false));
            g__UC.FixedStringMethods.Append(ref fs, '-');

            switch (Kind)
            {
                case IdKind.Kind:
                {
                    g__UC.FixedStringMethods.Append(ref fs, KindExtensions.ToDisplayFixedString(Id_Kind, false));
                    break;
                }

                case IdKind.Code:
                {
                    Append_Code(ref fs, Id_Code, true);
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
                IdKind.Code => Id_Code.ToString(),
                _ => Id.ToString(),
            };
        }

        public readonly string GetIdDisplayStringFast()
        {
            return Kind switch
            {
                IdKind.Kind => KindExtensions.ToDisplayStringFast(Id_Kind, false),
                IdKind.Code => Id_Code.ToString(),
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

                case IdKind.Code:
                {
                    Append_Code(ref fs, Id_Code, false);
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

                case IdKind.Code:
                {
                    Append_Code(ref fs, Id_Code, true);
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
        public static explicit operator ulong(Serializable value)
        {
            return new Union { id = (uint)value.Id, kind = value.Kind }.raw;
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
            return ((ulong)left) < ((ulong)right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Serializable left, Serializable right)
        {
            return ((ulong)left) <= ((ulong)right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Serializable left, Serializable right)
        {
            return ((ulong)left) > ((ulong)right);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Serializable left, Serializable right)
        {
            return ((ulong)left) >= ((ulong)right);
        }

        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        private struct Union
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public ulong raw;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public uint id;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(7)]
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
        public static string ToStringFast(global::TestProject.Kind value)
            => ToStringFast(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToStringFast(global::TestProject.Kind value, bool emptyIfUndefined)
            => Names.Get(value, emptyIfUndefined);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToDisplayStringFast(global::TestProject.Kind value)
            => ToDisplayStringFast(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string ToDisplayStringFast(global::TestProject.Kind value, bool emptyIfUndefined)
            => DisplayNames.Get(value, emptyIfUndefined);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToFixedString(global::TestProject.Kind value)
            => ToFixedString(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToFixedString(global::TestProject.Kind value, bool emptyIfUndefined)
            => FixedNames.Get(value, emptyIfUndefined);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(global::TestProject.Kind value)
            => ToDisplayFixedString(value, true);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes ToDisplayFixedString(global::TestProject.Kind value, bool emptyIfUndefined)
            => FixedDisplayNames.Get(value, emptyIfUndefined);

        private static g__UC.FixedString32Bytes ToFixedString(byte value)
        {
            var fs = new g__UC.FixedString32Bytes();
            g__UC.FixedStringMethods.Append(ref fs, value);
            return fs;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static byte ToUnderlyingValue(global::TestProject.Kind value)
            => (byte)value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string name, out global::TestProject.Kind value)
            => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(string name, out global::TestProject.Kind value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        public static bool TryParse(string name, out global::TestProject.Kind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
                    value = global::TestProject.Kind.None;
                    return true;
                }

                case string s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (global::TestProject.Kind)underlyingValue;
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
        public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::TestProject.Kind value)
            => TryParse(name, out value, false, false);

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::TestProject.Kind value, bool ignoreCase)
            => TryParse(name, out value, ignoreCase, false);

        public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::TestProject.Kind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
                    value = global::TestProject.Kind.None;
                    return true;
                }

                case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
                {
                    value = (global::TestProject.Kind)underlyingValue;
                    return true;
                }

                default:
                {
                    value = default;
                    return false;
                }
            }
        }

        public static bool IsDefined(global::TestProject.Kind value)
            => value switch
            {
                global::TestProject.Kind.None => true,
                _ => false,
            };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static bool IsNameDefined(string name, global::TestProject.Kind _)
            => IsNameDefined(name, default(global::TestProject.Kind), allowMatchingMetadataAttribute: false);

        public static bool IsNameDefined(string name, global::TestProject.Kind _, bool allowMatchingMetadataAttribute)
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
            public const string None = nameof(global::TestProject.Kind.None);

            private static readonly string[] s_names = new string[]
            {
                None,
            };

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public static string Get(global::TestProject.Kind value) => Get(value, true);

            public static string Get(global::TestProject.Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::TestProject.Kind.None => None,
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
            public static string Get(global::TestProject.Kind value) => Get(value, true);

            public static string Get(global::TestProject.Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::TestProject.Kind.None => None,
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
            public static g__UC.FixedString32Bytes Get(global::TestProject.Kind value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(global::TestProject.Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::TestProject.Kind.None => None,
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
            public static g__UC.FixedString32Bytes Get(global::TestProject.Kind value)
                => Get(value, true);

            public static g__UC.FixedString32Bytes Get(global::TestProject.Kind value, bool emptyIfUndefined)
                => value switch
                {
                    global::TestProject.Kind.None => None,
                    _ => emptyIfUndefined ? default : ToFixedString(ToUnderlyingValue(value)),
                };
        }

    }

}

partial struct Id { } // IdKindExtensions

#region    INTERFACE
#endregion =========

static partial class Id_IdKindExtensions { } // IId_IdKindExtensions

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Outer.Id.IdKind), typeof(IId_IdKindExtensions), typeof(Id_IdKindExtensions), typeof(Id_IdKindExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
public partial interface IId_IdKindExtensions
    : g__ETEE.IEnumExtensions<Id_IdKindExtended, global::TestProject.Outer.Id.IdKind, byte>
    , g__ETCon.IToFixedString
    , g__ETCon.IToDisplayFixedString
    , g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
    , g__ETCon.IToDisplayFixedString<g__UC.FixedString32Bytes>
{
}

#region    EXTENDED STRUCT
#endregion ===============

static partial class Id_IdKindExtensions { } // Id_IdKindExtended

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Outer.Id.IdKind), typeof(IId_IdKindExtensions), typeof(Id_IdKindExtensions), typeof(Id_IdKindExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
public readonly partial struct Id_IdKindExtended : IId_IdKindExtensions
    , g__S.IEquatable<Id_IdKindExtended>
    , g__S.IComparable<Id_IdKindExtended>
{
    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly global::TestProject.Outer.Id.IdKind _value;

    // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
    [g__SRIS.FieldOffset(0)]
    private readonly byte _underlyingValue;

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id_IdKindExtended(global::TestProject.Outer.Id.IdKind value) : this()
    {
        _value = value;
    }

    public global::TestProject.Outer.Id.IdKind Value
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
    public Id_IdKindExtended Create(global::TestProject.Outer.Id.IdKind value) => new Id_IdKindExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public Id_IdKindExtended CreateFromUnderlyingValue(byte value) => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)value);

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
    public bool IsNameDefined(string name) => Id_IdKindExtensions.IsNameDefined(name, default(global::TestProject.Outer.Id.IdKind));

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public bool IsNameDefined(string name, bool allowMatchingMetadataAttribute) => Id_IdKindExtensions.IsNameDefined(name, default(global::TestProject.Outer.Id.IdKind), allowMatchingMetadataAttribute);

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
    public static implicit operator Id_IdKindExtended(global::TestProject.Outer.Id.IdKind value) => new Id_IdKindExtended(value);

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

[g__ETEESG.GeneratedEnumExtensionsFor(typeof(global::TestProject.Outer.Id.IdKind), typeof(IId_IdKindExtensions), typeof(Id_IdKindExtensions), typeof(Id_IdKindExtended))]
[g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
[g__SDCA.ExcludeFromCodeCoverage]
public static partial class Id_IdKindExtensions // Id_IdKindExtensions
{
    /// <summary>
    /// The number of members in the enum.
    /// This is a non-distinct count of defined names.
    /// </summary>
    public const int Length = 2;

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(global::TestProject.Outer.Id.IdKind value)
        => ToStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToStringFast(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
        => Names.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(global::TestProject.Outer.Id.IdKind value)
        => ToDisplayStringFast(value, true);

    /// <summary>
    /// Returns the string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static string ToDisplayStringFast(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
        => DisplayNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(global::TestProject.Outer.Id.IdKind value)
        => ToFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToFixedString(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
        => FixedNames.Get(value, emptyIfUndefined);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(global::TestProject.Outer.Id.IdKind value)
        => ToDisplayFixedString(value, true);

    /// <summary>
    /// Returns the fixed string representation of the <see cref="global::TestProject.Outer.Id.IdKind"/> value.
    /// If the attribute is decorated with a <c>[Display]</c> attribute, then
    /// uses the provided value. Otherwise uses the name of the member, equivalent to
    /// calling <c>ToString()</c> on <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to retrieve the string value for</param>
    /// <param name="emptyIfUndefined">If <c>true</c>, returns an empty string for undefined values; otherwise, returns the string representation of the value</param>
    /// <returns>The fixed string representation of the value</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static g__UC.FixedString32Bytes ToDisplayFixedString(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
        => FixedDisplayNames.Get(value, emptyIfUndefined);

    private static g__UC.FixedString32Bytes ToFixedString(byte value)
    {
        var fs = new g__UC.FixedString32Bytes();
        g__UC.FixedStringMethods.Append(ref fs, value);
        return fs;
    }

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsExtended(global::TestProject.Outer.Id.IdKind value)
        => new Id_IdKindExtended(value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(byte value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(sbyte value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(short value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(ushort value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(int value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(uint value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(long value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static Id_IdKindExtended AsId_IdKindExtended(ulong value)
        => new Id_IdKindExtended((global::TestProject.Outer.Id.IdKind)(byte)value);

    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static byte ToUnderlyingValue(global::TestProject.Outer.Id.IdKind value)
        => (byte)value;

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Outer.Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Outer.Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Outer.Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string name, out global::TestProject.Outer.Id.IdKind value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Outer.Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Outer.Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Outer.Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(string name, out global::TestProject.Outer.Id.IdKind value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Outer.Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Outer.Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Outer.Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string name, out global::TestProject.Outer.Id.IdKind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
                value = global::TestProject.Outer.Id.IdKind.Kind;
                return true;
            }

            case string s when s.Equals(Names.Code, stringComparison):
            {
                value = global::TestProject.Outer.Id.IdKind.Code;
                return true;
            }

            case string s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::TestProject.Outer.Id.IdKind)underlyingValue;
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
    /// an <see cref="global::TestProject.Outer.Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Outer.Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Outer.Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::TestProject.Outer.Id.IdKind value)
        => TryParse(name, out value, false, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Outer.Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Outer.Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Outer.Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::TestProject.Outer.Id.IdKind value, bool ignoreCase)
        => TryParse(name, out value, ignoreCase, false);

    /// <summary>
    /// Converts the string representation of the name or numeric value of
    /// an <see cref="global::TestProject.Outer.Id.IdKind" /> to the equivalent instance.
    /// The return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="name">The case-sensitive string representation of the enumeration name or underlying value to convert</param>
    /// <param name="value">When this method returns, contains an object of type 
    /// <see cref="global::TestProject.Outer.Id.IdKind" /> whose
    /// value is represented by <paramref name="value"/> if the parse operation succeeds.
    /// If the parse operation fails, contains the default value of the underlying type
    /// of <see cref="global::TestProject.Outer.Id.IdKind" />. This parameter is passed uninitialized.</param>
    /// <param name="ignoreCase"><c>true</c> to read value in case insensitive mode; <c>false</c> to read value in case sensitive mode.</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value included in metadata attributes such as
    /// <c>[Display]</c> attribute when parsing, otherwise only considers the member names.</param>
    /// <returns><c>true</c> if the value parameter was converted successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(g__S.ReadOnlySpan<char> name, out global::TestProject.Outer.Id.IdKind value, bool ignoreCase, bool allowMatchingMetadataAttribute)
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
                value = global::TestProject.Outer.Id.IdKind.Kind;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when g__S.MemoryExtensions.Equals(s, g__S.MemoryExtensions.AsSpan(Names.Code), stringComparison):
            {
                value = global::TestProject.Outer.Id.IdKind.Code;
                return true;
            }

            case g__S.ReadOnlySpan<char> s when byte.TryParse(name, out var underlyingValue):
            {
                value = (global::TestProject.Outer.Id.IdKind)underlyingValue;
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
    public static bool IsDefined(global::TestProject.Outer.Id.IdKind value)
        => value switch
        {
            global::TestProject.Outer.Id.IdKind.Kind => true,
            global::TestProject.Outer.Id.IdKind.Code => true,
            _ => false,
        };

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, <c>false</c> otherwise</returns>
    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
    public static bool IsNameDefined(string name, global::TestProject.Outer.Id.IdKind _)
        => IsNameDefined(name, default(global::TestProject.Outer.Id.IdKind), allowMatchingMetadataAttribute: false);

    /// <summary>
    /// Returns a boolean telling whether an enum with the given name exists in the enumeration,
    /// or if a member decorated with a <c>[Display]</c> attribute
    /// with the required name exists.
    /// </summary>
    /// <param name="name">The name to check if it's defined</param>
    /// <param name="allowMatchingMetadataAttribute">If <c>true</c>, considers the value of metadata attributes, otherwise ignores them</param>
    /// <returns><c>true</c> if a member with the name exists in the enumeration, or a member is decorated
    /// with a <c>[Display]</c> attribute with the name, <c>false</c> otherwise</returns>
    public static bool IsNameDefined(string name, global::TestProject.Outer.Id.IdKind _, bool allowMatchingMetadataAttribute)
    {
        return name switch
        {
            Names.Kind => true,
            Names.Code => true,
            _ => false,
        };
    }

    public static bool TryFormat(
          global::TestProject.Outer.Id.IdKind value
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
          global::TestProject.Outer.Id.IdKind value
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
    public static int FindIndex(global::TestProject.Outer.Id.IdKind value)
        => value switch
        {
            global::TestProject.Outer.Id.IdKind.Kind => 0,
            global::TestProject.Outer.Id.IdKind.Code => 1,
            _ => -1,
        };

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class Values
    {
        private static readonly global::TestProject.Outer.Id.IdKind[] s_values = new global::TestProject.Outer.Id.IdKind[]
        {
            global::TestProject.Outer.Id.IdKind.Kind,
            global::TestProject.Outer.Id.IdKind.Code,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<global::TestProject.Outer.Id.IdKind> AsMemory() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<global::TestProject.Outer.Id.IdKind> AsSpan() => s_values;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<global::TestProject.Outer.Id.IdKind> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => g__UC.CollectionHelper.CreateNativeArray<global::TestProject.Outer.Id.IdKind>(s_values, allocator);
    }

    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.UnionIds.UnionIdGenerator", "0.1.8-preview.1")]
    [g__SDCA.ExcludeFromCodeCoverage]
    public static partial class UnderlyingValues
    {
        private static readonly byte[] s_values = new byte[]
        {
            ToUnderlyingValue(global::TestProject.Outer.Id.IdKind.Kind),
            ToUnderlyingValue(global::TestProject.Outer.Id.IdKind.Code),
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
        public const string Kind = nameof(global::TestProject.Outer.Id.IdKind.Kind);

        public const string Code = nameof(global::TestProject.Outer.Id.IdKind.Code);

        private static readonly string[] s_names = new string[]
        {
            Kind,
            Code,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::TestProject.Outer.Id.IdKind value) => Get(value, true);

        public static string Get(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Outer.Id.IdKind.Kind => Kind,
                global::TestProject.Outer.Id.IdKind.Code => Code,
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

        public const string Code = Names.Code;

        private static readonly string[] s_names = new string[]
        {
            Kind,
            Code,
        };

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlyMemory<string> AsMemory() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__S.ReadOnlySpan<string> AsSpan() => s_names;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static string Get(global::TestProject.Outer.Id.IdKind value) => Get(value, true);

        public static string Get(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Outer.Id.IdKind.Kind => Kind,
                global::TestProject.Outer.Id.IdKind.Code => Code,
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

        public static g__UC.FixedString32Bytes Code
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)Names.Code;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(Id_IdKindExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Kind);
            names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Code);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::TestProject.Outer.Id.IdKind value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Outer.Id.IdKind.Kind => Kind,
                global::TestProject.Outer.Id.IdKind.Code => Code,
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

        public static g__UC.FixedString32Bytes Code
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => (g__UC.FixedString32Bytes)DisplayNames.Code;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.NativeArray<g__UC.FixedString32Bytes> ToNativeArray(g__UC.AllocatorManager.AllocatorHandle allocator)
            => ToNativeArray<g__UC.FixedString32Bytes>(allocator);

        public static g__UC.NativeArray<TFixedString> ToNativeArray<TFixedString>(g__UC.AllocatorManager.AllocatorHandle  allocator)
            where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
        {
            var names = g__UC.CollectionHelper.CreateNativeArray<TFixedString>(Id_IdKindExtensions.Length, allocator, g__UC.NativeArrayOptions.UninitializedMemory);
            names[0] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Kind);
            names[1] = g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(Code);
            return names;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__UC.FixedString32Bytes Get(global::TestProject.Outer.Id.IdKind value)
            => Get(value, true);

        public static g__UC.FixedString32Bytes Get(global::TestProject.Outer.Id.IdKind value, bool emptyIfUndefined)
            => value switch
            {
                global::TestProject.Outer.Id.IdKind.Kind => Kind,
                global::TestProject.Outer.Id.IdKind.Code => Code,
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
    /// <item><see cref="global::TestProject.Kind"/> (1)</item>
    /// </list>
    /// </summary>
    public const int Length = 1;

    public static void CopyTo(g__S.Span<Id> dest)
    {
        if (dest.Length < Length) throw new g__S.ArgumentOutOfRangeException(nameof(dest));

        dest[0] = global::TestProject.Kind.None;
    }

    public static bool TryCopyTo(g__S.Span<Id> dest)
    {
        if (dest.Length < Length) return false;

        dest[0] = global::TestProject.Kind.None;

        return true;
    }

    public static void AddTo<TCollection>([g__SDCA.NotNull] TCollection dest)
        where TCollection : g__SCG.ICollection<Id>
    {
        g__ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(dest);

        dest.Add(global::TestProject.Kind.None);
    }

}



    }
}

