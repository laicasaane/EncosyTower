using EncosyTower.SourceGen.Tests.Helpers;

namespace EncosyTower.SourceGen.Tests.Data.Databases;

[TestClass]
public sealed class ProductionGeneratorContractTests
{
    private static readonly ProductionGeneratorContractProvider s_provider = new();

    public static IEnumerable<object[]> ContractCases
        => ProductionGeneratorContractTestHelper.ToDynamicData(s_provider.Contracts);

    [TestMethod]
    [DynamicData(nameof(ContractCases), DynamicDataSourceType.Property)]
    public Task ContractMatrix_PassesEveryTransition(ProductionGeneratorContractCase contract)
        => ProductionGeneratorContractTestHelper.VerifyAsync(contract);
}
