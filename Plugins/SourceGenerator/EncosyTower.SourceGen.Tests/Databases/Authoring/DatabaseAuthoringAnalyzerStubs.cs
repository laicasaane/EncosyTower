namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

internal static class DatabaseAuthoringAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Data
        {
            public interface IData { }

            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class DataPropertyAttribute : System.Attribute { }
        }

        namespace EncosyTower.Data.Authoring
        {
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class DataAuthoringConverterAttribute : System.Attribute
            {
                public DataAuthoringConverterAttribute(System.Type type) { }
                public System.Type Type { get; }
            }
        }

        namespace EncosyTower.Databases
        {
            public abstract class DataTableAssetBase<TDataId, TData>
                : DataTableAssetBase<TDataId, TData, TDataId> { }

            public abstract class DataTableAssetBase<TDataId, TData, TConvertedId> { }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false)]
            public sealed class DatabaseAttribute : System.Attribute
            {
                public DatabaseAttribute(params System.Type[] converters) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class TableAttribute : System.Attribute
            {
                public TableAttribute(params System.Type[] converters) { }
            }
        }

        namespace EncosyTower.Databases.Authoring
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
            public sealed class AuthorDatabaseAttribute : System.Attribute
            {
                public AuthorDatabaseAttribute(System.Type databaseType, params System.Type[] converters) { }
                public System.Type DatabaseType { get; }
                public System.Type[] Converters { get; }
                public bool FullyQualifiedSheetNames { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
            public sealed class ConverterForTableAttribute : System.Attribute
            {
                public ConverterForTableAttribute(string tableName, System.Type converterType) { }
                public ConverterForTableAttribute(System.Type tableType, System.Type converterType) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
            public sealed class ConverterForDataPropertyAttribute : System.Attribute
            {
                public ConverterForDataPropertyAttribute(System.Type dataType, string propertyName, System.Type converterType) { }
                public ConverterForDataPropertyAttribute(System.Type dataType, string propertyName, System.Type converterType, string tableName) { }
                public ConverterForDataPropertyAttribute(System.Type dataType, string propertyName, System.Type converterType, System.Type tableType) { }
            }

            [System.AttributeUsage(
                  System.AttributeTargets.Property | System.AttributeTargets.Field
                , AllowMultiple = true
            )]
            public sealed class HorizontalAttribute : System.Attribute
            {
                public HorizontalAttribute(System.Type targetType, string propertyName) { }
            }
        }
        """;
}
