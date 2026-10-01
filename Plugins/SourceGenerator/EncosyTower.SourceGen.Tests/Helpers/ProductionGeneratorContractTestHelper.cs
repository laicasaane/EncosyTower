using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Tests.Helpers;

internal static class ProductionGeneratorContractTestHelper
{
    internal static IEnumerable<object[]> ToDynamicData(IReadOnlyList<ProductionGeneratorContractCase> contracts)
        => contracts.Select(static contract => new object[] { contract });

    internal static Task VerifyAsync(ProductionGeneratorContractCase contract)
    {
        var fixturePath = Path.Combine(
              GetTestProjectDirectory()
            , contract.FixturePath.Replace('/', Path.DirectorySeparatorChar)
        );
        var validSource = LoadFixtureSource(fixturePath, contract.FixtureMethod);
        var relevantSource = ReplaceRelevantValue(validSource, contract);
        var expectedOutputs = LoadExpectedOutputs(fixturePath, contract);

        return global::EncosyTower.SourceGen.Tests.GeneratorTestHelper.VerifyProductionContractAsync(
              contract
            , validSource
            , relevantSource
            , expectedOutputs
        );
    }

    private static string LoadFixtureSource(string fixturePath, string fixtureMethod)
    {
        var root = CSharpSyntaxTree.ParseText(File.ReadAllText(fixturePath)).GetRoot();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Single(node => string.Equals(node.Identifier.ValueText, fixtureMethod, StringComparison.Ordinal));

        var invocation = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(node => node.Expression.DescendantNodesAndSelf()
                .OfType<GenericNameSyntax>()
                .Any(name => string.Equals(
                      name.Identifier.ValueText
                    , "VerifyGeneratedSourcesAsync"
                    , StringComparison.Ordinal
                ))
            );
        var expression = invocation.ArgumentList.Arguments[0].Expression;
        return ResolveStringExpression(root, expression);
    }

    private static string ResolveStringExpression(SyntaxNode root, ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal)
        {
            return literal.Token.ValueText;
        }

        if (expression is IdentifierNameSyntax identifier)
        {
            var value = root.DescendantNodes()
                .OfType<VariableDeclaratorSyntax>()
                .Single(node => string.Equals(
                      node.Identifier.ValueText
                    , identifier.Identifier.ValueText
                    , StringComparison.Ordinal
                ))
                .Initializer?.Value
                ?? throw new InvalidOperationException(
                    $"Fixture constant '{identifier.Identifier.ValueText}' has no value."
                );
            return ResolveStringExpression(root, value);
        }

        throw new InvalidOperationException($"Unsupported fixture source expression: {expression.Kind()}.");
    }

    private static string ReplaceRelevantValue(string source, ProductionGeneratorContractCase contract)
    {
        var index = source.IndexOf(contract.RelevantEditOldValue, StringComparison.Ordinal);

        Assert.IsTrue(index >= 0, $"Relevant-edit value was not found: {contract.RelevantEditOldValue}.");

        return source.Remove(index, contract.RelevantEditOldValue.Length).Insert(index, contract.RelevantEditNewValue);
    }

    private static ProductionGeneratorExpectedOutput[] LoadExpectedOutputs(
          string fixturePath
        , ProductionGeneratorContractCase contract
    )
    {
        var fixtureDirectory = Path.GetDirectoryName(fixturePath)
            ?? throw new InvalidOperationException($"Fixture directory is missing: {fixturePath}.");
        var snapshotDirectory = Path.Combine(fixtureDirectory, "Snapshots");
        var generatorTypes = new[] { contract.GeneratorType }.Concat(contract.AdditionalGeneratorTypes).ToArray();

        var typeByName = generatorTypes.ToDictionary(
              static type => type.Name
            , StringComparer.Ordinal
        );
        var prefix = $"{contract.FixtureMethod}#";
        var outputs = new List<ProductionGeneratorExpectedOutput>();

        foreach (var snapshotPath in Directory.EnumerateFiles(
                  snapshotDirectory
                , $"{contract.FixtureMethod}#*.verified.cs"
                , SearchOption.TopDirectoryOnly
            )
        )
        {
            var fileName = Path.GetFileName(snapshotPath);
            var generatorSeparator = fileName.IndexOf('#', prefix.Length);

            if (generatorSeparator < 0)
            {
                continue;
            }

            var generatorName = fileName.Substring(prefix.Length, generatorSeparator - prefix.Length);

            if (typeByName.TryGetValue(generatorName, out var generatorType) == false)
            {
                continue;
            }

            const string SUFFIX = ".verified.cs";
            var hintName = fileName.Substring(
                  generatorSeparator + 1
                , fileName.Length - generatorSeparator - 1 - SUFFIX.Length
            );

            var source = File.ReadAllText(snapshotPath);

            if (IsSourceLocationSensitive(contract)
                || source.Contains("VariantCapacityRegistration.Register<", StringComparison.Ordinal))
            {
                source = source.Replace("\"Test0.cs\"", "\"Valid.cs\"", StringComparison.Ordinal);
            }

            outputs.Add(new ProductionGeneratorExpectedOutput(generatorType, hintName, source));
        }

        Assert.IsTrue(outputs.Count > 0, $"No expected snapshots were found for {contract.GeneratorType.FullName}.");

        return outputs
            .OrderBy(static output => output.GeneratorType.FullName, StringComparer.Ordinal)
            .ThenBy(static output => output.HintName, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetTestProjectDirectory()
        => Path.Combine(FindSourceGeneratorRoot(), "EncosyTower.SourceGen.Tests");

    private static string FindSourceGeneratorRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            var solutionPath = Path.Combine(directory.FullName, "EncosyTower.SourceGen.slnx");

            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate EncosyTower.SourceGen.slnx from '{AppContext.BaseDirectory}'."
        );
    }

    internal static bool IsSourceLocationSensitive(ProductionGeneratorContractCase contract)
    {
        var name = contract.GeneratorType.FullName;
        return string.Equals(
              name
            , "EncosyTower.Mvvm.Generators.InternalVariants.InternalVariantGenerator"
            , StringComparison.Ordinal
        ) || string.Equals(
              name
            , "EncosyTower.Mvvm.Generators.InternalStringAdapters.InternalStringAdapterGenerator"
            , StringComparison.Ordinal
        );
    }
}
