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
}
