namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

internal static class PolyEnumStructsAnalyzerStubs
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
        }
        """;
}
