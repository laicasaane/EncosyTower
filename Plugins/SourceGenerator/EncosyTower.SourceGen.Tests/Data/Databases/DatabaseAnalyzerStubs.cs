namespace EncosyTower.SourceGen.Tests.Data.Databases;

internal static class DatabaseAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Data
        {
            public interface IData { }
        }

        namespace EncosyTower.Databases
        {
            public abstract class DataTableAsset<TDataId, TData> { }

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
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = true)]
            public sealed class HorizontalAttribute : System.Attribute
            {
                public HorizontalAttribute(System.Type targetType, string propertyName) { }
            }

        }
        """;
}
