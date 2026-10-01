using System.Collections.Immutable;
using EncosyTower.Databases.Authoring.Generators;
using EncosyTower.SourceGen.Data.Helpers;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public sealed class DatabaseAuthoringCompilationSpecEqualityTests
{
    [TestMethod]
    public void DatabaseAuthoringCompilationSpec_EachFactParticipatesInEquality()
    {
        var compilation = new CompilationSpec("Assembly", true);
        var baseline = new DatabaseAuthoringCompilationSpec(compilation, true, true);
        var copy = new DatabaseAuthoringCompilationSpec(compilation, true, true);
        var otherAssembly = compilation with { AssemblyName = "Other" };
        var invalid = compilation with { IsValid = false };

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, baseline with { Compilation = otherAssembly });
        Assert.AreNotEqual(baseline, baseline with { Compilation = invalid });
        Assert.AreNotEqual(baseline, baseline with { DatabaseAuthoring = false });
        Assert.AreNotEqual(baseline, baseline with { BakingSheet = false });
    }

    [TestMethod]
    public void CollectionSpec_RecursiveArgumentsParticipateInEquality()
    {
        var elementType = new TypeSpec { fullName = "System.Int32", simpleName = "Int32", isValueType = true };
        var child = new CollectionSpec {
            kind = CollectionKind.List,
            elementType = elementType,
            argumentCollections = new(default(CollectionSpec)),
        };
        var baseline = new CollectionSpec {
            kind = CollectionKind.Dictionary,
            keyType = new TypeSpec { fullName = "System.String", simpleName = "String" },
            elementType = new TypeSpec { fullName = "System.Collections.Generic.List<int>", simpleName = "List" },
            argumentCollections = new(default(CollectionSpec), child),
        };
        var childCopy = new CollectionSpec {
            kind = CollectionKind.List,
            elementType = elementType,
            argumentCollections = new(default(CollectionSpec)),
        };
        var copy = baseline;
        copy.argumentCollections = new(default(CollectionSpec), childCopy);
        var changed = baseline;
        changed.argumentCollections = new(default(CollectionSpec), default(CollectionSpec));
        var reordered = baseline;
        reordered.argumentCollections = new(child, default(CollectionSpec));
        var defaultValue = default(CollectionSpecArray);

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, changed);
        Assert.AreNotEqual(baseline, reordered);
        Assert.AreEqual(0, defaultValue.Count);
        Assert.AreEqual(defaultValue, default(CollectionSpecArray));
        Assert.AreEqual(defaultValue.GetHashCode(), default(CollectionSpecArray).GetHashCode());
    }

    [TestMethod]
    public void TableSpec_TransposeParticipatesInEquality()
    {
        var baseline = new TableSpec { typeFullName = "Table", propertyName = "Items" };
        var changed = baseline;
        changed.transpose = true;

        Assert.AreNotEqual(baseline, changed);
    }

    [TestMethod]
    public void SheetSpec_LayoutFactsParticipateInEquality()
    {
        var horizontal = new HorizontalCollectionSpec {
            targetTypeFullName = "Data",
            propertyNames = ImmutableArray.Create("Values").AsEquatableArray(),
        };
        var baseline = new SheetSpec {
            idTypeFullName = "Id",
            dataTypeFullName = "Data",
            horizontalCollections = ImmutableArray.Create(horizontal).AsEquatableArray(),
            generatedKeyTypeFullNames = ImmutableArray.Create("Id").AsEquatableArray(),
            generatedKeyEquality = ImmutableArray.Create(new GeneratedKeyEqualitySpec {
                typeFullName = "Id",
                mode = GeneratedKeyEqualityMode.Preserve,
            }).AsEquatableArray(),
        };
        var copy = new SheetSpec {
            idTypeFullName = "Id",
            dataTypeFullName = "Data",
            horizontalCollections = ImmutableArray.Create(horizontal).AsEquatableArray(),
            generatedKeyTypeFullNames = ImmutableArray.Create("Id").AsEquatableArray(),
            generatedKeyEquality = ImmutableArray.Create(new GeneratedKeyEqualitySpec {
                typeFullName = "Id",
                mode = GeneratedKeyEqualityMode.Preserve,
            }).AsEquatableArray(),
        };
        var withoutHorizontal = baseline;
        withoutHorizontal.horizontalCollections = default;
        var withoutKeyRole = baseline;
        withoutKeyRole.generatedKeyTypeFullNames = default;
        var withoutEqualityCustomization = baseline;
        withoutEqualityCustomization.generatedKeyEquality = default;

        Assert.AreEqual(baseline, copy);
        Assert.AreEqual(baseline.GetHashCode(), copy.GetHashCode());
        Assert.AreNotEqual(baseline, withoutHorizontal);
        Assert.AreNotEqual(baseline, withoutKeyRole);
        Assert.AreNotEqual(baseline, withoutEqualityCustomization);
    }
}
