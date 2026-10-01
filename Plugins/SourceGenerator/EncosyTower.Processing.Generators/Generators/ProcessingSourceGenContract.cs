namespace EncosyTower.Processing.Generators
{
    internal static class ProcessingSourceGenContract
    {
        public const string NAMESPACE = "EncosyTower.Processing";
        public const string PROCESSING_ATTRIBUTE = NAMESPACE + ".ProcessingAttribute";
        public const string REQUEST = NAMESPACE + ".IRequest";
        public const string REQUEST_OF_T = NAMESPACE + ".IRequest`1";
        public const string ASYNC_REQUEST = NAMESPACE + ".IAsyncRequest";
        public const string ASYNC_REQUEST_OF_T = NAMESPACE + ".IAsyncRequest`1";
        public const string HUB_OF_T = NAMESPACE + ".Processor+Hub`1";
        public const string HUB_OF_T_STATE = NAMESPACE + ".Processor+Hub`2";
        public const string UNITY_HUB_OF_T = NAMESPACE + ".Processor+UnityHub`1";
        public const string UNITY_HUB_OF_T_STATE = NAMESPACE + ".Processor+UnityHub`2";
        public const string PROCESSOR_EXTENSIONS = NAMESPACE + ".ProcessorExtensions";
        public const string GLOBAL_SCOPE = "EncosyTower.Common.GlobalScope";
        public const string SKIP_ATTRIBUTE = "global::" + NAMESPACE + ".SkipSourceGeneratorsForAssemblyAttribute";
        public const string UNITY_OBJECT = "UnityEngine.Object";
        public const string GENERATOR_METADATA_NAME
            = "EncosyTower.Processing.Generators.ProcessingRequestGenerator";
        public const string REQUEST_ROLE = "ProcessingRequest";
        public const string SCOPE_ROLE = "ProcessingScope";
    }
}
