#pragma warning disable 0219
#define DATABASE_AUTHORING
#define BAKING_SHEET

using EncosyTower.Data;
using EncosyTower.Databases;
using EncosyTower.Databases.Authoring;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SCG = global::System.Collections.Generic;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SRIS = global::System.Runtime.InteropServices;
using g__CBS = Cathei.BakingSheet;
using g__CBSU = Cathei.BakingSheet.Unity;
using g__ET = global::EncosyTower.Common;
using g__ETC = EncosyTower.Collections;
using g__ETCE = global::EncosyTower.Collections.Extensions;
using g__ETD = global::EncosyTower.Data;
using g__ETDBA = EncosyTower.Databases.Authoring;
using g__ETDBASG = EncosyTower.Databases.Authoring.SourceGen;
using g__ETDSG = global::EncosyTower.Data.SourceGen;
using g__ETN = EncosyTower.Naming;
using g__MEL = Microsoft.Extensions.Logging;
using g__UE = global::UnityEngine;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

    partial struct GameDbAuthoring : g__ETDBASG.IContains<GameDbAuthoring.SheetContainer>
    {
#if DATABASE_AUTHORING && BAKING_SHEET

        [g__S.Serializable]
        [g__ETDBASG.GeneratedSheetContainer]
        [g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class SheetContainer : g__ETDBA.DataSheetContainerBase, g__ETDBA.IPostExportDatabase
        {
            public SheetContainer() : this(g__CBSU.UnityLogger.Default)
            { }

            public SheetContainer(g__MEL.ILogger logger) : base(logger)
            {
                this.LocTable_LocSheet_Items = new LocTable_LocSheet_Items();
            }

            public LocTable_LocSheet_Items LocTable_LocSheet_Items { get; set; }

            public g__SCG.List<LocSheet> LocSheets
            {
                get => new g__SCG.List<LocSheet>(1)
                {
                    this.LocTable_LocSheet_Items
                };
            }

            /// <inheritdoc cref="g__ETDBA.IPostExportDatabase.PostExport(g__ETDBA.DatabaseExportingContext)"/>
            public void PostExport(g__ETDBA.DatabaseExportingContext context)
            {
                OnPostExport(context);
            }

            /// <inheritdoc cref="g__ETDBA.IPostExportDatabase.PostExport(g__ETDBA.DatabaseExportingContext)"/>
            partial void OnPostExport(g__ETDBA.DatabaseExportingContext context);

        }

#else

        [g__S.Serializable]
        [g__ETDBASG.GeneratedSheetContainer]
        [g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class SheetContainer { }

#endif

        [g__S.Serializable]
        [g__ETDBASG.TableNaming("Items", g__ETN.NameCasing.Pascal)]
        [g__ETDBASG.GeneratedSheet(typeof(int), typeof(global::TestProject.Loc), typeof(global::TestProject.LocTable), "LocTable")]
        [g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class LocTable_LocSheet_Items : LocSheet { }

    }


}

