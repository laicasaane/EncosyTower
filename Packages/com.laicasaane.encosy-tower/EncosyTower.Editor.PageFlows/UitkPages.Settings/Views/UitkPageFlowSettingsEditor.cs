#if UNITY_EDITOR

using System;
using EncosyTower.Editor.Settings;
using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages.Settings.Views
{
    internal class UitkPageFlowSettingsEditor
    {
        public static readonly string ProjectSettingsUssClassName = "project-settings";
        public static readonly string ProjectSettingsTitleBarUssClassName = $"{ProjectSettingsUssClassName}-title-bar";
        public static readonly string ProjectSettingsTitleBarLabelUssClassName =
            $"{ProjectSettingsTitleBarUssClassName}__label";

        public static readonly string UssClassName = "uitk-page-flow";

        private readonly SerializedObject _serializedSettings;

        private bool _valueUpdated;

        public UitkPageFlowSettingsEditor(SerializedObject serializedSettings, VisualElement root)
        {
            _serializedSettings = serializedSettings;

            root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = "UI Toolkit Page Flow",
            };

            titleBar.AddToClassList(ProjectSettingsTitleBarUssClassName);
            titleLabel.AddToClassList(ProjectSettingsTitleBarLabelUssClassName);

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var container = new ScrollView();
            container.AddToClassList(UssClassName);
            root.Add(container);

            var contentContainer = container.Q("unity-content-container");

            var warnNoSubscriber = new Toggle("Warn No Subscriber");
            var loaderStrategy = new EnumField("Loader Strategy", default(PageLoaderStrategy));
            var logEnvironment = new EnumField("Log Environment", default(LogEnvironment));
            var forceUIDocument = new Toggle("Force UI Document") {
                tooltip = "Use UI Document instead of Panel Renderer on Unity 6000.5 or newer.",
            };

            var callerInfoFoldout = PageFlowCallerInfoSection.Create();

            contentContainer.Add(warnNoSubscriber.WithAlignFieldClass());
            contentContainer.Add(loaderStrategy.WithAlignFieldClass());
            contentContainer.Add(logEnvironment.WithAlignFieldClass());
            contentContainer.Add(forceUIDocument.WithAlignFieldClass());
            contentContainer.Add(callerInfoFoldout);

            warnNoSubscriber.RegisterValueChangedCallback(OnValueChanged);
            loaderStrategy.RegisterValueChangedCallback(OnValueChanged);
            logEnvironment.RegisterValueChangedCallback(OnValueChanged);
            forceUIDocument.RegisterValueChangedCallback(OnValueChanged);

            warnNoSubscriber.WithBindProperty(Find(nameof(UitkPageFlowSettings.warnNoSubscriber)));
            loaderStrategy.WithBindProperty(Find(nameof(UitkPageFlowSettings.loaderStrategy)));
            logEnvironment.WithBindProperty(Find(nameof(UitkPageFlowSettings.logEnvironment)));
            forceUIDocument.WithBindProperty(Find(nameof(UitkPageFlowSettings.forceUIDocument)));

            contentContainer.WithBind(serializedSettings);
        }

        public void Update()
        {
            if (_valueUpdated == false)
            {
                return;
            }

            _valueUpdated = false;

            var serializedObject = _serializedSettings;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private SerializedProperty Find(string name)
            => _serializedSettings.FindProperty(name);

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

        [CustomEditor(typeof(UitkPageFlowSettings), true)]
        private sealed class Inspector : UnityEditor.Editor
        {
            private UitkPageFlowSettings _settings;

            private void OnEnable()
            {
                _settings = target as UitkPageFlowSettings;
            }

            public override VisualElement CreateInspectorGUI()
            {
                var root = new VisualElement();
                root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
                root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

                var button = new Button(OpenSettingsWindow) {
                    text = "Open UI Toolkit Page Flow Settings Window",
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
