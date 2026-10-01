using System.Text;
using EncosyTower.Core.Generators.Types.Caches;
using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Core.Types.Caches;

[TestClass]
public sealed class BroadProviderEfficiencyTests
{
    [TestMethod]
    public Task RuntimeTypeCaches_TransformsExactlyTwoCandidates()
        => GeneratorTestHelper.VerifyCandidateCountsAsync<RuntimeTypeCachesGenerator>(
              new[] { new NamedSource("RuntimeTypeCaches.cs", BuildSource()) }
            , new Dictionary<string, int>(StringComparer.Ordinal) {
                  ["RuntimeTypeCachesGenerator.Candidates"] = 2
                , ["RuntimeTypeCachesGenerator.ValidSpecs"] = 2
              }
        );

    private static string BuildSource()
    {
        var builder = new StringBuilder();
        builder.AppendLine("using EncosyTower.Types;");
        builder.AppendLine("namespace StructuralFixture;");
        BroadProviderTestSourceBuilder.AppendIrrelevantDeclarations(builder);
        builder.AppendLine(
            """
            public class CacheBase { }

            public partial class CacheUsage
            {
                public void Execute()
                {
                    RuntimeTypeCache.GetTypesDerivedFrom<CacheBase>();
                    RuntimeTypeCache.GetInfo<CacheUsage>();
                }
            }
            """
        );
        return builder.ToString();
    }
}
