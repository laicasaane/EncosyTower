using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.PageFlows.UitkPages.Internals;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages.Codex
{
    /// <summary>Creates the page flows of a screen from flow definitions, and gives each one a scope.</summary>
    /// <remarks>
    /// <para>
    /// Put the component on a GameObject with a component that implements
    /// <see cref="IUitkPageCodexOnInitialize"/>, such as a <see cref="UitkPageCodexInitializer{TScopes}"/>. Each flow
    /// definition of the codex creates one flow, and its identifier must match a scope property of the scope
    /// collection. Check Auto Initialize On Awake of the flow context, or call <see cref="Initialize"/> or
    /// <see cref="InitializeAsync"/>.
    /// </para>
    /// <para>
    /// The codex draws on a UIDocument or, from Unity 6000.5, a PanelRenderer on its GameObject, and adds it when
    /// missing. It puts a root element, <see cref="CodexRoot"/>, under the panel root. A flow goes into the
    /// element of the panel that its container name names, or under <see cref="CodexRoot"/> when the name is empty
    /// or not found.
    /// </para>
    /// <para>
    /// All page flow operations and callbacks run on the Unity main thread. A failure to initialize comes back as a
    /// <see cref="PageFlowCodexError"/>; initializing again replaces the flows.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [PageFlowScopeCollection]
    /// public partial struct GamePageFlowScopes
    /// {
    ///     public PageFlowScope Screen { get; private set; }
    ///
    ///     public PageFlowScope Popup { get; private set; }
    /// }
    ///
    /// // On the GameObject of the UitkPageCodex, with flow definitions named Screen and Popup.
    /// public sealed class GameUitkCodex : UitkPageCodexInitializer&lt;GamePageFlowScopes&gt;
    /// {
    ///     protected override UnityTask OnInitializeAsync(UitkPageCodex codex, GamePageFlowScopes scopes)
    ///         => UnityTask.CompletedTask;
    /// }
    /// </code>
    /// </example>
    [DisallowMultipleComponent]
    public class UitkPageCodex : MonoBehaviour
    {
        /// <summary>The name of the <see cref="CodexRoot"/> element.</summary>
        public const string CODEX_ROOT_NAME = "uitk-page-codex";

        /// <summary>The USS class of the <see cref="CodexRoot"/> element.</summary>
        public const string CODEX_ROOT_USS_CLASS_NAME = "uitk-page-codex";

        /// <summary>The name of the hidden GameObject that parents the page behaviours the flows create.</summary>
        public const string BEHAVIOUR_PARENT_NAME = "[UitkPages]";

        [SerializeField] internal FlowDefinition[] _flows;
        [SerializeField] internal UitkPageFlowContext _flowContext = new();
        [SerializeField] internal PanelSettings _panelSettings;
        [SerializeField] internal float _sortingOrder;
        [SerializeField] internal VisualTreeAsset _layoutAsset;

        private ArrayMap<string, PageFlowScope> _flowScopeMap = new();
        private List<UitkPageFlow> _pageFlows = new();

        private IUitkPanelComponentAdapter _adapter;
        private VisualElement _codexRoot;
        private VisualElement _panelRoot;
        private Transform _behaviourParent;
        private IPageFlowScopeCollectionApplier _flowScopeCollectionApplier;
        private CancellationTokenSource _initializationCts;
        private CancellationToken _initializationToken;
        private int _rootVersion;

        /// <summary>Gets the context that the codex gives to the flows it creates.</summary>
        public UitkPageFlowContext FlowContext => _flowContext;

        /// <summary>
        /// Cancelled when this codex is destroyed or initialized again; pass it to the work an initializer awaits.
        /// </summary>
        public CancellationToken InitializationToken => _initializationToken;

        /// <summary>Gets the root element of the codex.</summary>
        /// <remarks>
        /// It fills its parent and ignores pointer input. The codex creates it before it initializes.
        /// </remarks>
        public VisualElement CodexRoot => _codexRoot;

        /// <summary>
        /// Gets the root element of the panel that the codex draws on, or <c>null</c> until the panel has one.
        /// </summary>
        public VisualElement PanelRoot => _panelRoot;

        /// <summary>Gets the number that changes whenever the panel gives the codex a new root element.</summary>
        public int RootVersion => _rootVersion;

        /// <summary>Gets the display name of the panel component: UI Document or Panel Renderer.</summary>
        public string PanelComponentName => _adapter?.DisplayName ?? string.Empty;

        /// <summary>Gets the number of flows this codex created.</summary>
        public int FlowCount => _pageFlows.Count;

        /// <summary>Creates the flows and starts the initializer without waiting for it.</summary>
        /// <param name="flowContext">The context to use. <c>null</c> uses <see cref="FlowContext"/>.</param>
        /// <param name="getPublisherFunc">Supplies the publisher. <c>null</c> uses the global publisher.</param>
        /// <param name="getSubscriberFunc">Supplies the subscriber. <c>null</c> uses the global subscriber.</param>
        /// <param name="getProcessorFunc">Supplies the processor. <c>null</c> uses the global processor.</param>
        /// <returns>
        /// Success when the codex created its flows; otherwise a failure; see <see cref="PageFlowCodexError"/>.
        /// </returns>
        /// <remarks>
        /// The suppliers are used only when the context is not initialized yet. The initializer runs in the
        /// background; an exception it throws is not returned. Use <see cref="InitializeAsync"/> to wait for it.
        /// </remarks>
        public Success<PageFlowCodexError> Initialize(
              UitkPageFlowContext flowContext = null
            , Func<MessagePublisher> getPublisherFunc = null
            , Func<MessageSubscriber> getSubscriberFunc = null
            , Func<Processor> getProcessorFunc = null
        )
        {
            var result = InitializeFlows(
                  flowContext
                , getPublisherFunc
                , getSubscriberFunc
                , getProcessorFunc
                , out var initializer
            );

            if (result.IsSuccess)
            {
                initializer.OnInitializeAsync(this).Forget();
            }

            return result;
        }

        /// <summary>Creates the flows and waits for the initializer.</summary>
        /// <param name="flowContext">The context to use. <c>null</c> uses <see cref="FlowContext"/>.</param>
        /// <param name="getPublisherFunc">Supplies the publisher. <c>null</c> uses the global publisher.</param>
        /// <param name="getSubscriberFunc">Supplies the subscriber. <c>null</c> uses the global subscriber.</param>
        /// <param name="getProcessorFunc">Supplies the processor. <c>null</c> uses the global processor.</param>
        /// <returns>
        /// Success when the codex created its flows and the initializer ended; otherwise a failure; see
        /// <see cref="PageFlowCodexError"/>.
        /// </returns>
        /// <remarks>
        /// An exception the initializer throws comes back as the <c>InitializerFailed</c> case. When the codex is
        /// destroyed or initialized again meanwhile, the call throws the
        /// <see cref="System.OperationCanceledException"/>.
        /// </remarks>
        public async UnityTask<Success<PageFlowCodexError>> InitializeAsync(
              UitkPageFlowContext flowContext = null
            , Func<MessagePublisher> getPublisherFunc = null
            , Func<MessageSubscriber> getSubscriberFunc = null
            , Func<Processor> getProcessorFunc = null
        )
        {
            var result = InitializeFlows(
                  flowContext
                , getPublisherFunc
                , getSubscriberFunc
                , getProcessorFunc
                , out var initializer
            );

            if (result.IsFailure)
            {
                return result;
            }

            var token = _initializationToken;

            try
            {
                await initializer.OnInitializeAsync(this);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return PageFlowCodexError.InitializerFailed(ex);
            }

            // A superseded or destroyed run reports cancellation even when its initializer ignored the token.
            token.ThrowIfCancellationRequested();
            return Success.Yes;
        }

        private Success<PageFlowCodexError> InitializeFlows(
              UitkPageFlowContext flowContext
            , Func<MessagePublisher> getPublisherFunc
            , Func<MessageSubscriber> getSubscriberFunc
            , Func<Processor> getProcessorFunc
            , out IUitkPageCodexOnInitialize initializer
        )
        {
            EnsureCreated();

            initializer = null;
            flowContext ??= _flowContext;
            flowContext.Owner = this;

            if (flowContext.IsInitialized == false)
            {
                var settings = UitkPageFlowSettings.Instance;
                var subscriber = getSubscriberFunc?.Invoke();
                var publisher = getPublisherFunc?.Invoke();
                var processor = getProcessorFunc?.Invoke();

                flowContext.Initialize(subscriber, publisher, processor, settings);
            }

            var foundInitializer = GetComponent<IUitkPageCodexOnInitialize>();

            if (foundInitializer is not Component initializerComponent || initializerComponent.IsInvalid())
            {
                return PageFlowCodexError.InitializerMissing();
            }

            var flowScopeCollectionApplier = foundInitializer.PageFlowScopeCollectionApplier;

            if (flowScopeCollectionApplier == null)
            {
                return PageFlowCodexError.ScopeCollectionApplierMissing();
            }

            var flowScopeCollectionType = flowScopeCollectionApplier.CollectionType;

            if (flowScopeCollectionType == null)
            {
                return PageFlowCodexError.ScopeCollectionTypeMissing();
            }

            var identifiers = flowScopeCollectionApplier.ScopeIdentifiers;

            if (identifiers.Length < 1)
            {
                return PageFlowCodexError.NoScopeProperties(flowScopeCollectionType);
            }

            var definitions = (_flows ?? Array.Empty<FlowDefinition>()).AsSpan();
            var validation = PageFlowCodexValidator.Validate(GetDefinitionIdentifiers(definitions), identifiers.Span);

            if (validation.IsValid == false)
            {
                return PageFlowCodexError.InvalidDefinitions(validation);
            }

            // Clones made by CreatePageFlows copy the applier.
            var previousApplier = flowContext.FlowScopeCollectionApplier;
            flowContext.FlowScopeCollectionApplier = Option.Some(flowScopeCollectionApplier);

            var flowScopeMap = new ArrayMap<string, PageFlowScope>();
            var pageFlows = new List<UitkPageFlow>(definitions.Length);
            var result = CreatePageFlows(flowContext, definitions, flowScopeMap, pageFlows);

            if (result.IsSuccess)
            {
                result = flowScopeCollectionApplier.Build(flowScopeMap);
            }

            if (result.IsFailure)
            {
                var createdCount = pageFlows.Count;

                for (var i = 0; i < createdCount; i++)
                {
                    pageFlows[i].Dispose();
                }

                flowContext.FlowScopeCollectionApplier = previousApplier;
                return result;
            }

            // Commit: the D10 re-initialization fix, now applied only after success.
            var oldFlows = _pageFlows;
            var oldCount = oldFlows.Count;

            for (var i = 0; i < oldCount; i++)
            {
                oldFlows[i].Dispose();
            }

            _flowContext = flowContext;
            _flowScopeMap = flowScopeMap;
            _pageFlows = pageFlows;
            _flowScopeCollectionApplier = flowScopeCollectionApplier;

            PlaceFlows();

            CancelInitialization();
            _initializationCts = new CancellationTokenSource();
            _initializationToken = _initializationCts.Token;

            initializer = foundInitializer;
            return Success.Yes;
        }

        /// <summary>Gets the scope of a flow by the identifier of its definition.</summary>
        /// <param name="identifier">The identifier of the flow definition.</param>
        /// <param name="result">The scope of the flow.</param>
        /// <returns><c>true</c> when the codex created a flow with that identifier; otherwise <c>false</c>.</returns>
        public bool TryGetFlowScope(string identifier, out PageFlowScope result)
            => _flowScopeMap.TryGetValue(identifier, out result);

        /// <summary>Gets a flow by the index of its definition.</summary>
        /// <param name="index">The index of the flow definition.</param>
        /// <returns>The flow, or none when the index is out of range.</returns>
        public Option<UitkPageFlow> GetFlowAt(int index)
            => (uint)index < (uint)_pageFlows.Count ? _pageFlows[index] : Option.None;

        /// <summary>Gets a flow by the identifier of its definition.</summary>
        /// <param name="identifier">The identifier of the flow definition.</param>
        /// <returns>The flow, or none when the codex created no flow with that identifier.</returns>
        public Option<UitkPageFlow> GetFlow(string identifier)
        {
            var count = _pageFlows.Count;

            for (var i = 0; i < count; i++)
            {
                var flow = _pageFlows[i];

                if (string.Equals(flow.Identifier, identifier, StringComparison.Ordinal))
                {
                    return flow;
                }
            }

            return Option.None;
        }

        /// <summary>Creates the codex root and initializes the codex when <c>autoInitializeOnAwake</c> is on.</summary>
        protected void Awake()
        {
            EnsureCreated();

            if (_flowContext.autoInitializeOnAwake)
            {
                // The synchronous steps of InitializeAsync complete before Awake returns.
                AutoInitializeAsync().Forget();
            }
        }

        /// <summary>Attaches the codex to its panel component.</summary>
        protected void OnEnable()
        {
            EnsureCreated();
            _adapter.Attach(OnRoot);
        }

        /// <summary>Detaches the codex from its panel component.</summary>
        protected void OnDisable()
        {
            _adapter?.Detach();
        }

        /// <summary>Lets the panel component report a new root element.</summary>
        protected void LateUpdate()
        {
            _adapter?.Poll();
        }

        /// <summary>Cancels the initialization, disposes the flows and removes the codex root from the panel.</summary>
        protected void OnDestroy()
        {
            CancelInitialization();
            _adapter?.Detach();

            var count = _pageFlows.Count;

            for (var i = 0; i < count; i++)
            {
                _pageFlows[i].Dispose();
            }

            _pageFlows.Clear();
            _flowScopeMap.Clear();
            _flowScopeCollectionApplier = null;
            _codexRoot?.RemoveFromHierarchy();
        }

        private static string[] GetDefinitionIdentifiers(ReadOnlySpan<FlowDefinition> definitions)
        {
            var identifiers = new string[definitions.Length];

            for (var i = 0; i < definitions.Length; i++)
            {
                identifiers[i] = definitions[i].identifier;
            }

            return identifiers;
        }

        private void EnsureCreated()
        {
            if (_codexRoot == null)
            {
                _codexRoot = new VisualElement {
                    name = CODEX_ROOT_NAME,
                    pickingMode = PickingMode.Ignore,
                };

                _codexRoot.AddToClassList(CODEX_ROOT_USS_CLASS_NAME);
                UitkPageFlow.FillParent(_codexRoot.style);
                _codexRoot.RegisterCallback<DetachFromPanelEvent>(OnCodexRootDetached);
            }

            if (_behaviourParent.IsInvalid())
            {
                var parent = new GameObject(BEHAVIOUR_PARENT_NAME) {
                    hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave,
                };

                _behaviourParent = parent.transform;
                _behaviourParent.SetParent(transform, false);
            }

            _adapter ??= CreateAdapter();
        }

        private IUitkPanelComponentAdapter CreateAdapter()
        {
#if UNITY_6000_5_OR_NEWER
            if (TryGetComponent<PanelRenderer>(out var existingRenderer))
            {
                return new PanelRendererAdapter(existingRenderer);
            }
#endif

            if (TryGetComponent<UIDocument>(out var existingDocument))
            {
                return new UIDocumentAdapter(existingDocument);
            }

#if UNITY_6000_5_OR_NEWER
            if (UitkPageFlowSettings.Instance.forceUIDocument == false)
            {
                var renderer = gameObject.AddComponent<PanelRenderer>();
                renderer.hideFlags = HideFlags.DontSave;
                renderer.panelSettings = _panelSettings;
                renderer.sortingOrder = (int)_sortingOrder;
                renderer.visualTreeAsset = _layoutAsset;
                return new PanelRendererAdapter(renderer);
            }
#endif

            var document = gameObject.AddComponent<UIDocument>();
            document.hideFlags = HideFlags.DontSave;
            document.panelSettings = _panelSettings;
            document.sortingOrder = _sortingOrder;
            document.visualTreeAsset = _layoutAsset;
            return new UIDocumentAdapter(document);
        }

        private void OnRoot(VisualElement root, int version)
        {
            if (root == null)
            {
                return;
            }

            if (_codexRoot.parent != root)
            {
                root.Add(_codexRoot);
            }

            _panelRoot = root;
            _rootVersion = version;
            PlaceFlows();
        }

        private void OnCodexRootDetached(DetachFromPanelEvent evt)
        {
            _adapter?.NotifyRootLost();
        }

        private void PlaceFlows()
        {
            if (_panelRoot == null)
            {
                AddFlowsTo(_codexRoot);
                return;
            }

            var count = _pageFlows.Count;

            for (var i = 0; i < count; i++)
            {
                var flow = _pageFlows[i];
                var container = ResolveContainer(flow);

                if (flow.FlowElement.parent != container)
                {
                    container.Add(flow.FlowElement);
                }
            }
        }

        private void AddFlowsTo(VisualElement container)
        {
            var count = _pageFlows.Count;

            for (var i = 0; i < count; i++)
            {
                var element = _pageFlows[i].FlowElement;

                if (element.parent == null)
                {
                    container.Add(element);
                }
            }
        }

        private VisualElement ResolveContainer(UitkPageFlow flow)
        {
            var containerName = flow.ContainerName;

            if (string.IsNullOrEmpty(containerName))
            {
                return _codexRoot;
            }

            var container = _panelRoot.Q(containerName);

            if (container != null)
            {
                return container;
            }

            ErrorIfContainerNotFound(flow.Identifier, containerName, this);
            return _codexRoot;
        }

        private Success<PageFlowCodexError> CreatePageFlows(
              UitkPageFlowContext originalContext
            , ReadOnlySpan<FlowDefinition> definitions
            , ArrayMap<string, PageFlowScope> flowScopeMap
            , List<UitkPageFlow> pageFlows
        )
        {
            var length = definitions.Length;

            flowScopeMap.EnsureCapacity(length);

            for (var i = 0; i < length; i++)
            {
                var definition = definitions[i];
                var identifier = definition.identifier;

                if (flowScopeMap.ContainsKey(identifier))
                {
                    return PageFlowCodexError.FlowCreationFailed(i, identifier);
                }

                if (UitkPageFlow.TryCreate(definition.kind, out var flow) == false)
                {
                    return PageFlowCodexError.FlowCreationFailed(i, identifier);
                }

                var context = originalContext.CloneWithoutOwner();
                context.autoInitializeOnAwake = false;
                flow.Initialize(identifier, definition.containerName, this, context, _behaviourParent);

                if (flowScopeMap.TryAdd(identifier, flow.Context.FlowScope) == false)
                {
                    flow.Dispose();
                    return PageFlowCodexError.FlowCreationFailed(i, identifier);
                }

                pageFlows.Add(flow);
            }

            return Success.Yes;
        }

        private async UnityTask AutoInitializeAsync()
        {
            var result = await InitializeAsync(_flowContext);

            if (result.TryGetFailure(out var error))
            {
                error.Log(_flowContext.Logger);
            }
        }

        private void CancelInitialization()
        {
            if (_initializationCts == null)
            {
                return;
            }

            // _initializationToken keeps the cancelled token.
            _initializationCts.Cancel();
            _initializationCts.Dispose();
            _initializationCts = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfContainerNotFound(string identifier, string containerName, UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"Cannot find the container '{containerName}' of the flow '{identifier}' in the panel root. " +
                $"The flow is placed under the codex root instead."
            );
        }

        [Serializable]
        internal struct FlowDefinition
        {
            public string identifier;
            public PageFlowKind kind;
            public string containerName;
        }
    }
}
