#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Editor.PageFlows
{
    internal enum RowProblemKind
    {
        EmptyIdentifier,
        UnknownIdentifier,
        DuplicateIdentifier,
        ContainerNotInLayout,
        ContainerMatchesMany,
        ContainerWithoutLayout,
    }

    internal readonly record struct RowProblem(
          RowProblemKind Kind
        , string Value
        , string TypeName
        , string[] Scopes
        , int[] OtherRows
        , string AssetName = null
        , int MatchCount = 0
        , string FirstPath = null
    )
    {
        public bool IsIdentifierProblem => Kind <= RowProblemKind.DuplicateIdentifier;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToProblem()
            => Kind switch {
                RowProblemKind.EmptyIdentifier
                    => "This row has no identifier, so the codex cannot match it to a scope.",
                RowProblemKind.UnknownIdentifier => $"'{Value}' is not a scope of {TypeName}.",
                RowProblemKind.DuplicateIdentifier => $"'{Value}' is also used by row {JoinRows(OtherRows)}. "
                    + "Each scope can have only one flow.",
                RowProblemKind.ContainerNotInLayout => $"'{Value}' is not an element of Layout Asset {AssetName}. "
                    + "Unless code creates it before the codex initializes, the flow goes under the codex root.",
                RowProblemKind.ContainerMatchesMany => $"'{Value}' matches {MatchCount} elements of Layout Asset "
                    + $"{AssetName}; the first one ({FirstPath}) is used.",
                RowProblemKind.ContainerWithoutLayout => $"No Layout Asset is assigned, so the container '{Value}' "
                    + "must be created by code before the codex initializes; otherwise the flow goes under the "
                    + "codex root.",
                _ => string.Empty,
            };

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToFix()
            => Kind switch {
                RowProblemKind.EmptyIdentifier => "Choose a scope from the Identifier dropdown, or press Sync Scopes.",
                RowProblemKind.UnknownIdentifier => $"Choose one of {string.Join(", ", Scopes)}, add a PageFlowScope "
                    + $"property named {Value} to {TypeName}, or press Sync Scopes.",
                RowProblemKind.DuplicateIdentifier => "Choose another scope for one of these rows, "
                    + "or press Sync Scopes (it keeps the first row).",
                RowProblemKind.ContainerNotInLayout => $"Press ⋯ and pick an element, or name an element of "
                    + $"{AssetName} '{Value}'.",
                RowProblemKind.ContainerMatchesMany => $"Give the intended element a unique name in {AssetName}, "
                    + "then pick it with ⋯.",
                RowProblemKind.ContainerWithoutLayout => $"Assign a Layout Asset that contains an element named "
                    + $"'{Value}', or clear the field to use the codex root.",
                _ => string.Empty,
            };

        private static string JoinRows(int[] rows)
        {
            var numbers = new string[rows.Length];

            for (var i = 0; i < rows.Length; i++)
            {
                numbers[i] = (rows[i] + 1).ToString();
            }

            return string.Join(", ", numbers);
        }
    }

    internal readonly record struct ListProblem(string[] MissingScopes)
    {
        public bool HasProblem => MissingScopes != null && MissingScopes.Length > 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToProblem()
            => MissingScopes.Length > 1
                ? $"{string.Join(", ", MissingScopes)} have no flow definition, so the codex cannot create those flows."
                : $"{string.Join(", ", MissingScopes)} has no flow definition, so the codex cannot create that flow.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToFix()
            => "Press Sync Scopes.";
    }

    internal readonly record struct IdentifierChoice(
          string Value
        , bool Checked
        , bool Disabled
        , string Note
        , bool SeparatorAfter
    );

    internal static class ProblemText
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ToTooltip(string problem, string fix)
            => $"{problem}\nFix: {fix}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ToTooltip(ReadOnlySpan<RowProblem> problems)
        {
            var parts = new string[problems.Length];

            for (var i = 0; i < problems.Length; i++)
            {
                var problem = problems[i];
                parts[i] = ToTooltip(problem.ToProblem(), problem.ToFix());
            }

            return string.Join("\n\n", parts);
        }
    }
}

#endif
