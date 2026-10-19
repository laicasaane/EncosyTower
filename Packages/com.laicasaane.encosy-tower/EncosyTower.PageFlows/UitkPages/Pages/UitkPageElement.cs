using System.Runtime.CompilerServices;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.PubSub;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>A base class for a UI Toolkit page that is a custom element of a VisualTreeAsset.</summary>
    /// <remarks>
    /// The element is its own root. The flow finds it in the instantiated layout, and sets <see cref="Slot"/>, the
    /// element that holds it. The flow sets <see cref="FlowScope"/>, <see cref="Subscriber"/> and
    /// <see cref="Publisher"/> when the page attaches, and clears them when it detaches.
    /// </remarks>
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

        /// <inheritdoc/>
        /// <remarks>It is the element itself.</remarks>
        public VisualElement PageRoot => this;

        /// <summary>Gets the element the flow created from the layout, or <c>null</c> before it exists.</summary>
        public TemplateContainer Slot => _slot;

        /// <inheritdoc/>
        /// <remarks>Override it to set the options. The default is no options.</remarks>
        public virtual PageOptions PageOptions => default;

        /// <inheritdoc/>
        /// <remarks>Override it to move focus. The default is <see cref="PageFocusMode.None"/>.</remarks>
        public virtual PageFocusMode FocusMode => PageFocusMode.None;

        /// <inheritdoc/>
        /// <remarks>The default is <c>null</c>.</remarks>
        public virtual string FocusElementName => null;

        /// <inheritdoc/>
        /// <remarks>It is created from <see cref="CreateTransition"/> on first use.</remarks>
        public IPageTransition PageTransition => _transition ??= CreateTransition(_slot ?? (VisualElement)this);

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

        /// <summary>Creates the transition of the page.</summary>
        /// <param name="slot">The element that the transition animates.</param>
        /// <returns>
        /// A <see cref="Transitions.UitkUssClassTransition"/> that has no classes, so it uses its preset.
        /// </returns>
        protected virtual IPageTransition CreateTransition(VisualElement slot)
            => new UitkUssClassTransition(slot);

        void IUitkPageSlotReceiver.SetSlot(TemplateContainer slot)
        {
            _slot = slot;
            _transition = null;
        }
    }
}
