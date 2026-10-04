#if UNITY_EDITOR && UNITY_UGUI

using System;
using EncosyTower.Editor.Settings;
using EncosyTower.Editor.UIElements;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Pooling;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings.Views
{
    internal class UguiPageFlowSettingsEditor
    {
        public static readonly string ProjectSettingsUssClassName = "project-settings";
        public static readonly string ProjectSettingsTitleBarUssClassName = $"{ProjectSettingsUssClassName}-title-bar";
        public static readonly string ProjectSettingsTitleBarLabelUssClassName = $"{ProjectSettingsTitleBarUssClassName}__label";
        public static readonly string UssClassName = "ugui-page-flow";

        private readonly SerializedContext _context;

        private bool _valueUpdated;

        public UguiPageFlowSettingsEditor(
              ScriptableObject settings
            , SerializedObject serializedSettings
            , VisualElement root
        )
        {
            root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);
            root.WithEditorStyleSheet(Constants.THEME_STYLE_SHEET);

            var texts = PageFlowsViewResources.Get().Settings;
            var context = _context = new SerializedContext(settings, serializedSettings);
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
            var loaderStrategy = new EnumField(texts.LoaderStrategy, default(PageLoaderStrategy));
            var messageScope = new EnumField(texts.MessageScope, default(UguiMessageScope));
            var logEnvironment = new EnumField(texts.LogEnvironment, default(LogEnvironment));
            var poolFoldout = new Foldout { text = texts.Pooling };
            var poolRentingStrategy = new EnumField(texts.Renting, default(RentingStrategy));
            var poolReturningStrategy = new EnumField(texts.Returning, default(ReturningStrategy));
            var callerInfoFoldout = PageFlowCallerInfoSection.Create();

            contentContainer.Add(warnNoSubscriber.WithAlignFieldClass());
            contentContainer.Add(loaderStrategy.WithAlignFieldClass());
            contentContainer.Add(messageScope.WithAlignFieldClass());
            contentContainer.Add(logEnvironment.WithAlignFieldClass());

            contentContainer.Add(poolFoldout);
            poolFoldout.Add(poolRentingStrategy.WithAlignFieldClass());
            poolFoldout.Add(poolReturningStrategy.WithAlignFieldClass());

            contentContainer.Add(callerInfoFoldout);

            callerInfoFoldout.RegisterValueChangedCallback(OnValueChanged);
            warnNoSubscriber.RegisterValueChangedCallback(OnValueChanged);
            loaderStrategy.RegisterValueChangedCallback(OnValueChanged);
            poolRentingStrategy.RegisterValueChangedCallback(OnValueChanged);
            poolReturningStrategy.RegisterValueChangedCallback(OnValueChanged);
            messageScope.RegisterValueChangedCallback(OnValueChanged);
            logEnvironment.RegisterValueChangedCallback(OnValueChanged);

            warnNoSubscriber.WithBindProperty(context.GetWarnNoSubscriber());
            loaderStrategy.WithBindProperty(context.GetLoaderStrategyProperty());
            poolRentingStrategy.WithBindProperty(context.GetPoolRentingStrategyProperty());
            poolReturningStrategy.WithBindProperty(context.GetPoolReturningStrategyProperty());
            messageScope.WithBindProperty(context.GetMessageScopeProperty());
            logEnvironment.WithBindProperty(context.GetLogEnvironmentProperty());

            contentContainer.WithBind(serializedSettings);
        }

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

        [CustomEditor(typeof(UguiPageFlowSettings), true)]
        private sealed class Inspector : UnityEditor.Editor
        {
            private UguiPageFlowSettings _settings;

            private void OnEnable()
            {
                _settings = target as UguiPageFlowSettings;
            }

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
