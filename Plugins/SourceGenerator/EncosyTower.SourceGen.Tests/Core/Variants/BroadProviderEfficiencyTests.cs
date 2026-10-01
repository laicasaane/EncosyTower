using System.Text;
using EncosyTower.SourceGen.Tests.Helpers;
using CoreInternalVariantGenerator = EncosyTower.Core.Generators.Variants.InternalVariantGenerator;

namespace EncosyTower.SourceGen.Tests.Core.Variants;

[TestClass]
public sealed class BroadProviderEfficiencyTests
{
    [TestMethod]
    public Task InternalVariants_TransformsExactlyTwoCandidates()
        => GeneratorTestHelper.VerifyCandidateCountsAsync<CoreInternalVariantGenerator>(
              new[] { new NamedSource("InternalVariants.cs", BuildSource()) }
            , new Dictionary<string, int>(StringComparer.Ordinal) {
                  ["InternalVariantGenerator.Candidates"] = 2
                , ["InternalVariantGenerator.ValidSpecs"] = 2
              }
        );

    private static string BuildSource()
    {
        var builder = new StringBuilder();
        builder.AppendLine("using EncosyTower.Variants;");
        builder.AppendLine("namespace StructuralFixture;");
        BroadProviderTestSourceBuilder.AppendIrrelevantDeclarations(builder);
        builder.AppendLine(
            """
            public sealed class VariantUsage
            {
                public void Execute()
                {
                    _ = Variant<UnityEngine.Vector2>.GetConverter();
                    _ = Variant<UnityEngine.Vector3>.GetConverter();
                }
            }
            """
        );
        return builder.ToString();
    }
}
