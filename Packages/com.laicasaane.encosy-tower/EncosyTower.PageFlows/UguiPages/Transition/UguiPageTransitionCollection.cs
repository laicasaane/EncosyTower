#if UNITY_UGUI

using System;
using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages.Transitions
{
    /// <summary>Runs the transitions on a page together, as the transition of the page.</summary>
    /// <remarks>
    /// It gathers the <see cref="UguiPageTransition"/> components of its GameObject, and of its children when
    /// <see cref="ChildrenOptions.include"/> is on, in <c>Awake</c>. A step ends when every transition ended.
    /// <see cref="PageTransitionOptions.NoTransition"/> skips a transition unless that transition forces the run.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class UguiPageTransitionCollection : MonoBehaviour, IPageTransition
    {
        /// <summary>Whether the show step runs even with <c>NoTransition</c>.</summary>
        [Tooltip("Whether the show step runs even with NoTransition.")]
        public bool alwaysEnableEnter = true;

        /// <summary>Whether the hide step runs even with <c>NoTransition</c>.</summary>
        [Tooltip("Whether the hide step runs even with NoTransition.")]
        public bool alwaysEnableExit = true;

        /// <summary>Which child transitions the collection gathers.</summary>
        [Tooltip("Which child transitions the collection gathers.")]
        public ChildrenOptions childrenOptions = ChildrenOptions.Default;

        private UguiPageTransition[] _transitions;
        private UnityTask[] _tasks;

        /// <inheritdoc/>
        /// <remarks>It is <see cref="alwaysEnableExit"/>.</remarks>
        public bool ForceRunHide => alwaysEnableExit;

        /// <inheritdoc/>
        /// <remarks>It is <see cref="alwaysEnableEnter"/>.</remarks>
        public bool ForceRunShow => alwaysEnableEnter;

        private void Awake()
        {
            if (childrenOptions.include)
            {
                _transitions = GetComponentsInChildren<UguiPageTransition>(childrenOptions.includeInactive);
            }
            else
            {
                _transitions = GetComponents<UguiPageTransition>();
            }

            _tasks = new UnityTask[_transitions.Length];
        }

        /// <inheritdoc/>
        public UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token)
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;
            var tasks = _tasks.AsSpan();

            for (var i = 0; i < length; i++)
            {
                tasks[i] = transitions[i].OnBeforeTransitionAsync(context, token);
            }

            return UnityTask.WhenAll(_tasks);
        }

        /// <inheritdoc/>
        public void OnAfterTransition(PageTransitionContext context)
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;

            for (var i = 0; i < length; i++)
            {
                transitions[i].OnAfterTransition(context);
            }
        }

        /// <inheritdoc/>
        public UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token)
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;
            var tasks = _tasks.AsSpan();
            var withShow = context.ShowOptions.Contains(PageTransitionOptions.NoTransition) == false;

            for (var i = 0; i < length; i++)
            {
                var transition = transitions[i];

                tasks[i] = withShow || transition.ForceRunShow
                    ? transition.OnShowAsync(context, token)
                    : UnityTask.CompletedTask;
            }

            return UnityTask.WhenAll(_tasks);
        }

        /// <inheritdoc/>
        public UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token)
        {
            var transitions = _transitions.AsSpan();
            var length = transitions.Length;
            var tasks = _tasks.AsSpan();
            var withHide = context.HideOptions.Contains(PageTransitionOptions.NoTransition) == false;

            for (var i = 0; i < length; i++)
            {
                var transition = transitions[i];

                tasks[i] = withHide || transition.ForceRunHide
                    ? transition.OnHideAsync(context, token)
                    : UnityTask.CompletedTask;
            }

            return UnityTask.WhenAll(_tasks);
        }

        /// <summary>Which child transitions a collection gathers.</summary>
        [Serializable]
        public struct ChildrenOptions
        {
            /// <summary>Whether the collection gathers the transitions of its children.</summary>
            [Tooltip("Whether the collection gathers the transitions of its children.")]
            public bool include;

            /// <summary>Whether the collection also gathers the transitions of inactive children.</summary>
            [Tooltip("Whether the collection also gathers the transitions of inactive children.")]
            public bool includeInactive;

            /// <summary>Gets the default options: children are included, inactive ones are not.</summary>
            public static ChildrenOptions Default => new() {
                include = true,
            };
        }
    }
}

#endif
