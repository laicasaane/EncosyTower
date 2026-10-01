namespace EncosyTower.SourceGen.Tests.Entities.TypeHandles;

internal static class TypeHandleAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace Unity.Entities
        {
            public interface IComponentData { }

            public interface IBufferElementData { }

            public interface ISharedComponentData { }
        }

        namespace EncosyTower.Entities
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class TypeHandleAttribute : System.Attribute
            {
                public TypeHandleAttribute(System.Type type) { }
                public TypeHandleAttribute(System.Type type, bool isReadOnly) { }
            }
        }
        """;
}
