namespace EncosyTower.SourceGen.Tests.PubSub;

[TestClass]
public sealed class PubSubContainingTypeTests
{
    public static IEnumerable<object[]> EditCases
    {
        get
        {
            yield return new object[] {
                  "containing type kind"
                , """
                  using EncosyTower.PubSub;

                  namespace TestProject;

                  {{EDIT}}
                  {
                      [PubSub(ApiMode.Both, State = StateMode.Both)]
                      public partial class Message { }
                  }
                  """
                , ReusedDriverEditCase.CLASS_OUTER
                , ReusedDriverEditCase.STRUCT_OUTER
            };

            yield return new object[] {
                  "namespace"
                , """
                  using EncosyTower.PubSub;

                  {{EDIT}}
                  {
                      [PubSub(ApiMode.Both, State = StateMode.Both)]
                      public partial class Message { }
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
        var first = await PubSubRuntimeFixture.RunAsync(source.Replace(ReusedDriverEditCase.EDIT, before));
        var edited = new NamedSource("Message.cs", source.Replace(ReusedDriverEditCase.EDIT, after));
        var reused = await PubSubRuntimeFixture.RunAsync([edited], first);
        var fresh = await PubSubRuntimeFixture.RunAsync([edited]);

        PubSubRuntimeFixture.AssertNoOutputErrors(reused);
        CollectionAssert.AreEqual(GetSources(fresh), GetSources(reused), name);
        AssertAllModified(reused, "PubSubMessageGenerator.MessageOutputs");
        AssertAllModified(reused, "PubSubMessageGenerator.ScopeOutputs");
    }

    private static string[] GetSources(PubSubRun run)
        => run.Sources
            .OrderBy(static source => source.HintName, StringComparer.Ordinal)
            .Select(static source => source.HintName + "\n" + source.SourceText)
            .ToArray();

    private static void AssertAllModified(PubSubRun run, string trackingName)
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
