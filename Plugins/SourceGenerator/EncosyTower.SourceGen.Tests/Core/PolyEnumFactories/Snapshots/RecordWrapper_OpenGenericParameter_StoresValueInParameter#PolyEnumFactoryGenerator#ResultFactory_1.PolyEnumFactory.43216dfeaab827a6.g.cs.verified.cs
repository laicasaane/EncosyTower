
#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETCol = global::EncosyTower.Collections;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace

namespace TestProject
{

#pragma warning disable

#region    FACTORY API
#endregion ===========

    public readonly partial record struct ResultFactory<T> // Factory API
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static ResultFactory<T> Ok(T value)
        {
            return new ResultFactory<T>(new global::TestProject.Result<T>.Ok(value));
        }

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static ResultFactory<T> Undefined()
        {
            return new ResultFactory<T>(default(global::TestProject.Result<T>));
        }

    }

#region    TYPE API
#endregion ========

    public readonly partial record struct ResultFactory<T> // Type API
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        public bool Is(global::TestProject.ResultFactory.Type type)
        {
            return this.Value.GetEnumCase() == (global::TestProject.Result.EnumCase)((byte)type);
        }

    }
}
