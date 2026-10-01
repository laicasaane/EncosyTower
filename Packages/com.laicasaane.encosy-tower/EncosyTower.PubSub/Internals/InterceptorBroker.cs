using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;

namespace EncosyTower.PubSub.Internals
{
    internal interface IInterceptorBroker : IDisposable
    {
        public readonly struct AnyMessage : IMessage { }
    }

    internal sealed class InterceptorBroker<TMessage> : IInterceptorBroker
    {
        private readonly List<object> _interceptors = new();

        public ListFast<object>.ReadOnly Interceptors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _interceptors.AsListFast().AsReadOnly();
        }

        public void Add(object interceptor)
        {
            lock (_interceptors)
            {
                _interceptors.Add(interceptor);
            }
        }

        public void Dispose()
        {
            lock (_interceptors)
            {
                _interceptors.Clear();
            }
        }

        public void Remove(object interceptor)
        {
            var interceptors = _interceptors.AsListFast().AsSpan();

            for (var i = interceptors.Length - 1; i >= 0; i--)
            {
                if (interceptors[i] == interceptor)
                {
                    lock (_interceptors)
                    {
                        _interceptors.RemoveAt(i);
                    }
                }
            }
        }

        public void Remove<T>(Predicate<T> predicate)
        {
            var interceptors = _interceptors.AsListFast().AsSpan();

            for (var i = interceptors.Length - 1; i >= 0; i--)
            {
                bool shouldRemove;

                var candidate = interceptors[i];

                if (candidate is null)
                {
                    shouldRemove = true;
                }
                else if (candidate is T interceptor)
                {
                    shouldRemove = predicate(interceptor);
                }
                else
                {
                    shouldRemove = false;
                }

                if (shouldRemove)
                {
                    lock (_interceptors)
                    {
                        _interceptors.RemoveAt(i);
                    }
                }
            }
        }
    }
}
