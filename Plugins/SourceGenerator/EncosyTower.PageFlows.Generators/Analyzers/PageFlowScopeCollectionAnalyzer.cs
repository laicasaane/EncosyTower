using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.PageFlows.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed partial class PageFlowScopeCollectionAnalyzer : DiagnosticAnalyzer
    {
        private const string ATTRIBUTE = "global::EncosyTower.PageFlows.PageFlowScopeCollectionAttribute";
        private const string COLLECTION_INTERFACE = "global::EncosyTower.PageFlows.IPageFlowScopeCollection";
        private const string COLLECTION_INTERFACE_NAME = "IPageFlowScopeCollection";
        private const string SCOPE = "global::EncosyTower.PageFlows.PageFlowScope";
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.PageFlows.SkipSourceGeneratorsForAssemblyAttribute";

        private static readonly SymbolDisplayFormat s_displayFormat = SymbolDisplayFormat.FullyQualifiedFormat;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol type)
            {
                return;
            }

            var attribute = GetMarkerAttribute(type, token);

            if (attribute == null)
            {
                if (ImplementsCollection(type, token)
                    && context.Compilation.Assembly.HasAttribute(SKIP_ATTRIBUTE, token) == false
                )
                {
                    ReportImplementedByHand(context, type, token);
                }

                return;
            }

            if (type.TypeKind != TypeKind.Struct || context.Compilation.Assembly.HasAttribute(SKIP_ATTRIBUTE, token))
            {
                return;
            }

            AnalyzeCollection(context, type, attribute, token);
        }

        private static void AnalyzeCollection(
              SymbolAnalysisContext context
            , INamedTypeSymbol type
            , AttributeData attribute
            , CancellationToken token
        )
        {
            var typeName = type.ToDisplayString(s_displayFormat);
            var hasDeclarationError = false;

            if (TryFindNonPartialDeclaration(type, token, out var nonPartialType, out var nonPartialLocation))
            {
                hasDeclarationError = true;
                context.ReportDiagnostic(Diagnostic.Create(
                      NotPartial
                    , nonPartialLocation
                    , nonPartialType.ToDisplayString(s_displayFormat)
                    , typeName
                ));
            }
            else if (type.IsReadOnly || type.IsRefLikeType)
            {
                hasDeclarationError = true;
                context.ReportDiagnostic(Diagnostic.Create(
                      ReadOnlyOrRef
                    , GetAttributedIdentifierLocation(type, attribute, token)
                    , typeName
                ));
            }

            var hasScope = AnalyzeMembers(context, type, typeName, token);

            if (hasDeclarationError == false && hasScope == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      NoScope
                    , GetAttributedIdentifierLocation(type, attribute, token)
                    , typeName
                ));
            }
        }

        private static bool AnalyzeMembers(
              SymbolAnalysisContext context
            , INamedTypeSymbol type
            , string typeName
            , CancellationToken token
        )
        {
            var hasScope = false;
            var members = type.GetMembers();
            var memberCount = members.Length;

            for (var i = 0; i < memberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                switch (members[i])
                {
                    case IPropertySymbol property when IsScopeTypedProperty(property, token):
                    {
                        if (property.SetMethod is { IsInitOnly: false })
                        {
                            hasScope = true;
                            break;
                        }

                        context.ReportDiagnostic(Diagnostic.Create(
                              PropertyNotScope
                            , GetPropertyIdentifierLocation(property, token)
                            , property.Name
                            , typeName
                        ));
                        break;
                    }

                    case IFieldSymbol field when IsScopeTypedField(field, token):
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              FieldIgnored
                            , GetFieldIdentifierLocation(field, token)
                            , field.Name
                            , typeName
                        ));
                        break;
                    }
                }
            }

            return hasScope;
        }

        private static void ReportImplementedByHand(
              SymbolAnalysisContext context
            , INamedTypeSymbol type
            , CancellationToken token
        )
        {
            var references = type.DeclaringSyntaxReferences;
            var referenceCount = references.Length;
            var location = Location.None;

            for (var i = 0; i < referenceCount; i++)
            {
                token.ThrowIfCancellationRequested();

                if (references[i].GetSyntax(token) is not TypeDeclarationSyntax declaration)
                {
                    continue;
                }

                if (location == Location.None)
                {
                    location = declaration.Identifier.GetLocation();
                }

                if (TryFindInterfaceLocation(declaration, out var interfaceLocation))
                {
                    location = interfaceLocation;
                    break;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(
                  ImplementedByHand
                , location
                , type.ToDisplayString(s_displayFormat)
            ));
        }

        private static bool TryFindInterfaceLocation(TypeDeclarationSyntax declaration, out Location location)
        {
            if (declaration.BaseList != null)
            {
                var baseTypes = declaration.BaseList.Types;
                var baseTypeCount = baseTypes.Count;

                for (var i = 0; i < baseTypeCount; i++)
                {
                    var baseType = baseTypes[i].Type;

                    if (GetRightmostName(baseType) == COLLECTION_INTERFACE_NAME)
                    {
                        location = baseType.GetLocation();
                        return true;
                    }
                }
            }

            location = Location.None;
            return false;
        }

        private static string GetRightmostName(TypeSyntax type)
            => type switch {
                QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText,
                SimpleNameSyntax simple => simple.Identifier.ValueText,
                _ => string.Empty,
            };

        private static AttributeData GetMarkerAttribute(INamedTypeSymbol type, CancellationToken token)
        {
            var attributes = type.GetAttributes();
            var attributeCount = attributes.Length;

            for (var i = 0; i < attributeCount; i++)
            {
                token.ThrowIfCancellationRequested();
                var attribute = attributes[i];

                if (attribute.AttributeClass.HasFullName(ATTRIBUTE, token))
                {
                    return attribute;
                }
            }

            return null;
        }

        private static bool ImplementsCollection(INamedTypeSymbol type, CancellationToken token)
        {
            if (type.TypeKind == TypeKind.Interface)
            {
                return false;
            }

            var interfaces = type.Interfaces;
            var interfaceCount = interfaces.Length;

            for (var i = 0; i < interfaceCount; i++)
            {
                if (interfaces[i].HasFullName(COLLECTION_INTERFACE, token))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsScopeTypedProperty(IPropertySymbol property, CancellationToken token)
            => property.IsStatic == false
            && property.IsIndexer == false
            && property.ExplicitInterfaceImplementations.IsEmpty
            && property.Type.HasFullName(SCOPE, token)
            ;

        private static bool IsScopeTypedField(IFieldSymbol field, CancellationToken token)
            => field.IsStatic == false
            && field.IsImplicitlyDeclared == false
            && field.Type.HasFullName(SCOPE, token)
            ;

        private static bool TryFindNonPartialDeclaration(
              INamedTypeSymbol type
            , CancellationToken token
            , out INamedTypeSymbol nonPartialType
            , out Location location
        )
        {
            var chain = new List<INamedTypeSymbol>();

            for (var current = type; current != null; current = current.ContainingType)
            {
                chain.Add(current);
            }

            for (var i = chain.Count - 1; i >= 0; i--)
            {
                var current = chain[i];
                var references = current.DeclaringSyntaxReferences;
                var referenceCount = references.Length;

                for (var j = 0; j < referenceCount; j++)
                {
                    token.ThrowIfCancellationRequested();

                    if (references[j].GetSyntax(token) is TypeDeclarationSyntax declaration
                        && declaration.Modifiers.Any(SyntaxKind.PartialKeyword) == false
                    )
                    {
                        nonPartialType = current;
                        location = declaration.Identifier.GetLocation();
                        return true;
                    }
                }
            }

            nonPartialType = null;
            location = Location.None;
            return false;
        }

        private static Location GetAttributedIdentifierLocation(
              INamedTypeSymbol type
            , AttributeData attribute
            , CancellationToken token
        )
        {
            var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(token);

            if (syntax?.FirstAncestorOrSelf<TypeDeclarationSyntax>() is TypeDeclarationSyntax declaration)
            {
                return declaration.Identifier.GetLocation();
            }

            return type.Locations.Length > 0 ? type.Locations[0] : Location.None;
        }

        private static Location GetPropertyIdentifierLocation(IPropertySymbol property, CancellationToken token)
        {
            var references = property.DeclaringSyntaxReferences;

            if (references.Length > 0 && references[0].GetSyntax(token) is PropertyDeclarationSyntax declaration)
            {
                return declaration.Identifier.GetLocation();
            }

            return property.Locations.Length > 0 ? property.Locations[0] : Location.None;
        }

        private static Location GetFieldIdentifierLocation(IFieldSymbol field, CancellationToken token)
        {
            var references = field.DeclaringSyntaxReferences;

            if (references.Length > 0 && references[0].GetSyntax(token) is VariableDeclaratorSyntax declarator)
            {
                return declarator.Identifier.GetLocation();
            }

            return field.Locations.Length > 0 ? field.Locations[0] : Location.None;
        }
    }
}
