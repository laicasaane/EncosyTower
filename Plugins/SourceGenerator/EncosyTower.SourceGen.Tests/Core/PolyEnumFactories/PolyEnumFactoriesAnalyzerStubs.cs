namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

internal static class PolyEnumFactoriesAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.PolyEnumStructs
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
            public sealed class PolyEnumStructAttribute : System.Attribute
            {
                public bool SortFieldsBySize { get; set; }
                public bool AutoEquatable { get; set; }
                public bool WithEnumExtensions { get; set; }
                public System.Type Container { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class PolyEnumFactoryForAttribute : System.Attribute
            {
                public PolyEnumFactoryForAttribute(System.Type type) { }
                public System.Type Type { get; }
            }
        }
        """;
}
