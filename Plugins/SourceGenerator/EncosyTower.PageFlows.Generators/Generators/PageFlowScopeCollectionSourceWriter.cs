namespace EncosyTower.PageFlows.Generators
{
    internal static class PageFlowScopeCollectionSourceWriter
    {
        private const string GENERATED_CODE = "[" + PageFlowsAliasSet.CODE_DOM_COMPILER + ".GeneratedCode(\""
            + PageFlowScopeCollectionSourceGenContract.GENERATOR_METADATA_NAME + "\", \""
            + SourceGenVersion.VALUE + "\")]";

        public static SourceText Write(in PageFlowScopeCollectionSpec spec, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);

            printer.PrintBeginLine("partial struct ").Print(spec.TypeName).Print(" : ")
                .Print(PageFlowsAliasSet.PAGE_FLOWS).PrintEndLine(".IPageFlowScopeCollection");
            printer.OpenScope();
            {
                WriteScopeIdentifiers(ref printer, spec);
                printer.PrintEndLine();
                WriteTrySetScope(ref printer, spec);
            }
            printer.CloseScope();

            return CreateSource(spec, printer.Result, token);
        }

        private static SourceText CreateSource(
              in PageFlowScopeCollectionSpec spec
            , string body
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var printer = new Printer(0, 1024 * 16, token);
            var source = TypeCreationHelpers.GenerateSourceText(
                  spec.OpeningSource
                , body
                , spec.ClosingSource
                , token
                , overridePrinter: printer
            ).ToString();

            token.ThrowIfCancellationRequested();
            printer.Clear().Print(source.TrimEnd('\r', '\n')).PrintEndLine();
            return SourceText.From(printer.Result, Encoding.UTF8);
        }

        private static void WriteScopeIdentifiers(ref Printer printer, in PageFlowScopeCollectionSpec spec)
        {
            var propertyNames = spec.PropertyNames;
            var count = propertyNames.Count;

            printer.PrintLine(GENERATED_CODE);
            printer.PrintBeginLine("private static readonly string[] s_scopeIdentifiers = new string[] { ");

            for (var i = 0; i < count; i++)
            {
                printer.Print("nameof(").Print(propertyNames[i]).Print("), ");
            }

            printer.PrintEndLine("};");
            printer.PrintEndLine();
            printer.PrintLine(GENERATED_CODE);
            printer.PrintBeginLine("public readonly ").Print(PageFlowsAliasSet.SYSTEM)
                .PrintEndLine(".ReadOnlyMemory<string> ScopeIdentifiers => s_scopeIdentifiers;");
        }

        private static void WriteTrySetScope(ref Printer printer, in PageFlowScopeCollectionSpec spec)
        {
            var propertyNames = spec.PropertyNames;
            var count = propertyNames.Count;

            printer.PrintLine(GENERATED_CODE);
            printer.PrintBeginLine("public bool TrySetScope(string identifier, ").Print(PageFlowsAliasSet.PAGE_FLOWS)
                .PrintEndLine(".PageFlowScope scope)");
            printer.OpenScope();
            {
                printer.PrintLine("switch (identifier)");
                printer.OpenScope();
                {
                    for (var i = 0; i < count; i++)
                    {
                        var propertyName = propertyNames[i];

                        printer.PrintBeginLine("case nameof(").Print(propertyName).PrintEndLine("):");
                        printer.OpenScope();
                        {
                            printer.PrintBeginLine("this.").Print(propertyName).PrintEndLine(" = scope;");
                            printer.PrintLine("return true;");
                        }
                        printer.CloseScope();
                        printer.PrintEndLine();
                    }

                    printer.PrintLine("default:");
                    printer.OpenScope();
                    {
                        printer.PrintLine("return false;");
                    }
                    printer.CloseScope();
                }
                printer.CloseScope();
            }
            printer.CloseScope();
        }
    }
}
