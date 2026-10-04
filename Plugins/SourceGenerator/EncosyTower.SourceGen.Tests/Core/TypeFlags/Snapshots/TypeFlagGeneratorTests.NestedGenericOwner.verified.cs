using g__ETT = global::EncosyTower.Types;
using g__ETTF = global::EncosyTower.TypeFlags;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace MyGame
{
    partial class Registry<TKey>
    {
        partial struct Slot<TValue>
        {
            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
            public static readonly TypeFlagAPI TypeFlag = default;

            #pragma warning disable CS0414
            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
            private static readonly TypeFlagReadWrite s_typeFlag = default;
            #pragma warning restore CS0414

            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
            [g__SDCA.ExcludeFromCodeCoverage]
            public readonly struct TypeFlagAPI
            {
                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.TypeId"/>
                public g__ETT.TypeId<global::MyGame.Registry<TKey>.Slot<TValue>> TypeId
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).TypeId;
                }

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.IsEnabled"/>
                public bool IsEnabled
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).IsEnabled;
                }

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.WaitUntilEnabledAsync"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                    => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).WaitUntilEnabledAsync(token);

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.TryGetValue{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue(out global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), out value);

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.GetValueOrThrow{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public global::MyGame.Registry<TKey>.Slot<TValue> GetValueOrThrow()
                    => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>));

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.GetValueAsync{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask<global::MyGame.Registry<TKey>.Slot<TValue>> GetValueAsync(g__ST.CancellationToken token = default)
                    => g__ETTF.TypeFlagLinkExtensions.GetValueAsync(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), token);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetObject{TOwner, TObject}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                    where TObject : class
                    => g__ETTF.TypeFlagLinkExtensions.TryGetObject(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), out obj);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow{TOwner, TObject}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TObject GetObjectOrThrow<TObject>()
                    where TObject : class
                    => g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>));

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetValue{TOwner, TValue}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue<TValue_>(out TValue_ value)
                    where TValue_ : struct
                    => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), out value);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow{TOwner, TValue}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TValue_ GetValueOrThrow<TValue_>()
                    where TValue_ : struct
                    => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>));
            }

            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
            [g__SDCA.ExcludeFromCodeCoverage]
            private readonly struct TypeFlagReadWrite
            {
                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.TypeId"/>
                public g__ETT.TypeId<global::MyGame.Registry<TKey>.Slot<TValue>> TypeId
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).TypeId;
                }

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.IsEnabled"/>
                public bool IsEnabled
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).IsEnabled;
                }

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.WaitUntilEnabledAsync"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                    => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).WaitUntilEnabledAsync(token);

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.TryGetValue{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue(out global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), out value);

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.GetValueOrThrow{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public global::MyGame.Registry<TKey>.Slot<TValue> GetValueOrThrow()
                    => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>));

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.GetValueAsync{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask<global::MyGame.Registry<TKey>.Slot<TValue>> GetValueAsync(g__ST.CancellationToken token = default)
                    => g__ETTF.TypeFlagLinkExtensions.GetValueAsync(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), token);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetObject{TOwner, TObject}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                    where TObject : class
                    => g__ETTF.TypeFlagLinkExtensions.TryGetObject(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), out obj);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow{TOwner, TObject}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TObject GetObjectOrThrow<TObject>()
                    where TObject : class
                    => g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>));

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetValue{TOwner, TValue}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue<TValue_>(out TValue_ value)
                    where TValue_ : struct
                    => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), out value);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow{TOwner, TValue}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TValue_ GetValueOrThrow<TValue_>()
                    where TValue_ : struct
                    => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>));

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Enable"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool Enable()
                    => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).Enable();

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Disable"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool Disable()
                    => default(g__ETTF.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).Disable();

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.SetValue{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public void SetValue(global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETTF.TypeFlagLinkExtensions.SetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), value);

                /// <inheritdoc cref="g__ETTF.TypeFlagExtensions.TryRemoveValue{T}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryRemoveValue(out global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETTF.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), out value);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryAddObject{TOwner, TObject}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryAddObject<TObject>([g__SDCA.NotNull] TObject obj)
                    where TObject : class
                    => g__ETTF.TypeFlagLinkExtensions.TryAddObject(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), obj);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryRemoveObject{TOwner, TObject}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryRemoveObject<TObject>(TObject expected)
                    where TObject : class
                    => g__ETTF.TypeFlagLinkExtensions.TryRemoveObject(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), expected);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.SetValue{TOwner, TValue}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public void SetValue<TValue_>(TValue_ value)
                    where TValue_ : struct
                    => g__ETTF.TypeFlagLinkExtensions.SetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), value);

                /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryRemoveValue{TOwner, TValue}"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryRemoveValue<TValue_>(out TValue_ value)
                    where TValue_ : struct
                    => g__ETTF.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETTF.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), out value);
            }
        }
    }
}
