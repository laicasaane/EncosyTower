namespace EncosyTower.PubSub.Generators
{
    internal static class PubSubAliasSet
    {
        public const string SYSTEM = "g__S";
        public const string CODE_ANALYSIS = "g__SDCA";
        public const string THREADING = "g__ST";
        public const string RUNTIME_COMPILER_SERVICES = "g__SRCS";
        public const string COMMON = "g__ET";
        public const string LOGGING = "g__ETL";
        public const string PUBSUB = "g__ETPS";
        public const string UNITY_EXTENSIONS = "g__ETUE";
        public const string TASKS = "g__ETT";

        public static void WriteAliases(ref Printer printer)
        {
            printer.PrintLine("using g__S = global::System;");
            printer.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            printer.PrintLine("using g__ST = global::System.Threading;");
            printer.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            printer.PrintLine("using g__ET = global::EncosyTower.Common;");
            printer.PrintLine("using g__ETL = global::EncosyTower.Logging;");
            printer.PrintLine("using g__ETPS = global::EncosyTower.PubSub;");
            printer.PrintLine("using g__ETUE = global::EncosyTower.UnityExtensions;");
            printer.PrintLine("using g__ETT = global::EncosyTower.Tasks;");
        }
    }
}
