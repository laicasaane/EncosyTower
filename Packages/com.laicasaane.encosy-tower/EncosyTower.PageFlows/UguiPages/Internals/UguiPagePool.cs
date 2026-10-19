#if UNITY_UGUI

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.PageFlows.Loading;
using EncosyTower.Pooling;
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.PageFlows.UguiPages.Internals
{
    using GameObjectId = UnityEntityId<GameObject>;

    internal class UguiPagePool : IDisposable, IIsInitialized
    {
        private readonly GameObjectPool _pool = new();
        private readonly HashSet<GameObjectId> _gameObjectIds = new();
        private readonly List<UguiPageIdentifier> _pageIds;
        private readonly Transform _poolParent;
        private readonly Transform _activeParent;
        private readonly int _poolLayer;
        private readonly int _activeLayer;

        private Option<UguiPageOptions> _options;
        private PageAssetHandle<GameObject> _sourceLease;

        public UguiPagePool(
              Transform poolParent
            , Transform activeParent
            , List<UguiPageIdentifier> pageIds
            , RentingStrategy rentingStrategy
            , ReturningStrategy returningStrategy
        )
        {
            _poolParent = poolParent;
            _activeParent = activeParent;
            _pageIds = pageIds;
            _poolLayer = poolParent.gameObject.layer;
            _activeLayer = activeParent.gameObject.layer;
            _pool.RentingStrategy = rentingStrategy;
            _pool.ReturningStrategy = returningStrategy;
        }

        public bool IsInitialized => _pool.Prefab != null;

        public int PoolingCount => _pool.UnusedCount;

        public void Initialize(PageAssetHandle<GameObject> lease)
        {
            if (IsInitialized)
            {
                lease.Release();
                return;
            }

            var source = lease.Asset;

            _pool.Prefab = new GameObjectPrefab {
                Parent = _poolParent,
                Source = source,
                InstantiateInWorldSpace = false,
            };

            _options = source.TryGetComponent<UguiPageOptions>(out var options) ? options : Option.None;
            _sourceLease = lease;
        }

        public void Prepool(int amount)
        {
            if (_pool.Prepool(amount, GetReturningStrategy()))
            {
                _gameObjectIds.EnsureCapacity(amount);
                _pageIds.AsListFast().IncreaseCapacityBy(amount);
            }
        }

        public Result<UguiPageIdentifier, PageFlowError> Rent(
              StringId keyId
            , AssetKey pageAssetKey
            , UnityObjectLogger logger
        )
        {
            var gameObject = _pool.RentGameObject(GetRentingStrategy());
            var findResult = gameObject.TryGetComponent<IUguiPage>(out var page);

            if (findResult == false)
            {
                UnityEngine.Object.Destroy(gameObject);
                return PageFlowError.AssetNotSinglePage(assetKey: pageAssetKey, pageCount: 0);
            }

            if (page is not Component component)
            {
                UnityEngine.Object.Destroy(gameObject);
                return PageFlowError.PageNotComponent(page);
            }

            gameObject.layer = _activeLayer;

            var transform = component.transform;
            var identifier = gameObject.GetOrAddComponent<UguiPageIdentifier>();

            component.TryGetComponent<CanvasGroup>(out var canvasGroup);

            identifier.KeyId = keyId;
            identifier.AssetKey = pageAssetKey;
            identifier.Transform = transform;
            identifier.GameObject = gameObject;
            identifier.CanvasGroup = canvasGroup;
            identifier.Transform.SetParent(_poolParent, false);
            identifier.GameObjectId = gameObject;
            identifier.Page = page;

            if (_gameObjectIds.Add(identifier.GameObjectId))
            {
                _pageIds.Add(identifier);
            }
            else
            {
                WarningIfAlreadyInActive(page, pageAssetKey, logger);
            }

            return identifier;
        }

        public void MoveToActive(UguiPageIdentifier identifier)
        {
            var transform = identifier.Transform;

            if (ReferenceEquals(transform.parent, _activeParent) == false)
            {
                transform.SetParent(_activeParent, false);
            }
        }

        public void MoveToPool(UguiPageIdentifier identifier)
        {
            var transform = identifier.Transform;

            if (ReferenceEquals(transform.parent, _poolParent) == false)
            {
                transform.SetParent(_poolParent, false);
            }
        }

        public void Return(UguiPageIdentifier identifier)
        {
            if (_gameObjectIds.Remove(identifier.GameObjectId) == false)
            {
                return;
            }

            _pageIds.Remove(identifier);

            identifier.GameObject.layer = _poolLayer;
            identifier.Transform.SetParent(_poolParent, false);

            _pool.Return(identifier.GameObject, GetReturningStrategy());
        }

        public void Dispose()
        {
            ReturnActiveObjects();

            _pool.ReleaseInstances(0);
            _pool.Dispose();
            _sourceLease.Release();
            _sourceLease = default;
            _options = Option.None;
        }

        public void Destroy(int amountToDestroy)
        {
            _pool.ReleaseInstances(_pool.UnusedCount - amountToDestroy);
        }

        private void ReturnActiveObjects()
        {
            var gameObjectIds = _gameObjectIds;
            var length = gameObjectIds.Count;
            var array = NativeArray.CreateFast<GameObjectId>(length, Allocator.Temp);
            var index = 0;

            foreach (var id in gameObjectIds)
            {
                array[index] = id;
                index++;
            }

            gameObjectIds.Clear();
            _pageIds.Clear();
            _pool.Return(array, GetReturningStrategy());
        }

        private RentingStrategy GetRentingStrategy()
            => _options.TryGetValue(out var options) ? options.RentingStrategy : RentingStrategy.Default;

        private ReturningStrategy GetReturningStrategy()
            => _options.TryGetValue(out var options) ? options.ReturningStrategy : ReturningStrategy.Default;

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void WarningIfAlreadyInActive(IUguiPage page, AssetKey assetKey, UnityObjectLogger logger)
        {
            logger.LogWarning(
                $"The page '{page.GetType()}' on the asset '{assetKey}' is already in active. " +
                $"This might indicate an unexpected bug in the pooling mechanism."
            );
        }
    }
}

#endif
