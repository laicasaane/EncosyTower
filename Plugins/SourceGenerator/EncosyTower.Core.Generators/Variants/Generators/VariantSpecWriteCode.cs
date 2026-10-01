using EncosyTower.SourceGen.Helpers.Variants;

namespace EncosyTower.Core.Generators.Variants
{
    internal static class VariantSpecWriteCode
    {
        private const string GENERATOR_NAME_REG = "VariantRegistrationDeclaration";

        private const string GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Core.Generators.Variants.VariantRegistrationGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string PRESERVE = "[g__UES.Preserve]";
        private const string RUNTIME_INITIALIZE_ON_LOAD_METHOD = "[g__UE.RuntimeInitializeOnLoadMethod(g__UE.RuntimeInitializeLoadType.BeforeSceneLoad)]";
        private const string GENERATED_GENERIC_VARIANTS = "[g__ETVSG.GeneratedGenericVariants]";

        public static void WriteStaticRegistrationClass(
              ref SourceProductionContext context
            , ImmutableArray<VariantSpec> declarations
            , CompilationSpec compilation
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.Variants.VariantRegistrationGenerator"
                , assemblyName
                , assemblyName
                , "VariantRegistration"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  PrintAdditionalUsings(context.CancellationToken)
                , BuildRegistrationClassSource(declarations, assemblyName, context.CancellationToken)
                , string.Empty
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static string BuildRegistrationClassSource(
            ImmutableArray<VariantSpec> declarations
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

            p.PrintLine($"namespace EncosyTower.Variants.__AttributeVariants__.{assemblyName.ToValidIdentifier()}");
            p.OpenScope();
            {
                p.PrintLine("/// <summary>");
                p.PrintLine("/// Automatically registers attribute-based variants");
                p.PrintLine($"/// to <see cref=\"g__ETVC.VariantConverter\"/>");
                p.PrintLine("/// on Unity3D platform.");
                p.PrintLine("/// </summary>");
                p.PrintLine(GENERATED_GENERIC_VARIANTS)
                    .PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(PRESERVE);
                p.PrintLine("public static partial class AttributeVariants");
                p.OpenScope();
                {
                    p.PrintLine(PRESERVE);
                    p.PrintLine("static AttributeVariants()");
                    p.OpenScope();
                    {
                        p.PrintLine("Init();");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(RUNTIME_INITIALIZE_ON_LOAD_METHOD);
                    p.PrintLine(PRESERVE);
                    p.PrintLine("private static void Init()");
                    p.OpenScope();
                    {
                        var wroteValueType = false;

                        foreach (var decl in declarations)
                        {
                            if (decl.isValueType == false)
                            {
                                continue;
                            }

                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.typeName
                                , decl.converterDefault
                                , decl.unmanagedSize
                            );

                            wroteValueType = true;
                        }

                        if (wroteValueType)
                        {
                            p.PrintEndLine();
                        }

                        foreach (var decl in declarations)
                        {
                            if (decl.isValueType)
                            {
                                continue;
                            }

                            variantPrinter.WriteRegister(
                                  ref p
                                , decl.fullTypeName
                                , decl.typeName
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

        public static void WriteRedundantTypeMarker(
              ref SourceProductionContext context
            , in VariantSpec declaration
            , CompilationSpec compilation
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Core.Generators.Variants.VariantRegistrationGenerator"
                , assemblyName
                , declaration.structFullName
                , "VariantRedundantTypeMarker"
                , declaration.fullTypeName
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  declaration.openingSource
                , BuildRedundantTypeSource(in declaration, context.CancellationToken)
                , declaration.closingSource
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static string BuildRedundantTypeSource(
              in VariantSpec declaration
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var fullTypeName = declaration.fullTypeName;

            p.PrintLine("/// <summary>");
            p.PrintLine($"/// A variant has already been implemented for <see cref=\"{fullTypeName}\"/>.");
            p.PrintLine("/// This declaration is redundant and can be removed.");
            p.PrintLine("/// </summary>");
            p.PrintLine($"[global::System.Obsolete(\"A variant has already been implemented for {fullTypeName}. This declaration is redundant and can be removed.\")]");
            p.PrintLine($"partial struct {declaration.structName} {{ }}");

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
