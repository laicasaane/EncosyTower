#if UNITY_UGUI

using System.Threading;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages.Transitions
{
    /// <summary>A base component for a transition that animates part of a uGUI page.</summary>
    /// <remarks>A <see cref="UguiPageTransitionCollection"/> on the page runs every transition under it.</remarks>
    public abstract class UguiPageTransition : MonoBehaviour, IPageTransition
    {
        /// <summary>A name for finding this transition. The flow does not read it.</summary>
        [Tooltip("A name for finding this transition. The flow does not read it.")]
        public string identifier;

        /// <inheritdoc/>
        public abstract bool ForceRunHide { get; }

        /// <inheritdoc/>
        public abstract bool ForceRunShow { get; }

        /// <inheritdoc/>
        public abstract UnityTask OnBeforeTransitionAsync(PageTransitionContext context, CancellationToken token);

        /// <inheritdoc/>
        public abstract void OnAfterTransition(PageTransitionContext context);

        /// <inheritdoc/>
        public abstract UnityTask OnShowAsync(PageTransitionContext context, CancellationToken token);

        /// <inheritdoc/>
        public abstract UnityTask OnHideAsync(PageTransitionContext context, CancellationToken token);
    }
}

#endif
