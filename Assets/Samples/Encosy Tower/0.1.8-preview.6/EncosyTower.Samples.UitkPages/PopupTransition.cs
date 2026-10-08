using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages
{
    internal sealed class PopupTransition : IPageTransition
    {
        private const string BACKDROP = "backdrop";
        private const string PANEL = "root";

        private readonly UitkValueAnimationTransition _backdrop;
        private readonly UitkValueAnimationTransition _panel;
        private readonly UnityTask[] _tasks = new UnityTask[2];

        private bool _animatePanel;

        public PopupTransition(VisualElement slot)
        {
            _backdrop = new UitkValueAnimationTransition(slot.Q(BACKDROP));
            _panel = new UitkValueAnimationTransition(slot.Q(PANEL));
        }

        public bool ForceRunHide => true;

        public bool ForceRunShow => true;

        public UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        )
        {
            var options = transition == PageTransition.Show ? showOptions : hideOptions;
            _animatePanel = options.Contains(PageTransitionOptions.NoTransition) == false;

            _tasks[0] = _backdrop.OnBeforeTransitionAsync(transition, showOptions, hideOptions, token);

            _tasks[1] = _panel.OnBeforeTransitionAsync(
                  transition
                , showOptions & ~PageTransitionOptions.ZeroDuration
                , hideOptions & ~PageTransitionOptions.ZeroDuration
                , token
            );

            return UnityTask.WhenAll(_tasks);
        }

        public void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        )
        {
            _backdrop.OnAfterTransition(transition, showOptions, hideOptions);

            if (_animatePanel)
            {
                _panel.OnAfterTransition(transition, showOptions, hideOptions);
            }
        }

        public UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token)
        {
            _tasks[0] = _backdrop.OnShowAsync(options, token);

            _tasks[1] = _animatePanel
                ? _panel.OnShowAsync(options & ~PageTransitionOptions.ZeroDuration, token)
                : UnityTask.CompletedTask;

            return UnityTask.WhenAll(_tasks);
        }

        public UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token)
        {
            _tasks[0] = _backdrop.OnHideAsync(options, token);

            _tasks[1] = _animatePanel
                ? _panel.OnHideAsync(options & ~PageTransitionOptions.ZeroDuration, token)
                : UnityTask.CompletedTask;

            return UnityTask.WhenAll(_tasks);
        }
    }
}
