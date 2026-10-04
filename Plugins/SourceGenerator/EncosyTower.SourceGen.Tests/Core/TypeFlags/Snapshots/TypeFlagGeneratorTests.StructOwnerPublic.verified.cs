using g__ETT = global::EncosyTower.Types;
using g__ETTF = global::EncosyTower.TypeFlags;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace MyGame.Graphics
{
    partial struct GraphicsPreset
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
        public static readonly TypeFlagAPI TypeFlag = default;

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public readonly struct TypeFlagAPI
        {
            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.TypeId"/>
            public g__ETT.TypeId<global::MyGame.Graphics.GraphicsPreset> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETTF.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).TypeId;
            }

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.IsEnabled"/>
            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETTF.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).IsEnabled;
            }

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.WaitUntilEnabledAsync"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETTF.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).WaitUntilEnabledAsync(token);

            /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.TryGetValue{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(out global::MyGame.Graphics.GraphicsPreset value)
                => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), out value);

            /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.GetValueOrThrow{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public global::MyGame.Graphics.GraphicsPreset GetValueOrThrow()
                => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>));

            /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.GetValueAsync{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask<global::MyGame.Graphics.GraphicsPreset> GetValueAsync(g__ST.CancellationToken token = default)
                => g__ETTF.TypeFlagLinkExtensions.GetValueAsync(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), token);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryGetObject(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>), out obj);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TObject GetObjectOrThrow<TObject>()
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>));

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>), out value);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow<TValue>()
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>));

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Enable"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Enable()
                => default(g__ETTF.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).Enable();

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Disable"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Disable()
                => default(g__ETTF.TypeFlag<global::MyGame.Graphics.GraphicsPreset>).Disable();

            /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.SetValue{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void SetValue(global::MyGame.Graphics.GraphicsPreset value)
                => g__ETTF.TypeFlagLinkExtensions.SetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), value);

            /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.TryRemoveValue{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveValue(out global::MyGame.Graphics.GraphicsPreset value)
                => g__ETTF.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, global::MyGame.Graphics.GraphicsPreset>), out value);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryAddObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryAddObject<TObject>([g__SDCA.NotNull] TObject obj)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryAddObject(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>), obj);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryRemoveObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveObject<TObject>(TObject expected)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryRemoveObject(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TObject>), expected);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.SetValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void SetValue<TValue>(TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.SetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>), value);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryRemoveValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETTF.TypeFlagLink<global::MyGame.Graphics.GraphicsPreset, TValue>), out value);
        }
    }
}
