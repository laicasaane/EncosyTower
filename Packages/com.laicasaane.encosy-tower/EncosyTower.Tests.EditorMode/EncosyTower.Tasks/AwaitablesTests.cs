using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tasks;

namespace EncosyTower.Tests.Tasks
{
    public class AwaitablesTests
    {
        [Test]
        public async Task Factories_AreFreshAndPreserveCancellationIdentity()
        {
            var first = Awaitables.CompletedTask;
            var second = Awaitables.CompletedTask;

            Assert.AreNotSame(first, second);
            await first;
            await second;

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var exception = await CaptureExpectedExceptionAsync<OperationCanceledException>(
                async () => await Awaitables.FromCanceled(cancellation.Token)
            );

            Assert.AreEqual(cancellation.Token, exception.CancellationToken);
            Assert.AreEqual(42, await Awaitables.FromResult(42));
        }

        [Test]
        public void Delay_RejectsNegativeDurationsSynchronously()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Awaitables.Delay(-1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Awaitables.Delay(TimeSpan.FromTicks(-1))
            );
        }

        [Test]
        public async Task Yield_QueuesExactlyOnePlayerLoopContinuation()
        {
            var task = Awaitables.Yield(AwaitablePlayerLoopTiming.Update);

            Assert.IsFalse(task.GetAwaiter().IsCompleted);
            await task;
        }

        [Test]
        public async Task Delay_ImmediateAndPolledCancellationPreserveToken()
        {
            using var immediateCancellation = new CancellationTokenSource();
            using var polledCancellation = new CancellationTokenSource();
            var immediate = Awaitables.Delay(
                  TimeSpan.FromMinutes(1)
                , cancellationToken: immediateCancellation.Token
                , cancelImmediately: true
            );
            var polled = Awaitables.Delay(TimeSpan.FromMinutes(1), cancellationToken: polledCancellation.Token);

            immediateCancellation.Cancel();
            polledCancellation.Cancel();

            var immediateException =
                await CaptureExpectedExceptionAsync<OperationCanceledException>(
                    async () => await immediate
                );
            var polledException =
                await CaptureExpectedExceptionAsync<OperationCanceledException>(
                    async () => await polled
                );

            Assert.AreEqual(immediateCancellation.Token, immediateException.CancellationToken);
            Assert.AreEqual(polledCancellation.Token, polledException.CancellationToken);
        }

        [Test]
        public async Task RunOnThreadPool_SwitchesBeforeInvocationAndReturnsToUnityLoop()
        {
            var mainThread = Thread.CurrentThread.ManagedThreadId;
            var workerThread = 0;

            await Awaitables.RunOnThreadPool(
                () => workerThread = Thread.CurrentThread.ManagedThreadId
            );

            Assert.AreNotEqual(mainThread, workerThread);
            Assert.AreEqual(mainThread, Thread.CurrentThread.ManagedThreadId);
        }

        [Test]
        public async Task WhenAll_PreservesOrderAndConsumesOnlyRequestedPrefix()
        {
            var first = new AwaitableCompletionSource<int>();
            var second = new AwaitableCompletionSource<int>();
            var tail = new AwaitableCompletionSource<int>();
            var combined = Awaitables.WhenAll(
                  new[] { first.Awaitable, second.Awaitable, tail.Awaitable }
                , 2
            );

            second.SetResult(2);
            first.SetResult(1);
            var results = await combined;

            CollectionAssert.AreEqual(new[] { 1, 2 }, results);
            Assert.IsFalse(tail.Awaitable.GetAwaiter().IsCompleted);
            tail.SetResult(3);
            Assert.AreEqual(3, await tail.Awaitable);
        }

        [Test]
        public async Task WhenAll_FirstObservedFaultCompletesBeforeLosersAndDoesNotAggregate()
        {
            var first = new AwaitableCompletionSource();
            var loser = new AwaitableCompletionSource();
            var combined = Awaitables.WhenAll(first.Awaitable, loser.Awaitable);
            var expected = new InvalidOperationException("first");

            first.SetException(expected);

            Assert.IsTrue(combined.GetAwaiter().IsCompleted);
            var exception = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await combined
            );
            Assert.AreSame(expected, exception);

            loser.SetResult();
            await Task.Yield();
        }

        [Test]
        public async Task WhenAny_RejectsEmptyAndResolvesCompletedTieByInputOrder()
        {
            Assert.Throws<ArgumentException>(
                () => Awaitables.WhenAny(Array.Empty<Awaitable>())
            );

            var result = await Awaitables.WhenAny(Awaitables.FromResult(10), Awaitables.FromResult(20));

            Assert.AreEqual(0, result.winArgumentIndex);
            Assert.AreEqual(10, result.result1);
            Assert.AreEqual(0, result.result2);
        }

        [Test]
        public async Task WhenAny_ConsumesLoserWithoutCancelingIt()
        {
            var winner = new AwaitableCompletionSource<int>();
            var loser = new AwaitableCompletionSource<int>();
            var combined = Awaitables.WhenAny(
                new[] { winner.Awaitable, loser.Awaitable }
            );

            winner.SetResult(7);
            var result = await combined;

            Assert.AreEqual((0, 7), result);
            loser.SetResult(9);
            await Task.Yield();
        }

        [Test]
        public async Task WhenEach_IsLazyAndSupportsDisposalBeforeFirstMove()
        {
            var enumerated = 0;
            var source = new CountingEnumerable<int>(
                new[] { Awaitables.FromResult(1) },
                () => enumerated++
            );
            var enumerable = Awaitables.WhenEach(source);

            Assert.AreEqual(0, enumerated);
            var enumerator = enumerable.GetAsyncEnumerator();
            await enumerator.DisposeAsync();
            Assert.AreEqual(0, enumerated);
        }

        [Test]
        public async Task WhenEach_YieldsCompletionOrderAndRepresentsFaults()
        {
            var first = new AwaitableCompletionSource<int>();
            var second = new AwaitableCompletionSource<int>();
            var expected = new InvalidOperationException("second");
            var enumerator = Awaitables.WhenEach(first.Awaitable, second.Awaitable).GetAsyncEnumerator();
            var firstMove = enumerator.MoveNextAsync();

            second.SetException(expected);
            Assert.IsTrue(await firstMove);
            Assert.IsTrue(enumerator.Current.IsFaulted);
            Assert.AreSame(expected, enumerator.Current.Exception);

            first.SetResult(1);
            Assert.IsTrue(await enumerator.MoveNextAsync());
            Assert.AreEqual(1, enumerator.Current.Result);
            Assert.IsFalse(await enumerator.MoveNextAsync());
            await enumerator.DisposeAsync();
        }

        [Test]
        public async Task WhenEach_EmptyCompletesAndEarlyDisposalCompletesPendingMove()
        {
            var empty = Awaitables.WhenEach(Array.Empty<Awaitable<int>>()).GetAsyncEnumerator();
            Assert.IsFalse(await empty.MoveNextAsync());
            await empty.DisposeAsync();

            var source = new AwaitableCompletionSource<int>();
            var pending = Awaitables.WhenEach(source.Awaitable).GetAsyncEnumerator();
            var move = pending.MoveNextAsync();
            await pending.DisposeAsync();
            Assert.IsFalse(await move);
            source.SetResult(1);
            await Task.Yield();
        }

        [Test]
        public async Task ContinueWith_RunsOnlyAfterSuccessAndPropagatesResult()
        {
            var invoked = false;
            var result = await Awaitables
                .FromResult(21)
                .ContinueWith(static value => value * 2);

            Assert.AreEqual(42, result);

            var failed = Awaitables
                .FromException(new InvalidOperationException("failed"))
                .ContinueWith(() => invoked = true);
            await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await failed
            );
            Assert.IsFalse(invoked);
        }

        [Test]
        public async Task TaskBridge_UnwrapsFaultWithoutAggregateException()
        {
            var expected = new InvalidOperationException("task");
            var exception = await CaptureExpectedExceptionAsync<InvalidOperationException>(
                async () => await Task.FromException(expected).AsAwaitable(false)
            );

            Assert.AreSame(expected, exception);
        }

        private static async Task<TException> CaptureExpectedExceptionAsync<TException>(
            Func<Task> action
        )
            where TException : Exception
        {
            try
            {
                await action();
            }
            catch (TException exception)
            {
                return exception;
            }

            Assert.Fail($"Expected exception of type {typeof(TException).FullName}.");
            return null;
        }

        private sealed class CountingEnumerable<T> : IEnumerable<Awaitable<T>>
        {
            private readonly IEnumerable<Awaitable<T>> _source;
            private readonly Action _onEnumerate;

            internal CountingEnumerable(IEnumerable<Awaitable<T>> source, Action onEnumerate)
            {
                _source = source;
                _onEnumerate = onEnumerate;
            }

            public IEnumerator<Awaitable<T>> GetEnumerator()
            {
                _onEnumerate();
                return _source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
