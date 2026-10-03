using EncosyTower.Core.Generators.PolyEnumFactories;
using EncosyTower.Core.Generators.PolyEnumStructs;
using EncosyTower.Core.PolyEnumFactories;
using EncosyTower.Core.PolyEnumStructs;

namespace EncosyTower.SourceGen.Tests.Core;

[TestClass]
public sealed class PolyEnumContainingTypeTests
{
    public static IEnumerable<object[]> EditCases
        => CreateEditCases().Select(static editCase => new object[] { editCase });

    public static IEnumerable<object[]> EqualityCases
        => CreateEqualityCases().Select(static equalityCase => new object[] { equalityCase });

    [TestMethod]
    [DynamicData(nameof(EditCases), DynamicDataSourceType.Property)]
    public Task ReusedDriverEdit_RegeneratesOutput(ReusedDriverEditCase editCase)
        => editCase.VerifyAsync();

    [TestMethod]
    [DynamicData(nameof(EqualityCases), DynamicDataSourceType.Property)]
    public void ChangedField_ChangesEqualityAndHash(SpecEqualityCase equalityCase)
        => equalityCase.Verify();

    private static IEnumerable<ReusedDriverEditCase> CreateEditCases()
    {
        yield return new(
              "PolyEnumStruct containing type kind"
            , static () => new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
            , """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              {{EDIT}}
              {
                  [PolyEnumStruct]
                  public partial struct Choice
                  {
                      public partial struct A { }

                      public partial struct C
                      {
                          public int Value;
                      }
                  }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "PolyEnumStructGenerator.Outputs" }
        );

        yield return new(
              "PolyEnumStruct namespace"
            , static () => new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
            , """
              using EncosyTower.PolyEnumStructs;

              {{EDIT}}
              {
                  [PolyEnumStruct]
                  public partial struct Choice
                  {
                      public partial struct A { }

                      public partial struct C
                      {
                          public int Value;
                      }
                  }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "PolyEnumStructGenerator.Outputs" }
        );

        yield return new(
              "PolyEnumFactory containing type kind"
            , static () => new IIncrementalGenerator[] { new PolyEnumFactoryGenerator(), new PolyEnumStructGenerator() }
            , """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial struct A { }
              }

              {{EDIT}}
              {
                  [PolyEnumFactoryFor(typeof(Choice))]
                  public partial class ChoiceFactory { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "PolyEnumFactoryGenerator.Outputs" }
        );

        yield return new(
              "PolyEnumFactory namespace"
            , static () => new IIncrementalGenerator[] { new PolyEnumFactoryGenerator(), new PolyEnumStructGenerator() }
            , """
              using EncosyTower.PolyEnumStructs;

              namespace Shared
              {
                  [PolyEnumStruct]
                  public partial struct Choice
                  {
                      public partial struct A { }
                  }
              }

              {{EDIT}}
              {
                  [PolyEnumFactoryFor(typeof(Shared.Choice))]
                  public partial class ChoiceFactory { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "PolyEnumFactoryGenerator.Outputs" }
        );

        yield return new(
              "PolyEnumFactory wrapper constraint imported by a global using"
            , static () => new IIncrementalGenerator[] { new PolyEnumFactoryGenerator(), new PolyEnumStructGenerator() }
            , """
              {{EDIT}}
              using EncosyTower.PolyEnumStructs;

              namespace A { public interface IMarker { } }
              namespace B { public interface IMarker { } }

              namespace TestProject
              {
                  [PolyEnumStruct]
                  public partial struct Error<T>
                      where T : unmanaged, IMarker
                  {
                      public partial record struct Invalid(T Data);
                  }

                  [PolyEnumFactoryFor(typeof(Error<>))]
                  public readonly partial struct DataError<U>
                      where U : unmanaged, IMarker
                  {
                  }
              }
              """
            , "global using A;"
            , "global using B;"
            , new[] { "PolyEnumFactoryGenerator.Outputs" }
        );
    }

    private static IEnumerable<SpecEqualityCase> CreateEqualityCases()
    {
        yield return new(
              "PolyEnumStructSpec.containingTypes"
            , static changed => new PolyEnumStructSpec {
                typeName = "Choice",
                typeNamespace = "TestProject",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "PolyEnumStructSpec.StructSpec.size"
            , static changed => new PolyEnumStructSpec.StructSpec {
                name = "A",
                size = changed ? 8 : 4,
            }
        );

        yield return new(
              "PolyEnumFactorySpec.containingTypes"
            , static changed => new PolyEnumFactorySpec {
                wrapperTypeName = "ChoiceFactory",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "PolyEnumFactorySpec.wrapperConstraintIdentity"
            , static changed => new PolyEnumFactorySpec {
                wrapperTypeName = "ChoiceFactory",
                wrapperConstraints = "where T : IMarker",
                wrapperConstraintIdentity = changed ? "where T : global::B.IMarker" : "where T : global::A.IMarker",
            }
        );

        yield return new(
              "PolyEnumFactorySpec.CaseSpec.size"
            , static changed => new PolyEnumFactorySpec.CaseSpec {
                name = "A",
                size = changed ? 8 : 4,
            }
        );

        yield return new(
              "PolyEnumFactorySpec.CtorSpec.isParameterless"
            , static changed => new PolyEnumFactorySpec.CtorSpec {
                isParameterless = changed,
            }
        );

        yield return new(
              "PolyEnumFactorySpec.ParamSpec.isParams"
            , static changed => new PolyEnumFactorySpec.ParamSpec {
                name = "value",
                isParams = changed,
            }
        );

        yield return new(
              "PolyEnumFactorySpec.MemberSpec.isProperty"
            , static changed => new PolyEnumFactorySpec.MemberSpec {
                name = "Value",
                isProperty = changed,
            }
        );

        yield return new(
              "FactoryOutputTypeSpec.IsReadOnly"
            , static changed => new FactoryOutputTypeSpec(
                  "Outer"
                , "struct"
                , "public"
                , string.Empty
                , string.Empty
                , false
                , changed
                , false
                , false
                , CancellationToken.None
            )
        );

        yield return new(
              "SupportTypeSpec.IsReadOnly"
            , static changed => new SupportTypeSpec(
                  "Outer"
                , "struct"
                , "public"
                , string.Empty
                , string.Empty
                , false
                , changed
                , false
                , false
                , CancellationToken.None
            )
        );
    }
}
