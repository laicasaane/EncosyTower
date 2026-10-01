using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.Types;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing.Internals
{
    internal abstract class ProcessHandlerMapBase { }

    internal sealed class ProcessHandlerMap<TScope> : ProcessHandlerMapBase
    {
        private readonly Dictionary<TScope, ProcessHandlerMap> _map = new();

        public ProcessHandlerMap Scope(Processor owner, TScope scope)
        {
            if (_map.TryGetValue(scope, out var handler) == false)
            {
                _map[scope] = handler = new ProcessHandlerMap(owner);
            }

            return handler;
        }
    }

    internal sealed class ProcessHandlerMap : ProcessHandlerMapBase
    {
        private readonly Dictionary<TypeId, IProcessHandler> _map = new();

        internal ProcessHandlerMap([NotNull] Processor owner)
        {
            DebuggingThrowHelper.ThrowIfNull(owner);
            Owner = owner;
        }

        /// <summary>
        /// Retains the creating processor and its map graph for scope pivots from live hubs.
        /// </summary>
        internal Processor Owner { get; }

        public ProcessRegistry Register([NotNull] IProcessHandler handler, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(handler);

            if (_map.ContainsKey(handler.Id))
            {
                ThrowHelper.LogWarningHandlerAlreadyRegistered(handler, logger);
                return default;
            }

            var id = handler.Id;

            _map[id] = handler;

            return new(this, handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unregister(TypeId id)
        {
            return _map.Remove(id);
        }

        public bool Unregister(IProcessHandler expected)
        {
            if (expected != null
                && _map.TryGetValue(expected.Id, out var current)
                && ReferenceEquals(current, expected)
            )
            {
                return _map.Remove(expected.Id);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _map.Clear();
        }

        public bool TryGet(TypeId id, out IProcessHandler handler)
        {
            if (_map.TryGetValue(id, out var result) && result is not null)
            {
                handler = result;
                return true;
            }

            handler = default;
            return false;
        }
    }
}
