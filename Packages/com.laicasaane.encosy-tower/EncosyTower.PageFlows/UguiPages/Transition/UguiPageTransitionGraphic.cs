#if UNITY_UGUI

using System;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.PageFlows.UguiPages.Transitions
{
    /// <summary>Fades the alpha of a Graphic.</summary>
    /// <remarks>
    /// The fade runs in <c>Update</c> with the smoothed delta time. <see cref="PageTransitionOptions.ZeroDuration"/>
    /// applies the end alpha at once unless <c>disableZeroDuration</c> of the options is on. After an abort, the fade
    /// starts from the current alpha and takes the part of the duration that is left.
    /// </remarks>
    public class UguiPageTransitionGraphic : UguiPageTransition
    {
        /// <summary>The component to fade. The transition adds an <c>Image</c> when it is missing.</summary>
        [Tooltip("The component to fade. The transition adds an Image when it is missing.")]
        public Graphic graphic;

        /// <summary>Whether the transition skips the fade.</summary>
        [Tooltip("Whether the transition skips the fade.")]
        public bool disableTweening;

        /// <summary>The options of the show.</summary>
        [Tooltip("The options of the show.")]
        public Options showOperationOptions = Options.DefaultShow;

        /// <summary>The options of the hide.</summary>
        [Tooltip("The options of the hide.")]
        public Options hideOperationOptions = Options.DefaultHide;

        private bool _isTransitioning;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;
        private float _elapsedTime;
        private float _duration;
        private TransitionFloat _alpha;

        /// <summary>Gets the component to fade.</summary>
        public Graphic Graphic
        {
            get
            {
                if (graphic.IsInvalid())
                {
                    graphic = this.GetOrAddComponent<Image>();
                }

                return graphic;
            }
        }

        /// <inheritdoc/>
        public override bool ForceRunHide => hideOperationOptions.forceRun;

        /// <inheritdoc/>
        public override bool ForceRunShow => showOperationOptions.forceRun;

        /// <inheritdoc/>
        public override UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            _ = Graphic;
            _isTransitioning = false;
            _elapsedTime = 0f;

            _zeroShowDuration = context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration)
                && showOperationOptions.disableZeroDuration == false;

            _zeroHideDuration = context.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
                && hideOperationOptions.disableZeroDuration == false;

            return UnityTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override void OnAfterTransition(PageTransitionContext context)
        {
        }

        /// <inheritdoc/>
        public override UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
        {
            return disableTweening
                ? UnityTask.CompletedTask
                : Transition(showOperationOptions, _zeroShowDuration, context.Abort, token);
        }

        /// <inheritdoc/>
        public override UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
        {
            return disableTweening
                ? UnityTask.CompletedTask
                : Transition(hideOperationOptions, _zeroHideDuration, context.Abort, token);
        }

        private async UnityTask Transition(
              Options options
            , bool zeroDuration
            , PageAbortHandle abort
            , CancellationToken token
        )
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var graphic = Graphic;
            var start = Mathf.Clamp(options.alpha.start, 0f, 1f);
            var end = Mathf.Clamp(options.alpha.end, 0f, 1f);
            var duration = Mathf.Max(0f, options.duration);

            if (abort.IsRequested)
            {
                var current = graphic.color.a;
                var range = Mathf.Abs(end - start);

                duration = range > 0f ? duration * Mathf.Abs(end - current) / range : 0f;
                start = current;
            }

            _alpha = new(start, end);
            _duration = duration;

            if (_duration > 0f && zeroDuration == false)
            {
                _isTransitioning = true;

                var color = graphic.color;
                color.a = start;
                graphic.color = color;

                try
                {
                    await UnityTask.WaitUntil(this, static state => state._isTransitioning == false, token);
                }
                finally
                {
                    if (token.IsCancellationRequested)
                    {
                        _isTransitioning = false;
                    }
                }
            }
            else
            {
                var color = graphic.color;
                color.a = end;
                graphic.color = color;
            }
        }

        private void Update()
        {
            if (disableTweening || _isTransitioning == false)
            {
                return;
            }

            var elapsedTime = _elapsedTime += Time.smoothDeltaTime;
            var duration = _duration;
            var progress = Mathf.Clamp(elapsedTime / duration, 0f, 1f);
            var alpha = _alpha;
            var color = graphic.color;
            color.a = Mathf.Lerp(alpha.start, alpha.end, progress);

            graphic.color = color;

            if (elapsedTime >= duration)
            {
                _isTransitioning = false;
            }
        }

        /// <summary>The options of the show or of the hide.</summary>
        [Serializable]
        public struct Options
        {
            /// <summary>Whether the step runs even with <c>NoTransition</c>.</summary>
            [Tooltip("Whether the step runs even with NoTransition.")]
            public bool forceRun;

            /// <summary>Whether the step keeps its duration with <c>ZeroDuration</c>.</summary>
            [Tooltip("Whether the step keeps its duration with ZeroDuration.")]
            public bool disableZeroDuration;

            /// <summary>The duration of the fade, in seconds.</summary>
            [Tooltip("The duration of the fade, in seconds.")]
            public float duration;

            /// <summary>The alpha the fade starts and ends with, from 0 to 1.</summary>
            [Tooltip("The alpha the fade starts and ends with, from 0 to 1.")]
            public TransitionFloat alpha;

            /// <summary>Gets the default show options: 0.2 seconds from alpha 0 to 1.</summary>
            public static Options DefaultShow => new() {
                duration = 0.2f,
                alpha = new(0f, 1f),
            };

            /// <summary>Gets the default hide options: 0.2 seconds from alpha 1 to 0.</summary>
            public static Options DefaultHide => new() {
                duration = 0.2f,
                alpha = new(1f, 0f),
            };
        }
    }
}

#endif
