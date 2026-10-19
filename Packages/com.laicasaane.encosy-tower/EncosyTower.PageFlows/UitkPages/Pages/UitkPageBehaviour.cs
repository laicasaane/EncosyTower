using System.Runtime.CompilerServices;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    /// <summary>A component for a UI Toolkit page defined by a VisualTreeAsset.</summary>
    /// <remarks>
    /// <para>
    /// Put it on the root of a prefab that is the page asset. The flow instantiates <see cref="VisualTreeAsset"/>
    /// as the element of the page and sets <see cref="Slot"/>. The flow sets <see cref="FlowScope"/>,
    /// <see cref="Subscriber"/> and <see cref="Publisher"/> when the page attaches, and clears them when it
    /// detaches.
    /// </para>
    /// <para>
    /// The default transition changes the opacity of the page root; see
    /// <see cref="CreateTransition"/>.
    /// </para>
    /// </remarks>
    public class UitkPageBehaviour : MonoBehaviour
        , IUitkPageHasFocus
        , IPageHasOptions
        , IPageHasTransition
        , IPageNeedsFlowScope
        , IPageNeedsMessageSubscriber
        , IPageNeedsMessagePublisher
        , IUitkPageSlotReceiver
    {
        /// <summary>The layout the flow instantiates as the page.</summary>
        [Tooltip("The layout the flow instantiates as the page.")]
        [SerializeField] internal VisualTreeAsset _visualTreeAsset;

        /// <summary>The options a flow reads when it shows or hides this page.</summary>
        [Tooltip("The options a flow reads when it shows or hides this page.")]
        [SerializeField] internal PageOptions _pageOptions;

        /// <summary>How the flow moves focus after the page shows.</summary>
        [Tooltip("How the flow moves focus after the page shows.")]
        [SerializeField] internal PageFocusMode _focusMode;

        /// <summary>The name of the element that takes focus with the Named focus mode.</summary>
        [Tooltip("The name of the element that takes focus with the Named focus mode.")]
        [SerializeField] internal string _focusElementName;

        /// <summary>The USS classes and options of the default transition.</summary>
        [Tooltip("The USS classes and options of the default transition.")]
        [SerializeField] internal UitkUssClassTransitionSettings _transition;

        private TemplateContainer _slot;
        private IPageTransition _pageTransition;
        private MessageSubscriber _subscriber;
        private MessagePublisher _publisher;
        private PageFlowScope _flowScope;

        /// <summary>Gets the layout the flow instantiates as the page.</summary>
        public VisualTreeAsset VisualTreeAsset => _visualTreeAsset;

        /// <inheritdoc/>
        /// <remarks>It is <see cref="Slot"/>; <c>null</c> before the flow creates the page.</remarks>
        public VisualElement PageRoot => _slot;

        /// <summary>Gets the element the flow created from the layout, or <c>null</c> before it exists.</summary>
        public TemplateContainer Slot => _slot;

        /// <inheritdoc/>
        public PageOptions PageOptions => _pageOptions;

        /// <inheritdoc/>
        public PageFocusMode FocusMode => _focusMode;

        /// <inheritdoc/>
        public string FocusElementName => _focusElementName;

        /// <inheritdoc/>
        /// <remarks>It is created from <see cref="CreateTransition"/> on first use.</remarks>
        public IPageTransition PageTransition => _pageTransition ??= CreateTransition(_slot);

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
        /// A <see cref="Transitions.UitkUssClassTransition"/> configured by the serialized transition settings.
        /// </returns>
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
