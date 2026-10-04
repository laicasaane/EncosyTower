#if UNITY_UGUI

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages
{
    using UnityObject = UnityEngine.Object;

    [RequireComponent(typeof(RectTransform))]
    public class UguiPageCodex : MonoBehaviour
    {
        [SerializeField] internal FlowDefinition[] _flows;
        [SerializeField] internal UguiPageFlowContext _flowContext = new();

        private readonly ArrayMap<string, PageFlowScope> _flowScopeMap = new();
        private readonly List<UguiPageFlow> _pageFlows = new();
        private IPageFlowScopeCollectionApplier _flowScopeCollectionApplier;

        public UguiPageFlowContext FlowContext => _flowContext;

        public void Initialize(
              UguiPageFlowContext flowContext = null
            , Func<MessagePublisher> getPublisherFunc = null
            , Func<MessageSubscriber> getSubscriberFunc = null
            , Func<Processor> getProcessorFunc = null
            , Func<ArrayPool<UnityTask>> getTaskArrayPoolFunc = null
        )
        {
            flowContext ??= _flowContext;
            _flowContext = flowContext;

            flowContext.Owner = this;

            if (flowContext.IsInitialized == false)
            {
                var settings = UguiPageFlowSettings.Instance;
                var subscriber = getSubscriberFunc?.Invoke();
                var publisher = getPublisherFunc?.Invoke();
                var processor = getProcessorFunc?.Invoke();
                var taskArrayPool = getTaskArrayPoolFunc?.Invoke();

                flowContext.Initialize(subscriber, publisher, processor, settings, taskArrayPool);
            }

            var initializer = GetComponent<IUguiPageCodexOnInitialize>();

            if (initializer is not Component initializerContext || initializerContext.IsInvalid())
            {
                ErrorIfCannotInitializeWithoutInitializer(this);
                return;
            }

            var flowScopeCollectionApplier = initializer.PageFlowScopeCollectionApplier;

            if (flowScopeCollectionApplier is null)
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

            var definitions = _flows.AsSpan();

            var validation = PageFlowCodexValidator.Validate(GetDefinitionIdentifiers(definitions), identifiers.Span);

            if (validation.IsValid == false)
            {
                LogValidationErrors(flowScopeCollectionType, validation, definitions);
                return;
            }

            flowContext.FlowScopeCollectionApplier = Option.Some(flowScopeCollectionApplier);

            CreatePageFlows(flowContext, definitions);

            if (flowScopeCollectionApplier.TryBuild(_flowScopeMap, out var missingIdentifier) == false)
            {
                ErrorIfCannotFindFlowScopeForProperty(flowScopeCollectionType, missingIdentifier, this);
                return;
            }

            _flowScopeCollectionApplier = flowScopeCollectionApplier;
            initializer.OnInitializeAsync(this).Forget();
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

        private void LogValidationErrors(
              Type type
            , in PageFlowCodexValidation validation
            , ReadOnlySpan<FlowDefinition> definitions
        )
        {
            if (validation.FirstEmptyIndex >= 0)
            {
                ErrorIfDefinitionIdentifierIsEmpty(validation.FirstEmptyIndex, this);
            }

            var duplicates = validation.DuplicateIdentifiers;

            for (var i = 0; i < duplicates.Length; i++)
            {
                var identifier = duplicates[i];
                ErrorIfDuplicateIdentifier(FindSecondIndex(definitions, identifier), identifier, this);
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

        private static int FindSecondIndex(ReadOnlySpan<FlowDefinition> definitions, string identifier)
        {
            var found = false;

            for (var i = 0; i < definitions.Length; i++)
            {
                if (string.Equals(definitions[i].identifier, identifier, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                if (found)
                {
                    return i;
                }

                found = true;
            }

            return -1;
        }

        private void CreatePageFlows(UguiPageFlowContext originalContext, ReadOnlySpan<FlowDefinition> definitions)
        {
            var parent = GetComponent<RectTransform>();
            var length = definitions.Length;

            var flowScopeMap = _flowScopeMap;
            flowScopeMap.Clear();
            flowScopeMap.EnsureCapacity(length);

            var pageFlows = _pageFlows.AsListFast();
            pageFlows.Clear();
            pageFlows.IncreaseCapacityTo(length);

            var layer = gameObject.layer;

            for (var i = 0; i < length; i++)
            {
                var definition = definitions[i];
                var identifier = definition.identifier;
                var context = originalContext.CloneWithoutOwner();
                context.autoInitializeOnAwake = false;

                if (flowScopeMap.ContainsKey(identifier))
                {
                    ErrorIfDuplicateIdentifier(i, identifier, this);
                    continue;
                }

                Option<UguiPageFlow> flowOpt = definition.kind switch {
                    PageFlowKind.SinglePageStack
                        => UguiPageFlow.Create<UguiSinglePageStack>(identifier, parent, context),
                    PageFlowKind.MultiPageStack
                        => UguiPageFlow.Create<UguiMultiPageStack>(identifier, parent, context),
                    PageFlowKind.SinglePageList
                        => UguiPageFlow.Create<UguiSinglePageList>(identifier, parent, context),
                    PageFlowKind.MultiPageList
                        => UguiPageFlow.Create<UguiMultiPageList>(identifier, parent, context),
                    _ => Option.None,
                };

                if (flowOpt.TryGetValue(out var flow) == false)
                {
                    ErrorIfUnexpectedErrorWhenCreate(i, this);
                    continue;
                }

                var scope = flow.Context.FlowScope;

                if (flowScopeMap.TryAdd(identifier, scope) == false)
                {
                    ErrorIfUnexpectedErrorWhenRegister(i, identifier, this);
                    Destroy(flow.gameObject);
                    continue;
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
        }

        public bool TryGetFlowScope(string identifier, out PageFlowScope result)
            => _flowScopeMap.TryGetValue(identifier, out result);

        public Option<UguiPageFlow> GetFlowAt(int index)
        {
            return (uint)index < (uint)_pageFlows.Count ? _pageFlows[index] : Option.None;
        }

        private void Awake()
        {
            if (_flowContext.autoInitializeOnAwake)
            {
                Initialize(_flowContext);
            }
        }

        private void OnDestroy()
        {
            _flowScopeCollectionApplier = null;
            _flowScopeMap.Clear();
            _pageFlows.Clear();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotInitializeWithoutInitializer(UguiPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"Cannot initialize {nameof(UguiPageCodex)} without an initializer. " +
                $"Please add another component implements {nameof(IUguiPageCodexOnInitialize)} " +
                $"to the GameObject of this {nameof(UguiPageCodex)}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDefinitionIdentifierIsEmpty(int index, UguiPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"Cannot create a flow for definition at index {index} because its identifier is null or empty."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnexpectedErrorWhenCreate(int index, UguiPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"An unexpected error occured when create a flow for definition at index {index}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfDuplicateIdentifier(int index, string identifier, UguiPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"The identifier '{identifier}' of definition at index {index} is duplicate thus will be ignored."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnexpectedErrorWhenRegister(int index, string identifier, UguiPageCodex context)
        {
            context.GetLogger(context._flowContext.LogEnvironment).LogError(
                $"An unexpected error occured when register the flow '{identifier}' at index {index}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfInitializerComponentReturnsNullApplier(UguiPageCodex codex, UnityObject context)
        {
            context.GetLogger(codex._flowContext.LogEnvironment).LogError(
                $"The {nameof(IUguiPageCodexOnInitialize)} component " +
                $"returned a null {nameof(IPageFlowScopeCollectionApplier)}. " +
                $"Please ensure the interface {nameof(IUguiPageCodexOnInitialize)} is correctly implemented."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfApplierReturnsNullFlowScopeCollectionType(UguiPageCodex codex, UnityObject context)
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
            , UguiPageCodex codex
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
            , UguiPageCodex codex
        )
        {
            if (missingIdentifiers.IsNotEmpty())
            {
                codex.GetLogger(codex._flowContext.LogEnvironment).LogError(
                    $"The flow definitions on this {nameof(UguiPageCodex)} " +
                    $"do not match the properties of type '{type}'. " +
                    $"The following identifiers are missing: {missingIdentifiers}. " +
                    $"Please specify all flow definitions according to the properties of type '{type}'."
                );
            }

            if (missingProperties.IsNotEmpty())
            {
                codex.GetLogger(codex._flowContext.LogEnvironment).LogError(
                    $"The properties of type '{type}' do not match the identifiers " +
                    $"of the flow definitions on this {nameof(UguiPageCodex)}. " +
                    $"The following properties are missing: {missingProperties}. " +
                    $"Please ensure the properties of type '{type}' match all flow definitions."
                );
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotFindFlowScopeForProperty(Type type, string identifier, UguiPageCodex codex)
        {
            codex.GetLogger(codex._flowContext.LogEnvironment).LogError(
                $"Cannot find a flow scope value for the property '{identifier}' of the type '{type}'. " +
                $"Please add the flow definitions to this {nameof(UguiPageCodex)} " +
                $"matching the properties of type '{type}'."
            );
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
