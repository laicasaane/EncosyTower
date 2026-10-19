using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Processing.Internals;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    public readonly struct ProcessRegistry : IDisposable, IIsCreated
    {
        private readonly WeakReference<ProcessHandlerMap> _map;
        private readonly WeakReference<IProcessHandler> _handler;

        internal ProcessRegistry([NotNull] ProcessHandlerMap map, [NotNull] IProcessHandler handler)
        {
            DebuggingThrowHelper.ThrowIfNull(map);
            DebuggingThrowHelper.ThrowIfNull(handler);

            _map = new WeakReference<ProcessHandlerMap>(map);
            _handler = new WeakReference<IProcessHandler>(handler);
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _map != null && _handler != null;
        }

        public void Unregister()
        {
            if (_map != null
                && _handler != null
                && _map.TryGetTarget(out var map)
                && _handler.TryGetTarget(out var handler)
            )
            {
                map.Unregister(handler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
            => Unregister();
    }
}
