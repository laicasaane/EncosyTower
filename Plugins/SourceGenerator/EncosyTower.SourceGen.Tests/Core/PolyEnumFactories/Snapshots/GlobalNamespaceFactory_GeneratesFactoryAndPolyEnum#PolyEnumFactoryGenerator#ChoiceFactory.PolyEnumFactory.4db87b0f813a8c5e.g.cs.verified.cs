#pragma warning disable 0219

using EncosyTower.PolyEnumStructs;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ETCol = global::EncosyTower.Collections;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace




#pragma warning disable

#region    TYPE
#endregion ====

    public partial class ChoiceFactory // Type
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        public enum Type : byte
        {
            Undefined = global::Choice.EnumCase.Undefined,
            A = global::Choice.EnumCase.A,
        }

    }

#region    FACTORY API
#endregion ===========

    public partial class ChoiceFactory // Factory API
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        private readonly global::Choice _enumStruct_Choice;

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        private ChoiceFactory(global::Choice value)
        {
            this._enumStruct_Choice = value;
        }

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static ChoiceFactory A()
        {
            return new ChoiceFactory(default(global::Choice.A));
        }

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static ChoiceFactory Undefined()
        {
            return new ChoiceFactory(default(global::Choice));
        }

    }

#region    TYPE API
#endregion ========

    public partial class ChoiceFactory // Type API
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Core.Generators.PolyEnumFactories.PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        public bool Is(Type type)
        {
            return this._enumStruct_Choice.GetEnumCase() == (global::Choice.EnumCase)((byte)type);
        }

    }


