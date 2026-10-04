using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.Processing;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UIElements;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.UitkPages
{
    using UnityObject = UnityEngine.Object;

    public abstract class UitkPageFlow : IIsInitialized, IDisposable
    {
        public const string FLOW_USS_CLASS_NAME = "uitk-page-flow";
        public const string BLOCKER_USS_CLASS_NAME = FLOW_USS_CLASS_NAME + "__blocker";

        private static int s_flowSerial;

        private readonly Dictionary<string, UitkPagePool> _pools = new(StringComparer.Ordinal);
        private readonly Dictionary<IUitkPage, UitkPageSlot> _activeSlots = new();
        private readonly List<ISubscription> _subscriptions = new();
        private readonly List<ProcessRegistry> _processRegistries = new();
        private readonly Stack<Focusable> _focusHistory = new();

        private Transform _behaviourParent;
        private int _blockingCount;

        public bool IsInitialized { get; private set; }

        public string Identifier { get; private set; }

        public string ContainerName { get; private set; }

        public UitkPageCodex Codex { get; private set; }

        public UitkPageFlowContext Context { get; private set; }

        public VisualElement FlowElement { get; private set; }

        public VisualElement Blocker { get; private set; }

        public abstract bool IsInTransition { get; }

        public static bool TryCreate(PageFlowKind kind, out UitkPageFlow flow)
        {
            flow = kind switch {
                PageFlowKind.SinglePageStack => new UitkSinglePageStack(),
                PageFlowKind.MultiPageStack => new UitkMultiPageStack(),
                PageFlowKind.SinglePageList => new UitkSinglePageList(),
                PageFlowKind.MultiPageList => new UitkMultiPageList(),
                _ => null,
            };

            return flow != null;
        }

        public void Initialize(
              string identifier
            , string containerName
            , [NotNull] UitkPageCodex codex
            , [NotNull] UitkPageFlowContext context
            , Transform behaviourParent
        )
        {
            ThrowHelper.ThrowIfNull(identifier);
            ThrowHelper.ThrowIfNullOrUnityObjectInvalid(codex);
            ThrowHelper.ThrowIfNull(context);

            Identifier = identifier;
            ContainerName = containerName;
            Codex = codex;
            Context = context;
            _behaviourParent = behaviourParent;

            context.Owner = codex;
            context.FlowScope = new PageFlowScope(new Id3(
                  (Id<UitkPageFlow>)Type<UitkPageFlow>.Id
                , Interlocked.Increment(ref s_flowSerial)
                , 0
            ));

            FlowElement = CreateFlowElement(identifier);
            Blocker = CreateBlocker();
            FlowElement.Add(Blocker);

            var initContext = new InitializationContext(
                  context.Subscriber.Scope(context.FlowScope).WithSubscriptions(_subscriptions)
                , context.ProcessHub.WithRegistries(_processRegistries)
            );

            SubscribeMessages(initContext);
            OnInitialize(initContext);
            IsInitialized = true;
        }

        public void Dispose()
        {
            _subscriptions.AsListFast().Unsubscribe();
            _processRegistries.AsListFast().Unregister();

            OnDispose();

            foreach (var pool in _pools.Values)
            {
                pool.Dispose();
            }

            _pools.Clear();
            _activeSlots.Clear();
            _focusHistory.Clear();
            FlowElement?.RemoveFromHierarchy();
            IsInitialized = false;
        }

        public async UnityTask PrepoolPageAsync(string pageAssetKey, int amount, CancellationToken token)
        {
            ThrowHelper.ThrowIfNull(pageAssetKey);

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (amount < 1)
            {
                WarningIfAmountLesserThanOne(this);
                return;
            }

            var poolOpt = await GetInitializedPoolAsync(pageAssetKey, token);

            if (poolOpt.TryGetValue(out var pool) == false || token.IsCancellationRequested)
            {
                return;
            }

            var differentAmount = amount - pool.PoolingCount;

            if (differentAmount > 0)
            {
                pool.Prepool(differentAmount);
            }
        }

        public void TrimPool(string pageAssetKey, int amountToKeep)
        {
            ThrowHelper.ThrowIfNull(pageAssetKey);

            if (_pools.TryGetValue(pageAssetKey, out var pool))
            {
                pool.Trim(Mathf.Max(amountToKeep, 0));
            }
        }

        public bool TryGetPooledCount(string pageAssetKey, out int count)
        {
            if (_pools.TryGetValue(pageAssetKey, out var pool))
            {
                count = pool.PoolingCount;
                return true;
            }

            count = 0;
            return false;
        }

        protected abstract void OnInitialize(in InitializationContext context);

        protected abstract void OnDispose();

        internal bool TryGetSlot(IUitkPage page, out UitkPageSlot slot)
        {
            if (page == null)
            {
                slot = null;
                return false;
            }

            return _activeSlots.TryGetValue(page, out slot);
        }

        internal async UnityTask<Option<UitkPageSlot>> RentPageAsync(
              string pageAssetKey
            , PageContext context
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNull(pageAssetKey);
            ThrowIfNotInitialized(IsInitialized, this);

            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            var poolOpt = await GetInitializedPoolAsync(pageAssetKey, token);

            if (poolOpt.TryGetValue(out var pool) == false || token.IsCancellationRequested)
            {
                return Option.None;
            }

            if (pool.Rent().TryGetValue(out var slot) == false)
            {
                return Option.None;
            }

            _activeSlots[slot.Page] = slot;

            if (slot.Page is IPageOnCreateAsync onCreate)
            {
                var creationResult = await onCreate.OnCreateAsync(context, token);

                if (creationResult == false)
                {
                    WarningIfPageCreationFailed(pageAssetKey, this);
                }

                if (creationResult == false || token.IsCancellationRequested)
                {
                    ReturnPage(slot.Page, context);
                    return Option.None;
                }
            }

            return slot;
        }

        internal bool ReturnPage(IUitkPage page, PageContext context)
        {
            if (page == null || _activeSlots.Remove(page, out var slot) == false)
            {
                return false;
            }

            BlurIfFocusedInside(slot);

            if (page is IPageOnReturnToPool onReturn)
            {
                onReturn.OnReturnToPool(context);
            }

            if (_pools.TryGetValue(slot.AssetKey, out var pool) == false)
            {
                ErrorIfCannotReturnToPool(slot.AssetKey, this);
                return false;
            }

            if (context.ReturnOperation == PageReturnOperation.Destroy)
            {
                pool.Destroy(slot);
                return false;
            }

            pool.Return(slot);
            return true;
        }

        internal void ShowSlot(UitkPageSlot slot)
        {
            var container = slot.Container;
            container.style.display = DisplayStyle.Flex;

            if (container.parent == FlowElement)
            {
                container.PlaceBehind(Blocker);
            }
        }

        internal void HideSlot(UitkPageSlot slot)
        {
            BlurIfFocusedInside(slot);
            slot.Container.style.display = DisplayStyle.None;
        }

        internal void BeginBlocking()
        {
            _blockingCount++;
            Blocker.style.display = DisplayStyle.Flex;
        }

        internal void EndBlocking()
        {
            _blockingCount = Mathf.Max(_blockingCount - 1, 0);

            if (_blockingCount < 1)
            {
                Blocker.style.display = DisplayStyle.None;
            }
        }

        internal void RecordFocus()
        {
            _focusHistory.Push(FlowElement.panel?.focusController?.focusedElement);
        }

        internal void RestoreFocus()
        {
            if (_focusHistory.TryPop(out var focusable) && focusable is VisualElement element && IsDisplayed(element))
            {
                element.Focus();
            }
        }

        internal static void FillParent(IStyle style)
        {
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
        }

        internal static void ApplyFocus(UitkPageSlot slot)
        {
            if (slot.Page is not IUitkPageHasFocus page)
            {
                return;
            }

            switch (page.FocusMode)
            {
                case PageFocusMode.FirstFocusable:
                {
                    var target = slot.Container.Query<VisualElement>()
                        .Where(static element => element.focusable && element.canGrabFocus)
                        .First();

                    target?.Focus();
                    break;
                }

                case PageFocusMode.Named:
                {
                    if (string.IsNullOrEmpty(page.FocusElementName) == false)
                    {
                        slot.Container.Q(page.FocusElementName)?.Focus();
                    }

                    break;
                }
            }
        }

        private static void BlurIfFocusedInside(UitkPageSlot slot)
        {
            var container = slot.Container;

            if (container.panel?.focusController?.focusedElement is VisualElement focused
                && (focused == container || container.Contains(focused))
            )
            {
                focused.Blur();
            }
        }

        private static bool IsDisplayed(VisualElement element)
        {
            if (element.panel == null)
            {
                return false;
            }

            for (var current = element; current != null; current = current.hierarchy.parent)
            {
                if (current.resolvedStyle.display == DisplayStyle.None)
                {
                    return false;
                }
            }

            return true;
        }

        private static VisualElement CreateFlowElement(string identifier)
        {
            var element = new VisualElement {
                name = identifier,
                pickingMode = PickingMode.Ignore,
            };

            element.AddToClassList(FLOW_USS_CLASS_NAME);
            element.AddToClassList($"{FLOW_USS_CLASS_NAME}--{identifier}");
            FillParent(element.style);
            return element;
        }

        private static VisualElement CreateBlocker()
        {
            var element = new VisualElement { pickingMode = PickingMode.Position };
            element.AddToClassList(BLOCKER_USS_CLASS_NAME);
            FillParent(element.style);
            element.style.display = DisplayStyle.None;
            return element;
        }

        private async UnityTask<Option<UitkPagePool>> GetInitializedPoolAsync(
              string pageAssetKey
            , CancellationToken token
        )
        {
            if (_pools.TryGetValue(pageAssetKey, out var pool) == false)
            {
                var logger = Codex.GetLogger(Context.LogEnvironment);
                pool = new UitkPagePool(pageAssetKey, FlowElement, Blocker, _behaviourParent, logger);
                _pools[pageAssetKey] = pool;
            }

            if (pool.IsInitialized)
            {
                return pool;
            }

            var sourceOpt = await PageAssetLoader.LoadAsync<UnityObject>(Context.LoadStrategy, pageAssetKey, token);

            if (token.IsCancellationRequested)
            {
                return Option.None;
            }

            if (sourceOpt.TryGetValue(out var source) == false || source.IsInvalid())
            {
                ErrorIfCannotLoadAsset(pageAssetKey, this);
                return Option.None;
            }

            return pool.TryInitialize(source) ? pool : Option.None;
        }

        private void SubscribeMessages(in InitializationContext context)
        {
            var subscriber = context.Subscriber.WithState(this);
            PrepoolPageMessage.Async.Subscribe(in subscriber, HandleAsync);
            TrimPoolMessage.Subscribe(in subscriber, Handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnityTask HandleAsync(UitkPageFlow flow, PrepoolPageMessage.Async msg, PublishingContext context)
        {
            var message = (PrepoolPageMessage)msg;
            return flow.PrepoolPageAsync(message.AssetKey, message.Amount, context.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Handle(UitkPageFlow flow, TrimPoolMessage msg)
            => flow.TrimPool(msg.AssetKey, msg.AmountToKeep);

        [HideInCallstack, StackTraceHidden]
        private static void ThrowIfNotInitialized([DoesNotReturnIf(false)] bool isInitialized, UitkPageFlow context)
        {
            if (isInitialized == false)
            {
                throw CreateException(context);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(UitkPageFlow context)
                => new($"The page flow '{context.Identifier}' must be initialized by its {nameof(UitkPageCodex)}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void WarningIfAmountLesserThanOne(UitkPageFlow context)
        {
            context.Codex.GetLogger(context.Context.LogEnvironment).LogWarning(
                "The amount of preloaded instances should be greater than 0."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotReturnToPool(string key, UitkPageFlow context)
        {
            context.Codex.GetLogger(context.Context.LogEnvironment).LogError(
                $"Cannot return the page originated from '{key}' to pool. The pool may not be created properly."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfCannotLoadAsset(string key, UitkPageFlow context)
        {
            context.Codex.GetLogger(context.Context.LogEnvironment).LogError(
                $"Cannot load asset by the key '{key}'"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void WarningIfPageCreationFailed(string key, UitkPageFlow context)
        {
            context.Codex.GetLogger(context.Context.LogEnvironment).LogWarning(
                $"The result of '{nameof(IPageOnCreateAsync)}.{nameof(IPageOnCreateAsync.OnCreateAsync)}' " +
                $"for page loaded by the key '{key}' was a failure."
            );
        }

        protected readonly record struct InitializationContext(
              MessageSubscriber.Subscriber<PageFlowScope> Subscriber
            , Processor.Hub<PageFlowScope> ProcessHub
        );
    }
}
