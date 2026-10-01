namespace EncosyTower.SourceGen.Tests.Core.NewtonsoftAotHelpers;

internal static class NewtonsoftJsonAotHelperAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Serialization.NewtonsoftJson
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class NewtonsoftJsonAotHelperAttribute : System.Attribute
            {
                public NewtonsoftJsonAotHelperAttribute() { }
                public NewtonsoftJsonAotHelperAttribute(System.Type baseType) { }
                public System.Type BaseType { get; }
            }
        }
        """;
}
