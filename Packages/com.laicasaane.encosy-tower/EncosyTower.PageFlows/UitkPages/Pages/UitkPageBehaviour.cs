using System.Runtime.CompilerServices;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    public class UitkPageBehaviour : MonoBehaviour
        , IUitkPageHasFocus
        , IPageHasOptions
        , IPageHasTransition
        , IPageNeedsFlowScope
        , IPageNeedsMessageSubscriber
        , IPageNeedsMessagePublisher
        , IUitkPageSlotReceiver
    {
        [SerializeField] internal VisualTreeAsset _visualTreeAsset;
        [SerializeField] internal PageOptions _pageOptions;
        [SerializeField] internal PageFocusMode _focusMode;
        [SerializeField] internal string _focusElementName;
        [SerializeField] internal UitkUssClassTransitionSettings _transition;

        private TemplateContainer _slot;
        private IPageTransition _pageTransition;
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private PageFlowScope _flowScope;

        public VisualTreeAsset VisualTreeAsset => _visualTreeAsset;

        public VisualElement PageRoot => _slot;

        public TemplateContainer Slot => _slot;

        public PageOptions PageOptions => _pageOptions;

        public PageFocusMode FocusMode => _focusMode;

        public string FocusElementName => _focusElementName;

        public IPageTransition PageTransition => _pageTransition ??= CreateTransition(_slot);

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

        protected virtual IPageTransition CreateTransition(VisualElement slot)
        {
            var transition = new UitkUssClassTransition(slot);
            _transition.ApplyTo(transition);
            return transition;
        }

        void IUitkPageSlotReceiver.SetSlot(TemplateContainer slot)
        {
            _slot = slot;
            _pageTransition = null;
        }
    }
}
