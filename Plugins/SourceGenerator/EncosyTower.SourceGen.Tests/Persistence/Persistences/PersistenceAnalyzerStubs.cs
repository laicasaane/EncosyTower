namespace EncosyTower.SourceGen.Tests.Persistence.Persistences;

internal static class PersistenceAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Persistences
        {
            public interface IPersist { }

            public interface IPersistAccessor { }

            public abstract class PersistStoreBase<TData> where TData : IPersist { }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public sealed class PersistAccessorAttribute : System.Attribute
            {
                public PersistAccessorAttribute(System.Type persistenceType) { }
                public System.Type PersistenceType { get; }
            }
        }
        """;
}
