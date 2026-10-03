using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        public static bool TryGetTypeOfOperandLocation(
              this AttributeData attribute
            , CancellationToken token
            , out Location location
        )
        {
            token.ThrowIfCancellationRequested();

            if (attribute?.ApplicationSyntaxReference?.GetSyntax(token) is not AttributeSyntax syntax
                || syntax.ArgumentList == null
            )
            {
                location = null;
                return false;
            }

            var arguments = syntax.ArgumentList.Arguments;
            var count = arguments.Count;

            for (var i = 0; i < count; i++)
            {
                var argument = arguments[i];

                if (argument.NameEquals == null && argument.Expression is TypeOfExpressionSyntax typeOf)
                {
                    location = typeOf.Type.GetLocation();
                    return true;
                }
            }

            location = null;
            return false;
        }
    }
}
