#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Gives typed access to the localized texts of the editor module.
    /// </summary>
    /// <param name="Root">The cloned UXML tree that holds the text labels.</param>
    /// <remarks>
    /// <para>
    /// Every text of the module is a <c>Label</c> of the text resources UXML, found by its name, which has the form
    /// <c>group__key</c>. The properties named after the groups return small wrappers over <paramref name="Root"/>.
    /// </para>
    /// <para>
    /// The UXML is cloned once and cached. It is cloned again when its content hash changes. If the asset cannot be
    /// loaded, an error is logged and every text is empty. A missing label logs an error and gives an empty text.
    /// </para>
    /// <para>
    /// A text with arguments goes through <see cref="string.Format(string, object[])"/>, so the placeholders in the
    /// UXML text must match the arguments of the call. Use it from the Editor main thread. The cache is static and is
    /// reset by a domain reload.
    /// </para>
    /// </remarks>
    internal readonly record struct PageFlowsViewResources(VisualElement Root)
    {
        /// <summary>
        /// The project path of the text resources UXML.
        /// </summary>
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

        public ViewResources_Transition Transition => new(this);

        public ViewResources_Scope Scope => new(this);

        public ViewResources_Problems Problems => new(this);

        public ViewResources_Panel Panel => new(this);

        public ViewResources_Picker Picker => new(this);

        public ViewResources_Settings Settings => new(this);

        public ViewResources_CallerInfo CallerInfo => new(this);

        /// <summary>
        /// Gets the text resources.
        /// </summary>
        /// <returns>
        /// A wrapper over the cached tree. It never throws. If the asset cannot be loaded, the tree is empty and all
        /// texts are empty.
        /// </returns>
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

        private string GetText(string name)
        {
            if (Root.Q<Label>(name: name)?.text is string value)
            {
                return value;
            }

            StaticDevLogger.LogError($"Cannot find Label by name `{name}`");
            return string.Empty;
        }

        private string Format(string name, params object[] args)
            => string.Format(GetText(name), args);

        /// <summary>
        /// The texts of the Context section (<c>context__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Context(PageFlowsViewResources Resources)
        {
            private const string BASE = "context";

            public string Heading => Resources.GetText($"{BASE}__heading");

            public string OpenSettings => Resources.GetText($"{BASE}__open-settings");

            public string AutoInitialize => Resources.GetText($"{BASE}__auto-initialize");

            /// <summary>
            /// Gets the text of a row of the Context section.
            /// </summary>
            /// <param name="key">The <see cref="ContextSetting.TextKey"/> or group key of the row.</param>
            /// <returns>The text <c>context__{key}</c>.</returns>
            public string GetText(string key)
                => Resources.GetText($"{BASE}__{key}");
        }

        /// <summary>
        /// The texts of the Flow Definitions section (<c>flows__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Flows(PageFlowsViewResources Resources)
        {
            private const string BASE = "flows";

            public string Heading => Resources.GetText($"{BASE}__heading");

            public string Sync => Resources.GetText($"{BASE}__sync");

            public string ScopeCollection => Resources.GetText($"{BASE}__scope-collection");

            public string MultiEdit => Resources.GetText($"{BASE}__multi-edit");

            public string SyncNoCollection => Resources.GetText($"{BASE}__sync-no-collection");

            public string NoteInUse => Resources.GetText($"{BASE}__note-in-use");

            public string NoteNotAScope => Resources.GetText($"{BASE}__note-not-a-scope");

            public string SyncMatches(string typeName)
                => Resources.Format($"{BASE}__sync-matches", typeName);

            public string SyncRule(string typeName)
                => Resources.Format($"{BASE}__sync-rule", typeName);
        }

        /// <summary>
        /// The column titles of the flow definition list (<c>column__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Columns(PageFlowsViewResources Resources)
        {
            private const string BASE = "column";

            public string Identifier => Resources.GetText($"{BASE}__identifier");

            public string Kind => Resources.GetText($"{BASE}__kind");

            public string ContainerName => Resources.GetText($"{BASE}__container-name");

            public string Override => Resources.GetText($"{BASE}__override");

            public string SortingLayer => Resources.GetText($"{BASE}__sorting-layer");

            public string Order => Resources.GetText($"{BASE}__order");
        }

        /// <summary>
        /// The labels of the start and end values of a transition (<c>transition__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Transition(PageFlowsViewResources Resources)
        {
            private const string BASE = "transition";

            public string Start => Resources.GetText($"{BASE}__start");

            public string End => Resources.GetText($"{BASE}__end");
        }

        /// <summary>
        /// The texts about the scope collection of a codex (<c>scope__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        /// <remarks>
        /// The tooltips take the name of the initializer interface or component, and for an unreadable scope collection
        /// also the exception type and message.
        /// </remarks>
        public readonly record struct ViewResources_Scope(PageFlowsViewResources Resources)
        {
            private const string BASE = "scope";

            public string NoInitializer => Resources.GetText($"{BASE}__no-initializer");

            public string NullApplier => Resources.GetText($"{BASE}__null-applier");

            public string ScopesUnreadable => Resources.GetText($"{BASE}__scopes-unreadable");

            public string NoInitializerTooltip(string interfaceName)
                => Resources.Format($"{BASE}__no-initializer-tooltip", interfaceName);

            public string NullApplierTooltip(string interfaceName)
                => Resources.Format($"{BASE}__null-applier-tooltip", interfaceName);

            public string ScopesUnreadableTooltip(string typeName, string exceptionTypeName, string message)
                => Resources.Format($"{BASE}__scopes-unreadable-tooltip", typeName, exceptionTypeName, message);
        }

        /// <summary>
        /// The problem and fix texts of the flow definitions (<c>problem__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Problems(PageFlowsViewResources Resources)
        {
            private const string BASE = "problem";

            public string EmptyIdentifier => Resources.GetText($"{BASE}__empty-identifier");

            public string EmptyIdentifierFix => Resources.GetText($"{BASE}__empty-identifier-fix");

            public string DuplicateIdentifierFix => Resources.GetText($"{BASE}__duplicate-identifier-fix");

            public string MissingScopesFix => Resources.GetText($"{BASE}__missing-scopes-fix");

            public string Tooltip(string problem, string fix)
                => Resources.Format($"{BASE}__tooltip", problem, fix);

            public string UnknownIdentifier(string value, string typeName)
                => Resources.Format($"{BASE}__unknown-identifier", value, typeName);

            public string UnknownIdentifierFix(string scopes, string value, string typeName)
                => Resources.Format($"{BASE}__unknown-identifier-fix", scopes, value, typeName);

            public string DuplicateIdentifier(string value, string definitions)
                => Resources.Format($"{BASE}__duplicate-identifier", value, definitions);

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

            /// <summary>
            /// Gets the text for scopes without a flow definition.
            /// </summary>
            /// <param name="scopes">The scope names, joined.</param>
            /// <param name="count">The number of scopes. One selects the singular text; more select the plural.</param>
            /// <returns>The formatted text.</returns>
            public string MissingScopes(string scopes, int count)
                => Resources.Format(count > 1 ? $"{BASE}__missing-scopes" : $"{BASE}__missing-scope", scopes);
        }

        /// <summary>
        /// The texts of the Panel section of the UI Toolkit codex inspector (<c>panel__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Panel(PageFlowsViewResources Resources)
        {
            private const string BASE = "panel";

            public string Heading => Resources.GetText($"{BASE}__heading");

            public string PanelSettings => Resources.GetText($"{BASE}__panel-settings");

            public string SortingOrder => Resources.GetText($"{BASE}__sorting-order");

            public string LayoutAsset => Resources.GetText($"{BASE}__layout-asset");

            public string LayoutAssetTooltip => Resources.GetText($"{BASE}__layout-asset-tooltip");

            public string LayoutAssetEmpty => Resources.GetText($"{BASE}__layout-asset-empty");

            public string ExistingDocument => Resources.GetText($"{BASE}__existing-document");

            public string ExistingRenderer => Resources.GetText($"{BASE}__existing-renderer");

            public string Component => Resources.GetText($"{BASE}__component");

            public string PanelRenderer => Resources.GetText($"{BASE}__panel-renderer");

            public string UIDocument => Resources.GetText($"{BASE}__ui-document");

            public string Added => Resources.GetText($"{BASE}__added");

            public string AddedDocument => Resources.GetText($"{BASE}__added-document");

            public string Forced => Resources.GetText($"{BASE}__forced");

            public string Existing => Resources.GetText($"{BASE}__existing");

            /// <summary>
            /// Gets the text that counts the named elements of the layout asset.
            /// </summary>
            /// <param name="count">The number of named elements.</param>
            /// <returns>The singular text for one element, otherwise the formatted plural text.</returns>
            public string LayoutAssetCount(int count)
                => count == 1
                    ? Resources.GetText($"{BASE}__layout-asset-one")
                    : Resources.Format($"{BASE}__layout-asset-many", count);
        }

        /// <summary>
        /// The texts of the container picker (<c>picker__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Picker(PageFlowsViewResources Resources)
        {
            private const string BASE = "picker";

            public string Tooltip => Resources.GetText($"{BASE}__tooltip");

            public string CodexRoot => Resources.GetText($"{BASE}__codex-root");

            public string NoLayoutAsset => Resources.GetText($"{BASE}__no-layout-asset");

            public string NoMatch => Resources.GetText($"{BASE}__no-match");

            public string NoIdentifier => Resources.GetText($"{BASE}__no-identifier");

            public string UsedBy(string identifiers)
                => Resources.Format($"{BASE}__used-by", identifiers);
        }

        /// <summary>
        /// The texts of the Project Settings pages (<c>settings__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_Settings(PageFlowsViewResources Resources)
        {
            private const string BASE = "settings";

            public string UguiTitle => Resources.GetText($"{BASE}__ugui-title");

            public string UitkTitle => Resources.GetText($"{BASE}__uitk-title");

            public string WarnNoSubscriber => Resources.GetText($"{BASE}__warn-no-subscriber");

            public string RejectDuplicatePages => Resources.GetText($"{BASE}__reject-duplicate-pages");

            public string LoaderStrategy => Resources.GetText($"{BASE}__loader-strategy");

            public string MessageScope => Resources.GetText($"{BASE}__message-scope");

            public string LogEnvironment => Resources.GetText($"{BASE}__log-environment");

            public string Pooling => Resources.GetText($"{BASE}__pooling");

            public string Renting => Resources.GetText($"{BASE}__renting");

            public string Returning => Resources.GetText($"{BASE}__returning");

            public string ForceUIDocument => Resources.GetText($"{BASE}__force-ui-document");

            public string ForceUIDocumentTooltip => Resources.GetText($"{BASE}__force-ui-document-tooltip");

            public string OpenUguiWindow => Resources.GetText($"{BASE}__open-ugui-window");

            public string OpenUitkWindow => Resources.GetText($"{BASE}__open-uitk-window");
        }

        /// <summary>
        /// The texts of the Caller Info foldout (<c>caller-info__*</c>).
        /// </summary>
        /// <param name="Resources">The text resources.</param>
        public readonly record struct ViewResources_CallerInfo(PageFlowsViewResources Resources)
        {
            private const string BASE = "caller-info";

            public string Heading => Resources.GetText($"{BASE}__heading");

            public string Option => Resources.GetText($"{BASE}__option");

            /// <summary>
            /// Gets the help text of the foldout.
            /// </summary>
            /// <param name="symbolForDev">The define symbol for development builds.</param>
            /// <param name="symbolAlways">The define symbol for all builds.</param>
            /// <returns>The formatted help text.</returns>
            public string Help(string symbolForDev, string symbolAlways)
                => Resources.Format($"{BASE}__help", symbolForDev, symbolAlways);
        }
    }
}

#endif
