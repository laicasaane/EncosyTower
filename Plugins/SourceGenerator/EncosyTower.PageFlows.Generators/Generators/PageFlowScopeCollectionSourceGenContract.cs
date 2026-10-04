namespace EncosyTower.PageFlows.Generators
{
    internal static class PageFlowScopeCollectionSourceGenContract
    {
        public const string NAMESPACE = "EncosyTower.PageFlows";
        public const string ATTRIBUTE = NAMESPACE + ".PageFlowScopeCollectionAttribute";
        public const string COLLECTION_INTERFACE = NAMESPACE + ".IPageFlowScopeCollection";
        public const string SCOPE = NAMESPACE + ".PageFlowScope";
        public const string SKIP_ATTRIBUTE = "global::" + NAMESPACE + ".SkipSourceGeneratorsForAssemblyAttribute";
        public const string GENERATOR_METADATA_NAME =
            "EncosyTower.PageFlows.Generators.PageFlowScopeCollectionGenerator";
        public const string OUTPUT_ROLE = "PageFlowScopeCollection";
    }
}
