#pragma warning disable 0219

using EncosyTower.Entities;
using Unity.Entities;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ET = global::EncosyTower.Common;
using g__ETETH = global::EncosyTower.Entities.TypeHandles;
using g__UC = global::Unity.Collections;
using g__UE = global::Unity.Entities;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Entities.Generators.Entities.TypeHandles.TypeHandleGenerator", "0.1.8-preview.1")]
    partial struct Handles : g__ETETH.ITypeHandles
    {
         [g__UC.ReadOnly] internal g__UE.ComponentTypeHandle<global::TestProject.Component> _handle_I_TestProject_x002EComponent;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Handles(ref g__UE.SystemState state)
        {
            _handle_I_TestProject_x002EComponent = state.GetComponentTypeHandle<global::TestProject.Component>(true);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Handles(g__UE.SystemBase system)
        {
            _handle_I_TestProject_x002EComponent = system.GetComponentTypeHandle<global::TestProject.Component>(true);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator g__UE.ComponentTypeHandle<global::TestProject.Component>(in Handles value) => value._handle_I_TestProject_x002EComponent;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public g__UE.ComponentTypeHandle<global::TestProject.Component> Get(g__ET.T<global::TestProject.Component> _) => _handle_I_TestProject_x002EComponent;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void Update(ref g__UE.SystemState state)
        {
            _handle_I_TestProject_x002EComponent.Update(ref state);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void Update(g__UE.SystemBase system)
        {
            _handle_I_TestProject_x002EComponent.Update(system);
        }

    }



}

