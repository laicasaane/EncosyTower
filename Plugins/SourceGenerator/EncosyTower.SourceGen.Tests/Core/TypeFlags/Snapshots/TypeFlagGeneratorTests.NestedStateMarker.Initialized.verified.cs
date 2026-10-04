using g__ETT = global::EncosyTower.Types;
using g__ETTF = global::EncosyTower.TypeFlags;
using g__ETTs = global::EncosyTower.Tasks;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__ST = global::System.Threading;

namespace MyGame.UIs
{
    partial class MenuScreen
    {
        partial struct Initialized
        {
            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
            public static readonly TypeFlagAPI TypeFlag = default;

            [g__SCDC.GeneratedCode("EncosyTower.Core.Generators.TypeFlags.TypeFlagGenerator", "0.1.8-preview.2")]
            [g__SDCA.ExcludeFromCodeCoverage]
            public readonly struct TypeFlagAPI
            {
                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.TypeId"/>
                public g__ETT.TypeId<global::MyGame.UIs.MenuScreen.Initialized> TypeId
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETTF.TypeFlag<global::MyGame.UIs.MenuScreen.Initialized>).TypeId;
                }

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.IsEnabled"/>
                public bool IsEnabled
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => default(g__ETTF.TypeFlag<global::MyGame.UIs.MenuScreen.Initialized>).IsEnabled;
                }

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.WaitUntilEnabledAsync"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public g__ETTs.UnityTask WaitUntilEnabledAsync(g__ST.CancellationToken token = default)
                    => default(g__ETTF.TypeFlag<global::MyGame.UIs.MenuScreen.Initialized>).WaitUntilEnabledAsync(token);

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Enable"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                internal bool Enable()
                    => default(g__ETTF.TypeFlag<global::MyGame.UIs.MenuScreen.Initialized>).Enable();

                /// <inheritdoc cref="g__ETTF.TypeFlag{T}.Disable"/>
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                internal bool Disable()
                    => default(g__ETTF.TypeFlag<global::MyGame.UIs.MenuScreen.Initialized>).Disable();
            }
        }
    }
}
