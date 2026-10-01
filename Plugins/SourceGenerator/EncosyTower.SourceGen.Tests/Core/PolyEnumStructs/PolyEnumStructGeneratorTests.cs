using System.Text;
using EncosyTower.Core.Generators.PolyEnumStructs;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests.Core.PolyEnumStructs;

[TestClass]
public class PolyEnumStructGeneratorTests
{
    private static readonly string[] s_observableChangeHintNames = {
        "ObservableDictionaryChange_2.PolyEnumStruct.4681a1ed9d33bc10.g.cs",
        "ObservableDictionaryChange_2.PolyEnumStructContainer.134039bb94c7bd76.g.cs",
        "ObservableListChange_1.PolyEnumStruct.d99d495542625af5.g.cs",
        "ObservableListChange_1.PolyEnumStructContainer.df6f5d5be14e8f03.g.cs",
    };

    [TestMethod]
    public Task EmptyInput_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumStructGenerator>();

    [TestMethod]
    public Task ChoiceCases_GeneratePolyEnum()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumStructGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial struct A { }

                  public partial record struct B(int Value);

                  public partial struct C
                  {
                      public int Value;
                  }

                  [EnumCaseIgnore]
                  public partial struct Ignored { }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Choice.PolyEnumStruct.a18c245f1860e08b.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task MergedFields_UseStoredGeneratedIdentifierWithoutReencoding()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumStructGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Choice
              {
                  public partial record struct ValueCase(int Value);
              }
              """
            , expectedSourceCount: 1
            , expectedFragments: new[] {
                "field_I_System_x002EInt32_1",
            }
            , unexpectedFragments: new[] {
                "field_I_I_uSystem_x002EInt32_1",
            }
        );

    [TestMethod]
    public Task GenericError_GeneratesConstructedPolyEnum()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumStructGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct]
              public partial struct Error<T>
                  where T : unmanaged
              {
                  public partial record struct Invalid(T Data);
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Error_1.PolyEnumStruct.08b7e4163a5b52ff.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Error_1.PolyEnumStructContainer.b12038ff4a8f19f7.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task GenericError_WithEnumExtensions_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumStructGenerator>(
            """
            using EncosyTower.PolyEnumStructs;

            namespace TestProject;

            [PolyEnumStruct(WithEnumExtensions = true)]
            public partial struct Error<T>
            {
                public partial struct Invalid { }
            }
            """
        );

    [TestMethod]
    public Task GenericResult_WithContainerAndEnumExtensions_GeneratesExtensions()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumStructGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              [PolyEnumStruct(
                    Container = typeof(ResultCases)
                  , WithEnumExtensions = true
              )]
              public partial struct Result<T>
              {
              }

              public static partial class ResultCases
              {
                  public readonly partial record struct Success<T>(T Value);
              }

              public static class Consumer
              {
                  public static string GetName()
                      => ResultCases.EnumCase.Success.ToStringFast();
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Result_1.PolyEnumStruct.b6d6bbbd45bb0aed.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "Result_1.PolyEnumStructContainer.2e31452a6b649d78.g.cs"
                ),
            }
        );

    [TestMethod]
    public Task LocallyNonGenericNestedTarget_WithEnumExtensions_GeneratesInMirroredContainer()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumStructGenerator>(
              """
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              public partial class Outer<T>
              {
                  [PolyEnumStruct(WithEnumExtensions = true)]
                  public partial struct Inner
                  {
                      public partial struct Case { }
                  }
              }
              """
            , expectedSourceCount: 2
            , expectedFragments: new[] {
                "partial class Outer",
                "partial class Inner",
                "public enum EnumCase : byte",
                "partial struct Inner_EnumCaseExtended",
                "static partial class Inner_EnumCaseExtensions",
                "public static string ToStringFast(Inner.EnumCase value)",
            }
            , unexpectedFragments: new[] {
                "public static string ToStringFast(this Inner.EnumCase value)",
            }
        );

    [TestMethod]
    public Task DependencyResult_UsesMinimalContainerCaseArity()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumStructGenerator>(
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

              internal static class Consumer
              {
                  internal static void Use()
                  {
                      DependencyResult<int, string> none = new DependencyResultCases.None();
                      var success = new DependencyResultCases.Success<int>(42)
                          .ToDependencyResult<string>();
                  }
              }
              }
              """
            , expectedSourceCount: 2
            , expectedFragments: new[] {
                "public enum EnumCase : byte",
                "public partial interface IEnumCase",
                "public partial struct DependencyResult_Undefined : IEnumCase",
                "partial record struct None : IEnumCase",
                "partial record struct Success<TValue> : IEnumCase",
                "partial record struct Failure<TError> : IEnumCase",
                "partial record struct Pair<TValue, TError> : IEnumCase",
                "public readonly global::TestProject.DependencyResult<TValue, TError> ToDependencyResult<TError>()",
                "internal static partial class EnumCaseAPI",
                "partial struct DependencyResult<TValue, TError> : global::TestProject.DependencyResultCases.IEnumCase",
            }
            , unexpectedFragments: new[] {
                "interface IEnumCase<",
                "class EnumCaseAPI<",
                "record struct None<",
            }
        );

    [TestMethod]
    public Task SplitInterface_RequiresOnlyCommonValueDependency()
        => GeneratorTestHelper.VerifyGeneratedSourceSetFragmentsAsync<PolyEnumStructGenerator>(
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
              }

              namespace TestProject
              {
                  [PolyEnumStruct(Container = typeof(ResultCases))]
                  internal partial struct Result<TValue, TError>
                  {
                  }

                  internal static partial class ResultCases
                  {
                      internal partial interface IEnumCase
                      {
                          int Code => 0;
                      }

                      internal partial interface IEnumCase<TValue> : IEnumCase
                      {
                          TValue Value => default;
                      }

                      internal readonly partial record struct Empty<TValue>
                      {
                          public int Code => 0;
                          public TValue Value => default;
                      }

                      internal readonly partial record struct Success<TValue>(TValue Value)
                      {
                          public int Code => 1;
                      }
                  }
              }
              """
            , expectedSourceCount: 2
            , expectedFragments: new[] {
                "partial interface IEnumCase<TValue> : IEnumCase",
                "partial struct Result_Undefined<TValue> : IEnumCase, IEnumCase<TValue>",
                "partial record struct Empty<TValue> : IEnumCase, IEnumCase<TValue>",
                "partial record struct Success<TValue> : IEnumCase, IEnumCase<TValue>",
                "partial struct Result<TValue, TError> : global::TestProject.ResultCases.IEnumCase, " +
                    "global::TestProject.ResultCases.IEnumCase<TValue>",
                "Property_Get_Value<TValue, TEnumCase>",
                "where TEnumCase : struct, IEnumCase<TValue>",
            }
            , unexpectedFragments: new[] {
                "IEnumCase<TValue, TError>",
                "Empty<TValue, TError>",
                "Success<TValue, TError>",
                "class EnumCaseAPI<",
            }
        );

    [TestMethod]
    public Task ObservableCollectionChangeContainers_GeneratePublicTypedCases()
        => GeneratorTestHelper.VerifyGeneratedSourcesAsync<PolyEnumStructGenerator>(
              """
              using System;
              using System.Collections.Generic;
              using EncosyTower.PolyEnumStructs;

              namespace TestProject;

              public interface IObservableDictionary<TKey, TValue>
              {
                  IEqualityComparer<TKey> Comparer { get; }
              }

              public static partial class ObservableListChange
              {
                  public partial interface IEnumCase
                  {
                      void GetAffectedRange(out int start, out int end, out bool following, out bool structural);
                  }

                  public readonly partial record struct Add<T>(int Index, T Value)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Index; end = Index + 1; following = structural = true;
                      }
                  }

                  public readonly partial record struct AddRange(int Index, int Count)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Index; end = Index + Count; following = structural = true;
                      }
                  }

                  public readonly partial record struct Remove<T>(int Index, T Value)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Index; end = Index + 1; following = structural = true;
                      }
                  }

                  public readonly partial record struct RemoveRange(int Index, int Count)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Index; end = Index + Count; following = structural = true;
                      }
                  }

                  public readonly partial record struct Replace<T>(int Index, T OldValue, T NewValue)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Index; end = Index + 1; following = structural = false;
                      }
                  }

                  public readonly partial record struct ReplaceRange(int Index, int Count)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Index; end = Index + Count; following = structural = false;
                      }
                  }

                  public readonly partial record struct Move<T>(int PreviousIndex, int Index, T Value)
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = Math.Min(PreviousIndex, Index);
                          end = Math.Max(PreviousIndex, Index) + 1;
                          following = false; structural = true;
                      }
                  }

                  public readonly partial record struct Clear
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = end = 0; following = structural = true;
                      }
                  }

                  public readonly partial record struct Reset
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = end = 0; following = structural = true;
                      }
                  }

                  public readonly partial struct ObservableListChange_Undefined
                  {
                      public void GetAffectedRange(out int start, out int end, out bool following, out bool structural)
                      {
                          start = end = 0; following = structural = true;
                      }
                  }
              }

              [PolyEnumStruct(Container = typeof(ObservableListChange))]
              public readonly partial struct ObservableListChange<T>
              {
              }

              public static partial class ObservableDictionaryChange
              {
                  public partial interface IEnumCase { }

                  public partial interface IEnumCase<TKey, TValue> : IEnumCase
                  {
                      bool AffectsKey(in TKey selectedKey, IObservableDictionary<TKey, TValue> source);
                  }

                  public readonly partial record struct Add<TKey, TValue>(TKey Key, TValue Value)
                  {
                      public bool AffectsKey(in TKey selectedKey, IObservableDictionary<TKey, TValue> source)
                          => source.Comparer.Equals(selectedKey, Key);
                  }

                  public readonly partial record struct Remove<TKey, TValue>(TKey Key, TValue Value)
                  {
                      public bool AffectsKey(in TKey selectedKey, IObservableDictionary<TKey, TValue> source)
                          => source.Comparer.Equals(selectedKey, Key);
                  }

                  public readonly partial record struct Replace<TKey, TValue>(
                      TKey Key,
                      TValue OldValue,
                      TValue NewValue
                  )
                  {
                      public bool AffectsKey(in TKey selectedKey, IObservableDictionary<TKey, TValue> source)
                          => source.Comparer.Equals(selectedKey, Key);
                  }

                  public readonly partial record struct Clear<TKey, TValue>
                  {
                      public bool AffectsKey(in TKey selectedKey, IObservableDictionary<TKey, TValue> source)
                          => true;
                  }

                  public readonly partial struct ObservableDictionaryChange_Undefined<TKey, TValue>
                  {
                      public bool AffectsKey(in TKey selectedKey, IObservableDictionary<TKey, TValue> source)
                          => true;
                  }
              }

              [PolyEnumStruct(Container = typeof(ObservableDictionaryChange))]
              public readonly partial struct ObservableDictionaryChange<TKey, TValue>
              {
              }

              public static class Consumer
              {
                  public static bool UseList<T>(ObservableListChange<T> change)
                  {
                      change.GetAffectedRange(out _, out _, out _, out _);
                      ObservableListChange<T> add = new ObservableListChange.Add<T>(0, default);
                      ObservableListChange<T> addRange = new ObservableListChange.AddRange(0, 1);
                      ObservableListChange<T> remove = new ObservableListChange.Remove<T>(0, default);
                      ObservableListChange<T> removeRange = new ObservableListChange.RemoveRange(0, 1);
                      ObservableListChange<T> replace = new ObservableListChange.Replace<T>(0, default, default);
                      ObservableListChange<T> replaceRange = new ObservableListChange.ReplaceRange(0, 1);
                      ObservableListChange<T> move = new ObservableListChange.Move<T>(0, 1, default);
                      ObservableListChange<T> clear = new ObservableListChange.Clear();
                      ObservableListChange<T> reset = new ObservableListChange.Reset();
                      ObservableListChange<T> undefined = new ObservableListChange.ObservableListChange_Undefined();
                      ObservableListChange.IEnumCase enumCase = add;

                      return change.TryGetValue(out ObservableListChange.Add<T> _)
                          || change.TryGetValue(out ObservableListChange.AddRange _)
                          || change.TryGetValue(out ObservableListChange.Remove<T> _)
                          || change.TryGetValue(out ObservableListChange.RemoveRange _)
                          || change.TryGetValue(out ObservableListChange.Replace<T> _)
                          || change.TryGetValue(out ObservableListChange.ReplaceRange _)
                          || change.TryGetValue(out ObservableListChange.Move<T> _)
                          || change.TryGetValue(out ObservableListChange.Clear _)
                          || change.TryGetValue(out ObservableListChange.Reset _)
                          || change.TryGetValue(out ObservableListChange.ObservableListChange_Undefined _);
                  }

                  public static bool UseDictionary<TKey, TValue>(
                      ObservableDictionaryChange<TKey, TValue> change,
                      IObservableDictionary<TKey, TValue> source
                  )
                  {
                      ObservableDictionaryChange<TKey, TValue> add =
                          new ObservableDictionaryChange.Add<TKey, TValue>(default, default);
                      ObservableDictionaryChange<TKey, TValue> remove =
                          new ObservableDictionaryChange.Remove<TKey, TValue>(default, default);
                      ObservableDictionaryChange<TKey, TValue> replace =
                          new ObservableDictionaryChange.Replace<TKey, TValue>(default, default, default);
                      ObservableDictionaryChange<TKey, TValue> clear =
                          new ObservableDictionaryChange.Clear<TKey, TValue>();
                      ObservableDictionaryChange<TKey, TValue> undefined =
                          new ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue>();
                      ObservableDictionaryChange.IEnumCase enumCase = add;

                      var selectedKey = default(TKey);
                      var affected = change.AffectsKey(in selectedKey, source);

                      return change.TryGetValue(out ObservableDictionaryChange.Add<TKey, TValue> _)
                          || change.TryGetValue(out ObservableDictionaryChange.Remove<TKey, TValue> _)
                          || change.TryGetValue(out ObservableDictionaryChange.Replace<TKey, TValue> _)
                          || change.TryGetValue(out ObservableDictionaryChange.Clear<TKey, TValue> _)
                          || change.TryGetValue(out
                              ObservableDictionaryChange.ObservableDictionaryChange_Undefined<TKey, TValue> _);
                  }
              }
              """
            , new[] {
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "ObservableListChange_1.PolyEnumStruct.d99d495542625af5.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "ObservableListChange_1.PolyEnumStructContainer.df6f5d5be14e8f03.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "ObservableDictionaryChange_2.PolyEnumStruct.4681a1ed9d33bc10.g.cs"
                ),
                ExpectedGeneratedSource.Create<PolyEnumStructGenerator>(
                    "ObservableDictionaryChange_2.PolyEnumStructContainer.134039bb94c7bd76.g.cs"
                ),
            }
        );

    [TestMethod]
    public async Task ObservableCollectionChangeContainers_CaseAndCandidateTransitionsUpdateExactOutputs()
    {
        const string INITIAL_SOURCE = """
            using EncosyTower.PolyEnumStructs;

            namespace TestProject;

            public static partial class ObservableListChange
            {
                public readonly partial record struct Add<T>(int Index, T Value);
            }

            [PolyEnumStruct(Container = typeof(ObservableListChange))]
            public readonly partial struct ObservableListChange<T>
            {
            }

            public static partial class ObservableDictionaryChange
            {
                public readonly partial record struct Add<TKey, TValue>(TKey Key, TValue Value);
            }

            [PolyEnumStruct(Container = typeof(ObservableDictionaryChange))]
            public readonly partial struct ObservableDictionaryChange<TKey, TValue>
            {
            }
            """;
        const string ADDED_CASE_SOURCE = """
            using EncosyTower.PolyEnumStructs;

            namespace TestProject;

            public static partial class ObservableListChange
            {
                public readonly partial record struct Add<T>(int Index, T Value);

                public readonly partial record struct Remove<T>(int Index, T Value);
            }

            [PolyEnumStruct(Container = typeof(ObservableListChange))]
            public readonly partial struct ObservableListChange<T>
            {
            }

            public static partial class ObservableDictionaryChange
            {
                public readonly partial record struct Add<TKey, TValue>(TKey Key, TValue Value);

                public readonly partial record struct Remove<TKey, TValue>(TKey Key, TValue Value);
            }

            [PolyEnumStruct(Container = typeof(ObservableDictionaryChange))]
            public readonly partial struct ObservableDictionaryChange<TKey, TValue>
            {
            }
            """;
        const string REMOVED_CASE_SOURCE = """
            using EncosyTower.PolyEnumStructs;

            namespace TestProject;

            public static partial class ObservableListChange
            {
                public readonly partial record struct Remove<T>(int Index, T Value);
            }

            [PolyEnumStruct(Container = typeof(ObservableListChange))]
            public readonly partial struct ObservableListChange<T>
            {
            }

            public static partial class ObservableDictionaryChange
            {
                public readonly partial record struct Remove<TKey, TValue>(TKey Key, TValue Value);
            }

            [PolyEnumStruct(Container = typeof(ObservableDictionaryChange))]
            public readonly partial struct ObservableDictionaryChange<TKey, TValue>
            {
            }
            """;
        const string CANDIDATES_REMOVED_SOURCE = "internal sealed class MarkerOnly { }";

        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync();
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
              new[] { new PolyEnumStructGenerator().AsSourceGenerator() }
            , parseOptions: parseOptions
            , driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true)
        );
        var initialSources = RunObservableChangeTransition(
              ref driver
            , INITIAL_SOURCE
            , references
            , parseOptions
            , s_observableChangeHintNames
            , out _
        );
        var addedCaseSources = RunObservableChangeTransition(
              ref driver
            , ADDED_CASE_SOURCE
            , references
            , parseOptions
            , s_observableChangeHintNames
            , out var addedCaseResult
        );

        AssertEveryGeneratedSourceChanged(initialSources, addedCaseSources);
        AssertOutputReasons(addedCaseResult, IncrementalStepRunReason.Modified);

        var removedCaseSources = RunObservableChangeTransition(
              ref driver
            , REMOVED_CASE_SOURCE
            , references
            , parseOptions
            , s_observableChangeHintNames
            , out var removedCaseResult
        );

        AssertEveryGeneratedSourceChanged(addedCaseSources, removedCaseSources);
        AssertOutputReasons(removedCaseResult, IncrementalStepRunReason.Modified);

        RunObservableChangeTransition(
              ref driver
            , CANDIDATES_REMOVED_SOURCE
            , references
            , parseOptions
            , Array.Empty<string>()
            , out var candidatesRemovedResult
        );

        AssertOutputReasons(candidatesRemovedResult, IncrementalStepRunReason.Removed);
    }

    [TestMethod]
    public Task InvalidContainer_ProducesNoOutput()
        => GeneratorTestHelper.VerifyNoOutputAsync<PolyEnumStructGenerator>(
            """
            #pragma warning disable CS0436

            namespace EncosyTower.PolyEnumStructs
            {
                [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = false)]
                public sealed class PolyEnumStructAttribute : System.Attribute
                {
                    public System.Type Container { get; set; }
                }
            }

            namespace TestProject
            {
                public enum Cases { }

                [EncosyTower.PolyEnumStructs.PolyEnumStruct(Container = typeof(Cases))]
                public partial struct Result<T> { }
            }
            """
        );

    private static IReadOnlyDictionary<string, string> RunObservableChangeTransition(
          ref GeneratorDriver driver
        , string source
        , IEnumerable<MetadataReference> references
        , CSharpParseOptions parseOptions
        , IReadOnlyList<string> expectedHintNames
        , out GeneratorRunResult generatorResult
    )
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
              SourceText.From(source, Encoding.UTF8)
            , parseOptions
            , "ObservableCollectionChanges.cs"
        );
        var compilation = CSharpCompilation.Create(
              "EncosyTower.SourceGen.Tests.Input"
            , new[] { syntaxTree }
            , references
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );
        driver = driver.RunGeneratorsAndUpdateCompilation(
              compilation
            , out var outputCompilation
            , out var diagnostics
        );
        var runResult = driver.GetRunResult();

        Assert.AreEqual(0, diagnostics.Length);
        Assert.AreEqual(0, runResult.Diagnostics.Length);
        Assert.AreEqual(1, runResult.Results.Length);
        generatorResult = runResult.Results[0];
        Assert.IsNull(generatorResult.Exception);
        Assert.AreEqual(0, generatorResult.Diagnostics.Length);
        var compilationDiagnostics = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, compilationDiagnostics.Length, string.Join("\n", compilationDiagnostics.AsEnumerable()));
        var actualHintNames = generatorResult.GeneratedSources
            .Select(static generatedSource => generatedSource.HintName)
            .OrderBy(static hintName => hintName, StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEqual(expectedHintNames.ToArray(), actualHintNames);
        return generatorResult.GeneratedSources.ToDictionary(
              static generatedSource => generatedSource.HintName
            , static generatedSource => generatedSource.SourceText.ToString()
            , StringComparer.Ordinal
        );
    }

    private static void AssertEveryGeneratedSourceChanged(
          IReadOnlyDictionary<string, string> previous
        , IReadOnlyDictionary<string, string> current
    )
    {
        CollectionAssert.AreEquivalent(previous.Keys.ToArray(), current.Keys.ToArray());

        foreach (var pair in previous)
        {
            Assert.AreNotEqual(pair.Value, current[pair.Key], $"Generated source did not change: {pair.Key}.");
        }
    }

    private static void AssertOutputReasons(GeneratorRunResult result, IncrementalStepRunReason expectedReason)
    {
        const string TRACKING_NAME = "PolyEnumStructGenerator.Outputs";

        Assert.IsTrue(
            result.TrackedOutputSteps.ContainsKey(TRACKING_NAME)
                || result.TrackedSteps.ContainsKey(TRACKING_NAME)
        );
        var steps = result.TrackedOutputSteps.TryGetValue(TRACKING_NAME, out var outputSteps)
            ? outputSteps
            : result.TrackedSteps[TRACKING_NAME];
        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();
        Assert.IsTrue(outputs.Length > 0);

        foreach (var output in outputs)
        {
            Assert.AreEqual(expectedReason, output.Reason);
        }
    }
}
