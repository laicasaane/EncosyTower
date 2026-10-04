using g__ETT = global::EncosyTower.Types;
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
                public g__ETT.TypeId<global::MyGame.Registry<TKey>.Slot<TValue>> TypeId
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).TypeId;
                }

                public bool IsEnabled
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).IsEnabled;
                }

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                    => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).WaitUntilEnabledAsync(token);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue(out global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), out value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public global::MyGame.Registry<TKey>.Slot<TValue> GetValueOrThrow()
                    => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>));

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask<global::MyGame.Registry<TKey>.Slot<TValue>> GetValueAsync(g__ST.CancellationToken token = default)
                    => g__ETT.TypeFlagLinkExtensions.GetValueAsync(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), token);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                    where TObject : class
                    => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), out obj);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TObject GetObjectOrThrow<TObject>()
                    where TObject : class
                    => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>));

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue<TValue_>(out TValue_ value)
                    where TValue_ : struct
                    => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), out value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TValue_ GetValueOrThrow<TValue_>()
                    where TValue_ : struct
                    => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>));
            }

            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
            [g__SDCA.ExcludeFromCodeCoverage]
            private readonly struct TypeFlagReadWrite
            {
                public g__ETT.TypeId<global::MyGame.Registry<TKey>.Slot<TValue>> TypeId
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).TypeId;
                }

                public bool IsEnabled
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).IsEnabled;
                }

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                    => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).WaitUntilEnabledAsync(token);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue(out global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), out value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public global::MyGame.Registry<TKey>.Slot<TValue> GetValueOrThrow()
                    => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>));

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask<global::MyGame.Registry<TKey>.Slot<TValue>> GetValueAsync(g__ST.CancellationToken token = default)
                    => g__ETT.TypeFlagLinkExtensions.GetValueAsync(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), token);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                    where TObject : class
                    => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), out obj);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TObject GetObjectOrThrow<TObject>()
                    where TObject : class
                    => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>));

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryGetValue<TValue_>(out TValue_ value)
                    where TValue_ : struct
                    => g__ETT.TypeFlagLinkExtensions.TryGetValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), out value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public TValue_ GetValueOrThrow<TValue_>()
                    where TValue_ : struct
                    => g__ETT.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>));

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool Enable()
                    => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).Enable();

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool Disable()
                    => default(g__ETT.TypeFlag<global::MyGame.Registry<TKey>.Slot<TValue>>).Disable();

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public void SetValue(global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETT.TypeFlagLinkExtensions.SetValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryRemoveValue(out global::MyGame.Registry<TKey>.Slot<TValue> value)
                    => g__ETT.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, global::MyGame.Registry<TKey>.Slot<TValue>>), out value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryAddObject<TObject>([g__SDCA.NotNull] TObject obj)
                    where TObject : class
                    => g__ETT.TypeFlagLinkExtensions.TryAddObject(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), obj);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryRemoveObject<TObject>(TObject expected)
                    where TObject : class
                    => g__ETT.TypeFlagLinkExtensions.TryRemoveObject(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TObject>), expected);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public void SetValue<TValue_>(TValue_ value)
                    where TValue_ : struct
                    => g__ETT.TypeFlagLinkExtensions.SetValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), value);

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public bool TryRemoveValue<TValue_>(out TValue_ value)
                    where TValue_ : struct
                    => g__ETT.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETT.TypeFlagLink<global::MyGame.Registry<TKey>.Slot<TValue>, TValue_>), out value);
            }
        }
    }
}
