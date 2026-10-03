namespace EncosyTower.Processing.Generators
{
    internal static class ProcessingAliasSet
    {
        public const string SYSTEM = "g__S";
        public const string CODE_DOM_COMPILER = "g__SCDC";
        public const string DIAGNOSTICS = "g__SD";
        public const string CODE_ANALYSIS = "g__SCA";
        public const string RUNTIME_COMPILER_SERVICES = "g__SR";
        public const string RUNTIME = "g__ETP";
        public const string LOGGING = "g__ETL";
        public const string TASKS = "g__ETT";
        public const string COMMON = "g__ETC";

        public static void WriteAliases(ref Printer printer)
        {
            printer.PrintLine("using g__S = global::System;");
            printer.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            printer.PrintLine("using g__SD = global::System.Diagnostics;");
            printer.PrintLine("using g__SCA = global::System.Diagnostics.CodeAnalysis;");
            printer.PrintLine("using g__SR = global::System.Runtime.CompilerServices;");
            printer.PrintLine("using g__ETP = global::EncosyTower.Processing;");
            printer.PrintLine("using g__ETL = global::EncosyTower.Logging;");
            printer.PrintLine("using g__ETT = global::EncosyTower.Tasks;");
            printer.PrintLine("using g__ETC = global::EncosyTower.Common;");
        }
    }
}
