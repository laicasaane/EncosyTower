using g__ETT = global::EncosyTower.Types;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace MyGame.Graphics
{
    partial struct GraphicsPreset
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        public static readonly TypeFlagAPI TypeFlag = default;

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public readonly struct TypeFlagAPI
        {
            public g__ETT.TypeId<global::MyGame.Graphics.GraphicsPreset> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).TypeId;
            }

            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).IsEnabled;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETT.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).WaitUntilEnabledAsync(token);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(out global::MyGame.Graphics.GraphicsPreset value)
                => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), out value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public global::MyGame.Graphics.GraphicsPreset GetValueOrThrow()
                => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask<global::MyGame.Graphics.GraphicsPreset> GetValueAsync(g__ST.CancellationToken token = default)
                => g__ETT.TypeFlagLinkExtensions.GetValueAsync(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), token);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>), out obj);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TObject GetObjectOrThrow<TObject>()
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>), out value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow<TValue>()
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Enable()
                => default(g__ETT.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).Enable();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Disable()
                => default(g__ETT.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).Disable();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void SetValue(global::MyGame.Graphics.GraphicsPreset value)
                => g__ETT.TypeFlagLinkExtensions.SetValue(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveValue(out global::MyGame.Graphics.GraphicsPreset value)
                => g__ETT.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), out value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryAddObject<TObject>([g__SDCA.NotNull] TObject obj)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryAddObject(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>), obj);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveObject<TObject>(TObject expected)
                where TObject : class
                => g__ETT.TypeFlagLinkExtensions.TryRemoveObject(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>), expected);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void SetValue<TValue>(TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.SetValue(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>), value);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETT.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETT.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>), out value);
        }
    }
}
