using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>Extensions for the result of a page flow call.</summary>
    public static class PageFlowResultExtensions
    {
        /// <summary>True when the flow applied the change.</summary>
        /// <param name="self">The result of the call.</param>
        /// <returns>
        /// <c>true</c> for a success, and for a failure whose <see cref="PageFlowError.TransitionState"/> is
        /// <see cref="PageTransitionState.Completed"/>.
        /// </returns>
        public static bool IsApplied(in this Success<PageFlowError> self)
            => self.IsSuccess || self.GetFailureOrDefault().TransitionState == PageTransitionState.Completed;
    }
}
