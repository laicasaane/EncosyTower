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

#region    EXTENSIONS
#endregion ==========

    public static partial class StatsExtensions
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ET.Option<g__StatSystem.ValuePair> TryGetValuePair(this g__ET.Option<global::TestProject.Outer.Stats.Hp> value)
            => value.TryGetValue(out var stat) ? stat.ToValuePair() : g__ET.Option.None;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ET.Option<float> TryGetBaseValue(this g__ET.Option<global::TestProject.Outer.Stats.Hp> value)
            => value.TryGetValue(out var stat) ? stat.baseValue : g__ET.Option.None;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ET.Option<float> TryGetCurrentValue(this g__ET.Option<global::TestProject.Outer.Stats.Hp> value)
            => value.TryGetValue(out var stat) ? stat.currentValue : g__ET.Option.None;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ET.Option<g__StatSystem.ValuePair> TryGetValuePair(this g__ET.Option<global::TestProject.Outer.Stats.Level> value)
            => value.TryGetValue(out var stat) ? stat.ToValuePair() : g__ET.Option.None;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ET.Option<global::TestProject.Outer.Rank> TryGetBaseValue(this g__ET.Option<global::TestProject.Outer.Stats.Level> value)
            => value.TryGetValue(out var stat) ? stat.baseValue : g__ET.Option.None;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static g__ET.Option<global::TestProject.Outer.Rank> TryGetCurrentValue(this g__ET.Option<global::TestProject.Outer.Stats.Level> value)
            => value.TryGetValue(out var stat) ? stat.currentValue : g__ET.Option.None;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static TComponentData ToComponent<TComponentData>(this global::TestProject.Outer.Stats.Baker<TComponentData> baker)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            return g__UCLU.UnsafeUtility.As<global::TestProject.Outer.Stats, TComponentData>(ref baker.statCollection);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void AddComponentToEntity<TComponentData>(this global::TestProject.Outer.Stats.Baker<TComponentData> baker)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<global::TestProject.Outer.Stats, TComponentData>(ref baker.statCollection);
            var statBaker = baker.baker;
            statBaker.IBaker.AddComponent(statBaker.Entity, component);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static TComponentData ToComponent<TComponentData>(this global::TestProject.Outer.Stats.Accessor<TComponentData> accessor)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            return g__UCLU.UnsafeUtility.As<global::TestProject.Outer.Stats, TComponentData>(ref accessor.statCollection);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void SetComponentToEntity<TComponentData>(this global::TestProject.Outer.Stats.Accessor<TComponentData> accessor, g__UECS.EntityManager entityManager)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<global::TestProject.Outer.Stats, TComponentData>(ref accessor.statCollection);
            entityManager.SetComponentData(accessor.entity, component);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void SetComponentToEntity<TComponentData>(this global::TestProject.Outer.Stats.Accessor<TComponentData> accessor, g__UECS.EntityCommandBuffer ecb)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<global::TestProject.Outer.Stats, TComponentData>(ref accessor.statCollection);
            ecb.SetComponent(accessor.entity, component);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static void SetComponentToEntity<TComponentData>(this global::TestProject.Outer.Stats.Accessor<TComponentData> accessor, g__UECS.EntityCommandBuffer.ParallelWriter ecb, int sortKey)
            where TComponentData : unmanaged, g__UECS.IComponentData
        {
            ref var component = ref g__UCLU.UnsafeUtility.As<global::TestProject.Outer.Stats, TComponentData>(ref accessor.statCollection);
            ecb.SetComponent(sortKey, accessor.entity, component);
        }

    }



}

