using static EncosyTower.Persistence.Generators.Helpers;

namespace EncosyTower.Persistence.Generators
{
    [Generator]
    internal sealed class PersistenceGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(PersistenceGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (compilation, token) =>
                    CompilationSpec.Create(compilation, token, NAMESPACE, SKIP_ATTRIBUTE)
                );

            var providerClassProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  PERSISTENCE_ATTRIBUTE_METADATA
                , static (node, _) => node is ClassDeclarationSyntax syntax
                      && syntax.HasModifier(SyntaxKind.AbstractKeyword) == false
                      && syntax.TypeParameterList is null
                , GetVaultInfo
            ).WithTrackingName("PersistenceGenerator.Candidates")
                .Where(static t => t.IsValid)
                .WithTrackingName("PersistenceGenerator.ValidSpecs");

            var accessProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
                  ACCESSOR_ATTRIBUTE_METADATA
                , static (node, _) => node is ClassDeclarationSyntax syntax
                      && syntax.HasModifier(SyntaxKind.AbstractKeyword) == false
                      && syntax.TypeParameterList is null
                , GetAccessorInfo
            ).Where(static t => t.isValid);

            var combined = providerClassProvider
                .Combine(accessProvider.Collect())
                .Combine(compilationProvider)
                .Where(static t => t.Right.IsValid)
                .WithTrackingName("PersistenceGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left.Left, source.Left.Right);
            });
        }

        private static PersistenceSpec GetVaultInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol
                || (symbol.IsAbstract && symbol.IsStatic == false)
                || symbol.IsGenericType
            )
            {
                return default;
            }

            var syntaxTree = context.TargetNode.SyntaxTree;
            var containingNs = symbol.ContainingNamespace;

            return new PersistenceSpec {
                metadataName = symbol.ToFullNameNoGlobal(),
                className = symbol.Name,
                isStatic = symbol.IsStatic,
                namespaceName = containingNs is { IsGlobalNamespace: false }
                    ? containingNs.ToDisplayString()
                    : string.Empty,
                containingTypeDeclarations = symbol.GetContainingTypes(),
                hintName = symbol.ToMetadataName(),
            };
        }

        private static PersistAccessorSpec GetAccessorInfo(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetSymbol is not INamedTypeSymbol symbol || symbol.IsAbstract || symbol.IsGenericType)
            {
                return default;
            }

            var vaultMetadataName = string.Empty;
            var attribute = context.Attributes[0];

            if (attribute.ConstructorArguments.Length > 0
                && attribute.ConstructorArguments[0].Value is INamedTypeSymbol persistenceType
            )
            {
                vaultMetadataName = persistenceType.ToFullNameNoGlobal();
            }

            var fieldName = string.Empty;

            foreach (var attrib in symbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attribName = attrib.AttributeClass?.Name ?? string.Empty;

                if (attribName is not ("LabelAttribute" or "DisplayNameAttribute"))
                {
                    continue;
                }

                if (attrib.ConstructorArguments.Length > 0)
                {
                    var arg = attrib.ConstructorArguments[0];

                    if (arg.Kind == TypedConstantKind.Primitive && arg.Value?.ToString() is string dn)
                    {
                        fieldName = dn;
                        goto NEXT;
                    }
                }
                else if (attrib.NamedArguments.Length > 0)
                {
                    foreach (var arg in attrib.NamedArguments)
                    {
                        if (arg.Key is "Name" or "DisplayName"
                            && arg.Value.Kind == TypedConstantKind.Primitive
                            && arg.Value.Value?.ToString() is string dn
                        )
                        {
                            fieldName = dn;
                            goto NEXT;
                        }
                    }
                }
            }

            NEXT:

            if (string.IsNullOrEmpty(fieldName))
            {
                fieldName = symbol.Name;
            }

            var constructors = symbol.Constructors;
            var constructorIndex = -1;
            var max = 0;

            for (var i = 0; i < constructors.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                if (constructors[i].Parameters.Length > max)
                {
                    max = constructors[i].Parameters.Length;
                    constructorIndex = i;
                }
            }

            if (constructorIndex != 0)
            {
                return default;
            }

            var constructor = constructors[constructorIndex];
            var parameters = constructor.Parameters;
            using var builder = ImmutableArrayBuilder<AccessorArgSpec>.Rent();
            var isValid = true;

            foreach (var param in parameters)
            {
                token.ThrowIfCancellationRequested();

                if (ParamDeclaration.TryGetParam(param.Type, out var argType))
                {
                    var dataTypeHasDefaultConstructor = false;

                    if (argType != null)
                    {
                        var nonDefaultCount = 0;
                        var defaultCount = 0;

                        foreach (var member in argType.GetMembers())
                        {
                            token.ThrowIfCancellationRequested();

                            if (member is IMethodSymbol method
                                && method.MethodKind == MethodKind.Constructor
                            )
                            {
                                if (method.Parameters.Length > 0)
                                    nonDefaultCount++;
                                else
                                    defaultCount++;
                            }
                        }

                        dataTypeHasDefaultConstructor = defaultCount > 0 || nonDefaultCount < 1;
                    }

                    builder.Add(new AccessorArgSpec(
                          isStore: argType != null
                        , fullTypeName: param.Type.ToFullName()
                        , fullDataTypeName: argType?.ToFullName() ?? string.Empty
                        , typeName: argType?.Name ?? param.Type.Name
                        , dataTypeHasDefaultConstructor: dataTypeHasDefaultConstructor
                    ));
                }
                else
                {
                    isValid = false;
                }
            }

            return new PersistAccessorSpec {
                metadataName = symbol.ToFullNameNoGlobal(),
                vaultMetadataName = vaultMetadataName,
                fieldName = fieldName,
                symbolName = symbol.Name,
                args = builder.ToImmutable().AsEquatableArray(),
                isInitializable = symbol.InheritsFromInterface("global::EncosyTower.Initialization.IInitializable"),
                isDeinitializable = symbol.InheritsFromInterface("global::EncosyTower.Initialization.IDeinitializable"),
                isValid = isValid,
            };
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , PersistenceSpec vaultInfo
            , ImmutableArray<PersistAccessorSpec> accessorInfos
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (vaultInfo.IsValid == false || accessorInfos.Length < 1)
            {
                return;
            }

            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            var accessDeclarations = new List<PersistAccessorDeclaration>(accessorInfos.Length);

            for (var i = 0; i < accessorInfos.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var aInfo = accessorInfos[i];

                if (string.IsNullOrEmpty(aInfo.vaultMetadataName) == false
                    && string.Equals(aInfo.vaultMetadataName, vaultInfo.metadataName, StringComparison.Ordinal) == false
                )
                {
                    continue;
                }

                var accessDeclaration = new PersistAccessorDeclaration(aInfo);

                if (accessDeclaration.IsValid)
                {
                    accessDeclarations.Add(accessDeclaration);
                }
            }

            if (accessDeclarations.Count < 1)
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            accessDeclarations.Sort(static (x, y) => {
                return string.Compare(x.SymbolName, y.SymbolName, StringComparison.Ordinal);
            });

            var declaration = new PersistenceDeclaration(vaultInfo.className, vaultInfo.isStatic, accessDeclarations);

            var openingPrinter = new Printer(0, 1024 * 16, token);
            PrintUsingDirectives(ref openingPrinter);

            var hasNamespace = string.IsNullOrEmpty(vaultInfo.namespaceName) == false;

            if (hasNamespace)
            {
                openingPrinter.PrintLine($"namespace {vaultInfo.namespaceName}");
                openingPrinter.OpenScope();
            }

            var containingTypes = vaultInfo.containingTypeDeclarations;

            for (var i = 0; i < containingTypes.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                openingPrinter.PrintLine(containingTypes[i]);
                openingPrinter.OpenScope();
            }

            var openingSource = openingPrinter.Result;
            var closingPrinter = new Printer(0, 1024 * 16, token);
            closingPrinter.PrintEndLine();

            var closingDepth = containingTypes.Count + (hasNamespace ? 1 : 0);

            for (var i = 0; i < closingDepth; i++)
            {
                token.ThrowIfCancellationRequested();

                closingPrinter = closingPrinter.DecreasedIndent();
                closingPrinter.PrintLine("}");
            }

            var assemblyName = compilation.AssemblyName;
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  "EncosyTower.Persistence.Generators.PersistenceGenerator"
                , assemblyName
                , vaultInfo.metadataName
                , "Persistence"
                , string.Empty
            );

            var generatedSource = TypeCreationHelpers.GenerateSourceText(
                  openingSource
                , declaration.WriteCode(token)
                , closingPrinter.Result.TrimEnd()
                , context.CancellationToken
            );
            context.CancellationToken.ThrowIfCancellationRequested();
            context.AddSource(hintName, generatedSource);
        }

        private static void PrintUsingDirectives(ref Printer p)
        {
            p.PrintEndLine();
            p.Print(
                "#pragma warning disable CS0105 // Using directive appeared previously in this namespace"
            ).PrintEndLine();
            p.PrintEndLine();
            PrintAdditionalUsings(ref p);
            p.Print(
                "#pragma warning restore CS0105 // Using directive appeared previously in this namespace"
            ).PrintEndLine();
            p.PrintEndLine();
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SC = global::System.Collections;");
            p.PrintLine("using g__SCG = global::System.Collections.Generic;");
            p.PrintLine("using g__ST = global::System.Threading;");
            p.PrintLine("using g__SD = global::System.Diagnostics;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SB = global::System.Buffers;");
            p.PrintLine("using g__UE = global::UnityEngine;");
            p.PrintLine("using g__ET = global::EncosyTower.Common;");
            p.PrintLine("using g__ETP = global::EncosyTower.Persistences;");
            p.PrintLine("using g__ETS = global::EncosyTower.StringIds;");
            p.PrintLine("using g__ETE = global::EncosyTower.Encryption;");
            p.PrintLine("using g__ETC = global::EncosyTower.Collections;");
            p.PrintLine("using g__ETDBG = global::EncosyTower.Debugging;");
            p.PrintLine("using g__ETDBGVD = global::EncosyTower.Debugging.ValidationDefines;");
            p.PrintLine("using g__ETT = global::EncosyTower.Tasks;");
            p.PrintLine("using g__ETL = global::EncosyTower.Logging;");
            p.PrintLine("using g__ETI = global::EncosyTower.Initialization;");
            p.PrintEndLine();
        }
    }
}
