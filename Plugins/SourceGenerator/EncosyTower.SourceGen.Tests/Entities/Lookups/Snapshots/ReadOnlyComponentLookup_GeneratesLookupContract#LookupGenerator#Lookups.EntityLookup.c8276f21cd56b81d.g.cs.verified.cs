#pragma warning disable 0219

using EncosyTower.Entities;
using Unity.Entities;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ET = global::EncosyTower.Common;
using g__ETEL = global::EncosyTower.Entities.Lookups;
using g__UC = global::Unity.Collections;
using g__UE = global::Unity.Entities;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Entities.Generators.Entities.Lookups.LookupGenerator", "0.1.8-preview.1")]
    partial struct Lookups : g__ETEL.ILookups
        , g__ETEL.IComponentLookupRO<global::TestProject.Component>
    {
         [g__UC.ReadOnly] internal g__UE.ComponentLookup<global::TestProject.Component> _lookup_I_TestProject_x002EComponent;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Lookups(ref g__UE.SystemState state)
        {
            _lookup_I_TestProject_x002EComponent = state.GetComponentLookup<global::TestProject.Component>(true);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public Lookups(g__UE.SystemBase system)
        {
            _lookup_I_TestProject_x002EComponent = system.GetComponentLookup<global::TestProject.Component>(true);
        }

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public static implicit operator g__UE.ComponentLookup<global::TestProject.Component>(in Lookups value) => value._lookup_I_TestProject_x002EComponent;

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public g__UE.ComponentLookup<global::TestProject.Component> Get(g__ET.T<global::TestProject.Component> _) => _lookup_I_TestProject_x002EComponent;

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void Update(ref g__UE.SystemState state)
        {
            _lookup_I_TestProject_x002EComponent.Update(ref state);
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void Update(g__UE.SystemBase system)
        {
            _lookup_I_TestProject_x002EComponent.Update(system);
        }

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool EntityExists(g__UE.Entity entity) => _lookup_I_TestProject_x002EComponent.EntityExists(entity);

        #region    Component

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void HasComponent(g__UE.Entity entity, out g__ET.Bool<global::TestProject.Component> result) => result = (g__ET.Bool<global::TestProject.Component>)_lookup_I_TestProject_x002EComponent.HasComponent(entity);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void HasComponent(g__UE.Entity entity, out g__ET.Bool<global::TestProject.Component> result, out bool entityExists) => result = (g__ET.Bool<global::TestProject.Component>)_lookup_I_TestProject_x002EComponent.HasComponent(entity, out entityExists);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void HasComponent(g__UE.SystemHandle systemHandle, out g__ET.Bool<global::TestProject.Component> result) => result = (g__ET.Bool<global::TestProject.Component>)_lookup_I_TestProject_x002EComponent.HasComponent(systemHandle);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void DidChange(g__UE.Entity entity, uint version, out g__ET.Bool<global::TestProject.Component> result) => result = (g__ET.Bool<global::TestProject.Component>)_lookup_I_TestProject_x002EComponent.DidChange(entity, version);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponent(g__UE.Entity entity, out global::TestProject.Component componentData, out bool entityExists) => _lookup_I_TestProject_x002EComponent.TryGetComponent(entity, out componentData, out entityExists);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponent(g__UE.Entity entity, out global::TestProject.Component componentData) => _lookup_I_TestProject_x002EComponent.TryGetComponent(entity, out componentData);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::TestProject.Component GetComponentData(g__UE.Entity entity, global::TestProject.Component _) => _lookup_I_TestProject_x002EComponent[entity];

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public global::TestProject.Component GetComponentData(g__UE.SystemHandle systemHandle, global::TestProject.Component _) => _lookup_I_TestProject_x002EComponent[systemHandle];

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void GetComponentData(g__UE.Entity entity, out global::TestProject.Component componentData) => componentData = _lookup_I_TestProject_x002EComponent[entity];

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void GetComponentData(g__UE.SystemHandle systemHandle, out global::TestProject.Component componentData) => componentData = _lookup_I_TestProject_x002EComponent[systemHandle];

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void GetRefRO(g__UE.Entity entity, out g__UE.RefRO<global::TestProject.Component> result) => result = _lookup_I_TestProject_x002EComponent.GetRefRO(entity);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void GetRefRO(g__UE.Entity entity, global::TestProject.Component _, out g__UE.RefRO<global::TestProject.Component> result) => result = _lookup_I_TestProject_x002EComponent.GetRefRO(entity);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void GetRefROOptional(g__UE.Entity entity, out g__UE.RefRO<global::TestProject.Component> result) => result = _lookup_I_TestProject_x002EComponent.GetRefROOptional(entity);

        /// <inheritdoc/>
        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
        public void GetRefROOptional(g__UE.Entity entity, global::TestProject.Component _, out g__UE.RefRO<global::TestProject.Component> result) => result = _lookup_I_TestProject_x002EComponent.GetRefROOptional(entity);

        #endregion Component

    }



}

