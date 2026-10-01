using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Initialization;
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using UnityEngine;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Databases
{
    public class DatabaseAsset : ScriptableObject, IInitializable, IDeinitializable, IIsInitialized
    {
        [SerializeField]
        internal DataTableAssetBase[] _tables = new DataTableAssetBase[0];

        [SerializeField]
        internal DataTableAssetBase[] _redundantTabless = new DataTableAssetBase[0];

        private readonly Dictionary<StringId, DataTableAssetBase> _idToAsset = new();
        private readonly Dictionary<Type, DataTableAssetBase> _typeToAsset = new();
        private readonly Dictionary<Type, List<StringId>> _typeToIds = new();

        private StringVault _stringVault;

        protected IReadOnlyDictionary<StringId, DataTableAssetBase> IdToAsset => _idToAsset;

        protected IReadOnlyDictionary<Type, DataTableAssetBase> TypeToAsset => _typeToAsset;

        protected ReadOnlyMemory<DataTableAssetBase> Tables => _tables;

        protected ReadOnlyMemory<DataTableAssetBase> RedundantTables => _redundantTabless;

        public bool IsInitialized { get; protected set; }

        public StringVault StringVault
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _stringVault ?? StringVault.Default;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _stringVault = value;
        }

        public virtual void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            var tables = Tables.Span;
            var assetsLength = tables.Length;
            var idToAsset = _idToAsset;
            var typeToAsset = _typeToAsset;
            var typeToIds = _typeToIds;

            idToAsset.Clear();
            idToAsset.EnsureCapacity(assetsLength);

            typeToAsset.Clear();
            typeToAsset.EnsureCapacity(assetsLength);

            typeToIds.Clear();
            typeToIds.EnsureCapacity(assetsLength);

            for (var i = 0; i < assetsLength; i++)
            {
                var table = tables[i];

                if (table.IsInvalid())
                {
                    ThrowHelper.LogErrorAssetIsInvalid(i, this);
                    continue;
                }

                var type = table.GetType();
                var name = table.name;
                var id = GetId(name);

                idToAsset[id] = table;

                if (typeToAsset.TryGetValue(type, out var otherAsset))
                {
                    ThrowHelper.LogWarningAmbiguousTypeAtInitialization(
                          i
                        , type
                        , name
                        , otherAsset.name
                        , this
                    );
                }
                else
                {
                    typeToAsset[type] = table;
                }

                if (typeToIds.TryGetValue(type, out var ids) == false)
                {
                    typeToIds[type] = ids = new(1);
                }

                ids.Add(id);
                table.Initialize();
            }

            IsInitialized = true;
        }

        public virtual void Deinitialize()
        {
            if (IsInitialized == false)
            {
                return;
            }

            IsInitialized = false;

            foreach (var asset in _idToAsset.Values)
            {
                asset.Deinitialize();
            }

            _idToAsset.Clear();
            _typeToAsset.Clear();
            _typeToIds.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset([NotNull] string name)
        {
            DebuggingThrowHelper.ThrowIfNull(name);
            return Option.SomeIf(TryGetDataTableAsset(name, out var asset), asset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset([NotNull] string name, out DataTableAssetBase tableAsset)
        {
            DebuggingThrowHelper.ThrowIfNull(name);
            return TryGetDataTableAsset(GetId(name), out tableAsset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset(StringId id)
            => Option.SomeIf(TryGetDataTableAsset(id, out var asset), asset);

        public bool TryGetDataTableAsset(StringId id, out DataTableAssetBase tableAsset)
        {
            ThrowHelper.ThrowsDatabaseIsNotInitialized(IsInitialized);

            if (_idToAsset.TryGetValue(id, out var asset))
            {
                tableAsset = asset;
                return true;
            }
            else
            {
                ThrowHelper.LogErrorCannotFindAsset(id, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<DataTableAssetBase> GetDataTableAsset([NotNull] Type type)
        {
            DebuggingThrowHelper.ThrowIfNull(type);
            return Option.SomeIf(TryGetDataTableAsset(type, out var asset), asset);
        }

        public bool TryGetDataTableAsset([NotNull] Type type, out DataTableAssetBase tableAsset)
        {
            DebuggingThrowHelper.ThrowIfNull(type);

            ThrowHelper.ThrowsDatabaseIsNotInitialized(IsInitialized);
            ThrowHelper.LogWarningAmbiguousTypeAtGetDataTableAsset(type, _typeToIds, this);

            if (_typeToAsset.TryGetValue(type, out var asset))
            {
                tableAsset = asset;
                return true;
            }
            else
            {
                ThrowHelper.LogErrorCannotFindAsset(type, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> GetDataTableAsset<T>() where T : DataTableAssetBase
            => Option.SomeIf(TryGetDataTableAsset<T>(out var asset), asset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset<T>(out T tableAsset)
            where T : DataTableAssetBase
        {
            ThrowHelper.ThrowsDatabaseIsNotInitialized(IsInitialized);

            var type = typeof(T);

            ThrowHelper.LogWarningAmbiguousTypeAtGetDataTableAsset(type, _typeToIds, this);

            if (_typeToAsset.TryGetValue(type, out var asset))
            {
                if (asset is T assetT)
                {
                    tableAsset = assetT;
                    return true;
                }
                else
                {
                    ThrowHelper.LogErrorFoundAssetIsNotValidType<T>(asset);
                }
            }
            else
            {
                ThrowHelper.LogErrorCannotFindAsset(type, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> GetDataTableAsset<T>([NotNull] string name)
            where T : DataTableAssetBase
        {
            DebuggingThrowHelper.ThrowIfNull(name);
            return Option.SomeIf(TryGetDataTableAsset<T>(name, out var asset), asset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDataTableAsset<T>([NotNull] string name, out T tableAsset)
            where T : DataTableAssetBase
        {
            DebuggingThrowHelper.ThrowIfNull(name);
            return TryGetDataTableAsset<T>(GetId(name), out tableAsset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> GetDataTableAsset<T>(StringId id)
            where T : DataTableAssetBase
            => Option.SomeIf(TryGetDataTableAsset<T>(id, out var asset), asset);

        public bool TryGetDataTableAsset<T>(StringId id, out T tableAsset)
            where T : DataTableAssetBase
        {
            ThrowHelper.ThrowsDatabaseIsNotInitialized(IsInitialized);

            if (_idToAsset.TryGetValue(id, out var asset))
            {
                if (asset is T assetT)
                {
                    tableAsset = assetT;
                    return true;
                }
                else
                {
                    ThrowHelper.LogErrorFoundAssetIsNotValidType<T>(asset);
                }
            }
            else
            {
                ThrowHelper.LogErrorCannotFindAsset(id, this);
            }

            tableAsset = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetName(StringId id)
        {
            StringVault.TryGetManagedString(id, out var name);
            return name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringId GetId(string name)
        {
            return StringVault.GetOrMakeId(name);
        }

    }
}
