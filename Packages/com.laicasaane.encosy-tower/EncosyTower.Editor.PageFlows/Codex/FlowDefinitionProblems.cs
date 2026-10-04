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
        {
            var problems = PageFlowsViewResources.Get().Problems;

            return Kind switch {
                RowProblemKind.EmptyIdentifier => problems.EmptyIdentifier,
                RowProblemKind.UnknownIdentifier => problems.UnknownIdentifier(Value, TypeName),
                RowProblemKind.DuplicateIdentifier => problems.DuplicateIdentifier(Value, JoinRows(OtherRows)),
                RowProblemKind.ContainerNotInLayout => problems.ContainerNotInLayout(Value, AssetName),
                RowProblemKind.ContainerMatchesMany => problems.ContainerMatchesMany(
                      Value
                    , MatchCount
                    , AssetName
                    , FirstPath
                ),
                RowProblemKind.ContainerWithoutLayout => problems.ContainerWithoutLayout(Value),
                _ => string.Empty,
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToFix()
        {
            var problems = PageFlowsViewResources.Get().Problems;

            return Kind switch {
                RowProblemKind.EmptyIdentifier => problems.EmptyIdentifierFix,
                RowProblemKind.UnknownIdentifier => problems.UnknownIdentifierFix(
                      string.Join(", ", Scopes)
                    , Value
                    , TypeName
                ),
                RowProblemKind.DuplicateIdentifier => problems.DuplicateIdentifierFix,
                RowProblemKind.ContainerNotInLayout => problems.ContainerNotInLayoutFix(AssetName, Value),
                RowProblemKind.ContainerMatchesMany => problems.ContainerMatchesManyFix(AssetName),
                RowProblemKind.ContainerWithoutLayout => problems.ContainerWithoutLayoutFix(Value),
                _ => string.Empty,
            };
        }

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
            => PageFlowsViewResources.Get().Problems.MissingScopes(
                  string.Join(", ", MissingScopes)
                , MissingScopes.Length
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToFix()
            => PageFlowsViewResources.Get().Problems.MissingScopesFix;
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
            => PageFlowsViewResources.Get().Problems.Tooltip(problem, fix);

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
