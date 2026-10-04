using System.Collections.Immutable;
using EncosyTower.Core.Generators.Types.Flags;
using EncosyTower.Core.Types.Flags;

namespace EncosyTower.SourceGen.Tests.Core.Types.Flags;

[TestClass]
public sealed class TypeFlagSpecEqualityTests
{
    private static readonly TypeFlagOptions s_defaultOptions = new(
          WriteAccess: TypeFlagAccess.Private
        , Api: TypeFlagApi.Default
        , UseExtensions: false
    );

    [TestMethod]
    public void EqualCopies_AreEqualWithEqualHashes()
    {
        var spec = CreateSpec();
        var copy = CreateSpec();

        Assert.IsTrue(spec.Equals(copy));
        Assert.IsTrue(spec.Equals((object)copy));
        Assert.IsTrue(spec == copy);
        Assert.IsFalse(spec != copy);
        Assert.AreEqual(spec.GetHashCode(), copy.GetHashCode());
    }

    [TestMethod]
    public void ChangedComparedField_ChangesEqualityAndHash()
    {
        var spec = CreateSpec();
        var changes = new (string Name, TypeFlagSpec Spec)[] {
            ("AssemblyName", CreateSpec(assemblyName: "OtherAssembly")),
            ("MetadataName", CreateSpec(metadataName: "OtherProject.Outer`1+Owner")),
            ("Declarations keyword", CreateSpec(outer: new("struct", "T"))),
            ("Declarations type parameter names", CreateSpec(outer: new("class", "TItem"))),
            ("Declarations count", CreateSpec(nested: false)),
            ("IsValueType", CreateSpec(isValueType: true)),
            ("Options.WriteAccess", CreateSpec(options: s_defaultOptions with { WriteAccess = TypeFlagAccess.Public })),
            ("Options.Api", CreateSpec(options: s_defaultOptions with { Api = TypeFlagApi.Related })),
            ("Options.UseExtensions", CreateSpec(options: s_defaultOptions with { UseExtensions = true })),
            ("HiddenMembers", CreateSpec(hiddenMembers: TypeFlagMember.TypeFlagField)),
        };

        foreach (var (name, changed) in changes)
        {
            Assert.IsFalse(spec.Equals(changed), name);
            Assert.IsTrue(spec != changed, name);
            Assert.AreNotEqual(spec.GetHashCode(), changed.GetHashCode(), name);
        }
    }

    [TestMethod]
    public void DefaultSpecs_AreEqualAndInvalid()
    {
        Assert.IsTrue(default(TypeFlagSpec).Equals(default));
        Assert.AreEqual(default(TypeFlagSpec).GetHashCode(), default(TypeFlagSpec).GetHashCode());
        Assert.IsFalse(default(TypeFlagSpec).IsValid);
        Assert.IsTrue(CreateSpec().IsValid);
    }

    private static TypeFlagSpec CreateSpec(
          string assemblyName = "TestProject"
        , string metadataName = "TestProject.Outer`1+Owner"
        , TypeFlagSpec.Declaration? outer = null
        , bool nested = true
        , bool isValueType = false
        , TypeFlagOptions? options = null
        , TypeFlagMember hiddenMembers = TypeFlagMember.None
    )
    {
        var owner = new TypeFlagSpec.Declaration(Copy("class"), string.Empty);
        var declarations = nested
            ? ImmutableArray.Create(outer ?? new(Copy("class"), Copy("T")), owner)
            : ImmutableArray.Create(owner);

        return new TypeFlagSpec(
              Copy(assemblyName)
            , Copy(metadataName)
            , declarations.AsEquatableArray()
            , isValueType
            , options ?? s_defaultOptions
            , hiddenMembers
            , Copy("TestProject")
            , ImmutableArray.Create(Copy("partial class Outer<T>")).AsEquatableArray()
            , Copy("partial class Owner")
            , Copy("global::TestProject.Outer<T>.Owner")
            , Copy("Owner.TypeFlag.0123456789abcdef.g.cs")
        );
    }

    private static string Copy(string value)
        => new(value.AsSpan());
}
