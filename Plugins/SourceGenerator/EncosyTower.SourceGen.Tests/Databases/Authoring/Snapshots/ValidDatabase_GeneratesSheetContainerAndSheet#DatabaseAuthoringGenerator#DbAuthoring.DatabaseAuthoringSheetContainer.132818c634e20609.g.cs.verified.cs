#pragma warning disable 0219
#define DATABASE_AUTHORING
#define BAKING_SHEET

using EncosyTower.Collections;
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

    partial struct DbAuthoring : g__ETDBASG.IContains<DbAuthoring.SheetContainer>
    {
#if DATABASE_AUTHORING && BAKING_SHEET

        [g__S.Serializable]
        [g__ETDBASG.GeneratedSheetContainer]
        [g__SCDC.GeneratedCode("EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class SheetContainer : g__ETDBA.DataSheetContainerBase, g__ETDBA.IPostExportDatabase
        {
            public SheetContainer() : this(g__CBSU.UnityLogger.Default)
            { }

            public SheetContainer(g__MEL.ILogger logger) : base(logger)
            {
                this.Rows_RowSheet_Items = new Rows_RowSheet_Items();
            }

            public Rows_RowSheet_Items Rows_RowSheet_Items { get; set; }

            public g__SCG.List<RowSheet> RowSheets
            {
                get => new g__SCG.List<RowSheet>(1)
                {
                    this.Rows_RowSheet_Items
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
        [g__SCDC.GeneratedCode("EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class SheetContainer { }

#endif

        [g__S.Serializable]
        [g__ETDBASG.TableNaming("Items", g__ETN.NameCasing.Pascal)]
        [g__ETDBASG.GeneratedSheet(typeof(int), typeof(global::TestProject.Row), typeof(global::TestProject.Rows), "Rows")]
        [g__SCDC.GeneratedCode("EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class Rows_RowSheet_Items : RowSheet { }

    }


}

