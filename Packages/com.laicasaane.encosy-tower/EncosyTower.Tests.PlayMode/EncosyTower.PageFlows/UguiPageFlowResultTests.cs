#if UNITY_UGUI

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.History;
using EncosyTower.PageFlows.Loading;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.PageFlows.UguiPages.Internals;
using EncosyTower.PageFlows.UguiPages.Transitions;
using EncosyTower.Pooling;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageFlowResultTests
    {
        private const string PROBE_KEY = "probe-key";
        private const string MISSING_KEY = "probe-missing";
        private const string FIRST_KEY = "probe-first";
        private const string THROWING_KEY = "probe-throwing";

        private GameObject _root;
        private PageHistory _history;

        private static UguiPageIdentifier CreateIdentifier(ProbePage page)
        {
            var identifier = page.gameObject.AddComponent<UguiPageIdentifier>();
            identifier.Page = page;
            identifier.GameObject = page.gameObject;
            identifier.GameObjectId = page.gameObject;
            identifier.AssetKey = PROBE_KEY;
            return identifier;
        }

        [TearDown]
        public void TearDown()
        {
            ThrowingPage.Failure = null;
            SlowCreatePage.Reset();
            _history?.Dispose();
            _history = null;

            if (_root.IsValid())
            {
                Object.Destroy(_root);
            }
        }

        [UnityTest]
        public IEnumerator Return_PlainPageReportsPageNotComponent()
        {
            var flow = CreateFlow();
            var page = new PlainPage();

            var result = flow.Return(page: page, context: default);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual("The page 'plain' is not a UnityEngine.Component.", error.ToMessage());
            LogAssert.NoUnexpectedReceived();
            yield break;
        }

        [UnityTest]
        public IEnumerator Return_PageWithoutIdentifierReportsPageIdentifierMissing()
        {
            var flow = CreateFlow();
            var page = CreatePage();

            var result = flow.Return(page: page, context: default);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(PageFlowError.PageIdentifierMissing(page).ToMessage(), error.ToMessage());
            LogAssert.NoUnexpectedReceived();
            yield break;
        }

        [UnityTest]
        public IEnumerator Return_UninitializedIdentifierReportsPageDestroyed()
        {
            var flow = CreateFlow();
            var page = CreatePage();
            var identifier = page.gameObject.AddComponent<UguiPageIdentifier>();
            identifier.Page = page;

            var result = flow.Return(identifier: identifier, context: default);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(PageFlowError.PageDestroyed(page).ToMessage(), error.ToMessage());
            LogAssert.NoUnexpectedReceived();
            yield break;
        }

        [UnityTest]
        public IEnumerator Return_UnknownPoolReportsPoolNotFound()
        {
            var flow = CreateFlow();
            var page = CreatePage();
            var identifier = CreateIdentifier(page);
            var context = new PageContext { ReturnOperation = PageReturnOperation.Pooling };

            var result = flow.Return(identifier, context);

            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.AreEqual(PageFlowError.PoolNotFound(PROBE_KEY).ToMessage(), error.ToMessage());
            LogAssert.NoUnexpectedReceived();
            yield break;
        }

        [UnityTest]
        public IEnumerator Return_DestroySucceedsAndDestroysThePage()
        {
            var flow = CreateFlow();
            var page = CreatePage();
            var gameObject = page.gameObject;
            CreateIdentifier(page);
            var context = new PageContext { ReturnOperation = PageReturnOperation.Destroy };

            var result = flow.Return(page, context);

            Assert.IsTrue(result.IsSuccess);
            yield return null;

            Assert.IsTrue(gameObject.IsInvalid());
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Rent_AssetWithoutPageReportsAssetNotSinglePage()
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var source = new GameObject("no-page");
            var poolParent = new GameObject("pool");
            var activeParent = new GameObject("active");

            source.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            poolParent.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            activeParent.transform.SetParent(parent: _root.transform, worldPositionStays: false);

            var pool = new UguiPagePool(
                  poolParent.transform
                , activeParent.transform
                , new List<UguiPageIdentifier>()
                , RentingStrategy.Default
                , ReturningStrategy.Default
            );

            pool.Initialize(new PageAssetHandle<GameObject>(source));

            var result = pool.Rent(keyId: default, pageAssetKey: PROBE_KEY, logger: default);

            Assert.IsTrue(result.TryGetError(out var error));

            Assert.AreEqual(
                  PageFlowError.AssetNotSinglePage(assetKey: PROBE_KEY, pageCount: 0).ToMessage()
                , error.ToMessage()
            );

            pool.Dispose();
            LogAssert.NoUnexpectedReceived();
            yield break;
        }

        [UnityTest]
        public IEnumerator Prepool_MissingResourceReportsAssetLoadFailed()
        {
            var flow = CreateFlow();

            yield return flow.PrepoolPageAsync(pageAssetKey: MISSING_KEY, amount: 1, token: CancellationToken.None)
                .ToCoroutine(AssertResult);

            LogAssert.NoUnexpectedReceived();

            static void AssertResult(Success<PageFlowError> result)
            {
                Assert.IsTrue(result.TryGetFailure(out var error));

                StringAssert.StartsWith(
                      "Cannot load the page asset with key 'probe-missing': Resources: "
                    , error.ToMessage()
                );
            }
        }

        [UnityTest]
        public IEnumerator Prepool_NonpositiveAmountsReportInvalidPrepoolAmount()
        {
            var flow = CreateFlow();

            yield return flow.PrepoolPageAsync(pageAssetKey: PROBE_KEY, amount: 0, token: CancellationToken.None)
                .ToCoroutine(AssertZeroResult);

            yield return flow.PrepoolPageAsync(pageAssetKey: PROBE_KEY, amount: -2, token: CancellationToken.None)
                .ToCoroutine(AssertNegativeResult);

            LogAssert.NoUnexpectedReceived();

            static void AssertZeroResult(Success<PageFlowError> result)
            {
                Assert.IsTrue(result.TryGetFailure(out var error));
                Assert.AreEqual(PageFlowError.InvalidPrepoolAmount(amount: 0).ToMessage(), error.ToMessage());
            }

            static void AssertNegativeResult(Success<PageFlowError> result)
            {
                Assert.IsTrue(result.TryGetFailure(out var error));
                Assert.AreEqual(PageFlowError.InvalidPrepoolAmount(amount: -2).ToMessage(), error.ToMessage());
            }
        }

        [UnityTest]
        public IEnumerator Prepool_CancelledTokenReportsCancelledWithoutLogging()
        {
            var flow = CreateFlow();
            using var source = new CancellationTokenSource();
            source.Cancel();

            yield return flow.PrepoolPageAsync(pageAssetKey: PROBE_KEY, amount: 1, token: source.Token)
                .ToCoroutine(AssertResult);

            LogAssert.NoUnexpectedReceived();

            static void AssertResult(Success<PageFlowError> result)
            {
                Assert.IsTrue(result.TryGetFailure(out var error));
                Assert.IsTrue(error.IsCancelled);
            }
        }

        [Test]
        public async Task PageStackSingle_BackReportsTheExceptionOfAThrowingPageAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<ProbePage>(stack, FIRST_KEY);
            AddPageSource<ThrowingPage>(stack, THROWING_KEY);

            _history = new PageHistory();
            Assert.IsTrue(_history.Attach(stack).IsSuccess);

            var context = new PageContext {
                ShowOptions = PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.ZeroDuration,
            };

            var firstResult = await stack.ShowPageAsync(FIRST_KEY, context, CancellationToken.None);
            var throwingResult = await stack.ShowPageAsync(THROWING_KEY, context, CancellationToken.None);

            Assert.IsTrue(firstResult.IsSuccess);
            Assert.IsTrue(throwingResult.IsSuccess);

            var thrown = new System.InvalidOperationException("replay-hook-failed");
            ThrowingPage.Failure = thrown;

            var result = await GoBackWithThrowingPageAsync();

            AssertReplayFailed(result, thrown);
            Assert.AreEqual(1, _history.CurrentIndex);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_BackFromTheFirstPageReportsTheHideExceptionAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<ThrowingPage>(stack, THROWING_KEY);

            _history = new PageHistory();
            Assert.IsTrue(_history.Attach(stack).IsSuccess);

            var context = new PageContext {
                ShowOptions = PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.ZeroDuration,
            };

            var showResult = await stack.ShowPageAsync(THROWING_KEY, context, CancellationToken.None);
            Assert.IsTrue(showResult.IsSuccess);

            var thrown = new System.InvalidOperationException("replay-hook-failed");
            ThrowingPage.Failure = thrown;

            var result = await GoBackWithThrowingPageAsync();

            AssertReplayFailed(result, thrown);
            Assert.AreEqual(0, _history.CurrentIndex);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowKeepsTheIncomingPageTransparentWhileItIsCreatedAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<SlowCreatePage>(stack, FIRST_KEY);

            var context = new PageContext {
                ShowOptions = PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.ZeroDuration,
            };

            var result = await stack.ShowPageAsync(FIRST_KEY, context, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreSame(expected: stack.PoolTransform, actual: SlowCreatePage.ParentOnCreateStart);
            Assert.AreEqual(expected: 0f, actual: SlowCreatePage.VisibilityOnCreateStart);
            Assert.AreEqual(expected: 0f, actual: SlowCreatePage.VisibilityAfterNextFrame);
            Assert.AreSame(expected: stack.RectTransform, actual: SlowCreatePage.Instance.transform.parent);
            Assert.AreEqual(expected: 1f, actual: GetVisibility(SlowCreatePage.Instance.transform));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowWithoutTransitionEndsVisibleAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<SlowCreatePage>(stack, FIRST_KEY);

            var context = new PageContext {
                ShowOptions = PageTransitionOptions.NoTransition,
                HideOptions = PageTransitionOptions.ZeroDuration,
            };

            var result = await stack.ShowPageAsync(FIRST_KEY, context, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expected: 0f, actual: SlowCreatePage.VisibilityAfterNextFrame);
            Assert.AreSame(expected: stack.RectTransform, actual: SlowCreatePage.Instance.transform.parent);
            Assert.AreEqual(expected: 1f, actual: GetVisibility(SlowCreatePage.Instance.transform));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowOfARejectedPageNeverReachesTheActiveRootAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<SlowCreatePage>(stack, FIRST_KEY);
            SlowCreatePage.Accept = false;

            var result = await stack.ShowPageAsync(FIRST_KEY, default, CancellationToken.None);

            Assert.IsTrue(result.IsFailure);
            Assert.AreSame(expected: stack.PoolTransform, actual: SlowCreatePage.ParentOnCreateStart);
            Assert.AreSame(expected: stack.PoolTransform, actual: SlowCreatePage.Instance.transform.parent);
            Assert.AreEqual(expected: 0f, actual: GetVisibility(SlowCreatePage.Instance.transform));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowCancelledWhileCreatingNeverReachesTheActiveRootAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<SlowCreatePage>(stack, FIRST_KEY);

            using var source = new CancellationTokenSource();
            SlowCreatePage.CancelAfterNextFrame = source;

            var result = await stack.ShowPageAsync(FIRST_KEY, default, source.Token);

            Assert.IsTrue(result.IsFailure);
            Assert.AreSame(expected: stack.PoolTransform, actual: SlowCreatePage.Instance.transform.parent);
            Assert.AreEqual(expected: 0f, actual: GetVisibility(SlowCreatePage.Instance.transform));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_PageAbortsItsOwnCreationAsync()
        {
            var stack = CreateFlow<UguiPageStackSingle>("Screen");
            AddPageSource<SlowCreatePage>(stack, FIRST_KEY);
            SlowCreatePage.AbortInCreate = PageAbortMode.Reverse;

            var result = await stack.ShowPageAsync(FIRST_KEY, default, CancellationToken.None);

            Assert.IsTrue(SlowCreatePage.AbortRequest.IsSuccess);
            Assert.IsTrue(result.TryGetFailure(out var error));
            Assert.IsTrue(error.IsAborted);
            Assert.IsTrue(error.TryGetAbort(out var origin, out var mode));
            Assert.IsTrue(origin.TryGetValue(out PageAbortOrigin.Page pageOrigin));
            Assert.AreSame(expected: SlowCreatePage.Instance, actual: pageOrigin.Source);
            Assert.AreEqual(expected: PageAbortMode.Reverse, actual: mode);
            Assert.AreSame(expected: stack.PoolTransform, actual: SlowCreatePage.Instance.transform.parent);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageListSingle_AddGivesOnCreateAHandleThatCannotAbortAsync()
        {
            var list = CreateFlow<UguiPageListSingle>("List");
            AddPageSource<SlowCreatePage>(list, FIRST_KEY);
            SlowCreatePage.AbortInCreate = PageAbortMode.Reverse;

            var result = await list.AddPageAsync(FIRST_KEY, default, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(SlowCreatePage.AbortRequest.TryGetFailure(out var abortError));

            Assert.AreEqual(PageAbortError.EnumCase.OperationNotAbortable, abortError.GetEnumCase());
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowOnAnUninitializedFlowReportsToTheReceiptAsync()
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var stack = CreateChildRect("Screen").gameObject.AddComponent<UguiPageStackSingle>();
            var receipt = new PageCallReceipt();
            System.InvalidOperationException thrown = null;

            try
            {
                await stack.ShowPageAsync(FIRST_KEY, new PageContext { Receipt = receipt }, CancellationToken.None);
            }
            catch (System.InvalidOperationException ex)
            {
                thrown = ex;
            }

            AssertReportedException(receipt, thrown);
        }

        [Test]
        public async Task PageStackMulti_ShowOnAnUninitializedFlowReportsToTheReceiptAsync()
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var stack = CreateChildRect("Screen").gameObject.AddComponent<UguiPageStackMulti>();
            var receipt = new PageCallReceipt();
            System.InvalidOperationException thrown = null;

            try
            {
                await stack.ShowPageAsync(FIRST_KEY, new PageContext { Receipt = receipt }, CancellationToken.None);
            }
            catch (System.InvalidOperationException ex)
            {
                thrown = ex;
            }

            AssertReportedException(receipt, thrown);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task PageStack_ShowWithAnInvalidKeyReportsToTheReceiptAsync(bool multi)
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var receipt = new PageCallReceipt();
            var context = new PageContext { Receipt = receipt };
            System.ArgumentNullException thrown = null;

            try
            {
                if (multi)
                {
                    var stack = CreateChildRect("Screen").gameObject.AddComponent<UguiPageStackMulti>();
                    await stack.ShowPageAsync(assetKey: default, context: context, token: CancellationToken.None);
                }
                else
                {
                    var stack = CreateChildRect("Screen").gameObject.AddComponent<UguiPageStackSingle>();
                    await stack.ShowPageAsync(assetKey: default, context: context, token: CancellationToken.None);
                }
            }
            catch (System.ArgumentNullException ex)
            {
                thrown = ex;
            }

            AssertReportedException(receipt, thrown);
        }

        [Test]
        public void Initialize_SecondCallThrowsAndKeepsThePoolRoot()
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var parent = CreateChildRect("Parent");
            var flow = CreateChildRect("Flow").gameObject.AddComponent<ProbeFlow>();
            flow.Initialize(parent: parent, context: CreateFlowContext(), identifier: null);
            var poolTransform = flow.PoolTransform;
            System.InvalidOperationException thrown = null;

            try
            {
                flow.Initialize(parent: parent, context: CreateFlowContext(), identifier: null);
            }
            catch (System.InvalidOperationException ex)
            {
                thrown = ex;
            }

            Assert.IsNotNull(thrown);
            Assert.AreSame(expected: poolTransform, actual: flow.PoolTransform);
            Assert.AreEqual(expected: 1, actual: parent.childCount);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Initialize_WithParentPlacesThePoolRootUnderTheParent()
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var parent = CreateChildRect("Parent");
            var flow = CreateChildRect("Flow").gameObject.AddComponent<ProbeFlow>();

            flow.Initialize(parent: parent, context: CreateFlowContext(), identifier: null);

            Assert.AreSame(expected: parent, actual: flow.PoolTransform.parent);
            Assert.AreNotSame(expected: flow.RectTransform, actual: flow.PoolTransform.parent);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Initialize_WithoutParentPlacesThePoolRootUnderTheFlowRect()
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            var flow = CreateChildRect("Flow").gameObject.AddComponent<ProbeFlow>();

            flow.Initialize(parent: default, context: CreateFlowContext(), identifier: null);

            Assert.AreSame(expected: flow.RectTransform, actual: flow.PoolTransform.parent);
            LogAssert.NoUnexpectedReceived();
        }

        private static float GetVisibility(Transform transform)
        {
            var visibility = 1f;
            var current = transform;

            while (current.IsValid())
            {
                if (current.TryGetComponent<CanvasGroup>(out var group))
                {
                    visibility *= group.alpha;

                    if (group.ignoreParentGroups)
                    {
                        break;
                    }
                }

                current = current.parent;
            }

            return visibility;
        }

        private async Task<Success<PageHistoryError>> GoBackWithThrowingPageAsync()
        {
            try
            {
                return await _history.GoBackAsync(CancellationToken.None);
            }
            finally
            {
                ThrowingPage.Failure = null;
            }
        }

        private static void AssertReplayFailed(Success<PageHistoryError> result, System.Exception thrown)
        {
            Assert.IsTrue(result.TryGetFailure(out var historyError));
            Assert.AreEqual(PageHistoryError.EnumCase.ReplayFailed, historyError.GetEnumCase());
            Assert.IsTrue(historyError.TryGetValue(out PageHistoryError.ReplayFailed failed));
            Assert.IsTrue(failed.Error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(thrown, wrapped.Ex);
        }

        private static void AssertReportedException(PageCallReceipt receipt, System.Exception thrown)
        {
            Assert.IsNotNull(thrown);
            Assert.IsTrue(receipt.IsReported);
            Assert.IsTrue(receipt.Result.TryGetFailure(out var error));
            Assert.IsTrue(error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
            Assert.AreSame(thrown, wrapped.Ex);
        }

        private ProbeFlow CreateFlow()
            => CreateFlow<ProbeFlow>("Probe");

        private T CreateFlow<T>(string name)
            where T : UguiPageFlow
        {
            _root = new GameObject(nameof(UguiPageFlowResultTests), typeof(RectTransform));

            return UguiPageFlow.Create<T>(name, _root.GetComponent<RectTransform>(), CreateFlowContext());
        }

        private static UguiPageFlowContext CreateFlowContext()
            => new() {
                loadStrategy = new(value: PageLoaderStrategy.Resources, isOverridden: true),
            };

        private RectTransform CreateChildRect(string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            return (RectTransform)child.transform;
        }

        private void AddPageSource<T>(UguiPageFlow flow, string key)
            where T : MonoBehaviour, IUguiPage
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

            var source = new GameObject(key, typeof(RectTransform));
            source.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            source.AddComponent<T>();
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

        private ProbePage CreatePage()
        {
            var gameObject = new GameObject(nameof(ProbePage));
            gameObject.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            return gameObject.AddComponent<ProbePage>();
        }

        private sealed class ProbeFlow : UguiPageFlow
        {
            public Success<PageFlowError> Return(IUguiPage page, PageContext context)
                => ReturnPageToPool(page, context);

            public Success<PageFlowError> Return(UguiPageIdentifier identifier, PageContext context)
                => ReturnPageToPool(identifier, context);

            protected override void OnInitialize(in InitializationContext context)
            {
            }

            protected override void OnDispose()
            {
            }
        }

        private sealed class ProbePage : MonoBehaviour, IUguiPage { }

        [RequireComponent(typeof(CanvasGroup))]
        [RequireComponent(typeof(UguiPageTransitionCanvasGroup))]
        private sealed class SlowCreatePage : UguiPageBehaviour, IPageOnCreateAsync
        {
            public static SlowCreatePage Instance { get; private set; }

            public static Transform ParentOnCreateStart { get; private set; }

            public static float VisibilityOnCreateStart { get; private set; } = -1f;

            public static float VisibilityAfterNextFrame { get; private set; } = -1f;

            public static bool Accept { get; set; } = true;

            public static CancellationTokenSource CancelAfterNextFrame { get; set; }

            public static PageAbortMode? AbortInCreate { get; set; }

            public static Success<PageAbortError> AbortRequest { get; private set; }

            public static void Reset()
            {
                Instance = null;
                ParentOnCreateStart = null;
                VisibilityOnCreateStart = -1f;
                VisibilityAfterNextFrame = -1f;
                Accept = true;
                CancelAfterNextFrame = null;
                AbortInCreate = null;
                AbortRequest = default;
            }

            public async UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
            {
                Instance = this;
                ParentOnCreateStart = transform.parent;
                VisibilityOnCreateStart = GetVisibility(transform);

                await UnityTask.NextFrameAsync(token);

                VisibilityAfterNextFrame = GetVisibility(transform);

                if (AbortInCreate.HasValue)
                {
                    AbortRequest = context.Abort.Request(AbortInCreate.Value);
                }

                CancelAfterNextFrame?.Cancel();
                return Accept;
            }
        }

        private sealed class ThrowingPage : MonoBehaviour, IUguiPage, IPageOnReturnToPool
        {
            public static System.Exception Failure { get; set; }

            public void OnReturnToPool(PageContext context)
            {
                if (Failure != null)
                {
                    throw Failure;
                }
            }
        }

        private sealed class PlainPage : IUguiPage
        {
            public override string ToString()
                => "plain";
        }
    }
}

#endif
