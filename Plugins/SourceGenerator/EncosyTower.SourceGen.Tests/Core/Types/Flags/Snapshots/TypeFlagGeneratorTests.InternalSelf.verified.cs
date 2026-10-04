using g__ETT = global::EncosyTower.Types;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace MyGame.Services
{
    partial class SessionService
    {
        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        public static readonly TypeFlagAPI TypeFlag = default;

        [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.Types.Flags.TypeFlagGenerator", "0.1.8-preview.2")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public readonly struct TypeFlagAPI
        {
            /// <inheritdoc cref="g__ETT.TypeFlag{T}.TypeId"/>
            public g__ETT.TypeId<global::MyGame.Services.SessionService> TypeId
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>).TypeId;
            }

            /// <inheritdoc cref="g__ETT.TypeFlag{T}.IsEnabled"/>
            public bool IsEnabled
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>).IsEnabled;
            }

            /// <inheritdoc cref="g__ETT.TypeFlag{T}.WaitUntilEnabledAsync"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                => default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>).WaitUntilEnabledAsync(token);

            /// <inheritdoc cref="g__ETT.TypeFlagExtensions.TryGetInstance{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryGetInstance([g__SDCA.MaybeNullWhen(false)] out global::MyGame.Services.SessionService instance)
                => g__ETT.TypeFlagLinkExtensions.TryGetObject(default(g__ETT.TypeFlagLink<global::MyGame.Services.SessionService, global::MyGame.Services.SessionService>), out instance);

            /// <inheritdoc cref="g__ETT.TypeFlagExtensions.GetInstanceOrThrow{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public global::MyGame.Services.SessionService GetInstanceOrThrow()
                => g__ETT.TypeFlagLinkExtensions.GetObjectOrThrow(default(g__ETT.TypeFlagLink<global::MyGame.Services.SessionService, global::MyGame.Services.SessionService>));

            /// <inheritdoc cref="g__ETT.TypeFlag{T}.Enable"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal bool Enable()
                => default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>).Enable();

            /// <inheritdoc cref="g__ETT.TypeFlag{T}.Disable"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal bool Disable()
                => default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>).Disable();

            /// <inheritdoc cref="g__ETT.TypeFlagExtensions.TryRegister{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal bool TryRegister([g__SDCA.NotNull] global::MyGame.Services.SessionService instance)
                => g__ETT.TypeFlagExtensions.TryRegister(default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>), instance);

            /// <inheritdoc cref="g__ETT.TypeFlagExtensions.TryUnregister{T}"/>
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal bool TryUnregister(global::MyGame.Services.SessionService instance)
                => g__ETT.TypeFlagExtensions.TryUnregister(default(g__ETT.TypeFlag<global::MyGame.Services.SessionService>), instance);
        }
    }
}
