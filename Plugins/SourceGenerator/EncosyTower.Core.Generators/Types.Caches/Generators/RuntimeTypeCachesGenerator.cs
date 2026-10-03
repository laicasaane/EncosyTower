using EncosyTower.SourceGen.Helpers.Types.Caches;

namespace EncosyTower.Core.Generators.Types.Caches
{
    [Generator]
    internal sealed class RuntimeTypeCachesGenerator : IIncrementalGenerator
    {
        public const string NAMESPACE = "EncosyTower.Types.Caches";
        public const string NAMESPACE_PREFIX = $"global::{NAMESPACE}";
        public const string SKIP_ATTRIBUTE = $"{NAMESPACE_PREFIX}.SkipSourceGeneratorsForAssemblyAttribute";
        public const string GENERATOR_NAME = nameof(RuntimeTypeCachesGenerator);
        public const string RUNTIME_TYPE_CACHE = "global::EncosyTower.Types.RuntimeTypeCache";

        private const string GENERATED_CODE = $"[g__SCDC.GeneratedCode(\"EncosyTower.Core.Generators.Types.Caches.RuntimeTypeCachesGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_RUNTIME_TYPE_CACHES = "[g__ETTCSG.GeneratedRuntimeTypeCaches]";
        private const string PRESERVE = "[g__UES.Preserve]";
        private const string EDITOR_BROWSABLE_NEVER = "[g__SCM.EditorBrowsable(g__SCM.EditorBrowsableState.Never)]";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var compilationProvider = context.CompilationProvider
                .Select(static (x, c) => CompilationSpec.Create(x, c, NAMESPACE, SKIP_ATTRIBUTE));

            var partialTypeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                  predicate: IsSyntaxMatched
                , transform: ExtractPartialType
            ).WithTrackingName("RuntimeTypeCachesGenerator.Candidates")
                .Where(static c => c.IsValid)
                .WithTrackingName("RuntimeTypeCachesGenerator.ValidSpecs");

            var combined = partialTypeProvider
                .Collect()
                .Combine(compilationProvider)
                .WithTrackingName("RuntimeTypeCachesGenerator.Outputs");

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(sourceProductionContext, source.Right, source.Left);
            });

            var typesProvider = partialTypeProvider.Select(static (x, _) => TypeSpec.From(x))
                .Collect()
                .Combine(compilationProvider)
                .WithTrackingName("RuntimeTypeCachesGenerator.Outputs");

            context.RegisterSourceOutput(typesProvider, static (sourceProductionContext, source) => {
                GenerateHeaderOutput(sourceProductionContext, source.Right, source.Left);
            });
        }

        private static bool IsSyntaxMatched(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is MemberAccessExpressionSyntax syntax
                && syntax.Expression is IdentifierNameSyntax { Identifier.ValueText: "RuntimeTypeCache" }
                && syntax.Name is GenericNameSyntax { TypeArgumentList.Arguments.Count: 1 } member
                && IsMemberSupported(member.Identifier.ValueText)
                && GetContainingType(syntax, token) is not null;
        }

        private static bool IsMemberSupported(string memberName)
        {
            return memberName switch {
                RuntimeTypeCacheMethods.GET_INFO => true,
                RuntimeTypeCacheMethods.GET_TYPES_DERIVED_FROM => true,
                RuntimeTypeCacheMethods.GET_TYPES_WITH_ATTRIBUTE => true,
                RuntimeTypeCacheMethods.GET_FIELDS_WITH_ATTRIBUTE => true,
                RuntimeTypeCacheMethods.GET_METHODS_WITH_ATTRIBUTE => true,
                _ => false,
            };
        }

        private static PartialTypeSpec ExtractPartialType(GeneratorSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not MemberAccessExpressionSyntax syntax
                || syntax.Expression is not IdentifierNameSyntax identifier
                || syntax.Name is not GenericNameSyntax member
            )
            {
                return default;
            }

            var semanticModel = context.SemanticModel;
            var identifierType = semanticModel.GetTypeInfo(identifier, token).Type;

            if (identifierType.HasFullName(RUNTIME_TYPE_CACHE, token) == false)
            {
                return default;
            }

            var typeArgList = member.TypeArgumentList;
            var typeInfo = semanticModel.GetTypeInfo(typeArgList.Arguments[0], token);

            if (typeInfo.Type is not INamedTypeSymbol type)
            {
                return default;
            }

            var cacheAttributeType = member.Identifier.ValueText switch {
                RuntimeTypeCacheMethods.GET_INFO => CacheAttributeType.CacheType,
                RuntimeTypeCacheMethods.GET_TYPES_DERIVED_FROM => CacheAttributeType.CacheTypesDerivedFrom,
                RuntimeTypeCacheMethods.GET_TYPES_WITH_ATTRIBUTE => CacheAttributeType.CacheTypesWithAttribute,
                RuntimeTypeCacheMethods.GET_FIELDS_WITH_ATTRIBUTE => CacheAttributeType.CacheFieldsWithAttribute,
                RuntimeTypeCacheMethods.GET_METHODS_WITH_ATTRIBUTE => CacheAttributeType.CacheMethodsWithAttribute,
                _ => CacheAttributeType.None,
            };

            if (cacheAttributeType == CacheAttributeType.None)
            {
                return default;
            }

            var typeFullName = type.ToFullName();

            if (cacheAttributeType == CacheAttributeType.CacheType)
            {
                if (type.IsStatic)
                {
                    return default;
                }
            }
            else if (cacheAttributeType == CacheAttributeType.CacheTypesDerivedFrom)
            {
                if (type.TypeKind is not (TypeKind.Class or TypeKind.Interface) || type.IsStatic || type.IsSealed)
                {
                    return default;
                }
            }
            else if (typeFullName.StartsWith(NAMESPACE_PREFIX))
            {
                return default;
            }

            var assemblyName = string.Empty;

            if (syntax.Parent is InvocationExpressionSyntax { ArgumentList.Arguments: { Count: 1 } arguments })
            {
                var constValueOpt = semanticModel.GetConstantValue(arguments[0].Expression, token);

                if (constValueOpt is { HasValue: true, Value: string asmName })
                {
                    assemblyName = string.IsNullOrWhiteSpace(asmName) ? string.Empty : asmName;
                }
                else
                {
                    return default;
                }
            }

            var containingSyntax = GetContainingType(syntax, token);

            if (containingSyntax is null)
            {
                return default;
            }

            if (semanticModel.GetDeclaredSymbol(containingSyntax, token) is not ITypeSymbol containingType)
            {
                return default;
            }

            var isStruct = containingType.TypeKind == TypeKind.Struct;

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  containingSyntax
                , token
                , out var scopeOpening
                , out var scopeClosing
                , printAdditionalUsings: PrintAdditionalUsings
            );

            return new PartialTypeSpec {
                containingTypeFullName = containingType.ToFullName(),
                containingTypeIdentifier = containingSyntax.Identifier.ValueText,
                isStruct = isStruct,
                isRefStruct = isStruct && containingSyntax.Modifiers.Any(SyntaxKind.RefKeyword),
                isRecord = containingSyntax is RecordDeclarationSyntax,
                openingSource = scopeOpening,
                closingSource = scopeClosing,
                containingTypes = TypeCreationHelpers.GetContainingTypeSpecs(containingSyntax, token),
                typeFullName = typeFullName,
                cacheAttributeType = cacheAttributeType,
                assemblyName = assemblyName,
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using g__S = global::System;");
            p.PrintLine("using g__SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using g__SCM = global::System.ComponentModel;");
            p.PrintLine("using g__SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using g__SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using g__SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using g__ETTC = global::EncosyTower.Types.Caches;");
            p.PrintLine("using g__ETTCSG = global::EncosyTower.Types.Caches.SourceGen;");
            p.PrintLine("using g__UES = global::UnityEngine.Scripting;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static TypeDeclarationSyntax GetContainingType(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var parent = node.Parent;

            while (parent is not null && parent is not TypeDeclarationSyntax)
            {
                token.ThrowIfCancellationRequested();

                parent = parent.Parent;
            }

            return parent as TypeDeclarationSyntax;
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , ImmutableArray<PartialTypeSpec> types
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (compilation.IsValid == false || types.IsDefaultOrEmpty)
            {
                return;
            }

            var assemblyName = compilation.AssemblyName;
            var orderedTypes = types
                .OrderBy(static type => type.containingTypeFullName, StringComparer.Ordinal)
                .ThenBy(static type => type.typeFullName, StringComparer.Ordinal)
                .ThenBy(static type => type.assemblyName, StringComparer.Ordinal)
                .ThenBy(static type => type.cacheAttributeType)
                .ToArray();
            var seenRequests = new HashSet<PartialTypeSpec>();
            var bodyPrinter = new Printer(0, 1024 * 16, token);
            var hasBodySource = false;
            var currentTarget = string.Empty;
            var currentOpening = string.Empty;
            var currentClosing = string.Empty;

            foreach (var type in orderedTypes)
            {
                token.ThrowIfCancellationRequested();

                if (seenRequests.Add(type) == false)
                {
                    continue;
                }

                if (string.Equals(currentTarget, type.containingTypeFullName, StringComparison.Ordinal) == false
                    && hasBodySource
                )
                {
                    AddSource(
                          context
                        , assemblyName
                        , currentTarget
                        , currentOpening
                        , bodyPrinter
                        , currentClosing
                    );
                    bodyPrinter.Clear();
                    hasBodySource = false;
                }

                if (hasBodySource == false)
                {
                    currentTarget = type.containingTypeFullName;
                    currentOpening = type.openingSource;
                    currentClosing = type.closingSource;
                }

                bodyPrinter.Print(WriteCode(type, token));
                hasBodySource = true;
            }

            if (hasBodySource)
            {
                AddSource(
                      context
                    , assemblyName
                    , currentTarget
                    , currentOpening
                    , bodyPrinter
                    , currentClosing
                );
            }

            static void AddSource(
                  SourceProductionContext context
                , string assemblyName
                , string targetMetadataName
                , string openingSource
                , Printer bodyPrinter
                , string closingSource
            )
            {
                var hintName = SourceGenHelpers.BuildSemanticHintName(
                      "EncosyTower.Core.Generators.Types.Caches.RuntimeTypeCachesGenerator"
                    , assemblyName
                    , targetMetadataName
                    , "RuntimeTypeCache"
                    , string.Empty
                );
                var generatedSource = TypeCreationHelpers.GenerateSourceText(
                      openingSource
                    , bodyPrinter.Result
                    , closingSource
                    , context.CancellationToken
                );
                context.CancellationToken.ThrowIfCancellationRequested();
                context.AddSource(hintName, generatedSource);
            }
        }

        private static void GenerateHeaderOutput(
              SourceProductionContext context
            , CompilationSpec compilation
            , ImmutableArray<TypeSpec> types
        )
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (compilation.IsValid == false || types.IsDefaultOrEmpty)
            {
                return;
            }

            var assemblyName = compilation.AssemblyName;
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var type in types.OrderBy(static type => type.containingTypeFullName, StringComparer.Ordinal))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (seen.Add(type.containingTypeFullName) == false)
                {
                    continue;
                }

                var hintName = SourceGenHelpers.BuildSemanticHintName(
                      "EncosyTower.Core.Generators.Types.Caches.RuntimeTypeCachesGenerator"
                    , assemblyName
                    , assemblyName
                    , "RuntimeTypeCacheHeader"
                    , type.containingTypeFullName
                );
                var generatedSource = TypeCreationHelpers.GenerateSourceText(
                      type.openingSource
                    , WriteCode(type, context.CancellationToken)
                    , type.closingSource
                    , context.CancellationToken
                );
                context.CancellationToken.ThrowIfCancellationRequested();
                context.AddSource(hintName, generatedSource);
            }
        }

        private static string WriteCode(in PartialTypeSpec type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(type.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(type.isRecord, "record ")
                    .PrintIf(type.isStruct, "struct ", "class ")
                    .Print(type.containingTypeIdentifier)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("[g__ETTC.");

                    switch (type.cacheAttributeType)
                    {
                        case CacheAttributeType.CacheType:
                            p.Print(nameof(CacheAttributeType.CacheType));
                            break;

                        case CacheAttributeType.CacheTypesDerivedFrom:
                            p.Print(nameof(CacheAttributeType.CacheTypesDerivedFrom));
                            break;

                        case CacheAttributeType.CacheTypesWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheTypesWithAttribute));
                            break;

                        case CacheAttributeType.CacheFieldsWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheFieldsWithAttribute));
                            break;

                        case CacheAttributeType.CacheMethodsWithAttribute:
                            p.Print(nameof(CacheAttributeType.CacheMethodsWithAttribute));
                            break;
                    }

                    p.Print("(typeof(").Print(type.typeFullName).Print(")");

                    if (string.IsNullOrEmpty(type.assemblyName) == false)
                    {
                        p.Print(", \"").Print(type.assemblyName).Print("\"");
                    }

                    p.PrintEndLine(")]");

                    p.PrintBeginLine("partial struct ")
                        .Print(type.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches")
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        private static string WriteCode(in TypeSpec type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.PrintBeginLine()
                    .PrintIf(type.isRefStruct, "ref ")
                    .Print("partial ")
                    .PrintIf(type.isRecord, "record ")
                    .PrintIf(type.isStruct, "struct ", "class ")
                    .Print(type.containingTypeIdentifier)
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintLine("/// <summary>");
                    p.PrintLine("/// Provides information about the types, fields and methods to be cached.");
                    p.PrintLine("/// </summary>");
                    p.PrintLine(GENERATED_RUNTIME_TYPE_CACHES)
                        .PrintLine(GENERATED_CODE)
                        .PrintLine(EXCLUDE_COVERAGE)
                        .PrintLine(EDITOR_BROWSABLE_NEVER)
                        .PrintLine(PRESERVE);

                    p.PrintBeginLine("private partial struct ")
                        .Print(type.containingTypeIdentifier)
                        .Print("_RuntimeTypeCaches")
                        .PrintEndLine(" { }");
                }
                p.CloseScope();
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        internal enum CacheAttributeType
        {
            None,
            CacheType,
            CacheTypesDerivedFrom,
            CacheTypesWithAttribute,
            CacheFieldsWithAttribute,
            CacheMethodsWithAttribute,
        }

        internal struct PartialTypeSpec : IEquatable<PartialTypeSpec>
        {
            public string containingTypeFullName;
            public string containingTypeIdentifier;
            public string typeFullName;
            public string assemblyName;
            public string openingSource;
            public string closingSource;
            public EquatableArray<ContainingTypeSpec> containingTypes;
            public CacheAttributeType cacheAttributeType;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            public readonly bool IsValid => containingTypeFullName is not null;

            public readonly bool Equals(PartialTypeSpec other)
            {
                return string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
                    && string.Equals(containingTypeIdentifier, other.containingTypeIdentifier, StringComparison.Ordinal)
                    && string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
                    && string.Equals(assemblyName, other.assemblyName, StringComparison.Ordinal)
                    && cacheAttributeType == other.cacheAttributeType
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    && containingTypes.Equals(other.containingTypes)
                    ;
            }

            public readonly override bool Equals(object obj)
                => obj is PartialTypeSpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(
                      containingTypeFullName
                    , containingTypeIdentifier
                    , typeFullName
                    , assemblyName
                    , cacheAttributeType
                    , isStruct
                )
                .Add(isRefStruct)
                .Add(isRecord)
                .Add(containingTypes)
                ;
        }

        internal struct TypeSpec : IEquatable<TypeSpec>
        {
            public string containingTypeFullName;
            public string containingTypeIdentifier;
            public string openingSource;
            public string closingSource;
            public EquatableArray<ContainingTypeSpec> containingTypes;
            public bool isStruct;
            public bool isRefStruct;
            public bool isRecord;

            public static TypeSpec From(PartialTypeSpec c)
                => new() {
                containingTypeFullName = c.containingTypeFullName,
                containingTypeIdentifier = c.containingTypeIdentifier,
                openingSource = c.openingSource,
                closingSource = c.closingSource,
                containingTypes = c.containingTypes,
                isStruct = c.isStruct,
                isRefStruct = c.isRefStruct,
                isRecord = c.isRecord,
            };

            public readonly bool Equals(TypeSpec other)
            {
                return string.Equals(containingTypeFullName, other.containingTypeFullName, StringComparison.Ordinal)
                    && isStruct == other.isStruct
                    && isRefStruct == other.isRefStruct
                    && isRecord == other.isRecord
                    && containingTypes.Equals(other.containingTypes)
                    ;
            }

            public readonly override bool Equals(object obj)
                => obj is TypeSpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(containingTypeFullName, isStruct, isRefStruct, isRecord, containingTypes);
        }
    }
}
