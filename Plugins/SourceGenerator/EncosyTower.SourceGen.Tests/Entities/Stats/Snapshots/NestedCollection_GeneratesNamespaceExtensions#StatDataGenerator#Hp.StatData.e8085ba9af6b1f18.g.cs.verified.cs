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
    partial class Outer 
    {
        partial struct Stats 
        {



#pragma warning disable

    [g__SCDC.GeneratedCode("EncosyTower.Entities.Stats.Generators.StatDataGenerator", "0.1.8-preview.1")][g__SDCA.ExcludeFromCodeCoverage]
    partial struct Hp : g__ETES.IStatData
    {
        public float baseValue;

        public float currentValue;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Hp(float value) : this()
        {
            this.baseValue = this.currentValue = value;
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Hp(float baseValue, float currentValue) : this()
        {
            this.baseValue = baseValue;
            this.currentValue = currentValue;
        }

        public readonly bool IsValuePair
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        public readonly g__ETES.StatVariantType ValueType
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => g__ETES.StatVariantType.Float;
        }

        public g__ETES.StatVariant BaseValue
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return this.baseValue;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (value.Type == g__ETES.StatVariantType.Float)
#endif
                {
                    this.baseValue = value.Float;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    g__UnityDebug.LogError($"The setter of 'Hp.BaseValue' expects a value of type 'g__ETES.StatVariantType.Float' but receives a value of type '{value.Type}'");
                }
#endif
            }
        }

        public g__ETES.StatVariant CurrentValue
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return this.currentValue;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            set
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (value.Type == g__ETES.StatVariantType.Float)
#endif
                {
                    this.currentValue = value.Float;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    g__UnityDebug.LogError($"The setter of 'Hp.CurrentValue' expects a value of type 'g__ETES.StatVariantType.Float' but receives a value of type '{value.Type}'");
                }
#endif
            }

        }

    }


        }
    }
}

