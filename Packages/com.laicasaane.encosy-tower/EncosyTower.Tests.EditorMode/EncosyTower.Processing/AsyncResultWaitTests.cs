using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.Processing
{
    public class AsyncResultWaitTests
    {
        [Test]
        public async Task DelayedResultHandler_ReleasesProcessAsyncWait()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var hub = processor.Global();
            var logger = new StringBuilderLogger();
            var expectedContext = ProcessingContext.WaitForHandler(logger, default, cancellation.Token);
            var pending = hub.ProcessAsync<AsyncResultRequest, int>(new AsyncResultRequest(23), expectedContext);
            var observedContext = default(ProcessingContext);
            Func<AsyncResultRequest, ProcessingContext, UnityTask<int>> handler =
                (request, context) =>
                {
                    observedContext = context;
                    return UnityTask.FromResult(request.Value);
                };

            hub.Register<AsyncResultRequest, int>(handler);

            Assert.AreEqual(23, await pending);
            Assert.AreEqual(expectedContext.Strategy, observedContext.Strategy);
            Assert.AreEqual(expectedContext.WarnNoHandler, observedContext.WarnNoHandler);
            Assert.AreSame(logger, observedContext.Logger);
            Assert.AreEqual(expectedContext.Token, observedContext.Token);
        }

        [Test]
        public async Task DelayedResultHandler_ReleasesTryProcessAsyncWait()
        {
            using var processor = new Processor();
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var hub = processor.Global();
            var pending = hub.TryProcessAsync<AsyncResultRequest, int>(
                  new AsyncResultRequest(29)
                , ProcessingContext.WaitForHandler(token: cancellation.Token)
            );
            Func<AsyncResultRequest, UnityTask<int>> handler =
                static request => UnityTask.FromResult(request.Value);

            hub.Register<AsyncResultRequest, int>(handler);

            var result = await pending;

            Assert.IsTrue(result.TryGetValue(out var value));
            Assert.AreEqual(29, value);
        }

        private readonly struct AsyncResultRequest : IAsyncRequest<int>
        {
            public AsyncResultRequest(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }
    }
}
