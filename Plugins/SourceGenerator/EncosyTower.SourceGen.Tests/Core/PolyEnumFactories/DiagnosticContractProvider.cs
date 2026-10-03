using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Core/PolyEnumFactories";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(global::EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0001"
            , "[PolyEnumFactoryFor] target must be partial"
            , "\"{0}\" is decorated with [PolyEnumFactoryFor] but is not declared as partial. Add the partial " +
              "keyword to allow code generation."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Types decorated with [PolyEnumFactoryFor] must be partial so the generator can extend them."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0002"
            , "[PolyEnumFactoryFor] target type must be a [PolyEnumStruct]"
            , "Type \"{0}\" passed to [PolyEnumFactoryFor] is not decorated with [PolyEnumStruct]. Factory " +
              "generation requires a poly-enum struct."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "[PolyEnumFactoryFor(typeof(T))] requires T to be decorated with [PolyEnumStruct]."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0003"
            , "[PolyEnumFactoryFor] target type has no case structs"
            , "Type \"{0}\" has no eligible case structs. The generated factory will only contain an Undefined() " +
              "method."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "The poly-enum struct passed to [PolyEnumFactoryFor] should declare at least one nested case struct."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0005"
            , "Open poly-enum target and factory arity must match"
            , "Open target \"{0}\" has effective arity {1}, but factory \"{2}\" has effective arity {3}."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Fully open poly-enum targets bind factory type parameters positionally and require equal effective " +
              "arity."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0006"
            , "Open poly-enum target and factory constraints must match"
            , "Open target \"{0}\" and factory \"{1}\" have incompatible positional type parameter constraints."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Fully open poly-enum target constraints must be semantically equivalent after positional substitution."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0004"
            , "Case constructor with out parameter is ignored"
            , "Constructor of case struct \"{0}\" has an out parameter and will be skipped by [PolyEnumFactoryFor] " +
              "code generation."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Warning
            , true
            , "Factory methods cannot forward out parameters. Such constructors are ignored when generating " +
              "factories."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0007"
            , "[PolyEnumFactoryFor] record must take the poly-enum struct as its first parameter"
            , "Record \"{0}\" takes \"{1}\" as positional parameter \"{2}\", which is not the first. Make it the " +
              "first positional parameter; no factory code is generated until then."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "A positional record decorated with [PolyEnumFactoryFor] stores the poly-enum struct in its first " +
              "positional parameter. The generator skips a record that declares it at a later position."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0008"
            , "[PolyEnumFactoryFor] wrapper needs a constructor that takes only the poly-enum struct"
            , "\"{0}\" has no constructor that takes \"{1}\" as its only required argument, and this constructor " +
              "keeps the generator from adding one. Add such a constructor; no factory code is generated until then."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "The factory creates the wrapper from the poly-enum struct alone. The generator cannot add that " +
              "constructor to a positional record or next to struct constructors that leave its field unassigned, " +
              "and skips the wrapper."
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Core.Analyzers.PolyEnumFactories.PolyEnumFactoryAnalyzer"
            , "SG_POLY_ENUM_FACTORY_0009"
            , "[PolyEnumFactoryFor] wrapper needs a field or auto-property of the poly-enum struct type"
            , "\"{0}\" has a constructor that takes \"{1}\" but no field or auto-property of type \"{1}\" to " +
              "store it in. Store the value in such a member; no factory code is generated until then."
            , "PolyEnumFactoryGenerator"
            , DiagnosticSeverity.Error
            , true
            , "When the wrapper declares a constructor whose first parameter is the poly-enum struct, the generated " +
              "members read the value from the wrapper's first instance field or auto-property of that type. The " +
              "generator skips a wrapper that declares none."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
