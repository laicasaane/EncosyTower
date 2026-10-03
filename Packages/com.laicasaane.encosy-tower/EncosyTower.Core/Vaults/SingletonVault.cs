#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Concurrent;
using EncosyTower.Types;

namespace EncosyTower.Vaults
{
    public class SingletonVault<TBase> : IDisposable
        where TBase : class
    {
        private readonly ConcurrentDictionary<TypeHash, TBase> _singletons = new();

        public bool Contains<T>()
            where T : class, TBase
            => _singletons.ContainsKey(Type<T>.Hash);

        public bool Contains<T>(T instance)
            where T : class, TBase
            => _singletons.TryGetValue(Type<T>.Hash, out var obj) && ReferenceEquals(obj, instance);

        public bool TryAdd<T>()
            where T : class, TBase, new()
        {
            var hash = Type<T>.Hash;

            if (_singletons.ContainsKey(hash))
            {
                ThrowHelper.LogErrorInstanceAlreadyExists<T>();
                return false;
            }

            var instance = new T();

            if (_singletons.TryAdd(hash, instance))
            {
                return true;
            }

            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ThrowHelper.LogErrorInstanceAlreadyExists<T>();
            return false;
        }

        public bool TryAdd<T>(T instance)
            where T : class, TBase
        {
            if (instance == null)
            {
#if __ENCOSY_VALIDATION__
                throw ThrowHelper.CreateArgumentNullExceptionInstance();
#else
                return false;
#endif
            }

            if (_singletons.ContainsKey(Type<T>.Hash))
            {
                ThrowHelper.LogErrorInstanceAlreadyExists<T>();
                return false;
            }

            return _singletons.TryAdd(Type<T>.Hash, instance);
        }

        public bool TryGetOrAdd<T>(out T instance)
            where T : class, TBase, new()
        {
            var hash = Type<T>.Hash;

            if (_singletons.TryGetValue(hash, out var obj) == false)
            {
                var created = new T();
                obj = _singletons.GetOrAdd(hash, created);

                if (ReferenceEquals(obj, created) == false && created is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            if (obj is T inst)
            {
                instance = inst;
                return true;
            }

            ThrowHelper.ThrowCannotCastEvenRegistered<T>(obj);
            instance = default;
            return false;
        }

        public bool TryGet<T>(out T instance)
            where T : class, TBase
        {
            if (_singletons.TryGetValue(Type<T>.Hash, out var obj))
            {
                if (obj is T inst)
                {
                    instance = inst;
                    return true;
                }
                else
                {
                    ThrowHelper.ThrowCannotCast<T>(obj);
                }
            }

            instance = default;
            return false;
        }

        public void Dispose()
        {
            var singletons = _singletons;

            foreach (var (hash, _) in singletons)
            {
                if (singletons.TryRemove(hash, out var obj) && obj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

    }
}
