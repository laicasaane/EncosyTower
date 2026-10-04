using System.Runtime.CompilerServices;
using EncosyTower.PubSub;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    public abstract class UitkPageElement : VisualElement
        , IUitkPageHasFocus
        , IPageHasOptions
        , IPageHasTransition
        , IPageNeedsFlowScope
        , IPageNeedsMessageSubscriber
        , IPageNeedsMessagePublisher
        , IUitkPageSlotReceiver
    {
        private TemplateContainer _slot;
        private IPageTransition _transition;
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private PageFlowScope _flowScope;

        public VisualElement PageRoot => this;

        public TemplateContainer Slot => _slot;

        public virtual PageOptions PageOptions => default;

        public virtual PageFocusMode FocusMode => PageFocusMode.None;

        public virtual string FocusElementName => null;

        public IPageTransition PageTransition => _transition ??= CreateTransition(_slot ?? (VisualElement)this);

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
            => new UitkUssClassTransition(slot);

        void IUitkPageSlotReceiver.SetSlot(TemplateContainer slot)
        {
            _slot = slot;
            _transition = null;
        }
    }
}
