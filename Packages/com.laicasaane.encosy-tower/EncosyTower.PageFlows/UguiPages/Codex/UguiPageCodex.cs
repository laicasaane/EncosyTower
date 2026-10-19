#if UNITY_UGUI

using System;
using System.Collections.Generic;
using System.Threading;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages.Codex
{
    /// <summary>Creates the page flows of a screen from flow definitions, and gives each one a scope.</summary>
    /// <remarks>
    /// <para>
    /// Put the component on a GameObject with a component that implements
    /// <see cref="IUguiPageCodexOnInitialize"/>, such as a <see cref="UguiPageCodexInitializer{TScopes}"/>. Each flow
    /// definition of the codex creates one flow under the codex, and its identifier must match a scope property of the
    /// scope collection. Check Auto Initialize On Awake of the flow context, or call <see cref="Initialize"/> or
    /// <see cref="InitializeAsync"/>.
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
    /// // On the GameObject of the UguiPageCodex, with flow definitions named Screen and Popup.
    /// public sealed class GamePageCodex : UguiPageCodexInitializer&lt;GamePageFlowScopes&gt;
    /// {
    ///     protected override UnityTask OnInitializeAsync(UguiPageCodex codex, GamePageFlowScopes scopes)
    ///         => UnityTask.CompletedTask;
    /// }
    /// </code>
    /// </example>
    [RequireComponent(typeof(RectTransform))]
    public class UguiPageCodex : MonoBehaviour
    {
        [SerializeField] internal FlowDefinition[] _flows;
        [SerializeField] internal UguiPageFlowContext _flowContext = new();

        private ArrayMap<string, PageFlowScope> _flowScopeMap = new();
        private List<UguiPageFlow> _pageFlows = new();
        private IPageFlowScopeCollectionApplier _flowScopeCollectionApplier;
        private CancellationTokenSource _initializationCts;
        private CancellationToken _initializationToken;

        /// <summary>Gets the context that the codex gives to the flows it creates.</summary>
        public UguiPageFlowContext FlowContext => _flowContext;

        /// <summary>
        /// Cancelled when this codex is destroyed or initialized again; pass it to the work an initializer awaits.
        /// </summary>
        public CancellationToken InitializationToken => _initializationToken;

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
              UguiPageFlowContext flowContext = null
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
              UguiPageFlowContext flowContext = null
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
              UguiPageFlowContext flowContext
            , Func<MessagePublisher> getPublisherFunc
            , Func<MessageSubscriber> getSubscriberFunc
            , Func<Processor> getProcessorFunc
            , out IUguiPageCodexOnInitialize initializer
        )
        {
            initializer = null;
            flowContext ??= _flowContext;
            flowContext.Owner = this;

            if (flowContext.IsInitialized == false)
            {
                var settings = UguiPageFlowSettings.Instance;
                var subscriber = getSubscriberFunc?.Invoke();
                var publisher = getPublisherFunc?.Invoke();
                var processor = getProcessorFunc?.Invoke();

                flowContext.Initialize(subscriber, publisher, processor, settings);
            }

            var foundInitializer = GetComponent<IUguiPageCodexOnInitialize>();

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

            var definitions = _flows.AsSpan();
            var validation = PageFlowCodexValidator.Validate(GetDefinitionIdentifiers(definitions), identifiers.Span);

            if (validation.IsValid == false)
            {
                return PageFlowCodexError.InvalidDefinitions(validation);
            }

            // Clones made by CreatePageFlows copy the applier.
            var previousApplier = flowContext.FlowScopeCollectionApplier;
            flowContext.FlowScopeCollectionApplier = Option.Some(flowScopeCollectionApplier);

            var flowScopeMap = new ArrayMap<string, PageFlowScope>();
            var pageFlows = new List<UguiPageFlow>(definitions.Length);
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
                    Destroy(pageFlows[i].gameObject);
                }

                flowContext.FlowScopeCollectionApplier = previousApplier;
                return result;
            }

            // Commit: the old flows go, like the UI Toolkit codex.
            var oldFlows = _pageFlows;
            var oldCount = oldFlows.Count;

            for (var i = 0; i < oldCount; i++)
            {
                var oldFlow = oldFlows[i];

                if (oldFlow.IsValid())
                {
                    Destroy(oldFlow.gameObject);
                }
            }

            _flowContext = flowContext;
            _flowScopeMap = flowScopeMap;
            _pageFlows = pageFlows;
            _flowScopeCollectionApplier = flowScopeCollectionApplier;

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

        /// <summary>The number of flows this codex created.</summary>
        public int FlowCount => _pageFlows.Count;

        /// <summary>Gets a flow by the index of its definition.</summary>
        /// <param name="index">The index of the flow definition.</param>
        /// <returns>The flow, or none when the index is out of range.</returns>
        public Option<UguiPageFlow> GetFlowAt(int index)
        {
            return (uint)index < (uint)_pageFlows.Count ? _pageFlows[index] : Option.None;
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

        private Success<PageFlowCodexError> CreatePageFlows(
              UguiPageFlowContext originalContext
            , ReadOnlySpan<FlowDefinition> definitions
            , ArrayMap<string, PageFlowScope> flowScopeMap
            , List<UguiPageFlow> pageFlows
        )
        {
            var parent = GetComponent<RectTransform>();
            var length = definitions.Length;
            var layer = gameObject.layer;

            flowScopeMap.EnsureCapacity(length);

            for (var i = 0; i < length; i++)
            {
                var definition = definitions[i];
                var identifier = definition.identifier;
                var context = originalContext.CloneWithoutOwner();
                context.autoInitializeOnAwake = false;

                if (flowScopeMap.ContainsKey(identifier))
                {
                    return PageFlowCodexError.FlowCreationFailed(i, identifier);
                }

                Option<UguiPageFlow> flowOpt = definition.kind switch {
                    PageFlowKind.PageStackSingle
                        => UguiPageFlow.Create<UguiPageStackSingle>(identifier, parent, context),
                    PageFlowKind.PageStackMulti
                        => UguiPageFlow.Create<UguiPageStackMulti>(identifier, parent, context),
                    PageFlowKind.PageListSingle
                        => UguiPageFlow.Create<UguiPageListSingle>(identifier, parent, context),
                    PageFlowKind.PageListMulti
                        => UguiPageFlow.Create<UguiPageListMulti>(identifier, parent, context),
                    _ => Option.None,
                };

                if (flowOpt.TryGetValue(out var flow) == false)
                {
                    return PageFlowCodexError.FlowCreationFailed(i, identifier);
                }

                var scope = flow.Context.FlowScope;

                if (flowScopeMap.TryAdd(identifier, scope) == false)
                {
                    Destroy(flow.gameObject);
                    return PageFlowCodexError.FlowCreationFailed(i, identifier);
                }

                pageFlows.Add(flow);

                if (definition.overrideSortingLayer == false)
                {
                    continue;
                }

                var canvas = flow.Canvas;
                canvas.gameObject.layer = layer;
                canvas.overrideSorting = true;
                canvas.sortingLayerID = definition.sortingLayer;
                canvas.sortingOrder = definition.sortingOrderInLayer;
            }

            return Success.Yes;
        }

        private void Awake()
        {
            if (_flowContext.autoInitializeOnAwake)
            {
                // The synchronous steps of InitializeAsync complete before Awake returns.
                AutoInitializeAsync().Forget();
            }
        }

        private void OnDestroy()
        {
            CancelInitialization();
            _flowScopeCollectionApplier = null;
            _flowScopeMap.Clear();
            _pageFlows.Clear();
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

        [Serializable]
        internal struct FlowDefinition
        {
            public string identifier;
            public PageFlowKind kind;
            public bool overrideSortingLayer;
            public SerializableSortingLayer sortingLayer;
            public int sortingOrderInLayer;
        }
    }
}

#endif
