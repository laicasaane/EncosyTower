using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.UnityExtensions;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Vaults
{
    using UnityObject = UnityEngine.Object;

    public sealed partial class ObjectVault<TId> : IDisposable
        where TId : unmanaged, IEquatable<TId>
    {
        private readonly ConcurrentDictionary<TId, object> _map = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _map.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TId id)
            => _map.ContainsKey(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd<T>(TId id, [NotNull] T obj)
            where T : class
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(obj);

            var map = _map;

            if (map.ContainsKey(id))
            {
                return false;
            }

            map[id] = obj;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove<T>(TId id, out Option<T> obj)
            where T : class
        {
            if (_map.TryRemove(id, out var weakRef))
            {
                obj = TryCast<T>(id, weakRef);
                return true;
            }

            obj = Option.None;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet<T>(TId id, out Option<T> obj)
            where T : class
        {
            if (_map.TryGetValue(id, out var weakRef))
            {
                obj = TryCast<T>(id, weakRef);
                return true;
            }

            obj = Option.None;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(TId id, out Option<object> obj)
        {
            if (_map.TryGetValue(id, out var weakRef))
            {
                obj = TryCast(id, weakRef);
                return true;
            }

            obj = default;
            return false;
        }

        private static Option<T> TryCast<T>(TId id, object obj, UnityObject context = null)
        {
            if (obj == null)
            {
                ThrowHelper.LogErrorRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            if (obj is not UnityObject unityObj)
            {
                if (obj is T objT)
                {
                    return Option.Some(objT);
                }

                goto FAILED;
            }

            if (unityObj && obj is T unityObjT)
            {
                return Option.Some(unityObjT);
            }

            if (unityObj == false)
            {
                ThrowHelper.LogErrorRegisteredObjectIsNull(id, context);
            }

        FAILED:
            ThrowHelper.LogErrorTypeMismatch<T, TId>(id, obj, context);
            return Option.None;
        }

        private static Option<object> TryCast(TId id, object obj, UnityObject context = null)
        {
            if (obj is UnityObject unityObj)
            {
                if (unityObj.IsValid())
                {
                    return Option.Some(unityObj);
                }

                ThrowHelper.LogErrorRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            if (obj == null)
            {
                ThrowHelper.LogErrorRegisteredObjectIsNull(id, context);
                return Option.None;
            }

            return Option.Some(obj);
        }

    }
}
