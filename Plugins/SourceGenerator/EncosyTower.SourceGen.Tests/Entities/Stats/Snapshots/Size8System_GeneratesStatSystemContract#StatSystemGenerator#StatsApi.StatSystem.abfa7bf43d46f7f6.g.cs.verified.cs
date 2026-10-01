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
using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;
using g__ETES = global::EncosyTower.Entities.Stats;
using g__UE = UnityEngine;
using g__UB = Unity.Burst;
using g__UC = global::Unity.Collections;
using g__UCLU = global::Unity.Collections.LowLevel.Unsafe;
using g__UECS = global::Unity.Entities;
using g__UM = global::Unity.Mathematics;
using g__UJ = Unity.Jobs;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

#region    STAT<TSTATDATA>
#endregion ===============

    partial class StatsApi // Stat<TStatData>
    {
        public partial struct Stat<TStatData>
            where TStatData : unmanaged, g__ETES.IStatData
        {
        }

    }

#region    STAT
#endregion ====

    partial class StatsApi // Stat
    {
        public partial struct Stat : g__UECS.IBufferElementData, g__ETES.IStat<ValuePair>
        {
            /// <summary>
            /// Size of the underlying <see cref="UserData"/> in bytes.
            /// </summary>
            /// <remarks>
            /// The size of a <see cref="byte">.
            /// </remarks>
            public const int USER_DATA_SIZE = 1;

            [g__UE.SerializeField]
            private g__ETES.ModifierRange _modifierRange;

            [g__UE.SerializeField]
            private g__ETES.ObserverRange _observerRange;

            [g__UE.SerializeField]
            private StatDataStore _valueData;

            [g__UE.SerializeField]
            private byte _userData;

            [g__UE.SerializeField]
            private g__ETES.StatVariantType _valueType;

            [g__UE.SerializeField]
            private g__ET.ByteBool _valueIsPair;

            [g__UE.SerializeField]
            private g__ET.ByteBool _produceChangeEvents;

            public g__ETES.ModifierRange ModifierRange
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _modifierRange;

                [g__SRCS.MethodImpl(INLINING)]
                set => _modifierRange = value;
            }

            public g__ETES.ObserverRange ObserverRange
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _observerRange;

                [g__SRCS.MethodImpl(INLINING)]
                set => _observerRange = value;
            }

            public bool ProduceChangeEvents
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _produceChangeEvents;

                [g__SRCS.MethodImpl(INLINING)]
                set => _produceChangeEvents = value;
            }

            public ValuePair ValuePair
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => StatValueUnion.Convert(_valueData, _valueIsPair, _valueType);

                [g__SRCS.MethodImpl(INLINING)]
                set => StatValueUnion.Convert(value, ref _valueData, ref _valueIsPair, ref _valueType);
            }

            /// <summary>
            /// To store user-defined values.
            /// </summary>
            /// <remarks>
            /// The underlying value is a <see cref="byte">.
            /// </remarks>
            public uint UserData
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => (uint)_userData;

                [g__SRCS.MethodImpl(INLINING)]
                set => _userData = (byte)value;
            }

            /// <summary>
            /// Size of the underlying <see cref="UserData"/> in bytes.
            /// </summary>
            /// <remarks>
            /// The size of a <see cref="byte">.
            /// </remarks>
            public readonly int UserDataSize
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => USER_DATA_SIZE;

            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatVariant GetBaseValueOrDefault(in g__ETES.StatVariant defaultValue = default)
                => ValuePair.GetBaseValueOrDefault(defaultValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatVariant GetCurrentValueOrDefault(in g__ETES.StatVariant defaultValue = default)
                => ValuePair.GetCurrentValueOrDefault(defaultValue);

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetBaseValue(in g__ETES.StatVariant value)
            {
                var valuePair = ValuePair;

                if (valuePair.TrySetBaseValue(value) == false) return false;

                ValuePair = valuePair;
                return true;
            }

            public bool TrySetCurrentValue(in g__ETES.StatVariant value)
            {
                var valuePair = ValuePair;

                if (valuePair.TrySetCurrentValue(value) == false) return false;

                ValuePair = valuePair;
                return true;
            }

            public bool TrySetValues(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
            {
                var valuePair = ValuePair;

                if (valuePair.TrySetValues(baseValue, currentValue) == false) return false;

                ValuePair = valuePair;
                return true;
            }

        }

    }

#region    STAT OBSERVER
#endregion =============

    partial class StatsApi // StatObserver
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct StatObserver : g__UECS.IBufferElementData, g__ETES.IStatObserver
        {
            [g__UE.SerializeField]
            private g__ETES.StatHandle _observerHandle;

            public g__ETES.StatHandle ObserverHandle
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _observerHandle;

                [g__SRCS.MethodImpl(INLINING)]
                set => _observerHandle = value;
            }

        }

    }

#region    STAT MODIFIER
#endregion =============

    partial class StatsApi // StatModifier
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct StatModifier : g__UECS.IBufferElementData, g__ETES.IStatModifier<ValuePair, Stat, StatModifier.Stack>
        {
            public uint Id
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get
                {
                    uint result = default;
                    GetIdInternal(ref result);
                    return result;
                }

                [g__SRCS.MethodImpl(INLINING)]
                set
                {
                    SetIdInternal(value);
                }
            }

            readonly partial void GetIdInternal(ref uint id);

            partial void SetIdInternal(uint value);
            [g__SRCS.MethodImpl(INLINING)]
            public readonly void AddObservedStatsToList(g__UC.NativeList<g__ETES.StatHandle> observedStatHandles)
            {
                AddObservedStatsToListInternal(observedStatHandles);
            }

            readonly partial void AddObservedStatsToListInternal(g__UC.NativeList<g__ETES.StatHandle> observedStatHandles);

            [g__SRCS.MethodImpl(INLINING)]
            public void Apply(g__ETES.StatReader<ValuePair, Stat> reader, ref StatModifier.Stack stack, out bool shouldProduceModifierTriggerEvent)
            {
                shouldProduceModifierTriggerEvent = default;
                ApplyInternal(reader, ref stack, ref shouldProduceModifierTriggerEvent);
            }

            partial void ApplyInternal(Reader reader, ref StatModifier.Stack stack, ref bool shouldProduceModifierTriggerEvent);

        }

    }

#region    STAT MODIFIER - STACK
#endregion =====================

    partial class StatsApi // StatModifier.Stack
    {
        partial struct StatModifier // Stack
        {
            [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
            public partial struct Stack : g__ETES.IStatModifierStack<ValuePair, Stat>
            {
                [g__SRCS.MethodImpl(INLINING)]
                public void Apply(in g__ETES.StatVariant baseValue, ref g__ETES.StatVariant currentValue)
                {
                    ApplyInternal(baseValue, ref currentValue);
                }

                partial void ApplyInternal(in g__ETES.StatVariant baseValue, ref g__ETES.StatVariant currentValue);

                [g__SRCS.MethodImpl(INLINING)]
                public void Reset(in Stat stat)
                {
                    ResetInternal(stat);
                }

                partial void ResetInternal(in Stat stat);
            }

        }
    }

#region    MODIFIER TRIGGER EVENT
#endregion ======================

    partial class StatsApi // ModifierTriggerEvent
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct ModifierTriggerEvent
        {
            public g__ETES.StatModifierHandle handle;

            public StatModifier modifier;

            [g__SRCS.MethodImpl(INLINING)]
            public ModifierTriggerEvent(g__ETES.StatModifierHandle handle, StatModifier modifier)
            {
                this.handle = handle;
                this.modifier = modifier;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly void Deconstruct(out g__ETES.StatModifierHandle handle, out StatModifier modifier)
            {
                handle = this.handle;
                modifier = this.modifier;
            }

        }

    }

#region    STAT MODIFIER RECORD
#endregion ====================

    partial class StatsApi // StatModifierRecord
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct StatModifierRecord
        {
            public g__ETES.StatModifierHandle handle;

            public StatModifier modifier;

            [g__SRCS.MethodImpl(INLINING)]
            public StatModifierRecord(g__ETES.StatModifierHandle handle, StatModifier modifier)
            {
                this.handle = handle;
                this.modifier = modifier;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly void Deconstruct(out g__ETES.StatModifierHandle handle, out StatModifier modifier)
            {
                handle = this.handle;
                modifier = this.modifier;
            }

        }

    }

#region    API
#endregion ===

    partial class StatsApi // API
    {
        public static partial class API { }
    }

#region    READER
#endregion ======

    partial class StatsApi // Reader
    {
        public partial struct Reader
        {
            public g__ETES.StatReader<ValuePair, Stat> reader;

            [g__SRCS.MethodImpl(INLINING)]
            private Reader(in g__ETES.StatReader<ValuePair, Stat> reader)
            {
                this.reader = reader;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Reader(in g__ETES.StatReader<ValuePair, Stat> reader)
                => new(reader);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatReader<ValuePair, Stat>(in Reader reader)
                => reader.reader;
        }

    }

#region    ACCESSOR
#endregion ========

    partial class StatsApi // Accessor
    {
        public partial struct Accessor
        {
            public g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> accessor;

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor(ref g__UECS.SystemState state) : this(ref state, default) { }

            [g__SRCS.MethodImpl(INLINING)]
            public Accessor(ref g__UECS.SystemState state, ValuePair.Composer valuePairComposer)
            {
                this.accessor = new(ref state, valuePairComposer);
            }

            [g__SRCS.MethodImpl(INLINING)]
            private Accessor(in g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> accessor)
            {
                this.accessor = accessor;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Accessor(in g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> accessor)
                => new(accessor);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>(in Accessor accessor)
                => accessor.accessor;
        }

    }

#region    ACCESSOR READ-ONLY
#endregion ==================

    partial class StatsApi // Accessor.ReadOnly
    {
        partial struct Accessor // ReadOnly
        {
            public partial struct ReadOnly
            {
                public g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>.ReadOnly accessor;

                [g__SRCS.MethodImpl(INLINING)]
                public ReadOnly(ref g__UECS.SystemState state)
                {
                    this.accessor = new(ref state);
                }

                [g__SRCS.MethodImpl(INLINING)]
                private ReadOnly(in g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>.ReadOnly accessor)
                {
                    this.accessor = accessor;
                }

                [g__SRCS.MethodImpl(INLINING)]
                public static implicit operator ReadOnly(in g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>.ReadOnly accessor)
                    => new(accessor);

                [g__SRCS.MethodImpl(INLINING)]
                public static implicit operator g__ETES.StatAccessor<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>.ReadOnly(in ReadOnly accessor)
                    => accessor.accessor;
            }

        }
    }

#region    BAKER
#endregion =====

    partial class StatsApi // Baker
    {
        public partial struct Baker
        {
            public g__ETES.StatBaker<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> baker;

            [g__SRCS.MethodImpl(INLINING)]
            private Baker(in g__ETES.StatBaker<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> baker)
            {
                this.baker = baker;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Baker(in g__ETES.StatBaker<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> baker)
                => new(baker);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatBaker<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>(in Baker baker)
                => baker.baker;
        }

    }

#region    WORLD DATA
#endregion ==========

    partial class StatsApi // WorldData
    {
        public partial struct WorldData
        {
            public g__ETES.StatWorldData<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver> worldData;

            [g__SRCS.MethodImpl(INLINING)]
            public WorldData(int initialCapacity, g__UC.AllocatorManager.AllocatorHandle allocator)
            {
                this.worldData = new(initialCapacity, allocator);
            }

            [g__SRCS.MethodImpl(INLINING)]
            private WorldData(in g__ETES.StatWorldData<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver> worldData)
            {
                this.worldData = worldData;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator WorldData(in g__ETES.StatWorldData<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver> worldData)
                => new(worldData);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator g__ETES.StatWorldData<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver>(in WorldData worldData)
                => worldData.worldData;
        }

    }

#region    DEFERRED UPDATE STAT LIST JOB
#endregion =============================

    partial class StatsApi // DeferredUpdateStatListJob
    {
        [g__UB.BurstCompile]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct DeferredUpdateStatListJob : g__UJ.IJob
        {
            private g__ETES.DeferredUpdateStatListJob<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> _jobData;

            [g__SRCS.MethodImpl(INLINING)]
            public DeferredUpdateStatListJob(in Accessor accessor, in WorldData worldData, g__UC.NativeList<g__ETES.StatHandle> statsToUpdate)
            {
                _jobData = new()
                {
                    statAccessor = accessor.accessor,
                    statWorldData = worldData.worldData,
                    statsToUpdate = statsToUpdate,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public void Execute()
                => _jobData.Execute();
        }

    }

#region    DEFERRED UPDATE STAT QUEUE JOB
#endregion ==============================

    partial class StatsApi // DeferredUpdateStatQueueJob
    {
        [g__UB.BurstCompile]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct DeferredUpdateStatQueueJob : g__UJ.IJob
        {
            private g__ETES.DeferredUpdateStatQueueJob<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> _jobData;

            [g__SRCS.MethodImpl(INLINING)]
            public DeferredUpdateStatQueueJob(in Accessor accessor, in WorldData worldData, g__UC.NativeQueue<g__ETES.StatHandle> statsToUpdate)
            {
                _jobData = new()
                {
                    statAccessor = accessor.accessor,
                    statWorldData = worldData.worldData,
                    statsToUpdate = statsToUpdate,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public void Execute()
                => _jobData.Execute();
        }

    }

#region    DEFERRED UPDATE STAT STREAM JOB
#endregion ===========================--==

    partial class StatsApi // DeferredUpdateStatStreamJob
    {
        [g__UB.BurstCompile]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct DeferredUpdateStatStreamJob : g__UJ.IJob
        {
            private g__ETES.DeferredUpdateStatStreamJob<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> _jobData;

            [g__SRCS.MethodImpl(INLINING)]
            public DeferredUpdateStatStreamJob(in Accessor accessor, in WorldData worldData, g__UC.NativeStream.Reader statsToUpdate)
            {
                _jobData = new()
                {
                    statAccessor = accessor.accessor,
                    statWorldData = worldData.worldData,
                    statsToUpdate = statsToUpdate,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public void Execute()
                => _jobData.Execute();
        }

    }

#region    DEFERRED UPDATE STAT UNSAFE BLOCK LIST JOB
#endregion ==========================================

    partial class StatsApi // DeferredUpdateStatUnsafeBlockListJob
    {
        [g__UB.BurstCompile]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public partial struct DeferredUpdateStatUnsafeBlockListJob : g__UJ.IJob
        {
            private g__ETES.DeferredUpdateStatUnsafeBlockListJob<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer> _jobData;

            [g__SRCS.MethodImpl(INLINING)]
            public DeferredUpdateStatUnsafeBlockListJob(in Accessor accessor, in WorldData worldData, global::Latios.Unsafe.UnsafeParallelBlockList<g__ETES.StatHandle> statsToUpdate)
            {
                _jobData = new()
                {
                    statAccessor = accessor.accessor,
                    statWorldData = worldData.worldData,
                    statsToUpdate = statsToUpdate,
                };
            }

            [g__SRCS.MethodImpl(INLINING)]
            public void Execute()
                => _jobData.Execute();
        }

    }

#region    VALUE PAIR
#endregion ==========

    partial class StatsApi // ValuePair
    {
        [g__S.Serializable]
        public partial struct ValuePair : g__ETES.IStatValuePair, g__S.IEquatable<ValuePair>
        {
            [g__UE.SerializeField]
            internal StatDataStore _data;

            [g__UE.SerializeField]
            internal g__ETES.StatVariantType _type;

            [g__UE.SerializeField]
            internal g__ET.ByteBool _isPair;

            [g__SRCS.MethodImpl(INLINING)]
            public ValuePair(in g__ETES.StatVariant value)
            {
                _type = value.Type;
                _data = StatDataStore.Store(_isPair = false, value, value);
            }

            [g__SRCS.MethodImpl(INLINING)]
            public ValuePair(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
            {
                _type = baseValue.Type;
                _data = StatDataStore.Store(_isPair = true, baseValue, currentValue);
            }

            [g__SRCS.MethodImpl(INLINING)]
            private ValuePair(bool isPair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
            {
                _type = baseValue.Type;
                _data = StatDataStore.Store(_isPair = isPair, baseValue, currentValue);
            }

            public readonly g__ETES.StatVariantType Type
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => _type;
            }

            public readonly bool IsPair
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => _isPair;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is ValuePair other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(ValuePair other)
                => StatDataStore.Equals(this, other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => StatDataStore.GetHashCode(this);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatVariant GetBaseValueOrDefault(in g__ETES.StatVariant defaultValue = default)
                => StatDataStore.TryGetBaseValue(this, out var value) ? value : defaultValue;

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatVariant GetCurrentValueOrDefault(in g__ETES.StatVariant defaultValue = default)
                => StatDataStore.TryGetCurrentValue(this, out var value) ? value : defaultValue;

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetBaseValue(in g__ETES.StatVariant value)
                => StatDataStore.TrySetBaseValue(ref this, value);

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetCurrentValue(in g__ETES.StatVariant value)
                => StatDataStore.TrySetCurrentValue(ref this, value);

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetValues(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
                => StatDataStore.TrySetValues(ref this, baseValue, currentValue);

        }

    }

#region    VALUE PAIR COMPOSER
#endregion ===================

    partial class StatsApi // ValuePair.Composer
    {
        partial struct ValuePair // Composer
        {
            [g__S.Serializable]
            public partial struct Composer : g__ETES.IStatValuePairComposer<ValuePair>
            {
                [g__SRCS.MethodImpl(INLINING)]
                public ValuePair Compose(bool isPair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
                {
                    var result = new ValuePair(isPair, baseValue, currentValue);
                    OnCompose(isPair, baseValue, currentValue, ref result);
                    return result;
                }

                partial void OnCompose(bool isPair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue, ref ValuePair result);
            }

        }

    }

#region    IS COMPATIBLE
#endregion =============

    partial class StatsApi // IsCompatible
    {
        [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public static bool IsCompatible<TStatData>(TStatData value)
            where TStatData : unmanaged, g__ETES.IStatData
            => IsCompatible(value.ValueType, value.IsValuePair);

        [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public static bool IsCompatible(in ValuePair value)
            => IsCompatible(value.Type, value.IsPair);

        [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public static bool IsCompatible(in g__ETES.StatVariant value, bool isPair)
            => IsCompatible(value.Type, isPair);

        [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public static bool IsCompatible(g__ETES.StatVariantType type, bool isPair)
            => isPair ? IsCompatiblePair(type) : IsCompatibleSingle(type);

        /// <summary>
        /// These types are compatible to the stat system <see cref="StatsApi"/>:
        /// <list type="bullet">
        /// <item><see cref="g__ETES.StatVariantType.Bool"/>    => <see cref="bool"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool2"/>   => <see cref="g__UM.bool2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool2x2"/> => <see cref="g__UM.bool2x2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool2x3"/> => <see cref="g__UM.bool2x3"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool2x4"/> => <see cref="g__UM.bool2x4"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool3"/>   => <see cref="g__UM.bool3"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool3x2"/> => <see cref="g__UM.bool3x2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool4"/>   => <see cref="g__UM.bool4"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool4x2"/> => <see cref="g__UM.bool4x2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Byte"/>    => <see cref="byte"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Double"/>  => <see cref="double"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Float"/>   => <see cref="float"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Float2"/>  => <see cref="g__UM.float2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Half"/>    => <see cref="g__UM.half"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Half2"/>   => <see cref="g__UM.half2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Half3"/>   => <see cref="g__UM.half3"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Half4"/>   => <see cref="g__UM.half4"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Int"/>     => <see cref="int"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Int2"/>    => <see cref="g__UM.int2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Long"/>    => <see cref="long"/></item>
        /// <item><see cref="g__ETES.StatVariantType.SByte"/>   => <see cref="sbyte"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Short"/>   => <see cref="short"/></item>
        /// <item><see cref="g__ETES.StatVariantType.UInt"/>    => <see cref="uint"/></item>
        /// <item><see cref="g__ETES.StatVariantType.UInt2"/>   => <see cref="g__UM.uint2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.ULong"/>   => <see cref="ulong"/></item>
        /// <item><see cref="g__ETES.StatVariantType.UShort"/>  => <see cref="ushort"/></item>
        /// </list>
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public static bool IsCompatibleSingle(g__ETES.StatVariantType type)
        {
            return type switch
            {
                   g__ETES.StatVariantType.Bool
                or g__ETES.StatVariantType.Bool2
                or g__ETES.StatVariantType.Bool2x2
                or g__ETES.StatVariantType.Bool2x3
                or g__ETES.StatVariantType.Bool2x4
                or g__ETES.StatVariantType.Bool3
                or g__ETES.StatVariantType.Bool3x2
                or g__ETES.StatVariantType.Bool4
                or g__ETES.StatVariantType.Bool4x2
                or g__ETES.StatVariantType.Byte
                or g__ETES.StatVariantType.Double
                or g__ETES.StatVariantType.Float
                or g__ETES.StatVariantType.Float2
                or g__ETES.StatVariantType.Half
                or g__ETES.StatVariantType.Half2
                or g__ETES.StatVariantType.Half3
                or g__ETES.StatVariantType.Half4
                or g__ETES.StatVariantType.Int
                or g__ETES.StatVariantType.Int2
                or g__ETES.StatVariantType.Long
                or g__ETES.StatVariantType.SByte
                or g__ETES.StatVariantType.Short
                or g__ETES.StatVariantType.UInt
                or g__ETES.StatVariantType.UInt2
                or g__ETES.StatVariantType.ULong
                or g__ETES.StatVariantType.UShort
                   => true,
                 _ => false,
            };
        }

        /// <summary>
        /// These types are compatible to the stat system <see cref="StatsApi"/>:
        /// <list type="bullet">
        /// <item><see cref="g__ETES.StatVariantType.Bool"/>    => <see cref="bool"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool2"/>   => <see cref="g__UM.bool2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool2x2"/> => <see cref="g__UM.bool2x2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool3"/>   => <see cref="g__UM.bool3"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Bool4"/>   => <see cref="g__UM.bool4"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Byte"/>    => <see cref="byte"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Float"/>   => <see cref="float"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Half"/>    => <see cref="g__UM.half"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Half2"/>   => <see cref="g__UM.half2"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Int"/>     => <see cref="int"/></item>
        /// <item><see cref="g__ETES.StatVariantType.SByte"/>   => <see cref="sbyte"/></item>
        /// <item><see cref="g__ETES.StatVariantType.Short"/>   => <see cref="short"/></item>
        /// <item><see cref="g__ETES.StatVariantType.UInt"/>    => <see cref="uint"/></item>
        /// <item><see cref="g__ETES.StatVariantType.UShort"/>  => <see cref="ushort"/></item>
        /// </list>
        /// </summary>
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        public static bool IsCompatiblePair(g__ETES.StatVariantType type)
        {
            return type switch
            {
                   g__ETES.StatVariantType.Bool
                or g__ETES.StatVariantType.Bool2
                or g__ETES.StatVariantType.Bool2x2
                or g__ETES.StatVariantType.Bool3
                or g__ETES.StatVariantType.Bool4
                or g__ETES.StatVariantType.Byte
                or g__ETES.StatVariantType.Float
                or g__ETES.StatVariantType.Half
                or g__ETES.StatVariantType.Half2
                or g__ETES.StatVariantType.Int
                or g__ETES.StatVariantType.SByte
                or g__ETES.StatVariantType.Short
                or g__ETES.StatVariantType.UInt
                or g__ETES.StatVariantType.UShort
                   => true,
                 _ => false,
            };
        }

    }

#region    IMPL - STAT
#endregion ===========

    partial class StatsApi // Impl: Stat
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct Stat
        {
            [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
            struct StatValueUnion
            {
                [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
                private struct ExposedValuePair
                {
                    public StatDataStore data;

                    public g__ETES.StatVariantType type;

                    public g__ET.ByteBool isPair;

                }

                // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
                [g__SRIS.FieldOffset(0)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
                private ValuePair _valuePair;

                // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
                [g__SRIS.FieldOffset(0)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")]
                private ExposedValuePair _exposed;

                [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
                public static ValuePair Convert(StatDataStore data, bool isPair, g__ETES.StatVariantType type)
                {
                    return new StatValueUnion
                    {
                        _exposed = new ExposedValuePair
                        {
                            data = data,
                            isPair = isPair,
                            type = type,
                        }
                    }._valuePair;
                }

                [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
                public static void Convert(ValuePair valuePair, ref StatDataStore data, ref g__ET.ByteBool isPair, ref g__ETES.StatVariantType type)
                {
                    var exposed = new StatValueUnion { _valuePair = valuePair }._exposed;
                    data = exposed.data;
                    isPair = exposed.isPair;
                    type = exposed.type;
                }

            }
        }

    }

#region    IMPL - STAT<TSTATDATA>
#endregion ======================

    partial class StatsApi // Impl: Stat<TStatData>
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct Stat<TStatData>
        {
            /// <inheritdoc cref="Stat.UserDataSize"/>
            public const int USER_DATA_SIZE = Stat.USER_DATA_SIZE;

            [g__UE.SerializeField]
            private Stat _value;

            static Stat()
            {
                ThrowIfNotCompatible(default(TStatData));
            }

            [g__SRCS.MethodImpl(INLINING)]
            public Stat(in Stat value)
            {
                _value = value;
            }

            public g__ETES.ModifierRange ModifierRange
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _value.ModifierRange;

                [g__SRCS.MethodImpl(INLINING)]
                set => _value.ModifierRange = value;
            }

            public g__ETES.ObserverRange ObserverRange
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _value.ObserverRange;

                [g__SRCS.MethodImpl(INLINING)]
                set => _value.ObserverRange = value;
            }

            public bool ProduceChangeEvents
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _value.ProduceChangeEvents;

                [g__SRCS.MethodImpl(INLINING)]
                set => _value.ProduceChangeEvents = value;
            }

            public TStatData Data
            {
                readonly get
                {
                    var valuePair = _value.ValuePair;
                    var result = new TStatData { BaseValue = valuePair.GetBaseValueOrDefault() };

                    if (result.IsValuePair)
                    {
                        result.CurrentValue = valuePair.GetCurrentValueOrDefault();
                    }

                    return result;
                }

                set
                {
                    var stat = _value;

                    if (value.IsValuePair)
                    {
                        stat.TrySetValues(value.BaseValue, value.CurrentValue);
                    }
                    else
                    {
                        stat.TrySetBaseValue(value.BaseValue);
                    }

                    _value = stat;
                }
            }

            public ValuePair ValuePair
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _value.ValuePair;

                [g__SRCS.MethodImpl(INLINING)]
                set
                {
                    var stat = _value;
                    stat.ValuePair = value;
                    _value = stat;
                }
            }

            /// <inheritdoc cref="Stat.UserData"/>
            public uint UserData
            {
                [g__SRCS.MethodImpl(INLINING)]
                readonly get => _value.UserData;

                [g__SRCS.MethodImpl(INLINING)]
                set => _value.UserData = value;
            }

            /// <inheritdoc cref="Stat.UserDataSize"/>
            public readonly int UserDataSize
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => USER_DATA_SIZE;

            }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Stat<TStatData>(in Stat value)
                => new(value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator Stat(in Stat<TStatData> value)
                => value._value;

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatVariant GetBaseValueOrDefault(in g__ETES.StatVariant defaultValue = default)
                => _value.GetBaseValueOrDefault(defaultValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__ETES.StatVariant GetCurrentValueOrDefault(in g__ETES.StatVariant defaultValue = default)
                => _value.GetCurrentValueOrDefault(defaultValue);

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetBaseValue(in g__ETES.StatVariant value)
            {
                var stat = _value;
                var result = stat.TrySetBaseValue(value);
                if (result) _value = stat;
                return result;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetCurrentValue(in g__ETES.StatVariant value)
            {
                var stat = _value;
                var result = stat.TrySetCurrentValue(value);
                if (result) _value = stat;
                return result;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetValues(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
            {
                var stat = _value;
                var result = stat.TrySetValues(baseValue, currentValue);
                if (result) _value = stat;
                return result;
            }

        }

    }

#region    IMPL - API
#endregion ==========

    partial class StatsApi // Impl: API
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial class API
        {
            /// <inheritdoc cref="g__ETES.StatAPI.GetStatComponentTypeSet{TStat, TStatModifier, TStatObserver}()"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static g__UECS.ComponentTypeSet GetStatComponentTypeSet()
                => g__ETES.StatAPI.GetStatComponentTypeSet<Stat, StatModifier, StatObserver>();

            /// <inheritdoc cref="g__ETES.StatAPI.AddStatComponents{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}(g__UECS.Entity, g__UECS.EntityManager)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void AddStatComponents(g__UECS.Entity entity, g__UECS.EntityManager entityManager)
                => g__ETES.StatAPI.AddStatComponents<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver>(entity, entityManager);

            /// <inheritdoc cref="g__ETES.StatAPI.AddStatComponents{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}(g__UECS.Entity, g__UECS.EntityCommandBuffer)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void AddStatComponents(g__UECS.Entity entity, g__UECS.EntityCommandBuffer ecb)
                => g__ETES.StatAPI.AddStatComponents<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver>(entity, ecb);

            /// <inheritdoc cref="g__ETES.StatAPI.AddStatComponents{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}(g__UECS.Entity, g__UECS.EntityCommandBuffer.ParallelWriter, int)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void AddStatComponents(g__UECS.Entity entity, g__UECS.EntityCommandBuffer.ParallelWriter ecb, int sortKey)
                => g__ETES.StatAPI.AddStatComponents<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver>(entity, ecb, sortKey);

            /// <inheritdoc cref="g__ETES.StatAPI.BakeStatComponents{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}(g__UECS.IBaker, g__UECS.Entity, out g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}, TValuePairComposer)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static Baker BakeStatComponents(g__UECS.IBaker baker, g__UECS.Entity entity, ValuePair.Composer valuePairComposer = default)
            {
                g__ETES.StatAPI.BakeStatComponents<ValuePair, Stat, StatModifier, StatModifier.Stack, StatObserver, ValuePair.Composer>(baker, entity, out var statBaker, valuePairComposer);
                return statBaker;
            }

            /// <inheritdoc cref="g__ETES.StatAPI.CreateStatHandle{TValuePair, TStat, TStatData, TValuePairComposer}(g__UECS.Entity, TStatData, bool, uint, ref g__UECS.DynamicBuffer{TStat}, int, int, TValuePairComposer)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static g__ETES.StatHandle<TStatData> CreateStatHandle<TStatData>(g__UECS.Entity entity, TStatData statData, bool produceChangeEvents, uint userData, ref g__UECS.DynamicBuffer<Stat> statBuffer, int modifierRangeStart, int observerRangeStart, ValuePair.Composer valuePairComposer = default)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.CreateStatHandle<ValuePair, Stat, TStatData, ValuePair.Composer>(entity, statData, produceChangeEvents, userData, ref statBuffer, modifierRangeStart, observerRangeStart, valuePairComposer);

            /// <inheritdoc cref="g__ETES.StatAPI.CreateStatHandle{TValuePair, TStat, TStatData, TValuePairComposer}(g__UECS.Entity, TValuePair, bool, uint, ref g__UECS.DynamicBuffer{TStat}, int, int, out TStatData, TValuePairComposer)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static g__ETES.StatHandle<TStatData> CreateStatHandle<TStatData>(g__UECS.Entity entity, ValuePair valuePair, bool produceChangeEvents, uint userData, ref g__UECS.DynamicBuffer<Stat> statBuffer, int modifierRangeStart, int observerRangeStart, out TStatData statData, ValuePair.Composer valuePairComposer = default)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.CreateStatHandle<ValuePair, Stat, TStatData, ValuePair.Composer>(entity, valuePair, produceChangeEvents, userData, ref statBuffer, modifierRangeStart, observerRangeStart, out statData, valuePairComposer);

            /// <inheritdoc cref="g__ETES.StatAPI.CreateStatHandle{TValuePair, TStat}(g__UECS.Entity, TValuePair, bool, uint, ref g__UECS.DynamicBuffer{TStat}, int, int)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static g__ETES.StatHandle CreateStatHandle(g__UECS.Entity entity, ValuePair valuePair, bool produceChangeEvents, uint userData, ref g__UECS.DynamicBuffer<Stat> statBuffer, int modifierRangeStart, int observerRangeStart)
                => g__ETES.StatAPI.CreateStatHandle<ValuePair, Stat>(entity, valuePair, produceChangeEvents, userData, ref statBuffer, modifierRangeStart, observerRangeStart);

            /// <inheritdoc cref="g__ETES.StatAPI.Contains{TValuePair, TStat}(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool Contains(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer)
                => g__ETES.StatAPI.Contains<ValuePair, Stat>(statHandle, statBuffer);

            /// <inheritdoc cref="g__ETES.StatAPI.Contains{TValuePair, TStat}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool Contains(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats)
                => g__ETES.StatAPI.Contains<ValuePair, Stat>(statHandle, lookupStats);

            /// <inheritdoc cref="g__ETES.StatAPI.Contains{TValuePair, TStat}(g__ETES.StatHandle, uint, g__S.ReadOnlySpan{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool Contains(g__ETES.StatHandle statHandle, uint userData, g__S.ReadOnlySpan<Stat> statBuffer)
                => g__ETES.StatAPI.Contains<ValuePair, Stat>(statHandle, userData, statBuffer);

            /// <inheritdoc cref="g__ETES.StatAPI.Contains{TValuePair, TStat}(g__ETES.StatHandle, uint, g__UECS.BufferLookup{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool Contains(g__ETES.StatHandle statHandle, uint userData, g__UECS.BufferLookup<Stat> lookupStats)
                => g__ETES.StatAPI.Contains<ValuePair, Stat>(statHandle, userData, lookupStats);

            /// <inheritdoc cref="g__ETES.StatAPI.EntityHasAnyOtherDependantStatEntities{TStatObserver}(g__UECS.Entity, g__UECS.BufferLookup{TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void EntityHasAnyOtherDependantStatEntities(g__UECS.Entity entity, g__UECS.BufferLookup<StatObserver> lookupObservers)
                => g__ETES.StatAPI.EntityHasAnyOtherDependantStatEntities<StatObserver>(entity, lookupObservers);

            /// <inheritdoc cref="g__ETES.StatAPI.EntityHasAnyOtherDependantStatEntities{TStatObserver}(g__UECS.Entity, g__S.ReadOnlySpan{TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void EntityHasAnyOtherDependantStatEntities(g__UECS.Entity entity, g__S.ReadOnlySpan<StatObserver> observerBufferOnEntity)
                => g__ETES.StatAPI.EntityHasAnyOtherDependantStatEntities<StatObserver>(entity, observerBufferOnEntity);

            /// <inheritdoc cref="g__ETES.StatAPI.GetOtherDependantStatEntitiesOfEntity{TStatObserver}(g__UECS.Entity, g__UECS.BufferLookup{TStatObserver}, g__UC.NativeHashSet{g__UECS.Entity})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void GetOtherDependantStatEntitiesOfEntity(g__UECS.Entity entity, g__UECS.BufferLookup<StatObserver> lookupObservers, g__UC.NativeHashSet<g__UECS.Entity> dependentEntities)
                => g__ETES.StatAPI.GetOtherDependantStatEntitiesOfEntity<StatObserver>(entity, lookupObservers, dependentEntities);

            /// <inheritdoc cref="g__ETES.StatAPI.GetOtherDependantStatEntitiesOfEntity{TStatObserver}(g__UECS.Entity, g__S.ReadOnlySpan{TStatObserver}, g__UC.NativeHashSet{g__UECS.Entity})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void GetOtherDependantStatEntitiesOfEntity(g__UECS.Entity entity, g__S.ReadOnlySpan<StatObserver> observerBufferOnEntity, g__UC.NativeHashSet<g__UECS.Entity> dependentEntities)
                => g__ETES.StatAPI.GetOtherDependantStatEntitiesOfEntity<StatObserver>(entity, observerBufferOnEntity, dependentEntities);

            /// <inheritdoc cref="g__ETES.StatAPI.GetOtherDependantStatsOfEntity{TStatObserver}(g__UECS.Entity, g__UECS.BufferLookup{TStatObserver}, g__UC.NativeList{g__ETES.StatHandle})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void GetOtherDependantStatsOfEntity(g__UECS.Entity entity, g__UECS.BufferLookup<StatObserver> lookupObservers, g__UC.NativeList<g__ETES.StatHandle> dependentStats)
                => g__ETES.StatAPI.GetOtherDependantStatsOfEntity<StatObserver>(entity, lookupObservers, dependentStats);

            /// <inheritdoc cref="g__ETES.StatAPI.GetOtherDependantStatsOfEntity{TStatObserver}(g__UECS.Entity, g__S.ReadOnlySpan{TStatObserver}, g__UC.NativeList{g__ETES.StatHandle})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void GetOtherDependantStatsOfEntity(g__UECS.Entity entity, g__S.ReadOnlySpan<StatObserver> observerBufferOnEntity, g__UC.NativeList<g__ETES.StatHandle> dependentStats)
                => g__ETES.StatAPI.GetOtherDependantStatsOfEntity<StatObserver>(entity, observerBufferOnEntity, dependentStats);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStatData{TValuePair, TStat, TStatData}(g__ETES.StatHandle{TStatData}, g__S.ReadOnlySpan{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static TStatData GetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.GetStatData<ValuePair, Stat, TStatData>(statHandle, statBuffer);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStat{TValuePair, TStat}(g__ETES.StatHandle{TStatData}, g__S.ReadOnlySpan{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static Stat<TStatData> GetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.GetStat<ValuePair, Stat>(statHandle, statBuffer);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStat{TValuePair, TStat}(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static Stat GetStat(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer)
                => g__ETES.StatAPI.GetStat<ValuePair, Stat>(statHandle, statBuffer);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStatValue{TValuePair, TStat}(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static ValuePair GetStatValue(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer)
                => g__ETES.StatAPI.GetStatValue<ValuePair, Stat>(statHandle, statBuffer);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStatEntitiesThatEntityDependsOn{TValuePair, TStat, TStatModifier, TStatModifierStack}(g__UECS.Entity, g__UECS.BufferLookup{TStatModifier}, g__UC.NativeHashSet{g__UECS.Entity}, g__UC.NativeList{g__ETES.StatHandle})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void GetStatEntitiesThatEntityDependsOn(g__UECS.Entity entity, g__UECS.BufferLookup<StatModifier> lookupModifiers, g__UC.NativeHashSet<g__UECS.Entity> dependsOnEntities, g__UC.NativeList<g__ETES.StatHandle> observerStatHandles)
                => g__ETES.StatAPI.GetStatEntitiesThatEntityDependsOn<ValuePair, Stat, StatModifier, StatModifier.Stack>(entity, lookupModifiers, dependsOnEntities, observerStatHandles);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStatEntitiesThatEntityDependsOn{TValuePair, TStat, TStatModifier, TStatModifierStack}(g__S.ReadOnlySpan{TStatModifier}, g__UC.NativeHashSet{g__UECS.Entity}, g__UC.NativeList{g__ETES.StatHandle})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static void GetStatEntitiesThatEntityDependsOn(g__S.ReadOnlySpan<StatModifier> modifierBufferOnEntity, g__UC.NativeHashSet<g__UECS.Entity> dependsOnEntities, g__UC.NativeList<g__ETES.StatHandle> observerStatHandles)
                => g__ETES.StatAPI.GetStatEntitiesThatEntityDependsOn<ValuePair, Stat, StatModifier, StatModifier.Stack>(modifierBufferOnEntity, dependsOnEntities, observerStatHandles);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetAllObservers{TStatObserver}(g__UECS.Entity, g__UECS.BufferLookup{TStatObserver}, g__UC.NativeList{TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetAllObservers(g__UECS.Entity entity, g__UECS.BufferLookup<StatObserver> lookupObservers, g__UC.NativeList<StatObserver> observers)
                => g__ETES.StatAPI.TryGetAllObservers<StatObserver>(entity, lookupObservers, observers);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetModifiersOfStat{TValuePair, TStat, TStatModifier, TStatModifierStack}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, g__UECS.BufferLookup{TStatModifier}, g__UC.NativeList{g__ETES.StatModifierRecord{TValuePair, TStat, TStatModifier, TStatModifierStack}})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetModifiersOfStat(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats, g__UECS.BufferLookup<StatModifier> lookupModifiers, g__UC.NativeList<StatModifierRecord> modifiers)
            {
                ref var modifiersTemp = ref g__UCLU.UnsafeUtility.As<g__UC.NativeList<StatModifierRecord>, g__UC.NativeList<g__ETES.StatModifierRecord<ValuePair, Stat, StatModifier, StatModifier.Stack>>>(ref modifiers);
                return g__ETES.StatAPI.TryGetModifiersOfStat<ValuePair, Stat, StatModifier, StatModifier.Stack>(statHandle, lookupStats, lookupModifiers, modifiersTemp);
            }

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetObserversOfStat{TValuePair, TStat, TStatObserver}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, g__UECS.BufferLookup{TStatObserver}, g__UC.NativeList{TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetObserversOfStat(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats, g__UECS.BufferLookup<StatObserver> lookupObservers, g__UC.NativeList<StatObserver> observers)
                => g__ETES.StatAPI.TryGetObserversOfStat<ValuePair, Stat, StatObserver>(statHandle, lookupStats, lookupObservers, observers);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetModifierCount{TValuePair, TStat}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, out int)"/>
            [g__SRCS.MethodImpl(INLINING)][g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
            public static bool TryGetModifierCount(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats, out int modifierCount)
                => g__ETES.StatAPI.TryGetModifierCount<ValuePair, Stat>(statHandle, lookupStats, out modifierCount);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetObserverCount{TValuePair, TStat}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, out int)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetObserverCount(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats, out int observerCount)
                => g__ETES.StatAPI.TryGetObserverCount<ValuePair, Stat>(statHandle, lookupStats, out observerCount);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStatData{TValuePair, TStat, TStatData}(g__ETES.StatHandle{TStatData}, g__UECS.BufferLookup{TStat}, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__UECS.BufferLookup<Stat> lookupStats, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.TryGetStatData<ValuePair, Stat, TStatData>(statHandle, lookupStats, out statData);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStatData{TValuePair, TStat, TStatData}(g__ETES.StatHandle{TStatData}, g__S.ReadOnlySpan{TStat}, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.TryGetStatData<ValuePair, Stat, TStatData>(statHandle, statBuffer, out statData);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStat{TValuePair, TStat}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__UECS.BufferLookup<Stat> lookupStats, out Stat<TStatData> stat)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                var result = g__ETES.StatAPI.TryGetStat<ValuePair, Stat>(statHandle, lookupStats, out var statUntyped);
                stat = statUntyped;
                return result;
            }

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStat{TValuePair, TStat}(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out Stat<TStatData> stat)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                var result = g__ETES.StatAPI.TryGetStat<ValuePair, Stat>(statHandle, statBuffer, out var statUntyped);
                stat = statUntyped;
                return result;
            }

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStat{TValuePair, TStat}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStat(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats, out Stat stat)
                => g__ETES.StatAPI.TryGetStat<ValuePair, Stat>(statHandle, lookupStats, out stat);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStat{TValuePair, TStat}(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStat(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out Stat stat)
                => g__ETES.StatAPI.TryGetStat<ValuePair, Stat>(statHandle, statBuffer, out stat);

            /// <inheritdoc cref="g__ETES.StatAPI.GetStats{TValuePair, TStat}(g__UECS.Entity, g__UECS.BufferLookup{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static g__UC.NativeArray<Stat>.ReadOnly GetStats(g__UECS.Entity entity, g__UECS.BufferLookup<Stat> lookupStats)
                => g__ETES.StatAPI.GetStats<ValuePair, Stat>(entity, lookupStats);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStatValue{TValuePair, TStat}(g__ETES.StatHandle, g__UECS.BufferLookup{TStat}, out TValuePair)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStatValue(g__ETES.StatHandle statHandle, g__UECS.BufferLookup<Stat> lookupStats, out ValuePair valuePair)
                => g__ETES.StatAPI.TryGetStatValue<ValuePair, Stat>(statHandle, lookupStats, out valuePair);

            /// <inheritdoc cref="g__ETES.StatAPI.TryGetStatValue{TValuePair, TStat}(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TValuePair)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetStatValue(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out ValuePair valuePair)
                => g__ETES.StatAPI.TryGetStatValue<ValuePair, Stat>(statHandle, statBuffer, out valuePair);

            /// <inheritdoc cref="g__ETES.StatAPI.MakeStatData{TValuePair, TStatData}(TValuePair)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public static TStatData MakeStatData<TStatData>(ValuePair valuePair)
                where TStatData : unmanaged, g__ETES.IStatData
                => g__ETES.StatAPI.MakeStatData<ValuePair, TStatData>(valuePair);

        }

    }

#region    IMPL - READER
#endregion =============

    partial class StatsApi // Impl: Reader
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct Reader
        {
            public readonly bool IsCreated
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => this.reader.IsCreated;
            }

            public readonly bool UseLookup
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => this.reader.UseLookup;
            }

            /// <inheritdoc cref="g__ETES.StatReader{TValuePair, TStat}.Contains(g__ETES.StatHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Contains(g__ETES.StatHandle statHandle)
                => this.reader.Contains(statHandle);

            /// <inheritdoc cref="g__ETES.StatReader{TValuePair, TStat}.Contains(g__ETES.StatHandle, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Contains(g__ETES.StatHandle statHandle, uint userData)
                => this.reader.Contains(statHandle, userData);

            /// <inheritdoc cref="g__ETES.StatReader{TValuePair, TStat}.TryGetStatData{TStatData}(g__ETES.StatHandle{TStatData}, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.reader.TryGetStatData<TStatData>(statHandle, out statData);

            /// <inheritdoc cref="g__ETES.StatReader{TValuePair, TStat}.TryGetStat{TStatData}(g__ETES.StatHandle{TStatData}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, out Stat<TStatData> stat)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                var result = this.reader.TryGetStat(statHandle, out var statUntyped);
                stat = statUntyped;
                return result;
            }

            /// <inheritdoc cref="g__ETES.StatReader{TValuePair, TStat}.TryGetStat(g__ETES.StatHandle, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStat(g__ETES.StatHandle statHandle, out Stat stat)
                => this.reader.TryGetStat(statHandle, out stat);

            /// <inheritdoc cref="g__ETES.StatReader{TValuePair, TStat}.TryGetStatValue(g__ETES.StatHandle, out TValuePair)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatValue(g__ETES.StatHandle statHandle, out ValuePair valuePair)
                => this.reader.TryGetStatValue(statHandle, out valuePair);

        }

    }

#region    IMPL - ACCESSOR
#endregion ===============

    partial class StatsApi // Impl: Accessor
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct Accessor
        {
            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryAddStatModifier(g__ETES.StatHandle, TStatModifier, out g__ETES.StatModifierHandle, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryAddStatModifier(g__ETES.StatHandle affectedStatHandle, StatModifier statModifier, out g__ETES.StatModifierHandle statModifierHandle, ref WorldData worldData)
                => this.accessor.TryAddStatModifier(affectedStatHandle, statModifier, out statModifierHandle, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryAddStatModifiersBatch(g__ETES.StatHandle, g__S.ReadOnlySpan{TStatModifier}, g__UC.NativeList{g__ETES.StatModifierHandle}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryAddStatModifiersBatch(g__ETES.StatHandle affectedStatHandle, g__S.ReadOnlySpan<StatModifier> modifiers, g__UC.NativeList<g__ETES.StatModifierHandle> statModifierHandles, ref WorldData worldData)
                => this.accessor.TryAddStatModifiersBatch(affectedStatHandle, modifiers, statModifierHandles, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryCreateStatHandle{TStatData}(g__UECS.Entity, TValuePair, bool, uint, out g__ETES.StatHandle{TStatData}, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryCreateStatHandle<TStatData>(g__UECS.Entity entity, ValuePair valuePair, bool produceChangeEvents, uint userData, out g__ETES.StatHandle<TStatData> statHandle, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TryCreateStatHandle<TStatData>(entity, valuePair, produceChangeEvents, userData, out statHandle, out statData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryCreateStatHandle{TStatData}(g__UECS.Entity, TValuePair, bool, uint, out g__ETES.StatHandle{TStatData})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryCreateStatHandle<TStatData>(g__UECS.Entity entity, ValuePair valuePair, bool produceChangeEvents, uint userData, out g__ETES.StatHandle<TStatData> statHandle)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TryCreateStatHandle<TStatData>(entity, valuePair, produceChangeEvents, userData, out statHandle);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryCreateStatHandle{TStatData}(g__UECS.Entity, TStatData, bool, uint, out g__ETES.StatHandle{TStatData})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryCreateStatHandle<TStatData>(g__UECS.Entity entity, TStatData statData, bool produceChangeEvents, uint userData, out g__ETES.StatHandle<TStatData> statHandle)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TryCreateStatHandle<TStatData>(entity, statData, produceChangeEvents, userData, out statHandle);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryCreateStatHandle(g__UECS.Entity, TValuePair, bool, uint, out g__ETES.StatHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryCreateStatHandle(g__UECS.Entity entity, ValuePair valuePair, bool produceChangeEvents, uint userData, out g__ETES.StatHandle statHandle)
                => this.accessor.TryCreateStatHandle(entity, valuePair, produceChangeEvents, userData, out statHandle);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetAllObservers(g__UECS.Entity, g__UC.NativeList{TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetAllObservers(g__UECS.Entity entity, g__UC.NativeList<StatObserver> observers)
                => this.accessor.TryGetAllObservers(entity, observers);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStatData{TStatData}(g__ETES.StatHandle{TStatData}, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TryGetStatData<TStatData>(statHandle, out statData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStatData{TStatData}(g__ETES.StatHandle{TStatData}, g__S.ReadOnlySpan{TStat}, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TryGetStatData<TStatData>(statHandle, statBuffer, out statData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStat(g__ETES.StatHandle, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, out Stat<TStatData> stat)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                var result = this.accessor.TryGetStat(statHandle, out var statUntyped);
                stat = statUntyped;
                return result;
            }

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStat(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out Stat<TStatData> stat)
                where TStatData : unmanaged, g__ETES.IStatData
            {
                var result = this.accessor.TryGetStat(statHandle, statBuffer, out var statUntyped);
                stat = statUntyped;
                return result;
            }

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStat(g__ETES.StatHandle, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStat(g__ETES.StatHandle statHandle, out Stat stat)
                => this.accessor.TryGetStat(statHandle, out stat);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStat(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TStat)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStat(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out Stat stat)
                => this.accessor.TryGetStat(statHandle, statBuffer, out stat);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.GetStats(g__UECS.Entity)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__UC.NativeArray<Stat>.ReadOnly GetStats(g__UECS.Entity entity)
                => this.accessor.GetStats(entity);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStatValue(g__ETES.StatHandle, out TValuePair)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatValue(g__ETES.StatHandle statHandle, out ValuePair valuePair)
                => this.accessor.TryGetStatValue(statHandle, out valuePair);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStatValue(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TValuePair)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatValue(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out ValuePair valuePair)
                => this.accessor.TryGetStatValue(statHandle, statBuffer, out valuePair);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetStatModifier(g__ETES.StatModifierHandle, out TStatModifier)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetStatModifier(g__ETES.StatModifierHandle modifierHandle, out StatModifier statModifier)
                => this.accessor.TryGetStatModifier(modifierHandle, out statModifier);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetModifiersOfStat(g__ETES.StatHandle, g__UC.NativeList{g__ETES.StatModifierRecord{TValuePair, TStat, TStatModifier, TStatModifierStack}})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetModifiersOfStat(g__ETES.StatHandle statHandle, g__UC.NativeList<StatModifierRecord> modifiers)
            {
                ref var modifiersTemp = ref g__UCLU.UnsafeUtility.As<g__UC.NativeList<StatModifierRecord>, g__UC.NativeList<g__ETES.StatModifierRecord<ValuePair, Stat, StatModifier, StatModifier.Stack>>>(ref modifiers);
                return this.accessor.TryGetModifiersOfStat(statHandle, modifiersTemp);
            }

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetObserversOfStat(g__ETES.StatHandle, g__UC.NativeList<StatObserver>)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetObserversOfStat(g__ETES.StatHandle statHandle, g__UC.NativeList<StatObserver> observers)
            {
                return this.accessor.TryGetObserversOfStat(statHandle, observers);
            }

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetModifierCount(g__ETES.StatHandle, out int)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetModifierCount(g__ETES.StatHandle statHandle, out int modifierCount)
                => this.accessor.TryGetModifierCount(statHandle, out modifierCount);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryGetObserverCount(g__ETES.StatHandle, out int)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool TryGetObserverCount(g__ETES.StatHandle statHandle, out int observerCount)
                => this.accessor.TryGetObserverCount(statHandle, out observerCount);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryRemoveModifiersOfStat(g__ETES.StatHandle, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryRemoveModifiersOfStat(g__ETES.StatHandle statHandle, ref WorldData worldData)
                => this.accessor.TryRemoveModifiersOfStat(statHandle, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryRemoveStatModifier(g__ETES.StatModifierHandle, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryRemoveStatModifier(g__ETES.StatModifierHandle modifierHandle, ref WorldData worldData)
                => this.accessor.TryRemoveStatModifier(modifierHandle, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatData{TStatData}(in g__ETES.StatDataParams{TStatData}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatData<TStatData>(in g__ETES.StatDataParams<TStatData> statParams, ref WorldData worldData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TrySetStatData(statParams, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatData(in g__ETES.StatValueParams{TValuePair}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatData(in g__ETES.StatValueParams<ValuePair> statParams, ref WorldData worldData)
                => this.accessor.TrySetStatData(statParams, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetBaseValueToStats(ReadOnlySpan{g__ETES.StatValueParams{TValuePair}}, Span{bool}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void TrySetBaseValueToStats(ReadOnlySpan<g__ETES.StatValueParams<ValuePair>> paramsForStats, Span<bool> results, ref WorldData worldData)
                => this.accessor.TrySetBaseValueToStats(paramsForStats, results, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetCurrentValueToStats(ReadOnlySpan{g__ETES.StatValueParams{TValuePair}}, Span{bool}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void TrySetCurrentValueToStats(ReadOnlySpan<g__ETES.StatValueParams<ValuePair>> paramsForStats, Span<bool> results, ref WorldData worldData)
                => this.accessor.TrySetCurrentValueToStats(paramsForStats, results, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetDataToStats(ReadOnlySpan{g__ETES.StatValueParams{TValuePair}}, Span{bool}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void TrySetDataToStats(ReadOnlySpan<g__ETES.StatValueParams<ValuePair>> paramsForStats, Span<bool> results, ref WorldData worldData)
                => this.accessor.TrySetDataToStats(paramsForStats, results, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatBaseValue(g__ETES.StatHandle, in TValuePair, ref g__UECS.DynamicBuffer{TStat}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatBaseValue(g__ETES.StatHandle statHandle, in ValuePair value, ref g__UECS.DynamicBuffer<Stat> statBuffer, ref WorldData worldData)
                => this.accessor.TrySetStatBaseValue(statHandle, value, ref statBuffer, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatBaseValue(g__ETES.StatHandle, in TValuePair, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatBaseValue(g__ETES.StatHandle statHandle, in ValuePair value, ref WorldData worldData)
                => this.accessor.TrySetStatBaseValue(statHandle, value, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatCurrentValue(g__ETES.StatHandle, in TValuePair, ref g__UECS.DynamicBuffer{TStat}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatCurrentValue(g__ETES.StatHandle statHandle, in ValuePair value, ref g__UECS.DynamicBuffer<Stat> statBuffer, ref WorldData worldData)
                => this.accessor.TrySetStatCurrentValue(statHandle, value, ref statBuffer, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatCurrentValue(g__ETES.StatHandle, in TValuePair, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatCurrentValue(g__ETES.StatHandle statHandle, in ValuePair value, ref WorldData worldData)
                => this.accessor.TrySetStatCurrentValue(statHandle, value, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatData{TStatData}(g__ETES.StatHandle{TStatData}, TStatData, ref g__UECS.DynamicBuffer{TStat}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, TStatData statData, ref g__UECS.DynamicBuffer<Stat> statBuffer, ref WorldData worldData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TrySetStatData<TStatData>(statHandle, statData, ref statBuffer, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatData{TStatData}(g__ETES.StatHandle{TStatData}, TStatData, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, TStatData statData, ref WorldData worldData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.accessor.TrySetStatData<TStatData>(statHandle, statData, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatValues(g__ETES.StatHandle, in TValuePair, in g__ETES.StatVariant, ref g__UECS.DynamicBuffer{TStat}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatValues(g__ETES.StatHandle statHandle, in ValuePair value, ref g__UECS.DynamicBuffer<Stat> statBuffer, ref WorldData worldData)
                => this.accessor.TrySetStatValues(statHandle, value, ref statBuffer, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatValues(g__ETES.StatHandle, in TValuePair, in g__ETES.StatVariant, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatValues(g__ETES.StatHandle statHandle, in ValuePair value, ref WorldData worldData)
                => this.accessor.TrySetStatValues(statHandle, value, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatValues(g__ETES.StatHandle, in g__ETES.StatVariant, in g__ETES.StatVariant, ref g__UECS.DynamicBuffer{TStat}, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatValues(g__ETES.StatHandle statHandle, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue, ref g__UECS.DynamicBuffer<Stat> statBuffer, ref WorldData worldData)
                => this.accessor.TrySetStatValues(statHandle, baseValue, currentValue, ref statBuffer, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatValues(g__ETES.StatHandle, in g__ETES.StatVariant, in g__ETES.StatVariant, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatValues(g__ETES.StatHandle statHandle, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue, ref WorldData worldData)                => this.accessor.TrySetStatValues(statHandle, baseValue, currentValue, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatProduceChangeEvents(g__ETES.StatHandle, bool)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatProduceChangeEvents(g__ETES.StatHandle statHandle, bool value)
                => this.accessor.TrySetStatProduceChangeEvents(statHandle, value);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatProduceChangeEvents(g__ETES.StatHandle, bool, ref g__UECS.DynamicBuffer{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatProduceChangeEvents(g__ETES.StatHandle statHandle, bool value, ref g__UECS.DynamicBuffer<Stat> statBuffer)
                => this.accessor.TrySetStatProduceChangeEvents(statHandle, value, ref statBuffer);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatUserData(g__ETES.StatHandle, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatUserData(g__ETES.StatHandle statHandle, uint value)
                => this.accessor.TrySetStatUserData(statHandle, value);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TrySetStatUserData(g__ETES.StatHandle, uint, ref g__UECS.DynamicBuffer{TStat})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TrySetStatUserData(g__ETES.StatHandle statHandle, uint value, ref g__UECS.DynamicBuffer<Stat> statBuffer)
                => this.accessor.TrySetStatUserData(statHandle, value, ref statBuffer);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryUpdateAllStats(g__UECS.Entity, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void TryUpdateAllStats(g__UECS.Entity entity, ref WorldData worldData)
                => this.accessor.TryUpdateAllStats(entity, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryUpdateStat(g__ETES.StatHandle, ref g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void TryUpdateStat(g__ETES.StatHandle statHandle, ref WorldData worldData)
                => this.accessor.TryUpdateStat(statHandle, ref worldData.worldData);

            /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.Update(ref g__UECS.SystemState)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void Update(ref g__UECS.SystemState state)
                => this.accessor.Update(ref state);

        }

    }

#region    IMPL - ACCESSOR READ-ONLY
#endregion =========================

    partial class StatsApi // Impl: Accessor.ReadOnly
    {
        partial struct Accessor // ReadOnly
        {
            [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
            partial struct ReadOnly
            {
                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetAllObservers(g__UECS.Entity, g__UC.NativeList{TStatObserver})"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetAllObservers(g__UECS.Entity entity, g__UC.NativeList<StatObserver> observers)
                    => this.accessor.TryGetAllObservers(entity, observers);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStatData{TStatData}(g__ETES.StatHandle{TStatData}, out TStatData)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, out TStatData statData)
                    where TStatData : unmanaged, g__ETES.IStatData
                    => this.accessor.TryGetStatData<TStatData>(statHandle, out statData);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStatData{TStatData}(g__ETES.StatHandle{TStatData}, g__S.ReadOnlySpan{TStat}, out TStatData)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStatData<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out TStatData statData)
                    where TStatData : unmanaged, g__ETES.IStatData
                    => this.accessor.TryGetStatData<TStatData>(statHandle, statBuffer, out statData);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStat(g__ETES.StatHandle, out TStat)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, out Stat<TStatData> stat)
                    where TStatData : unmanaged, g__ETES.IStatData
                {
                    var result = this.accessor.TryGetStat(statHandle, out var statUntyped);
                    stat = statUntyped;
                    return result;
                }

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStat(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TStat)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStat<TStatData>(g__ETES.StatHandle<TStatData> statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out Stat<TStatData> stat)
                    where TStatData : unmanaged, g__ETES.IStatData
                {
                    var result = this.accessor.TryGetStat(statHandle, statBuffer, out var statUntyped);
                    stat = statUntyped;
                    return result;
                }

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStat(g__ETES.StatHandle, out TStat)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStat(g__ETES.StatHandle statHandle, out Stat stat)
                    => this.accessor.TryGetStat(statHandle, out stat);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStat(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TStat)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStat(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out Stat stat)
                    => this.accessor.TryGetStat(statHandle, statBuffer, out stat);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.GetStats(g__UECS.Entity)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly g__UC.NativeArray<Stat>.ReadOnly GetStats(g__UECS.Entity entity)
                    => this.accessor.GetStats(entity);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStatValue(g__ETES.StatHandle, out TValuePair)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStatValue(g__ETES.StatHandle statHandle, out ValuePair valuePair)
                    => this.accessor.TryGetStatValue(statHandle, out valuePair);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStatValue(g__ETES.StatHandle, g__S.ReadOnlySpan{TStat}, out TValuePair)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStatValue(g__ETES.StatHandle statHandle, g__S.ReadOnlySpan<Stat> statBuffer, out ValuePair valuePair)
                    => this.accessor.TryGetStatValue(statHandle, statBuffer, out valuePair);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetStatModifier(g__ETES.StatModifierHandle, out TStatModifier)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetStatModifier(g__ETES.StatModifierHandle modifierHandle, out StatModifier statModifier)
                    => this.accessor.TryGetStatModifier(modifierHandle, out statModifier);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetModifiersOfStat(g__ETES.StatHandle, g__UC.NativeList{g__ETES.StatModifierRecord{TValuePair, TStat, TStatModifier, TStatModifierStack}})"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetModifiersOfStat(g__ETES.StatHandle statHandle, g__UC.NativeList<StatModifierRecord> modifiers)
                {
                    ref var modifiersTemp = ref g__UCLU.UnsafeUtility.As<g__UC.NativeList<StatModifierRecord>, g__UC.NativeList<g__ETES.StatModifierRecord<ValuePair, Stat, StatModifier, StatModifier.Stack>>>(ref modifiers);
                    return this.accessor.TryGetModifiersOfStat(statHandle, modifiersTemp);
                }

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetObserversOfStat(g__ETES.StatHandle, g__UC.NativeList<StatObserver>)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetObserversOfStat(g__ETES.StatHandle statHandle, g__UC.NativeList<StatObserver> observers)
                {
                    return this.accessor.TryGetObserversOfStat(statHandle, observers);
                }

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetModifierCount(g__ETES.StatHandle, out int)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetModifierCount(g__ETES.StatHandle statHandle, out int modifierCount)
                    => this.accessor.TryGetModifierCount(statHandle, out modifierCount);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.TryGetObserverCount(g__ETES.StatHandle, out int)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public readonly bool TryGetObserverCount(g__ETES.StatHandle statHandle, out int observerCount)
                    => this.accessor.TryGetObserverCount(statHandle, out observerCount);

                /// <inheritdoc cref="g__ETES.StatAccessor{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.ReadOnly.Update(ref g__UECS.SystemState)"/>
                [g__SRCS.MethodImpl(INLINING)]
                public void Update(ref g__UECS.SystemState state)
                    => this.accessor.Update(ref state);

            }
        }

    }

#region    IMPL - BAKER
#endregion ============

    partial class StatsApi // Impl: Baker
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct Baker
        {
            public readonly IBaker IBaker
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => this.baker.IBaker;
            }

            public readonly Entity Entity
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => this.baker.Entity;
            }

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.Clear(TValuePairComposer)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void Clear(ValuePair.Composer valuePairComposer = default)
                => this.baker.Clear(valuePairComposer);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.CreateStatHandle{TStatData}(TStatData, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle<TStatData> CreateStatHandle<TStatData>(TStatData statData, bool produceChangeEvents, uint userData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.CreateStatHandle<TStatData>(statData, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.CreateStatHandle{TStatData}(TValuePair, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle<TStatData> CreateStatHandle<TStatData>(ValuePair valuePair, bool produceChangeEvents, uint userData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.CreateStatHandle<TStatData>(valuePair, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.CreateStatHandle{TStatData}(TValuePair, bool, uint, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle<TStatData> CreateStatHandle<TStatData>(ValuePair valuePair, bool produceChangeEvents, uint userData, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.CreateStatHandle<TStatData>(valuePair, produceChangeEvents, userData, out statData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.CreateStatHandle(TValuePair, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle CreateStatHandle(ValuePair valuePair, bool produceChangeEvents, uint userData)
                => this.baker.CreateStatHandle(valuePair, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.Contains(g__ETES.StatHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool Contains(g__ETES.StatHandle statHandle)
                => this.baker.Contains(statHandle);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.Contains(g__ETES.StatHandle, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool Contains(g__ETES.StatHandle statHandle, uint userData)
                => this.baker.Contains(statHandle, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.SetStatOrCreateHandle{TStatData}(g__ETES.StatHandle{TStatData}, TStatData, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle<TStatData> SetStatOrCreateHandle<TStatData>(g__ETES.StatHandle<TStatData> handle, TStatData statData, bool produceChangeEvents, uint userData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.SetStatOrCreateHandle(handle, statData, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.SetStatOrCreateHandle{TStatData}(g__ETES.StatHandle{TStatData}, TValuePair, bool, uint, out TStatData)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle<TStatData> SetStatOrCreateHandle<TStatData>(g__ETES.StatHandle<TStatData> handle, ValuePair valuePair, bool produceChangeEvents, uint userData, out TStatData statData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.SetStatOrCreateHandle(handle, valuePair, produceChangeEvents, userData, out statData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.SetStatOrCreateHandle{TStatData}(g__ETES.StatHandle{TStatData}, TValuePair, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle<TStatData> SetStatOrCreateHandle<TStatData>(g__ETES.StatHandle<TStatData> handle, ValuePair valuePair, bool produceChangeEvents, uint userData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.SetStatOrCreateHandle(handle, valuePair, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.SetStatOrCreateHandle(g__ETES.StatHandle, TValuePair, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__ETES.StatHandle SetStatOrCreateHandle(g__ETES.StatHandle handle, ValuePair valuePair, bool produceChangeEvents, uint userData)
                => this.baker.SetStatOrCreateHandle(handle, valuePair, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.SetStat{TStatData}(g__ETES.StatHandle{TStatData}, TStatData, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void SetStat<TStatData>(g__ETES.StatHandle<TStatData> handle, TStatData statData, bool produceChangeEvents, uint userData)
                where TStatData : unmanaged, g__ETES.IStatData
                => this.baker.SetStat(handle, statData, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.SetStat(g__ETES.StatHandle, TValuePair, bool, uint)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void SetStat(g__ETES.StatHandle handle, ValuePair valuePair, bool produceChangeEvents, uint userData)
                => this.baker.SetStat(handle, valuePair, produceChangeEvents, userData);

            /// <inheritdoc cref="g__ETES.StatBaker{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver, TValuePairComposer}.TryAddStatModifier(g__ETES.StatHandle, TStatModifier, out g__ETES.StatModifierHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public bool TryAddStatModifier(g__ETES.StatHandle affectedStatHandle, StatModifier modifier, out g__ETES.StatModifierHandle statModifierHandle)
                => this.baker.TryAddStatModifier(affectedStatHandle, modifier, out statModifierHandle);

        }

    }

#region    IMPL - WORLD DATA
#endregion =================

    partial class StatsApi // Impl: WorldData
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct WorldData
            : global::System.IDisposable
            , global::Unity.Collections.INativeDisposable
        {
            public readonly bool IsCreated
            {
                [g__SRCS.MethodImpl(INLINING)]
                get => this.worldData.IsCreated;
            }

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.AddModifierTriggerEvent(in g__ETES.ModifierTriggerEvent{TValuePair, TStat, TStatModifier, TStatModifierStack})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void AddModifierTriggerEvent(in ModifierTriggerEvent evt)
                => this.worldData.AddModifierTriggerEvent(new(evt.handle, evt.modifier));

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.AddStatChangeEvent(in g__ETES.StatChangeEvent{TValuePair})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void AddStatChangeEvent(in g__ETES.StatChangeEvent<ValuePair> evt)
                => this.worldData.AddStatChangeEvent(evt);

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.ClearModifierTriggerEvents()"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void ClearModifierTriggerEvents()
                => this.worldData.ClearModifierTriggerEvents();

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.ClearStatChangeEvents()"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void ClearStatChangeEvents()
                => this.worldData.ClearStatChangeEvents();

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.Clear()"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void Clear()
                => this.worldData.Clear();

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.Dispose()"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void Dispose()
                => this.worldData.Dispose();

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.Dispose(g__UJ.JobHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public g__UJ.JobHandle Dispose(g__UJ.JobHandle inputDeps)
                => this.worldData.Dispose(inputDeps);

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.GetModifierTriggerEvents(g__UC.AllocatorManager.AllocatorHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__UC.NativeArray<ModifierTriggerEvent> GetModifierTriggerEvents(g__UC.AllocatorManager.AllocatorHandle allocator)
                => this.worldData.GetModifierTriggerEvents(allocator).Reinterpret<ModifierTriggerEvent>();

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.GetModifierTriggerEvents(g__UC.NativeList{g__ETES.ModifierTriggerEvent{TValuePair, TStat, TStatModifier, TStatModifierStack}})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly void GetModifierTriggerEvents(g__UC.NativeList<ModifierTriggerEvent> result)
            {
                ref var resultTemp = ref g__UCLU.UnsafeUtility.As<g__UC.NativeList<ModifierTriggerEvent>, g__UC.NativeList<g__ETES.ModifierTriggerEvent<ValuePair, Stat, StatModifier, StatModifier.Stack>>>(ref result);
                this.worldData.GetModifierTriggerEvents(resultTemp);
            }

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.GetStatChangeEvents(g__UC.AllocatorManager.AllocatorHandle)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly g__UC.NativeArray<g__ETES.StatChangeEvent<ValuePair>> GetStatChangeEvents(g__UC.AllocatorManager.AllocatorHandle allocator)
                => this.worldData.GetStatChangeEvents(allocator);

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.GetStatChangeEvents(g__UC.NativeList{g__ETES.StatChangeEvent{TValuePair}})"/>
            [g__SRCS.MethodImpl(INLINING)]
            public readonly void GetStatChangeEvents(g__UC.NativeList<g__ETES.StatChangeEvent<ValuePair>> result)
                => this.worldData.GetStatChangeEvents(result);

            /// <inheritdoc cref="g__ETES.StatWorldData{TValuePair, TStat, TStatModifier, TStatModifierStack, TStatObserver}.SetStatModifierStack(in TStatModifierStack)"/>
            [g__SRCS.MethodImpl(INLINING)]
            public void SetStatModifierStack(in StatModifier.Stack stack)
                => this.worldData.SetStatModifierStack(stack);
        }

    }

#region    IMPL - VALUE PAIR
#endregion =================

    partial class StatsApi // Impl: ValuePair
    {
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        partial struct ValuePair
        {
            [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
            partial struct Composer { }

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<bool> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool2x2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool2x3> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool2x4> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool3> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool3x2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool4> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.bool4x2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<byte> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<double> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<float> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.float2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.half> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.half2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.half3> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.half4> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<int> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.int2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<long> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<sbyte> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<short> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<uint> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<g__UM.uint2> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<ulong> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__ETES.StatSingle<ushort> value)
                => new(value.Value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(bool value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((bool, bool) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__UM.bool2 value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((g__UM.bool2, g__UM.bool2) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__UM.bool2x2 value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((g__UM.bool2x2, g__UM.bool2x2) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__UM.bool3 value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((g__UM.bool3, g__UM.bool3) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__UM.bool4 value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((g__UM.bool4, g__UM.bool4) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(byte value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((byte, byte) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(float value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((float, float) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__UM.half value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((g__UM.half, g__UM.half) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(g__UM.half2 value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((g__UM.half2, g__UM.half2) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(int value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((int, int) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(sbyte value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((sbyte, sbyte) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(short value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((short, short) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(uint value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((uint, uint) value)
                => new(value.Item1, value.Item2);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair(ushort value)
                => new(value, value);

            [g__SRCS.MethodImpl(INLINING)]
            public static implicit operator ValuePair((ushort, ushort) value)
                => new(value.Item1, value.Item2);

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool2x3' is 6 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.bool2x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool2x4' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.bool2x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool3x2' is 6 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.bool3x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool4x2' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.bool4x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(double value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float2' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.float2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'half3' is 6 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.half3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'half4' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.half4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int2' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.int2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'long' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(long value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint2' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(g__UM.uint2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'ulong' is 8 bytes thus cannot be stored as a pair value.", true)]
            public static implicit operator ValuePair(ulong value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'bool3x3' is 9 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.bool3x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'bool3x4' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.bool3x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'bool4x3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.bool4x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'bool4x4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.bool4x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double2x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double2x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double2x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double2x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double2x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double2x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double3x2' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double3x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double3x3' is 72 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double3x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double3x4' is 96 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double3x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double4x2' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double4x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double4x3' is 96 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double4x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'double4x4' is 128 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.double4x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float2x2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float2x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float2x3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float2x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float2x4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float2x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float3x2' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float3x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float3x3' is 36 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float3x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float3x4' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float3x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float4x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float4x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float4x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float4x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'float4x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.float4x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int2x2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int2x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int2x3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int2x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int2x4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int2x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int3x2' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int3x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int3x3' is 36 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int3x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int3x4' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int3x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int4x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int4x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int4x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int4x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'int4x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.int4x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint2x2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint2x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint2x3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint2x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint2x4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint2x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint3x2' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint3x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint3x3' is 36 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint3x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint3x4' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint3x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint4x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint4x2> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint4x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint4x3> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 8 bytes for a single value, while 'uint4x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__ETES.StatSingle<g__UM.uint4x4> value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool3x3' is 9 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.bool3x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool3x4' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.bool3x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool4x3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.bool4x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'bool4x4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.bool4x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double2x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double2x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double2x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double2x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double2x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double2x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double3x2' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double3x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double3x3' is 72 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double3x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double3x4' is 96 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double3x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double4x2' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double4x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double4x3' is 96 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double4x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'double4x4' is 128 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.double4x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float2x2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float2x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float2x3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float2x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float2x4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float2x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float3x2' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float3x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float3x3' is 36 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float3x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float3x4' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float3x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float4x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float4x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float4x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float4x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'float4x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.float4x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int2x2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int2x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int2x3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int2x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int2x4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int2x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int3x2' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int3x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int3x3' is 36 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int3x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int3x4' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int3x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int4x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int4x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int4x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int4x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'int4x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.int4x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint2x2' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint2x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint2x3' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint2x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint2x4' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint2x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint3' is 12 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint3x2' is 24 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint3x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint3x3' is 36 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint3x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint3x4' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint3x4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint4' is 16 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint4 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint4x2' is 32 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint4x2 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint4x3' is 48 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint4x3 value)
                => default;

            [global::System.Obsolete("'StatsApi' can only store up to 4 bytes for a pair value, while 'uint4x4' is 64 bytes thus incompatible.", true)]
            public static implicit operator ValuePair(in g__UM.uint4x4 value)
                => default;

        }

    }

#region    IMPL - STAT DATA STORE
#endregion ======================

    partial class StatsApi // Impl: StatDataStore
    {
        [g__S.Serializable]
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        internal partial struct StatDataStore
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool _Bool;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool2 _Bool2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool2x2 _Bool2x2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool2x3 _Bool2x3;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool2x4 _Bool2x4;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool3 _Bool3;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool3x2 _Bool3x2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool4 _Bool4;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__ET.ByteBool4x2 _Bool4x2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private byte _Byte;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private double _Double;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private float _Float;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.float2 _Float2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.half _Half;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.half2 _Half2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.half3 _Half3;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.half4 _Half4;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private int _Int;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.int2 _Int2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private long _Long;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private sbyte _SByte;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private short _Short;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private uint _UInt;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private g__UM.uint2 _UInt2;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private ulong _ULong;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__UE.SerializeField][g__SRIS.FieldOffset(0)]
            private ushort _UShort;

            [g__SRCS.MethodImpl(INLINING)]
            public static StatDataStore Store(bool isPair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
            {
                ThrowIfMismatchedTypes(baseValue, currentValue);
                ThrowIfNotCompatible(baseValue, isPair);

                return isPair ? StorePair(baseValue, currentValue) : StoreSingle(baseValue, currentValue);

                static StatDataStore StoreSingle(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
                {
                    return baseValue.Type switch
                    {
                        g__ETES.StatVariantType.Bool => new StatDataStore { _Bool = currentValue.Bool },
                        g__ETES.StatVariantType.Bool2 => new StatDataStore { _Bool2 = currentValue.Bool2 },
                        g__ETES.StatVariantType.Bool2x2 => new StatDataStore { _Bool2x2 = currentValue.Bool2x2 },
                        g__ETES.StatVariantType.Bool2x3 => new StatDataStore { _Bool2x3 = currentValue.Bool2x3 },
                        g__ETES.StatVariantType.Bool2x4 => new StatDataStore { _Bool2x4 = currentValue.Bool2x4 },
                        g__ETES.StatVariantType.Bool3 => new StatDataStore { _Bool3 = currentValue.Bool3 },
                        g__ETES.StatVariantType.Bool3x2 => new StatDataStore { _Bool3x2 = currentValue.Bool3x2 },
                        g__ETES.StatVariantType.Bool4 => new StatDataStore { _Bool4 = currentValue.Bool4 },
                        g__ETES.StatVariantType.Bool4x2 => new StatDataStore { _Bool4x2 = currentValue.Bool4x2 },
                        g__ETES.StatVariantType.Byte => new StatDataStore { _Byte = currentValue.Byte },
                        g__ETES.StatVariantType.Double => new StatDataStore { _Double = currentValue.Double },
                        g__ETES.StatVariantType.Float => new StatDataStore { _Float = currentValue.Float },
                        g__ETES.StatVariantType.Float2 => new StatDataStore { _Float2 = currentValue.Float2 },
                        g__ETES.StatVariantType.Half => new StatDataStore { _Half = currentValue.Half },
                        g__ETES.StatVariantType.Half2 => new StatDataStore { _Half2 = currentValue.Half2 },
                        g__ETES.StatVariantType.Half3 => new StatDataStore { _Half3 = currentValue.Half3 },
                        g__ETES.StatVariantType.Half4 => new StatDataStore { _Half4 = currentValue.Half4 },
                        g__ETES.StatVariantType.Int => new StatDataStore { _Int = currentValue.Int },
                        g__ETES.StatVariantType.Int2 => new StatDataStore { _Int2 = currentValue.Int2 },
                        g__ETES.StatVariantType.Long => new StatDataStore { _Long = currentValue.Long },
                        g__ETES.StatVariantType.SByte => new StatDataStore { _SByte = currentValue.SByte },
                        g__ETES.StatVariantType.Short => new StatDataStore { _Short = currentValue.Short },
                        g__ETES.StatVariantType.UInt => new StatDataStore { _UInt = currentValue.UInt },
                        g__ETES.StatVariantType.UInt2 => new StatDataStore { _UInt2 = currentValue.UInt2 },
                        g__ETES.StatVariantType.ULong => new StatDataStore { _ULong = currentValue.ULong },
                        g__ETES.StatVariantType.UShort => new StatDataStore { _UShort = currentValue.UShort },
                        _ => default,
                    };
                }

                static StatDataStore StorePair(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
                {
                    return baseValue.Type switch
                    {
                        g__ETES.StatVariantType.Bool => new PairBool(baseValue.Bool, currentValue.Bool).data,
                        g__ETES.StatVariantType.Bool2 => new PairBool2(baseValue.Bool2, currentValue.Bool2).data,
                        g__ETES.StatVariantType.Bool2x2 => new PairBool2x2(baseValue.Bool2x2, currentValue.Bool2x2).data,
                        g__ETES.StatVariantType.Bool3 => new PairBool3(baseValue.Bool3, currentValue.Bool3).data,
                        g__ETES.StatVariantType.Bool4 => new PairBool4(baseValue.Bool4, currentValue.Bool4).data,
                        g__ETES.StatVariantType.Byte => new PairByte(baseValue.Byte, currentValue.Byte).data,
                        g__ETES.StatVariantType.Float => new PairFloat(baseValue.Float, currentValue.Float).data,
                        g__ETES.StatVariantType.Half => new PairHalf(baseValue.Half, currentValue.Half).data,
                        g__ETES.StatVariantType.Half2 => new PairHalf2(baseValue.Half2, currentValue.Half2).data,
                        g__ETES.StatVariantType.Int => new PairInt(baseValue.Int, currentValue.Int).data,
                        g__ETES.StatVariantType.SByte => new PairSByte(baseValue.SByte, currentValue.SByte).data,
                        g__ETES.StatVariantType.Short => new PairShort(baseValue.Short, currentValue.Short).data,
                        g__ETES.StatVariantType.UInt => new PairUInt(baseValue.UInt, currentValue.UInt).data,
                        g__ETES.StatVariantType.UShort => new PairUShort(baseValue.UShort, currentValue.UShort).data,
                        _ => default,
                    };
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetBaseValue(in ValuePair pair, out g__ETES.StatVariant result)
            {
                return pair._isPair ? TryGetPair(pair, out result) : TryGetSingle(pair, out result);

                static bool TryGetSingle(in ValuePair pair, out g__ETES.StatVariant result)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            result = (bool)pair._data._Bool;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            result = (g__UM.bool2)pair._data._Bool2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            result = (g__UM.bool2x2)pair._data._Bool2x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x3:
                        {
                            result = (g__UM.bool2x3)pair._data._Bool2x3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x4:
                        {
                            result = (g__UM.bool2x4)pair._data._Bool2x4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            result = (g__UM.bool3)pair._data._Bool3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3x2:
                        {
                            result = (g__UM.bool3x2)pair._data._Bool3x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            result = (g__UM.bool4)pair._data._Bool4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4x2:
                        {
                            result = (g__UM.bool4x2)pair._data._Bool4x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            result = pair._data._Byte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Double:
                        {
                            result = pair._data._Double;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            result = pair._data._Float;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float2:
                        {
                            result = pair._data._Float2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            result = pair._data._Half;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            result = pair._data._Half2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half3:
                        {
                            result = pair._data._Half3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half4:
                        {
                            result = pair._data._Half4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            result = pair._data._Int;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int2:
                        {
                            result = pair._data._Int2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Long:
                        {
                            result = pair._data._Long;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            result = pair._data._SByte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            result = pair._data._Short;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            result = pair._data._UInt;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt2:
                        {
                            result = pair._data._UInt2;
                            return true;
                        }

                        case g__ETES.StatVariantType.ULong:
                        {
                            result = pair._data._ULong;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            result = pair._data._UShort;
                            return true;
                        }

                        default:
                        {
                            result = default;
                            return false;
                        }
                    }
                }

                static bool TryGetPair(in ValuePair pair, out g__ETES.StatVariant result)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            result = (bool)new PairBool(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            result = (g__UM.bool2)new PairBool2(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            result = (g__UM.bool2x2)new PairBool2x2(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            result = (g__UM.bool3)new PairBool3(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            result = (g__UM.bool4)new PairBool4(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            result = new PairByte(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            result = new PairFloat(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            result = new PairHalf(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            result = new PairHalf2(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            result = new PairInt(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            result = new PairSByte(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            result = new PairShort(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            result = new PairUInt(pair._data).baseValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            result = new PairUShort(pair._data).baseValue;
                            return true;
                        }

                        default:
                        {
                            result = default;
                            return false;
                        }
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool TryGetCurrentValue(in ValuePair pair, out g__ETES.StatVariant result)
            {
                return pair._isPair ? TryGetPair(pair, out result) : TryGetSingle(pair, out result);

                static bool TryGetSingle(in ValuePair pair, out g__ETES.StatVariant result)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            result = (bool)pair._data._Bool;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            result = (g__UM.bool2)pair._data._Bool2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            result = (g__UM.bool2x2)pair._data._Bool2x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x3:
                        {
                            result = (g__UM.bool2x3)pair._data._Bool2x3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x4:
                        {
                            result = (g__UM.bool2x4)pair._data._Bool2x4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            result = (g__UM.bool3)pair._data._Bool3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3x2:
                        {
                            result = (g__UM.bool3x2)pair._data._Bool3x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            result = (g__UM.bool4)pair._data._Bool4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4x2:
                        {
                            result = (g__UM.bool4x2)pair._data._Bool4x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            result = pair._data._Byte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Double:
                        {
                            result = pair._data._Double;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            result = pair._data._Float;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float2:
                        {
                            result = pair._data._Float2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            result = pair._data._Half;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            result = pair._data._Half2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half3:
                        {
                            result = pair._data._Half3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half4:
                        {
                            result = pair._data._Half4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            result = pair._data._Int;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int2:
                        {
                            result = pair._data._Int2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Long:
                        {
                            result = pair._data._Long;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            result = pair._data._SByte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            result = pair._data._Short;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            result = pair._data._UInt;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt2:
                        {
                            result = pair._data._UInt2;
                            return true;
                        }

                        case g__ETES.StatVariantType.ULong:
                        {
                            result = pair._data._ULong;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            result = pair._data._UShort;
                            return true;
                        }

                        default:
                        {
                            result = default;
                            return false;
                        }
                    }
                }

                static bool TryGetPair(in ValuePair pair, out g__ETES.StatVariant result)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            result = (bool)new PairBool(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            result = (g__UM.bool2)new PairBool2(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            result = (g__UM.bool2x2)new PairBool2x2(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            result = (g__UM.bool3)new PairBool3(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            result = (g__UM.bool4)new PairBool4(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            result = new PairByte(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            result = new PairFloat(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            result = new PairHalf(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            result = new PairHalf2(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            result = new PairInt(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            result = new PairSByte(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            result = new PairShort(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            result = new PairUInt(pair._data).currentValue;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            result = new PairUShort(pair._data).currentValue;
                            return true;
                        }

                        default:
                        {
                            result = default;
                            return false;
                        }
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool TrySetBaseValue(ref ValuePair pair, in g__ETES.StatVariant value)
            {
                if (pair._type != value.Type) return false;

                return pair._isPair ? TrySetPair(ref pair, value) : TrySetSingle(ref pair, value);

                static bool TrySetSingle(ref ValuePair pair, in g__ETES.StatVariant value)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            pair._data._Bool = value.Bool;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            pair._data._Bool2 = value.Bool2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            pair._data._Bool2x2 = value.Bool2x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x3:
                        {
                            pair._data._Bool2x3 = value.Bool2x3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x4:
                        {
                            pair._data._Bool2x4 = value.Bool2x4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            pair._data._Bool3 = value.Bool3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3x2:
                        {
                            pair._data._Bool3x2 = value.Bool3x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            pair._data._Bool4 = value.Bool4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4x2:
                        {
                            pair._data._Bool4x2 = value.Bool4x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            pair._data._Byte = value.Byte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Double:
                        {
                            pair._data._Double = value.Double;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            pair._data._Float = value.Float;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float2:
                        {
                            pair._data._Float2 = value.Float2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            pair._data._Half = value.Half;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            pair._data._Half2 = value.Half2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half3:
                        {
                            pair._data._Half3 = value.Half3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half4:
                        {
                            pair._data._Half4 = value.Half4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            pair._data._Int = value.Int;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int2:
                        {
                            pair._data._Int2 = value.Int2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Long:
                        {
                            pair._data._Long = value.Long;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            pair._data._SByte = value.SByte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            pair._data._Short = value.Short;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            pair._data._UInt = value.UInt;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt2:
                        {
                            pair._data._UInt2 = value.UInt2;
                            return true;
                        }

                        case g__ETES.StatVariantType.ULong:
                        {
                            pair._data._ULong = value.ULong;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            pair._data._UShort = value.UShort;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

                static bool TrySetPair(ref ValuePair pair, in g__ETES.StatVariant value)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            pair._data = new PairBool(pair._data) { baseValue = value.Bool }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            pair._data = new PairBool2(pair._data) { baseValue = value.Bool2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            pair._data = new PairBool2x2(pair._data) { baseValue = value.Bool2x2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            pair._data = new PairBool3(pair._data) { baseValue = value.Bool3 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            pair._data = new PairBool4(pair._data) { baseValue = value.Bool4 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            pair._data = new PairByte(pair._data) { baseValue = value.Byte }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            pair._data = new PairFloat(pair._data) { baseValue = value.Float }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            pair._data = new PairHalf(pair._data) { baseValue = value.Half }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            pair._data = new PairHalf2(pair._data) { baseValue = value.Half2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            pair._data = new PairInt(pair._data) { baseValue = value.Int }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            pair._data = new PairSByte(pair._data) { baseValue = value.SByte }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            pair._data = new PairShort(pair._data) { baseValue = value.Short }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            pair._data = new PairUInt(pair._data) { baseValue = value.UInt }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            pair._data = new PairUShort(pair._data) { baseValue = value.UShort }.data;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool TrySetCurrentValue(ref ValuePair pair, in g__ETES.StatVariant value)
            {
                if (pair._type != value.Type) return false;

                return pair._isPair ? TrySetPair(ref pair, value) : TrySetSingle(ref pair, value);

                static bool TrySetSingle(ref ValuePair pair, in g__ETES.StatVariant value)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            pair._data._Bool = value.Bool;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            pair._data._Bool2 = value.Bool2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            pair._data._Bool2x2 = value.Bool2x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x3:
                        {
                            pair._data._Bool2x3 = value.Bool2x3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x4:
                        {
                            pair._data._Bool2x4 = value.Bool2x4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            pair._data._Bool3 = value.Bool3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3x2:
                        {
                            pair._data._Bool3x2 = value.Bool3x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            pair._data._Bool4 = value.Bool4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4x2:
                        {
                            pair._data._Bool4x2 = value.Bool4x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            pair._data._Byte = value.Byte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Double:
                        {
                            pair._data._Double = value.Double;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            pair._data._Float = value.Float;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float2:
                        {
                            pair._data._Float2 = value.Float2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            pair._data._Half = value.Half;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            pair._data._Half2 = value.Half2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half3:
                        {
                            pair._data._Half3 = value.Half3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half4:
                        {
                            pair._data._Half4 = value.Half4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            pair._data._Int = value.Int;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int2:
                        {
                            pair._data._Int2 = value.Int2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Long:
                        {
                            pair._data._Long = value.Long;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            pair._data._SByte = value.SByte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            pair._data._Short = value.Short;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            pair._data._UInt = value.UInt;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt2:
                        {
                            pair._data._UInt2 = value.UInt2;
                            return true;
                        }

                        case g__ETES.StatVariantType.ULong:
                        {
                            pair._data._ULong = value.ULong;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            pair._data._UShort = value.UShort;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

                static bool TrySetPair(ref ValuePair pair, in g__ETES.StatVariant value)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            pair._data = new PairBool(pair._data) { currentValue = value.Bool }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            pair._data = new PairBool2(pair._data) { currentValue = value.Bool2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            pair._data = new PairBool2x2(pair._data) { currentValue = value.Bool2x2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            pair._data = new PairBool3(pair._data) { currentValue = value.Bool3 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            pair._data = new PairBool4(pair._data) { currentValue = value.Bool4 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            pair._data = new PairByte(pair._data) { currentValue = value.Byte }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            pair._data = new PairFloat(pair._data) { currentValue = value.Float }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            pair._data = new PairHalf(pair._data) { currentValue = value.Half }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            pair._data = new PairHalf2(pair._data) { currentValue = value.Half2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            pair._data = new PairInt(pair._data) { currentValue = value.Int }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            pair._data = new PairSByte(pair._data) { currentValue = value.SByte }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            pair._data = new PairShort(pair._data) { currentValue = value.Short }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            pair._data = new PairUInt(pair._data) { currentValue = value.UInt }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            pair._data = new PairUShort(pair._data) { currentValue = value.UShort }.data;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool TrySetValues(ref ValuePair pair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
            {
                ThrowIfMismatchedTypes(baseValue, currentValue);

                if (pair._type != baseValue.Type) return false;

                return pair._isPair ? TrySetPair(ref pair, baseValue, currentValue) : TrySetSingle(ref pair, baseValue, currentValue);

                static bool TrySetSingle(ref ValuePair pair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            pair._data._Bool = currentValue.Bool;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            pair._data._Bool2 = currentValue.Bool2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            pair._data._Bool2x2 = currentValue.Bool2x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x3:
                        {
                            pair._data._Bool2x3 = currentValue.Bool2x3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x4:
                        {
                            pair._data._Bool2x4 = currentValue.Bool2x4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            pair._data._Bool3 = currentValue.Bool3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3x2:
                        {
                            pair._data._Bool3x2 = currentValue.Bool3x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            pair._data._Bool4 = currentValue.Bool4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4x2:
                        {
                            pair._data._Bool4x2 = currentValue.Bool4x2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            pair._data._Byte = currentValue.Byte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Double:
                        {
                            pair._data._Double = currentValue.Double;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            pair._data._Float = currentValue.Float;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float2:
                        {
                            pair._data._Float2 = currentValue.Float2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            pair._data._Half = currentValue.Half;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            pair._data._Half2 = currentValue.Half2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half3:
                        {
                            pair._data._Half3 = currentValue.Half3;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half4:
                        {
                            pair._data._Half4 = currentValue.Half4;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            pair._data._Int = currentValue.Int;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int2:
                        {
                            pair._data._Int2 = currentValue.Int2;
                            return true;
                        }

                        case g__ETES.StatVariantType.Long:
                        {
                            pair._data._Long = currentValue.Long;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            pair._data._SByte = currentValue.SByte;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            pair._data._Short = currentValue.Short;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            pair._data._UInt = currentValue.UInt;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt2:
                        {
                            pair._data._UInt2 = currentValue.UInt2;
                            return true;
                        }

                        case g__ETES.StatVariantType.ULong:
                        {
                            pair._data._ULong = currentValue.ULong;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            pair._data._UShort = currentValue.UShort;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

                static bool TrySetPair(ref ValuePair pair, in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
                {
                    switch (pair._type)
                    {
                        case g__ETES.StatVariantType.Bool:
                        {
                            pair._data = new PairBool(pair._data) { baseValue = baseValue.Bool, currentValue = currentValue.Bool }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2:
                        {
                            pair._data = new PairBool2(pair._data) { baseValue = baseValue.Bool2, currentValue = currentValue.Bool2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool2x2:
                        {
                            pair._data = new PairBool2x2(pair._data) { baseValue = baseValue.Bool2x2, currentValue = currentValue.Bool2x2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool3:
                        {
                            pair._data = new PairBool3(pair._data) { baseValue = baseValue.Bool3, currentValue = currentValue.Bool3 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Bool4:
                        {
                            pair._data = new PairBool4(pair._data) { baseValue = baseValue.Bool4, currentValue = currentValue.Bool4 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Byte:
                        {
                            pair._data = new PairByte(pair._data) { baseValue = baseValue.Byte, currentValue = currentValue.Byte }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Float:
                        {
                            pair._data = new PairFloat(pair._data) { baseValue = baseValue.Float, currentValue = currentValue.Float }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half:
                        {
                            pair._data = new PairHalf(pair._data) { baseValue = baseValue.Half, currentValue = currentValue.Half }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Half2:
                        {
                            pair._data = new PairHalf2(pair._data) { baseValue = baseValue.Half2, currentValue = currentValue.Half2 }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Int:
                        {
                            pair._data = new PairInt(pair._data) { baseValue = baseValue.Int, currentValue = currentValue.Int }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.SByte:
                        {
                            pair._data = new PairSByte(pair._data) { baseValue = baseValue.SByte, currentValue = currentValue.SByte }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.Short:
                        {
                            pair._data = new PairShort(pair._data) { baseValue = baseValue.Short, currentValue = currentValue.Short }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.UInt:
                        {
                            pair._data = new PairUInt(pair._data) { baseValue = baseValue.UInt, currentValue = currentValue.UInt }.data;
                            return true;
                        }

                        case g__ETES.StatVariantType.UShort:
                        {
                            pair._data = new PairUShort(pair._data) { baseValue = baseValue.UShort, currentValue = currentValue.UShort }.data;
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static int GetHashCode(in ValuePair pair)
            {
                ThrowIfNotCompatible(pair);
                return pair._isPair ? GetPair(pair) : GetSingle(pair);

                static int GetSingle(in ValuePair pair)
                {
                    return pair._type switch
                    {
                        g__ETES.StatVariantType.Bool => g__ET.HashValue.Combine(pair._type, pair._data._Bool),
                        g__ETES.StatVariantType.Bool2 => g__ET.HashValue.Combine(pair._type, pair._data._Bool2),
                        g__ETES.StatVariantType.Bool2x2 => g__ET.HashValue.Combine(pair._type, pair._data._Bool2x2),
                        g__ETES.StatVariantType.Bool2x3 => g__ET.HashValue.Combine(pair._type, pair._data._Bool2x3),
                        g__ETES.StatVariantType.Bool2x4 => g__ET.HashValue.Combine(pair._type, pair._data._Bool2x4),
                        g__ETES.StatVariantType.Bool3 => g__ET.HashValue.Combine(pair._type, pair._data._Bool3),
                        g__ETES.StatVariantType.Bool3x2 => g__ET.HashValue.Combine(pair._type, pair._data._Bool3x2),
                        g__ETES.StatVariantType.Bool4 => g__ET.HashValue.Combine(pair._type, pair._data._Bool4),
                        g__ETES.StatVariantType.Bool4x2 => g__ET.HashValue.Combine(pair._type, pair._data._Bool4x2),
                        g__ETES.StatVariantType.Byte => g__ET.HashValue.Combine(pair._type, pair._data._Byte),
                        g__ETES.StatVariantType.Double => g__ET.HashValue.Combine(pair._type, pair._data._Double),
                        g__ETES.StatVariantType.Float => g__ET.HashValue.Combine(pair._type, pair._data._Float),
                        g__ETES.StatVariantType.Float2 => g__ET.HashValue.Combine(pair._type, pair._data._Float2),
                        g__ETES.StatVariantType.Half => g__ET.HashValue.Combine(pair._type, pair._data._Half),
                        g__ETES.StatVariantType.Half2 => g__ET.HashValue.Combine(pair._type, pair._data._Half2),
                        g__ETES.StatVariantType.Half3 => g__ET.HashValue.Combine(pair._type, pair._data._Half3),
                        g__ETES.StatVariantType.Half4 => g__ET.HashValue.Combine(pair._type, pair._data._Half4),
                        g__ETES.StatVariantType.Int => g__ET.HashValue.Combine(pair._type, pair._data._Int),
                        g__ETES.StatVariantType.Int2 => g__ET.HashValue.Combine(pair._type, pair._data._Int2),
                        g__ETES.StatVariantType.Long => g__ET.HashValue.Combine(pair._type, pair._data._Long),
                        g__ETES.StatVariantType.SByte => g__ET.HashValue.Combine(pair._type, pair._data._SByte),
                        g__ETES.StatVariantType.Short => g__ET.HashValue.Combine(pair._type, pair._data._Short),
                        g__ETES.StatVariantType.UInt => g__ET.HashValue.Combine(pair._type, pair._data._UInt),
                        g__ETES.StatVariantType.UInt2 => g__ET.HashValue.Combine(pair._type, pair._data._UInt2),
                        g__ETES.StatVariantType.ULong => g__ET.HashValue.Combine(pair._type, pair._data._ULong),
                        g__ETES.StatVariantType.UShort => g__ET.HashValue.Combine(pair._type, pair._data._UShort),
                        _ => 0,
                    };
                }

                static int GetPair(in ValuePair pair)
                {
                    return pair._type switch
                    {
                        g__ETES.StatVariantType.Bool => g__ET.HashValue.Combine(pair._type, new PairBool(pair._data)),
                        g__ETES.StatVariantType.Bool2 => g__ET.HashValue.Combine(pair._type, new PairBool2(pair._data)),
                        g__ETES.StatVariantType.Bool2x2 => g__ET.HashValue.Combine(pair._type, new PairBool2x2(pair._data)),
                        g__ETES.StatVariantType.Bool3 => g__ET.HashValue.Combine(pair._type, new PairBool3(pair._data)),
                        g__ETES.StatVariantType.Bool4 => g__ET.HashValue.Combine(pair._type, new PairBool4(pair._data)),
                        g__ETES.StatVariantType.Byte => g__ET.HashValue.Combine(pair._type, new PairByte(pair._data)),
                        g__ETES.StatVariantType.Float => g__ET.HashValue.Combine(pair._type, new PairFloat(pair._data)),
                        g__ETES.StatVariantType.Half => g__ET.HashValue.Combine(pair._type, new PairHalf(pair._data)),
                        g__ETES.StatVariantType.Half2 => g__ET.HashValue.Combine(pair._type, new PairHalf2(pair._data)),
                        g__ETES.StatVariantType.Int => g__ET.HashValue.Combine(pair._type, new PairInt(pair._data)),
                        g__ETES.StatVariantType.SByte => g__ET.HashValue.Combine(pair._type, new PairSByte(pair._data)),
                        g__ETES.StatVariantType.Short => g__ET.HashValue.Combine(pair._type, new PairShort(pair._data)),
                        g__ETES.StatVariantType.UInt => g__ET.HashValue.Combine(pair._type, new PairUInt(pair._data)),
                        g__ETES.StatVariantType.UShort => g__ET.HashValue.Combine(pair._type, new PairUShort(pair._data)),
                        _ => 0,
                    };
                }
            }

            [g__SRCS.MethodImpl(INLINING)]
            public static bool Equals(in ValuePair pairA, in ValuePair pairB)
            {
                if (pairA._type != pairB._type || pairA._isPair != pairB._isPair) return false;

                return pairA._isPair ? EqualsPair(pairA, pairB) : EqualsSingle(pairA, pairB);

                static bool EqualsSingle(in ValuePair pairA, in ValuePair pairB)
                {
                    return pairA._type switch
                    {
                        g__ETES.StatVariantType.Bool => pairA._data._Bool.Equals(pairB._data._Bool),
                        g__ETES.StatVariantType.Bool2 => pairA._data._Bool2.Equals(pairB._data._Bool2),
                        g__ETES.StatVariantType.Bool2x2 => pairA._data._Bool2x2.Equals(pairB._data._Bool2x2),
                        g__ETES.StatVariantType.Bool2x3 => pairA._data._Bool2x3.Equals(pairB._data._Bool2x3),
                        g__ETES.StatVariantType.Bool2x4 => pairA._data._Bool2x4.Equals(pairB._data._Bool2x4),
                        g__ETES.StatVariantType.Bool3 => pairA._data._Bool3.Equals(pairB._data._Bool3),
                        g__ETES.StatVariantType.Bool3x2 => pairA._data._Bool3x2.Equals(pairB._data._Bool3x2),
                        g__ETES.StatVariantType.Bool4 => pairA._data._Bool4.Equals(pairB._data._Bool4),
                        g__ETES.StatVariantType.Bool4x2 => pairA._data._Bool4x2.Equals(pairB._data._Bool4x2),
                        g__ETES.StatVariantType.Byte => pairA._data._Byte.Equals(pairB._data._Byte),
                        g__ETES.StatVariantType.Double => pairA._data._Double.Equals(pairB._data._Double),
                        g__ETES.StatVariantType.Float => pairA._data._Float.Equals(pairB._data._Float),
                        g__ETES.StatVariantType.Float2 => pairA._data._Float2.Equals(pairB._data._Float2),
                        g__ETES.StatVariantType.Half => pairA._data._Half.Equals(pairB._data._Half),
                        g__ETES.StatVariantType.Half2 => pairA._data._Half2.Equals(pairB._data._Half2),
                        g__ETES.StatVariantType.Half3 => pairA._data._Half3.Equals(pairB._data._Half3),
                        g__ETES.StatVariantType.Half4 => pairA._data._Half4.Equals(pairB._data._Half4),
                        g__ETES.StatVariantType.Int => pairA._data._Int.Equals(pairB._data._Int),
                        g__ETES.StatVariantType.Int2 => pairA._data._Int2.Equals(pairB._data._Int2),
                        g__ETES.StatVariantType.Long => pairA._data._Long.Equals(pairB._data._Long),
                        g__ETES.StatVariantType.SByte => pairA._data._SByte.Equals(pairB._data._SByte),
                        g__ETES.StatVariantType.Short => pairA._data._Short.Equals(pairB._data._Short),
                        g__ETES.StatVariantType.UInt => pairA._data._UInt.Equals(pairB._data._UInt),
                        g__ETES.StatVariantType.UInt2 => pairA._data._UInt2.Equals(pairB._data._UInt2),
                        g__ETES.StatVariantType.ULong => pairA._data._ULong.Equals(pairB._data._ULong),
                        g__ETES.StatVariantType.UShort => pairA._data._UShort.Equals(pairB._data._UShort),
                        _ => false,
                    };
                }

                static bool EqualsPair(in ValuePair pairA, in ValuePair pairB)
                {
                    return pairA._type switch
                    {
                        g__ETES.StatVariantType.Bool => new PairBool(pairA._data).Equals(new PairBool(pairB._data)),
                        g__ETES.StatVariantType.Bool2 => new PairBool2(pairA._data).Equals(new PairBool2(pairB._data)),
                        g__ETES.StatVariantType.Bool2x2 => new PairBool2x2(pairA._data).Equals(new PairBool2x2(pairB._data)),
                        g__ETES.StatVariantType.Bool3 => new PairBool3(pairA._data).Equals(new PairBool3(pairB._data)),
                        g__ETES.StatVariantType.Bool4 => new PairBool4(pairA._data).Equals(new PairBool4(pairB._data)),
                        g__ETES.StatVariantType.Byte => new PairByte(pairA._data).Equals(new PairByte(pairB._data)),
                        g__ETES.StatVariantType.Float => new PairFloat(pairA._data).Equals(new PairFloat(pairB._data)),
                        g__ETES.StatVariantType.Half => new PairHalf(pairA._data).Equals(new PairHalf(pairB._data)),
                        g__ETES.StatVariantType.Half2 => new PairHalf2(pairA._data).Equals(new PairHalf2(pairB._data)),
                        g__ETES.StatVariantType.Int => new PairInt(pairA._data).Equals(new PairInt(pairB._data)),
                        g__ETES.StatVariantType.SByte => new PairSByte(pairA._data).Equals(new PairSByte(pairB._data)),
                        g__ETES.StatVariantType.Short => new PairShort(pairA._data).Equals(new PairShort(pairB._data)),
                        g__ETES.StatVariantType.UInt => new PairUInt(pairA._data).Equals(new PairUInt(pairB._data)),
                        g__ETES.StatVariantType.UShort => new PairUShort(pairA._data).Equals(new PairUShort(pairB._data)),
                        _ => false,
                    };
                }

            }

        }

    }

#region    IMPL - PAIR BOOL
#endregion ================

    partial class StatsApi // Impl: PairBool
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairBool : g__S.IEquatable<PairBool>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__ET.ByteBool baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(1)]
            public g__ET.ByteBool currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool(g__ET.ByteBool baseValue, g__ET.ByteBool currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairBool other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairBool other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR BOOL2
#endregion =================

    partial class StatsApi // Impl: PairBool2
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairBool2 : g__S.IEquatable<PairBool2>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__ET.ByteBool2 baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(2)]
            public g__ET.ByteBool2 currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool2(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool2(g__ET.ByteBool2 baseValue, g__ET.ByteBool2 currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairBool2 other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairBool2 other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR BOOL2X2
#endregion ===================

    partial class StatsApi // Impl: PairBool2x2
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairBool2x2 : g__S.IEquatable<PairBool2x2>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__ET.ByteBool2x2 baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(4)]
            public g__ET.ByteBool2x2 currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool2x2(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool2x2(g__ET.ByteBool2x2 baseValue, g__ET.ByteBool2x2 currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairBool2x2 other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairBool2x2 other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR BOOL3
#endregion =================

    partial class StatsApi // Impl: PairBool3
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairBool3 : g__S.IEquatable<PairBool3>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__ET.ByteBool3 baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(3)]
            public g__ET.ByteBool3 currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool3(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool3(g__ET.ByteBool3 baseValue, g__ET.ByteBool3 currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairBool3 other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairBool3 other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR BOOL4
#endregion =================

    partial class StatsApi // Impl: PairBool4
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairBool4 : g__S.IEquatable<PairBool4>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__ET.ByteBool4 baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(4)]
            public g__ET.ByteBool4 currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool4(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairBool4(g__ET.ByteBool4 baseValue, g__ET.ByteBool4 currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairBool4 other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairBool4 other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR BYTE
#endregion ================

    partial class StatsApi // Impl: PairByte
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairByte : g__S.IEquatable<PairByte>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public byte baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(1)]
            public byte currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairByte(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairByte(byte baseValue, byte currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairByte other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairByte other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR FLOAT
#endregion =================

    partial class StatsApi // Impl: PairFloat
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairFloat : g__S.IEquatable<PairFloat>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public float baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(4)]
            public float currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairFloat(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairFloat(float baseValue, float currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairFloat other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairFloat other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR HALF
#endregion ================

    partial class StatsApi // Impl: PairHalf
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairHalf : g__S.IEquatable<PairHalf>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__UM.half baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(2)]
            public g__UM.half currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairHalf(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairHalf(g__UM.half baseValue, g__UM.half currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairHalf other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairHalf other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR HALF2
#endregion =================

    partial class StatsApi // Impl: PairHalf2
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairHalf2 : g__S.IEquatable<PairHalf2>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public g__UM.half2 baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(4)]
            public g__UM.half2 currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairHalf2(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairHalf2(g__UM.half2 baseValue, g__UM.half2 currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairHalf2 other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairHalf2 other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR INT
#endregion ===============

    partial class StatsApi // Impl: PairInt
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairInt : g__S.IEquatable<PairInt>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public int baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(4)]
            public int currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairInt(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairInt(int baseValue, int currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairInt other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairInt other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR SBYTE
#endregion =================

    partial class StatsApi // Impl: PairSByte
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairSByte : g__S.IEquatable<PairSByte>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public sbyte baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(1)]
            public sbyte currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairSByte(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairSByte(sbyte baseValue, sbyte currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairSByte other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairSByte other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR SHORT
#endregion =================

    partial class StatsApi // Impl: PairShort
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairShort : g__S.IEquatable<PairShort>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public short baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(2)]
            public short currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairShort(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairShort(short baseValue, short currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairShort other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairShort other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR UINT
#endregion ================

    partial class StatsApi // Impl: PairUInt
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairUInt : g__S.IEquatable<PairUInt>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public uint baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(4)]
            public uint currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairUInt(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairUInt(uint baseValue, uint currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairUInt other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairUInt other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - PAIR USHORT
#endregion ==================

    partial class StatsApi // Impl: PairUShort
    {
        [g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        struct PairUShort : g__S.IEquatable<PairUShort>
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public StatDataStore data;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(0)]
            public ushort baseValue;

            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [g__SRIS.FieldOffset(2)]
            public ushort currentValue;

            [g__SRCS.MethodImpl(INLINING)]
            public PairUShort(in StatDataStore data) : this()
            {
                this.data = data;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public PairUShort(ushort baseValue, ushort currentValue) : this()
            {
                this.baseValue = baseValue;
                this.currentValue = currentValue;
            }

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override int GetHashCode()
                => g__ET.HashValue.Combine(baseValue, currentValue);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly override bool Equals(object obj)
                => obj is PairUShort other && Equals(other);

            [g__SRCS.MethodImpl(INLINING)]
            public readonly bool Equals(PairUShort other)
                => baseValue.Equals(other.baseValue) && currentValue.Equals(other.currentValue);
        }

    }

#region    IMPL - HELPERS
#endregion ==============

    partial class StatsApi // Impl: Helpers
    {
        private const g__SRCS.MethodImplOptions INLINING = g__SRCS.MethodImplOptions.AggressiveInlining;

        private const string GENERATOR = "EncosyTower.Entities.Stats.Generators.StatSystemGenerator";

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        private static void ThrowIfMismatchedTypes(in g__ETES.StatVariant baseValue, in g__ETES.StatVariant currentValue)
        {
            if (baseValue.Type == currentValue.Type) return;

            throw new g__ETES.StatVariantTypeException($"Base and current values are of different types, respectively '{baseValue.Type.ToStringFast()}' and '{currentValue.Type.ToStringFast()}'.");
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        private static void ThrowIfNotCompatible<TStatData>(TStatData value)
            where TStatData : unmanaged, g__ETES.IStatData
        {
            if (IsCompatible(value)) return;

            throw new g__ETES.StatVariantTypeException($"Stat data 'typeof(TStatData)' contains values of type '{value.ValueType.ToStringFast()}' which is not compatible to the stat system 'typeof(StatsApi)'.");
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        private static void ThrowIfNotCompatible(in ValuePair value)
        {
            if (IsCompatible(value)) return;

            throw new g__ETES.StatVariantTypeException($"Value of type '{value.Type.ToStringFast()}' is not compatible to the stat system 'typeof(StatsApi)'.");
        }

        [g__UE.HideInCallstack, g__SD.StackTraceHidden, g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDVD.STATS_CHECKS)]
        [g__SCDC.GeneratedCode(GENERATOR, "0.1.7-preview.3")][g__SDCA.ExcludeFromCodeCoverage]
        private static void ThrowIfNotCompatible(in g__ETES.StatVariant value, bool isPair)
        {
            if (IsCompatible(value, isPair)) return;

            throw new g__ETES.StatVariantTypeException($"Value of type '{value.Type.ToStringFast()}' is not compatible to the stat system 'typeof(StatsApi)'.");
        }

    }



}

