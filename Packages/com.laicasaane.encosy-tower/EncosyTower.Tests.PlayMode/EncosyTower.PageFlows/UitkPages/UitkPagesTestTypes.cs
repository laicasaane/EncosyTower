using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    [UxmlElement]
    public partial class TestElementPage : UitkPageElement
    {
        public TestElementPage()
        {
            TestFocusMode = DefaultFocusMode;
            TestFocusElementName = DefaultFocusElementName;
        }

        public static PageFocusMode DefaultFocusMode { get; set; }

        public static string DefaultFocusElementName { get; set; }

        public int Counter { get; set; }

        public PageFocusMode TestFocusMode { get; set; }

        public string TestFocusElementName { get; set; }

        public override PageFocusMode FocusMode => TestFocusMode;

        public override string FocusElementName => TestFocusElementName;
    }

    [PageFlowScopeCollection]
    public partial struct UitkTestScopes
    {
        public PageFlowScope Screen { get; private set; }

        public PageFlowScope Popup { get; private set; }
    }

    public sealed class UitkTestInitializer : UitkPageCodexInitializer<UitkTestScopes>
    {
        public int CallCount { get; private set; }

        public UitkTestScopes Scopes { get; private set; }

        protected override UnityTask OnInitializeAsync(UitkPageCodex codex, UitkTestScopes scopes)
        {
            CallCount++;
            Scopes = scopes;
            return UnityTask.CompletedTask;
        }
    }
}
