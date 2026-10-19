#if DISABLE_ENCOSY_CHECKS || !(((UNITY_EDITOR || DEBUG) && !DISABLE_ENCOSY_EDITOR_CHECKS) || ((ENCOSY_RUNTIME_CHECKS || ENCOSY_PUBSUB_RUNTIME_CHECKS) && !DISABLE_ENCOSY_RUNTIME_CHECKS))
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
    partial class MessagePublisher
    {
        partial struct UnityPublisher<TScope>
        {
            /// <remarks>
            /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask<Success<PublishError>> PublishAsync<TMessage>(PublishingContext context = default)
#if ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : new()
#else
                where TMessage : IMessage, new()
#endif
            {
                ThrowHelper.ThrowIfUnityPublisherNotCreated(IsCreated, typeof(UnityPublisher<TScope>));

                return _publisher.PublishAsync<TMessage>(context);
            }

            /// <remarks>
            /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
            /// </remarks>
#if __ENCOSY_NO_VALIDATION__
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public UnityTask<Success<PublishError>> PublishAsync<TMessage>(
                  TMessage message
                , PublishingContext context = default
            )
#if !ENCOSY_PUBSUB_RELAX_MODE
                where TMessage : IMessage
#endif
            {
                ThrowHelper.ThrowIfUnityPublisherNotCreated(IsCreated, typeof(UnityPublisher<TScope>));
                ThrowHelper.ThrowIfMessageNull(message);

                return _publisher.PublishAsync(message, context);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            partial void RetainUsings_Async();
        }
    }
}
