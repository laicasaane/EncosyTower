#if UNITY_UGUI

using System.Runtime.CompilerServices;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UguiPages.Transitions;
using EncosyTower.PubSub;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    /// <summary>A base component for a uGUI page.</summary>
    /// <remarks>
    /// <para>
    /// It reads <see cref="UguiPageOptions"/> and a <see cref="Transitions.UguiPageTransitionCollection"/> from its
    /// GameObject, and adds them when they are missing. The flow sets <see cref="FlowScope"/>,
    /// <see cref="Subscriber"/> and <see cref="Publisher"/> when the page attaches, and clears them when it detaches.
    /// </para>
    /// <para>
    /// For a page that fades as one unit, put a <see cref="Transitions.UguiPageTransitionCanvasGroup"/> on its
    /// root GameObject.
    /// </para>
    /// </remarks>
    public abstract class UguiPageBehaviour : MonoBehaviour
        , IUguiPage
        , IPageHasOptions
        , IPageHasTransition
        , IPageNeedsFlowScope
        , IPageNeedsMessageSubscriber
        , IPageNeedsMessagePublisher
    {
        private UguiPageOptions _options;
        private UguiPageTransitionCollection _transition;
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private PageFlowScope _flowScope;

        /// <inheritdoc/>
        /// <remarks>It comes from the <see cref="UguiPageOptions"/> component of the page.</remarks>
        public PageOptions PageOptions
        {
            get
            {
                if (_options.IsInvalid())
                {
                    _options = this.GetOrAddComponent<UguiPageOptions>();
                }

                return _options.PageOptions;
            }
        }

        /// <inheritdoc/>
        /// <remarks>It is the <see cref="Transitions.UguiPageTransitionCollection"/> component of the page.</remarks>
        public IPageTransition PageTransition
        {
            get
            {
                if (_transition.IsInvalid())
                {
                    _transition = this.GetOrAddComponent<UguiPageTransitionCollection>();
                }

                return _transition;
            }
        }

        /// <summary>Gets the scope of the flow the page is attached to.</summary>
        public PageFlowScope FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flowScope;
        }

        /// <summary>Gets the subscriber of the flow the page is attached to.</summary>
        public MessageSubscriber Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscriber;
        }

        /// <summary>Gets the publisher of the flow the page is attached to.</summary>
        public MessagePublisher Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _publisher;
        }

        PageFlowScope IPageNeedsFlowScope.FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _flowScope = value;
        }

        MessageSubscriber IPageNeedsMessageSubscriber.Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _subscriber = value;
        }

        MessagePublisher IPageNeedsMessagePublisher.Publisher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _publisher = value;
        }
    }
}

#endif
