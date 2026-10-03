#pragma warning disable 0219

using System;
using EncosyTower.Entities.Stats;
using Unity.Entities;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ETES = global::EncosyTower.Entities.Stats;
using g__ETL = global::EncosyTower.Logging;
using g__UM = global::Unity.Mathematics;

using g__UnityDebug = global::UnityEngine.Debug;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{
    partial struct Stats 
    {



#pragma warning disable

    [g__SCDC.GeneratedCode("EncosyTower.Entities.Stats.Generators.StatDataGenerator", "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Level : g__ETES.IStatData
    {
        public global::TestProject.Rank value;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Level(global::TestProject.Rank value) : this()
        {
            this.value = value;
        }

        public readonly bool IsValuePair
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        public readonly g__ETES.StatVariantType ValueType
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => g__ETES.StatVariantType.Byte;
        }

        public g__ETES.StatVariant BaseValue
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (byte)this.value;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (value.Type == g__ETES.StatVariantType.Byte)
#endif
                {
                    this.value = (global::TestProject.Rank)value.Byte;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    g__UnityDebug.LogError($"The setter of 'Level.BaseValue' expects a value of type 'g__ETES.StatVariantType.Byte' but receives a value of type '{value.Type}'");
                }
#endif
            }
        }

        public g__ETES.StatVariant CurrentValue
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return (byte)this.value;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (value.Type == g__ETES.StatVariantType.Byte)
#endif
                {
                    this.value = (global::TestProject.Rank)value.Byte;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    g__UnityDebug.LogError($"The setter of 'Level.CurrentValue' expects a value of type 'g__ETES.StatVariantType.Byte' but receives a value of type '{value.Type}'");
                }
#endif
            }

        }

    }


    }
}

