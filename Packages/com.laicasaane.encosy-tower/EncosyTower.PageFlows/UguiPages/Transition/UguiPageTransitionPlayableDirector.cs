#if UNITY_UGUI

using System;
using System.Threading;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.Playables;

namespace EncosyTower.PageFlows.UguiPages.Transitions
{
    /// <summary>Plays a timeline asset for the show or the hide.</summary>
    /// <remarks>
    /// With <see cref="PageTransitionOptions.ZeroDuration"/>, the director jumps to the end of the asset unless
    /// <c>disableZeroDuration</c> of the options is on. A step with no asset does nothing.
    /// </remarks>
    public class UguiPageTransitionPlayableDirector : UguiPageTransition
    {
        /// <summary>The director that plays the assets. The transition adds one when it is missing.</summary>
        [Tooltip("The director that plays the assets. The transition adds one when it is missing.")]
        public PlayableDirector director;

        /// <summary>The options of the show.</summary>
        [Tooltip("The options of the show.")]
        public Options showOperationOptions;

        /// <summary>The options of the hide.</summary>
        [Tooltip("The options of the hide.")]
        public Options hideOperationOptions;

        private bool _isTransitioning;
        private bool _zeroShowDuration;
        private bool _zeroHideDuration;

        /// <summary>Gets the director that plays the assets.</summary>
        public PlayableDirector Director
        {
            get
            {
                if (director.IsInvalid())
                {
                    director = this.GetOrAddComponent<PlayableDirector>();
                }

                return director;
            }
        }

        /// <inheritdoc/>
        public override bool ForceRunHide => hideOperationOptions.forceRun;

        /// <inheritdoc/>
        public override bool ForceRunShow => showOperationOptions.forceRun;

        /// <inheritdoc/>
        public override UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            var director = Director;
            director.stopped -= OnStopped;
            director.stopped += OnStopped;

            _isTransitioning = false;

            _zeroShowDuration = context.ShowOptions.Contains(PageTransitionOptions.ZeroDuration)
                && showOperationOptions.disableZeroDuration == false;

            _zeroHideDuration = context.HideOptions.Contains(PageTransitionOptions.ZeroDuration)
                && hideOperationOptions.disableZeroDuration == false;

            return UnityTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override void OnAfterTransition(PageTransitionContext context)
        {
            var director = Director;
            director.stopped -= OnStopped;
        }

        /// <inheritdoc/>
        public override UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
        {
            return showOperationOptions.playableAsset.IsInvalid()
                ? UnityTask.CompletedTask
                : Transition(showOperationOptions, _zeroShowDuration, token);
        }

        /// <inheritdoc/>
        public override UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
        {
            return hideOperationOptions.playableAsset.IsInvalid()
                ? UnityTask.CompletedTask
                : Transition(hideOperationOptions, _zeroHideDuration, token);
        }

        private async UnityTask Transition(Options options, bool zeroDuration, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var director = Director;
            director.Play(options.playableAsset, DirectorWrapMode.None);

            if (zeroDuration == false)
            {
                _isTransitioning = true;

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
                director.time = options.playableAsset.duration;
            }
        }

        private void OnStopped(PlayableDirector _)
        {
            _isTransitioning = false;
        }

        /// <summary>The options of the show or of the hide.</summary>
        [Serializable]
        public struct Options
        {
            /// <summary>Whether the step runs even with <c>NoTransition</c>.</summary>
            [Tooltip("Whether the step runs even with NoTransition.")]
            public bool forceRun;

            /// <summary>Whether the step plays in full with <c>ZeroDuration</c>.</summary>
            [Tooltip("Whether the step plays in full with ZeroDuration.")]
            public bool disableZeroDuration;

            /// <summary>The asset to play.</summary>
            [Tooltip("The asset to play.")]
            public PlayableAsset playableAsset;
        }
    }
}

#endif
