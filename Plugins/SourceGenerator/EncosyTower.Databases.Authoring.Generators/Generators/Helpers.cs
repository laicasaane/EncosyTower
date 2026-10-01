namespace EncosyTower.Databases.Authoring.Generators
{
    public static class Helpers
    {
        public const string DATABASES_NAMESPACE = "EncosyTower.Databases";
        public const string DATA_TABLE_ASSET = $"global::{DATABASES_NAMESPACE}.DataTableAsset";
        public const string SKIP_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string IDATABASE = $"global::{DATABASES_NAMESPACE}.IDatabase";
        public const string DATABASE_ASSET = $"global::{DATABASES_NAMESPACE}.DatabaseAsset";
        public const string DATABASE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = $"global::{DATABASES_NAMESPACE}.TableAttribute";

        public const string DEFINE_DATABASE_AUTHORING = "#define DATABASE_AUTHORING";
        public const string DEFINE_BAKING_SHEET = "#define BAKING_SHEET";

        public const string DEFINE_NO_DATABASE_AUTHORING = "#define __NO_DATABASE_AUTHORING__";
        public const string DEFINE_NO_BAKING_SHEET = "#define __NO_BAKING_SHEET__";

        public const string DIRECTIVE = "#if DATABASE_AUTHORING && BAKING_SHEET";

        public const string DATA_NAMESPACE = "EncosyTower.Data";
        public const string IDATA = $"global::{DATA_NAMESPACE}.IData";
        public const string DATA_ATTRIBUTE = $"{DATA_NAMESPACE}.DataAttribute";
        public const string PROPERTY_TYPE_ATTRIBUTE = $"global::{DATA_NAMESPACE}.PropertyTypeAttribute";
        public const string DATA_PROPERTY_ATTRIBUTE = $"global::{DATA_NAMESPACE}.DataPropertyAttribute";
        public const string DATA_AUTHORING_CONVERTER_ATTRIBUTE = $"global::{DATA_NAMESPACE}.Authoring.DataAuthoringConverterAttribute";
        public const string DATA_MANUAL_AUTHORING_ATTRIBUTE = $"global::{DATA_NAMESPACE}.Authoring.DataManualAuthoringAttribute";

        public const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";

        public const string LIST_FAST_TYPE_T = "global::EncosyTower.Collections.ListFast<";
        public const string LIST_TYPE_T = "global::System.Collections.Generic.List<";
        public const string DICTIONARY_TYPE_T = "global::System.Collections.Generic.Dictionary<";
        public const string DICTIONARY_READ_ONLY_TYPE_T = "global::EncosyTower.Collections.DictionaryReadOnly<";
        public const string HASH_SET_TYPE_T = "global::System.Collections.Generic.HashSet<";
        public const string HASH_SET_READ_ONLY_TYPE_T = "global::EncosyTower.Collections.HashSetReadOnly<";
        public const string QUEUE_TYPE_T = "global::System.Collections.Generic.Queue<";
        public const string STACK_TYPE_T = "global::System.Collections.Generic.Stack<";
        public const string VERTICAL_LIST_TYPE = "VerticalList<";

        public const string IREADONLY_LIST_TYPE_T = "global::System.Collections.Generic.IReadOnlyList<";
        public const string ILIST_TYPE_T = "global::System.Collections.Generic.IList<";
        public const string ISET_TYPE_T = "global::System.Collections.Generic.ISet<";
        public const string IREADONLY_DICTIONARY_TYPE_T = "global::System.Collections.Generic.IReadOnlyDictionary<";
        public const string IDICTIONARY_TYPE_T = "global::System.Collections.Generic.IDictionary<";
        public const string READONLY_MEMORY_TYPE_T = "global::System.ReadOnlyMemory<";
        public const string READONLY_SPAN_TYPE_T = "global::System.ReadOnlySpan<";
        public const string MEMORY_TYPE_T = "global::System.Memory<";
        public const string SPAN_TYPE_T = "global::System.Span<";

        public const string GENERATED_PROPERTY_FROM_FIELD = $"global::{DATA_NAMESPACE}.SourceGen.GeneratedPropertyFromFieldAttribute";
        public const string GENERATED_FIELD_FROM_PROPERTY = $"global::{DATA_NAMESPACE}.SourceGen.GeneratedFieldFromPropertyAttribute";

        public const string DATABASES_AUTHORING_NAMESPACE = $"{DATABASES_NAMESPACE}.Authoring";
        public const string AUTHOR_DATABASE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.AuthorDatabaseAttribute";
        public const string CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.ConverterForDataPropertyAttribute";
        public const string CONVERTER_FOR_TABLE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.ConverterForTableAttribute";
        public const string HORIZONTAL_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.HorizontalAttribute";
        public const string TRANSPOSE_ATTRIBUTE = $"global::{DATABASES_AUTHORING_NAMESPACE}.TransposeAttribute";

        public const string PR_AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string PR_EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        public const string PR_GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Data.Generators.Databases.DatabaseGenerator\", \"{SourceGenVersion.VALUE}\")]";
        public const string PR_SERIALIZABLE = "[g__S.Serializable]";
        public const string PR_STRUCT_LAYOUT_AUTO = "[g__SRIS.StructLayout(g__SRIS.LayoutKind.Auto)]";

        public const string PR_LIST_T = "g__SCG.List";
        public const string PR_HASH_SET_T = "g__SCG.HashSet";
        public const string PR_QUEUE_T = "g__SCG.Queue";
        public const string PR_STACK_T = "g__SCG.Stack";
        public const string PR_DICTIONARY_T = "g__SCG.Dictionary";
        public const string PR_VERTICAL_LIST_T = "g__CBS.VerticalList";
        public const string PR_VERTICAL_DICTIONARY_T = "g__CBS.VerticalDictionary";

        public const string PR_ICONTAINS = $"g__ETDBASG.IContains";
        public const string PR_AUTHOR_DATABASE_ATTRIBUTE = $"g__ETDBA.AuthorDatabaseAttribute";
        public const string PR_DATA_SHEET_CONTAINER_BASE = $"g__ETDBA.DataSheetContainerBase";
        public const string PR_TABLE_NAMING = $"[g__ETDBASG.TableNaming(\"{{0}}\", g__ETN.NameCasing.{{1}})]";
        public const string PR_GENERATED_SHEET_CONTAINER = $"[g__ETDBASG.GeneratedSheetContainer]";
        public const string PR_GENERATED_SHEET_ATTRIBUTE = $"[g__ETDBASG.GeneratedSheet(typeof({{0}}), typeof({{1}}), typeof({{2}}), \"{{3}}\")]";
        public const string PR_GENERATED_SHEET_ROW = $"[g__ETDBASG.GeneratedSheetRow(typeof({{0}}), typeof({{1}}))]";
        public const string PR_GENERATED_DATA_ROW = $"[g__ETDBASG.GeneratedDataRow(typeof({{0}}))]";
    }
}
