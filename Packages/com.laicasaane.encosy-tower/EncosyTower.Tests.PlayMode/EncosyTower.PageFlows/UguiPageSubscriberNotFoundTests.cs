#if UNITY_UGUI

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
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageSubscriberNotFoundTests
    {
        private const string PAGE_KEY = "probe-page";

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root.IsValid())
            {
                Object.Destroy(_root);
            }
        }

        [Test]
        public async Task PageStackSingle_ShowWithWarningOff_SucceedsWithoutANavigatedSubscriberAsync()
        {
            var stack = CreateStack(warnNoSubscriber: false);

            var result = await stack.ShowPageAsync(PAGE_KEY, CreateContext(), CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowWithWarningOn_ReturnsSubscriberNotFoundAndKeepsThePageShownAsync()
        {
            var stack = CreateStack(warnNoSubscriber: true);

            var result = await stack.ShowPageAsync(PAGE_KEY, CreateContext(), CancellationToken.None);

            Assert.IsTrue(result.IsFailure);
            Assert.IsTrue(result.IsApplied());
            Assert.IsTrue(result.GetFailureOrDefault().IsSubscriberNotFound);
            Assert.IsTrue(Mentions(result, nameof(PageNavigatedMessage)));
            Assert.IsTrue(((IPageFlowRecordable)stack).IsShowingPages);
            LogAssert.NoUnexpectedReceived();
        }

        private static PageContext CreateContext()
            => new() {
                ShowOptions = PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.ZeroDuration,
            };

        private static bool Mentions(in Success<PageFlowError> result, string messageTypeName)
        {
            var error = result.GetFailureOrDefault();

            if (error.ToMessage().Contains(messageTypeName))
            {
                return true;
            }

            var suppressed = error.SuppressedErrors;
            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                if (suppressed[i].ToMessage().Contains(messageTypeName))
                {
                    return true;
                }
            }

            return false;
        }

        private UguiPageStackSingle CreateStack(bool warnNoSubscriber)
        {
            _root = new GameObject(nameof(UguiPageSubscriberNotFoundTests), typeof(RectTransform));

            var context = new UguiPageFlowContext {
                loadStrategy = new(value: PageLoaderStrategy.Resources, isOverridden: true),
                warnNoSubscriber = new(value: warnNoSubscriber, isOverridden: true),
            };

            var stack = UguiPageFlow.Create<UguiPageStackSingle>(
                  "Screen"
                , _root.GetComponent<RectTransform>()
                , context
            );

            AddPageSource(stack, PAGE_KEY);
            return stack;
        }

        private void AddPageSource(UguiPageFlow flow, string key)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

            var source = new GameObject(key, typeof(RectTransform));
            source.transform.SetParent(parent: _root.transform, worldPositionStays: false);
            source.AddComponent<ProbePage>();
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

        private sealed class ProbePage : MonoBehaviour, IUguiPage { }
    }
}

#endif
