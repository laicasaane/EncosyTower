#pragma warning disable 0219

using EncosyTower.PolyEnumStructs;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETCol = global::EncosyTower.Collections;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

#region    TYPE
#endregion ====

    public readonly partial record struct ResultFactory // Type
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        public enum Type : byte
        {
            Undefined = global::TestProject.Result.EnumCase.Undefined,
            Ok = global::TestProject.Result.EnumCase.Ok,
        }

    }

#region    FACTORY API
#endregion ===========

    public readonly partial record struct ResultFactory // Factory API
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static ResultFactory Ok(int value)
        {
            return new ResultFactory(new global::TestProject.Result<int>.Ok(value));
        }

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static ResultFactory Undefined()
        {
            return new ResultFactory(default(global::TestProject.Result<int>));
        }

    }

#region    TYPE API
#endregion ========

    public readonly partial record struct ResultFactory // Type API
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        public bool Is(Type type)
        {
            return this.Value.GetEnumCase() == (global::TestProject.Result.EnumCase)((byte)type);
        }

    }


}

