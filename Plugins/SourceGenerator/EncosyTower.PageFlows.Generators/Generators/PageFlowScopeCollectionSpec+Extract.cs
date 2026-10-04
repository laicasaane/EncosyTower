using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.PageFlows.Generators
{
    internal readonly partial struct PageFlowScopeCollectionSpec
    {
        public static PageFlowScopeCollectionSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not StructDeclarationSyntax currentDeclaration
                || context.TargetSymbol is not INamedTypeSymbol symbol
            )
            {
                return default;
            }

            var compilation = context.SemanticModel.Compilation;

            if (compilation.IsValidCompilation(
                  token
                , PageFlowScopeCollectionSourceGenContract.NAMESPACE
                , PageFlowScopeCollectionSourceGenContract.SKIP_ATTRIBUTE
            ) == false
            )
            {
                return default;
            }

            var scope = compilation.GetTypeByMetadataName(PageFlowScopeCollectionSourceGenContract.SCOPE);
            var marker = compilation.GetTypeByMetadataName(PageFlowScopeCollectionSourceGenContract.ATTRIBUTE);

            if (scope == null
                || marker == null
                || symbol.IsReadOnly
                || symbol.IsRefLikeType
                || IsPartialEverywhere(symbol, token) == false
                || IsFirstAttributedDeclaration(currentDeclaration, symbol, marker, token) == false
            )
            {
                return default;
            }

            var propertyNames = GetPropertyNames(symbol, scope, token);

            if (propertyNames.Length == 0)
            {
                return default;
            }

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  currentDeclaration
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PageFlowsAliasSet.WriteAliases
            );

            var assemblyName = compilation.AssemblyName ?? string.Empty;
            var metadataName = symbol.ToMetadataName();
            var hintName = SourceGenHelpers.BuildSemanticHintName(
                  PageFlowScopeCollectionSourceGenContract.GENERATOR_METADATA_NAME
                , assemblyName
                , metadataName
                , PageFlowScopeCollectionSourceGenContract.OUTPUT_ROLE
                , string.Empty
            );

            return new PageFlowScopeCollectionSpec(
                  openingSource
                , closingSource
                , currentDeclaration.Identifier.Text + currentDeclaration.TypeParameterList
                , metadataName
                , assemblyName
                , hintName
                , propertyNames.AsEquatableArray()
            );
        }

        private static ImmutableArray<string> GetPropertyNames(
              INamedTypeSymbol symbol
            , INamedTypeSymbol scope
            , CancellationToken token
        )
        {
            var builder = ImmutableArray.CreateBuilder<string>();
            var members = symbol.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                if (members[i] is not IPropertySymbol property
                    || IsScopeProperty(property, scope) == false
                )
                {
                    continue;
                }

                var name = property.Name;

                if (SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
                {
                    name = "@" + name;
                }

                builder.Add(name);
            }

            return builder.ToImmutable();
        }

        private static bool IsScopeProperty(IPropertySymbol property, INamedTypeSymbol scope)
            => property.IsStatic == false
            && property.IsIndexer == false
            && property.ExplicitInterfaceImplementations.IsEmpty
            && SymbolEqualityComparer.Default.Equals(property.Type, scope)
            && property.SetMethod is { IsInitOnly: false }
            ;

        private static bool IsPartialEverywhere(INamedTypeSymbol symbol, CancellationToken token)
        {
            for (var type = symbol; type != null; type = type.ContainingType)
            {
                var references = type.DeclaringSyntaxReferences;
                var referenceCount = references.Length;

                for (var i = 0; i < referenceCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    if (references[i].GetSyntax(token) is TypeDeclarationSyntax declaration
                        && declaration.Modifiers.Any(SyntaxKind.PartialKeyword) == false
                    )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsFirstAttributedDeclaration(
              StructDeclarationSyntax currentDeclaration
            , INamedTypeSymbol symbol
            , INamedTypeSymbol marker
            , CancellationToken token
        )
        {
            var references = symbol.DeclaringSyntaxReferences;
            var referenceCount = references.Length;

            for (var i = 0; i < referenceCount; i++)
            {
                token.ThrowIfCancellationRequested();

                if (references[i].GetSyntax(token) is not StructDeclarationSyntax declaration
                    || HasMarker(declaration, symbol, marker, token) == false
                )
                {
                    continue;
                }

                return declaration.SyntaxTree == currentDeclaration.SyntaxTree
                    && declaration.Span == currentDeclaration.Span;
            }

            return false;
        }

        private static bool HasMarker(
              StructDeclarationSyntax declaration
            , INamedTypeSymbol symbol
            , INamedTypeSymbol marker
            , CancellationToken token
        )
        {
            var attributes = symbol.GetAttributes();
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                var attribute = attributes[i];

                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marker)
                    && attribute.ApplicationSyntaxReference?.GetSyntax(token) is SyntaxNode syntax
                    && declaration.SyntaxTree == syntax.SyntaxTree
                    && declaration.Span.Contains(syntax.Span)
                )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
