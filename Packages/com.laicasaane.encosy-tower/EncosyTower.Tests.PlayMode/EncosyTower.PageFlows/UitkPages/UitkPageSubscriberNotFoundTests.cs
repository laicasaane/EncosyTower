using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Common;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    using static UitkPagesHarness;

    public sealed class UitkPageSubscriberNotFoundTests
    {
        private const string NO_THEME_WARNING
            = "No Theme Style Sheet set to PanelSettings , UI will not render properly";

        private UitkPagesHarness _harness;

        [TearDown]
        public void TearDown()
        {
            _harness?.Dispose();
            _harness = null;
        }

        [Test]
        public async Task PageStackSingle_ShowWithWarningOff_SucceedsWithoutANavigatedSubscriberAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateStackAsync(warnNoSubscriber: false);

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(result.IsSuccess);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public async Task PageStackSingle_ShowWithWarningOn_ReturnsSubscriberNotFoundAndKeepsThePageShownAsync()
        {
            LogAssert.Expect(LogType.Warning, NO_THEME_WARNING);
            var stack = await CreateStackAsync(warnNoSubscriber: true);

            var result = await stack.ShowPageAsync(ELEMENT_PAGE, ZeroDuration, CancellationToken.None);

            Assert.IsTrue(result.IsFailure);
            Assert.IsTrue(result.IsApplied());
            Assert.IsTrue(result.GetFailureOrDefault().IsSubscriberNotFound);
            Assert.IsTrue(Mentions(result, nameof(PageNavigatedMessage)));
            Assert.IsTrue(stack.CurrentPage.TryGetValue(out var page));
            Assert.IsTrue(IsDisplayed(GetSlot(page)));
            LogAssert.NoUnexpectedReceived();
        }

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

        private async Task<UitkPageStackSingle> CreateStackAsync(bool warnNoSubscriber)
        {
            VisualTreeAsset layoutAsset = null;

            _harness = await CreateAsync(
                  layoutAsset
                , warnNoSubscriber
                , ("Screen", PageFlowKind.PageStackSingle, "")
                , ("Popup", PageFlowKind.PageStackSingle, "")
            );

            return _harness.GetFlow<UitkPageStackSingle>("Screen");
        }
    }
}
