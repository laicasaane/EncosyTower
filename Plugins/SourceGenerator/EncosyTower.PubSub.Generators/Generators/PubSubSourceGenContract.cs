namespace EncosyTower.PubSub.Generators
{
    internal static class PubSubSourceGenContract
    {
        public const string NAMESPACE = "EncosyTower.PubSub";
        public const string PUBSUB_ATTRIBUTE = NAMESPACE + ".PubSubAttribute";
        public const string MESSAGE = NAMESPACE + ".IMessage";
        public const string PUBLISHER_OF_T = NAMESPACE + ".MessagePublisher+Publisher`1";
        public const string UNITY_PUBLISHER_OF_T = NAMESPACE + ".MessagePublisher+UnityPublisher`1";
        public const string GLOBAL_SCOPE = "EncosyTower.Common.GlobalScope";
        public const string SKIP_ATTRIBUTE = "global::" + NAMESPACE + ".SkipSourceGeneratorsForAssemblyAttribute";
        public const string UNITY_OBJECT = "UnityEngine.Object";
        public const string GENERATOR_METADATA_NAME =
            "EncosyTower.PubSub.Generators.PubSubMessageGenerator";
        public const string MESSAGE_ROLE = "PubSubMessage";
        public const string SCOPE_ROLE = "PubSubScope";
    }
}
