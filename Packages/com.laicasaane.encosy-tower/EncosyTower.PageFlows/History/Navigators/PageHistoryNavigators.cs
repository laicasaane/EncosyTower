using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.PageFlows.Codex;

using ThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.History
{
    /// <summary>Creates the built-in navigator for a flow's kind.</summary>
    public static class PageHistoryNavigators
    {
        /// <summary>Creates the built-in navigator for a flow.</summary>
        /// <param name="flow">The flow to replay entries on.</param>
        /// <returns>The navigator that matches <see cref="IPageFlowRecordable.Kind"/> of the flow.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="flow"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The kind of the flow has no built-in navigator.
        /// </exception>
        public static IPageHistoryNavigator Create([NotNull] IPageFlowRecordable flow)
        {
            ThrowHelper.ThrowIfNull(flow);

            switch (flow.Kind)
            {
                case PageFlowKind.PageStackSingle:
                {
                    return new PageStackSingleNavigator(flow);
                }

                case PageFlowKind.PageStackMulti:
                {
                    return new PageStackMultiNavigator(flow);
                }

                case PageFlowKind.PageListSingle:
                {
                    return new PageListSingleNavigator(flow);
                }

                case PageFlowKind.PageListMulti:
                {
                    return new PageListMultiNavigator(flow);
                }

                default:
                {
                    ThrowKindOutOfRange(flow.Kind);
                    return null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [UnityEngine.HideInCallstack, StackTraceHidden, DoesNotReturn]
        private static void ThrowKindOutOfRange(PageFlowKind kind)
            => throw new ArgumentOutOfRangeException("kind", kind, "The page flow kind has no built-in navigator.");
    }
}
