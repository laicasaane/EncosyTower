using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Processing.Internals;
using EncosyTower.Vaults;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    public sealed partial class Processor : IDisposable
    {
        private readonly SingletonVault<ProcessHandlerMapBase> _maps = new();

        public void Dispose()
        {
            _maps.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Hub<GlobalScope> Global()
            => Scope(default(GlobalScope));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Hub<TScope> Scope<TScope>()
            where TScope : struct
            => Scope<TScope>(default);

        public Hub<TScope> Scope<TScope>(TScope scope)
        {
            ThrowHelper.ThrowIfUnityObjectScope(scope);

            _maps.TryGetOrAdd(out ProcessHandlerMap<TScope> map);
            return new(scope, map.Scope(this, scope));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityHub<TScope> UnityScope<TScope>([NotNull] TScope scope)
            where TScope : UnityEngine.Object
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);
            return new(this, scope);
        }
    }
}
