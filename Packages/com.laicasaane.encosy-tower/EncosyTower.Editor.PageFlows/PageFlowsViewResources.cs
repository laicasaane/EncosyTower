#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    internal readonly record struct PageFlowsViewResources(VisualElement Root)
    {
        public const string RESOURCES_PATH =
            $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor.PageFlows/StyleSheets/PageFlows_Resources.uxml";

        private static VisualTreeAsset s_asset;
        private static VisualElement s_root;
        private static int s_contentHash;

        public static implicit operator PageFlowsViewResources(VisualElement root)
            => new(root);

        public ViewResources_Context Context => new(this);

        public ViewResources_Flows Flows => new(this);

        public ViewResources_Columns Columns => new(this);

        public ViewResources_Scope Scope => new(this);

        public ViewResources_Problems Problems => new(this);

        public ViewResources_Panel Panel => new(this);

        public ViewResources_Picker Picker => new(this);

        public ViewResources_Settings Settings => new(this);

        public ViewResources_CallerInfo CallerInfo => new(this);

        public static PageFlowsViewResources Get()
        {
            if (s_asset.IsInvalid())
            {
                s_asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(RESOURCES_PATH);
                s_root = null;
            }

            if (s_asset.IsInvalid())
            {
                StaticDevLogger.LogError($"Cannot load the PageFlows view resources at `{RESOURCES_PATH}`");
                return new(new VisualElement());
            }

            if (s_root == null || s_contentHash != s_asset.contentHash)
            {
                s_root = s_asset.CloneTree();
                s_contentHash = s_asset.contentHash;
            }

            return new(s_root);
        }

        private string GetLabelText(string name)
        {
            if (Root.Q<Label>(name: name)?.text is string value)
            {
                return value;
            }

            StaticDevLogger.LogError($"Cannot find Label by name `{name}`");
            return string.Empty;
        }

        private string Format(string name, params object[] args)
            => string.Format(GetLabelText(name), args);

        public readonly record struct ViewResources_Context(PageFlowsViewResources Resources)
        {
            private const string BASE = "context";

            public string Heading => Resources.GetLabelText($"{BASE}__heading");

            public string OpenSettings => Resources.GetLabelText($"{BASE}__open-settings");

            public string AutoInitialize => Resources.GetLabelText($"{BASE}__auto-initialize");

            public string GetText(string key)
                => Resources.GetLabelText($"{BASE}__{key}");
        }

        public readonly record struct ViewResources_Flows(PageFlowsViewResources Resources)
        {
            private const string BASE = "flows";

            public string Heading => Resources.GetLabelText($"{BASE}__heading");

            public string Sync => Resources.GetLabelText($"{BASE}__sync");

            public string ScopeCollection => Resources.GetLabelText($"{BASE}__scope-collection");

            public string MultiEdit => Resources.GetLabelText($"{BASE}__multi-edit");

            public string SyncNoCollection => Resources.GetLabelText($"{BASE}__sync-no-collection");

            public string NoteInUse => Resources.GetLabelText($"{BASE}__note-in-use");

            public string NoteNotAScope => Resources.GetLabelText($"{BASE}__note-not-a-scope");

            public string SyncMatches(string typeName)
                => Resources.Format($"{BASE}__sync-matches", typeName);

            public string SyncRule(string typeName)
                => Resources.Format($"{BASE}__sync-rule", typeName);
        }

        public readonly record struct ViewResources_Columns(PageFlowsViewResources Resources)
        {
            private const string BASE = "column";

            public string Identifier => Resources.GetLabelText($"{BASE}__identifier");

            public string Kind => Resources.GetLabelText($"{BASE}__kind");

            public string ContainerName => Resources.GetLabelText($"{BASE}__container-name");

            public string Override => Resources.GetLabelText($"{BASE}__override");

            public string SortingLayer => Resources.GetLabelText($"{BASE}__sorting-layer");

            public string Order => Resources.GetLabelText($"{BASE}__order");
        }

        public readonly record struct ViewResources_Scope(PageFlowsViewResources Resources)
        {
            private const string BASE = "scope";

            public string NoInitializer => Resources.GetLabelText($"{BASE}__no-initializer");

            public string NullApplier => Resources.GetLabelText($"{BASE}__null-applier");

            public string NoInitializerTooltip(string interfaceName)
                => Resources.Format($"{BASE}__no-initializer-tooltip", interfaceName);

            public string NullApplierTooltip(string interfaceName)
                => Resources.Format($"{BASE}__null-applier-tooltip", interfaceName);
        }

        public readonly record struct ViewResources_Problems(PageFlowsViewResources Resources)
        {
            private const string BASE = "problem";

            public string EmptyIdentifier => Resources.GetLabelText($"{BASE}__empty-identifier");

            public string EmptyIdentifierFix => Resources.GetLabelText($"{BASE}__empty-identifier-fix");

            public string DuplicateIdentifierFix => Resources.GetLabelText($"{BASE}__duplicate-identifier-fix");

            public string MissingScopesFix => Resources.GetLabelText($"{BASE}__missing-scopes-fix");

            public string Tooltip(string problem, string fix)
                => Resources.Format($"{BASE}__tooltip", problem, fix);

            public string UnknownIdentifier(string value, string typeName)
                => Resources.Format($"{BASE}__unknown-identifier", value, typeName);

            public string UnknownIdentifierFix(string scopes, string value, string typeName)
                => Resources.Format($"{BASE}__unknown-identifier-fix", scopes, value, typeName);

            public string DuplicateIdentifier(string value, string rows)
                => Resources.Format($"{BASE}__duplicate-identifier", value, rows);

            public string ContainerNotInLayout(string value, string assetName)
                => Resources.Format($"{BASE}__container-not-in-layout", value, assetName);

            public string ContainerNotInLayoutFix(string assetName, string value)
                => Resources.Format($"{BASE}__container-not-in-layout-fix", assetName, value);

            public string ContainerMatchesMany(string value, int count, string assetName, string firstPath)
                => Resources.Format($"{BASE}__container-matches-many", value, count, assetName, firstPath);

            public string ContainerMatchesManyFix(string assetName)
                => Resources.Format($"{BASE}__container-matches-many-fix", assetName);

            public string ContainerWithoutLayout(string value)
                => Resources.Format($"{BASE}__container-without-layout", value);

            public string ContainerWithoutLayoutFix(string value)
                => Resources.Format($"{BASE}__container-without-layout-fix", value);

            public string MissingScopes(string scopes, int count)
                => Resources.Format(count > 1 ? $"{BASE}__missing-scopes" : $"{BASE}__missing-scope", scopes);
        }

        public readonly record struct ViewResources_Panel(PageFlowsViewResources Resources)
        {
            private const string BASE = "panel";

            public string Heading => Resources.GetLabelText($"{BASE}__heading");

            public string PanelSettings => Resources.GetLabelText($"{BASE}__panel-settings");

            public string SortingOrder => Resources.GetLabelText($"{BASE}__sorting-order");

            public string LayoutAsset => Resources.GetLabelText($"{BASE}__layout-asset");

            public string LayoutAssetTooltip => Resources.GetLabelText($"{BASE}__layout-asset-tooltip");

            public string LayoutAssetEmpty => Resources.GetLabelText($"{BASE}__layout-asset-empty");

            public string ExistingDocument => Resources.GetLabelText($"{BASE}__existing-document");

            public string ExistingRenderer => Resources.GetLabelText($"{BASE}__existing-renderer");

            public string Component => Resources.GetLabelText($"{BASE}__component");

            public string PanelRenderer => Resources.GetLabelText($"{BASE}__panel-renderer");

            public string UIDocument => Resources.GetLabelText($"{BASE}__ui-document");

            public string Added => Resources.GetLabelText($"{BASE}__added");

            public string AddedDocument => Resources.GetLabelText($"{BASE}__added-document");

            public string Forced => Resources.GetLabelText($"{BASE}__forced");

            public string Existing => Resources.GetLabelText($"{BASE}__existing");

            public string LayoutAssetCount(int count)
                => count == 1
                    ? Resources.GetLabelText($"{BASE}__layout-asset-one")
                    : Resources.Format($"{BASE}__layout-asset-many", count);
        }

        public readonly record struct ViewResources_Picker(PageFlowsViewResources Resources)
        {
            private const string BASE = "picker";

            public string Tooltip => Resources.GetLabelText($"{BASE}__tooltip");

            public string CodexRoot => Resources.GetLabelText($"{BASE}__codex-root");

            public string NoLayoutAsset => Resources.GetLabelText($"{BASE}__no-layout-asset");

            public string NoMatch => Resources.GetLabelText($"{BASE}__no-match");

            public string NoIdentifier => Resources.GetLabelText($"{BASE}__no-identifier");

            public string UsedBy(string identifiers)
                => Resources.Format($"{BASE}__used-by", identifiers);
        }

        public readonly record struct ViewResources_Settings(PageFlowsViewResources Resources)
        {
            private const string BASE = "settings";

            public string UguiTitle => Resources.GetLabelText($"{BASE}__ugui-title");

            public string UitkTitle => Resources.GetLabelText($"{BASE}__uitk-title");

            public string WarnNoSubscriber => Resources.GetLabelText($"{BASE}__warn-no-subscriber");

            public string LoaderStrategy => Resources.GetLabelText($"{BASE}__loader-strategy");

            public string MessageScope => Resources.GetLabelText($"{BASE}__message-scope");

            public string LogEnvironment => Resources.GetLabelText($"{BASE}__log-environment");

            public string Pooling => Resources.GetLabelText($"{BASE}__pooling");

            public string Renting => Resources.GetLabelText($"{BASE}__renting");

            public string Returning => Resources.GetLabelText($"{BASE}__returning");

            public string ForceUIDocument => Resources.GetLabelText($"{BASE}__force-ui-document");

            public string ForceUIDocumentTooltip => Resources.GetLabelText($"{BASE}__force-ui-document-tooltip");

            public string OpenUguiWindow => Resources.GetLabelText($"{BASE}__open-ugui-window");

            public string OpenUitkWindow => Resources.GetLabelText($"{BASE}__open-uitk-window");
        }

        public readonly record struct ViewResources_CallerInfo(PageFlowsViewResources Resources)
        {
            private const string BASE = "caller-info";

            public string Heading => Resources.GetLabelText($"{BASE}__heading");

            public string Option => Resources.GetLabelText($"{BASE}__option");

            public string Help(string symbolForDev, string symbolAlways)
                => Resources.Format($"{BASE}__help", symbolForDev, symbolAlways);
        }
    }
}

#endif
