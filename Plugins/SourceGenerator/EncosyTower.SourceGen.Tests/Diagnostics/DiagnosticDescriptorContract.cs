namespace EncosyTower.SourceGen.Tests.Diagnostics;

internal readonly record struct DiagnosticDescriptorContract(
      string OwnerType
    , string Id
    , string Title
    , string MessageFormat
    , string Category
    , DiagnosticSeverity DefaultSeverity
    , bool IsEnabledByDefault
    , string Description
    , string HelpLinkUri
    , string[] CustomTags
);

internal readonly record struct SuppressionDescriptorContract(
      string SuppressorType
    , string SuppressionId
    , string SuppressedDiagnosticId
    , string Justification
);
