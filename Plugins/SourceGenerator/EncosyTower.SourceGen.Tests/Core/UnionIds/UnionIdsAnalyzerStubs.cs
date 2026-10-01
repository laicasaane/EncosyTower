namespace EncosyTower.SourceGen.Tests.Core.UnionIds;

internal static class UnionIdsAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.UnionIds
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public sealed class UnionIdAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class UnionIdKindAttribute : System.Attribute
            {
                public UnionIdKindAttribute(
                      System.Type kindType
                    , ulong order
                    , string name = ""
                    , string displayName = ""
                    , bool signed = false
                ) { }
                public System.Type KindType { get; }
                public ulong Order { get; }
                public string Name { get; }
                public string DisplayName { get; }
                public bool Signed { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Enum)]
            public sealed class KindForUnionIdAttribute : System.Attribute
            {
                public KindForUnionIdAttribute(
                      System.Type idType
                    , ulong order
                    , string name = ""
                    , string displayName = ""
                    , bool signed = false
                ) { }
                public System.Type IdType { get; }
                public ulong Order { get; }
                public string Name { get; }
                public string DisplayName { get; }
                public bool Signed { get; }
            }
        }
        """;
}
