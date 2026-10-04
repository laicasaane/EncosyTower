using g__ETT = global::EncosyTower.Types;
using g__ETTF = global::EncosyTower.TypeFlags;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace MyGame.Settings
{
    partial class SettingsLoader
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
            public g__ETT.TypeId<global::MyGame.Settings.SettingsLoader> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).TypeId;
            }

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.IsEnabled"/>
            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).IsEnabled;
            }

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.WaitUntilEnabledAsync"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).WaitUntilEnabledAsync(token);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryGetObject(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TObject>), out obj);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TObject GetObjectOrThrow<TObject>()
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TObject>));

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TValue>), out value);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow<TValue>()
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TValue>));
        }

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
        [g__SDCA.ExcludeFromCodeCoverage]
        private readonly struct TypeFlagReadWrite
        {
            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.TypeId"/>
            public g__ETT.TypeId<global::MyGame.Settings.SettingsLoader> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).TypeId;
            }

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.IsEnabled"/>
            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).IsEnabled;
            }

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.WaitUntilEnabledAsync"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).WaitUntilEnabledAsync(token);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetObject<TObject>([g__SDCA.MaybeNullWhen(false)] out TObject obj)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryGetObject(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TObject>), out obj);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TObject GetObjectOrThrow<TObject>()
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TObject>));

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryGetValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.TryGetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TValue>), out value);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public TValue GetValueOrThrow<TValue>()
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.GetValueOrThrow(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TValue>));

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Enable"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Enable()
                => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).Enable();

            /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Disable"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool Disable()
                => default(g__ETTF.TypeFlag<global::MyGame.Settings.SettingsLoader>).Disable();

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryAddObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryAddObject<TObject>([g__SDCA.NotNull] TObject obj)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryAddObject(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TObject>), obj);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryRemoveObject{TOwner, TObject}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveObject<TObject>(TObject expected)
                where TObject : class
                => g__ETTF.TypeFlagLinkExtensions.TryRemoveObject(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TObject>), expected);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.SetValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void SetValue<TValue>(TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.SetValue(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TValue>), value);

            /// <inheritdoc cref="g__ETTF.TypeFlagLinkExtensions.TryRemoveValue{TOwner, TValue}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryRemoveValue<TValue>(out TValue value)
                where TValue : struct
                => g__ETTF.TypeFlagLinkExtensions.TryRemoveValue(default(g__ETTF.TypeFlagLink<global::MyGame.Settings.SettingsLoader, TValue>), out value);
        }
    }
}
