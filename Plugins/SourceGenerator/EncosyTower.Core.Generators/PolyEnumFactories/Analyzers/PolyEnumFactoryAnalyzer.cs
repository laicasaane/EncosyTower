using EncosyTower.Core.PolyEnumFactories;
using EncosyTower.SourceGen.Helpers.PolyEnumStructs;

namespace EncosyTower.Core.Analyzers.PolyEnumFactories
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class PolyEnumFactoryAnalyzer : DiagnosticAnalyzer
    {
        private const string NAMESPACE = "EncosyTower.PolyEnumStructs";
        private const string POLY_ENUM_FACTORY_FOR_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumFactoryForAttribute";
        private const string POLY_ENUM_STRUCT_ATTRIBUTE = $"global::{NAMESPACE}.PolyEnumStructAttribute";
        private const string ENUM_CASE_IGNORE_ATTRIBUTE = $"global::{NAMESPACE}.EnumCaseIgnoreAttribute";
        private const string CATEGORY = "PolyEnumFactoryGenerator";

        public static readonly DiagnosticDescriptor MustBePartial = new(
              id: "SG_POLY_ENUM_FACTORY_0001"
            , title: "[PolyEnumFactoryFor] target must be partial"
            , messageFormat: "\"{0}\" is decorated with [PolyEnumFactoryFor] but is not declared as partial. Add the partial keyword to allow code generation."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Types decorated with [PolyEnumFactoryFor] must be partial so the generator can extend them."
        );

        public static readonly DiagnosticDescriptor TargetMustBePolyEnumStruct = new(
              id: "SG_POLY_ENUM_FACTORY_0002"
            , title: "[PolyEnumFactoryFor] target type must be a [PolyEnumStruct]"
            , messageFormat: "Type \"{0}\" passed to [PolyEnumFactoryFor] is not decorated with [PolyEnumStruct]. Factory generation requires a poly-enum struct."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "[PolyEnumFactoryFor(typeof(T))] requires T to be decorated with [PolyEnumStruct]."
        );

        public static readonly DiagnosticDescriptor MustHaveCaseStructs = new(
              id: "SG_POLY_ENUM_FACTORY_0003"
            , title: "[PolyEnumFactoryFor] target type has no case structs"
            , messageFormat: "Type \"{0}\" has no eligible case structs. The generated factory will only contain an Undefined() method."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The poly-enum struct passed to [PolyEnumFactoryFor] should declare at least one nested case struct."
        );

        public static readonly DiagnosticDescriptor CaseCtorOutParameterIgnored = new(
              id: "SG_POLY_ENUM_FACTORY_0004"
            , title: "Case constructor with out parameter is ignored"
            , messageFormat: "Constructor of case struct \"{0}\" has an out parameter and will be skipped by [PolyEnumFactoryFor] code generation."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Factory methods cannot forward out parameters. Such constructors are ignored when generating factories."
        );

        public static readonly DiagnosticDescriptor TargetArityMismatch = new(
              id: "SG_POLY_ENUM_FACTORY_0005"
            , title: "Open poly-enum target and factory arity must match"
            , messageFormat: "Open target \"{0}\" has effective arity {1}, but factory \"{2}\" has effective arity {3}."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Fully open poly-enum targets bind factory type parameters positionally and require " +
              "equal effective arity."
        );

        public static readonly DiagnosticDescriptor TargetConstraintMismatch = new(
              id: "SG_POLY_ENUM_FACTORY_0006"
            , title: "Open poly-enum target and factory constraints must match"
            , messageFormat: "Open target \"{0}\" and factory \"{1}\" have incompatible positional type parameter " +
              "constraints."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Fully open poly-enum target constraints must be semantically equivalent after positional " +
              "substitution."
        );

        public static readonly DiagnosticDescriptor EnumStructMustBeFirstParameter = new(
              id: "SG_POLY_ENUM_FACTORY_0007"
            , title: "[PolyEnumFactoryFor] record must take the poly-enum struct as its first parameter"
            , messageFormat: "Record \"{0}\" takes \"{1}\" as positional parameter \"{2}\", which is not the first. " +
              "Make it the first positional parameter; no factory code is generated until then."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "A positional record decorated with [PolyEnumFactoryFor] stores the poly-enum struct in " +
              "its first positional parameter. The generator skips a record that declares it at a later position."
        );

        public static readonly DiagnosticDescriptor WrapperNeedsEnumStructConstructor = new(
              id: "SG_POLY_ENUM_FACTORY_0008"
            , title: "[PolyEnumFactoryFor] wrapper needs a constructor that takes only the poly-enum struct"
            , messageFormat: "\"{0}\" has no constructor that takes \"{1}\" as its only required argument, and this " +
              "constructor keeps the generator from adding one. Add such a constructor; no factory code is generated " +
              "until then."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "The factory creates the wrapper from the poly-enum struct alone. The generator cannot " +
              "add that constructor to a positional record or next to struct constructors that leave its field " +
              "unassigned, and skips the wrapper."
        );

        public static readonly DiagnosticDescriptor WrapperNeedsEnumStructStorage = new(
              id: "SG_POLY_ENUM_FACTORY_0009"
            , title: "[PolyEnumFactoryFor] wrapper needs a field or auto-property of the poly-enum struct type"
            , messageFormat: "\"{0}\" has a constructor that takes \"{1}\" but no field or auto-property of type " +
              "\"{1}\" to store it in. Store the value in such a member; no factory code is generated until then."
            , category: CATEGORY
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "When the wrapper declares a constructor whose first parameter is the poly-enum struct, " +
              "the generated members read the value from the wrapper's first instance field or auto-property of " +
              "that type. The generator skips a wrapper that declares none."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(
                  MustBePartial
                , TargetMustBePolyEnumStruct
                , MustHaveCaseStructs
                , CaseCtorOutParameterIgnored
                , TargetArityMismatch
                , TargetConstraintMismatch
                , EnumStructMustBeFirstParameter
                , WrapperNeedsEnumStructConstructor
                , WrapperNeedsEnumStructStorage
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
                || typeSymbol.HasAttribute(POLY_ENUM_FACTORY_FOR_ATTRIBUTE, token) == false
            )
            {
                return;
            }

            var attrib = typeSymbol.GetAttribute(POLY_ENUM_FACTORY_FOR_ATTRIBUTE, token);
            var attribLocation = attrib?.ApplicationSyntaxReference?.GetSyntax(token)?.GetLocation()
                ?? typeSymbol.Locations[0];
            var typeLocation = typeSymbol.Locations.Length > 0 ? typeSymbol.Locations[0] : attribLocation;

            if (IsDeclaredPartial(typeSymbol, token) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustBePartial, typeLocation, typeSymbol.Name));
            }

            if (attrib == null
                || attrib.ConstructorArguments.Length < 1
                || attrib.ConstructorArguments[0].Value is not INamedTypeSymbol enumStructSymbol
            )
            {
                return;
            }

            if (enumStructSymbol.OriginalDefinition.HasAttribute(POLY_ENUM_STRUCT_ATTRIBUTE, token) == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TargetMustBePolyEnumStruct
                    , attribLocation
                    , enumStructSymbol.Name
                ));
                return;
            }

            var polyEnumAttribute = enumStructSymbol.OriginalDefinition.GetAttribute(
                  POLY_ENUM_STRUCT_ATTRIBUTE
                , token
            );
            var containerResolution = ContainerResolver.Resolve(
                  enumStructSymbol.OriginalDefinition
                , polyEnumAttribute
                , context.Compilation
                , token
            );

            if (containerResolution.Kind != ContainerResolutionKind.Valid)
            {
                return;
            }

            var resolution = FactoryTargetResolver.Resolve(
                  enumStructSymbol
                , typeSymbol
                , context.Compilation
                , token
            );

            if (resolution.Kind == ResolutionKind.ArityMismatch)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TargetArityMismatch
                    , attribLocation
                    , enumStructSymbol.Name
                    , FactoryTargetResolver.GetEffectiveArity(enumStructSymbol, token)
                    , typeSymbol.Name
                    , FactoryTargetResolver.GetEffectiveArity(typeSymbol, token)
                ));
                return;
            }

            if (resolution.Kind == ResolutionKind.ConstraintMismatch)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      TargetConstraintMismatch
                    , attribLocation
                    , enumStructSymbol.Name
                    , typeSymbol.Name
                ));
                return;
            }

            if (resolution.Kind != ResolutionKind.Valid)
            {
                return;
            }

            enumStructSymbol = resolution.Target;

            var wrapperShape = FactoryWrapperRules.GetWrapperShape(
                  typeSymbol
                , enumStructSymbol
                , token
                , out var recordParameter
                , out var blockingConstructor
                , out _
            );

            if (wrapperShape == WrapperShape.LaterRecordParameter)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      EnumStructMustBeFirstParameter
                    , recordParameter.Locations[0]
                    , typeSymbol.Name
                    , enumStructSymbol.Name
                    , recordParameter.Name
                ));
            }
            else if (wrapperShape == WrapperShape.BlockingConstructor)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      WrapperNeedsEnumStructConstructor
                    , GetParameterListLocation(blockingConstructor, token)
                    , typeSymbol.Name
                    , enumStructSymbol.Name
                ));
            }
            else if (wrapperShape == WrapperShape.MissingStorage)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                      WrapperNeedsEnumStructStorage
                    , GetParameterListLocation(blockingConstructor, token)
                    , typeSymbol.Name
                    , enumStructSymbol.Name
                ));
            }


            token.ThrowIfCancellationRequested();

            var hasCase = false;

            foreach (var @case in containerResolution.Cases)
            {
                token.ThrowIfCancellationRequested();
                var nested = @case.Symbol;

                if (nested.TypeKind != TypeKind.Struct)
                {
                    continue;
                }

                if (nested.HasAttribute(ENUM_CASE_IGNORE_ATTRIBUTE, token))
                {
                    continue;
                }

                hasCase = true;
                ReportOutParamCtors(context, nested);
            }

            if (hasCase == false)
            {
                context.ReportDiagnostic(Diagnostic.Create(MustHaveCaseStructs, attribLocation, enumStructSymbol.Name));
            }
        }

        private static Location GetParameterListLocation(IMethodSymbol constructor, CancellationToken token)
        {
            foreach (var reference in constructor.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax(token);

                if (syntax is RecordDeclarationSyntax recordSyntax && recordSyntax.ParameterList is not null)
                {
                    return recordSyntax.ParameterList.GetLocation();
                }

                if (syntax is ConstructorDeclarationSyntax constructorSyntax)
                {
                    return constructorSyntax.ParameterList.GetLocation();
                }
            }

            return constructor.Locations[0];
        }

        private static bool IsDeclaredPartial(INamedTypeSymbol typeSymbol, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
            {
                token.ThrowIfCancellationRequested();

                if (syntaxRef.GetSyntax(token) is TypeDeclarationSyntax typeSyntax)
                {
                    foreach (var modifier in typeSyntax.Modifiers)
                    {
                        token.ThrowIfCancellationRequested();

                        if (modifier.IsKind(SyntaxKind.PartialKeyword))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static void ReportOutParamCtors(SymbolAnalysisContext context, INamedTypeSymbol caseSymbol)
        {
            var token = context.CancellationToken;
            token.ThrowIfCancellationRequested();

            foreach (var ctor in caseSymbol.InstanceConstructors)
            {
                token.ThrowIfCancellationRequested();

                if (ctor.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (ctor.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                var hasOut = false;

                foreach (var p in ctor.Parameters)
                {
                    token.ThrowIfCancellationRequested();

                    if (p.RefKind == RefKind.Out)
                    {
                        hasOut = true;
                        break;
                    }
                }

                if (hasOut == false)
                {
                    continue;
                }

                var location = ctor.Locations.Length > 0 ? ctor.Locations[0] : caseSymbol.Locations[0];

                context.ReportDiagnostic(Diagnostic.Create(CaseCtorOutParameterIgnored, location, caseSymbol.Name));
            }
        }
    }
}
