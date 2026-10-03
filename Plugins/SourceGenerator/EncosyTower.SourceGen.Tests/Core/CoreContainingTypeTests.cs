using EncosyTower.Core.Generators.NewtonsoftAotHelpers;
using EncosyTower.Core.Generators.TypeWraps;
using EncosyTower.Core.Generators.Types.Caches;

namespace EncosyTower.SourceGen.Tests.Core;

[TestClass]
public sealed class CoreContainingTypeTests
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
              "TypeWrap containing type kind"
            , static () => new IIncrementalGenerator[] { new TypeWrapGenerator() }
            , """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              {{EDIT}}
              {
                  [WrapType(typeof(int), "value")]
                  public partial struct Id { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "TypeWrapGenerator.Outputs" }
        );

        yield return new(
              "RuntimeTypeCaches containing type kind"
            , static () => new IIncrementalGenerator[] { new RuntimeTypeCachesGenerator() }
            , """
              using System;
              using EncosyTower.Types;

              namespace TestProject;

              public class SpecialAttribute : Attribute { }

              {{EDIT}}
              {
                  public partial class Usage
                  {
                      public void Execute()
                      {
                          RuntimeTypeCache.GetInfo<SpecialAttribute>();
                      }
                  }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "RuntimeTypeCachesGenerator.Outputs" }
        );

        yield return new(
              "NewtonsoftJsonAotHelper containing type kind"
            , static () => new IIncrementalGenerator[] { new NewtonsoftJsonAotHelperGenerator() }
            , """
              using EncosyTower.Serialization.NewtonsoftJson;

              namespace TestProject;

              public class BaseModel { }

              public sealed class ConcreteDerived : BaseModel { }

              {{EDIT}}
              {
                  [NewtonsoftJsonAotHelper(typeof(BaseModel))]
                  public static partial class Helper { }
              }
              """
            , ReusedDriverEditCase.CLASS_OUTER
            , ReusedDriverEditCase.STRUCT_OUTER
            , new[] { "NewtonsoftJsonAotHelperGenerator.Outputs" }
        );

        yield return new(
              "NewtonsoftJsonAotHelper namespace"
            , static () => new IIncrementalGenerator[] { new NewtonsoftJsonAotHelperGenerator() }
            , """
              using EncosyTower.Serialization.NewtonsoftJson;

              namespace Shared
              {
                  public class BaseModel { }

                  public sealed class ConcreteDerived : BaseModel { }
              }

              {{EDIT}}
              {
                  [NewtonsoftJsonAotHelper(typeof(Shared.BaseModel))]
                  public static partial class Helper { }
              }
              """
            , ReusedDriverEditCase.TEST_NAMESPACE
            , ReusedDriverEditCase.OTHER_NAMESPACE
            , new[] { "NewtonsoftJsonAotHelperGenerator.Outputs" }
        );

        yield return new(
              "TypeWrap target kind"
            , static () => new IIncrementalGenerator[] { new TypeWrapGenerator() }
            , """
              using System.Collections.Generic;
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(List<int>), "value")]
              public partial {{EDIT}} Ids { }
              """
            , "struct"
            , "class"
            , new[] { "TypeWrapGenerator.Outputs" }
        );

        yield return new(
              "TypeWrap readonly modifier"
            , static () => new IIncrementalGenerator[] { new TypeWrapGenerator() }
            , """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int), "value")]
              public {{EDIT}}partial struct Id { }
              """
            , ""
            , "readonly "
            , new[] { "TypeWrapGenerator.Outputs" }
        );

        yield return new(
              "TypeWrap declared backing field"
            , static () => new IIncrementalGenerator[] { new TypeWrapGenerator() }
            , """
              using EncosyTower.TypeWraps;

              namespace TestProject;

              [WrapType(typeof(int), "value")]
              public partial struct Id
              {
                  {{EDIT}}
              }
              """
            , ""
            , "public int value;"
            , new[] { "TypeWrapGenerator.Outputs" }
        );
    }

    private static IEnumerable<SpecEqualityCase> CreateEqualityCases()
    {
        yield return new(
              "TypeWrapSpec.containingTypes"
            , static changed => new TypeWrapSpec {
                fullTypeName = "global::TestProject.Outer.Id",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "RuntimeTypeCachesGenerator.PartialTypeSpec.containingTypes"
            , static changed => new RuntimeTypeCachesGenerator.PartialTypeSpec {
                containingTypeFullName = "global::TestProject.Outer.Usage",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "RuntimeTypeCachesGenerator.TypeSpec.containingTypes"
            , static changed => new RuntimeTypeCachesGenerator.TypeSpec {
                containingTypeFullName = "global::TestProject.Outer.Usage",
                containingTypes = SpecEqualityCase.Outer(changed),
            }
        );

        yield return new(
              "TypeWrapSpec.isStruct"
            , static changed => new TypeWrapSpec {
                fullTypeName = "global::TestProject.Ids",
                isStruct = changed,
            }
        );

        yield return new(
              "TypeWrapSpec.isReadOnly"
            , static changed => new TypeWrapSpec {
                fullTypeName = "global::TestProject.Id",
                isReadOnly = changed,
            }
        );

        yield return new(
              "TypeWrapSpec.isFieldDeclared"
            , static changed => new TypeWrapSpec {
                fullTypeName = "global::TestProject.Id",
                isFieldDeclared = changed,
            }
        );

        yield return new(
              "FieldSpec.isReadOnly"
            , static changed => new FieldSpec {
                name = "MaxValue",
                typeName = "global::System.Int32",
                isReadOnly = changed,
            }
        );

        yield return new(
              "EventSpec.isStatic"
            , static changed => new EventSpec {
                name = "Changed",
                typeName = "global::System.Action",
                isStatic = changed,
            }
        );

        yield return new(
              "PropertySpec.hasSetter"
            , static changed => new PropertySpec {
                name = "Count",
                typeName = "global::System.Int32",
                hasSetter = changed,
            }
        );

        yield return new(
              "MethodSpec.typeParameterConstraints"
            , static changed => new MethodSpec {
                name = "Convert",
                returnTypeName = "T",
                typeParameters = "<T>",
                typeParameterConstraints = changed ? "where T : global::B.IMarker" : "where T : global::A.IMarker",
            }
        );
    }
}
