#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.SourceGen.Settings.Views
{
    internal sealed class SourceGenSettingsEditor
    {
        public static readonly string ProjectSettingsUssClassName = "project-settings";
        public static readonly string ProjectSettingsTitleBarUssClassName = $"{ProjectSettingsUssClassName}-title-bar";
        public static readonly string ProjectSettingsTitleBarLabelUssClassName
            = $"{ProjectSettingsTitleBarUssClassName}__label";
        public static readonly string UssClassName = "source-gen";

        private readonly ScriptableObject _settings;
        private readonly SerializedObject _serializedSettings;
        private bool _valueUpdated;

        public SourceGenSettingsEditor(
              ScriptableObject settings
            , SerializedObject serializedSettings
            , VisualElement root
        )
        {
            _settings = settings;
            _serializedSettings = serializedSettings;

            root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

            var titleBar = new VisualElement();
            var titleLabel = new Label() {
                text = "SourceGen",
            };

            titleBar.AddToClassList(ProjectSettingsTitleBarUssClassName);
            titleLabel.AddToClassList(ProjectSettingsTitleBarLabelUssClassName);

            titleBar.Add(titleLabel);
            root.Add(titleBar);

            var container = new ScrollView();
            container.AddToClassList(UssClassName);
            root.Add(container);

            var contentContainer = container.Q("unity-content-container");
            var retainOutput = new Toggle("Retain Output");

            contentContainer.Add(retainOutput.WithAlignFieldClass());
            retainOutput.RegisterValueChangedCallback(OnValueChanged);
            retainOutput.WithBindProperty(
                serializedSettings.FindProperty(nameof(SourceGenSettings._retainOutput))
            );

            contentContainer.WithBind(serializedSettings);
        }

        public void Update()
            => Save();

        public void Save()
        {
            if (_valueUpdated == false)
            {
                return;
            }

            _serializedSettings.ApplyModifiedProperties();
            _serializedSettings.Update();

            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssetIfDirty(_settings);

            _valueUpdated = false;
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
    }
}

#endif
