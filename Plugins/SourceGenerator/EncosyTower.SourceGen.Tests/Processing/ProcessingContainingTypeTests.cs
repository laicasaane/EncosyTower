namespace EncosyTower.SourceGen.Tests.Processing;

[TestClass]
public sealed class ProcessingContainingTypeTests
{
    public static IEnumerable<object[]> EditCases
    {
        get
        {
            yield return new object[] {
                  "containing type kind"
                , """
                  using EncosyTower.Processing;

                  namespace TestProject;

                  {{EDIT}}
                  {
                      [Processing(ApiMode.Both, State = StateMode.Both)]
                      public partial class Request { }
                  }
                  """
                , ReusedDriverEditCase.CLASS_OUTER
                , ReusedDriverEditCase.STRUCT_OUTER
            };

            yield return new object[] {
                  "namespace"
                , """
                  using EncosyTower.Processing;

                  {{EDIT}}
                  {
                      [Processing(ApiMode.Both, State = StateMode.Both)]
                      public partial class Request { }
                  }
                  """
                , ReusedDriverEditCase.TEST_NAMESPACE
                , ReusedDriverEditCase.OTHER_NAMESPACE
            };
        }
    }

    [TestMethod]
    [DynamicData(nameof(EditCases), DynamicDataSourceType.Property)]
    public async Task ReusedDriverEdit_RegeneratesOutput(string name, string source, string before, string after)
    {
        var first = await ProcessingRequestFixture.RunAsync(source.Replace(ReusedDriverEditCase.EDIT, before));
        var edited = new NamedSource("Request.cs", source.Replace(ReusedDriverEditCase.EDIT, after));
        var reused = await ProcessingRequestFixture.RunAsync([edited], first);
        var fresh = await ProcessingRequestFixture.RunAsync([edited]);

        ProcessingRequestFixture.AssertNoOutputErrors(reused);
        CollectionAssert.AreEqual(GetSources(fresh), GetSources(reused), name);
        AssertAllModified(reused, "ProcessingRequestGenerator.RequestOutputs");
        AssertAllModified(reused, "ProcessingRequestGenerator.ScopeOutputs");
    }

    private static string[] GetSources(ProcessingRun run)
        => run.Result.GeneratedSources
            .OrderBy(static source => source.HintName, StringComparer.Ordinal)
            .Select(static source => source.HintName + "\n" + source.SourceText)
            .ToArray();

    private static void AssertAllModified(ProcessingRun run, string trackingName)
    {
        Assert.IsTrue(run.Result.TrackedSteps.TryGetValue(trackingName, out var steps), trackingName);
        var outputs = steps.SelectMany(static step => step.Outputs).ToArray();
        Assert.IsTrue(outputs.Length > 0, trackingName);

        foreach (var output in outputs)
        {
            Assert.AreEqual(IncrementalStepRunReason.Modified, output.Reason, trackingName);
        }
    }
}
