using System.Text;

using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen
{
    public static class StringExtensions
    {
        private const string HEX = "0123456789ABCDEF";

        public static string EscapeStringLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return EscapeStringLiteral(value, new StringBuilder());
        }

        public static string EscapeStringLiteral(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : EscapeStringLiteral_Internal(value, sb);

        public static string ToValidIdentifier(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return ToValidIdentifier(value, new StringBuilder());
        }

        public static string ToValidIdentifier(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : ToValidIdentifier_Internal(value, sb);

        public static string NormalizeAuthoredIdentifier(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return NormalizeAuthoredIdentifier_Internal(value, new StringBuilder());
        }

        public static string ToValidIdentifierV2(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return ToValidIdentifierV2(value, new StringBuilder());
        }

        public static string ToValidIdentifierV2(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : ToValidIdentifierV2_Internal(value, sb);

        public static string EscapeCSharpIdentifier(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return SyntaxFacts.GetKeywordKind(value) != SyntaxKind.None
                || SyntaxFacts.GetContextualKeywordKind(value) != SyntaxKind.None
                ? $"@{value}"
                : value;
        }

        public static string ToFileName(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return ToFileName(value, new StringBuilder());
        }

        public static string ToFileName(this string value, StringBuilder sb)
            => string.IsNullOrEmpty(value) ? string.Empty : ToFileName_Internal(value, sb);

        public static int GetByteCount(this string value)
            => value == null ? 0 : Encoding.UTF8.GetByteCount(value);

        private static string EscapeStringLiteral_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");

            return sb.ToString();
        }

        private static string ToValidIdentifier_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append("I_");

            foreach (var c in value)
            {
                if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9')
                {
                    sb.Append(c);
                    continue;
                }

                if (c == '_')
                {
                    sb.Append("_u");
                    continue;
                }

                sb.Append("_x")
                    .Append(HEX[(c >> 12) & 0xF])
                    .Append(HEX[(c >> 8) & 0xF])
                    .Append(HEX[(c >> 4) & 0xF])
                    .Append(HEX[c & 0xF]);
            }

            return sb.ToString();
        }

        private static string ToValidIdentifierV2_Internal(string value, StringBuilder sb)
        {
            sb.Clear();

            var length = value.Length;

            for (var i = 0; i < length; i++)
            {
                var c = value[i];

                if (SyntaxFacts.IsIdentifierPartCharacter(c))
                {
                    sb.Append(c);
                    continue;
                }

                var symbol = GetIdentifierSymbol(c);

                if (symbol != "\0")
                {
                    sb.Append(symbol);
                    continue;
                }

                if (char.IsHighSurrogate(c) && i + 1 < length && char.IsLowSurrogate(value[i + 1]))
                {
                    var lowSurrogate = value[i + 1];

                    if (SyntaxFacts.IsValidIdentifier("I_" + c + lowSurrogate))
                    {
                        sb.Append(c).Append(lowSurrogate);
                        i++;
                        continue;
                    }

                    AppendScalarFallback(sb, char.ConvertToUtf32(c, value[++i]));
                    continue;
                }

                if (char.IsSurrogate(c))
                {
                    AppendSurrogateFallback(sb, c);
                    continue;
                }

                AppendScalarFallback(sb, c);
            }

            var result = sb.ToString();

            if (RequiresIdentifierPrefix(result))
            {
                sb.Insert(0, "I_");
                return sb.ToString();
            }

            return result;
        }

        private static string GetIdentifierSymbol(char c)
            => c switch {
                '.' => "___",
                '-' => "__ds__",
                ':' => "__sc__",
                '+' => "__p__",
                '`' => "__bt__",
                '?' => "__q__",
                '*' => "__a__",
                '<' => "__lt__",
                '>' => "__gt__",
                _ => "\0",
            };

        private static bool RequiresIdentifierPrefix(string value)
            => SyntaxFacts.IsValidIdentifier(value) == false || SyntaxFacts.GetKeywordKind(value) != SyntaxKind.None;

        private static void AppendScalarFallback(StringBuilder sb, int value)
        {
            sb.Append("__u0");
            AppendHex(sb, value, 20);
            sb.Append("__");
        }

        private static void AppendSurrogateFallback(StringBuilder sb, char value)
        {
            sb.Append("__u1");
            AppendHex(sb, value, 12);
            sb.Append("__");
        }

        private static void AppendHex(StringBuilder sb, int value, int firstShift)
        {
            for (var shift = firstShift; shift >= 0; shift -= 4)
            {
                sb.Append(HEX[(value >> shift) & 0xF]);
            }
        }

        private static string NormalizeAuthoredIdentifier_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace("global::", "")
                .Replace(' ', '_')
                .Replace(':', '_')
                .Replace('.', '_')
                .Replace("-", "__")
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                .Replace("[]", "Array")
                ;

            return sb.ToString();
        }

        private static string ToFileName_Internal(string value, StringBuilder sb)
        {
            sb.Clear().Append(value)
                .Replace("global::", "")
                .Replace('\\', '-')
                .Replace('/', '-')
                .Replace(':', '-')
                .Replace('*', '-')
                .Replace('?', '-')
                .Replace('|', '-')
                .Replace('\"', '\'')
                .Replace('<', 'ᐸ')
                .Replace('>', 'ᐳ')
                ;

            return sb.ToString();
        }
    }
}
