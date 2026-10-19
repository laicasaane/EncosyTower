#if UNITY_EDITOR && UNITY_UGUI

using System;
using EncosyTower.Editor.Settings;
using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Loading;
using EncosyTower.Pooling;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings.Views
{
    /// <summary>
    /// Builds the uGUI page of the Page Flows settings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The page has the rows Warn No Subscriber, Reject Duplicate Pages, Loader Strategy, Message Scope and Log
    /// Environment, a GameObject Pooling foldout for the renting and returning strategies, and the Caller Info foldout
    /// of <see cref="PageFlowCallerInfoSection"/>. The rows are bound to the serialized settings, so the binding gives
    /// the Undo behaviour. The Caller Info foldout is not bound.
    /// </para>
    /// <para>
    /// A change of a bound value only sets a flag. The next call of <see cref="Update"/> applies the change and saves
    /// the settings asset, so the asset is written to disk one editor update after each change.
    /// </para>
    /// </remarks>
    /// <seealso cref="UguiPageFlowSettings"/>
    internal class UguiPageFlowSettingsEditor
    {
        public static readonly string ProjectSettingsUssClassName = "project-settings";
        public static readonly string ProjectSettingsTitleBarUssClassName = $"{ProjectSettingsUssClassName}-title-bar";
        public static readonly string ProjectSettingsTitleBarLabelUssClassName = $"{ProjectSettingsTitleBarUssClassName}__label";
        public static readonly string UssClassName = "ugui-page-flow";

        private readonly SerializedContext _context;

        private bool _valueUpdated;

        /// <summary>
        /// Builds the page and binds its rows.
        /// </summary>
        /// <param name="serializedSettings">
        /// The serialized <see cref="UguiPageFlowSettings"/> asset that the rows bind to.
        /// </param>
        /// <param name="root">
        /// The root element of the page. It receives the style sheets, the title bar and a scroll view.
        /// </param>
        public UguiPageFlowSettingsEditor(SerializedObject serializedSettings, VisualElement root)
        {
            root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

            var texts = PageFlowsViewResources.Get().Settings;
            var context = _context = new SerializedContext(serializedSettings);
            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = texts.UguiTitle,
            };

            titleBar.AddToClassList(ProjectSettingsTitleBarUssClassName);
            titleLabel.AddToClassList(ProjectSettingsTitleBarLabelUssClassName);

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var container = new ScrollView();
            container.AddToClassList(UssClassName);
            root.Add(container);

            var contentContainer = container.Q("unity-content-container");

            var warnNoSubscriber = new Toggle(texts.WarnNoSubscriber);
            var rejectDuplicatePages = new Toggle(texts.RejectDuplicatePages);
            var loaderStrategy = new EnumField(texts.LoaderStrategy, default(PageLoaderStrategy));
            var messageScope = new EnumField(texts.MessageScope, default(UguiMessageScope));
            var logEnvironment = new EnumField(texts.LogEnvironment, default(LogEnvironment));
            var poolFoldout = new Foldout { text = texts.Pooling };
            var poolRentingStrategy = new EnumField(texts.Renting, default(RentingStrategy));
            var poolReturningStrategy = new EnumField(texts.Returning, default(ReturningStrategy));
            var callerInfoFoldout = PageFlowCallerInfoSection.Create();

            contentContainer.Add(warnNoSubscriber.WithAlignFieldClass());
            contentContainer.Add(rejectDuplicatePages.WithAlignFieldClass());
            contentContainer.Add(loaderStrategy.WithAlignFieldClass());
            contentContainer.Add(messageScope.WithAlignFieldClass());
            contentContainer.Add(logEnvironment.WithAlignFieldClass());

            contentContainer.Add(poolFoldout);
            poolFoldout.Add(poolRentingStrategy.WithAlignFieldClass());
            poolFoldout.Add(poolReturningStrategy.WithAlignFieldClass());

            contentContainer.Add(callerInfoFoldout);

            warnNoSubscriber.RegisterValueChangedCallback(OnValueChanged);
            rejectDuplicatePages.RegisterValueChangedCallback(OnValueChanged);
            loaderStrategy.RegisterValueChangedCallback(OnValueChanged);
            poolRentingStrategy.RegisterValueChangedCallback(OnValueChanged);
            poolReturningStrategy.RegisterValueChangedCallback(OnValueChanged);
            messageScope.RegisterValueChangedCallback(OnValueChanged);
            logEnvironment.RegisterValueChangedCallback(OnValueChanged);

            warnNoSubscriber.WithBindProperty(context.GetWarnNoSubscriber());
            rejectDuplicatePages.WithBindProperty(context.GetRejectDuplicatePages());
            loaderStrategy.WithBindProperty(context.GetLoaderStrategyProperty());
            poolRentingStrategy.WithBindProperty(context.GetPoolRentingStrategyProperty());
            poolReturningStrategy.WithBindProperty(context.GetPoolReturningStrategyProperty());
            messageScope.WithBindProperty(context.GetMessageScopeProperty());
            logEnvironment.WithBindProperty(context.GetLogEnvironmentProperty());

            contentContainer.WithBind(serializedSettings);
        }

        /// <summary>
        /// Saves the settings asset when a bound value changed.
        /// </summary>
        /// <remarks>
        /// Does nothing when no value changed since the last call. Otherwise it applies the modified properties, reads
        /// them again, marks the asset dirty and saves it at once. Call it on every inspector update.
        /// </remarks>
        public void Update()
        {
            if (_valueUpdated == false)
            {
                return;
            }

            _valueUpdated = false;

            var serializedObject = _context.Object;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            if (evt == null)
            {
                return;
            }

            if (evt.newValue.Equals(evt.previousValue) == false)
            {
                _valueUpdated = true;
            }
        }

        private void OnValueChanged(ChangeEvent<Enum> evt)
        {
            if (evt == null)
            {
                return;
            }

            if (Equals(evt.newValue, evt.previousValue) == false)
            {
                _valueUpdated = true;
            }
        }

        /// <summary>
        /// Replaces the default inspector of the settings asset with a button that opens the settings page.
        /// </summary>
        [CustomEditor(typeof(UguiPageFlowSettings), true)]
        private sealed class Inspector : UnityEditor.Editor
        {
            private UguiPageFlowSettings _settings;

            private void OnEnable()
            {
                _settings = target as UguiPageFlowSettings;
            }

            /// <inheritdoc/>
            public override VisualElement CreateInspectorGUI()
            {
                var root = new VisualElement();
                root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
                root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

                var button = new Button(OpenSettingsWindow) {
                    text = PageFlowsViewResources.Get().Settings.OpenUguiWindow,
                };

                button.AddToClassList("button-open-settings-window");
                root.Add(button);
                return root;
            }

            private void OpenSettingsWindow()
            {
                if (_settings.IsValid())
                {
                    _settings.OpenSettingsWindow();
                }
            }
        }
    }
}

#endif
