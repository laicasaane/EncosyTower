using EncosyTower.Core.Generators.PolyEnumFactories;
using EncosyTower.Core.PolyEnumFactories;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

[TestClass]
public sealed class PolyEnumFactorySpecEqualityTests
{
    [TestMethod]
    public void PolyEnumFactorySpec_GenericOutputFactsParticipateInEquality()
    {
        var baseline = new PolyEnumFactorySpec {
            wrapperTypeName = "DataError",
            wrapperSelfName = "DataError<T>",
            wrapperConstraints = "where T : unmanaged",
            wrapperTypeNamespace = "TestProject",
            wrapperKindKeyword = "struct",
            enumStructTypeName = "global::TestProject.Error<T>",
            enumCaseTypeName = "global::TestProject.Error.EnumCase",
            supportTypeName = "global::TestProject.Error",
            fieldName = "_enumStruct_Error",
            hintName = "TestProject.DataError`1",
            openingSource = "namespace TestProject {",
            closingSource = "}",
            separateTypeContainer = true,
            typeContainer = CreateOutputScope("DataError"),
            wrapperOutputScope = CreateOutputScope("DataErrorGeneric"),
        };
        var copy = baseline;

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { wrapperSelfName = "DataError<U>" });
        Assert.AreNotEqual(baseline, baseline with { wrapperConstraints = "where T : struct" });
        Assert.AreNotEqual(baseline, baseline with { hintName = "Other" });
        Assert.AreNotEqual(baseline, baseline with { openingSource = "namespace Other {" });
        Assert.AreNotEqual(baseline, baseline with { closingSource = "}}" });
        Assert.AreNotEqual(baseline, baseline with { enumCaseTypeName = "global::Other.EnumCase" });
        Assert.AreNotEqual(baseline, baseline with { supportTypeName = "global::Other.Error" });
        Assert.AreNotEqual(baseline, baseline with { separateTypeContainer = false });
        Assert.AreNotEqual(baseline, baseline with { typeContainer = CreateOutputScope("Other") });
        Assert.AreNotEqual(baseline, baseline with { wrapperOutputScope = CreateOutputScope("Other") });
    }

    private static FactoryOutputScopeSpec CreateOutputScope(string name)
        => new(
              $"global::TestProject.{name}"
            , $"TestProject.{name}"
            , "TestProject"
            , default
            , new FactoryOutputTypeSpec(
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
