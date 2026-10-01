namespace EncosyTower.SourceGen.Tests.Data;

internal static class DataAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Data
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class DataAttribute : System.Attribute { }

            [System.Flags]
            public enum DataMutableOptions
            {
                Default = 0,
                WithoutPropertySetters = 1,
                WithReadOnlyView = 2,
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class DataMutableAttribute : System.Attribute
            {
                public DataMutableAttribute(DataMutableOptions options = DataMutableOptions.Default) { }
                public DataMutableOptions Options { get; }
            }

            public enum DataFieldPolicy
            {
                Private = 0,
                Internal = 1,
                Public = 2,
            }

            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class DataFieldPolicyAttribute : System.Attribute
            {
                public DataFieldPolicyAttribute(DataFieldPolicy policy) { }
                public DataFieldPolicy Policy { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
            public sealed class DataPropertyAttribute : System.Attribute
            {
                public DataPropertyAttribute() { }
                public DataPropertyAttribute(System.Type fieldType) { }
                public System.Type FieldType { get; }
            }

        }

        namespace UnityEngine
        {
            [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
            public sealed class SerializeField : System.Attribute { }
        }

        namespace EncosyTower.Collections
        {
            public struct ListFast<T>
            {
                public readonly struct ReadOnly { }
            }

            public readonly struct HashSetReadOnly<T> { }

            public readonly struct DictionaryReadOnly<TKey, TValue> { }
        }

        """;
}
