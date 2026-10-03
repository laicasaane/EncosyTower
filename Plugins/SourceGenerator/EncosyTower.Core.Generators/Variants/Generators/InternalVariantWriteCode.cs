using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Core.Generators.Variants
{
    internal static class InternalVariantWriteCode
    {
        public const string GENERATOR_NAME = "InternalVariantDeclaration";

        private const string INTERNAL_VARIANTS_NAMESPACE = "EncosyTower.Variants.__InternalVariants__";

        private const string GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Core.Generators.Variants.InternalVariantGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string STRUCT_LAYOUT = "[g__SRIS.StructLayout(g__SRIS.LayoutKind.Explicit)]";
        private const string PRESERVE = "[g__UES.Preserve]";
        private const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[g__UE.RuntimeInitializeOnLoadMethod(g__UE.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        private const string GENERATED_INTERNAL_VARIANTS = "[g__ETVSG.GeneratedInternalVariants]";

        public static void WriteVariantCode(
              ref SourceProductionContext context
            , in InternalVariantSpec decl
            , string assemblyName
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.Variants.InternalVariantGenerator"
                , assemblyName
                , decl.fullTypeName
                , "InternalVariant"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  PrintAdditionalUsings(context.CancellationToken)
                , BuildVariantSource(in decl, assemblyName, context.CancellationToken)
                , string.Empty
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static string BuildVariantSource(
              in InternalVariantSpec decl
            , string assemblyName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var typeName = decl.fullTypeName;
            var structName = decl.structName;
            var isValueType = decl.isValueType;
            var variantName = $"g__ETV.Variant<{typeName}>";

            var p = new Printer(0, 1024 * 16, token);
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(INTERNAL_VARIANTS_NAMESPACE)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("static partial class InternalVariants");
                p.OpenScope();
                {
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                    p.PrintLineIf(isValueType, STRUCT_LAYOUT);
                    p.PrintBeginLine()
                        .Print("private partial struct ").Print(structName)
                        .Print($" : g__ETV.IVariant<{typeName}>")
                        .PrintEndLine();

                    variantPrinter.WriteVariantBody(
                          ref p
                        , isValueType
                        , decl.hasImplicitFromStructToType
                        , typeName
                        , structName
                        , variantName
                        , GENERATED_CODE
                    );
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;
        }

        public static void WriteStaticClass(
              ref SourceProductionContext context
            , ImmutableArray<InternalVariantSpec> valueTypes
            , ImmutableArray<InternalVariantSpec> refTypes
            , string assemblyName
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.Variants.InternalVariantGenerator"
                , assemblyName
                , assemblyName
                , "InternalVariantRegistry"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  PrintAdditionalUsings(context.CancellationToken)
                , BuildStaticClassSource(valueTypes, refTypes, assemblyName, context.CancellationToken)
                , string.Empty
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static string BuildStaticClassSource(
              ImmutableArray<InternalVariantSpec> valueTypes
            , ImmutableArray<InternalVariantSpec> refTypes
            , string assemblyName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);
            var variantPrinter = new VariantPrinter();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine("namespace ").Print(INTERNAL_VARIANTS_NAMESPACE)
                .Print(".").PrintEndLine(assemblyName.ToValidIdentifier());
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Contains auto-generated variants for types detected from usage of either");
                p.PrintLine($"/// <c>Variant&lt;T&gt;.GetConverter()</c> or <c>CachedVariantConverter&lt;T&gt;.Default</c>.");
                p.PrintLine("/// <br/>");
                p.PrintLine($"/// Automatically register these variants to <see cref=\"g__ETVC.VariantConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// <br/>");
                p.PrintLine("/// These variants are not intended to be used directly by user-code");
                p.PrintLine("/// thus they are declared <c>private</c> inside this class.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_INTERNAL_VARIANTS)
                    .PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class InternalVariants");
                p.OpenScope();
                {
                    p.PrintLine(PRESERVE);
                    p.PrintLine("static InternalVariants()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Register all variants inside this class");
                    p.PrintLine($"/// to <see cref=\"g__ETVC.VariantConverter\"/>.");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(PRESERVE);
                    p.PrintLine("public static void Register() => Init();");
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(PRESERVE);
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        foreach (var decl in valueTypes)
                        {
                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.simpleTypeName
                                , decl.converterDefault
                                , decl.unmanagedSize
                            );
                        }

                        p.PrintEndLine();

                        foreach (var decl in refTypes)
                        {
                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.simpleTypeName
                                , decl.converterDefault
                                , decl.unmanagedSize
                            );
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    variantPrinter.WriteRegisterMethod(ref p);
                }
                p.CloseScope();
            }
            p.CloseScope();

            return p.Result;
        }

        private static string PrintAdditionalUsings(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ETT = global::EncosyTower.Types;");
            p.PrintLine("using g__ETV = global::EncosyTower.Variants;");
            p.PrintLine("using g__ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintLine("using g__ETVSG = global::EncosyTower.Variants.SourceGen;");
            p.PrintLine("using g__UE = global::UnityEngine;");
            p.PrintLine("using g__UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();

            return p.Result;
        }
    }
}
