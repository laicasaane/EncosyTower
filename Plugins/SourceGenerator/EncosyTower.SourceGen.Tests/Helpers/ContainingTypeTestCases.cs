using System.Collections.Immutable;

namespace EncosyTower.SourceGen.Tests;

/// <summary>
/// One reused-driver edit: the source before and after differ only at <see cref="EDIT"/>.
/// </summary>
public sealed class ReusedDriverEditCase
{
    internal const string EDIT = "{{EDIT}}";
    internal const string CLASS_OUTER = "public partial class Outer";
    internal const string STRUCT_OUTER = "public partial struct Outer";
    internal const string TEST_NAMESPACE = "namespace TestProject";
    internal const string OTHER_NAMESPACE = "namespace OtherProject";

    internal ReusedDriverEditCase(
          string name
        , Func<IReadOnlyList<IIncrementalGenerator>> createGenerators
        , string source
        , string before
        , string after
        , IReadOnlyList<string> modifiedOutputTrackingNames
    )
    {
        Name = name;
        CreateGenerators = createGenerators;
        Source = source;
        Before = before;
        After = after;
        ModifiedOutputTrackingNames = modifiedOutputTrackingNames;
    }

    internal string Name { get; }

    internal Func<IReadOnlyList<IIncrementalGenerator>> CreateGenerators { get; }

    internal string Source { get; }

    internal string Before { get; }

    internal string After { get; }

    internal IReadOnlyList<string> ModifiedOutputTrackingNames { get; }

    internal Task VerifyAsync()
        => GeneratorTestHelper.VerifyReusedDriverEditAsync(
              CreateGenerators()
            , Source.Replace(EDIT, Before)
            , Source.Replace(EDIT, After)
            , ModifiedOutputTrackingNames
        );

    public override string ToString()
        => Name;
}

/// <summary>
/// Two models that differ only in one compared field.
/// </summary>
public sealed class SpecEqualityCase
{
    internal SpecEqualityCase(string name, Func<bool, object> create)
    {
        Name = name;
        Create = create;
    }

    internal string Name { get; }

    internal Func<bool, object> Create { get; }

    internal static EquatableArray<ContainingTypeSpec> Outer(bool changed)
        => ImmutableArray.Create(
            new ContainingTypeSpec(changed ? "struct" : "class", "Outer", string.Empty, string.Empty)
        ).AsEquatableArray();

    internal void Verify()
    {
        var baseline = Create(false);
        var copy = Create(false);
        var changed = Create(true);

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, changed);
        Assert.AreNotEqual(baseline.GetHashCode(), changed.GetHashCode());
    }

    public override string ToString()
        => Name;
}
