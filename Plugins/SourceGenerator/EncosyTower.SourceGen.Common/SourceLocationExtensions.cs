using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen
{
    public static class SourceLocationExtensions
    {
        public static bool TryGetSourceLocation(this ISymbol symbol, out Location location)
        {
            if (symbol != null)
            {
                foreach (var candidate in symbol.Locations)
                {
                    if (candidate.IsInSource)
                    {
                        location = candidate;
                        return true;
                    }
                }
            }

            location = null;
            return false;
        }

        public static bool TryGetSourceLocation(
              this AttributeData attribute
            , CancellationToken token
            , out Location location
        )
        {
            token.ThrowIfCancellationRequested();

            if (attribute?.ApplicationSyntaxReference?.GetSyntax(token) is { } syntax)
            {
                location = syntax.GetLocation();
                return true;
            }

            location = null;
            return false;
        }
    }
}
