using EncosyTower.Core.Generators.EnumTemplates;
using EncosyTower.Core.Generators.TypeWraps;

namespace EncosyTower.SourceGen.Tests.Core.TypeWraps;

[TestClass]
public class TypeWrapGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<TypeWrapGenerator>();

    [TestMethod]
    public Task WrapTypeStruct_GeneratesWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int), "value")]
              public partial struct Id { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Id.TypeWrap.7b1c397e3fee068b.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapRecordStruct_GeneratesWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public readonly partial record struct Id(int Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Id.TypeWrap.7b1c397e3fee068b.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapTypeClass_GeneratesWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int), "value")]
              public partial class Points { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Points.TypeWrap.9b9d1a987bc1de4d.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapRecordClass_GeneratesWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public partial record class Score(int Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Score.TypeWrap.090f91e7e768ff52.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapTypeClassAndStruct_KeepsStructOutput()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int), "value")]
              public partial class Points { }

              [WrapType(typeof(int), "value")]
              public partial struct Id { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>(
                      "Points.TypeWrap.9b9d1a987bc1de4d.g.cs"
                    , testMethod: nameof(WrapTypeClass_GeneratesWrapper)
                ),
                ExpectedGeneratedSource.Create<TypeWrapGenerator>(
                      "Id.TypeWrap.7b1c397e3fee068b.g.cs"
                    , testMethod: nameof(WrapTypeStruct_GeneratesWrapper)
                ),
            }
        );

    [TestMethod]
    public Task WrapperOfGeneratedEnum_SkipsOnlyThatWrapper()
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<TypeWrapGenerator>(
              $$"""
              using EncosyTower.TypeWraps;

              namespace TestProject;

              {{ProducerFixtures.SCREEN_TYPE_TEMPLATE}}

              [WrapRecord]
              public readonly partial record struct ScreenScope(ScreenType Screen);

              [WrapType(typeof(ScreenType))]
              public partial struct ScreenValue { }

              [WrapRecord]
              public readonly partial record struct Id(int Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>(
                      "Id.TypeWrap.7b1c397e3fee068b.g.cs"
                    , testMethod: nameof(WrapRecordStruct_GeneratesWrapper)
                ),
            }
            , new IIncrementalGenerator[] { new EnumTemplateGenerator() }
        );

    [TestMethod]
    public Task NonNamedWrappedTypes_SkipOnlyThoseWrappers()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int[]))]
              public partial struct IntArray { }

              [WrapRecord]
              public readonly partial record struct Items(int[] Value);

              [WrapRecord]
              public readonly partial record struct Box<T>(T Value);

              [WrapRecord]
              public readonly partial record struct Id(int Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>(
                      "Id.TypeWrap.7b1c397e3fee068b.g.cs"
                    , testMethod: nameof(WrapRecordStruct_GeneratesWrapper)
                ),
            }
        );

    [TestMethod]
    public Task WrapType_PlainAttribute_GeneratesWrapper()
        => VerifyWrapTypeAsync(
              "[WrapType(typeof(int), \"value\", ExcludeConverter = true)]"
            , nameof(WrapType_PlainAttribute_GeneratesWrapper)
        );

    [DataTestMethod]
    [DataRow("[WrapTypeAttribute(typeof(int), \"value\", ExcludeConverter = true)]")]
    [DataRow("[EncosyTower.TypeWraps.WrapTypeAttribute(typeof(int), \"value\", ExcludeConverter = true)]")]
    [DataRow("[global::EncosyTower.TypeWraps.WrapType(typeof(int), \"value\", ExcludeConverter = true)]")]
    [DataRow("[WrapType(typeof(int), WrapTypeAttribute.DEFAULT_MEMBER_NAME, ExcludeConverter = !false)]")]
    public Task WrapType_AttributeSpellings_ProduceSameWrapper(string attribute)
        => VerifyWrapTypeAsync(attribute, nameof(WrapType_PlainAttribute_GeneratesWrapper));

    [TestMethod]
    public Task WrapType_DefaultMemberName_GeneratesWrapper()
        => VerifyWrapTypeAsync(
              "[WrapType(typeof(int), ExcludeConverter = true)]"
            , nameof(WrapType_DefaultMemberName_GeneratesWrapper)
        );

    [TestMethod]
    public Task WrapType_NullMemberName_UsesDefaultMemberName()
        => VerifyWrapTypeAsync(
              "[WrapType(typeof(int), null, ExcludeConverter = true)]"
            , nameof(WrapType_DefaultMemberName_GeneratesWrapper)
        );

    [TestMethod]
    public Task WrapRecord_PlainAttribute_GeneratesWrapper()
        => VerifyWrapRecordAsync("[WrapRecord(ExcludeConverter = true)]", "int Value");

    [DataTestMethod]
    [DataRow("[WrapRecordAttribute(ExcludeConverter = true)]", "int Value")]
    [DataRow("[EncosyTower.TypeWraps.WrapRecordAttribute(ExcludeConverter = true)]", "int Value")]
    [DataRow("[WrapRecord(ExcludeConverter = !false)]", "int Value")]
    [DataRow("[WrapRecord(ExcludeConverter = true)]", "global::System.Int32 Value")]
    public Task WrapRecord_Spellings_ProduceSameWrapper(string attribute, string parameter)
        => VerifyWrapRecordAsync(attribute, parameter);

    [TestMethod]
    public Task WrapRecordStruct_String_OmitsClone()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public readonly partial record struct Name(string Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Name.TypeWrap.15144ce4828838d9.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapRecordClass_String_OmitsClone()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public partial record class Name(string Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Name.TypeWrap.15144ce4828838d9.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapTypeStruct_String_KeepsClone()
        => GeneratorTestHelper.VerifyGeneratedSourceFragmentsAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(string))]
              public readonly partial struct Name { }
              """
            , new[] { "public object Clone()" }
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task WrapRecordStruct_Tuple_MatchesValueTupleType()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public readonly partial record struct Pair((int, int) Value);
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Pair.TypeWrap.cc299dc5cb885d52.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapTypeStruct_Tuple_MatchesValueTupleType()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              """
              using System;
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(ValueTuple<int, int>), "value")]
              public partial struct Pair { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Pair.TypeWrap.cc299dc5cb885d52.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapRecordStruct_WideTuple_MatchesNestedValueTupleType()
        => GeneratorTestHelper.VerifyGeneratedSourceFragmentsAsync<TypeWrapGenerator>(
              """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapRecord]
              public readonly partial record struct Wide((int, int, int, int, int, int, int, int) Value);
              """
            , new[] {
                "global::System.ValueTuple<int, int, int, int, int, int, int, global::System.ValueTuple<int>> other"
                    + " => this.Value.CompareTo(other),",
            }
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task WrapRecordClass_MutableStruct_OmitsSetters()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              WrapCounter("[WrapRecord]\npublic partial record class Counted(Counter Value);")
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Counted.TypeWrap.2749653df0a8ffe4.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapRecordStruct_MutableStruct_OmitsSetters()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              WrapCounter("[WrapRecord]\npublic partial record struct Counted(Counter Value);")
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Counted.TypeWrap.2749653df0a8ffe4.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapTypeClass_MutableStruct_OmitsSetters()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              WrapCounter("[WrapType(typeof(Counter), \"value\")]\npublic partial class Counted { }")
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>("Counted.TypeWrap.2749653df0a8ffe4.g.cs"),
            }
        );

    [TestMethod]
    public Task WrapTypeStruct_DeclaredReadOnlyField_OmitsSetters()
        => GeneratorTestHelper.VerifyGeneratedSourceFragmentsAsync<TypeWrapGenerator>(
              WrapCounter(
                  "[WrapType(typeof(Counter), \"value\")]\n"
                      + "public partial struct Counted { public readonly Counter value; }"
              )
            , new[] { "get => this.value.Count;" }
            , new[] { "set => this.value" }
        );

    [TestMethod]
    public Task WrapTypeStruct_MutableStruct_KeepsSetters()
        => GeneratorTestHelper.VerifyGeneratedSourceFragmentsAsync<TypeWrapGenerator>(
              WrapCounter("[WrapType(typeof(Counter), \"value\")]\npublic partial struct Counted { }")
            , new[] {
                "set => this.value.Count = value;",
                "set => this.value.Step = value;",
                "set => this.value[index] = value;",
            }
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task WrapTypeClass_DeclaredMutableField_KeepsSetters()
        => GeneratorTestHelper.VerifyGeneratedSourceFragmentsAsync<TypeWrapGenerator>(
              WrapCounter(
                  "[WrapType(typeof(Counter), \"value\")]\n"
                      + "public partial class Counted { public Counter value; }"
              )
            , new[] {
                "set => this.value.Count = value;",
                "set => this.value.Step = value;",
                "set => this.value[index] = value;",
            }
            , Array.Empty<string>()
        );

    [TestMethod]
    public Task ClassWrappers_NullOperands_AreGuarded()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<TypeWrapGenerator>(
              """
              using System;
              using EncosyTower.TypeWraps;

              namespace TestProject;

              public enum Rank { Low, High }

              public readonly struct Tag : IEquatable<Tag>
              {
                  private readonly int _id;

                  public Tag(int id)
                  {
                      _id = id;
                  }

                  public bool Equals(Tag other) => _id == other._id;
              }

              public struct Flags
              {
                  public bool Any;
              }

              public readonly struct Money
              {
                  public static Money Zero;

                  public readonly int Amount;

                  public Money(int amount)
                  {
                      Amount = amount;
                  }

                  public static bool operator <(Money left, int right) => left.Amount < right;

                  public static bool operator >(Money left, int right) => left.Amount > right;

                  public static bool operator <=(int left, Money right) => left <= right.Amount;

                  public static bool operator >=(int left, Money right) => left >= right.Amount;

                  public static Flags operator <(Money left, long right) => default;

                  public static Flags operator >(Money left, long right) => default;
              }

              public sealed class Node
              {
                  public Node Next;
              }

              [WrapType(typeof(int), "value")]
              public partial class Points { }

              [WrapRecord]
              public partial record class Score(int Value);

              [WrapType(typeof(Rank), "value")]
              public partial class RankBox { }

              [WrapType(typeof(Tag), "value")]
              public partial class TagBox { }

              [WrapType(typeof(Money), "value")]
              public partial class Wallet { }

              [WrapType(typeof(Node), "value")]
              public partial class NodeBox { }

              public static class NullOperands
              {
                  public static bool EqualsNull(Points value) => value.Equals(null);

                  public static bool EqualsNullOnTheRight(Points value) => value == null;

                  public static bool EqualsNullOnTheLeft(Points value) => null == value;

                  public static bool NullEqualsNull() => (Points)null == (Points)null;

                  public static bool EqualsSameReference(Points value)
                  {
                      var same = value;
                      return value == same;
                  }

                  public static int CompareToNull(Points value) => value.CompareTo(null);

                  public static int CompareToSameReference(Points value) => value.CompareTo(value);

                  public static bool LessThanNull(Score value) => value < null;

                  public static Points AddNull(Points value) => value + null;

                  public static int ToValue(Points value) => value;
              }
              """
            , expectedSourceCount: 6
            , expectedFragments: new[] {
                "public virtual int CompareTo(global::TestProject.RankBox other)\n"
                    + "        {\n"
                    + "            if (global::System.Object.ReferenceEquals(this, other))\n"
                    + "            {\n"
                    + "                return 0;\n"
                    + "            }\n"
                    + "\n"
                    + "            if (other is null)\n"
                    + "            {\n"
                    + "                return 1;\n"
                    + "            }\n"
                    + "\n"
                    + "            return this.CompareTo(other.value);\n"
                    + "        }",
                "public virtual bool Equals(global::TestProject.RankBox other)\n"
                    + "        {\n"
                    + "            if (global::System.Object.ReferenceEquals(this, other))\n"
                    + "            {\n"
                    + "                return true;\n"
                    + "            }\n"
                    + "\n"
                    + "            if (other is null)\n"
                    + "            {\n"
                    + "                return false;\n"
                    + "            }\n"
                    + "\n"
                    + "            return this.value == other.value;\n"
                    + "        }",
                "public virtual bool Equals(global::TestProject.TagBox other)\n"
                    + "        {\n"
                    + "            if (global::System.Object.ReferenceEquals(this, other))\n"
                    + "            {\n"
                    + "                return true;\n"
                    + "            }\n"
                    + "\n"
                    + "            if (other is null)\n"
                    + "            {\n"
                    + "                return false;\n"
                    + "            }\n"
                    + "\n"
                    + "            return this.value.Equals(other.value);\n"
                    + "        }",
                "public static bool operator ==(global::TestProject.RankBox left, global::TestProject.RankBox right)\n"
                    + "        {\n"
                    + "            if (global::System.Object.ReferenceEquals(left, right))\n"
                    + "            {\n"
                    + "                return true;\n"
                    + "            }\n"
                    + "\n"
                    + "            if (left is null || right is null)\n"
                    + "            {\n"
                    + "                return false;\n"
                    + "            }\n"
                    + "\n"
                    + "            return left.value == right.value;\n"
                    + "        }",
                "public static bool operator <(global::TestProject.Wallet left, int right)\n"
                    + "        {\n"
                    + "            if (left is null)\n"
                    + "            {\n"
                    + "                return true;\n"
                    + "            }",
                "public static bool operator >(global::TestProject.Wallet left, int right)\n"
                    + "        {\n"
                    + "            if (left is null)\n"
                    + "            {\n"
                    + "                return false;\n"
                    + "            }",
                "public static bool operator <=(int left, global::TestProject.Wallet right)\n"
                    + "        {\n"
                    + "            if (right is null)\n"
                    + "            {\n"
                    + "                return false;\n"
                    + "            }",
                "public static bool operator >=(int left, global::TestProject.Wallet right)\n"
                    + "        {\n"
                    + "            if (right is null)\n"
                    + "            {\n"
                    + "                return true;\n"
                    + "            }",
                "public static global::TestProject.Flags operator <(global::TestProject.Wallet left, long right)\n"
                    + "        {\n"
                    + "            if (left is null)\n"
                    + "            {\n"
                    + "                throw global::EncosyTower.Debugging.ThrowHelper"
                    + ".CreateArgumentNullException(nameof(left));\n"
                    + "            }\n"
                    + "\n"
                    + "            return left.value < right;\n"
                    + "        }",
                "public static global::TestProject.RankBox operator -(global::TestProject.RankBox left, int right)\n"
                    + "        {\n"
                    + "            if (left is null)\n"
                    + "            {\n"
                    + "                throw global::EncosyTower.Debugging.ThrowHelper"
                    + ".CreateArgumentNullException(nameof(left));\n"
                    + "            }\n"
                    + "\n"
                    + "            return new global::TestProject.RankBox(left.value - right);\n"
                    + "        }",
                "public static implicit operator global::TestProject.Tag(global::TestProject.TagBox value)\n"
                    + "        {\n"
                    + "            if (value is null)\n"
                    + "            {\n"
                    + "                throw global::EncosyTower.Debugging.ThrowHelper"
                    + ".CreateArgumentNullException(nameof(value));\n"
                    + "            }\n"
                    + "\n"
                    + "            return value.value;\n"
                    + "        }",
                "public static global::TestProject.Wallet Zero\n"
                    + "        {\n"
                    + "            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]\n"
                    + "            get => new global::TestProject.Wallet(global::TestProject.Money.Zero);\n"
                    + "\n"
                    + "            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]\n"
                    + "            set\n"
                    + "            {\n"
                    + "                if (value is null)\n"
                    + "                {\n"
                    + "                    throw global::EncosyTower.Debugging.ThrowHelper"
                    + ".CreateArgumentNullException(nameof(value));\n"
                    + "                }\n"
                    + "\n"
                    + "                global::TestProject.Money.Zero = value.value;\n"
                    + "            }",
                "public global::TestProject.NodeBox Next\n"
                    + "        {\n"
                    + "            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]\n"
                    + "            get => new global::TestProject.NodeBox(this.value.Next);\n"
                    + "\n"
                    + "            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]\n"
                    + "            set\n"
                    + "            {\n"
                    + "                if (value is null)\n"
                    + "                {\n"
                    + "                    throw global::EncosyTower.Debugging.ThrowHelper"
                    + ".CreateArgumentNullException(nameof(value));\n"
                    + "                }\n"
                    + "\n"
                    + "                this.value.Next = value.value;\n"
                    + "            }",
            }
            , unexpectedFragments: Array.Empty<string>()
        );

    private static Task VerifyWrapTypeAsync(string attribute, string snapshotTestMethod)
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              $$"""
              using EncosyTower.TypeWraps;

              namespace TestProject;

              {{attribute}}
              public partial struct Id { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>(
                      "Id.TypeWrap.7b1c397e3fee068b.g.cs"
                    , testMethod: snapshotTestMethod
                ),
            }
        );

    private static Task VerifyWrapRecordAsync(string attribute, string parameter)
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<TypeWrapGenerator>(
              $$"""
              using EncosyTower.TypeWraps;

              namespace TestProject;

              {{attribute}}
              public readonly partial record struct Id({{parameter}});
              """
            , new[] {
                ExpectedGeneratedSource.Create<TypeWrapGenerator>(
                      "Id.TypeWrap.7b1c397e3fee068b.g.cs"
                    , testMethod: nameof(WrapRecord_PlainAttribute_GeneratesWrapper)
                ),
            }
        );

    private const string COUNTER_SOURCE = """
        using EncosyTower.TypeWraps;

        namespace TestProject;

        public struct Counter
        {
            public int Count;

            public int Step { get; set; }

            public int this[int index]
            {
                get => Count;
                set => Count = value;
            }
        }

        {{WRAPPER}}
        """;

    private static string WrapCounter(string wrapper)
        => COUNTER_SOURCE.Replace("{{WRAPPER}}", wrapper);
}
