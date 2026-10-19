using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages.Transitions;
using EncosyTower.Samples.UguiPages.Dialogs;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Transitions
{
    /// <summary>The transition of the incantation dialog: a cast motion before the cast, a fade after it.</summary>
    /// <remarks>
    /// Demonstrates a custom <see cref="UguiPageTransition"/>. It honors
    /// <see cref="PageTransitionOptions.ZeroDuration"/> and <see cref="PageTransitionOptions.NoTransition"/>, and
    /// continues from the current pose after an abort.
    /// </remarks>
    public class TowerIncantationTransition : UguiPageTransition
    {
        /// <summary>The dialog that this transition moves.</summary>
        [Tooltip("The dialog that this transition moves.")]
        [SerializeField] private IncantationDialog _dialog;

        /// <summary>The canvas group that blocks raycasts while the dialog shows.</summary>
        [Tooltip("The canvas group that blocks raycasts while the dialog shows.")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isRunning;
        private bool _casting;
        private float _from;
        private float _to;
        private float _duration;
        private float _elapsed;
        private float _progress;

        /// <inheritdoc/>
        public override bool ForceRunHide => false;

        /// <inheritdoc/>
        public override bool ForceRunShow => false;

        /// <inheritdoc/>
        public override UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            _isRunning = false;
            _casting = _dialog.IsCommitted == false;

            if (IsBypassed(context) == false)
            {
                _dialog.SetMotionMode(_casting);
            }

            return UnityTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override void OnAfterTransition(PageTransitionContext context)
        {
            _isRunning = false;
            _canvasGroup.blocksRaycasts = context.Transition == PageTransition.Show;

            if (IsBypassed(context))
            {
                return;
            }

            Apply(context.Transition == PageTransition.Show ? 1f : 0f);
        }

        /// <inheritdoc/>
        public override UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
        {
            _canvasGroup.blocksRaycasts = true;

            return RunAsync(
                  show: true
                , zeroDuration: context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration)
                , context.Abort
                , token
            );
        }

        /// <inheritdoc/>
        public override UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
            => RunAsync(
                  show: false
                , zeroDuration: context.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
                , context.Abort
                , token
            );

        private static bool IsBypassed(PageTransitionContext context)
        {
            var options = context.Transition == PageTransition.Show ? context.ShowOptions : context.HideOptions;
            return options.Contains(PageTransitionOptions.NoTransition);
        }

        private async UnityTask RunAsync(bool show, bool zeroDuration, PageAbortHandle abort, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var end = show ? 1f : 0f;
            var full = _casting ? _dialog.CastSeconds : _dialog.FadeSeconds;

            _from = show ? 0f : 1f;
            _to = end;
            _duration = full;

            if (abort.IsRequested)
            {
                _from = _progress;
                _duration = full * Mathf.Abs(_to - _from);
            }

            if (zeroDuration || _duration <= 0f)
            {
                Apply(end);
                return;
            }

            _elapsed = 0f;
            _isRunning = true;
            Apply(_from);

            try
            {
                await UnityTask.WaitUntil(this, static transition => transition._isRunning == false, token);
            }
            finally
            {
                if (token.IsCancellationRequested)
                {
                    _isRunning = false;
                }
            }
        }

        private void Update()
        {
            if (_isRunning == false)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;

            var t = Mathf.Clamp01(_elapsed / _duration);

            Apply(Mathf.Lerp(_from, _to, t));

            if (t >= 1f)
            {
                _isRunning = false;
            }
        }

        private void Apply(float value)
        {
            _progress = value;

            if (_casting)
            {
                _dialog.ApplyCastPose(value);
            }
            else
            {
                _dialog.ApplyOpacity(value);
            }
        }
    }
}
