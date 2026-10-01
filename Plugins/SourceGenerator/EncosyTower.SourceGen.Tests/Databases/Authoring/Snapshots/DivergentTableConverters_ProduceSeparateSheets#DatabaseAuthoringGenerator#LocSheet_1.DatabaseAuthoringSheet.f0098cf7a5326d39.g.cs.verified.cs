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

    partial struct GameDbAuthoring : g__ETDBASG.IContains<GameDbAuthoring.LocSheet_1>
    {
#if DATABASE_AUTHORING && BAKING_SHEET

        [g__S.Serializable]
        [g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public abstract partial class LocSheet_1 : g__CBS.Sheet<int, LocSheet_1.__Loc>, g__ETDBA.IDataSheet, g__ETDBA.IToDataArray<global::TestProject.Loc>
        {
            /// <inheritdoc cref="g__ETDBA.IDataSheet.Preprocess(g__CBS.SheetConvertingContext)"/>
            public void Preprocess(g__CBS.SheetConvertingContext context)
            {
                OnPreprocess(context);
            }

            /// <inheritdoc cref="g__ETDBA.IDataSheet.Preprocess(g__CBS.SheetConvertingContext)"/>
            partial void OnPreprocess(g__CBS.SheetConvertingContext context);

            /// <inheritdoc cref="g__ETDBA.IDataSheet.Process(g__CBS.SheetConvertingContext)"/>
            public void Process(g__CBS.SheetConvertingContext context)
            {
                OnProcess(context);
            }

            /// <inheritdoc cref="g__ETDBA.IDataSheet.Process(g__CBS.SheetConvertingContext)"/>
            partial void OnProcess(g__CBS.SheetConvertingContext context);

            /// <inheritdoc cref="g__ETDBA.IDataSheet.Postprocess(g__CBS.SheetConvertingContext)"/>
            public void Postprocess(g__CBS.SheetConvertingContext context)
            {
                OnPostprocess(context);
            }

            /// <inheritdoc cref="g__ETDBA.IDataSheet.Postprocess(g__CBS.SheetConvertingContext)"/>
            partial void OnPostprocess(g__CBS.SheetConvertingContext context);

            public global::TestProject.Loc[] ToDataArray()
            {
                if (this.Items == null || this.Count == 0)
                    return new global::TestProject.Loc[0];

                var rows = this.Items;
                var count = this.Count;
                var result = new global::TestProject.Loc[count];

                for (var i = 0; i < count; i++)
                {
                    result[i] = (rows[i] ?? __Loc.Default).ToLoc();
                }

                return result;
            }

            [g__S.Serializable]
            [g__ETDBASG.GeneratedSheetRow(typeof(int), typeof(global::TestProject.Loc))]
            [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.7-preview.3")]
            public partial class __Loc : g__CBS.SheetRow<int>
            {
                public static readonly __Loc Default = new __Loc();

                public __Loc()
                {
                    this.Id = default;
                    this.Points = string.Empty;

                    OnConstructor();
                }

                partial void OnConstructor();

                public string Points { get; set; }

                public global::TestProject.Loc ToLoc()
                {
                    var result = new global::TestProject.Loc();

                    global::TestProject.Loc.I_TestProject_x002ELoc_ValueSetter.Set_Id(ref result, this.Id);
                    global::TestProject.Loc.I_TestProject_x002ELoc_ValueSetter.Set_Points(ref result, global::TestProject.ConvB.Convert(this.Points ?? string.Empty));

                    return result;
                }

            }

        }

#else

        [g__S.Serializable]
        [g__SCDC.GeneratedCode("EncosyTower.Data.Generators.Databases.DatabaseGenerator", "0.1.7-preview.3")]
        [g__SDCA.ExcludeFromCodeCoverage]
        public abstract partial class LocSheet_1 { }

#endif

    }


}

