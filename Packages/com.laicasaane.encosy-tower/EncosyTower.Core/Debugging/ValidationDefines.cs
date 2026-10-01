namespace EncosyTower.Debugging
{
    /// <summary>
    /// Compiler symbol aliases and package-wide validation switches.
    /// </summary>
    /// <remarks>
    /// Multiple <c>Conditional</c> attributes on one method combine with OR: the call is kept
    /// when any listed symbol is defined at the call site. When Core is compiled with
    /// <c>DISABLE_ENCOSY_CHECKS</c>, every public symbol alias maps to a reserved, undefined symbol
    /// and the package switches return false. Editor and runtime vetoes affect their respective domains.
    /// </remarks>
    public static class ValidationDefines
    {
        private const string DISABLED_SYMBOL = "__DISABLE_ENCOSY_CHECKS__";

#if DISABLE_ENCOSY_CHECKS
        private const bool PACKAGE_CHECKS_ENABLED = false;
#else
        private const bool PACKAGE_CHECKS_ENABLED = true;
#endif

#if DISABLE_ENCOSY_CHECKS || DISABLE_ENCOSY_RUNTIME_CHECKS
        private const bool PACKAGE_RUNTIME_CHECKS_ENABLED = false;
#else
        private const bool PACKAGE_RUNTIME_CHECKS_ENABLED = true;
#endif

#if DISABLE_ENCOSY_CHECKS || DISABLE_ENCOSY_EDITOR_CHECKS
        private const bool PACKAGE_EDITOR_CHECKS_ENABLED = false;
#else
        private const bool PACKAGE_EDITOR_CHECKS_ENABLED = true;
#endif

#if DISABLE_ENCOSY_CHECKS || DISABLE_ENCOSY_EDITOR_CHECKS
        public const string UNITY_EDITOR = DISABLED_SYMBOL;
        public const string DEBUG = DISABLED_SYMBOL;
#else
        public const string UNITY_EDITOR = "UNITY_EDITOR";
        public const string DEBUG = "DEBUG";
#endif

#if DISABLE_ENCOSY_CHECKS || DISABLE_ENCOSY_RUNTIME_CHECKS
        public const string RUNTIME_CHECKS = DISABLED_SYMBOL;
        public const string COLLECTIONS_CHECKS = DISABLED_SYMBOL;
        public const string UNITY_COLLECTIONS_CHECKS = DISABLED_SYMBOL;
        public const string PUBSUB_CHECKS = DISABLED_SYMBOL;
        public const string PROCESSING_CHECKS = DISABLED_SYMBOL;
        public const string STATS_CHECKS = DISABLED_SYMBOL;
        public const string PERSISTENCE_CHECKS = DISABLED_SYMBOL;
        public const string MVVM_CHECKS = DISABLED_SYMBOL;
#else
        public const string RUNTIME_CHECKS = "ENCOSY_RUNTIME_CHECKS";
        public const string COLLECTIONS_CHECKS = "ENCOSY_COLLECTIONS_RUNTIME_CHECKS";
        public const string UNITY_COLLECTIONS_CHECKS = "ENABLE_UNITY_COLLECTIONS_CHECKS";
        public const string PUBSUB_CHECKS = "ENCOSY_PUBSUB_RUNTIME_CHECKS";
        public const string PROCESSING_CHECKS = "ENCOSY_PROCESSING_RUNTIME_CHECKS";
        public const string STATS_CHECKS = "ENCOSY_STATS_RUNTIME_CHECKS";
        public const string PERSISTENCE_CHECKS = "ENCOSY_PERSISTENCE_RUNTIME_CHECKS";
        public const string MVVM_CHECKS = "ENCOSY_MVVM_RUNTIME_CHECKS";
#endif

        /// <summary>
        /// Whether this Core assembly permits package validation checks.
        /// </summary>
        public static bool PackageChecksEnabled => PACKAGE_CHECKS_ENABLED;

        /// <summary>
        /// Whether this Core assembly permits package runtime checks.
        /// </summary>
        public static bool PackageRuntimeChecksEnabled => PACKAGE_RUNTIME_CHECKS_ENABLED;

        /// <summary>
        /// Whether this Core assembly permits package editor checks.
        /// </summary>
        public static bool PackageEditorChecksEnabled => PACKAGE_EDITOR_CHECKS_ENABLED;
    }
}
