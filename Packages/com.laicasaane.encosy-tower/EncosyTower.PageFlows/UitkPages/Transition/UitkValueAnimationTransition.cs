using System;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace EncosyTower.PageFlows.UitkPages
{
    public enum UitkValueAnimationPreset
    {
        Fade,
        SlideFromBottom,
        Custom,
    }

    public sealed class UitkValueAnimationTransition : IPageTransition
    {
        public const int DEFAULT_DURATION_MS = 200;
        public const float SLIDE_DISTANCE = 40f;

        private readonly VisualElement _slot;

        private ValueAnimation<float> _animation;
        private UnityTaskCompletionSource _completion;
        private CancellationTokenRegistration _registration;
        private bool _show;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;

        public UitkValueAnimationTransition(
              VisualElement slot
            , UitkValueAnimationPreset preset = UitkValueAnimationPreset.Fade
            , Action<VisualElement, float> apply = null
        )
        {
            _slot = slot;
            Preset = preset;
            Apply = apply;
        }

        public UitkValueAnimationPreset Preset { get; set; }

        public Action<VisualElement, float> Apply { get; set; }

        public int ShowDurationMs { get; set; } = DEFAULT_DURATION_MS;

        public int HideDurationMs { get; set; } = DEFAULT_DURATION_MS;

        public bool ForceRunShow { get; set; }

        public bool ForceRunHide { get; set; }

        public bool IsRunning => _completion != null;

        public static void ApplyFade(VisualElement element, float value)
        {
            element.style.opacity = value;
        }

        public static void ApplySlideFromBottom(VisualElement element, float value)
        {
            element.style.opacity = value;
            element.style.translate = new Translate(x: 0f, y: (1f - value) * SLIDE_DISTANCE);
        }

        public UnityTask OnBeforeTransitionAsync(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
            , CancellationToken token
        )
        {
            _zeroShowDuration = showOptions.Contains(PageTransitionOptions.ZeroDuration);
            _zeroHideDuration = hideOptions.Contains(PageTransitionOptions.ZeroDuration);
            return UnityTask.CompletedTask;
        }

        public void OnAfterTransition(
              PageTransition transition
            , PageTransitionOptions showOptions
            , PageTransitionOptions hideOptions
        )
        {
            Finish(transition == PageTransition.Show);
        }

        public UnityTask OnShowAsync(PageTransitionOptions options, CancellationToken token)
            => Run(
                  show: true
                , zeroDuration: _zeroShowDuration || options.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        public UnityTask OnHideAsync(PageTransitionOptions options, CancellationToken token)
            => Run(
                  show: false
                , zeroDuration: _zeroHideDuration || options.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        private UnityTask Run(bool show, bool zeroDuration, CancellationToken token)
        {
            if (IsRunning)
            {
                Finish(_show);
            }

            var durationMs = show ? ShowDurationMs : HideDurationMs;

            if (zeroDuration || durationMs <= 0 || _slot.panel == null || token.IsCancellationRequested)
            {
                ApplyValue(show ? 1f : 0f);
                return UnityTask.CompletedTask;
            }

            _show = show;
            _completion = new UnityTaskCompletionSource();

            if (token.CanBeCanceled)
            {
                _registration = token.Register(OnCanceled);
            }

            var from = show ? 0f : 1f;
            var to = show ? 1f : 0f;

            ApplyValue(from);
            _animation = _slot.experimental.animation.Start(from, to, durationMs, OnAnimate);
            _animation.KeepAlive().OnCompleted(OnAnimationCompleted);
            return _completion.Task;
        }

        private void OnAnimate(VisualElement element, float value)
        {
            ApplyValue(value);
        }

        private void OnAnimationCompleted()
        {
            if (IsRunning)
            {
                ApplyValue(_show ? 1f : 0f);
                Complete();
            }
        }

        private void OnCanceled()
        {
            if (IsRunning)
            {
                Finish(_show);
            }
        }

        private void Finish(bool show)
        {
            if (_animation != null)
            {
                var animation = _animation;
                _animation = null;

                if (animation.isRunning)
                {
                    animation.Stop();
                }
            }

            ApplyValue(show ? 1f : 0f);

            if (IsRunning)
            {
                Complete();
            }
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
            switch (Preset)
            {
                case UitkValueAnimationPreset.SlideFromBottom:
                {
                    ApplySlideFromBottom(_slot, value);
                    break;
                }

                case UitkValueAnimationPreset.Custom:
                {
                    Apply?.Invoke(_slot, value);
                    break;
                }

                default:
                {
                    ApplyFade(_slot, value);
                    break;
                }
            }
        }
    }
}
