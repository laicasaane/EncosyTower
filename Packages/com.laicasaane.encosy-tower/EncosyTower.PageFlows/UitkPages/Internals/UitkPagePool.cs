using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages
{
    using UnityObject = UnityEngine.Object;

    internal sealed class UitkPageSlot
    {
        public UitkPageSlot(string assetKey, TemplateContainer container, IUitkPage page, GameObject gameObject)
        {
            AssetKey = assetKey;
            Container = container;
            Page = page;
            GameObject = gameObject;
        }

        public string AssetKey { get; }

        public TemplateContainer Container { get; }

        public IUitkPage Page { get; }

        public GameObject GameObject { get; }
    }

    internal sealed class UitkPagePool : IDisposable
    {
        public const string SLOT_USS_CLASS_NAME = "uitk-page-slot";

        private readonly Stack<UitkPageSlot> _unused = new();
        private readonly List<UitkPageSlot> _created = new();
        private readonly string _assetKey;
        private readonly VisualElement _flowElement;
        private readonly VisualElement _blocker;
        private readonly Transform _behaviourParent;
        private readonly UnityObjectLogger _logger;

        private UnityObject _source;

        public UitkPagePool(
              string assetKey
            , VisualElement flowElement
            , VisualElement blocker
            , Transform behaviourParent
            , UnityObjectLogger logger
        )
        {
            _assetKey = assetKey;
            _flowElement = flowElement;
            _blocker = blocker;
            _behaviourParent = behaviourParent;
            _logger = logger;
        }

        public bool IsInitialized => _source.IsValid();

        public int PoolingCount => _unused.Count;

        public bool TryInitialize(UnityObject source)
        {
            if (source is VisualTreeAsset or GameObject)
            {
                _source = source;
                return true;
            }

            ErrorIfUnsupportedAsset(_assetKey, source, _logger);
            return false;
        }

        public Option<UitkPageSlot> Rent()
        {
            if (_unused.TryPop(out var slot) == false && TryCreate(out slot) == false)
            {
                return Option.None;
            }

            if (slot.GameObject.IsValid())
            {
                slot.GameObject.SetActive(true);
            }

            return slot;
        }

        public void Return(UitkPageSlot slot)
        {
            Hide(slot);
            _unused.Push(slot);
        }

        public void Destroy(UitkPageSlot slot)
        {
            _created.Remove(slot);
            DestroySlot(slot);
        }

        public void Prepool(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                if (TryCreate(out var slot) == false)
                {
                    return;
                }

                _unused.Push(slot);
            }
        }

        public void Trim(int amountToKeep)
        {
            while (_unused.Count > amountToKeep)
            {
                Destroy(_unused.Pop());
            }
        }

        public void Dispose()
        {
            var count = _created.Count;

            for (var i = 0; i < count; i++)
            {
                DestroySlot(_created[i]);
            }

            _created.Clear();
            _unused.Clear();
            _source = null;
        }

        private static void Hide(UitkPageSlot slot)
        {
            slot.Container.style.display = DisplayStyle.None;

            if (slot.GameObject.IsValid())
            {
                slot.GameObject.SetActive(false);
            }
        }

        private static void DestroySlot(UitkPageSlot slot)
        {
            slot.Container.RemoveFromHierarchy();

            if (slot.GameObject.IsValid())
            {
                UnityObject.Destroy(slot.GameObject);
            }
        }

        private bool TryCreate(out UitkPageSlot slot)
        {
            if (TryInstantiate(out var container, out var page, out var gameObject) == false)
            {
                slot = null;
                return false;
            }

            container.AddToClassList(SLOT_USS_CLASS_NAME);
            container.usageHints = UsageHints.DynamicTransform;

            var style = container.style;
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.display = DisplayStyle.None;

            _flowElement.Insert(_flowElement.IndexOf(_blocker), container);

            if (page is IUitkPageSlotReceiver receiver)
            {
                receiver.SetSlot(container);
            }

            slot = new UitkPageSlot(_assetKey, container, page, gameObject);
            _created.Add(slot);
            return true;
        }

        private bool TryInstantiate(out TemplateContainer container, out IUitkPage page, out GameObject gameObject)
        {
            gameObject = null;

            if (_source is VisualTreeAsset visualTreeAsset)
            {
                container = visualTreeAsset.Instantiate();
                return TryFindSinglePage(container, out page);
            }

            if (_source is GameObject prefab)
            {
                gameObject = UnityObject.Instantiate(prefab, _behaviourParent, false);
                gameObject.SetActive(false);

                if (gameObject.TryGetComponent<UitkPageBehaviour>(out var behaviour) == false
                    || behaviour.VisualTreeAsset.IsInvalid()
                )
                {
                    ErrorIfBehaviourInvalid(_assetKey, _logger);
                    UnityObject.Destroy(gameObject);
                    gameObject = null;
                    container = null;
                    page = null;
                    return false;
                }

                container = behaviour.VisualTreeAsset.Instantiate();
                page = behaviour;
                return true;
            }

            container = null;
            page = null;
            return false;
        }

        private bool TryFindSinglePage(TemplateContainer container, out IUitkPage page)
        {
            var pages = container.Query<VisualElement>().Where(static element => element is IUitkPage).ToList();

            if (pages.Count == 1)
            {
                page = (IUitkPage)pages[0];
                return true;
            }

            ErrorIfPageCountInvalid(_assetKey, pages.Count, _logger);
            page = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfUnsupportedAsset(string key, UnityObject source, UnityObjectLogger logger)
        {
            logger.LogError(
                $"The asset loaded by the key '{key}' is a '{source.GetType()}'. " +
                $"A UI Toolkit page must be a VisualTreeAsset or a GameObject with a {nameof(UitkPageBehaviour)}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfPageCountInvalid(string key, int count, UnityObjectLogger logger)
        {
            logger.LogError(
                $"The UXML loaded by the key '{key}' must contain exactly one element implementing " +
                $"{nameof(IUitkPage)}, but it contains {count}."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void ErrorIfBehaviourInvalid(string key, UnityObjectLogger logger)
        {
            logger.LogError(
                $"The GameObject loaded by the key '{key}' must have a {nameof(UitkPageBehaviour)} " +
                $"that references a VisualTreeAsset."
            );
        }
    }
}
