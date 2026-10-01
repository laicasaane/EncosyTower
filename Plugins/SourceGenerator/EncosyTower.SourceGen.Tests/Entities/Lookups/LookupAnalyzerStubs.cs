namespace EncosyTower.SourceGen.Tests.Entities.Lookups;

internal static class LookupAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace Unity.Entities
        {
            public interface IComponentData { }

            public interface IBufferElementData { }

            public interface IEnableableComponent { }
        }

        namespace EncosyTower.Entities
        {
            public interface IBufferLookups { }

            public interface IComponentLookups { }

            public interface IEnableableBufferLookups { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class LookupAttribute : System.Attribute
            {
                public System.Type Type { get; }
                public bool IsReadOnly { get; }

                public LookupAttribute(System.Type type)
                {
                    Type = type;
                    IsReadOnly = false;
                }

                public LookupAttribute(System.Type type, bool isReadOnly)
                {
                    Type = type;
                    IsReadOnly = isReadOnly;
                }
            }
        }
        """;
}
