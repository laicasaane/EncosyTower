namespace EncosyTower.SourceGen.Tests.Core.EnumExtensions;

internal static class EnumExtensionsAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Class)]
            public class EnumExtensionsForAttribute : System.Attribute
            {
                public EnumExtensionsForAttribute(System.Type enumType) { }
                public System.Type EnumType { get; }
            }
        }
        """;
}
