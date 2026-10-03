using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace EncosyTower.SourceGen
{
    public readonly record struct GeneratedMemberReference(
          ExpressionSyntax Expression
        , INamedTypeSymbol GeneratedType
        , string Tool
    );

    public static class TypeReachabilityExtensions
    {
        private const string GENERATED_CODE_ATTRIBUTE = "global::System.CodeDom.Compiler.GeneratedCodeAttribute";
        private const string GENERATED_FILE_SUFFIX = ".g.cs";
        private const string TOOL_PREFIX = "EncosyTower.";
        private const string TOOL_GENERATORS_SEGMENT = ".Generators.";

        public static bool HasOnlyGeneratedDeclarations(this ISymbol symbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (symbol == null || symbol.DeclaringSyntaxReferences.Length < 1)
            {
                return false;
            }

            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                var filePath = reference.SyntaxTree.FilePath;

                if (filePath.EndsWith(GENERATED_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryGetGeneratingTool(
              this INamedTypeSymbol type
            , CancellationToken token
            , out string tool
        )
        {
            token.ThrowIfCancellationRequested();
            tool = null;

            if (type == null || type.TypeKind == TypeKind.Error)
            {
                return false;
            }

            var definition = type.OriginalDefinition;

            if (definition.HasOnlyGeneratedDeclarations(token) == false)
            {
                return false;
            }

            foreach (var attribute in definition.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                if (attribute.AttributeClass.HasFullName(GENERATED_CODE_ATTRIBUTE, token) == false)
                {
                    continue;
                }

                var arguments = attribute.ConstructorArguments;

                if (arguments.Length > 0
                    && arguments[0].Value is string candidate
                    && candidate.StartsWith(TOOL_PREFIX, StringComparison.Ordinal)
                    && candidate.IndexOf(TOOL_GENERATORS_SEGMENT, StringComparison.Ordinal) >= 0
                )
                {
                    tool = candidate;
                    return true;
                }

                return false;
            }

            return false;
        }

        public static bool TryFindEncosyTowerGeneratedType(
              this ITypeSymbol type
            , CancellationToken token
            , out INamedTypeSymbol generatedType
            , out string tool
        )
        {
            token.ThrowIfCancellationRequested();

            switch (type)
            {
                case IArrayTypeSymbol arrayType:
                {
                    return arrayType.ElementType.TryFindEncosyTowerGeneratedType(token, out generatedType, out tool);
                }

                case IPointerTypeSymbol pointerType:
                {
                    return pointerType.PointedAtType.TryFindEncosyTowerGeneratedType(
                          token
                        , out generatedType
                        , out tool
                    );
                }

                case INamedTypeSymbol namedType when namedType.TypeKind != TypeKind.Error:
                {
                    if (namedType.TryGetGeneratingTool(token, out tool))
                    {
                        generatedType = namedType.OriginalDefinition;
                        return true;
                    }

                    if (namedType.ContainingType != null
                        && namedType.ContainingType.TryFindEncosyTowerGeneratedType(
                              token
                            , out generatedType
                            , out tool
                        )
                    )
                    {
                        return true;
                    }

                    var typeArguments = namedType.TypeArguments;
                    var count = typeArguments.Length;

                    for (var i = 0; i < count; i++)
                    {
                        if (typeArguments[i].TryFindEncosyTowerGeneratedType(token, out generatedType, out tool))
                        {
                            return true;
                        }
                    }

                    break;
                }
            }

            generatedType = null;
            tool = null;
            return false;
        }

        private static readonly SymbolDisplayFormat s_typeNameFormat = new(
              globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted
            , typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
            , genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            , miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                | SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName
        );

        /// <summary>
        /// Gets the fully qualified name of <paramref name="type"/> in its runtime form: the namespace without
        /// <c>global::</c>, then the containing types and the type joined by <c>+</c>.
        /// </summary>
        public static string ToGeneratedTypeFullName(this ITypeSymbol type)
        {
            var name = type.ToDisplayString(s_typeNameFormat);

            for (var containingType = type.ContainingType; containingType is not null;)
            {
                name = $"{containingType.ToDisplayString(s_typeNameFormat)}+{name}";
                containingType = containingType.ContainingType;
            }

            return type.ContainingNamespace is { IsGlobalNamespace: false } ns
                ? $"{ns.ToDisplayString()}.{name}"
                : name;
        }

        public static bool ContainsErrorType(this ITypeSymbol type, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            switch (type)
            {
                case null:
                {
                    return false;
                }

                case IArrayTypeSymbol arrayType:
                {
                    return arrayType.ElementType.ContainsErrorType(token);
                }

                case IPointerTypeSymbol pointerType:
                {
                    return pointerType.PointedAtType.ContainsErrorType(token);
                }

                case INamedTypeSymbol namedType:
                {
                    if (namedType.TypeKind == TypeKind.Error)
                    {
                        return true;
                    }

                    if (namedType.ContainingType.ContainsErrorType(token))
                    {
                        return true;
                    }

                    var typeArguments = namedType.TypeArguments;
                    var count = typeArguments.Length;

                    for (var i = 0; i < count; i++)
                    {
                        if (typeArguments[i].ContainsErrorType(token))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            return false;
        }

        public static bool TryFindGeneratedMemberReference(
              this ExpressionSyntax expression
            , SemanticModel semanticModel
            , CancellationToken token
            , out INamedTypeSymbol generatedType
            , out string tool
        )
        {
            token.ThrowIfCancellationRequested();

            if (semanticModel.GetOperation(expression, token) is IOperation operation)
            {
                return TryFindInOperation(operation, token, out generatedType, out tool);
            }

            generatedType = null;
            tool = null;
            return false;

            static bool TryFindInOperation(
                  IOperation operation
                , CancellationToken token
                , out INamedTypeSymbol generatedType
                , out string tool
            )
            {
                token.ThrowIfCancellationRequested();

                if (operation is INameOfOperation or ITypeOfOperation)
                {
                    generatedType = null;
                    tool = null;
                    return false;
                }

                if (operation is IMemberReferenceOperation memberReference)
                {
                    var containingType = memberReference.Member.ContainingType;

                    while (containingType != null)
                    {
                        token.ThrowIfCancellationRequested();

                        if (containingType.TryGetGeneratingTool(token, out tool))
                        {
                            generatedType = containingType.OriginalDefinition;
                            return true;
                        }

                        containingType = containingType.ContainingType;
                    }
                }

                foreach (var child in operation.ChildOperations)
                {
                    if (TryFindInOperation(child, token, out generatedType, out tool))
                    {
                        return true;
                    }
                }

                generatedType = null;
                tool = null;
                return false;
            }
        }

        public static ImmutableArray<GeneratedMemberReference> FindForwardedGeneratedMemberReferences(
              this ISymbol member
            , SyntaxKind targetKeyword
            , bool includePartialImplementation
            , Compilation compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            using var references = ImmutableArrayBuilder<GeneratedMemberReference>.Rent();

            switch (member)
            {
                case IFieldSymbol:
                {
                    if (GetSingleSyntax(member, token) is VariableDeclaratorSyntax declarator
                        && declarator.Parent?.Parent is FieldDeclarationSyntax fieldDeclaration
                        && fieldDeclaration.Declaration.Variables[0] == declarator
                    )
                    {
                        AddReferences(
                              fieldDeclaration.AttributeLists
                            , targetKeyword
                            , compilation
                            , in references
                            , token
                        );
                    }

                    break;
                }

                case IPropertySymbol:
                {
                    if (GetSingleSyntax(member, token) is PropertyDeclarationSyntax propertyDeclaration)
                    {
                        AddReferences(
                              propertyDeclaration.AttributeLists
                            , targetKeyword
                            , compilation
                            , in references
                            , token
                        );
                    }

                    break;
                }

                case IMethodSymbol method:
                {
                    var definitionPart = method.PartialDefinitionPart ?? method;

                    if (GetSingleSyntax(definitionPart, token) is MethodDeclarationSyntax definitionDeclaration)
                    {
                        AddReferences(
                              definitionDeclaration.AttributeLists
                            , targetKeyword
                            , compilation
                            , in references
                            , token
                        );
                    }

                    if (includePartialImplementation == false)
                    {
                        break;
                    }

                    var implementationPart = method.PartialDefinitionPart != null
                        ? method
                        : method.PartialImplementationPart;

                    if (implementationPart == null)
                    {
                        break;
                    }

                    if (GetSingleSyntax(implementationPart, token) is MethodDeclarationSyntax implementationDeclaration)
                    {
                        AddReferences(
                              implementationDeclaration.AttributeLists
                            , targetKeyword
                            , compilation
                            , in references
                            , token
                        );
                    }

                    break;
                }
            }

            return references.ToImmutable();

            static SyntaxNode GetSingleSyntax(ISymbol symbol, CancellationToken token)
            {
                var declaringReferences = symbol.DeclaringSyntaxReferences;

                return declaringReferences.Length == 1 ? declaringReferences[0].GetSyntax(token) : null;
            }

            static void AddReferences(
                  SyntaxList<AttributeListSyntax> attributeLists
                , SyntaxKind targetKeyword
                , Compilation compilation
                , in ImmutableArrayBuilder<GeneratedMemberReference> references
                , CancellationToken token
            )
            {
                SemanticModel semanticModel = null;

                foreach (var attributeList in attributeLists)
                {
                    token.ThrowIfCancellationRequested();

                    if (attributeList.Target == null || attributeList.Target.Identifier.IsKind(targetKeyword) == false)
                    {
                        continue;
                    }

                    foreach (var attribute in attributeList.Attributes)
                    {
                        token.ThrowIfCancellationRequested();

                        if (attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count < 1)
                        {
                            continue;
                        }

                        semanticModel ??= compilation.GetSemanticModel(attribute.SyntaxTree);

                        if (semanticModel.GetSymbolInfo(attribute, token).TryGetAttributeTypeSymbol(out _) == false)
                        {
                            continue;
                        }

                        foreach (var argument in attribute.ArgumentList.Arguments)
                        {
                            if (argument.Expression.TryFindGeneratedMemberReference(
                                  semanticModel
                                , token
                                , out var generatedType
                                , out var tool
                            ))
                            {
                                references.Add(new(argument.Expression, generatedType, tool));
                            }
                        }
                    }
                }
            }
        }
    }
}
