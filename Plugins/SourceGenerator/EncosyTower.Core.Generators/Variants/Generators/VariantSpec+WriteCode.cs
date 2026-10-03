using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Core.Generators.Variants
{
    partial struct VariantSpec
    {
        private const string GENERATOR_NAME_STRUCT = "VariantStructDeclaration";

        private const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Core.Generators.Variants.VariantStructGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string STRUCT_LAYOUT = "[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]";
        private const string PRESERVE = "[g__UES.Preserve]";
        private const string IVARIANT_T = "g__ETV.IVariant<";

        internal readonly void WriteVariantCode(ref SourceProductionContext context, CompilationSpec compilation)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.Variants.VariantStructGenerator"
                , assemblyName
                , structFullName
                , "VariantStruct"
                , string.Empty
            );
            var variantName = $"g__ETV.Variant<{fullTypeName}>";

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  openingSource
                , BuildVariantSource(variantName, context.CancellationToken)
                , closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private readonly string BuildVariantSource(string variantName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintLineIf(isValueType, STRUCT_LAYOUT);
            p.PrintLine(GENERATED_CODE);
            p.PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine(PRESERVE);
            p.PrintBeginLine()
                .Print("partial struct ").Print(structName)
                .Print($" : {IVARIANT_T}{fullTypeName}>")
                .PrintEndLine();

            variantPrinter.WriteVariantBody(
                  ref p
                , isValueType
                , hasImplicitFromStructToType
                , fullTypeName
                , structName
                , variantName
                , GENERATED_CODE
            );

            return p.Result;
        }
    }
}
