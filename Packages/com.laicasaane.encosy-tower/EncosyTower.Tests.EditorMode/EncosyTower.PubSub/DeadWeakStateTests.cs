using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

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

            var context = PublishingContext.Default(logger: logger);

            var result = await messenger.Publisher.Global().PublishAsync(new Message(), context);

            Assert.AreEqual(1, earlierCount);
            Assert.AreEqual(1, laterCount);
            AssertEightStateUnavailable(result);
            Assert.AreEqual(0, logger.LogEntryCount);

            var second = await messenger.Publisher.Global().PublishAsync(new Message(), context);

            Assert.AreEqual(2, earlierCount);
            Assert.AreEqual(2, laterCount);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(0, logger.LogEntryCount);
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertEightStateUnavailable(in Success<PublishError> result)
        {
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.TryGetStateType(out var stateType));
            Assert.AreEqual(typeof(State), stateType);
            StringAssert.Contains(
                $"The state instance of type {typeof(State)} is not alive anymore.",
                error.ToMessage()
            );

            var suppressed = error.SuppressedErrors;
            var count = suppressed.Length;

            Assert.AreEqual(7, count);

            for (var i = 0; i < count; i++)
            {
                Assert.IsTrue(suppressed[i].TryGetStateType(out var suppressedStateType));
                Assert.AreEqual(typeof(State), suppressedStateType);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static WeakReference RegisterDeadHandlers(Messenger messenger)
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
        internal static WeakReference RunOnShortLivedThread(Func<WeakReference> action)
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
        internal static void CollectGarbage(WeakReference reference)
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

        internal sealed class Message : IMessage
        {
        }

        internal sealed class State
        {
        }
    }
}
