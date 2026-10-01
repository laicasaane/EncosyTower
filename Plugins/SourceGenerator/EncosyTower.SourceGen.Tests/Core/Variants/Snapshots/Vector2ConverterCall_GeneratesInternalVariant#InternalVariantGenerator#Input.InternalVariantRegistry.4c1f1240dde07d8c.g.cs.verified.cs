#pragma warning disable 0219

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ETT = global::EncosyTower.Types;
using g__ETV = global::EncosyTower.Variants;
using g__ETVC = global::EncosyTower.Variants.Converters;
using g__ETVSG = global::EncosyTower.Variants.SourceGen;
using g__UE = global::UnityEngine;
using g__UES = global::UnityEngine.Scripting;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace



#pragma warning disable

namespace EncosyTower.Variants.__InternalVariants__.I_EncosyTower_x002ESourceGen_x002ETests_x002EInput
{
    /// <summary>
    /// Contains auto-generated variants for types detected from usage of either
    /// <c>Variant&lt;T&gt;.GetConverter()</c> or <c>CachedVariantConverter&lt;T&gt;.Default</c>.
    /// <br/>
    /// Automatically register these variants to <see cref="g__ETVC.VariantConverter"/>
    /// on Unity3D platform.
    /// <br/>
    /// These variants are not intended to be used directly by user-code
    /// thus they are declared <c>private</c> inside this class.
    /// </summary>
    [g__ETVSG.GeneratedInternalVariants]
    [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Variants.InternalVariantGenerator", "0.1.7-preview.3")]
    [g__SDCA.ExcludeFromCodeCoverage]
    [g__UES.Preserve]
    public static partial class InternalVariants
    {
        [g__UES.Preserve]
        static InternalVariants()
        {
            Init();
        }

        /// <summary>
        /// Register all variants inside this class
        /// to <see cref="g__ETVC.VariantConverter"/>.
        /// </summary>
        [g__UES.Preserve]
        public static void Register() => Init();

        [g__UE.RuntimeInitializeOnLoadMethod(g__UE.RuntimeInitializeLoadType.BeforeSceneLoad)]
        [g__UES.Preserve]
        private static void Init()
        {
            if (g__ETVC.VariantConverter.CanStore<global::UnityEngine.Vector2>())
            {
                Register<global::UnityEngine.Vector2>(Variant__I_UnityEngine_x002EVector2.Converter.Default
#if UNITY_EDITOR && ENCOSY_LOG_VARIANTS_REGISTRIES
                    , "UnityEngine.Vector2"
#endif
                );
            }


        }

#if !UNITY_EDITOR || !ENCOSY_LOG_VARIANTS_REGISTRIES
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
#endif
        [g__UES.Preserve]
        private static void Register<T>(
              g__ETVC.IVariantConverter<T> converter
#if UNITY_EDITOR && ENCOSY_LOG_VARIANTS_REGISTRIES
            , string typeName
#endif
        )
        {
#if UNITY_EDITOR && ENCOSY_LOG_VARIANTS_REGISTRIES
            var result =
#endif
            g__ETVC.VariantConverter.TryRegister<T>(converter);

#if UNITY_EDITOR && ENCOSY_LOG_VARIANTS_REGISTRIES
            if (result)
            {
                g__UE.Debug.Log("Register variant for {typeName}");
            }
            else
            {
                g__UE.Debug.LogError("Cannot register variant for {typeName}");
            }
#endif
        }

    }
}


