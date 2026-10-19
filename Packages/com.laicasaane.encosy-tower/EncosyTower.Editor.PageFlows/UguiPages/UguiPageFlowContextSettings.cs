#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.PageFlows;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    /// <summary>
    /// Holds the context rows that the uGUI inspectors show.
    /// </summary>
    internal static class UguiPageFlowContextSettings
    {
        /// <summary>
        /// The rows of the Context section for <see cref="UguiPageFlowContext"/>, in display order.
        /// </summary>
        /// <remarks>
        /// The field names are <c>nameof</c> of the fields of the runtime context. The <c>pooling</c> group heading
        /// comes before the renting strategy row. <see cref="UguiPageFlowInspector"/> and
        /// <see cref="UguiPageCodexInspector"/> share the array, so do not modify it.
        /// </remarks>
        public static readonly ContextSetting[] All = {
            new(nameof(UguiPageFlowContext.warnNoSubscriber), "warn-no-subscriber"),
            new(nameof(UguiPageFlowContext.rejectDuplicatePages), "reject-duplicate-pages"),
            new(nameof(UguiPageFlowContext.loadStrategy), "load-strategy"),
            new(nameof(UguiPageFlowContext.messageScope), "message-scope"),
            new(nameof(UguiPageFlowContext.logEnvironment), "log-environment"),
            new(nameof(UguiPageFlowContext.poolRentingStrategy), "renting-strategy", "pooling"),
            new(nameof(UguiPageFlowContext.poolReturningStrategy), "returning-strategy"),
        };
    }
}

#endif
