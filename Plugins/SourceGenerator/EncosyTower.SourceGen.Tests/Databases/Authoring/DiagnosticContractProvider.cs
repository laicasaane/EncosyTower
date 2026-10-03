using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

internal sealed class DiagnosticContractProvider : IDiagnosticContractProvider
{
    public string FeaturePath => "Databases/Authoring";

    public IReadOnlyList<Type> ComponentTypes { get; } = new[] {
        typeof(
            global::EncosyTower.Databases.Authoring.Analyzers
                .ConverterForDataPropertyDiagnosticAnalyzer
        ),
        typeof(global::EncosyTower.Databases.Authoring.Analyzers.ConverterForTableDiagnosticAnalyzer),
        typeof(global::EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer),
    };

    public IReadOnlyList<DiagnosticDescriptorContract> Diagnostics { get; } = new[] {
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.ConverterForDataPropertyDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0080"
            , "Misplaced converter attribute"
            , "The \"{0}\" attribute can only be placed on a type annotated with \"AuthorDatabaseAttribute\""
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.ConverterForTableDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0081"
            , "Misplaced converter attribute"
            , "The \"{0}\" attribute can only be placed on a type annotated with \"AuthorDatabaseAttribute\""
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0010"
            , "Missing default constructor"
            , "The type \"{0}\" must contain a default (parameterless) constructor"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0020"
            , "Static \"Convert\" method ambiguity"
            , "The type \"{0}\" contains multiple public static methods named \"Convert\" thus it cannot be used " +
              "as " +
              "a converter"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0021"
            , "Instanced \"Convert\" method ambiguity"
            , "The type \"{0}\" contains multiple public instanced methods named \"Convert\" thus it cannot be " +
              "used " +
              "as a converter"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0030"
            , "Missing \"Convert\" method"
            , "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that " +
              "accepts a single parameter of any non-void type and returns a value of type \"{1}\""
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0031"
            , "Missing \"Convert\" method"
            , "The type \"{0}\" does not contain any public (static nor instanced) method named \"Convert\" that " +
              "accepts a single parameter of any non-void type and returns a value of any non-void type"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0040"
            , "Invalid static \"Convert\" method"
            , "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void " +
              "type and must return a value of type \"{1}\""
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0041"
            , "Invalid instanced \"Convert\" method"
            , "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any " +
              "non-void type and must return a value of type \"{1}\""
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0042"
            , "Invalid static \"Convert\" method"
            , "The public static \"Convert\" method of type \"{0}\" must accept a single parameter of any non-void " +
              "type and must return a value of any non-void type"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0043"
            , "Invalid instanced \"Convert\" method"
            , "The public instanced \"Convert\" method of type \"{0}\" must accept a single parameter of any " +
              "non-void type and must return a value of any non-void type"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0050"
            , "Not a typeof expression"
            , "The first argument must be a 'typeof' expression"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0051"
            , "Not a typeof expression"
            , "The argument at position {0} must be a 'typeof' expression"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0060"
            , "Abstract type is not supported"
            , "The type \"{0}\" must not be abstract"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0061"
            , "Open generic type is not supported"
            , "The type \"{0}\" must not be open generic"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0070"
            , "Converter ambiguity"
            , "The type \"{0}\" at position {3} will be ignored because a \"Convert\" method that returns a value " +
              "of " +
              "\"{2}\" has already been defined in \"{1}\""
            , "DatabaseGenerator"
            , DiagnosticSeverity.Warning
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0090"
            , "Conflicting data property converters"
            , "Multiple different converters are specified by \"ConverterForDataProperty\" for property \"{0}\" of " +
              "data type \"{1}\" within {2}"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0091"
            , "Redundant data property converter"
            , "The converter \"{0}\" is specified more than once by \"ConverterForDataProperty\" for property " +
              "\"{1}\" of data type \"{2}\" within {3}"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Warning
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0092"
            , "Conflicting table converters"
            , "Multiple different converters are specified by \"ConverterForTable\" for source type \"{0}\" within " +
              "{1}"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0093"
            , "Redundant table converter"
            , "The converter \"{0}\" is specified more than once by \"ConverterForTable\" for source type \"{1}\" " +
              "within {2}"
            , "DatabaseGenerator"
            , DiagnosticSeverity.Warning
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0100"
            , "Invalid horizontal collection selection"
            , "Horizontal selection for property \"{0}\" of type \"{1}\" is invalid: {2}."
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0101"
            , "Invalid generated key equality customization"
            , "Generated key equality customization for \"{0}\" is invalid: {1}."
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , ""
            , ""
            , Array.Empty<string>()
        ),
        new DiagnosticDescriptorContract(
              "EncosyTower.Databases.Authoring.Analyzers.DatabaseAuthoringDiagnosticAnalyzer"
            , "SG_AUTHOR_DATABASE_0110"
            , "Database authoring data member type is generated by another source generator"
            , "\"{0}\" is generated by {3}, so the generator for {1} on \"{2}\" cannot see it. " +
              "Declare \"{0}\" in hand-written source or move it to a referenced assembly."
            , "DatabaseGenerator"
            , DiagnosticSeverity.Error
            , true
            , "Database authoring data member type is generated by another source generator."
            , ""
            , Array.Empty<string>()
        ),
    };

    public IReadOnlyList<SuppressionDescriptorContract> Suppressions => Array.Empty<SuppressionDescriptorContract>();
}
