#pragma warning disable 0219

using System;
using EncosyTower.Entities.Stats;
using Unity.Entities;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SD = global::System.Diagnostics;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ET = global::EncosyTower.Common;
using g__ETCol = global::EncosyTower.Collections;
using g__ETCon = global::EncosyTower.Conversion;
using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;
using g__ETES = global::EncosyTower.Entities.Stats;
using g__ETL = global::EncosyTower.Logging;
using g__UC = global::Unity.Collections;
using g__UCLU = global::Unity.Collections.LowLevel.Unsafe;
using g__UECS = global::Unity.Entities;
using g__UM = global::Unity.Mathematics;
using g__UE = global::UnityEngine;

using g__StatSystem = global::TestProject.StatsApi;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    partial struct Stats
    {
        public const int LENGTH = 3;

        private readonly static Type[] s_types = new Type[]
        {
            Type.Hp,
            Type.Gold,
            Type.Level,
        };

        public Indices indices;

        static Stats()
        {
            ThrowIfTypesLengthExceedsStatUserDataCapacity();
        }

        public static g__S.ReadOnlySpan<Type> Types
        {
            [g__SRCS.MethodImpl(INLINING)]
            get => s_types;
        }

        /// <summary>
        /// Returns a <see cref="Stats"/> from the value of <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// The layout and size of <typeparamref name="T"/> must be the same as <see cref="Stats"/>, because this method uses <c>g__UCLU.UnsafeUtility.As&lt;T, Stats&gt;</c> under the hood.
        /// </remarks>
        /// <seealso cref="g__UCLU.UnsafeUtility.As{U, T}"/>
        [g__SRCS.MethodImpl(INLINING)]
        public static Stats CastFrom<T>(T value)
            where T : unmanaged
        {
            ThrowIfCannotCastFromType<T>();

            return g__UCLU.UnsafeUtility.As<T, Stats>(ref value);
        }

        /// <inheritdoc cref="CastFrom{T}(T)"/>
        [g__SRCS.MethodImpl(INLINING)]
        public static ref Stats CastFrom<T>(ref T value)
            where T : unmanaged
        {
            ThrowIfCannotCastFromType<T>();

            return ref g__UCLU.UnsafeUtility.As<T, Stats>(ref value);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static Indices GetIndicesFrom<T>(T value)
            where T : unmanaged
        {
            return CastFrom<T>(value).indices;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static ref Indices GetIndicesFrom<T>(ref T value)
            where T : unmanaged
        {
            return ref CastFrom<T>(ref value).indices;
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static StatIndices GetStatIndicesFrom<T>(T value)
            where T : unmanaged
        {
            return CastFrom<T>(value).GetStatIndices();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static StatHandles GetStatHandlesFrom<T>(T value, g__UECS.Entity entity)
            where T : unmanaged
        {
            return CastFrom<T>(value).GetStatHandles(entity);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly StatIndices GetStatIndices()
        {
            return indices.ToStatIndices();
        }

        [g__SRCS.MethodImpl(INLINING)]
        public readonly StatHandles GetStatHandles(g__UECS.Entity entity)
        {
            return indices.ToStatHandles(entity);
        }

        public readonly void FindValidIndices(g__UC.NativeHashMap<TypeId, Index> result)
        {
            result.Clear();
            g__ETCol.EncosyNativeHashMapExtensions.IncreaseCapacityTo(result, LENGTH);

            var indices = this.indices;
            var types = Types;
            var length = types.Length;

            for (var i = 0; i < length; i++)
            {
                var type = types[i];
                var index = indices[type];

                if (index.IsValid)
                {
                    result.TryAdd(type, index);
                }
            }
        }

        public readonly void FindValidStatIndices(g__UC.NativeHashMap<TypeId, g__ETES.StatIndex> result)
        {
            result.Clear();
            g__ETCol.EncosyNativeHashMapExtensions.IncreaseCapacityTo(result, LENGTH);

            var indices = this.indices;
            var types = Types;
            var length = types.Length;

            for (var i = 0; i < length; i++)
            {
                var type = types[i];
                var index = indices[type];

                if (index.IsValid)
                {
                    result.TryAdd(type, index);
                }
            }
        }

        public readonly void FindValidStatHandles(g__UECS.Entity entity, g__UC.NativeHashMap<TypeId, g__ETES.StatHandle> result)
        {
            result.Clear();
            g__ETCol.EncosyNativeHashMapExtensions.IncreaseCapacityTo(result, LENGTH);

            var indices = this.indices;
            var types = Types;
            var length = types.Length;

            for (var i = 0; i < length; i++)
            {
                var type = types[i];
                var index = indices[type];

                if (index.IsValid)
                {
                    result.TryAdd(type, new(entity, index));
                }
            }
        }

    }

#region    TYPE ENUM
#endregion =========

    partial struct Stats // Type
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")]
        public enum Type : byte
        {
            Undefined = 0,
            Hp = 1,
            Gold = 2,
            Level = 3,
        }

    }

#region    TYPE ID
#endregion =======

    partial struct Stats // TypeId
    {
        /// <summary>
        /// Identifies a stat type within <see cref="Stats"/>.
        /// </summary>
        /// <remarks>
        /// Wraps the <see cref="Type"/> enum and provides methods for encoding and decoding stat user data,
        /// validating types, and converting to array indices.
        /// </remarks>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public readonly partial struct TypeId : g__S.IEquatable<TypeId>
        {
            public const uint OFFSET = 1000;

            public readonly Type Value;

            [g__SRCS.MethodImpl(INLINING)]
            public TypeId(Type value)
            {
                Value = value;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator TypeId(Type value)
                => new(value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Type(TypeId id)
                => id.Value;

            [g__SRCS.MethodImpl(INLINING)]
            public static bool operator ==(TypeId lhs, TypeId rhs)
                => lhs.Equals(rhs);

            [g__SRCS.MethodImpl(INLINING)]
            public static bool operator !=(TypeId lhs, TypeId rhs)
                => !lhs.Equals(rhs);

            /// <summary>
            /// Converts <paramref name="type"/> to the corresponding valid index of the array <see cref="Stats.Types"/>.
            /// </summary>
            /// <remarks>
            /// The value of <see cref="Type.Undefined"/> is invalid.
            /// </remarks>
            [g__SRCS.MethodImpl(INLINING)]
            public static int ToValidArrayIndex(Type type)
            {
                // Remove the value of Type.Undefined
                return (int)type - 1;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static uint EncodeToStatUserData(Type type)
            {
                return (uint)type + OFFSET;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static Type DecodeFromStatUserData(uint userData)
            {
                return (Type)(userData - OFFSET);
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static Type DecodeFromStatUserData(in g__StatSystem.Stat stat)
            {
                return DecodeFromStatUserData(stat.UserData);
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool ValidateType(Type type)
            {
                return (uint)type < (uint)LENGTH;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool ValidateStatUserData(uint userData)
            {
                return (uint)DecodeFromStatUserData(userData) < (uint)LENGTH;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool ValidateStatUserData(in g__StatSystem.Stat stat)
            {
                return ValidateStatUserData(stat.UserData);
            }

            public static bool ValidateStat<TStatData>(in g__StatSystem.Stat<TStatData> stat)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                if (ValidateStatUserData(stat.UserData) == false) return false;

                if (typeof(TStatData) == typeof(Hp)) return true;
                if (typeof(TStatData) == typeof(Gold)) return true;
                if (typeof(TStatData) == typeof(Level)) return true;

                return false;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public bool Equals(TypeId other)
                => Value == other.Value;

            [g__SRCS.MethodImpl(INLINING)]
            public override bool Equals(object obj)
                => obj is TypeId other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public override int GetHashCode()
                => ((byte)Value).GetHashCode();

        }

    }

#region    INDICES
#endregion =======

    partial struct Stats // Indices
    {
        /// <summary>
        /// A fixed-size, blittable collection of <see cref="Index"/> values, one per stat type in <see cref="Stats"/>.
        /// </summary>
        /// <remarks>
        /// Each field corresponds to a stat type and stores the index of that stat in the stat buffer.
        /// </remarks>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Indices : g__ETCol.IHasLength, g__ETCol.IAsSpan<Index>, g__ETCol.IAsReadOnlySpan<Index>
        {
            public const int LENGTH = Stats.LENGTH;

            public Index<Hp> hp;
            public Index<Gold> gold;
            public Index<Level> level;

            [g__SRCS.MethodImpl(INLINING)]
            public Indices(g__S.ReadOnlySpan<Index> values) : this()
            {
                values.CopyTo(AsSpan());
            }

            public ref Index this[int index]
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => ref AsSpan()[index];
            }

            public ref Index this[Type type]
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => ref AsSpan()[TypeId.ToValidArrayIndex(type)];
            }

            public readonly int Length
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => LENGTH;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator Indices(g__S.Span<Index> values)
                => new(values);

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator Indices(g__S.ReadOnlySpan<Index> values)
                => new(values);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGet(Type type, out Index result)
            {
                var index = TypeId.ToValidArrayIndex(type);

                if ((uint)index < (uint)LENGTH)
                {
                    result = AsReadOnlySpan()[index];
                    return true;
                }

                result = default;
                return false;
            }

            public readonly g__S.Span<Index> AsSpan()
            {
                // SAFETY: The fixed span is built from the contiguous generated stat storage.
                unsafe
                {
                    fixed (void* ptr = &hp)
                    {
                        return new g__S.Span<Index>(ptr, LENGTH);
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__S.ReadOnlySpan<Index> AsReadOnlySpan()
                => AsSpan();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__S.ReadOnlySpan<Index>.Enumerator GetEnumerator()
                => AsReadOnlySpan().GetEnumerator();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly StatIndices ToStatIndices()
            {
                return new()
                {
                    hp = hp,
                    gold = gold,
                    level = level,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly StatHandles ToStatHandles(g__UECS.Entity entity)
            {
                return new()
                {
                    hp = new(entity, hp),
                    gold = new(entity, gold),
                    level = new(entity, level),
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Index<TStatData> GetIndexFor<TStatData>()
                where TStatData : unmanaged, g__ETES.IStatData
            {
                if (typeof(TStatData) == typeof(Hp))
                {
                    return (Index<TStatData>)((Index)hp);
                }

                if (typeof(TStatData) == typeof(Gold))
                {
                    return (Index<TStatData>)((Index)gold);
                }

                if (typeof(TStatData) == typeof(Level))
                {
                    return (Index<TStatData>)((Index)level);
                }

                return default;
            }

            public readonly g__UC.NativeList<IndexRecord> ToRecords(g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                var result = new g__UC.NativeList<IndexRecord>(LENGTH, allocator);
                var types = Types;
                var indices = AsReadOnlySpan();

                for (var i = 0; i < LENGTH; i++)
                {
                    var index = indices[i];
                    result.AddNoResize(new IndexRecord(index, types[i], index.IsValid));
                }

                return result;
            }

            public readonly g__UC.NativeList<IndexRecord> ToValidRecords(g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                var result = new g__UC.NativeList<IndexRecord>(LENGTH, allocator);
                var types = Types;
                var indices = AsReadOnlySpan();

                for (var i = 0; i < LENGTH; i++)
                {
                    var index = indices[i];

                    if (index.IsValid)
                    {
                        result.AddNoResize(new IndexRecord(index, types[i], true));
                    }
                }

                return result;
            }

        }

    }

#region    INDEX
#endregion =====

    partial struct Stats // Index
    {
        /// <summary>
        /// A type-erased index into the stat buffer for a stat slot.
        /// </summary>
        /// <remarks>
        /// Use <see cref="Index{TStatData}"/> when the stat data type is known at compile time.         /// An index is valid when its <c>value</c> is greater than zero.
        /// </remarks>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        [g__S.Serializable]
        public partial struct Index : g__S.IEquatable<Index>, g__ET.IIsValid, g__ETCon.IToFixedString, g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
        {
            public static readonly Index Null = default;

            /// <inheritdoc cref="g__ETES.StatIndex.value"/>
            public byte value;

            [g__SRCS.MethodImpl(INLINING)]
            public Index(byte value)
            {
                this.value = value;
            }

            public readonly bool IsValid
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => value > Null.value;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Index(byte value)
                => new(value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator byte(Index index)
                => index.value;

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Index(g__ETES.StatIndex index)
                => new((byte)index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatIndex(Index index)
                => new((int)index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(Index other)
                => value == other.value;

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is Index other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => value.GetHashCode();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override string ToString()
                => value.ToString();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__UC.FixedString32Bytes ToFixedString()
                => g__ETCol.EncosyFixedStringExtensions.ToFixedString(value);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly TFixedString ToFixedString<TFixedString>()
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
                => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatHandle ToStatHandle(g__UECS.Entity entity)
                => new(entity, this);

        }

    }

#region    INDEX<TSTAT_DATA>
#endregion =================

    partial struct Stats // Index<TStatData>
    {
        /// <summary>
        /// A strongly-typed index into the stat buffer for a specific <typeparamref name="TStatData"/>.
        /// </summary>
        /// <remarks>
        /// Implicitly converts to and from <see cref="Index"/> and <see cref="g__ETES.StatIndex{TStatData}"/>.         /// An index is valid when its <c>value</c> is greater than zero.
        /// </remarks>
        /// <typeparam name="TStatData">The stat data type this index refers to.</typeparam>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        [g__S.Serializable]
        public partial struct Index<TStatData> : g__S.IEquatable<Index<TStatData>>, g__ET.IIsValid, g__ETCon.IToFixedString, g__ETCon.IToFixedString<g__UC.FixedString32Bytes>
            where TStatData : unmanaged, g__ETES.IStatData
        {
            public static readonly Index<TStatData> Null = default;

            /// <inheritdoc cref="StatIndex.value"/>
            public byte value;

            [g__SRCS.MethodImpl(INLINING)]
            public Index(byte value)
            {
                this.value = value;
            }

            public readonly bool IsValid
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => value > Null.value;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Index<TStatData>(byte value)
                => new(value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator byte(Index<TStatData> index)
                => index.value;

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Index<TStatData>(g__ETES.StatIndex<TStatData> index)
                => new((byte)index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator Index<TStatData>(Index index)
                => new(index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Index(Index<TStatData> index)
                => new(index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatIndex<TStatData>(Index<TStatData> index)
                => new((int)index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatIndex(Index<TStatData> index)
                => new((int)index.value);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(Index<TStatData> other)
                => value == other.value;

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is Index<TStatData> other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => value.GetHashCode();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override string ToString()
                => value.ToString();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__UC.FixedString32Bytes ToFixedString()
                => g__ETCol.EncosyFixedStringExtensions.ToFixedString(value);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly TFixedString ToFixedString<TFixedString>()
                where TFixedString : unmanaged, g__UC.INativeList<byte>, g__UC.IUTF8Bytes
                => g__ETCol.EncosyFixedStringExtensions.CastTo<TFixedString>(ToFixedString());

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatHandle<TStatData> ToStatHandle(g__UECS.Entity entity)
                => new(entity, this);

        }

    }

#region    INDEX RECORD
#endregion ============

    partial struct Stats // IndexRecord
    {
        /// <summary>
        /// An immutable record that pairs an <see cref="Index"/> with its associated <see cref="Type"/> and validity flag.
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public readonly partial struct IndexRecord : g__S.IEquatable<IndexRecord>, g__ET.IIsValid
        {
            public readonly Index Index;
            public readonly Type Type;

            [g__SRCS.MethodImpl(INLINING)]
            public IndexRecord(Index index, Type type, bool isValid)
            {
                Index = index;
                Type = type;
                IsValid = isValid;
            }

            public readonly bool IsValid { get; }

            [g__SRCS.MethodImpl(INLINING)]
            public void Deconstruct(out Index index, out Type type, out bool isValid)
            {
                index = Index;
                type = Type;
                isValid = IsValid;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public bool Equals(IndexRecord other)
                => Index == other.Index && Type == other.Type && IsValid == other.IsValid;

            [g__SRCS.MethodImpl(INLINING)]
            public override bool Equals(object obj)
                => obj is IndexRecord other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public override int GetHashCode()
                => g__ET.HashValue.Combine(Index, Type, IsValid);

        }

    }

#region    STAT INDEX RECORD
#endregion =================

    partial struct Stats // StatIndexRecord
    {
        /// <summary>
        /// An immutable record that pairs an <see cref="g__ETES.StatIndex"/> with its associated <see cref="Type"/> and validity flag.
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public readonly partial struct StatIndexRecord : g__S.IEquatable<StatIndexRecord>, g__ET.IIsValid
        {
            public readonly g__ETES.StatIndex Index;
            public readonly Type Type;

            [g__SRCS.MethodImpl(INLINING)]
            public StatIndexRecord(g__ETES.StatIndex index, Type type, bool isValid)
            {
                Index = index;
                Type = type;
                IsValid = isValid;
            }

            public readonly bool IsValid { get; }

            [g__SRCS.MethodImpl(INLINING)]
            public void Deconstruct(out g__ETES.StatIndex index, out Type type, out bool isValid)
            {
                index = Index;
                type = Type;
                isValid = IsValid;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public bool Equals(StatIndexRecord other)
                => Index == other.Index && Type == other.Type && IsValid == other.IsValid;

            [g__SRCS.MethodImpl(INLINING)]
            public override bool Equals(object obj)
                => obj is StatIndexRecord other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public override int GetHashCode()
                => g__ET.HashValue.Combine(Index, Type, IsValid);

        }

    }

#region    STAT HANDLE RECORD
#endregion ==================

    partial struct Stats // StatHandleRecord
    {
        /// <summary>
        /// An immutable record that pairs an <see cref="g__ETES.StatHandle"/> with its associated <see cref="Type"/> and validity flag.
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public readonly partial struct StatHandleRecord : g__S.IEquatable<StatHandleRecord>, g__ET.IIsValid
        {
            public readonly g__ETES.StatHandle Handle;
            public readonly Type Type;

            [g__SRCS.MethodImpl(INLINING)]
            public StatHandleRecord(g__ETES.StatHandle handle, Type type, bool isValid)
            {
                Handle = handle;
                Type = type;
                IsValid = isValid;
            }

            public readonly bool IsValid { get; }

            [g__SRCS.MethodImpl(INLINING)]
            public void Deconstruct(out g__ETES.StatHandle handle, out Type type, out bool isValid)
            {
                handle = Handle;
                type = Type;
                isValid = IsValid;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public bool Equals(StatHandleRecord other)
                => Handle == other.Handle && Type == other.Type && IsValid == other.IsValid;

            [g__SRCS.MethodImpl(INLINING)]
            public override bool Equals(object obj)
                => obj is StatHandleRecord other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public override int GetHashCode()
                => g__ET.HashValue.Combine(Handle, Type, IsValid);

        }

    }

#region    STAT INDICES
#endregion ============

    partial struct Stats // StatIndices
    {
        /// <summary>
        /// A fixed-size, blittable collection of <see cref="g__ETES.StatIndex"/> values, one per stat type in <see cref="Stats"/>.
        /// </summary>
        /// <remarks>
        /// Mirrors <see cref="Indices"/> but stores raw <see cref="g__ETES.StatIndex"/> values.         /// Obtained via <see cref="Indices.ToStatIndices"/>.
        /// </remarks>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct StatIndices : g__ETCol.IHasLength, g__ETCol.IAsSpan<g__ETES.StatIndex>, g__ETCol.IAsReadOnlySpan<g__ETES.StatIndex>
        {
            public const int LENGTH = Stats.LENGTH;

            public g__ETES.StatIndex<Hp> hp;
            public g__ETES.StatIndex<Gold> gold;
            public g__ETES.StatIndex<Level> level;

            [g__SRCS.MethodImpl(INLINING)]
            public StatIndices(g__S.ReadOnlySpan<g__ETES.StatIndex> values) : this()
            {
                values.CopyTo(AsSpan());
            }

            public ref g__ETES.StatIndex this[int index]
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => ref AsSpan()[index];
            }

            public ref g__ETES.StatIndex this[Type type]
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => ref AsSpan()[TypeId.ToValidArrayIndex(type)];
            }

            public readonly int Length
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => LENGTH;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator StatIndices(g__S.Span<g__ETES.StatIndex> values)
                => new(values);

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator StatIndices(g__S.ReadOnlySpan<g__ETES.StatIndex> values)
                => new(values);
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGet(Type type, out g__ETES.StatIndex result)
            {
                var index = TypeId.ToValidArrayIndex(type);

                if ((uint)index < (uint)LENGTH)
                {
                    result = AsReadOnlySpan()[index];
                    return true;
                }

                result = default;
                return false;
            }


            public readonly g__S.Span<g__ETES.StatIndex> AsSpan()
            {
                // SAFETY: The fixed span is built from the contiguous generated stat storage.
                unsafe
                {
                    fixed (void* ptr = &hp)
                    {
                        return new g__S.Span<g__ETES.StatIndex>(ptr, LENGTH);
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__S.ReadOnlySpan<g__ETES.StatIndex> AsReadOnlySpan()
                => AsSpan();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__S.ReadOnlySpan<g__ETES.StatIndex>.Enumerator GetEnumerator()
                => AsReadOnlySpan().GetEnumerator();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Indices ToIndices()
            {
                return new()
                {
                    hp = (Index<Hp>)hp,
                    gold = (Index<Gold>)gold,
                    level = (Index<Level>)level,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly StatHandles ToStatHandles(Entity entity)
            {
                return new()
                {
                    hp = new(entity, hp),
                    gold = new(entity, gold),
                    level = new(entity, level),
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatIndex<TStatData> GetStatIndexFor<TStatData>()
                where TStatData : unmanaged, IStatData
            {
                if (typeof(TStatData) == typeof(Hp))
                {
                    return (g__ETES.StatIndex<TStatData>)((StatIndex)hp);
                }

                if (typeof(TStatData) == typeof(Gold))
                {
                    return (g__ETES.StatIndex<TStatData>)((StatIndex)gold);
                }

                if (typeof(TStatData) == typeof(Level))
                {
                    return (g__ETES.StatIndex<TStatData>)((StatIndex)level);
                }

                return default;
            }

            public readonly g__UC.NativeList<StatIndexRecord> ToRecords(g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                var result = new g__UC.NativeList<StatIndexRecord>(LENGTH, allocator);
                var types = Types;
                var indices = AsReadOnlySpan();

                for (var i = 0; i < LENGTH; i++)
                {
                    var index = indices[i];
                    result.AddNoResize(new StatIndexRecord(index, types[i], index.IsValid));
                }

                return result;
            }

            public readonly g__UC.NativeList<StatIndexRecord> ToValidRecords(g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                var result = new g__UC.NativeList<StatIndexRecord>(LENGTH, allocator);
                var types = Types;
                var indices = AsReadOnlySpan();

                for (var i = 0; i < LENGTH; i++)
                {
                    var index = indices[i];

                    if (index.IsValid)
                    {
                        result.AddNoResize(new StatIndexRecord(index, types[i], true));
                    }
                }

                return result;
            }

        }

    }

#region    STAT HANDLES
#endregion ============

    partial struct Stats // StatHandles
    {
        /// <summary>
        /// A fixed-size, blittable collection of <see cref="g__ETES.StatHandle"/> values, one per stat type in <see cref="Stats"/>.
        /// </summary>
        /// <remarks>
        /// Each handle encodes both an entity reference and a stat index, allowing direct stat access.
        /// Obtained via <see cref="Indices.ToStatHandles"/>.        /// </remarks>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct StatHandles : g__ETCol.IHasLength, g__ETCol.IAsSpan<g__ETES.StatHandle>, g__ETCol.IAsReadOnlySpan<g__ETES.StatHandle>
        {
            public const int LENGTH = Stats.LENGTH;

            public g__ETES.StatHandle<Hp> hp;
            public g__ETES.StatHandle<Gold> gold;
            public g__ETES.StatHandle<Level> level;

            [g__SRCS.MethodImpl(INLINING)]
            public StatHandles(g__S.ReadOnlySpan<g__ETES.StatHandle> values) : this()
            {
                values.CopyTo(AsSpan());
            }

            public ref g__ETES.StatHandle this[int index]
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => ref AsSpan()[index];
            }

            public ref g__ETES.StatHandle this[Type type]
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => ref AsSpan()[TypeId.ToValidArrayIndex(type)];
            }

            public readonly int Length
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => LENGTH;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator StatHandles(g__S.Span<g__ETES.StatHandle> values)
                => new(values);

            [g__SRCS.MethodImpl(INLINING)]
            public static explicit operator StatHandles(g__S.ReadOnlySpan<g__ETES.StatHandle> values)
                => new(values);
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGet(Type type, out g__ETES.StatHandle result)
            {
                var index = TypeId.ToValidArrayIndex(type);

                if ((uint)index < (uint)LENGTH)
                {
                    result = AsReadOnlySpan()[index];
                    return true;
                }

                result = default;
                return false;
            }


            public readonly g__S.Span<g__ETES.StatHandle> AsSpan()
            {
                // SAFETY: The fixed span is built from the contiguous generated stat storage.
                unsafe
                {
                    fixed (void* ptr = &hp)
                    {
                        return new g__S.Span<g__ETES.StatHandle>(ptr, LENGTH);
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__S.ReadOnlySpan<g__ETES.StatHandle> AsReadOnlySpan()
                => AsSpan();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__S.ReadOnlySpan<g__ETES.StatHandle>.Enumerator GetEnumerator()
                => AsReadOnlySpan().GetEnumerator();

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Indices ToIndices()
            {
                return new()
                {
                    hp = (Index<Hp>)hp.index,
                    gold = (Index<Gold>)gold.index,
                    level = (Index<Level>)level.index,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly StatIndices ToStatIndices()
            {
                return new()
                {
                    hp = hp.index,
                    gold = gold.index,
                    level = level.index,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatHandle<TStatData> GetStatHandleFor<TStatData>()
                where TStatData : unmanaged, g__ETES.IStatData
            {
                if (typeof(TStatData) == typeof(Hp))
                {
                    return (g__ETES.StatHandle<TStatData>)((StatHandle)hp);
                }

                if (typeof(TStatData) == typeof(Gold))
                {
                    return (g__ETES.StatHandle<TStatData>)((StatHandle)gold);
                }

                if (typeof(TStatData) == typeof(Level))
                {
                    return (g__ETES.StatHandle<TStatData>)((StatHandle)level);
                }

                return default;
            }

            public readonly g__UC.NativeList<StatHandleRecord> ToRecords(g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                var result = new g__UC.NativeList<StatHandleRecord>(LENGTH, allocator);
                var types = Types;
                var handles = AsReadOnlySpan();

                for (var i = 0; i < LENGTH; i++)
                {
                    var handle = handles[i];
                    result.AddNoResize(new StatHandleRecord(handle, types[i], handle.IsValid));
                }

                return result;
            }

            public readonly g__UC.NativeList<StatHandleRecord> ToValidRecords(g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                var result = new g__UC.NativeList<StatHandleRecord>(LENGTH, allocator);
                var types = Types;
                var handles = AsReadOnlySpan();

                for (var i = 0; i < LENGTH; i++)
                {
                    var handle = handles[i];

                    if (handle.IsValid)
                    {
                        result.AddNoResize(new StatHandleRecord(handle, types[i], true));
                    }
                }

                return result;
            }

        }

    }

#region    STAT DATA - HP
#endregion ==============

    partial struct Stats // Stat: Hp
    {
        partial struct Hp
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__StatSystem.ValuePair ToValuePair()
                => IsValuePair ? new g__StatSystem.ValuePair(BaseValue, CurrentValue) : new g__StatSystem.ValuePair(CurrentValue);

            [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
            public static partial class Params
            {
                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Hp> Create(g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Hp(), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Hp));

                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Hp> Create(float baseValue, g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Hp(baseValue), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Hp));

                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Hp> Create(float baseValue, float currentValue, g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Hp(baseValue, currentValue), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Hp));

            }
        }

    }

#region    STAT DATA - GOLD
#endregion ================

    partial struct Stats // Stat: Gold
    {
        partial struct Gold
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__StatSystem.ValuePair ToValuePair()
                => IsValuePair ? new g__StatSystem.ValuePair(BaseValue, CurrentValue) : new g__StatSystem.ValuePair(CurrentValue);

            [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
            public static partial class Params
            {
                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Gold> Create(g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Gold(), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Gold));

                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Gold> Create(int value, g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Gold(value), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Gold));

            }
        }

    }

#region    STAT DATA - LEVEL
#endregion =================

    partial struct Stats // Stat: Level
    {
        partial struct Level
        {
            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__StatSystem.ValuePair ToValuePair()
                => IsValuePair ? new g__StatSystem.ValuePair(BaseValue, CurrentValue) : new g__StatSystem.ValuePair(CurrentValue);

            [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
            public static partial class Params
            {
                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Level> Create(g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Level(), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Level));

                [g__SRCS.MethodImpl(INLINING)]
                public static g__ETES.StatDataParams<Level> Create(Rank value, g__ET.Option<bool> produceChangeEvents = default)
                    => new(new Level(value), default, produceChangeEvents, TypeId.EncodeToStatUserData(Type.Level));

            }
        }

    }

#region    OPTIONS
#endregion =======

    partial struct Stats // Options
    {
        /// <summary>
        /// Contains optional stat data structures for <see cref="Stats"/>.
        /// </summary>
        /// <remarks>
        /// Provides <see cref="Options.Data"/> for per-stat stat data options and
        /// <see cref="Options.ProduceChangeEvents"/> for per-stat change-event flags.
        /// </remarks>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Options
        {
            [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
            public partial struct Data
            {
                public g__ET.Option<Hp> hp;
                public g__ET.Option<Gold> gold;
                public g__ET.Option<Level> level;

                [g__SRCS.MethodImpl(INLINING)]
                public Data(
                      g__ET.Option<Hp> hp = default
                    , g__ET.Option<Gold> gold = default
                    , g__ET.Option<Level> level = default
                )
                {
                    this.hp = hp;
                    this.gold = gold;
                    this.level = level;
                }

                public bool TrySet(Type type, in g__StatSystem.Stat stat)
                {
                    switch (type)
                    {
                        case Type.Hp:
                        {
                            this.hp = g__StatSystem.API.MakeStatData<Hp>(stat.ValuePair);
                            return true;
                        }

                        case Type.Gold:
                        {
                            this.gold = g__StatSystem.API.MakeStatData<Gold>(stat.ValuePair);
                            return true;
                        }

                        case Type.Level:
                        {
                            this.level = g__StatSystem.API.MakeStatData<Level>(stat.ValuePair);
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

            }

            [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
            public partial struct ProduceChangeEvents
            {
                public g__ET.Option<bool> hp;
                public g__ET.Option<bool> gold;
                public g__ET.Option<bool> level;

                [g__SRCS.MethodImpl(INLINING)]
                public ProduceChangeEvents(
                      g__ET.Option<bool> hp = default
                    , g__ET.Option<bool> gold = default
                    , g__ET.Option<bool> level = default
                )
                {
                    this.hp = hp;
                    this.gold = gold;
                    this.level = level;
                }

                public bool TrySet(Type type, in g__StatSystem.Stat stat)
                {
                    switch (type)
                    {
                        case Type.Hp:
                        {
                            this.hp = stat.ProduceChangeEvents;
                            return true;
                        }

                        case Type.Gold:
                        {
                            this.gold = stat.ProduceChangeEvents;
                            return true;
                        }

                        case Type.Level:
                        {
                            this.level = stat.ProduceChangeEvents;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

            }

        }

    }

#region    BAKER
#endregion =====

    partial struct Stats // Baker
    {
        /// <summary>
        /// Factory class for creating <see cref="Baker{T}"/> instances for <see cref="Stats"/>.
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Baker
        {
            [g__SRCS.MethodImpl(INLINING)]
            public static Baker<Stats> Create(g__StatSystem.Baker baker)
            {
                return new() { baker = baker, statCollection = new(), };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static Baker<Stats> Bake(g__UECS.IBaker ibaker, Entity entity, g__StatSystem.ValuePair.Composer valuePairComposer = default)
            {
                var baker = g__StatSystem.API.BakeStatComponents(ibaker, entity, valuePairComposer);
                return new() { baker = baker, statCollection = new(), };
            }

        }

    }

#region    BAKER<T>
#endregion ========

    partial struct Stats // Baker<T>
    {
        /// <summary>
        /// Provides functionality to create and configure stat components for <see cref="Stats"/> during entity baking.
        /// </summary>
        /// <remarks>
        /// Create via <see cref="Baker.Create"/> or <see cref="Baker.Bake"/>.
        /// </remarks>
        /// <typeparam name="T">The component data type to bake into. Must be layout-compatible with the stat collection type.</typeparam>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Baker<T> where T : unmanaged
        {
            public g__StatSystem.Baker baker;
            public Stats statCollection;

            static Baker()
            {
                ThrowIfCannotCastToType<T>();
            }

            public Baker<T> Reset(g__StatSystem.ValuePair.Composer valuePairComposer = default)
            {
                ref var indices = ref statCollection.indices;
                indices.AsSpan().Fill(default);
                baker.Clear(valuePairComposer);
                return this;
            }

            public Baker<T> CreateAllStats(bool produceChangeEvents = false)
            {
                ref var indices = ref statCollection.indices;

                indices.hp = baker.CreateStatHandle(new Hp(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.Hp)).index;
                indices.gold = baker.CreateStatHandle(new Gold(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.Gold)).index;
                indices.level = baker.CreateStatHandle(new Level(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.Level)).index;

                return this;
            }

            public Baker<T> CreateStats(
                  g__ETES.StatDataParams<Hp> hp = default
                , g__ETES.StatDataParams<Gold> gold = default
                , g__ETES.StatDataParams<Level> level = default
            )
            {
                ref var indices = ref statCollection.indices;

                if (hp.IsCreated && hp.StatData.HasValue)
                {
                    indices.hp = baker.CreateStatHandle(hp.StatData.GetValueOrThrow(), hp.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Hp)).index;
                }

                if (gold.IsCreated && gold.StatData.HasValue)
                {
                    indices.gold = baker.CreateStatHandle(gold.StatData.GetValueOrThrow(), gold.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Gold)).index;
                }

                if (level.IsCreated && level.StatData.HasValue)
                {
                    indices.level = baker.CreateStatHandle(level.StatData.GetValueOrThrow(), level.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Level)).index;
                }

                return this;
            }

            public Baker<T> CreateStat(g__ETES.StatDataParams<Hp> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    statCollection.indices.hp = baker.CreateStatHandle(value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Hp)).index;
                }

                return this;
            }

            public Baker<T> CreateStat(g__ETES.StatDataParams<Gold> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    statCollection.indices.gold = baker.CreateStatHandle(value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Gold)).index;
                }

                return this;
            }

            public Baker<T> CreateStat(g__ETES.StatDataParams<Level> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    statCollection.indices.level = baker.CreateStatHandle(value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Level)).index;
                }

                return this;
            }

            public Baker<T> SetStats(
                  g__ETES.StatDataParams<Hp> hp = default
                , g__ETES.StatDataParams<Gold> gold = default
                , g__ETES.StatDataParams<Level> level = default
            )
            {
                var entity = baker.Entity;
                ref var indices = ref statCollection.indices;

                if (hp.IsCreated && hp.StatData.HasValue)
                {
                    var handle = new g__ETES.StatHandle<Hp>(entity, indices.hp);
                    var userData = TypeId.EncodeToStatUserData(Type.Hp);

                    ThrowCannotSetIfDoesNotExist(baker, handle, userData, "Hp");

                    baker.SetStat(handle, hp.StatData.GetValueOrThrow(), hp.ProduceChangeEvents.GetValueOrDefault(), userData);
                }

                if (gold.IsCreated && gold.StatData.HasValue)
                {
                    var handle = new g__ETES.StatHandle<Gold>(entity, indices.gold);
                    var userData = TypeId.EncodeToStatUserData(Type.Gold);

                    ThrowCannotSetIfDoesNotExist(baker, handle, userData, "Gold");

                    baker.SetStat(handle, gold.StatData.GetValueOrThrow(), gold.ProduceChangeEvents.GetValueOrDefault(), userData);
                }

                if (level.IsCreated && level.StatData.HasValue)
                {
                    var handle = new g__ETES.StatHandle<Level>(entity, indices.level);
                    var userData = TypeId.EncodeToStatUserData(Type.Level);

                    ThrowCannotSetIfDoesNotExist(baker, handle, userData, "Level");

                    baker.SetStat(handle, level.StatData.GetValueOrThrow(), level.ProduceChangeEvents.GetValueOrDefault(), userData);
                }

                return this;
            }

            public Baker<T> SetStat(g__ETES.StatDataParams<Hp> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    var handle = new g__ETES.StatHandle<Hp>(baker.Entity, statCollection.indices.hp);
                    var userData = TypeId.EncodeToStatUserData(Type.Hp);

                    ThrowCannotSetIfDoesNotExist(baker, handle, userData, "Hp");

                    baker.SetStat(handle, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), userData);
                }

                return this;
            }

            public Baker<T> SetStat(g__ETES.StatDataParams<Gold> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    var handle = new g__ETES.StatHandle<Gold>(baker.Entity, statCollection.indices.gold);
                    var userData = TypeId.EncodeToStatUserData(Type.Gold);

                    ThrowCannotSetIfDoesNotExist(baker, handle, userData, "Gold");

                    baker.SetStat(handle, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), userData);
                }

                return this;
            }

            public Baker<T> SetStat(g__ETES.StatDataParams<Level> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    var handle = new g__ETES.StatHandle<Level>(baker.Entity, statCollection.indices.level);
                    var userData = TypeId.EncodeToStatUserData(Type.Level);

                    ThrowCannotSetIfDoesNotExist(baker, handle, userData, "Level");

                    baker.SetStat(handle, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), userData);
                }

                return this;
            }

            public Baker<T> SetOrCreateStats(
                  g__ETES.StatDataParams<Hp> hp = default
                , g__ETES.StatDataParams<Gold> gold = default
                , g__ETES.StatDataParams<Level> level = default
            )
            {
                var entity = baker.Entity;
                ref var indices = ref statCollection.indices;

                if (hp.IsCreated && hp.StatData.HasValue)
                {
                    ref var index = ref indices.hp;
                    index = baker.SetStatOrCreateHandle(new(entity, index), hp.StatData.GetValueOrThrow(), hp.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Hp)).index;
                }

                if (gold.IsCreated && gold.StatData.HasValue)
                {
                    ref var index = ref indices.gold;
                    index = baker.SetStatOrCreateHandle(new(entity, index), gold.StatData.GetValueOrThrow(), gold.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Gold)).index;
                }

                if (level.IsCreated && level.StatData.HasValue)
                {
                    ref var index = ref indices.level;
                    index = baker.SetStatOrCreateHandle(new(entity, index), level.StatData.GetValueOrThrow(), level.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Level)).index;
                }

                return this;
            }

            public Baker<T> SetOrCreateStat(g__ETES.StatDataParams<Hp> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    ref var index = ref statCollection.indices.hp;
                    index = baker.SetStatOrCreateHandle(new(baker.Entity, index), value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Hp)).index;
                }

                return this;
            }

            public Baker<T> SetOrCreateStat(g__ETES.StatDataParams<Gold> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    ref var index = ref statCollection.indices.gold;
                    index = baker.SetStatOrCreateHandle(new(baker.Entity, index), value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Gold)).index;
                }

                return this;
            }

            public Baker<T> SetOrCreateStat(g__ETES.StatDataParams<Level> value)
            {
                if (value.IsCreated && value.StatData.HasValue)
                {
                    ref var index = ref statCollection.indices.level;
                    index = baker.SetStatOrCreateHandle(new(baker.Entity, index), value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Level)).index;
                }

                return this;
            }

            /// <summary>
            /// Returns a new baker with the type <typeparamref name="TComponentData"/> that is compatible to <see cref="Stats"/>.
            /// </summary>
            /// <remarks>
            /// The layout and size of <typeparamref name="TComponentData"/> must be the same as <see cref="Stats"/>, because this method uses <c>g__UCLU.UnsafeUtility.As&lt;Stats, TComponentData&gt;</c> under the hood.
            /// </remarks>
            /// <seealso cref="g__UCLU.UnsafeUtility.As{U, T}"/>
            public Baker<TComponentData> CreateComponentData<TComponentData>()
                where TComponentData : unmanaged, g__UECS.IComponentData
            {
                ThrowIfCannotCastToType<TComponentData>();

                return new() { baker = baker, statCollection = statCollection, };
            }

        }

    }

#region    ACCESSOR
#endregion ========

    partial struct Stats // Accessor
    {
        /// <summary>
        /// Factory class for creating <see cref="Accessor{T}"/> instances for <see cref="Stats"/>.
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Accessor
        {
            [g__SRCS.MethodImpl(INLINING)]
            public static Accessor<T> Create<T>(Entity entity, T statCollection, g__StatSystem.Accessor accessor, g__StatSystem.WorldData worldData)
                where T : unmanaged
            {
                return new() { entity = entity, accessor = accessor, worldData = worldData, statCollection = Stats.CastFrom(statCollection) };
            }

        }

    }

#region    ACCESSOR<T>
#endregion ===========

    partial struct Stats // Accessor<T>
    {
        /// <summary>
        /// Provides read and write access to stat data for <see cref="Stats"/> on an entity.
        /// </summary>
        /// <remarks>
        /// Create via <see cref="Accessor.Create"/>.
        /// </remarks>
        /// <typeparam name="T">The component data type to access. Must be layout-compatible with the stat collection type.</typeparam>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Accessor<T> where T : unmanaged
        {
            public g__StatSystem.Accessor accessor;
            public g__StatSystem.WorldData worldData;
            public g__UECS.Entity entity;
            public Stats statCollection;

            static Accessor()
            {
                ThrowIfCannotCastFromType<T>();
            }

            public readonly bool IsCreated
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => entity != g__UECS.Entity.Null && worldData.IsCreated;
            }

            public readonly Indices Indices
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => statCollection.indices;
            }

            public Accessor<T> TryCreateAllStats(bool produceChangeEvents = false)
            {
                var entity = this.entity;
                ref var indices = ref statCollection.indices;
                var handles = indices.ToStatHandles(entity);

                if (accessor.TryGetStat(handles.hp, out _) == false)
                {
                    if (accessor.TryCreateStatHandle<Hp>(entity, new Hp(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.Hp), out var statHandle))
                    {
                        indices.hp = statHandle.index;
                    }
                }

                if (accessor.TryGetStat(handles.gold, out _) == false)
                {
                    if (accessor.TryCreateStatHandle<Gold>(entity, new Gold(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.Gold), out var statHandle))
                    {
                        indices.gold = statHandle.index;
                    }
                }

                if (accessor.TryGetStat(handles.level, out _) == false)
                {
                    if (accessor.TryCreateStatHandle<Level>(entity, new Level(), produceChangeEvents, TypeId.EncodeToStatUserData(Type.Level), out var statHandle))
                    {
                        indices.level = statHandle.index;
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateStats(
                  g__ETES.StatDataParams<Hp> hp = default
                , g__ETES.StatDataParams<Gold> gold = default
                , g__ETES.StatDataParams<Level> level = default
            )
            {
                var entity = this.entity;
                ref var indices = ref statCollection.indices;
                var handles = indices.ToStatHandles(entity);

                if (hp.IsCreated && hp.StatData.HasValue && accessor.TryGetStat(handles.hp, out _) == false)
                {
                    if (accessor.TryCreateStatHandle<Hp>(entity, hp.StatData.GetValueOrThrow(), hp.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Hp), out var statHandle))
                    {
                        indices.hp = statHandle.index;
                    }
                }

                if (gold.IsCreated && gold.StatData.HasValue && accessor.TryGetStat(handles.gold, out _) == false)
                {
                    if (accessor.TryCreateStatHandle<Gold>(entity, gold.StatData.GetValueOrThrow(), gold.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Gold), out var statHandle))
                    {
                        indices.gold = statHandle.index;
                    }
                }

                if (level.IsCreated && level.StatData.HasValue && accessor.TryGetStat(handles.level, out _) == false)
                {
                    if (accessor.TryCreateStatHandle<Level>(entity, level.StatData.GetValueOrThrow(), level.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Level), out var statHandle))
                    {
                        indices.level = statHandle.index;
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateStat(g__ETES.StatDataParams<Hp> value, out bool success)
            {
                success = false;

                if (value.IsCreated && value.StatData.HasValue)
                {
                    var entity = this.entity;
                    ref var indices = ref statCollection.indices;
                    var handle = new StatHandle<Hp>(entity, indices.hp);

                    if (accessor.TryGetStat(handle, out _) == false)
                    {
                        if (accessor.TryCreateStatHandle<Hp>(entity, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Hp), out var statHandle))
                        {
                            indices.hp = statHandle.index;
                            success = true;
                        }
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateStat(g__ETES.StatDataParams<Gold> value, out bool success)
            {
                success = false;

                if (value.IsCreated && value.StatData.HasValue)
                {
                    var entity = this.entity;
                    ref var indices = ref statCollection.indices;
                    var handle = new StatHandle<Gold>(entity, indices.gold);

                    if (accessor.TryGetStat(handle, out _) == false)
                    {
                        if (accessor.TryCreateStatHandle<Gold>(entity, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Gold), out var statHandle))
                        {
                            indices.gold = statHandle.index;
                            success = true;
                        }
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateStat(g__ETES.StatDataParams<Level> value, out bool success)
            {
                success = false;

                if (value.IsCreated && value.StatData.HasValue)
                {
                    var entity = this.entity;
                    ref var indices = ref statCollection.indices;
                    var handle = new StatHandle<Level>(entity, indices.level);

                    if (accessor.TryGetStat(handle, out _) == false)
                    {
                        if (accessor.TryCreateStatHandle<Level>(entity, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), TypeId.EncodeToStatUserData(Type.Level), out var statHandle))
                        {
                            indices.level = statHandle.index;
                            success = true;
                        }
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateOrSetStat(g__ETES.StatDataParams<Hp> value, out bool success)
            {
                success = false;

                if (value.IsCreated && value.StatData.HasValue)
                {
                    var entity = this.entity;
                    ref var indices = ref statCollection.indices;
                    var handle = new StatHandle<Hp>(entity, indices.hp);
                    var userData = TypeId.EncodeToStatUserData(Type.Hp);

                    if (accessor.TryGetStat(handle, out _) == false)
                    {
                        if (accessor.TryCreateStatHandle<Hp>(entity, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), userData, out var statHandle))
                        {
                            indices.hp = statHandle.index;
                            success = true;
                        }
                    }
                    else
                    {
                        if (success = accessor.TrySetStatData<Hp>(handle, value.StatData.GetValueOrThrow(), ref worldData))
                        {
                            accessor.TrySetStatUserData(handle, userData);
                        }

                        if (success && value.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))
                        {
                            accessor.TrySetStatProduceChangeEvents(handle, produceChangeEvents);
                        }
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateOrSetStat(g__ETES.StatDataParams<Gold> value, out bool success)
            {
                success = false;

                if (value.IsCreated && value.StatData.HasValue)
                {
                    var entity = this.entity;
                    ref var indices = ref statCollection.indices;
                    var handle = new StatHandle<Gold>(entity, indices.gold);
                    var userData = TypeId.EncodeToStatUserData(Type.Gold);

                    if (accessor.TryGetStat(handle, out _) == false)
                    {
                        if (accessor.TryCreateStatHandle<Gold>(entity, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), userData, out var statHandle))
                        {
                            indices.gold = statHandle.index;
                            success = true;
                        }
                    }
                    else
                    {
                        if (success = accessor.TrySetStatData<Gold>(handle, value.StatData.GetValueOrThrow(), ref worldData))
                        {
                            accessor.TrySetStatUserData(handle, userData);
                        }

                        if (success && value.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))
                        {
                            accessor.TrySetStatProduceChangeEvents(handle, produceChangeEvents);
                        }
                    }
                }

                return this;
            }

            public Accessor<T> TryCreateOrSetStat(g__ETES.StatDataParams<Level> value, out bool success)
            {
                success = false;

                if (value.IsCreated && value.StatData.HasValue)
                {
                    var entity = this.entity;
                    ref var indices = ref statCollection.indices;
                    var handle = new StatHandle<Level>(entity, indices.level);
                    var userData = TypeId.EncodeToStatUserData(Type.Level);

                    if (accessor.TryGetStat(handle, out _) == false)
                    {
                        if (accessor.TryCreateStatHandle<Level>(entity, value.StatData.GetValueOrThrow(), value.ProduceChangeEvents.GetValueOrDefault(), userData, out var statHandle))
                        {
                            indices.level = statHandle.index;
                            success = true;
                        }
                    }
                    else
                    {
                        if (success = accessor.TrySetStatData<Level>(handle, value.StatData.GetValueOrThrow(), ref worldData))
                        {
                            accessor.TrySetStatUserData(handle, userData);
                        }

                        if (success && value.ProduceChangeEvents.TryGetValue(out var produceChangeEvents))
                        {
                            accessor.TrySetStatProduceChangeEvents(handle, produceChangeEvents);
                        }
                    }
                }

                return this;
            }

            public readonly Accessor<T> FindValidStats(g__UC.NativeHashMap<TypeId, g__StatSystem.Stat> result)
            {
                result.Clear();
                g__ETCol.EncosyNativeHashMapExtensions.IncreaseCapacityTo(result, LENGTH);

                var stats = accessor.GetStats(entity);
                var length = stats.Length;

                for (var i = 0; i < length; i++)
                {
                    var stat = stats[i];
                    var type = TypeId.DecodeFromStatUserData(stat.UserData);

                    if (TypeId.ValidateType(type))
                    {
                        result.TryAdd(type, stat);
                    }
                }

                return this;
            }

            public readonly Accessor<T> TryGetStat(Type type, out g__StatSystem.Stat stat, out g__ETES.StatHandle handle, out bool success)
            {
                if (Indices.TryGet(type, out var index))
                {
                    handle = new g__ETES.StatHandle(entity, index);
                    success = accessor.TryGetStat(handle, out stat) && TypeId.DecodeFromStatUserData(stat.UserData) == type;
                }
                else
                {
                    handle = default;
                    stat = default;
                    success = false;
                }

                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Accessor<T> TryGetStat(out g__StatSystem.Stat<Hp> stat, out bool success, out g__ETES.StatHandle<Hp> handle, out bool typeMatched)
            {
                handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);
                success = accessor.TryGetStat(handle, out stat);
                typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.Hp;
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Accessor<T> TryGetStat(out g__StatSystem.Stat<Gold> stat, out bool success, out g__ETES.StatHandle<Gold> handle, out bool typeMatched)
            {
                handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);
                success = accessor.TryGetStat(handle, out stat);
                typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.Gold;
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Accessor<T> TryGetStat(out g__StatSystem.Stat<Level> stat, out bool success, out g__ETES.StatHandle<Level> handle, out bool typeMatched)
            {
                handle = new g__ETES.StatHandle<Level>(entity, Indices.level);
                success = accessor.TryGetStat(handle, out stat);
                typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.Level;
                return this;
            }

            public readonly Accessor<T> TryGetStatData(out Hp statData, out bool success, out g__ETES.StatHandle<Hp> handle)
            {
                handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);

                if (accessor.TryGetStat(handle, out var stat) && TypeId.DecodeFromStatUserData(stat.UserData) == Type.Hp)
                {
                    statData = g__StatSystem.API.MakeStatData<Hp>(stat.ValuePair);
                    success = true;
                }
                else
                {
                    statData = default;
                    success = false;
                }

                return this;
            }

            public readonly Accessor<T> TryGetStatData(out Gold statData, out bool success, out g__ETES.StatHandle<Gold> handle)
            {
                handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);

                if (accessor.TryGetStat(handle, out var stat) && TypeId.DecodeFromStatUserData(stat.UserData) == Type.Gold)
                {
                    statData = g__StatSystem.API.MakeStatData<Gold>(stat.ValuePair);
                    success = true;
                }
                else
                {
                    statData = default;
                    success = false;
                }

                return this;
            }

            public readonly Accessor<T> TryGetStatData(out Level statData, out bool success, out g__ETES.StatHandle<Level> handle)
            {
                handle = new g__ETES.StatHandle<Level>(entity, Indices.level);

                if (accessor.TryGetStat(handle, out var stat) && TypeId.DecodeFromStatUserData(stat.UserData) == Type.Level)
                {
                    statData = g__StatSystem.API.MakeStatData<Level>(stat.ValuePair);
                    success = true;
                }
                else
                {
                    statData = default;
                    success = false;
                }

                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetBaseValueToStats(in Options.Data options)
            {
                return TrySetBaseValueToStats(
                      options.hp.TryGetBaseValue()
                    , options.gold.TryGetBaseValue()
                    , options.level.TryGetBaseValue()
                );
            }

            public Accessor<T> TrySetBaseValueToStats(
                  g__ET.Option<float> hp = default
                , g__ET.Option<int> gold = default
                , g__ET.Option<Rank> level = default
            )
            {
                var handles = Indices.ToStatHandles(entity);
                var results = g__ETCol.NativeArray.Create<bool>(LENGTH, g__UC.Allocator.Temp);
                var paramsForStats = g__ETCol.NativeArray.CreateFast<g__ETES.StatValueParams<g__StatSystem.ValuePair>>(LENGTH, g__UC.Allocator.Temp);

                paramsForStats[0] = new((hp.HasValue ? new Hp { baseValue = hp.GetValueOrThrow() }.ToValuePair() : g__ET.Option.None), (g__ETES.StatHandle)handles.hp);
                paramsForStats[1] = new((gold.HasValue ? new Gold { value = gold.GetValueOrThrow() }.ToValuePair() : g__ET.Option.None), (g__ETES.StatHandle)handles.gold);
                paramsForStats[2] = new((level.HasValue ? new Level { value = level.GetValueOrThrow() }.ToValuePair() : g__ET.Option.None), (g__ETES.StatHandle)handles.level);

                accessor.TrySetBaseValueToStats(paramsForStats, results, ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatBaseValue(Hp value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);
                success = accessor.TrySetStatBaseValue(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatBaseValue(Gold value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);
                success = accessor.TrySetStatBaseValue(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatBaseValue(Level value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Level>(entity, Indices.level);
                success = accessor.TrySetStatBaseValue(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetCurrentValueToStats(in Options.Data options)
            {
                return TrySetCurrentValueToStats(
                      options.hp.TryGetCurrentValue()
                    , options.gold.TryGetCurrentValue()
                    , options.level.TryGetCurrentValue()
                );
            }

            public Accessor<T> TrySetCurrentValueToStats(
                  g__ET.Option<float> hp = default
                , g__ET.Option<int> gold = default
                , g__ET.Option<Rank> level = default
            )
            {
                var handles = Indices.ToStatHandles(entity);
                var results = g__ETCol.NativeArray.Create<bool>(LENGTH, g__UC.Allocator.Temp);
                var paramsForStats = g__ETCol.NativeArray.CreateFast<g__ETES.StatValueParams<g__StatSystem.ValuePair>>(LENGTH, g__UC.Allocator.Temp);

                paramsForStats[0] = new((hp.HasValue ? new Hp { currentValue = hp.GetValueOrThrow() }.ToValuePair() :  g__ET.Option.None), (g__ETES.StatHandle)handles.hp);
                paramsForStats[1] = new((gold.HasValue ? new Gold { value = gold.GetValueOrThrow() }.ToValuePair() :  g__ET.Option.None), (g__ETES.StatHandle)handles.gold);
                paramsForStats[2] = new((level.HasValue ? new Level { value = level.GetValueOrThrow() }.ToValuePair() :  g__ET.Option.None), (g__ETES.StatHandle)handles.level);

                accessor.TrySetCurrentValueToStats(paramsForStats, results, ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatCurrentValue(Hp value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);
                success = accessor.TrySetStatCurrentValue(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatCurrentValue(Gold value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);
                success = accessor.TrySetStatCurrentValue(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatCurrentValue(Level value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Level>(entity, Indices.level);
                success = accessor.TrySetStatCurrentValue(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetValuesToStats(in Options.Data options)
            {
                return TrySetValuesToStats(
                      options.hp
                    , options.gold
                    , options.level
                );
            }

            public Accessor<T> TrySetValuesToStats(
                  g__ET.Option<Hp> hp = default
                , g__ET.Option<Gold> gold = default
                , g__ET.Option<Level> level = default
            )
            {
                var handles = Indices.ToStatHandles(entity);
                var results = g__ETCol.NativeArray.Create<bool>(LENGTH, g__UC.Allocator.Temp);
                var paramsForStats = g__ETCol.NativeArray.CreateFast<g__ETES.StatValueParams<g__StatSystem.ValuePair>>(LENGTH, g__UC.Allocator.Temp);

                paramsForStats[0] = new(hp.TryGetValuePair(), (g__ETES.StatHandle)handles.hp);
                paramsForStats[1] = new(gold.TryGetValuePair(), (g__ETES.StatHandle)handles.gold);
                paramsForStats[2] = new(level.TryGetValuePair(), (g__ETES.StatHandle)handles.level);

                accessor.TrySetDataToStats(paramsForStats, results, ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatValues(Hp value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);
                success = accessor.TrySetStatValues(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatValues(Gold value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);
                success = accessor.TrySetStatValues(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatValues(Level value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Level>(entity, Indices.level);
                success = accessor.TrySetStatValues(handle, value.ToValuePair(), ref worldData);
                return this;
            }

            public Accessor<T> TrySetProduceChangeEventsForAllStats(bool value)
            {
                var handles = Indices.ToStatHandles(entity);
                var results = g__ETCol.NativeArray.Create<bool>(LENGTH, g__UC.Allocator.Temp);
                var paramsForStats = g__ETCol.NativeArray.CreateFast<g__ETES.StatValueParams<g__StatSystem.ValuePair>>(LENGTH, g__UC.Allocator.Temp);

                paramsForStats[0] = new( g__ET.Option.None, (StatHandle)handles.hp, value);
                paramsForStats[1] = new( g__ET.Option.None, (StatHandle)handles.gold, value);
                paramsForStats[2] = new( g__ET.Option.None, (StatHandle)handles.level, value);

                accessor.TrySetDataToStats(paramsForStats, results, ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetProduceChangeEventsForStats(in Options.ProduceChangeEvents options)
            {
                return TrySetProduceChangeEventsForStats(
                      options.hp
                    , options.gold
                    , options.level
                );
            }

            public Accessor<T> TrySetProduceChangeEventsForStats(
                  g__ET.Option<bool> hp = default
                , g__ET.Option<bool> gold = default
                , g__ET.Option<bool> level = default
            )
            {
                var handles = Indices.ToStatHandles(entity);
                var results = g__ETCol.NativeArray.Create<bool>(LENGTH, g__UC.Allocator.Temp);
                var paramsForStats = g__ETCol.NativeArray.CreateFast<g__ETES.StatValueParams<g__StatSystem.ValuePair>>(LENGTH, g__UC.Allocator.Temp);

                paramsForStats[0] = new(g__ET.Option.None, (g__ETES.StatHandle)handles.hp, hp);
                paramsForStats[1] = new(g__ET.Option.None, (g__ETES.StatHandle)handles.gold, gold);
                paramsForStats[2] = new(g__ET.Option.None, (g__ETES.StatHandle)handles.level, level);

                accessor.TrySetDataToStats(paramsForStats, results, ref worldData);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatProduceChangeEvents(g__ET.Bool<Hp> value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);
                success = accessor.TrySetStatProduceChangeEvents(handle, value);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatProduceChangeEvents(g__ET.Bool<Gold> value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);
                success = accessor.TrySetStatProduceChangeEvents(handle, value);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor<T> TrySetStatProduceChangeEvents(g__ET.Bool<Level> value, out bool success)
            {
                var handle = new g__ETES.StatHandle<Level>(entity, Indices.level);
                success = accessor.TrySetStatProduceChangeEvents(handle, value);
                return this;
            }

        }

    }

#region    READER
#endregion ======

    partial struct Stats // Reader
    {
        /// <summary>
        /// Factory class for creating <see cref="Reader{T}"/> instances for <see cref="Stats"/>.
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public static partial class Reader
        {
            [g__SRCS.MethodImpl(INLINING)]
            public static Reader<T> Create<T>(g__UECS.Entity entity, T statCollection, g__UECS.DynamicBuffer<g__StatSystem.Stat> statBuffer)
                where T : unmanaged
            {
                return new() { entity = entity, statBuffer = statBuffer.AsNativeArray().AsReadOnly(), statCollection = Stats.CastFrom(statCollection) };
            }

        }

    }

#region    READER
#endregion ======

    partial struct Stats // Reader<T>
    {
        /// <summary>
        /// Provides read-only access to stat data for <see cref="Stats"/> on an entity.
        /// </summary>
        /// <remarks>
        /// Create via <see cref="Reader.Create"/>.
        /// </remarks>
        /// <typeparam name="T">The component data type to read from. Must be layout-compatible with the stat collection type.</typeparam>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct Reader<T> where T : unmanaged
        {
            public g__UECS.Entity entity;
            public g__UC.NativeArray<g__StatSystem.Stat>.ReadOnly statBuffer;
            public Stats statCollection;

            static Reader()
            {
                ThrowIfCannotCastFromType<T>();
            }

            public readonly bool IsCreated
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => entity != g__UECS.Entity.Null && statBuffer.IsCreated;
            }

            public readonly Indices Indices
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => statCollection.indices;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> Contains(Type type, out bool result)
            {
                var stats = statBuffer.AsReadOnlySpan();
                result = Indices.ToStatHandles(entity).TryGet(type, out var handle)
                    && g__StatSystem.API.Contains(handle, TypeId.EncodeToStatUserData(type), stats);
                return this;
            }

            public readonly Reader<T> Contains<TStatData>(out bool result)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                var stats = statBuffer.AsReadOnlySpan();
                var handles = Indices.ToStatHandles(entity);

                if (typeof(TStatData) == typeof(Hp))
                {
                    result = g__StatSystem.API.Contains(handles.hp, TypeId.EncodeToStatUserData(Type.Hp), stats);
                    return this;
                }

                if (typeof(TStatData) == typeof(Gold))
                {
                    result = g__StatSystem.API.Contains(handles.gold, TypeId.EncodeToStatUserData(Type.Gold), stats);
                    return this;
                }

                if (typeof(TStatData) == typeof(Level))
                {
                    result = g__StatSystem.API.Contains(handles.level, TypeId.EncodeToStatUserData(Type.Level), stats);
                    return this;
                }

                result = false;
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> Contains(Index<Hp> index, out bool result)
            {
                var stats = statBuffer.AsReadOnlySpan();
                var handle = new g__ETES.StatHandle<Hp>(entity, index);
                result = g__StatSystem.API.Contains(handle, TypeId.EncodeToStatUserData(Type.Hp), stats);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> Contains(Index<Gold> index, out bool result)
            {
                var stats = statBuffer.AsReadOnlySpan();
                var handle = new g__ETES.StatHandle<Gold>(entity, index);
                result = g__StatSystem.API.Contains(handle, TypeId.EncodeToStatUserData(Type.Gold), stats);
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> Contains(Index<Level> index, out bool result)
            {
                var stats = statBuffer.AsReadOnlySpan();
                var handle = new g__ETES.StatHandle<Level>(entity, index);
                result = g__StatSystem.API.Contains(handle, TypeId.EncodeToStatUserData(Type.Level), stats);
                return this;
            }

            public readonly Reader<T> FindValidStats(g__UC.NativeHashMap<TypeId, g__StatSystem.Stat> result)
            {
                result.Clear();
                g__ETCol.EncosyNativeHashMapExtensions.IncreaseCapacityTo(result, LENGTH);

                var stats = statBuffer.AsReadOnlySpan();
                var length = stats.Length;

                for (var i = 0; i < length; i++)
                {
                    var stat = stats[i];
                    var type = TypeId.DecodeFromStatUserData(stat.UserData);

                    if (TypeId.ValidateType(type))
                    {
                        result.TryAdd(type, stat);
                    }
                }

                return this;
            }

            public readonly Reader<T> GetStatDataOptions(out Options.Data result)
            {
                result = new();

                var stats = statBuffer.AsReadOnlySpan();
                var length = stats.Length;

                for (var i = 0; i < length; i++)
                {
                    var stat = stats[i];
                    var type = TypeId.DecodeFromStatUserData(stat.UserData);

                    if (TypeId.ValidateType(type))
                    {
                        result.TrySet(type, stat);
                    }
                }

                return this;
            }

            public readonly Reader<T> GetProduceChangeEventsOptions(out Options.ProduceChangeEvents result)
            {
                result = new();

                var stats = statBuffer.AsReadOnlySpan();
                var length = stats.Length;

                for (var i = 0; i < length; i++)
                {
                    var stat = stats[i];
                    var type = TypeId.DecodeFromStatUserData(stat.UserData);

                    if (TypeId.ValidateType(type))
                    {
                        result.TrySet(type, stat);
                    }
                }

                return this;
            }

            public readonly Reader<T> TryGetStat(Type type, out g__StatSystem.Stat stat, out g__ETES.StatHandle handle, out bool success)
            {
                if (Indices.TryGet(type, out var index))
                {
                    var stats = statBuffer.AsReadOnlySpan();
                    handle = new g__ETES.StatHandle(entity, index);
                    success = g__StatSystem.API.TryGetStat(handle, stats, out stat) && TypeId.DecodeFromStatUserData(stat.UserData) == type;
                }
                else
                {
                    handle = default;
                    stat = default;
                    success = false;
                }

                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> TryGetStat(out g__StatSystem.Stat<Hp> stat, out bool success, out g__ETES.StatHandle<Hp> handle, out bool typeMatched)
            {
                var stats = statBuffer.AsReadOnlySpan();
                handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);
                success = g__StatSystem.API.TryGetStat(handle, stats, out stat);
                typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.Hp;
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> TryGetStat(out g__StatSystem.Stat<Gold> stat, out bool success, out g__ETES.StatHandle<Gold> handle, out bool typeMatched)
            {
                var stats = statBuffer.AsReadOnlySpan();
                handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);
                success = g__StatSystem.API.TryGetStat(handle, stats, out stat);
                typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.Gold;
                return this;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly Reader<T> TryGetStat(out g__StatSystem.Stat<Level> stat, out bool success, out g__ETES.StatHandle<Level> handle, out bool typeMatched)
            {
                var stats = statBuffer.AsReadOnlySpan();
                handle = new g__ETES.StatHandle<Level>(entity, Indices.level);
                success = g__StatSystem.API.TryGetStat(handle, stats, out stat);
                typeMatched = TypeId.DecodeFromStatUserData(stat.UserData) == Type.Level;
                return this;
            }

            public readonly Reader<T> TryGetStatData(out Hp statData, out bool success, out g__ETES.StatHandle<Hp> handle)
            {
                var stats = statBuffer.AsReadOnlySpan();
                handle = new g__ETES.StatHandle<Hp>(entity, Indices.hp);

                if (g__StatSystem.API.TryGetStat(handle, stats, out var stat) && TypeId.DecodeFromStatUserData(stat.UserData) == Type.Hp)
                {
                    statData = g__StatSystem.API.MakeStatData<Hp>(stat.ValuePair);
                    success = true;
                }
                else
                {
                    statData = default;
                    success = false;
                }

                return this;
            }

            public readonly Reader<T> TryGetStatData(out Gold statData, out bool success, out g__ETES.StatHandle<Gold> handle)
            {
                var stats = statBuffer.AsReadOnlySpan();
                handle = new g__ETES.StatHandle<Gold>(entity, Indices.gold);

                if (g__StatSystem.API.TryGetStat(handle, stats, out var stat) && TypeId.DecodeFromStatUserData(stat.UserData) == Type.Gold)
                {
                    statData = g__StatSystem.API.MakeStatData<Gold>(stat.ValuePair);
                    success = true;
                }
                else
                {
                    statData = default;
                    success = false;
                }

                return this;
            }

            public readonly Reader<T> TryGetStatData(out Level statData, out bool success, out g__ETES.StatHandle<Level> handle)
            {
                var stats = statBuffer.AsReadOnlySpan();
                handle = new g__ETES.StatHandle<Level>(entity, Indices.level);

                if (g__StatSystem.API.TryGetStat(handle, stats, out var stat) && TypeId.DecodeFromStatUserData(stat.UserData) == Type.Level)
                {
                    statData = g__StatSystem.API.MakeStatData<Level>(stat.ValuePair);
                    success = true;
                }
                else
                {
                    statData = default;
                    success = false;
                }

                return this;
            }

        }

    }

#region    EXTENSIONS
#endregion ==========

    partial struct Stats { } // StatsExtensions

    public static partial class StatsExtensions
    {
        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<g__StatSystem.ValuePair> TryGetValuePair(this g__ET.Option<Stats.Hp> value)
            => value.TryGetValue(out var stat) ? stat.ToValuePair() : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<float> TryGetBaseValue(this g__ET.Option<Stats.Hp> value)
            => value.TryGetValue(out var stat) ? stat.baseValue : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<float> TryGetCurrentValue(this g__ET.Option<Stats.Hp> value)
            => value.TryGetValue(out var stat) ? stat.currentValue : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<g__StatSystem.ValuePair> TryGetValuePair(this g__ET.Option<Stats.Gold> value)
            => value.TryGetValue(out var stat) ? stat.ToValuePair() : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<int> TryGetBaseValue(this g__ET.Option<Stats.Gold> value)
            => value.TryGetValue(out var stat) ? stat.value : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<int> TryGetCurrentValue(this g__ET.Option<Stats.Gold> value)
            => value.TryGetValue(out var stat) ? stat.value : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<g__StatSystem.ValuePair> TryGetValuePair(this g__ET.Option<Stats.Level> value)
            => value.TryGetValue(out var stat) ? stat.ToValuePair() : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<Rank> TryGetBaseValue(this g__ET.Option<Stats.Level> value)
            => value.TryGetValue(out var stat) ? stat.value : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static g__ET.Option<Rank> TryGetCurrentValue(this g__ET.Option<Stats.Level> value)
            => value.TryGetValue(out var stat) ? stat.value : g__ET.Option.None;

        [g__SRCS.MethodImpl(INLINING)]
        public static TComponentData ToComponent<TComponentData>(this Stats.Baker<TComponentData> baker)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            return g__UCLU.UnsafeUtility.As<Stats, TComponentData>(ref baker.statCollection);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static void AddComponentToEntity<TComponentData>(this Stats.Baker<TComponentData> baker)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<Stats, TComponentData>(ref baker.statCollection);
            var statBaker = baker.baker;
            statBaker.IBaker.AddComponent(statBaker.Entity, component);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static TComponentData ToComponent<TComponentData>(this Stats.Accessor<TComponentData> accessor)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            return g__UCLU.UnsafeUtility.As<Stats, TComponentData>(ref accessor.statCollection);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static void SetComponentToEntity<TComponentData>(this Stats.Accessor<TComponentData> accessor, g__UECS.EntityManager entityManager)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<Stats, TComponentData>(ref accessor.statCollection);
            entityManager.SetComponentData(accessor.entity, component);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static void SetComponentToEntity<TComponentData>(this Stats.Accessor<TComponentData> accessor, g__UECS.EntityCommandBuffer ecb)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<Stats, TComponentData>(ref accessor.statCollection);
            ecb.SetComponent(accessor.entity, component);
        }

        [g__SRCS.MethodImpl(INLINING)]
        public static void SetComponentToEntity<TComponentData>(this Stats.Accessor<TComponentData> accessor, g__UECS.EntityCommandBuffer.ParallelWriter ecb, int sortKey)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<Stats, TComponentData>(ref accessor.statCollection);
            ecb.SetComponent(sortKey, accessor.entity, component);
        }

    }

#region INTERNALS
#endregion ======

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Stats // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Entities.Stats.Generators.StatCollectionGenerator";

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        private static void ThrowCannotSetIfDoesNotExist(g__StatSystem.Baker baker, StatHandle handle, uint userData, string statDataName)
        {
            if (baker.Contains(handle) == false)
            {
                throw new g__S.InvalidOperationException(
                    $"Cannot set data for stat '{statDataName}' because the handle index is invalid. Please consider using CreateAllStats first."
                );
            }

            if (baker.Contains(handle, userData) == false)
            {
                throw new g__S.InvalidOperationException(
                    $"Cannot set data for stat '{statDataName}' because the stored user data does not equal to '{TypeId.DecodeFromStatUserData(userData)}'.Please double-check the creation of the handle '{handle}'."
                );
            }

        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        private static void ThrowIfCannotCastToType<T>()
            where T : unmanaged
        {
            if (g__UCLU.UnsafeUtility.SizeOf<Stats>() == g__UCLU.UnsafeUtility.SizeOf<T>()) return;

            throw new g__S.InvalidCastException(
                $"Cannot cast 'Stats' into '{typeof(T)}' because these types are not of the same size."
            );
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        private static void ThrowIfCannotCastFromType<T>()
            where T : unmanaged
        {
            if (g__UCLU.UnsafeUtility.SizeOf<Stats>() == g__UCLU.UnsafeUtility.SizeOf<T>()) return;

            throw new g__S.InvalidCastException(
                $"Cannot cast '{typeof(T)}' into 'Stats' because these types are not of the same size."
            );
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        private static void ThrowIfTypesLengthExceedsStatUserDataCapacity()
        {
            if (g__StatSystem.Stat.USER_DATA_SIZE >= g__UCLU.UnsafeUtility.SizeOf<byte>()) return;

            throw new g__S.OverflowException(
                $"'Stats' contains 3 stat data types thus exceeds the maximum capacity of 'global::TestProject.StatsApi.Stat.UserData' which is {g__StatSystem.Stat.USER_DATA_SIZE} bytes."
            );
        }

    }

    [g__SCDC.GeneratedCode(GENERATOR, "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    static partial class StatsExtensions // Internals
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Entities.Stats.Generators.StatCollectionGenerator";

    }



}

