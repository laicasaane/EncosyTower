namespace EncosyTower.Processing
{
    public static class GlobalProcessor
    {
        private static Processor s_instance;

        public static Processor Instance => s_instance ??= new();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_instance?.Dispose();
            s_instance = null;
        }
#endif
    }
}
