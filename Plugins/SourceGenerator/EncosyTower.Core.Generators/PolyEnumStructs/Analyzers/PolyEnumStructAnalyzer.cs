using EncosyTower.SourceGen.Helpers.PolyEnumStructs;

namespace EncosyTower.Core.Analyzers.PolyEnumStructs
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class PolyEnumStructAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string INTERFACE_NAME = "IEnumCase";
        private const string UNDEFINED_NAME = "Undefined";

        public static readonly DiagnosticDescriptor MustHaveCaseStructs = new(
              id: "SG_POLY_ENUM_STRUCT_0002"
            , title: "No case structs declared in [PolyEnumStruct]"
            , messageFormat: "\"{0}\" has no case structs. At least one nested struct (other than the implicit Undefined case) must be declared."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A [PolyEnumStruct] type must contain at least one nested case struct."
        );

        public static readonly DiagnosticDescriptor IEnumCaseMethodMustNotBeGeneric = new(
              id: "SG_POLY_ENUM_STRUCT_0003"
            , title: "Generic method in IEnumCase interface is not supported"
            , messageFormat: "Method \"{0}\" declared on IEnumCase is generic. Generic methods in IEnumCase are not supported and will be ignored by the generator."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Generic methods declared inside the nested IEnumCase interface are not supported and will be dropped by the generator."
        );

        public static readonly DiagnosticDescriptor CaseStructMethodMustNotBeGeneric = new(
              id: "SG_POLY_ENUM_STRUCT_0004"
            , title: "Generic method on case struct is not supported"
            , messageFormat: "Method \"{0}\" on case struct \"{1}\" is generic. Generic methods on case structs are not supported and will be ignored by the generator."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Generic methods declared on case structs nested inside a [PolyEnumStruct] type are not supported and will be dropped by the generator."
        );

        public static readonly DiagnosticDescriptor GenericEnumExtensionsUnsupported = new(
              id: "SG_POLY_ENUM_STRUCT_0005"
            , title: "Generic [PolyEnumStruct] enum extensions require a container"
            , messageFormat: "\"{0}\" is directly generic. Set Container to an explicit non-generic partial type " +
              "when WithEnumExtensions is enabled."
            , category: "PolyEnumStructGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "A directly generic [PolyEnumStruct] type must provide a valid explicit non-generic " +
              "Container when WithEnumExtensions is enabled."
        );

        #pragma warning disable RS2008
        public static readonly DiagnosticDescriptor ContainerMustBeNonGeneric = CreateError(
              "SG_POLY_ENUM_STRUCT_0006"
            , "Poly-enum container must be non-generic"
            , "Container \"{0}\" for \"{1}\" must be non-generic, including its containing types."
        );

        public static readonly DiagnosticDescriptor ContainerMustBeSameAssembly = CreateError(
              "SG_POLY_ENUM_STRUCT_0007"
            , "Poly-enum container must be in the target assembly"
            , "Container \"{0}\" must be declared in the same assembly as \"{1}\"."
        );

        public static readonly DiagnosticDescriptor ContainerKindUnsupported = CreateError(
              "SG_POLY_ENUM_STRUCT_0008"
            , "Poly-enum container cannot contain generated types"
            , "Container \"{0}\" cannot contain generated PolyEnum types."
        );

        public static readonly DiagnosticDescriptor TypeParameterMappingInvalid = CreateError(
              "SG_POLY_ENUM_STRUCT_0009"
            , "Poly-enum type parameters do not map uniquely"
            , "Type parameters on \"{0}\" do not map uniquely to \"{1}\"."
        );

        public static readonly DiagnosticDescriptor TypeParameterConstraintsMismatch = CreateError(
              "SG_POLY_ENUM_STRUCT_0010"
            , "Poly-enum type parameter constraints differ"
            , "Type parameter constraints on \"{0}\" must match \"{1}\"."
        );

        public static readonly DiagnosticDescriptor CaseMissingInterfaceParameter = CreateError(
              "SG_POLY_ENUM_STRUCT_0011"
            , "Poly-enum case omits an interface dependency"
            , "Case \"{0}\" must include type parameter \"{1}\" required by IEnumCase."
        );

        public static readonly DiagnosticDescriptor InterfaceMustMoveToContainer = CreateError(
              "SG_POLY_ENUM_STRUCT_0012"
            , "Poly-enum interface must move to the container"
            , "Move IEnumCase for \"{0}\" into its non-generic PolyEnum container."
        );

        public static readonly DiagnosticDescriptor InterfaceMissingMemberParameter = CreateError(
              "SG_POLY_ENUM_STRUCT_0013"
            , "Poly-enum interface omits a member dependency"
            , "IEnumCase \"{0}\" must include type parameter \"{1}\" required by common member \"{2}\"."
        );

        public static readonly DiagnosticDescriptor InterfaceMemberDuplicated = CreateError(
              "SG_POLY_ENUM_STRUCT_0014"
            , "Poly-enum interface member is duplicated"
            , "Member \"{0}\" is already declared by non-generic IEnumCase."
        );

        public static readonly DiagnosticDescriptor UndefinedCaseDuplicated = CreateError(
              "SG_POLY_ENUM_STRUCT_0015"
            , "Poly-enum has multiple undefined cases"
            , "PolyEnum \"{0}\" has more than one authored undefined case."
        );
        #pragma warning restore RS2008

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustHaveCaseStructs
                , IEnumCaseMethodMustNotBeGeneric
                , CaseStructMethodMustNotBeGeneric
                , GenericEnumExtensionsUnsupported
                , ContainerMustBeNonGeneric
                , ContainerMustBeSameAssembly
                , ContainerKindUnsupported
                , TypeParameterMappingInvalid
                , TypeParameterConstraintsMismatch
                , CaseMissingInterfaceParameter
                , InterfaceMustMoveToContainer
                , InterfaceMissingMemberParameter
                , InterfaceMemberDuplicated
                , UndefinedCaseDuplicated
            );

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

            if (context.Symbol is not INamedTypeSymbol typeSymbol
                || typeSymbol.HasAttribute(POLY_ENUM_STRUCT_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(POLY_ENUM_STRUCT_ATTRIBUTE, token);
            var location = attrib?.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? typeSymbol.Locations[0];

            var resolution = ContainerResolver.Resolve(typeSymbol, attrib, context.Compilation, token);

            if (resolution.Kind != ContainerResolutionKind.Valid)
            {
                ReportResolutionDiagnostic(context, typeSymbol, attrib, resolution);
                return;
            }

            if (typeSymbol.TypeParameters.Length > 0
                && resolution.Container is null
                && HasEnumExtensionsEnabled(attrib)
            )
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      GenericEnumExtensionsUnsupported
                    , location
                    , typeSymbol.Name
                ));
                return;
            }

            token.ThrowIfCancellationRequested();

            var parentName = typeSymbol.Name;
            var verboseUndefinedName = $"{parentName}_Undefined";
            var validCaseCount = 0;

            foreach (var @case in resolution.Cases)
            {
                token.ThrowIfCancellationRequested();
                var nested = @case.Symbol;

                if (@case.IsUndefined)
                {
                    continue;
                }

                token.ThrowIfCancellationRequested();

                validCaseCount++;

                foreach (var member in nested.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IMethodSymbol method
                        && method.MethodKind == MethodKind.Ordinary
                        && method.TypeParameters.Length > 0
                        && method.ExplicitInterfaceImplementations.Length == 0
                    )
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                              CaseStructMethodMustNotBeGeneric
                            , method.Locations.Length > 0 ? method.Locations[0] : location
                            , method.Name
                            , nested.Name
                        ));
                    }
                }
            }

            ReportGenericInterfaceMethods(context, resolution.BaseInterface, location);
            ReportGenericInterfaceMethods(context, resolution.GenericInterface, location);

            if (validCaseCount == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustHaveCaseStructs, location, parentName));
            }
        }

        private static bool HasEnumExtensionsEnabled(AttributeData attribute)
        {
            foreach (var argument in attribute.NamedArguments)
            {
                if (string.Equals(argument.Key, "WithEnumExtensions", StringComparison.Ordinal)
                    && argument.Value.Value is true
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static DiagnosticDescriptor CreateError(string id, string title, string message)
            => new(
                  id
                , title
                , message
                , "PolyEnumStructGenerator"
                , DiagnosticSeverity.Error
                , true
                , title
            );

        private static void ReportResolutionDiagnostic(
              SymbolAnalysisContext context
            , INamedTypeSymbol target
            , AttributeData attribute
            , ContainerResolution resolution
        )
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();
            var attributeLocation = attribute.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? target.Locations[0];
            var invalidType = resolution.InvalidType;
            var typeLocation = invalidType?.Locations.FirstOrDefault() ?? attributeLocation;
            var descriptor = resolution.Kind switch {
                ContainerResolutionKind.GenericContainer => ContainerMustBeNonGeneric,
                ContainerResolutionKind.CrossAssemblyContainer => ContainerMustBeSameAssembly,
                ContainerResolutionKind.InvalidContainerKind => ContainerKindUnsupported,
                ContainerResolutionKind.ParameterMapping => TypeParameterMappingInvalid,
                ContainerResolutionKind.ConstraintMismatch => TypeParameterConstraintsMismatch,
                ContainerResolutionKind.MissingInterfaceDependency => CaseMissingInterfaceParameter,
                ContainerResolutionKind.InterfaceInsideTarget => InterfaceMustMoveToContainer,
                ContainerResolutionKind.InterfaceDependency => InterfaceMissingMemberParameter,
                ContainerResolutionKind.DuplicateInterfaceMember => InterfaceMemberDuplicated,
                ContainerResolutionKind.DuplicateUndefined => UndefinedCaseDuplicated,
                _ => null,
            };

            if (descriptor is null)
            {
                return;
            }

            var diagnosticLocation = resolution.Kind is ContainerResolutionKind.GenericContainer
                or ContainerResolutionKind.CrossAssemblyContainer
                or ContainerResolutionKind.InvalidContainerKind
                    ? GetContainerLocation(attribute, token) ?? attributeLocation
                    : typeLocation;
            object[] arguments = resolution.Kind switch {
                ContainerResolutionKind.GenericContainer => new object[] { invalidType.Name, target.Name },
                ContainerResolutionKind.CrossAssemblyContainer => new object[] { invalidType.Name, target.Name },
                ContainerResolutionKind.InvalidContainerKind => new object[] { invalidType.Name },
                ContainerResolutionKind.ParameterMapping => new object[] { invalidType.Name, target.Name },
                ContainerResolutionKind.ConstraintMismatch => new object[] { invalidType.Name, target.Name },
                ContainerResolutionKind.MissingInterfaceDependency => new object[] {
                    invalidType.Name,
                    resolution.MissingParameter.Name,
                },
                ContainerResolutionKind.InterfaceInsideTarget => new object[] { target.Name },
                ContainerResolutionKind.InterfaceDependency => new object[] {
                    invalidType.Name,
                    resolution.MissingParameter.Name,
                    resolution.MemberName,
                },
                ContainerResolutionKind.DuplicateInterfaceMember => new object[] { resolution.MemberName },
                ContainerResolutionKind.DuplicateUndefined => new object[] { target.Name },
                _ => Array.Empty<object>(),
            };
            context.ReportDiagnostic(Diagnostic.Create(descriptor, diagnosticLocation, arguments));
        }

        private static Location GetContainerLocation(AttributeData attribute, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (attribute.ApplicationSyntaxReference?.GetSyntax(token) is not AttributeSyntax syntax)
            {
                return null;
            }

            foreach (var argument in syntax.ArgumentList?.Arguments ?? default)
            {
                token.ThrowIfCancellationRequested();

                if (string.Equals(
                      argument.NameEquals?.Name.Identifier.ValueText
                    , "Container"
                    , StringComparison.Ordinal
                ))
                {
                    return argument.Expression.GetLocation();
                }
            }

            return null;
        }

        private static void ReportGenericInterfaceMethods(
              SymbolAnalysisContext context
            , INamedTypeSymbol interfaceSymbol
            , Location fallback
        )
        {
            if (interfaceSymbol is null)
            {
                return;
            }

            foreach (var member in interfaceSymbol.GetMembers())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (member is IMethodSymbol method
                    && method.MethodKind == MethodKind.Ordinary
                    && method.TypeParameters.Length > 0
                )
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                          IEnumCaseMethodMustNotBeGeneric
                        , method.Locations.FirstOrDefault() ?? fallback
                        , method.Name
                    ));
                }
            }
        }
    }
}
