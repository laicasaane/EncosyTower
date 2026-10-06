#if UNITY_UGUI

using System.Runtime.CompilerServices;
using EncosyTower.PubSub;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
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

        public PageFlowScope FlowScope
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _flowScope;
        }

        public MessageSubscriber Subscriber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subscriber;
        }

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
