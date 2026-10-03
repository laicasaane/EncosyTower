using EncosyTower.Core.Generators.PolyEnumFactories;
using EncosyTower.Core.Generators.PolyEnumStructs;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumFactories;

[TestClass]
public class PolyEnumFactoryGeneratorTests
{
    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumFactoryGenerator>();

    [TestMethod]
    public Task ChoiceFactory_GeneratesFactoryAndPolyEnum()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial struct A { }
              }

              [PolyEnumFactoryFor(typeof(Choice))]
              public partial class ChoiceFactory { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumFactoryGenerator>(
                    "ChoiceFactory.PolyEnumFactory.49747cb4cbc3c1c5.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Choice.PolyEnumStruct.a18c245f1860e08b.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task CaseName_PreservesAuthoredSpelling()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial struct A { }
              }

              [PolyEnumFactoryFor(typeof(Choice))]
              public partial class ChoiceFactory { }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "public static ChoiceFactory A()",
            }
            , unexpectedFragments: new[] {
                "public static ChoiceFactory I_A()",
            }
            , additionalGenerators: new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task GenericErrorFactory_BindsTargetPositionally()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Error<T>
                  where T : unmanaged
              {
                  public partial record struct Invalid(T Data);
              }

              [PolyEnumFactoryFor(typeof(Error<>))]
              public readonly partial struct DataError<U>
                  where U : unmanaged
              {
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumFactoryGenerator>(
                    "DataError_1.PolyEnumFactory.65dd023f3f36fc4f.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumFactoryGenerator>(
                    "DataError_1.PolyEnumFactoryContainer.95454be34ebfaac7.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Error_1.PolyEnumStruct.08b7e4163a5b52ff.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Error_1.PolyEnumStructContainer.b12038ff4a8f19f7.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task ThreeParameterTarget_BindsPositionally()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;
              namespace TestProject;
              [PolyEnumStruct]
              public partial struct Error<TValue, TCode, TContext>
              {
                  public partial record struct Invalid(TValue Data, TCode Code, TContext Context);
              }
              [PolyEnumFactoryFor(typeof(Error<,,>))]
              public readonly partial struct DataError<UValue, UCode, UContext> { }
              """
            , 2
            , new[] {
                "private readonly global::TestProject.Error<UValue, UCode, UContext> _enumStruct_Error;",
                "public static DataError<UValue, UCode, UContext> Invalid(UValue data, UCode code, UContext context)",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task TargetNestedInDifferentGenericOwner_BindsFlattenedVector()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;
              namespace TestProject;
              public partial class ErrorOwner<TOuter>
              {
                  [PolyEnumStruct]
                  public partial struct Error<TInner>
                  {
                      public partial record struct Invalid(TOuter Outer, TInner Inner);
                  }
              }
              [PolyEnumFactoryFor(typeof(ErrorOwner<>.Error<>))]
              public readonly partial struct DataError<UOuter, UInner> { }
              """
            , 2
            , new[] {
                "private readonly global::TestProject.ErrorOwner<UOuter>.Error<UInner> _enumStruct_Error;",
                "public static DataError<UOuter, UInner> Invalid(UOuter outer, UInner inner)",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task ClosedTarget_WithGenericFactory_KeepsExactTarget()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;
              namespace TestProject;
              [PolyEnumStruct]
              public partial struct Error<T>
              {
                  public partial record struct Invalid(T Data);
              }
              [PolyEnumFactoryFor(typeof(Error<int>))]
              public readonly partial struct DataError<TUnused> { }
              """
            , 2
            , new[] {
                "private readonly global::TestProject.Error<int> _enumStruct_Error;",
                "public static DataError<TUnused> Invalid(int data)",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task SameOwnerNestedTarget_RebindsThroughFactory()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;
              namespace TestProject;
              [PolyEnumFactoryFor(typeof(DataError<,,>.Error))]
              public readonly partial struct DataError<TValue, TCode, TContext>
              {
                  [PolyEnumStruct(WithEnumExtensions = true)]
                  public partial struct Error
                  {
                      public partial record struct Invalid(TValue Data, TCode Code, TContext Context);
                  }
              }
              """
            , 2
            , new[] {
                "private readonly Error _enumStruct_Error;",
                "public static DataError<TValue, TCode, TContext> Invalid(TValue data, TCode code, TContext context)",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task SameOwnerNestedTarget_InGlobalNamespace_GeneratesEnumExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;
              [PolyEnumFactoryFor(typeof(DataError<>.Error))]
              public readonly partial struct DataError<T>
              {
                  [PolyEnumStruct(WithEnumExtensions = true)]
                  public partial struct Error
                  {
                      public partial record struct Invalid(T Data);
                  }
              }
              """
            , 2
            , new[] {
                "private readonly Error _enumStruct_Error;",
                "public static DataError<T> Invalid(T data)",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task FactoryNestedInDifferentGenericOwner_BindsFlattenedVector()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;
              namespace TestProject;
              [PolyEnumStruct]
              public partial struct Error<TOuter, TInner>
              {
                  public partial record struct Invalid(TOuter Outer, TInner Inner);
              }
              public partial class Consumer<UOuter>
              {
                  [PolyEnumFactoryFor(typeof(Error<,>))]
                  public readonly partial struct DataError<UInner> { }
              }
              """
            , 2
            , new[] {
                "private readonly global::TestProject.Error<UOuter, UInner> _enumStruct_Error;",
                "public static DataError<UInner> Invalid(UOuter outer, UInner inner)",
            }
            , Array.Empty<string>()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task OpenTarget_WithArityMismatch_ProducesNoFactoryOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumFactoryGenerator>(
            """
            using EncosyTower.PolyEnumStructs;
            namespace TestProject;
            [PolyEnumStruct]
            public partial struct Error<T1, T2>
            {
                public partial struct Invalid { }
            }
            [PolyEnumFactoryFor(typeof(Error<,>))]
            public readonly partial struct DataError<T> { }
            """
        );

    [TestMethod]
    public Task OpenTarget_WithConstraintMismatch_ProducesNoFactoryOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumFactoryGenerator>(
            """
            using EncosyTower.PolyEnumStructs;
            namespace TestProject;
            [PolyEnumStruct]
            public partial struct Error<T>
                where T : unmanaged
            {
                public partial struct Invalid { }
            }
            [PolyEnumFactoryFor(typeof(Error<>))]
            public readonly partial struct DataError<U>
                where U : struct
            {
            }
            """
        );

    [TestMethod]
    public Task DependencyResultFactory_ConstructsMappedContainerCases()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              #pragma warning disable CS0436

              namespace EncosyTower.PolyEnumStructs
              {
                  [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
                  public sealed class PolyEnumStructAttribute : System.Attribute
                  {
                      public System.Type Container { get; set; }
                  }

                  [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
                  public sealed class PolyEnumFactoryForAttribute : System.Attribute
                  {
                      public PolyEnumFactoryForAttribute(System.Type type) { }
                  }
              }

              namespace TestProject
              {
                  [PolyEnumStruct(Container = typeof(DependencyResultCases))]
                  internal partial struct DependencyResult<TValue, TError>
                  {
                  }

                  internal static partial class DependencyResultCases
                  {
                      internal readonly partial record struct None;
                      internal readonly partial record struct Success<TValue>(TValue Value);
                      internal readonly partial record struct Failure<TError>(TError Error);
                      internal readonly partial record struct Pair<TValue, TError>(TValue Value, TError Error);
                  }

                  [PolyEnumFactoryFor(typeof(DependencyResult<,>))]
                  internal readonly partial struct DependencyResultFactory<TValue, TError>
                  {
                  }

                  internal static class Consumer
                  {
                      internal static bool Use()
                      {
                          var none = DependencyResultFactory<int, string>.None();
                          var success = DependencyResultFactory<int, string>.Success(42);
                          var failure = DependencyResultFactory<int, string>.Failure("invalid");
                          var pair = DependencyResultFactory<int, string>.Pair(42, "invalid");
                          return none.Is(DependencyResultFactory.Type.None)
                              && success.Is(DependencyResultFactory.Type.Success)
                              && failure.Is(DependencyResultFactory.Type.Failure)
                              && pair.Is(DependencyResultFactory.Type.Pair);
                      }
                  }
              }
              """
            , expectedSourceCount: 2
            , expectedFragments: new[] {
                "internal static partial class DependencyResultFactory",
                "public enum Type : byte",
                "None = global::TestProject.DependencyResultCases.EnumCase.None",
                "Success = global::TestProject.DependencyResultCases.EnumCase.Success",
                "public static DependencyResultFactory<TValue, TError> None()",
                "default(global::TestProject.DependencyResultCases.None)",
                "public static DependencyResultFactory<TValue, TError> Success(TValue value)",
                "new global::TestProject.DependencyResultCases.Success<TValue>(value)",
                "new global::TestProject.DependencyResultCases.Failure<TError>(error)",
                "new global::TestProject.DependencyResultCases.Pair<TValue, TError>(value, error)",
                "public bool Is(global::TestProject.DependencyResultFactory.Type type)",
            }
            , unexpectedFragments: new[] {
                "partial struct DependencyResultFactory<TValue, TError> // Type\n",
                "DependencyResult<TValue, TError>.EnumCase",
            }
            , additionalGenerators: new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [DataTestMethod]
    [DataRow(
          """
          [PolyEnumStruct]
          public partial struct Result<T>
              where T : IMarker
          {
              public partial record struct Ok(T Value);
          }

          [PolyEnumFactoryFor(typeof(Result<>))]
          public readonly partial struct ResultFactory<T>
              where T : IMarker
          {
          }
          """
    )]
    [DataRow(
          """
          [PolyEnumStruct]
          public partial struct Choice
          {
              public partial struct A { }
          }

          public partial class Outer<T>
              where T : IMarker
          {
              [PolyEnumFactoryFor(typeof(Choice))]
              public readonly partial struct ChoiceFactory { }
          }
          """
    )]
    public Task SeparateContainer_ImportedConstraint_CompilesFullyQualified(string declarations)
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              $$"""
              using EncosyTower.PolyEnumStructs;
              using Markers;

              namespace Markers
              {
                  public interface IMarker { }
              }

              namespace TestProject
              {
              {{declarations}}
              }
              """
            , expectedSourceCount: 2
            , expectedFragments: new[] { "where T : global::Markers.IMarker" }
            , unexpectedFragments: new[] { " : IMarker" }
            , additionalGenerators: new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task RecordWrapper_EnumStructParameter_StoresValueInParameter()
        => VerifyWrapperAsync(
              "[PolyEnumFactoryFor(typeof(Choice))]\n"
                + "public readonly partial record struct ChoiceFactory(Choice Value);"
            , nameof(RecordWrapper_EnumStructParameter_StoresValueInParameter)
            , CHOICE_FACTORY_HINT
        );

    [DataTestMethod]
    [DataRow("public readonly partial record struct ChoiceFactory(TestProject.Choice Value);")]
    [DataRow("public readonly partial record struct ChoiceFactory(global::TestProject.Choice Value);")]
    [DataRow(
          "public readonly partial record struct ChoiceFactory;\n\n"
            + "public readonly partial record struct ChoiceFactory(Choice Value);"
    )]
    public Task RecordWrapper_EnumStructParameterSpellings_ProduceSameFactory(string wrapper)
        => VerifyWrapperAsync(
              "[PolyEnumFactoryFor(typeof(Choice))]\n" + wrapper
            , nameof(RecordWrapper_EnumStructParameter_StoresValueInParameter)
            , CHOICE_FACTORY_HINT
        );

    [TestMethod]
    public Task RecordWrapper_ClosedGenericParameter_StoresValueInParameter()
        => VerifyWrapperAsync(
              "[PolyEnumFactoryFor(typeof(Result<int>))]\n"
                + "public readonly partial record struct ResultFactory(Result<int> Value);"
            , nameof(RecordWrapper_ClosedGenericParameter_StoresValueInParameter)
            , "ResultFactory.PolyEnumFactory.2abf286cb371986d.g.cs"
        );

    [TestMethod]
    public Task RecordWrapper_OpenGenericParameter_StoresValueInParameter()
        => VerifyWrapperAsync(
              "[PolyEnumFactoryFor(typeof(Result<>))]\n"
                + "public readonly partial record struct ResultFactory<T>(Result<T> Value);"
            , nameof(RecordWrapper_OpenGenericParameter_StoresValueInParameter)
            , "ResultFactory_1.PolyEnumFactory.43216dfeaab827a6.g.cs"
            , "ResultFactory_1.PolyEnumFactoryContainer.72e2429a07d5eb5d.g.cs"
        );

    [TestMethod]
    public Task ClassWrapper_AutoPropertySetByConstructor_StoresValueInProperty()
        => VerifyWrapperAsync(
              """
              [PolyEnumFactoryFor(typeof(Choice))]
              public partial class ChoiceFactory
              {
                  public Choice Value { get; }

                  public ChoiceFactory(Choice value)
                  {
                      Value = value;
                  }
              }
              """
            , nameof(ClassWrapper_AutoPropertySetByConstructor_StoresValueInProperty)
            , CHOICE_FACTORY_HINT
        );

    [DataTestMethod]
    [DataRow("public readonly partial record struct ChoiceFactory(int Id, Choice Value);")]
    [DataRow("public partial record class ChoiceFactory(int Id, Choice Value);")]
    public Task RecordWrapper_EnumStructNotFirstParameter_ProducesNoFactoryOutput(string wrapper)
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumFactoryGenerator>(ChoiceWrapperSource(wrapper));

    [DataTestMethod]
    [DataRow("public readonly partial record struct ChoiceFactory(int Id);")]
    [DataRow("public readonly partial record struct ChoiceFactory();")]
    [DataRow("public partial record class ChoiceFactory(int Id);")]
    [DataRow("public readonly partial record struct ChoiceFactory(Choice First, Choice Second);")]
    [DataRow(
          """
          public readonly partial struct ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(int id, Choice value)
              {
                  _value = value;
              }
          }
          """
    )]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(Choice value, int id)
              {
                  _value = value;
              }
          }
          """
    )]
    public Task Wrapper_ConstructorBlocksFactory_ProducesNoFactoryOutput(string wrapper)
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumFactoryGenerator>(ChoiceWrapperSource(wrapper));

    [DataTestMethod]
    [DataRow("public readonly partial record struct ChoiceFactory;")]
    [DataRow("public readonly partial record struct ChoiceFactory(Choice Value, int Id = 0);")]
    [DataRow(
          """
          public readonly partial record struct ChoiceFactory(Choice First, Choice Second)
          {
              public ChoiceFactory(Choice value) : this(value, default)
              {
              }
          }
          """
    )]
    [DataRow(
          """
          public partial record class ChoiceFactory(int Id)
          {
              private readonly Choice _value;

              public ChoiceFactory(Choice value) : this(0)
              {
                  _value = value;
              }
          }
          """
    )]
    [DataRow(
          """
          public readonly partial struct ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(Choice value)
              {
                  _value = value;
              }
          }
          """
    )]
    [DataRow(
          """
          public readonly partial struct ChoiceFactory
          {
              public ChoiceFactory(int id) : this()
              {
              }
          }
          """
    )]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(int id, Choice value)
              {
                  _value = value;
              }
          }
          """
    )]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(in Choice value)
              {
                  _value = value;
              }
          }
          """
    )]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(Choice value, int id)
              {
                  _value = value;
              }

              public ChoiceFactory(object value)
              {
                  _value = (Choice)value;
              }
          }
          """
    )]
    public Task Wrapper_SupportedConstructorShape_GeneratesCompilingFactory(string wrapper)
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              ChoiceWrapperSource(wrapper)
            , expectedSourceCount: 1
            , expectedFragments: new[] { "public static ChoiceFactory A()" }
            , unexpectedFragments: Array.Empty<string>()
            , additionalGenerators: new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [DataTestMethod]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              private readonly object _boxed;

              public ChoiceFactory(Choice value)
              {
                  _boxed = value;
              }
          }
          """
    )]
    [DataRow(
          """
          public readonly partial struct ChoiceFactory
          {
              public static Choice Default { get; }

              public ChoiceFactory(Choice value)
              {
              }
          }
          """
    )]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              public Choice Value => default;

              public ChoiceFactory(in Choice value)
              {
              }
          }
          """
    )]
    [DataRow(
          """
          public partial record class ChoiceFactory(int Id)
          {
              public ChoiceFactory(Choice value) : this(0)
              {
              }
          }
          """
    )]
    public Task Wrapper_EnumStructNotStored_ProducesNoFactoryOutput(string wrapper)
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumFactoryGenerator>(ChoiceWrapperSource(wrapper));

    [DataTestMethod]
    [DataRow(
          """
          public partial class ChoiceFactory
          {
              private readonly Choice _value;

              public ChoiceFactory(Choice value)
              {
                  _value = value;
              }
          }
          """
        , "return this._value.GetEnumCase()"
    )]
    [DataRow(
          """
          public readonly partial struct ChoiceFactory
          {
              public Choice Value { get; }

              public ChoiceFactory(Choice value)
              {
                  Value = value;
              }
          }
          """
        , "return this.Value.GetEnumCase()"
    )]
    [DataRow("public partial class ChoiceFactory { }", "return this._enumStruct_Choice.GetEnumCase()")]
    [DataRow("public partial record class ChoiceFactory(Choice Value);", "return this.Value.GetEnumCase()")]
    public Task Wrapper_EnumStructStorage_GeneratesFactoryReadingStorage(string wrapper, string storageRead)
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumFactoryGenerator>(
              ChoiceWrapperSource(wrapper)
            , expectedSourceCount: 1
            , expectedFragments: new[] { "public static ChoiceFactory A()", storageRead }
            , unexpectedFragments: Array.Empty<string>()
            , additionalGenerators: new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    private static string ChoiceWrapperSource(string wrapper)
        => $$"""
            using EncosyTower.PolyEnumStructs;

            namespace TestProject;

            [PolyEnumStruct]
            public partial struct Choice
            {
                public partial struct A { }
            }

            [PolyEnumFactoryFor(typeof(Choice))]
            {{wrapper}}
            """;

    private const string CHOICE_FACTORY_HINT = "ChoiceFactory.PolyEnumFactory.49747cb4cbc3c1c5.g.cs";

    private static Task VerifyWrapperAsync(
          string wrapper
        , string snapshotTestMethod
        , params string[] hintNames
    )
        => GeneratorTestHelper.VerifyGeneratedSourcesWithProducersAsync<PolyEnumFactoryGenerator>(
              $$"""
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial struct A { }
              }

              [PolyEnumStruct]
              public partial struct Result<T>
              {
                  public partial record struct Ok(T Value);
              }

              {{wrapper}}
              """
            , hintNames
                .Select(hintName => ExpectedGeneratedSource.Create<PolyEnumFactoryGenerator>(
                      hintName
                    , testMethod: snapshotTestMethod
                ))
                .ToArray()
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );

    [TestMethod]
    public Task GlobalNamespaceFactory_GeneratesFactoryAndPolyEnum()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumFactoryGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial struct A { }
              }

              [PolyEnumFactoryFor(typeof(Choice))]
              public partial class ChoiceFactory { }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumFactoryGenerator>(
                    "ChoiceFactory.PolyEnumFactory.4db87b0f813a8c5e.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Choice.PolyEnumStruct.85ed3cc2d96bac08.g.cs"
                ),
            }
            , new IIncrementalGenerator[] { new PolyEnumStructGenerator() }
        );
}
