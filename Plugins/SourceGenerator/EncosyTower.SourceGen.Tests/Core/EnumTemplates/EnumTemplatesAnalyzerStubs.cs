namespace EncosyTower.SourceGen.Tests.Core.EnumTemplates;

internal static class EnumTemplatesAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.EnumExtensions
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public sealed class EnumTemplateAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class EnumTemplateMembersFromEnumAttribute : System.Attribute
            {
                public EnumTemplateMembersFromEnumAttribute(System.Type enumType, ulong order) { }
                public System.Type EnumType { get; }
                public ulong Order { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class EnumTemplateMemberFromTypeAttribute : System.Attribute
            {
                public EnumTemplateMemberFromTypeAttribute(
                      System.Type type
                    , ulong order
                    , string displayName = ""
                    , string alternateName = ""
                ) { }
                public System.Type Type { get; }
                public ulong Order { get; }
                public string DisplayName { get; }
                public string AlternateName { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Enum, AllowMultiple = true)]
            public sealed class EnumMembersForTemplateAttribute : System.Attribute
            {
                public EnumMembersForTemplateAttribute(System.Type templateType, ulong order) { }
                public System.Type TemplateType { get; }
                public ulong Order { get; }
            }
        }
        """;
}
