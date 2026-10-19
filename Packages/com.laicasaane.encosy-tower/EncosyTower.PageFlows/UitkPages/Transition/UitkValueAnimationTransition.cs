using System;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace EncosyTower.PageFlows.UitkPages.Transitions
{
    /// <summary>Selects what a <see cref="UitkValueAnimationTransition"/> animates.</summary>
    public enum UitkValueAnimationPreset
    {
        /// <summary>Fades the opacity of the element.</summary>
        Fade,

        /// <summary>Fades the opacity and slides the element up from below.</summary>
        SlideFromBottom,

        /// <summary>Calls <see cref="UitkValueAnimationTransition.Apply"/> with the progress.</summary>
        Custom,
    }

    /// <summary>
    /// Animates a page element with a value animation that goes from 0 to 1 on a show and from 1 to 0 on a hide.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Preset"/> says how the value changes the element. <see cref="PageTransitionOptions.ZeroDuration"/>
    /// applies the end value at once. After an abort, the animation continues from the current value and takes the
    /// part of the duration that is left.
    /// </para>
    /// </remarks>
    public sealed class UitkValueAnimationTransition : IPageTransition
    {
        /// <summary>The default duration of a show or hide, in milliseconds.</summary>
        public const int DEFAULT_DURATION_MS = 200;

        /// <summary>The distance of the slide of <see cref="UitkValueAnimationPreset.SlideFromBottom"/>.</summary>
        public const float SLIDE_DISTANCE = 40f;

        private readonly VisualElement _slot;

        private ValueAnimation<float> _animation;
        private UnityTaskCompletionSource _completion;
        private CancellationTokenRegistration _registration;
        private PageAbortHandle _abort;
        private bool _show;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;
        private float _currentVisibilityProgress;

        /// <summary>Creates a transition for an element.</summary>
        /// <param name="slot">The element to animate.</param>
        /// <param name="preset">What the animation changes.</param>
        /// <param name="apply">Applies the value with <see cref="UitkValueAnimationPreset.Custom"/>.</param>
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

        /// <summary>Gets or sets what the animation changes.</summary>
        public UitkValueAnimationPreset Preset { get; set; }

        /// <summary>
        /// Gets or sets the function that applies the value with <see cref="UitkValueAnimationPreset.Custom"/>.
        /// </summary>
        public Action<VisualElement, float> Apply { get; set; }

        /// <summary>Gets or sets the duration of the show, in milliseconds.</summary>
        public int ShowDurationMs { get; set; } = DEFAULT_DURATION_MS;

        /// <summary>Gets or sets the duration of the hide, in milliseconds.</summary>
        public int HideDurationMs { get; set; } = DEFAULT_DURATION_MS;

        /// <inheritdoc/>
        public bool ForceRunShow { get; set; }

        /// <inheritdoc/>
        public bool ForceRunHide { get; set; }

        /// <summary>Gets whether an animation is running.</summary>
        public bool IsRunning => _completion != null;

        /// <summary>Sets the opacity of an element to a value.</summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The progress from 0 to 1.</param>
        public static void ApplyFade(VisualElement element, float value)
        {
            element.style.opacity = value;
        }

        /// <summary>Sets the opacity of an element to a value and moves it down by the part that is left.</summary>
        /// <param name="element">The element.</param>
        /// <param name="value">The progress from 0 to 1.</param>
        public static void ApplySlideFromBottom(VisualElement element, float value)
        {
            element.style.opacity = value;
            element.style.translate = new Translate(x: 0f, y: (1f - value) * SLIDE_DISTANCE);
        }

        /// <inheritdoc/>
        public UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            _zeroShowDuration = context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration);
            _zeroHideDuration = context.HideOptions.Contains(PageTransitionOptions.ZeroDuration);
            return UnityTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void OnAfterTransition(PageTransitionContext context)
        {
            var show = context.Transition == PageTransition.Show;
            var options = show ? context.ShowOptions : context.HideOptions;
            var forceRun = show ? ForceRunShow : ForceRunHide;

            if (options.Contains(PageTransitionOptions.NoTransition) && forceRun == false)
            {
                return;
            }

            Finish(show);
        }

        /// <inheritdoc/>
        public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
            => Run(
                  context
                , show: true
                , zeroDuration: _zeroShowDuration || context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        /// <inheritdoc/>
        public UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
            => Run(
                  context
                , show: false
                , zeroDuration: _zeroHideDuration || context.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
                , token: token
            );

        private UnityTask Run(PageTransitionContext context, bool show, bool zeroDuration, CancellationToken token)
        {
            _abort = context.Abort;

            if (IsRunning)
            {
                Finish(_show);
            }

            var durationMs = show ? ShowDurationMs : HideDurationMs;
            var from = show ? 0f : 1f;
            var to = show ? 1f : 0f;

            if (context.Abort.IsRequested)
            {
                from = _currentVisibilityProgress;
                durationMs = (int)MathF.Round(durationMs * MathF.Abs(to - from));
            }

            if (zeroDuration || durationMs <= 0 || _slot.panel == null || token.IsCancellationRequested)
            {
                ApplyValue(to);
                return UnityTask.CompletedTask;
            }

            _show = show;
            _completion = new UnityTaskCompletionSource();

            if (token.CanBeCanceled)
            {
                _registration = token.Register(OnCanceled);
            }

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
            if (IsRunning == false)
            {
                return;
            }

            if (_abort.IsRequested)
            {
                var animation = _animation;
                _animation = null;

                Complete();

                if (animation != null && animation.isRunning)
                {
                    animation.Stop();
                }

                return;
            }

            Finish(_show);
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
            _currentVisibilityProgress = value;

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
