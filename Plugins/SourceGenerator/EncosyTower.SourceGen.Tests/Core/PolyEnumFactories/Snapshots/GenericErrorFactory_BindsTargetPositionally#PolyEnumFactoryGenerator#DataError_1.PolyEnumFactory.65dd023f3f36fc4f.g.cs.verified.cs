
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

    public readonly partial struct DataError<U> // Factory API
    where U : unmanaged
    {
        [g__SCDC.GeneratedCode("PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        private readonly global::TestProject.Error<U> _enumStruct_Error;

        [g__SCDC.GeneratedCode("PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        private DataError(global::TestProject.Error<U> value) : this()
        {
            this._enumStruct_Error = value;
        }

        [g__SCDC.GeneratedCode("PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static DataError<U> Invalid(U data)
        {
            return new DataError<U>(new global::TestProject.Error<U>.Invalid(data));
        }

        [g__SCDC.GeneratedCode("PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static DataError<U> Undefined()
        {
            return new DataError<U>(default(global::TestProject.Error<U>));
        }

    }

#region    TYPE API
#endregion ========

    public readonly partial struct DataError<U> // Type API
    where U : unmanaged
    {
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("PolyEnumFactoryGenerator", "0.1.8-preview.1")]
        public bool Is(global::TestProject.DataError.Type type)
        {
            return this._enumStruct_Error.GetEnumCase() == (global::TestProject.Error.EnumCase)((byte)type);
        }

    }
}
