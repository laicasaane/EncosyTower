#if UNITY_UGUI

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.Processing;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Internals;
using EncosyTower.PageFlows.UguiPages.Processing;
using EncosyTower.Pooling;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageErrorDeliveryTests
    {
        private const string ELEMENT_PAGE = "error-delivery-page";

        private static readonly PageContext s_context = new() {
            ShowOptions = PageTransitionOptions.ZeroDuration,
            HideOptions = PageTransitionOptions.ZeroDuration,
        };

        private readonly List<ISubscription> _subscriptions = new();
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            _subscriptions.Unsubscribe();
            ErrorDeliveryProbePage.AbortOnAttach = null;
            ErrorDeliveryProbePage.ThrowOnAttach = null;
            ErrorDeliveryProbePage.CancelOnAttach = null;
            ErrorDeliveryProbePage.CancelOnShow = null;

            if (_root.IsValid())
            {
                UnityEngine.Object.Destroy(_root);
            }

            _root = null;
        }

        [Test]
        public async Task G1_AbortWithAHookFailure_PageAbortedMessageCarriesTheErrorAsync()
        {
            var stack = CreateFlow<UguiPageStackMulti>("Popup");
            var pool = AddPageSource(stack, ELEMENT_PAGE);
            var log = Subscribe(stack);
            var exception = new InvalidOperationException("attach failed");
            var report = new PageCallReport();
            var context = s_context with { Report = report };
            ErrorDeliveryProbePage.AbortOnAttach = PageAbortMode.Reverse;
            ErrorDeliveryProbePage.ThrowOnAttach = exception;

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, context, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.IsFalse(error.IsWithdrawn);
            Assert.IsFalse(error.IsCancelled);
            Assert.IsFalse(result.IsApplied());
            Assert.AreEqual(expected: PageTransitionState.NotStarted, actual: error.TransitionState);
            Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
            Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Page pageOrigin));
            Assert.IsInstanceOf<ErrorDeliveryProbePage>(pageOrigin.Source);
            Assert.AreEqual(expected: PageAbortMode.Reverse, actual: mode);
            AssertSingleSuppressedException(error, exception);
            AssertReport(report, "Popup", error);

            Assert.AreEqual(expected: 1, actual: log.Aborted.Count);
            var message = log.Aborted[0];
            Assert.AreEqual(expected: origin, actual: message.Origin);
            Assert.AreEqual(expected: mode, actual: message.Mode);
            Assert.AreEqual(expected: error, actual: message.Error);
            AssertSingleSuppressedException(message.Error, exception);
            Assert.AreEqual(expected: context, actual: message.Context);
            Assert.AreSame(report, message.Context.Report);
            Assert.IsEmpty(log.Failed);
            Assert.IsFalse(GetCurrentPage(stack).TryGetValue(out _));
            Assert.AreEqual(expected: 0, actual: GetPageCollection(stack).Count);
            Assert.AreEqual(expected: 1, actual: pool.PoolingCount);
            Assert.IsFalse(IsInTransition(stack));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task G3_ThrownCancel_ReportAndListenerGetTheFailureItWonOverAsync()
        {
            var stack = CreateFlow<UguiPageStackMulti>("Popup");
            AddPageSource(stack, ELEMENT_PAGE);
            var log = Subscribe(stack);
            var exception = new InvalidOperationException("began subscriber failed");
            var report = new PageCallReport();
            var context = s_context with { Report = report };
            using var cancellation = new CancellationTokenSource();
            ErrorDeliveryProbePage.CancelOnShow = cancellation;
            OperationCanceledException thrown = null;

            var subscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(_subscriptions);

            PageTransitionBeganMessage.Async.Subscribe(in subscriber, FailBeganAsync);

            try
            {
                await stack.ShowPageAsync(ELEMENT_PAGE, context, cancellation.Token);
            }
            catch (OperationCanceledException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.IsTrue(cancellation.IsCancellationRequested);
            Assert.IsTrue(report.Result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsCancelled);
            AssertSingleSuppressedException(error, exception);
            AssertReport(report, "Popup", error);
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            Assert.AreEqual(expected: error, actual: log.Failed[0].Error);
            AssertSingleSuppressedException(log.Failed[0].Error, exception);
            Assert.AreEqual(expected: context, actual: log.Failed[0].Context);
            Assert.AreSame(report, log.Failed[0].Context.Report);
            Assert.IsEmpty(log.Aborted);
            Assert.IsTrue(GetCurrentPage(stack).TryGetValue(out var page));
            Assert.IsInstanceOf<ErrorDeliveryProbePage>(page);
            Assert.AreEqual(expected: 1, actual: GetPageCollection(stack).Count);
            Assert.IsFalse(IsInTransition(stack));
            LogAssert.NoUnexpectedReceived();

            static UnityTask FailBeganAsync(
                  InvalidOperationException ex
                , PageTransitionBeganMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        [Test]
        public async Task G4_NavigatedSubscriberFailure_IsInTheResultAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource(stack, ELEMENT_PAGE);
            var log = Subscribe(stack);
            var exception = new InvalidOperationException("navigated subscriber failed");
            var report = new PageCallReport();
            var context = s_context with { Report = report };

            var subscriber = stack.Context.Subscriber.Scope(stack.Context.FlowScope)
                .WithState(exception)
                .WithSubscriptions(_subscriptions);

            PageNavigatedMessage.Async.Subscribe(in subscriber, FailNavigatedAsync);

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, context, CancellationToken.None);

            Assert.IsTrue(result.TryGetFailure(out var error));
            AssertException(error, exception);
            Assert.IsFalse(error.HasSuppressedErrors);
            Assert.AreEqual(expected: PageTransitionState.Completed, actual: error.TransitionState);
            Assert.IsTrue(result.IsApplied());
            AssertReport(report, "Screen", error);
            Assert.IsTrue(report.Result.IsApplied());
            Assert.AreEqual(expected: 1, actual: log.Failed.Count);
            Assert.AreEqual(expected: error, actual: log.Failed[0].Error);
            AssertException(log.Failed[0].Error, exception);
            Assert.AreEqual(expected: context, actual: log.Failed[0].Context);
            Assert.AreSame(report, log.Failed[0].Context.Report);
            Assert.IsEmpty(log.Aborted);
            Assert.IsTrue(GetCurrentPage(stack).TryGetValue(out var page));
            Assert.IsInstanceOf<ErrorDeliveryProbePage>(page);
            Assert.IsTrue(((ErrorDeliveryProbePage)page).gameObject.activeSelf);
            Assert.IsFalse(IsInTransition(stack));
            LogAssert.NoUnexpectedReceived();

            static UnityTask FailNavigatedAsync(
                  InvalidOperationException ex
                , PageNavigatedMessage.Async message
                , PublishingContext publishingContext
            )
                => throw ex;
        }

        private static void AssertException(PageFlowError error, Exception exception)
        {
            Assert.IsTrue(error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(exception, wrapped.Ex);
            Assert.AreEqual(expected: PageFlowError.Exception(exception).ToMessage(), actual: error.ToMessage());
        }

        private static void AssertSingleSuppressedException(PageFlowError error, Exception exception)
        {
            Assert.IsTrue(error.HasSuppressedErrors);
            Assert.AreEqual(expected: 1, actual: error.SuppressedErrors.Length);
            var suppressed = error.SuppressedErrors[0];
            AssertException(suppressed, exception);
            Assert.IsFalse(suppressed.HasSuppressedErrors);
        }

        private static void AssertReport(PageCallReport report, string flowId, PageFlowError error)
        {
            Assert.IsTrue(report.IsReported);
            Assert.AreEqual(expected: flowId, actual: report.FlowId);
            Assert.IsTrue(report.Result.TryGetFailure(out var reported));
            Assert.AreEqual(expected: error, actual: reported);
        }

        private static Option<IUguiPage> GetCurrentPage(UguiPageFlow flow)
        {
            var hub = flow.Context.ProcessHub;

            var result = GetCurrentPageRequest.TryProcess(
                  in hub
                , new GetCurrentPageRequest()
                , ProcessingContext.DropIfNoHandler()
            );

            Assert.IsTrue(result.TryGetValue(out var page));
            return page;
        }

        private static IReadOnlyCollection<IUguiPage> GetPageCollection(UguiPageFlow flow)
        {
            var hub = flow.Context.ProcessHub;

            var result = GetPageCollectionRequest.TryProcess(
                  in hub
                , new GetPageCollectionRequest()
                , ProcessingContext.DropIfNoHandler()
            );

            Assert.IsTrue(result.TryGetValue(out var pages));
            return pages;
        }

        private static bool IsInTransition(UguiPageFlow flow)
        {
            var hub = flow.Context.ProcessHub;

            var result = IsInTransitionRequest.TryProcess(
                  in hub
                , new IsInTransitionRequest()
                , ProcessingContext.DropIfNoHandler()
            );

            Assert.IsTrue(result.TryGetValue(out var inTransition));
            return inTransition;
        }

        private T CreateFlow<T>(string name)
            where T : UguiPageFlow
        {
            _root = new GameObject(nameof(UguiPageErrorDeliveryTests), typeof(RectTransform));

            var context = new UguiPageFlowContext {
                loadStrategy = new(value: PageLoaderStrategy.Resources, isOverridden: true),
            };

            return UguiPageFlow.Create<T>(name, _root.GetComponent<RectTransform>(), context);
        }

        private UguiPagePool AddPageSource(UguiPageFlow flow, string key)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

            var source = new GameObject(key, typeof(RectTransform));
            source.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            source.AddComponent<ErrorDeliveryProbePage>();
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
            return pool;
        }

        private MessageLog Subscribe(UguiPageFlow flow)
        {
            var log = new MessageLog();

            var subscriber = flow.Context.Subscriber.Scope(flow.Context.FlowScope)
                .WithState(log)
                .WithSubscriptions(_subscriptions);

            PageAbortedMessage.Subscribe(in subscriber, HandleAborted);
            PageFlowFailedMessage.Subscribe(in subscriber, HandleFailed);
            return log;

            static void HandleAborted(MessageLog messages, PageAbortedMessage message)
                => messages.Aborted.Add(message);

            static void HandleFailed(MessageLog messages, PageFlowFailedMessage message)
                => messages.Failed.Add(message);
        }

        private sealed class MessageLog
        {
            public readonly List<PageAbortedMessage> Aborted = new();
            public readonly List<PageFlowFailedMessage> Failed = new();
        }

        private sealed class ErrorDeliveryProbePage
            : MonoBehaviour
            , IUguiPage
            , IPageOnAttachToFlowAsync
            , IPageOnShowAsync
        {
            public static PageAbortMode? AbortOnAttach { get; set; }

            public static Exception ThrowOnAttach { get; set; }

            public static CancellationTokenSource CancelOnAttach { get; set; }

            public static CancellationTokenSource CancelOnShow { get; set; }

            public UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
            {
                if (AbortOnAttach.HasValue)
                {
                    _ = context.Abort.Request(AbortOnAttach.Value);
                }

                if (ThrowOnAttach != null)
                {
                    throw ThrowOnAttach;
                }

                CancelOnAttach?.Cancel();
                return UnityTask.FromResult(true);
            }

            public UnityTask OnShowAsync(PageContext context, CancellationToken token)
            {
                if (CancelOnShow == null)
                {
                    return UnityTask.CompletedTask;
                }

                CancelOnShow.Cancel();
                return UnityTask.FromCanceled(token);
            }
        }
    }
}

#endif
