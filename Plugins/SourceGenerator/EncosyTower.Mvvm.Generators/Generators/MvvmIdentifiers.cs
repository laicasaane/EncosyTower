namespace EncosyTower.Mvvm.Generators
{
    internal static class MvvmIdentifiers
    {
        private static readonly SymbolDisplayFormat s_qualifiedFormat = new(
              typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
            , genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            , miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                | SymbolDisplayMiscellaneousOptions.ExpandNullable
        );

        private static readonly SymbolDisplayFormat s_simpleFormat = new(
              typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly
            , globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted
            , genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            , miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
                | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        );

        public static string FromType(ITypeSymbol symbol)
            => FromName(symbol.ToDisplayString(s_qualifiedFormat));

        public static string FromSimpleType(ITypeSymbol symbol)
            => FromName(symbol.ToDisplayString(s_simpleFormat));

        public static string FromName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return new StringBuilder(value)
                .Replace("global::", "")
                .Replace(' ', '_')
                .Replace(':', '_')
                .Replace('.', '_')
                .Replace("-", "__")
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                .Replace("[]", "Array")
                .ToString();
        }
    }
}
