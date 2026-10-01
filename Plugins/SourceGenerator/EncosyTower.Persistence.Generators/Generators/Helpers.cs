namespace EncosyTower.Persistence.Generators
{
    internal static class Helpers
    {
        public const string NAMESPACE = "EncosyTower.Persistences";
        public const string SKIP_ATTRIBUTE = $"global::{NAMESPACE}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string PERSIST_ATTRIBUTE = $"global::{NAMESPACE}.PersistAttribute";
        public const string PERSISTENCE_ATTRIBUTE = $"global::{NAMESPACE}.PersistenceAttribute";
        public const string ACCESSOR_ATTRIBUTE = $"global::{NAMESPACE}.PersistAccessorAttribute";
        public const string PERSIST_ATTRIBUTE_METADATA = $"{NAMESPACE}.PersistAttribute";
        public const string PERSISTENCE_ATTRIBUTE_METADATA = $"{NAMESPACE}.PersistenceAttribute";
        public const string ACCESSOR_ATTRIBUTE_METADATA = $"{NAMESPACE}.PersistAccessorAttribute";
        public const string IPERSIST = $"global::{NAMESPACE}.IPersist";
        public const string IPERSIST_ACCESSOR = $"global::{NAMESPACE}.IPersistAccessor";
        public const string PERSIST_STORE_BASE = $"global::{NAMESPACE}.PersistStoreBase<";
        public const string STORE_BASE = "g__ETP.PersistStoreBase<";
        public const string ENCRYPTION_BASE = "g__ETE.EncryptionBase";
        public const string STRING_VAULT = "g__ETS.StringVault";
        public const string ILOGGER = "g__ETL.ILogger";
        public const string TASK_ARRAY_POOL = "g__SB.ArrayPool<g__ETT.UnityTask>";
        public const string STORE_ARGS = "g__ETP.PersistStoreArgs";
        public const string COMPLETED_TASK = "g__ETT.UnityTask.CompletedTask";
        public const string WHEN_ALL_TASKS = "g__ETT.UnityTask.WhenAll(tasks)";
        public const string NOT_NULL = "[g__SDCA.NotNull]";
        public const string STRING_ID = "g__ETS.StringId<string>";
        public const string GENERATED_CODE = $"[g__SCDC.GeneratedCode({GENERATOR}, \"{SourceGenVersion.VALUE}\")]";
        public const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        public const string AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]";
        public const string GENERATOR = "\"EncosyTower.Persistence.Generators.PersistenceGenerator\"";
        public const string HIDE_IN_CALL_STACK = "[g__UE.HideInCallstack, g__SD.StackTraceHidden]";
    }
}
