#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Formats the problem and fix texts as tooltips.
    /// </summary>
    /// <remarks>
    /// Both methods are marked <see cref="MethodImplOptions.NoInlining"/> because they run on a cold path.
    /// </remarks>
    internal static class TooltipAPI
    {
        /// <summary>
        /// Formats a problem and its fix as one tooltip.
        /// </summary>
        /// <param name="problem">The text that says what is wrong.</param>
        /// <param name="fix">The text that says how to resolve it.</param>
        /// <returns>The problem, a new line, then <c>Fix:</c> and the fix.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ToTooltip(string problem, string fix)
            => PageFlowsViewResources.Get().Problems.Tooltip(problem, fix);

        /// <summary>
        /// Formats the problems of a row as one tooltip.
        /// </summary>
        /// <param name="errors">The problems of the row.</param>
        /// <returns>
        /// The tooltip of each problem, separated by a blank line. An empty string when
        /// <paramref name="errors"/> is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ToTooltip(ReadOnlySpan<FlowDefinitionError> errors)
        {
            var parts = new string[errors.Length];

            for (var i = 0; i < errors.Length; i++)
            {
                var error = errors[i];
                parts[i] = ToTooltip(error.ToProblem(), error.ToFix());
            }

            return string.Join("\n\n", parts);
        }
    }
}

#endif
