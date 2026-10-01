using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.PubSub
{
    public sealed class DeadWeakStateTests
    {
        [Test]
        public async Task DeadWeakState_AllEightShapesRemoveOnlyThemselvesAndContinueFanOut()
        {
            using var messenger = new Messenger(ArrayPool<UnityTask>.Shared);
            var logger = new StringBuilderLogger();
            var earlierCount = 0;
            var laterCount = 0;
            using var earlier = messenger.Subscriber.Global()
                .Subscribe<Message>(() => earlierCount++, order: -1);
            using var later = messenger.Subscriber.Global()
                .Subscribe<Message>(() => laterCount++, order: 1);
            var weakState = RunOnShortLivedThread(
                () => RegisterDeadHandlers(messenger)
            );

            CollectGarbage(weakState);

            var context = PublishingContext.Default(warnNoSubscriber: false, logger: logger);

            await messenger.Publisher.Global().PublishAsync(new Message(), context);

            Assert.AreEqual(1, earlierCount);
            Assert.AreEqual(1, laterCount);
            Assert.AreEqual(8, logger.LogEntryCount);
            StringAssert.Contains(
                $"The state instance of type {typeof(State)} is not alive anymore.",
                logger.ToString()
            );

            await messenger.Publisher.Global().PublishAsync(new Message(), context);

            Assert.AreEqual(2, earlierCount);
            Assert.AreEqual(2, laterCount);
            Assert.AreEqual(8, logger.LogEntryCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RegisterDeadHandlers(Messenger messenger)
        {
            var state = new State();
            var subscriber = messenger.Subscriber.Global().WithState(state);

            subscriber.Subscribe<Message>(static _ => { });
            subscriber.Subscribe<Message>((Action<State, Message>)(static (_, _) => { }));
            subscriber.Subscribe<Message>(
                (Action<State, PublishingContext>)(static (_, _) => { })
            );

            subscriber.Subscribe<Message>(
                static (_, _, _) => { }
            );

            subscriber.Subscribe<Message>(
                static _ => UnityTask.CompletedTask
            );

            subscriber.Subscribe<Message>(
                (Func<State, Message, UnityTask>)(static (_, _) =>
                    UnityTask.CompletedTask
                )
            );

            subscriber.Subscribe<Message>(
                (Func<State, PublishingContext, UnityTask>)(static (_, _) =>
                    UnityTask.CompletedTask
                )
            );

            subscriber.Subscribe<Message>(
                static (_, _, _) => UnityTask.CompletedTask
            );

            return new WeakReference(state);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference RunOnShortLivedThread(Func<WeakReference> action)
        {
            WeakReference result = null;
            Exception failure = null;
            var thread = new Thread(
                () =>
                {
                    try
                    {
                        result = action();
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                    }
                }
            );

            thread.Start();
            thread.Join();

            Assert.IsNull(failure, failure?.ToString());
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CollectGarbage(WeakReference reference)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                if (reference.IsAlive == false)
                {
                    break;
                }
            }

            Assert.IsFalse(reference.IsAlive);
        }

        private sealed class Message : IMessage
        {
        }

        private sealed class State
        {
        }
    }
}
