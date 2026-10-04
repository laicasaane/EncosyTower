using g__ETT = global::EncosyTower.Types;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace TestProject
{
    partial class Base
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        public static readonly TypeFlagAPI TypeFlag = default;

        #pragma warning disable CS0414
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        private static readonly TypeFlagReadWrite s_typeFlag = default;
        #pragma warning restore CS0414

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public readonly struct TypeFlagAPI
        {
            public g__ETT.TypeId<global::TestProject.Base> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::TestProject.Base>).TypeId;
            }

            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::TestProject.Base>).IsEnabled;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETT.TypeFlag<global::TestProject.Base>).WaitUntilEnabledAsync(token);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetInstance([g__SDCA.MaybeNullWhen(false)] out global::TestProject.Base instance)
                => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::TestProject.Base, global::TestProject.Base>), out instance);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public global::TestProject.Base GetInstanceOrThrow()
                => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::TestProject.Base, global::TestProject.Base>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask<global::TestProject.Base> GetInstanceAsync(g__ST.CancellationToken token = default)
                => g__ETT.TypeFlagLinkExtensions.GetObjectAsync(default(g__ETT.TypeFlagLink<global::TestProject.Base, global::TestProject.Base>), token);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::TestProject.Base, TObject>), out obj);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TObject GetObjectOrThrow<TObject>()
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::TestProject.Base, TObject>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::TestProject.Base, TValue>), out value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow<TValue>()
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::TestProject.Base, TValue>));
        }

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        [g__SDCA.ExcludeFromCodeCoverage]
        private readonly struct TypeFlagReadWrite
        {
            public g__ETT.TypeId<global::TestProject.Base> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::TestProject.Base>).TypeId;
            }

            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::TestProject.Base>).IsEnabled;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETT.TypeFlag<global::TestProject.Base>).WaitUntilEnabledAsync(token);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetInstance([g__SDCA.MaybeNullWhen(false)] out global::TestProject.Base instance)
                => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::TestProject.Base, global::TestProject.Base>), out instance);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public global::TestProject.Base GetInstanceOrThrow()
                => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::TestProject.Base, global::TestProject.Base>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask<global::TestProject.Base> GetInstanceAsync(g__ST.CancellationToken token = default)
                => g__ETT.TypeFlagLinkExtensions.GetObjectAsync(default(g__ETT.TypeFlagLink<global::TestProject.Base, global::TestProject.Base>), token);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::TestProject.Base, TObject>), out obj);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TObject GetObjectOrThrow<TObject>()
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::TestProject.Base, TObject>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::TestProject.Base, TValue>), out value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow<TValue>()
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::TestProject.Base, TValue>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Enable()
                => default(g__ETT.TypeFlag<global::TestProject.Base>).Enable();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Disable()
                => default(g__ETT.TypeFlag<global::TestProject.Base>).Disable();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRegister([g__SDCA.NotNull] global::TestProject.Base instance)
                => g__ETT.TypeFlagExtensions.TryRegister(default(g__ETT.TypeFlag<global::TestProject.Base>), instance);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryUnregister(global::TestProject.Base instance)
                => g__ETT.TypeFlagExtensions.TryUnregister(default(g__ETT.TypeFlag<global::TestProject.Base>), instance);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryAddObject<TObject>([g__SDCA.NotNull] TObject obj)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryAddObject(default(g__ETT.TypeFlagLink<global::TestProject.Base, TObject>), obj);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveObject<TObject>(TObject expected)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryRemoveObject(default(g__ETT.TypeFlagLink<global::TestProject.Base, TObject>), expected);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void SetValue<TValue>(TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.SetValue(default(g__ETT.TypeFlagLink<global::TestProject.Base, TValue>), value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETT.TypeFlagLink<global::TestProject.Base, TValue>), out value);
        }
    }
}
