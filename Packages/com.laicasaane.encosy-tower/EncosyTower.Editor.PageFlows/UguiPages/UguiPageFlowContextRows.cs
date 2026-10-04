#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.PageFlows.UguiPages;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    internal static class UguiPageFlowContextRows
    {
        public static readonly ContextRow[] Rows = {
            new(nameof(UguiPageFlowContext.warnNoSubscriber), "warn-no-subscriber"),
            new(nameof(UguiPageFlowContext.loadStrategy), "load-strategy"),
            new(nameof(UguiPageFlowContext.messageScope), "message-scope"),
            new(nameof(UguiPageFlowContext.logEnvironment), "log-environment"),
            new(nameof(UguiPageFlowContext.poolRentingStrategy), "renting-strategy", "pooling"),
            new(nameof(UguiPageFlowContext.poolReturningStrategy), "returning-strategy"),
        };
    }
}

#endif
