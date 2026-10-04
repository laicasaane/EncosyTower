using System;
using System.Collections.Generic;

namespace EncosyTower.PageFlows
{
    public static class PageFlowCodexValidator
    {
        public static PageFlowCodexValidation Validate(
              ReadOnlySpan<string> definitionIdentifiers
            , ReadOnlySpan<string> scopeIdentifiers
        )
        {
            var firstEmptyIndex = -1;
            var definitionSet = new HashSet<string>(StringComparer.Ordinal);
            var duplicateSet = new HashSet<string>(StringComparer.Ordinal);
            var duplicates = new List<string>();

            for (var i = 0; i < definitionIdentifiers.Length; i++)
            {
                var identifier = definitionIdentifiers[i];

                if (string.IsNullOrEmpty(identifier))
                {
                    if (firstEmptyIndex < 0)
                    {
                        firstEmptyIndex = i;
                    }

                    continue;
                }

                if (definitionSet.Add(identifier) == false && duplicateSet.Add(identifier))
                {
                    duplicates.Add(identifier);
                }
            }

            var scopeSet = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < scopeIdentifiers.Length; i++)
            {
                scopeSet.Add(scopeIdentifiers[i]);
            }

            return new PageFlowCodexValidation(
                  firstEmptyIndex
                , ToArray(duplicates)
                , GetMissing(definitionIdentifiers, scopeSet)
                , GetMissing(scopeIdentifiers, definitionSet)
            );
        }

        private static string[] GetMissing(ReadOnlySpan<string> identifiers, HashSet<string> existing)
        {
            var reported = new HashSet<string>(StringComparer.Ordinal);
            var missing = new List<string>();

            for (var i = 0; i < identifiers.Length; i++)
            {
                var identifier = identifiers[i];

                if (string.IsNullOrEmpty(identifier) || existing.Contains(identifier))
                {
                    continue;
                }

                if (reported.Add(identifier))
                {
                    missing.Add(identifier);
                }
            }

            return ToArray(missing);
        }

        private static string[] ToArray(List<string> list)
            => list.Count > 0 ? list.ToArray() : Array.Empty<string>();
    }
}
