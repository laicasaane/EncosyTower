using System;
using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.Samples.UitkPages.Dialogs;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Transitions
{
    /// <summary>The transition of the incantation dialog: a cast motion before the cast, a fade after it.</summary>
    /// <remarks>
    /// Demonstrates a custom <see cref="IPageTransition"/>. It honors <see cref="PageTransitionOptions.ZeroDuration"/>
    /// and <see cref="PageTransitionOptions.NoTransition"/>, and continues from the current pose after an abort.
    /// </remarks>
    internal sealed class TowerIncantationTransition : IPageTransition
    {
        private const long TICK_INTERVAL_MS = 1;

        private readonly VisualElement _slot;
        private readonly IncantationDialog _dialog;
        private readonly int _castDurationMs;
        private readonly int _fadeDurationMs;
        private readonly Action _onTick;
        private readonly Action _onCanceled;

        private IVisualElementScheduledItem _tick;
        private UnityTaskCompletionSource _completion;
        private CancellationTokenRegistration _registration;
        private PageAbortHandle _abort;
        private bool _casting;
        private bool _show;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;
        private float _from;
        private float _to;
        private float _durationSeconds;
        private float _startTime;
        private float _progress;

        public TowerIncantationTransition(
              VisualElement slot
            , IncantationDialog dialog
            , int castDurationMs
            , int fadeDurationMs
        )
        {
            _slot = slot;
            _dialog = dialog;
            _castDurationMs = castDurationMs;
            _fadeDurationMs = fadeDurationMs;
            _onTick = OnTick;
            _onCanceled = OnCanceled;
        }

        public bool ForceRunHide => false;

        public bool ForceRunShow => false;

        private bool IsRunning => _completion != null;

        public UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            _casting = _dialog.IsCommitted == false;
            _zeroShowDuration = context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration);
            _zeroHideDuration = context.HideOptions.Contains(PageTransitionOptions.ZeroDuration);

            if (IsBypassed(context) == false)
            {
                _dialog.SetMotionMode(_casting);
            }

            return UnityTask.CompletedTask;
        }

        public void OnAfterTransition(PageTransitionContext context)
        {
            if (IsBypassed(context))
            {
                return;
            }

            Finish(show: context.Transition == PageTransition.Show);
        }

        public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
            => Run(
                  context
                , show: true
                , zeroDuration: _zeroShowDuration || context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        public UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
            => Run(
                  context
                , show: false
                , zeroDuration: _zeroHideDuration || context.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        private static bool IsBypassed(PageTransitionContext context)
        {
            var options = context.Transition == PageTransition.Show ? context.ShowOptions : context.HideOptions;
            return options.Contains(PageTransitionOptions.NoTransition);
        }

        private UnityTask Run(PageTransitionContext context, bool show, bool zeroDuration, CancellationToken token)
        {
            _abort = context.Abort;

            if (IsRunning)
            {
                Finish(_show);
            }

            var fullSeconds = (_casting ? _castDurationMs : _fadeDurationMs) * 0.001f;
            var from = show ? 0f : 1f;
            var to = show ? 1f : 0f;

            if (context.Abort.IsRequested)
            {
                from = _progress;
            }

            var durationSeconds = fullSeconds * Mathf.Abs(to - from);

            if (zeroDuration || durationSeconds <= 0f || _slot.panel == null || token.IsCancellationRequested)
            {
                ApplyValue(to);
                return UnityTask.CompletedTask;
            }

            _show = show;
            _from = from;
            _to = to;
            _durationSeconds = durationSeconds;
            _startTime = Time.unscaledTime;
            _completion = new UnityTaskCompletionSource();

            if (token.CanBeCanceled)
            {
                _registration = token.Register(_onCanceled);
            }

            ApplyValue(from);
            CreateTick();
            _tick.Resume();

            return _completion.Task;
        }

        private void OnTick()
        {
            if (IsRunning == false)
            {
                return;
            }

            var t = _durationSeconds <= 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - _startTime) / _durationSeconds);

            ApplyValue(Mathf.Lerp(_from, _to, t));

            if (t >= 1f)
            {
                StopTick();
                Complete();
            }
        }

        private void OnCanceled()
        {
            if (IsRunning == false)
            {
                return;
            }

            if (_abort.IsRequested)
            {
                StopTick();
                Complete();
                return;
            }

            Finish(_show);
        }

        private void Finish(bool show)
        {
            StopTick();
            ApplyValue(show ? 1f : 0f);

            if (IsRunning)
            {
                Complete();
            }
        }

        private void CreateTick()
        {
            if (_tick != null)
            {
                return;
            }

            _tick = _slot.schedule.Execute(_onTick).Every(TICK_INTERVAL_MS);
            _tick.Pause();
        }

        private void StopTick()
        {
            _tick?.Pause();
        }

        private void Complete()
        {
            _registration.Dispose();
            _registration = default;

            var completion = _completion;

            _completion = null;
            completion?.TrySetResult();
        }

        private void ApplyValue(float value)
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
