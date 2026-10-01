namespace EncosyTower.Data.Generators.DataTableAssets
{
    public static class Helpers
    {
        public const string GENERATOR_NAME = "DataTableAssetGenerator";

        public const string NAMESPACE = "EncosyTower.Databases";
        public const string IDATA = $"global::{NAMESPACE}.IData";
        public const string DATA_TABLE_ASSET = $"global::{NAMESPACE}.DataTableAsset";
        public const string DATA_TABLE_ASSET_ATTRIBUTE = $"{NAMESPACE}.DataTableAssetAttribute";
        public const string DATABASE_ATTRIBUTE = $"global::{NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{NAMESPACE}.TableAttribute";
        public const string HORIZONTAL_LIST_ATTRIBUTE = $"global::{NAMESPACE}.HorizontalAttribute";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";

        public const string IINITIALIZABLE = "global::EncosyTower.Initialization.IInitializable";

        public const string PR_AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Data.Generators.DataTableAssets.DataTableAssetGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string PR_EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
    }
}
