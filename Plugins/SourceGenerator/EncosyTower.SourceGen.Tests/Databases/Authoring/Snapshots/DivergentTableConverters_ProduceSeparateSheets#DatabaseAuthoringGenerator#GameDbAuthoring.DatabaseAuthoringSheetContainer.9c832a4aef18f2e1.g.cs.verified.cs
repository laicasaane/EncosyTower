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
        [g__SCDC.GeneratedCode("EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class SheetContainer : g__ETDBA.DataSheetContainerBase, g__ETDBA.IPostExportDatabase
        {
            public SheetContainer() : this(g__CBSU.UnityLogger.Default)
            { }

            public SheetContainer(g__MEL.ILogger logger) : base(logger)
            {
                this.LocTableA_LocSheet_0_TableA = new LocTableA_LocSheet_0_TableA();
                this.LocTableB_LocSheet_1_TableB = new LocTableB_LocSheet_1_TableB();
            }

            public LocTableA_LocSheet_0_TableA LocTableA_LocSheet_0_TableA { get; set; }

            public LocTableB_LocSheet_1_TableB LocTableB_LocSheet_1_TableB { get; set; }

            public g__SCG.List<LocSheet_0> LocSheet_0s
            {
                get => new g__SCG.List<LocSheet_0>(1)
                {
                    this.LocTableA_LocSheet_0_TableA
                };
            }

            public g__SCG.List<LocSheet_1> LocSheet_1s
            {
                get => new g__SCG.List<LocSheet_1>(1)
                {
                    this.LocTableB_LocSheet_1_TableB
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
        [g__ETDBASG.TableNaming("TableA", g__ETN.NameCasing.Pascal)]
        [g__ETDBASG.GeneratedSheet(typeof(int), typeof(global::TestProject.Loc), typeof(global::TestProject.LocTableA), "LocTableA")]
        [g__SCDC.GeneratedCode("EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class LocTableA_LocSheet_0_TableA : LocSheet_0 { }

        [g__S.Serializable]
        [g__ETDBASG.TableNaming("TableB", g__ETN.NameCasing.Pascal)]
        [g__ETDBASG.GeneratedSheet(typeof(int), typeof(global::TestProject.Loc), typeof(global::TestProject.LocTableB), "LocTableB")]
        [g__SCDC.GeneratedCode("EncosyTower.Databases.Authoring.Generators.DatabaseAuthoringGenerator", "0.1.8-preview.1")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public partial class LocTableB_LocSheet_1_TableB : LocSheet_1 { }

    }


}

