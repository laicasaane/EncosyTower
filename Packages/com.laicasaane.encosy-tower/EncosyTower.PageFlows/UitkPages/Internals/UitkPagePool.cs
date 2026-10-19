using System;
using System.Collections.Generic;
using EncosyTower.AssetKeys;
using EncosyTower.Common;
using EncosyTower.PageFlows.Loading;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.PageFlows.UitkPages.Internals
{
    using UnityObject = UnityEngine.Object;

    internal sealed class UitkPageSlot
    {
        public UitkPageSlot(AssetKey assetKey, TemplateContainer container, IUitkPage page, GameObject gameObject)
        {
            AssetKey = assetKey;
            Container = container;
            Page = page;
            GameObject = gameObject;
        }

        public AssetKey AssetKey { get; }

        public TemplateContainer Container { get; }

        public IUitkPage Page { get; }

        public GameObject GameObject { get; }
    }

    internal sealed class UitkPagePool : IDisposable
    {
        public const string SLOT_USS_CLASS_NAME = "uitk-page-slot";

        private readonly Stack<UitkPageSlot> _unused = new();
        private readonly List<UitkPageSlot> _created = new();
        private readonly AssetKey _assetKey;
        private readonly VisualElement _flowElement;
        private readonly VisualElement _blocker;
        private readonly Transform _behaviourParent;

        private UnityObject _source;
        private PageAssetHandle<UnityObject> _sourceLease;

        public UitkPagePool(
              AssetKey assetKey
            , VisualElement flowElement
            , VisualElement blocker
            , Transform behaviourParent
        )
        {
            _assetKey = assetKey;
            _flowElement = flowElement;
            _blocker = blocker;
            _behaviourParent = behaviourParent;
        }

        public bool IsInitialized => _source.IsValid();

        public int PoolingCount => _unused.Count;

        public Success<PageAssetLoadError> Initialize(PageAssetHandle<UnityObject> lease)
        {
            if (IsInitialized)
            {
                lease.Release();
                return Success.Yes;
            }

            var source = lease.Asset;

            if (source is VisualTreeAsset or GameObject)
            {
                _source = source;
                _sourceLease = lease;
                return Success.Yes;
            }

            lease.Release();
            return PageAssetLoadError.UnsupportedAsset(_assetKey, source.GetType());
        }

        public Result<UitkPageSlot, PageFlowError> Rent()
        {
            if (_unused.TryPop(out var slot) == false)
            {
                var createResult = TryCreate(out slot);

                if (createResult.TryGetFailure(out var error))
                {
                    return error;
                }
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

        public Success<PageFlowError> Prepool(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var createResult = TryCreate(out var slot);

                if (createResult.IsFailure)
                {
                    return createResult;
                }

                _unused.Push(slot);
            }

            return Success.Yes;
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
            _sourceLease.Release();
            _sourceLease = default;
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

        private Success<PageFlowError> TryCreate(out UitkPageSlot slot)
        {
            var instantiateResult = TryInstantiate(out var container, out var page, out var gameObject);

            if (instantiateResult.IsFailure)
            {
                slot = null;
                return instantiateResult;
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
            return Success.Yes;
        }

        private Success<PageFlowError> TryInstantiate(
              out TemplateContainer container
            , out IUitkPage page
            , out GameObject gameObject
        )
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
                    UnityObject.Destroy(gameObject);
                    gameObject = null;
                    container = null;
                    page = null;
                    return PageFlowError.PageBehaviourMissing(_assetKey);
                }

                container = behaviour.VisualTreeAsset.Instantiate();
                page = behaviour;
                return Success.Yes;
            }

            container = null;
            page = null;
            var sourceType = _source.IsValid() ? _source.GetType() : null;
            var unsupported = PageAssetLoadError.UnsupportedAsset(_assetKey, sourceType);
            return PageFlowError.AssetLoadFailed(_assetKey, unsupported);
        }

        private Success<PageFlowError> TryFindSinglePage(TemplateContainer container, out IUitkPage page)
        {
            var pages = container.Query<VisualElement>().Where(static element => element is IUitkPage).ToList();

            if (pages.Count == 1)
            {
                page = (IUitkPage)pages[0];
                return Success.Yes;
            }

            page = null;
            return PageFlowError.AssetNotSinglePage(assetKey: _assetKey, pageCount: pages.Count);
        }
    }
}
