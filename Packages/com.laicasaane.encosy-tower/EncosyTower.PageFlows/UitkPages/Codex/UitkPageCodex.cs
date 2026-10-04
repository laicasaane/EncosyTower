using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    using UnityObject = UnityEngine.Object;

    [DisallowMultipleComponent]
    public class UitkPageCodex : MonoBehaviour
    {
        public const string CODEX_ROOT_NAME = "uitk-page-codex";
        public const string CODEX_ROOT_USS_CLASS_NAME = "uitk-page-codex";
        public const string BEHAVIOUR_PARENT_NAME = "[UitkPages]";

        [SerializeField] internal FlowDefinition[] _flows;
        [SerializeField] internal UitkPageFlowContext _flowContext = new();
        [SerializeField] internal PanelSettings _panelSettings;
        [SerializeField] internal float _sortingOrder;
        [SerializeField] internal VisualTreeAsset _layoutAsset;

        private readonly ArrayMap<string, PageFlowScope> _flowScopeMap = new();
        private readonly List<UitkPageFlow> _pageFlows = new();

        private IUitkPanelComponentAdapter _adapter;
        private VisualElement _codexRoot;
        private VisualElement _panelRoot;
        private Transform _behaviourParent;
        private IPageFlowScopeCollectionApplier _flowScopeCollectionApplier;
        private int _rootVersion;

        public UitkPageFlowContext FlowContext => _flowContext;

        public VisualElement CodexRoot => _codexRoot;

        public VisualElement PanelRoot => _panelRoot;

        public int RootVersion => _rootVersion;

        public string PanelComponentName => _adapter?.DisplayName ?? string.Empty;

        public int FlowCount => _pageFlows.Count;

        public void Initialize(
              UitkPageFlowContext flowContext = null
            , Func<MessagePublisher> getPublisherFunc = null
            , Func<MessageSubscriber> getSubscriberFunc = null
            , Func<Processor> getProcessorFunc = null
            , Func<ArrayPool<UnityTask>> getTaskArrayPoolFunc = null
        )
        {
            EnsureCreated();

            flowContext ??= _flowContext;
            _flowContext = flowContext;

            flowContext.Owner = this;

            if (flowContext.IsInitialized == false)
            {
                var settings = UitkPageFlowSettings.Instance;
                var subscriber = getSubscriberFunc?.Invoke();
                var publisher = getPublisherFunc?.Invoke();
                var processor = getProcessorFunc?.Invoke();
                var taskArrayPool = getTaskArrayPoolFunc?.Invoke();

                flowContext.Initialize(subscriber, publisher, processor, settings, taskArrayPool);
            }

            var initializer = GetComponent<IUitkPageCodexOnInitialize>();

            if (initializer is not Component initializerContext || initializerContext.IsInvalid())
            {
                ErrorIfCannotInitializeWithoutInitializer(this);
                return;
            }

            var flowScopeCollectionApplier = initializer.PageFlowScopeCollectionApplier;

            if (flowScopeCollectionApplier == null)
            {
                ErrorIfInitializerComponentReturnsNullApplier(this, initializerContext);
                return;
            }

            var flowScopeCollectionType = flowScopeCollectionApplier.CollectionType;

            if (flowScopeCollectionType == null)
            {
                ErrorIfApplierReturnsNullFlowScopeCollectionType(this, initializerContext);
                return;
            }

            var identifiers = flowScopeCollectionApplier.ScopeIdentifiers;

            if (identifiers.Length < 1)
            {
                ErrorIfFlowScopeCollectionTypeHasNoValidProperty(flowScopeCollectionType, this, initializerContext);
                return;
            }

            var definitions = (_flows ?? Array.Empty<FlowDefinition>()).AsSpan();
            var validation = PageFlowCodexValidator.Validate(GetDefinitionIdentifiers(definitions), identifiers.Span);

            if (validation.IsValid == false)
            {
                LogValidationErrors(flowScopeCollectionType, validation);
                return;
            }

            flowContext.FlowScopeCollectionApplier = Option.Some(flowScopeCollectionApplier);

            CreatePageFlows(flowContext, definitions);
            PlaceFlows();

            if (flowScopeCollectionApplier.TryBuild(_flowScopeMap, out var missingIdentifier) == false)
            {
                ErrorIfCannotFindFlowScopeForProperty(flowScopeCollectionType, missingIdentifier, this);
                return;
            }

            _flowScopeCollectionApplier = flowScopeCollectionApplier;
            initializer.OnInitializeAsync(this).Forget();
        }

        public bool TryGetFlowScope(string identifier, out PageFlowScope result)
            => _flowScopeMap.TryGetValue(identifier, out result);

        public Option<UitkPageFlow> GetFlowAt(int index)
            => (uint)index < (uint)_pageFlows.Count ? _pageFlows[index] : Option.None;

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

        protected void Awake()
        {
            EnsureCreated();

            if (_flowContext.autoInitializeOnAwake)
            {
                Initialize(_flowContext);
            }
        }

        protected void OnEnable()
        {
            EnsureCreated();
            _adapter.Attach(OnRoot);
        }

        protected void OnDisable()
        {
            _adapter?.Detach();
        }

        protected void LateUpdate()
        {
            _adapter?.Poll();
        }

        protected void OnDestroy()
        {
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

        private void CreatePageFlows(UitkPageFlowContext originalContext, ReadOnlySpan<FlowDefinition> definitions)
        {
            var length = definitions.Length;

            _flowScopeMap.Clear();
            _flowScopeMap.EnsureCapacity(length);

            for (var i = 0; i < length; i++)
            {
                var definition = definitions[i];
                var identifier = definition.identifier;

                if (_flowScopeMap.ContainsKey(identifier))
                {
                    ErrorIfDuplicateIdentifier(i, identifier, this);
                    continue;
                }

                if (UitkPageFlow.TryCreate(definition.kind, out var flow) == false)
                {
                    ErrorIfUnexpectedErrorWhenCreate(i, this);
                    continue;
                }

                var context = originalContext.CloneWithoutOwner();
                context.autoInitializeOnAwake = false;
                flow.Initialize(identifier, definition.containerName, this, context, _behaviourParent);

                if (_flowScopeMap.TryAdd(identifier, flow.Context.FlowScope) == false)
                {
                    ErrorIfUnexpectedErrorWhenRegister(i, identifier, this);
                    flow.Dispose();
                    continue;
                }

                _pageFlows.Add(flow);
            }
        }

        private void LogValidationErrors(Type type, in PageFlowCodexValidation validation)
        {
            if (validation.FirstEmptyIndex >= 0)
            {
                ErrorIfDefinitionIdentifierIsEmpty(validation.FirstEmptyIndex, this);
            }

            var duplicates = validation.DuplicateIdentifiers;

            for (var i = 0; i < duplicates.Length; i++)
            {
                ErrorIfDuplicateDefinition(duplicates[i], this);
            }

            if (validation.ScopesWithoutDefinition.Length > 0 || validation.DefinitionsWithoutScope.Length > 0)
            {
                ErrorIfFlowIdentifiersMismatch(
                      type
                    , string.Join(", ", validation.ScopesWithoutDefinition)
                    , string.Join(", ", validation.DefinitionsWithoutScope)
                    , this
                );
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotInitializeWithoutInitializer(UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"Cannot initialize {nameof(UitkPageCodex)} without an initializer. " +
                $"Please add another component implements {nameof(IUitkPageCodexOnInitialize)} " +
                $"to the GameObject of this {nameof(UitkPageCodex)}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDefinitionIdentifierIsEmpty(int index, UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"Cannot create a flow for definition at index {index} because its identifier is null or empty."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDuplicateDefinition(string identifier, UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"The identifier '{identifier}' is used by more than one flow definition."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnexpectedErrorWhenCreate(int index, UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"An unexpected error occured when create a flow for definition at index {index}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDuplicateIdentifier(int index, string identifier, UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"The identifier '{identifier}' of definition at index {index} is duplicate thus will be ignored."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnexpectedErrorWhenRegister(int index, string identifier, UitkPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"An unexpected error occured when register the flow '{identifier}' at index {index}."
            );
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfInitializerComponentReturnsNullApplier(UitkPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.LogEnvironment).LogError(
                $"The {nameof(IUitkPageCodexOnInitialize)} component " +
                $"returned a null {nameof(IPageFlowScopeCollectionApplier)}. " +
                $"Please ensure the interface {nameof(IUitkPageCodexOnInitialize)} is correctly implemented."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfApplierReturnsNullFlowScopeCollectionType(UitkPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.LogEnvironment).LogError(
                $"The {nameof(IPageFlowScopeCollectionApplier)} returned a null " +
                $"flow scope collection type. Please ensure the interface " +
                $"{nameof(IPageFlowScopeCollectionApplier)} is correctly implemented."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfFlowScopeCollectionTypeHasNoValidProperty(
              Type type
            , UitkPageCodex codex
            , UnityObject context
        )
        {
            context.GetLogger(codex._flowContext.LogEnvironment).LogError(
                $"Cannot find any valid property to store {nameof(PageFlowScope)} values. " +
                $"Please ensure the type '{type}' is a partial struct with [PageFlowScopeCollection] " +
                $"and has properties whose signature looks like this: " +
                $"`public {nameof(PageFlowScope)} PropertyName {{ get; private set; }}`."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfFlowIdentifiersMismatch(
              Type type
            , string missingIdentifiers
            , string missingProperties
            , UitkPageCodex codex
        )
        {
            if (missingIdentifiers.IsNotEmpty())
            {
                codex.GetLogger(codex._flowContext.LogEnvironment).LogError(
                    $"The flow definitions on this {nameof(UitkPageCodex)} " +
                    $"do not match the properties of type '{type}'. " +
                    $"The following identifiers are missing: {missingIdentifiers}. " +
                    $"Please specify all flow definitions according to the properties of type '{type}'."
                );
            }

            if (missingProperties.IsNotEmpty())
            {
                codex.GetLogger(codex._flowContext.LogEnvironment).LogError(
                    $"The properties of type '{type}' do not match the identifiers " +
                    $"of the flow definitions on this {nameof(UitkPageCodex)}. " +
                    $"The following properties are missing: {missingProperties}. " +
                    $"Please ensure the properties of type '{type}' match all flow definitions."
                );
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotFindFlowScopeForProperty(Type type, string identifier, UitkPageCodex codex)
        {
            codex.GetLogger(codex._flowContext.LogEnvironment).LogError(
                $"Cannot find a flow scope value for the property '{identifier}' of the type '{type}'. " +
                $"Please add the flow definitions to this {nameof(UitkPageCodex)} " +
                $"matching the properties of type '{type}'."
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
