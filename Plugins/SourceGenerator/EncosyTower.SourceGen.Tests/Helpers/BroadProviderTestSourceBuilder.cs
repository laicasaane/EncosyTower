using System.Text;

namespace EncosyTower.SourceGen.Tests.Helpers;

internal static class BroadProviderTestSourceBuilder
{
    private const int IRRELEVANT_DECLARATION_COUNT = 10_000;

    internal static void AppendIrrelevantDeclarations(StringBuilder builder)
    {
        for (var i = 0; i < IRRELEVANT_DECLARATION_COUNT; i++)
        {
            builder.Append("internal sealed class Irrelevant").Append(i).AppendLine(" { }");
        }
    }
}
