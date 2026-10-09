#if UNITY_EDITOR

using EncosyTower.Editor.Settings;
using EncosyTower.Editor.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.DevTools.Showcases.Overridables
{
    /// <summary>
    /// Shows <see cref="OverridableShowcaseSettings"/> under Project Settings > Encosy Tower > Overridable Showcase.
    /// </summary>
    internal static class OverridableShowcaseSettingsProvider
    {
        private const string LABEL = "Overridables";
        private const string SCRIPT_PROPERTY = "m_Script";
        private const string TITLE_BAR_USS_CLASS_NAME = "project-settings-title-bar";
        private const string TITLE_LABEL_USS_CLASS_NAME = "project-settings-title-bar__label";

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = OverridableShowcaseSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = LABEL;
            provider.activateHandler = (_, root) => Create(provider, root);
            return provider;
        }

        [MenuItem("Dev Tools/Showcases/Overridables Settings", priority = 90_77_00_01)]
        private static void OpenSettings()
            => OverridableShowcaseSettings.Instance.OpenSettingsWindow();

        private static void Create(ScriptableObjectSettingsProvider provider, VisualElement root)
        {
            root.WithEditorBuiltInStyleSheet(EditorStyleSheetPaths.PROJECT_SETTINGS_STYLE_SHEET);

            var titleBar = new VisualElement();
            titleBar.AddToClassList(TITLE_BAR_USS_CLASS_NAME);
            root.Add(titleBar);

            var titleLabel = new Label(LABEL);
            titleLabel.AddToClassList(TITLE_LABEL_USS_CLASS_NAME);
            titleBar.Add(titleLabel);

            var serializedSettings = provider.SerializedSettings;
            var property = serializedSettings.GetIterator();

            if (property.NextVisible(true))
            {
                do
                {
                    if (property.propertyPath == SCRIPT_PROPERTY)
                    {
                        continue;
                    }

                    root.Add(new PropertyField(property.Copy()));
                }
                while (property.NextVisible(false));
            }

            root.Bind(serializedSettings);
        }
    }
}

#endif
