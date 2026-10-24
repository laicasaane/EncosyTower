using System.Collections.Generic;
using EncosyTower.Data;
using EncosyTower.Databases;
using EncosyTower.Naming;

namespace EncosyTower.Samples.Data
{
    [Data]
    public partial struct BakingSheetFeatureId
    {
        [DataProperty] public readonly string Kind => Get_Kind();

        [DataProperty] public readonly int SubId => Get_SubId();
    }

    [Data]
    public partial struct BakingSheetRewardData
    {
        [DataProperty] public readonly int Amount => Get_Amount();
    }

    [Data]
    public partial struct BakingSheetFeatureData
    {
        [DataProperty] public readonly BakingSheetFeatureId Id => Get_Id();

        [DataProperty]
        public readonly Dictionary<string, List<BakingSheetRewardData>> Rewards => Get_Rewards();
    }

    [DataTableAsset]
    public sealed partial class BakingSheetFeatureTableAsset
        : DataTableAssetBase<BakingSheetFeatureId, BakingSheetFeatureData>
    {
    }

    public readonly partial struct SampleDatabase
    {
        [Table(NameCasing.Pascal)]
        public readonly BakingSheetFeatureTableAsset BakingSheetFeatures => Get_BakingSheetFeatures();

        [Table(NameCasing.Pascal)]
#if UNITY_EDITOR || ENCOSY_INCLUDE_AUTHORING
        [EncosyTower.Databases.Authoring.Horizontal(
              typeof(BakingSheetFeatureData)
            , nameof(BakingSheetFeatureData.Rewards)
        )]
        [EncosyTower.Databases.Authoring.Transpose]
#endif
        public readonly BakingSheetFeatureTableAsset HorizontalBakingSheetFeatures
            => Get_HorizontalBakingSheetFeatures();
    }
}

#if UNITY_EDITOR && BAKING_SHEET

namespace EncosyTower.Samples.DatabaseAuthoring
{
    using System.IO;
    using Cathei.BakingSheet;
    using Cathei.BakingSheet.Unity;
    using EncosyTower.Databases.Authoring;
    using UnityEditor;

    public static class BakingSheetFeatures
    {
        [MenuItem("Encosy Tower/Samples/Bake BakingSheet Features")]
        public static async void Bake()
        {
            var guids = AssetDatabase.FindAssets("BakingSheetFeatures t:TextAsset");

            if (guids.Length != 1)
            {
                return;
            }

            var csvPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var inputPath = Path.GetDirectoryName(csvPath);
            var outputPath = Path.Combine(inputPath, "RuntimeAsset.Tables");
            var container = new SampleDatabaseAuthoring.SheetContainer();
            var context = new SheetConvertingContext {
                Container = container,
                Logger = UnityLogger.Default,
            };
            var importer = new DatabaseCsvSheetConverter(inputPath, includeSubFolders: false);

            if (await importer.Import(context) == false)
            {
                return;
            }

            var exporter = new DatabaseAssetExporter(outputPath, "BakingSheetFeatureDatabase");
            await exporter.Export(context);
        }
    }
}

#endif
