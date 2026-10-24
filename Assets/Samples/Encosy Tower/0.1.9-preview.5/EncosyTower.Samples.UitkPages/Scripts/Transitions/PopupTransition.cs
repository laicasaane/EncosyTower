using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Transitions
{
    /// <summary>The transition of a popup: its backdrop and its panel fade together, as two animations.</summary>
    /// <remarks>
    /// Demonstrates a custom <see cref="IPageTransition"/> built from two <see cref="UitkValueAnimationTransition"/>
    /// values.
    /// </remarks>
    internal sealed class PopupTransition : IPageTransition
    {
        private const string BACKDROP = "backdrop";
        private const string PANEL = "root";

        private readonly UitkValueAnimationTransition _backdrop;
        private readonly UitkValueAnimationTransition _panel;
        private readonly UnityTask[] _tasks = new UnityTask[2];

        public PopupTransition(VisualElement slot, int durationMs)
        {
            _backdrop = new UitkValueAnimationTransition(slot.Q(BACKDROP)) {
                ShowDurationMs = durationMs,
                HideDurationMs = durationMs,
            };

            _panel = new UitkValueAnimationTransition(slot.Q(PANEL)) {
                ShowDurationMs = durationMs,
                HideDurationMs = durationMs,
            };
        }

        public bool ForceRunHide => false;

        public bool ForceRunShow => false;

        public UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            _tasks[0] = _backdrop.OnBeforeTransitionAsync(context, token);
            _tasks[1] = _panel.OnBeforeTransitionAsync(context, token);

            return UnityTask.WhenAll(_tasks);
        }

        public void OnAfterTransition(PageTransitionContext context)
        {
            _backdrop.OnAfterTransition(context);
            _panel.OnAfterTransition(context);
        }

        public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
        {
            _tasks[0] = _backdrop.OnShowAsync(context, token);
            _tasks[1] = _panel.OnShowAsync(context, token);

            return UnityTask.WhenAll(_tasks);
        }

        public UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
        {
            _tasks[0] = _backdrop.OnHideAsync(context, token);
            _tasks[1] = _panel.OnHideAsync(context, token);

            return UnityTask.WhenAll(_tasks);
        }
    }
}
