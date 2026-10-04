using System.Globalization;
using EncosyTower.Core.Types.Flags;

namespace EncosyTower.Core.Analyzers.Types.Flags
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed partial class TypeFlagAnalyzer : DiagnosticAnalyzer
    {
        private const string ACCESS_ENUM_NAME = "TypeFlagAccess";
        private const string API_ENUM_NAME = "TypeFlagApi";

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private static void AnalyzeType(SymbolAnalysisContext context)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            if (context.Symbol is not INamedTypeSymbol owner
                || owner.HasAttribute(TypeFlagRules.ATTRIBUTE, token) == false
            )
            {
                return;
            }

            var compilation = context.Compilation;

            if (compilation.IsValidCompilation(token, TypeFlagRules.NAMESPACE, TypeFlagRules.SKIP_ATTRIBUTE) == false)
            {
                return;
            }

            var marker = compilation.GetTypeByMetadataName(TypeFlagRules.ATTRIBUTE_METADATA_NAME);
            var typeFlag = compilation.GetTypeByMetadataName(TypeFlagRules.TYPE_FLAG_METADATA_NAME);

            if (marker == null
                || typeFlag == null
                || TypeFlagRules.CountMarkers(owner, marker, token) != 1
                || owner.TypeKind is not (TypeKind.Class or TypeKind.Struct)
            )
            {
                return;
            }

            var ownerName = owner.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            var attribute = TypeFlagRules.GetMarker(owner, marker, token);
            var markedLocation = GetMarkedIdentifierLocation(owner, attribute, token);

            if (TypeFlagRules.TryGetUnsupportedKind(owner, out var kind))
            {
                context.ReportDiagnostic(Diagnostic.Create(UnsupportedOwner, markedLocation, ownerName, kind));
                return;
            }

            var values = TypeFlagRules.ReadOptions(attribute, token);

            if (values.HasErrorValue || ReportOptions(context, attribute, values, ownerName, markedLocation))
            {
                return;
            }

            if (TypeFlagRules.HasUnresolvedBase(owner, token))
            {
                return;
            }

            for (var baseType = owner.BaseType; baseType != null; baseType = baseType.BaseType)
            {
                token.ThrowIfCancellationRequested();

                if (baseType.OriginalDefinition.TryFindEncosyTowerGeneratedType(token, out var generated, out var tool))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          GeneratedBase
                        , markedLocation
                        , ownerName
                        , generated.ToGeneratedTypeFullName()
                        , tool
                    ));

                    return;
                }
            }

            var members = TypeFlagRules.GetMembers(values.ToOptions());

            for (var bit = 1; bit <= 8; bit <<= 1)
            {
                token.ThrowIfCancellationRequested();

                var member = (TypeFlagMember)bit;

                if ((members & member) != 0)
                {
                    ReportConflict(context, owner, ownerName, member, marker, typeFlag, markedLocation);
                }
            }
        }

        private static bool ReportOptions(
              SymbolAnalysisContext context
            , AttributeData attribute
            , in TypeFlagOptionValues values
            , string ownerName
            , Location markedLocation
        )
        {
            var token = context.CancellationToken;

            if (values.UseExtensions && values.HasApi)
            {
                var location = GetArgumentLocation(attribute, TypeFlagRules.API_OPTION, markedLocation, token);
                context.ReportDiagnostic(Diagnostic.Create(IneffectiveApi, location, ownerName));
            }

            var reported = false;

            if (values.IsWriteAccessDefined == false)
            {
                ReportUndefinedOption(
                      context
                    , attribute
                    , TypeFlagRules.WRITE_ACCESS_OPTION
                    , values.WriteAccess
                    , ACCESS_ENUM_NAME
                    , ownerName
                    , markedLocation
                );

                reported = true;
            }

            if (values.UseExtensions == false && values.IsApiDefined == false)
            {
                ReportUndefinedOption(
                      context
                    , attribute
                    , TypeFlagRules.API_OPTION
                    , values.Api
                    , API_ENUM_NAME
                    , ownerName
                    , markedLocation
                );

                reported = true;
            }

            return reported;
        }

        private static void ReportUndefinedOption(
              SymbolAnalysisContext context
            , AttributeData attribute
            , string option
            , long value
            , string enumName
            , string ownerName
            , Location markedLocation
        )
        {
            var location = GetArgumentLocation(attribute, option, markedLocation, context.CancellationToken);

            context.ReportDiagnostic(Diagnostic.Create(
                  UndefinedOption
                , location
                , option
                , Convert.ToString(value, CultureInfo.InvariantCulture)
                , enumName
                , ownerName
            ));
        }

        private static void ReportConflict(
              SymbolAnalysisContext context
            , INamedTypeSymbol owner
            , string ownerName
            , TypeFlagMember member
            , INamedTypeSymbol marker
            , INamedTypeSymbol typeFlag
            , Location markedLocation
        )
        {
            var token = context.CancellationToken;
            var plan = TypeFlagRules.PlanMember(
                  owner
                , member
                , marker
                , typeFlag
                , context.Compilation
                , token
                , out var conflict
            );

            if (plan != TypeFlagMemberPlan.Conflict)
            {
                return;
            }

            var isOwnHandWrittenMember = SymbolEqualityComparer.Default.Equals(conflict.ContainingType, owner)
                && conflict.HasAttribute(TypeFlagRules.GENERATED_CODE_ATTRIBUTE, token) == false;

            var location = isOwnHandWrittenMember ? GetFirstSourceLocation(conflict, markedLocation) : markedLocation;
            var memberName = TypeFlagRules.GetMemberName(member);

            context.ReportDiagnostic(Diagnostic.Create(MemberNameConflict, location, ownerName, memberName));
        }

        private static Location GetFirstSourceLocation(ISymbol symbol, Location fallback)
        {
            foreach (var location in symbol.Locations)
            {
                if (location.IsInSource)
                {
                    return location;
                }
            }

            return fallback;
        }

        private static Location GetMarkedIdentifierLocation(
              INamedTypeSymbol owner
            , AttributeData attribute
            , CancellationToken token
        )
        {
            var syntax = attribute.ApplicationSyntaxReference?.GetSyntax(token);

            if (syntax?.FirstAncestorOrSelf<TypeDeclarationSyntax>() is TypeDeclarationSyntax declaration)
            {
                return declaration.Identifier.GetLocation();
            }

            return owner.Locations.Length > 0 ? owner.Locations[0] : Location.None;
        }

        private static Location GetArgumentLocation(
              AttributeData attribute
            , string option
            , Location fallback
            , CancellationToken token
        )
        {
            if (attribute.ApplicationSyntaxReference?.GetSyntax(token) is not AttributeSyntax syntax
                || syntax.ArgumentList == null
            )
            {
                return fallback;
            }

            foreach (var argument in syntax.ArgumentList.Arguments)
            {
                token.ThrowIfCancellationRequested();

                if (argument.NameEquals != null
                    && string.Equals(argument.NameEquals.Name.Identifier.ValueText, option, StringComparison.Ordinal)
                )
                {
                    return argument.GetLocation();
                }
            }

            return fallback;
        }
    }
}
