using System.Globalization;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EncosyTower.SourceGen.Tests.Diagnostics;

internal static class DiagnosticContractTestHelper
{
    internal static void VerifyDiagnostics(IDiagnosticContractProvider provider)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var analyzerTypes = provider.ComponentTypes
                .Where(static type => typeof(DiagnosticSuppressor).IsAssignableFrom(type) == false)
                .OrderBy(static type => type.FullName, StringComparer.Ordinal)
                .ToArray();

            var actual = analyzerTypes
                .SelectMany(static type => {
                    var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(type, nonPublic: true)!;
                    return analyzer.SupportedDiagnostics.Select(descriptor => (type, descriptor));
                })
                .OrderBy(static item => item.type.FullName, StringComparer.Ordinal)
                .ThenBy(static item => item.descriptor.Id, StringComparer.Ordinal)
                .ToArray();

            var expected = provider.Diagnostics
                .OrderBy(static item => item.OwnerType, StringComparer.Ordinal)
                .ThenBy(static item => item.Id, StringComparer.Ordinal)
                .ToArray();

            Assert.AreEqual(expected.Length, actual.Length);
            AssertUniqueIds(expected.Select(static item => item.Id));
            CollectionAssert.AreEqual(
                expected.Select(static item => (item.OwnerType, item.Id)).ToArray(),
                actual.Select(static item => (item.type.FullName!, item.descriptor.Id)).ToArray()
            );

            for (var i = 0; i < expected.Length; i++)
            {
                AssertDiagnostic(expected[i], actual[i].type, actual[i].descriptor);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    internal static void VerifySuppressions(IDiagnosticContractProvider provider)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            var suppressorTypes = provider.ComponentTypes
                .Where(typeof(DiagnosticSuppressor).IsAssignableFrom)
                .OrderBy(static type => type.FullName, StringComparer.Ordinal)
                .ToArray();

            var actual = suppressorTypes
                .SelectMany(static type => {
                    var suppressor = (DiagnosticSuppressor)Activator.CreateInstance(type, nonPublic: true)!;
                    return suppressor.SupportedSuppressions
                        .Select(descriptor => (type, descriptor));
                })
                .OrderBy(static item => item.type.FullName, StringComparer.Ordinal)
                .ThenBy(static item => item.descriptor.Id, StringComparer.Ordinal)
                .ToArray();

            var expected = provider.Suppressions
                .OrderBy(static item => item.SuppressorType, StringComparer.Ordinal)
                .ThenBy(static item => item.SuppressionId, StringComparer.Ordinal)
                .ToArray();

            Assert.AreEqual(expected.Length, actual.Length);
            AssertUniqueIds(expected.Select(static item => item.SuppressionId));
            CollectionAssert.AreEqual(
                expected.Select(static item => (item.SuppressorType, item.SuppressionId)).ToArray(),
                actual.Select(static item => (item.type.FullName!, item.descriptor.Id)).ToArray()
            );

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].SuppressorType, actual[i].type.FullName);
                Assert.AreEqual(expected[i].SuppressionId, actual[i].descriptor.Id);
                Assert.AreEqual(expected[i].SuppressedDiagnosticId, actual[i].descriptor.SuppressedDiagnosticId);
                Assert.AreEqual(expected[i].Justification, actual[i].descriptor.Justification);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static void AssertDiagnostic(
          DiagnosticDescriptorContract expected
        , Type actualOwner
        , DiagnosticDescriptor actual
    )
    {
        Assert.AreEqual(expected.OwnerType, actualOwner.FullName);
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Title, actual.Title.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(expected.MessageFormat, actual.MessageFormat.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(expected.Category, actual.Category);
        Assert.AreEqual(expected.DefaultSeverity, actual.DefaultSeverity);
        Assert.AreEqual(expected.IsEnabledByDefault, actual.IsEnabledByDefault);
        Assert.AreEqual(expected.Description, actual.Description.ToString(CultureInfo.InvariantCulture));
        Assert.AreEqual(expected.HelpLinkUri, actual.HelpLinkUri);
        CollectionAssert.AreEqual(expected.CustomTags, actual.CustomTags.ToArray());
    }

    private static void AssertUniqueIds(IEnumerable<string> ids)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var id in ids)
        {
            Assert.IsTrue(seen.Add(id), $"Duplicate descriptor ID: {id}");
        }
    }
}
