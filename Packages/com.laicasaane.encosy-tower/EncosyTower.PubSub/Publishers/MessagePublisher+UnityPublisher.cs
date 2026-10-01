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
using EncosyTower.UnityExtensions;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    partial class MessagePublisher
    {
        public readonly partial struct UnityPublisher<TScope> : IIsCreated
            where TScope : UnityEngine.Object
        {
            internal readonly Publisher<UnityEntityId<TScope>> _publisher;

            public bool IsCreated => _publisher.IsCreated;

            public UnityEntityId<TScope> Scope => _publisher.Scope;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Publisher<TNewScope> WithScope<TNewScope>()
                where TNewScope : struct
            {
                return _publisher.WithScope<TNewScope>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Publisher<TNewScope> WithScope<TNewScope>([NotNull] TNewScope scope)
            {
                DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(scope);

                return _publisher.WithScope(scope);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Publisher<GlobalScope> WithGlobalScope()
            {
                return _publisher.WithGlobalScope();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityPublisher<TNewScope> WithUnityScope<TNewScope>([NotNull] TNewScope scope)
                where TNewScope : UnityEngine.Object
            {
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                return _publisher.WithUnityScope(scope);
            }

            internal UnityPublisher([NotNull] MessagePublisher publisher, [NotNull] TScope scope)
            {
                DebuggingThrowHelper.ThrowIfNull(publisher);
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                _publisher = new(publisher, scope);
            }

            internal UnityPublisher(MessagePublisher publisher, TScope scope, bool invalid)
            {
                _publisher = new Publisher<UnityEntityId<TScope>>(publisher, scope, invalid);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public CachedPublisher<UnityEntityId<TScope>, TMessage> Cache<TMessage>(
                  [NotNull] Func<TMessage> createFunc
                , ILogger logger = null
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                DebuggingThrowHelper.ThrowIfNull(createFunc);

#if __ENCOSY_VALIDATION__
                if (Validate(logger) == false)
                {
                    return default;
                }
#endif

                return _publisher.Cache<TMessage>(createFunc, logger);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return;
                }
#endif

                _publisher.Publish<TMessage>(context);
            }

#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public void Publish<TMessage>(TMessage message, PublishingContext context = default)
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
#if __ENCOSY_VALIDATION__
                if (Validate(context.Logger) == false)
                {
                    return;
                }
#endif

                _publisher.Publish(message, context);
            }

#if __ENCOSY_VALIDATION__
            private bool Validate(ILogger logger)
            {
                if (IsCreated)
                {
                    return true;
                }

                ThrowHelper.LogErrorInvalidUnityPublisherAccess(GetType(), logger);

                return false;
            }
#endif

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings();
        }
    }
}
