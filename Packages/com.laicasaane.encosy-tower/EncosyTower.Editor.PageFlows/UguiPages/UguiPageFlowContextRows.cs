#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.PageFlows.UguiPages;

namespace EncosyTower.Editor.PageFlows.UguiPages
{
    internal static class UguiPageFlowContextRows
    {
        public static readonly ContextRow[] Rows = {
            new(nameof(UguiPageFlowContext.warnNoSubscriber), "Warn No Subscriber"),
            new(nameof(UguiPageFlowContext.loadStrategy), "Load Strategy"),
            new(nameof(UguiPageFlowContext.messageScope), "Message Scope"),
            new(nameof(UguiPageFlowContext.logEnvironment), "Log Environment"),
            new(nameof(UguiPageFlowContext.poolRentingStrategy), "Renting Strategy", "GameObject Pooling"),
            new(nameof(UguiPageFlowContext.poolReturningStrategy), "Returning Strategy"),
        };
    }
}

#endif
