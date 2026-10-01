namespace EncosyTower.Data.Generators.Databases
{
    public static class Helpers
    {
        public const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        public const string DATA_TABLE_ASSET = $"global::{DATABASES_NAMESPACE}.DataTableAsset";
        public const string SKIP_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string PR_DATABASE_ASSET = "g__ETDB.DatabaseAsset";

        public const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";

        public const string PR_AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        public const string PR_GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Data.Generators.Databases.DatabaseGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string PR_GENERATED_ASSET_NAME = $"[g__ETDBSG.GeneratedAssetNameConstant(typeof({{0}}), typeof({{1}}))]";
        public const string PR_STRUCT_LAYOUT_AUTO = "[g__SRIS.StructLayout(g__SRIS.LayoutKind.Auto)]";
        public const string PR_SERIALIZABLE = "[g__S.Serializable]";

        public const string DATA_NAMESPACE = "EncosyTower.Data";
        public const string DATA_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataAttribute";
        public const string IDATA = $"global::{DATA_NAMESPACE}.IData";
    }
}
