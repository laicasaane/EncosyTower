namespace EncosyTower.PageFlows.Generators
{
    internal static class PageFlowsAliasSet
    {
        public const string SYSTEM = "g__S";
        public const string CODE_DOM_COMPILER = "g__SCDC";
        public const string PAGE_FLOWS = "g__ETPF";

        public static void WriteAliases(ref Printer printer)
        {
            printer.PrintLine("using g__S = global::System;");
            printer.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            printer.PrintLine("using g__ETPF = global::EncosyTower.PageFlows;");
        }
    }
}
