namespace EncosyTower.SourceGen.Tests;

internal readonly record struct GeneratorExpectedOutput(string HintName, string Source);

internal sealed class GeneratorTransitionExpectation
{
    internal GeneratorTransitionExpectation(
          IReadOnlyList<NamedSource> sources
        , IReadOnlyList<GeneratorExpectedOutput> outputs
        , IReadOnlyList<string>? changedHintNames = null
        , bool expectStableOutputSteps = false
    )
    {
        Sources = sources;
        Outputs = outputs;
        ChangedHintNames = changedHintNames ?? Array.Empty<string>();
        ExpectStableOutputSteps = expectStableOutputSteps;
    }

    internal IReadOnlyList<NamedSource> Sources { get; }

    internal IReadOnlyList<GeneratorExpectedOutput> Outputs { get; }

    internal IReadOnlyList<string> ChangedHintNames { get; }

    internal bool ExpectStableOutputSteps { get; }
}

internal sealed class GeneratorContractCase
{
    internal GeneratorContractCase(
          GeneratorTransitionExpectation markerOnly
        , GeneratorTransitionExpectation valid
        , GeneratorTransitionExpectation unrelatedEdit
        , GeneratorTransitionExpectation fileMove
        , GeneratorTransitionExpectation precedingLineEdit
        , GeneratorTransitionExpectation orderEdit
        , GeneratorTransitionExpectation relevantEdit
        , GeneratorTransitionExpectation removal
        , IReadOnlyList<string> ownedTrackingNames
        , IReadOnlyList<string> ownedOutputTrackingNames
    )
    {
        MarkerOnly = markerOnly;
        Valid = valid;
        UnrelatedEdit = unrelatedEdit;
        FileMove = fileMove;
        PrecedingLineEdit = precedingLineEdit;
        OrderEdit = orderEdit;
        RelevantEdit = relevantEdit;
        Removal = removal;
        OwnedTrackingNames = ownedTrackingNames;
        OwnedOutputTrackingNames = ownedOutputTrackingNames;
    }

    internal GeneratorTransitionExpectation MarkerOnly { get; }

    internal GeneratorTransitionExpectation Valid { get; }

    internal GeneratorTransitionExpectation UnrelatedEdit { get; }

    internal GeneratorTransitionExpectation FileMove { get; }

    internal GeneratorTransitionExpectation PrecedingLineEdit { get; }

    internal GeneratorTransitionExpectation OrderEdit { get; }

    internal GeneratorTransitionExpectation RelevantEdit { get; }

    internal GeneratorTransitionExpectation Removal { get; }

    internal IReadOnlyList<string> OwnedTrackingNames { get; }

    internal IReadOnlyList<string> OwnedOutputTrackingNames { get; }
}
