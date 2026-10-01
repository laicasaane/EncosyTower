using EncosyTower.SourceGen.Tests.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class DiagnosticContractIdTests
{
    [TestMethod]
    public void ProviderDiagnosticAndSuppressionIds_AreGloballyUnique()
    {
        var providers = GetProviders();

        AssertUnique(
            providers
                .SelectMany(static provider => provider.Diagnostics)
                .Select(static contract => contract.Id)
        );
        AssertUnique(
            providers
                .SelectMany(static provider => provider.Suppressions)
                .Select(static contract => contract.SuppressionId)
        );
    }

    private static IDiagnosticContractProvider[] GetProviders()
        => typeof(IDiagnosticContractProvider).Assembly
            .GetTypes()
            .Where(static type => type.IsAbstract == false)
            .Where(static type => typeof(IDiagnosticContractProvider).IsAssignableFrom(type))
            .Select(static type => (IDiagnosticContractProvider)Activator.CreateInstance(type, nonPublic: true)!)
            .OrderBy(static provider => provider.FeaturePath, StringComparer.Ordinal)
            .ToArray();

    private static void AssertUnique(IEnumerable<string> ids)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in ids)
        {
            Assert.IsTrue(seen.Add(id), $"Duplicate descriptor ID: {id}");
        }
    }
}
