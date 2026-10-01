#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    public readonly partial struct MessageInterceptors : IIsCreated
    {
        private readonly Internals.InterceptorBrokers _brokers;

        internal MessageInterceptors(Internals.InterceptorBrokers brokers)
        {
            _brokers = brokers;
        }

        public bool IsCreated => _brokers != null;

        public bool HasInterceptors => _brokers?.HasInterceptors ?? false;

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddInterceptor([NotNull] IMessageInterceptor interceptor, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.AddInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddInterceptor([NotNull] IScopedMessageInterceptor interceptor, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.AddInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddInterceptor<TMessage>(
              [NotNull] IMessageInterceptor<TMessage> interceptor
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.AddInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddInterceptor<TScope, TMessage>(
              [NotNull] IScopedMessageInterceptor<TScope, TMessage> interceptor
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.AddInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor([NotNull] IMessageInterceptor interceptor, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor([NotNull] IScopedMessageInterceptor interceptor, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor<TMessage>(
              [NotNull] IMessageInterceptor<TMessage> interceptor
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor<TScope, TMessage>(
              [NotNull] IScopedMessageInterceptor<TScope, TMessage> interceptor
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(interceptor);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(interceptor);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor([NotNull] Predicate<IMessageInterceptor> predicate, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(predicate);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor([NotNull] Predicate<IScopedMessageInterceptor> predicate, ILogger logger = null)
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(predicate);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor<TMessage>(
              [NotNull] Predicate<IMessageInterceptor<TMessage>> predicate
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(predicate);
        }

#if __ENCOSY_NO_VALIDATION__
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveInterceptor<TScope, TMessage>(
              [NotNull] Predicate<IScopedMessageInterceptor<TScope, TMessage>> predicate
            , ILogger logger = null
        )
#if !ENCOSY_PUBSUB_RELAX_MODE
            where TMessage : IMessage
#endif
        {
            DebuggingThrowHelper.ThrowIfNull(predicate);

#if __ENCOSY_VALIDATION__
            if (Validate(logger) == false) return;
#endif

            _brokers.RemoveInterceptor(predicate);
        }

#if __ENCOSY_VALIDATION__
        private bool Validate(ILogger logger)
        {
            if (IsCreated)
            {
                return true;
            }

            ThrowHelper.LogErrorInvalidInterceptorsAccess(logger);
            return false;
        }
#endif

#pragma warning disable IDE0051
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void RetainUsings();
#pragma warning restore IDE0051
    }
}
