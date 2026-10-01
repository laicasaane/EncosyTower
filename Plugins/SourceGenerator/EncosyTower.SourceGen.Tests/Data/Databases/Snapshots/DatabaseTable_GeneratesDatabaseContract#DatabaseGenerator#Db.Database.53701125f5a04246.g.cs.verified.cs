#pragma warning disable 0219

using EncosyTower.Data;
using EncosyTower.Databases;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SD = System.Diagnostics;
using g__SDCA = System.Diagnostics.CodeAnalysis;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__ETAK = global::EncosyTower.AssetKeys;
using g__ET = global::EncosyTower.Common;
using g__ETDB = EncosyTower.Databases;
using g__ETDBSG = global::EncosyTower.Databases.SourceGen;
using g__ETDBG = global::EncosyTower.Debugging;
using g__ETDVD = global::EncosyTower.Debugging.ValidationDefines;
using g__ETI = global::EncosyTower.Initialization;
using g__ETSI = global::EncosyTower.StringIds;
using g__ETUE = global::EncosyTower.UnityExtensions;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    partial class Db : g__ETDB.IDatabase, g__ET.IIsValid, g__ETI.IInitializable, g__ETI.IIsInitialized, g__ETI.IDeinitializable
    {
        private readonly g__ETDB.DatabaseAsset _database;

        public Db(g__ETDB.DatabaseAsset database)
        {
            ThrowIfInvalid(database);
            _database = database;
        }

        public Db(g__ETDB.DatabaseAsset database, g__ETI.InitializationBehaviour initializationBehaviour)
        {
            ThrowIfInvalid(database);
            _database = database;

            switch (initializationBehaviour)
            {
                case g__ETI.InitializationBehaviour.Respect:
                {
                    if (IsInitialized == false)
                    {
                        Initialize();
                    }

                    break;
                }

                case g__ETI.InitializationBehaviour.Forced:
                {
                    Deinitialize();
                    Initialize();
                    break;
                }
            }
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public bool IsValid
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => g__ETUE.EncosyUnityObjectExtensions.IsValid(_database);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public bool IsInitialized
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfNotCreated(IsValid);
                return _database.IsInitialized;
            }
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public g__ETDB.DatabaseAsset Database
        {
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            get => _database;
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public void Initialize()
        {
            ThrowIfNotCreated(IsValid);

            Ids.Initialize();

            _database.StringVault = Vault.Instance;
            _database.Initialize();
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public void Deinitialize()
        {
            ThrowIfNotCreated(IsValid);
            _database.Deinitialize();
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        private global::TestProject.Rows Get_Items()
        {
            ThrowIfNotCreated(IsValid);
            return _database.GetDataTableAsset<global::TestProject.Rows>(Ids.Tables.Items).GetValueOrDefault();
        }

        [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)][g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public static explicit operator Db(g__ETDB.DatabaseAsset database)
        {
            return new Db(database);
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        [g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfInvalid(global::UnityEngine.Object asset)
        {
            if (g__ETUE.EncosyUnityObjectExtensions.IsInvalid(asset))
            {
                throw new g__S.ArgumentNullException("asset");
            }
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        [g__SD.Conditional(g__ETDVD.UNITY_EDITOR), g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]
        private static void ThrowIfNotCreated([g__SDCA.DoesNotReturnIf(false)] bool value)
        {
            if (value == false)
            {
                throw new g__S.InvalidOperationException("Db must be created using the constructor that takes a DatabaseAsset.");
            }
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public static partial class Names
        {
            [g__ETDBSG.GeneratedAssetNameConstant(typeof(Db), typeof(g__ETDB.DatabaseAsset))]
            public const string DATABASE = "DatabaseAsset_Db";

            public static partial class Tables
            {
                [g__ETDBSG.GeneratedAssetNameConstant(typeof(Db), typeof(global::TestProject.Rows))]
                public const string ITEMS = "Rows";

            }
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public static partial class Keys
        {
            public static readonly g__ETAK.AssetKey<g__ETDB.DatabaseAsset> Database = new(Names.DATABASE);

            public static partial class Tables
            {
                public static readonly g__ETAK.AssetKey<global::TestProject.Rows> Items = new(Names.Tables.ITEMS);

            }
        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public static partial class Ids
        {
            public static g__ETSI.StringId Database { get; private set; }

            public static void Initialize()
            {
#if UNITY_EDITOR
                Vault.Instance.Clear();
#endif

                Database = Vault.Instance.GetOrMakeId(Names.DATABASE);

                Tables.Initialize();
            }

            public static partial class Tables
            {
                public static g__ETSI.StringId Items { get; private set; }

                public static void Initialize()
                {
                    Items = Vault.Instance.GetOrMakeId(Names.Tables.ITEMS);
                }
            }

        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        public static partial class Instance
        {
            private static Db s_instance;

            public static bool IsInitialized
            {
                get
                {
                    if (s_instance is not null)
                    {
                        return s_instance.IsValid && s_instance.IsInitialized;
                    }

                    return false;
                }
            }

            public static g__ETDB.DatabaseAsset Database
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => s_instance.Database;
            }

            public static void Initialize([g__SDCA.NotNull] Db instance)
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(instance);
                s_instance = instance;
                s_instance.Initialize();
            }

            public static void Deinitialize()
            {
                s_instance.Deinitialize();
                s_instance = default;
            }

#if UNITY_EDITOR
            [global::UnityEditor.InitializeOnEnterPlayMode]
            private static void InitWhenDomainReloadDisabled()
            {
                if (IsInitialized)
                {
                    Deinitialize();
                }
            }
#endif

            public static partial class Tables
            {
                public static global::TestProject.Rows Items
                {
                    [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                    get => s_instance.Items;
                }

            }

        }

        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.8-preview.1")]
        private static partial class Vault
        {
            public static readonly g__ETSI.StringVault Instance = new(2);

        }

    }



}

