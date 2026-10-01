using System.Collections.Immutable;
using EncosyTower.Core.Generators.PolyEnumStructs;
using EncosyTower.Core.PolyEnumStructs;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

[TestClass]
public sealed class PolyEnumStructSpecEqualityTests
{
    [TestMethod]
    public void PolyEnumStructSpec_GenericOutputFactsParticipateInEquality()
    {
        var baseline = new PolyEnumStructSpec {
            typeName = "Error",
            typeSelfName = "Error<T>",
            typeFullName = "global::TestProject.Error<T>",
            typeConstraints = "where T : unmanaged",
            enumExtensionsAttributeOwner = "global::TestProject.Owner<>",
            typeNamespace = "TestProject",
            typeIdentifier = "TestProject_Error_1",
            hintName = "TestProject.Error`1",
            openingSource = "namespace TestProject {",
            closingSource = "}",
            typeAccessibility = Accessibility.Public,
            supportContainer = CreateSupportContainer("Cases"),
            targetOutputScope = CreateSupportContainer("Error"),
            targetParameters = new[] {
                new PolyEnumStructSpec.TypeParameterSpec { name = "T", constraint = "where T : unmanaged" },
            }.ToImmutableArray().AsEquatableArray(),
            genericInterfaceTargetIndices = new[] { 0 }.ToImmutableArray().AsEquatableArray(),
            separateContainer = true,
        };
        var copy = baseline;

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { typeSelfName = "Error<U>" });
        Assert.AreNotEqual(baseline, baseline with { typeFullName = "global::Other.Error<T>" });
        Assert.AreNotEqual(baseline, baseline with { typeConstraints = "where T : struct" });
        Assert.AreNotEqual(baseline, baseline with { enumExtensionsAttributeOwner = "global::Other.Owner<>" });
        Assert.AreNotEqual(baseline, baseline with { typeIdentifier = "Other" });
        Assert.AreNotEqual(baseline, baseline with { hintName = "Other" });
        Assert.AreNotEqual(baseline, baseline with { openingSource = "namespace Other {" });
        Assert.AreNotEqual(baseline, baseline with { closingSource = "}}" });
        Assert.AreNotEqual(baseline, baseline with { typeAccessibility = Accessibility.Internal });
        Assert.AreNotEqual(baseline, baseline with { supportContainer = CreateSupportContainer("Other") });
        Assert.AreNotEqual(baseline, baseline with { targetOutputScope = CreateSupportContainer("Other") });
        Assert.AreNotEqual(baseline, baseline with { targetParameters = default });
        Assert.AreNotEqual(baseline, baseline with { genericInterfaceTargetIndices = default });
        Assert.AreNotEqual(baseline, baseline with { withEnumExtensions = true });
        Assert.AreNotEqual(baseline, baseline with { parentIsNamespace = true });
        Assert.AreNotEqual(baseline, baseline with { separateContainer = false });
    }

    private static SupportContainerSpec CreateSupportContainer(string name)
        => new(
              $"global::TestProject.{name}"
            , $"TestProject.{name}"
            , "TestProject"
            , default
            , new SupportTypeSpec(
                  name
                , "class"
                , "internal"
                , string.Empty
                , string.Empty
                , true
                , false
                , false
                , false
                , CancellationToken.None
            )
            , CancellationToken.None
        );
}
