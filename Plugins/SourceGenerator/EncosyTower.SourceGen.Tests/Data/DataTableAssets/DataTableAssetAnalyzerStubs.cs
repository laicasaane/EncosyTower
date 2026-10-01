namespace EncosyTower.SourceGen.Tests.Data.DataTableAssets;

internal static class DataTableAssetAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Databases
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class DataTableAssetAttribute : System.Attribute { }

            public abstract class DataTableAsset<TDataId, TData> { }

            public abstract class DataTableAsset<TDataId, TData, TConvertedId> { }
        }
        """;
}
