#if UNITY_UGUI

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Internals;
using EncosyTower.Pooling;
using EncosyTower.PubSub;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageFlowManyTests
    {
        private const string SLOW_KEY = "many-slow";
        private const string FAST_KEY = "many-fast";
        private const string THIRD_KEY = "many-third";

        private static readonly PageContext s_context = new() {
            ShowOptions = PageTransitionOptions.ZeroDuration,
            HideOptions = PageTransitionOptions.ZeroDuration,
        };

        private readonly List<ISubscription> _subscriptions = new();
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            ManyProbePage.Events.Clear();
            ManyProbePage.Instances.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Unsubscribe();
            ManyProbePage.Events.Clear();
            ManyProbePage.Instances.Clear();
            ManyProbePage.ThrowCancelledOnDetach = false;

            if (_root.IsValid())
            {
                UnityEngine.Object.Destroy(_root);
            }
        }

        [Test]
        public async Task M1_StackHidesEveryPageTogetherThenPoolsThemAsync()
        {
            var stack = CreateStack();
            var log = Subscribe(stack);

            Assert.IsTrue((await stack.ShowPageAsync(SLOW_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await stack.ShowPageAsync(FAST_KEY, s_context, CancellationToken.None)).IsSuccess);

            var top = ManyProbePage.Instances[1];
            ManyProbePage.Events.Clear();

            var result = await stack.HideAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);

            CollectionAssert.AreEqual(
                  new[] {
                      "Hide:many-fast", "Hide:many-slow"
                    , "AfterHide:many-fast", "AfterHide:many-slow"
                    , "Detach:many-fast", "Detach:many-slow"
                    , "Return:many-fast", "Return:many-slow"
                  }
                , ManyProbePage.Events
            );

            Assert.AreEqual(expected: 4, actual: log.Navigated.Count);
            Assert.AreEqual(expected: PageNavigationKind.Pop, actual: log.Navigated[2].Kind);
            Assert.AreSame(top, log.Navigated[2].From.Page);
            Assert.AreSame(ManyProbePage.Instances[0], log.Navigated[2].To.Page);
            Assert.AreEqual(expected: PageNavigationKind.Pop, actual: log.Navigated[3].Kind);
            Assert.AreSame(log.Navigated[2].To.Page, log.Navigated[3].From.Page);
            Assert.IsNull(log.Navigated[3].To.Page);
            Assert.IsEmpty(log.Failed);
            await AssertStackIsEmptyAsync(stack);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M2_StackWithoutPageReturnsNoPageAsync()
        {
            var stack = CreateStack();

            var result = await stack.HideAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: error.ToMessage());
            Assert.IsEmpty(ManyProbePage.Events);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M3_AbortCurrentAbortsEveryPageTransitionAsync()
        {
            var stack = CreateStack();

            Assert.IsTrue((await stack.ShowPageAsync(SLOW_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await stack.ShowPageAsync(SLOW_KEY, s_context, CancellationToken.None)).IsSuccess);

            ManyProbePage.Events.Clear();

            var pending = stack.HideAllPagesAsync(s_context, CancellationToken.None);

            await WaitUntilAsync(() => ManyProbePage.Events.Count >= 2);
            Assert.IsTrue(stack.AbortCurrent(PageAbortMode.Reverse).IsSuccess);

            var result = await pending;

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.AreEqual(expected: PageTransitionState.Undone, actual: error.TransitionState);
            Assert.AreEqual(expected: 2, actual: CountEvents("Hide:"));
            Assert.AreEqual(expected: 2, actual: CountEvents("HideCancelled:"));

            Assert.AreEqual(expected: 0, actual: CountEvents("AfterHide:"));
            Assert.AreEqual(expected: 0, actual: CountEvents("Return:"));
            Assert.IsTrue(stack.AbortCurrent(PageAbortMode.Reverse).IsFailure);

            Assert.IsTrue((await stack.HideAllPagesAsync(s_context, CancellationToken.None)).IsSuccess);
            Assert.AreEqual(expected: 2, actual: CountEvents("Return:"));
            await AssertStackIsEmptyAsync(stack);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M6_CancelledCallWithCancelledDetachReturnsEveryPageOnceAsync()
        {
            var stack = CreateStack();
            using var source = new CancellationTokenSource();

            Assert.IsTrue((await stack.ShowPageAsync(SLOW_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await stack.ShowPageAsync(FAST_KEY, s_context, CancellationToken.None)).IsSuccess);

            ManyProbePage.Events.Clear();
            ManyProbePage.ThrowCancelledOnDetach = true;

            var pending = stack.HideAllPagesAsync(s_context, source.Token);

            await WaitUntilAsync(() => ManyProbePage.Events.Count >= 2);
            source.Cancel();

            OperationCanceledException thrown = null;

            try
            {
                await pending;
            }
            catch (OperationCanceledException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.AreEqual(expected: 1, actual: CountEvents("Return:many-fast"));
            Assert.AreEqual(expected: 1, actual: CountEvents("Return:many-slow"));
            await AssertStackIsEmptyAsync(stack);
        }

        [Test]
        public async Task M4_MessageHidesEveryPageAndReportsOnceAsync()
        {
            var stack = CreateStack();

            Assert.IsTrue((await stack.ShowPageAsync(SLOW_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await stack.ShowPageAsync(FAST_KEY, s_context, CancellationToken.None)).IsSuccess);

            var report = new PageCallReport();
            var publisher = stack.Context.Publisher.Scope(stack.Context.FlowScope);

            var published = await HideAllPagesMessage.Async.Publish(
                  in publisher
                , new HideAllPagesMessage(s_context with { Report = report })
                , PublishingContext.Default()
            );

            Assert.IsTrue(published.IsSuccess);
            Assert.IsTrue(report.IsReported);
            Assert.IsTrue(report.Result.IsSuccess);
            await AssertStackIsEmptyAsync(stack);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task M5_ListHidesShownPagesThenShowsAllPagesAsync()
        {
            var list = CreateFlow<UguiPageListMulti>("List");
            var log = Subscribe(list);

            AddPageSource(list, SLOW_KEY);
            AddPageSource(list, FAST_KEY);
            AddPageSource(list, THIRD_KEY);

            Assert.IsTrue((await list.AddPageAsync(SLOW_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.AddPageAsync(FAST_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.AddPageAsync(THIRD_KEY, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.ShowPageAtIndexAsync(0, s_context, CancellationToken.None)).IsSuccess);
            Assert.IsTrue((await list.ShowPageAtIndexAsync(1, s_context, CancellationToken.None)).IsSuccess);

            ManyProbePage.Events.Clear();
            log.Navigated.Clear();

            var hidden = await list.HideAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(hidden.IsSuccess);

            CollectionAssert.AreEqual(
                  new[] { "Hide:many-slow", "Hide:many-fast", "AfterHide:many-slow", "AfterHide:many-fast" }
                , ManyProbePage.Events
            );

            Assert.AreEqual(expected: 3, actual: ManyProbePage.Instances.Count);
            Assert.AreEqual(expected: 0, actual: CountEvents("Detach:"));
            Assert.IsFalse(IsActive(0));
            Assert.IsFalse(IsActive(1));
            Assert.IsFalse(IsActive(2));
            Assert.AreEqual(expected: 2, actual: log.Navigated.Count);
            Assert.AreEqual(expected: PageNavigationKind.HideAt, actual: log.Navigated[0].Kind);
            Assert.AreEqual(expected: 0, actual: log.Navigated[0].From.Index.GetValueOrThrow());
            Assert.AreEqual(expected: PageNavigationKind.HideAt, actual: log.Navigated[1].Kind);
            Assert.AreEqual(expected: 1, actual: log.Navigated[1].From.Index.GetValueOrThrow());

            var none = await list.HideAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(none.TryGetFailure(out var noPage));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: noPage.ToMessage());

            ManyProbePage.Events.Clear();
            log.Navigated.Clear();

            var shown = await list.ShowAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(shown.IsSuccess);

            CollectionAssert.AreEqual(
                  new[] {
                      "Show:many-slow", "Show:many-fast", "Show:many-third"
                    , "AfterShow:many-slow", "AfterShow:many-fast", "AfterShow:many-third"
                  }
                , ManyProbePage.Events
            );

            Assert.AreEqual(expected: 3, actual: ManyProbePage.Instances.Count);
            Assert.AreEqual(expected: 0, actual: CountEvents("Detach:"));
            Assert.IsTrue(IsActive(0));
            Assert.IsTrue(IsActive(1));
            Assert.IsTrue(IsActive(2));
            Assert.AreEqual(expected: 3, actual: log.Navigated.Count);

            for (var i = 0; i < 3; i++)
            {
                Assert.AreEqual(expected: PageNavigationKind.ShowAt, actual: log.Navigated[i].Kind);
                Assert.AreEqual(expected: i, actual: log.Navigated[i].To.Index.GetValueOrThrow());
            }

            var all = await list.ShowAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(all.TryGetFailure(out var allShown));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: allShown.ToMessage());
            LogAssert.NoUnexpectedReceived();
        }

        private static int CountEvents(string prefix)
            => ManyProbePage.Events.FindAll(e => e.StartsWith(prefix)).Count;

        private static async Task AssertStackIsEmptyAsync(UguiPageStackMulti stack)
        {
            var result = await stack.HideAllPagesAsync(s_context, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(expected: PageFlowError.NoPage().ToMessage(), actual: error.ToMessage());
        }

        private static bool IsActive(int index)
            => ManyProbePage.Instances[index].gameObject.activeSelf;

        private static async Task WaitUntilAsync(System.Func<bool> condition, int maxFrames = 300)
        {
            for (var i = 0; i < maxFrames; i++)
            {
                if (condition())
                {
                    return;
                }

                await UnityTask.NextFrameAsync();
            }

            Assert.Fail($"The condition was not met within {maxFrames} frames.");
        }

        private static void Handle(MessageLog log, PageFlowFailedMessage message)
            => log.Failed.Add(message);

        private static UnityTask HandleAsync(MessageLog log, PageNavigatedMessage.Async message, PublishingContext _)
        {
            log.Navigated.Add(((PageNavigatedMessage)message).Navigation);
            return UnityTask.CompletedTask;
        }

        private UguiPageStackMulti CreateStack()
        {
            var stack = CreateFlow<UguiPageStackMulti>("Stack");

            AddPageSource(stack, SLOW_KEY);
            AddPageSource(stack, FAST_KEY);

            return stack;
        }

        private T CreateFlow<T>(string name)
            where T : UguiPageFlow
        {
            _root = new GameObject(nameof(UguiPageFlowManyTests), typeof(RectTransform));

            var context = new UguiPageFlowContext {
                loadStrategy = new(value: PageLoaderStrategy.Resources, isOverridden: true),
            };

            return UguiPageFlow.Create<T>(name, _root.GetComponent<RectTransform>(), context);
        }

        private void AddPageSource(UguiPageFlow flow, string key)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

            var source = new GameObject(key, typeof(RectTransform));
            source.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            source.AddComponent<ManyProbePage>();
            source.SetActive(false);

            var flowType = typeof(UguiPageFlow);
            var pools = (Dictionary<StringId, UguiPagePool>)flowType.GetField("_pageIdToPool", FLAGS).GetValue(flow);
            var pageIds = (List<UguiPageIdentifier>)flowType.GetField("_pageIds", FLAGS).GetValue(flow);

            var pool = new UguiPagePool(
                  flow.PoolTransform
                , flow.RectTransform
                , pageIds
                , RentingStrategy.Default
                , ReturningStrategy.Default
            );

            pool.Initialize(new PageAssetHandle<GameObject>(source));
            pools[flow.Context.MakeStringId(key)] = pool;
        }

        private MessageLog Subscribe(UguiPageFlow flow)
        {
            var log = new MessageLog();

            var subscriber = flow.Context.Subscriber.Scope(flow.Context.FlowScope)
                .WithState(log)
                .WithSubscriptions(_subscriptions);

            PageFlowFailedMessage.Subscribe(in subscriber, Handle);
            PageNavigatedMessage.Async.Subscribe(in subscriber, HandleAsync);
            return log;
        }

        private sealed class MessageLog
        {
            public readonly List<PageFlowFailedMessage> Failed = new();
            public readonly List<PageNavigation> Navigated = new();
        }

        private sealed class ManyProbePage
            : MonoBehaviour
            , IUguiPage
            , IPageOnAttachToFlowAsync
            , IPageOnDetachFromFlowAsync
            , IPageOnShowAsync
            , IPageOnAfterShow
            , IPageOnHideAsync
            , IPageOnAfterHide
            , IPageOnReturnToPool
        {
            public static List<string> Events { get; } = new();

            public static List<ManyProbePage> Instances { get; } = new();

            public static bool ThrowCancelledOnDetach { get; set; }

            private string Key
                => name.Replace("(Clone)", string.Empty);

            public UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
            {
                Instances.Add(this);
                return UnityTask.FromResult(true);
            }

            public UnityTask<bool> OnDetachFromFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
            {
                Events.Add($"Detach:{Key}");

                if (ThrowCancelledOnDetach)
                {
                    throw new OperationCanceledException();
                }

                return UnityTask.FromResult(true);
            }

            public UnityTask OnShowAsync(PageContext context, CancellationToken token)
            {
                Events.Add($"Show:{Key}");
                return UnityTask.CompletedTask;
            }

            public void OnAfterShow(PageContext context)
            {
                Events.Add($"AfterShow:{Key}");
            }

            public async UnityTask OnHideAsync(PageContext context, CancellationToken token)
            {
                Events.Add($"Hide:{Key}");

                var frames = Key == SLOW_KEY ? 6 : 1;

                for (var i = 0; i < frames; i++)
                {
                    await UnityTask.NextFrameAsync();

                    if (token.IsCancellationRequested)
                    {
                        Events.Add($"HideCancelled:{Key}");
                        token.ThrowIfCancellationRequested();
                    }
                }
            }

            public void OnAfterHide(PageContext context)
            {
                Events.Add($"AfterHide:{Key}");
            }

            public void OnReturnToPool(PageContext context)
            {
                Events.Add($"Return:{Key}");
            }
        }
    }
}

#endif
