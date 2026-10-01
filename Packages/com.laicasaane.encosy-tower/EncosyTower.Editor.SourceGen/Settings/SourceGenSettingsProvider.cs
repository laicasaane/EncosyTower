#if UNITY_EDITOR

using EncosyTower.Editor.Settings;
using EncosyTower.Editor.SourceGen.Settings.Views;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.SourceGen
{
    internal static class SourceGenSettingsProvider
    {
        private static SourceGenSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = SourceGenSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = "SourceGen";
            provider.activateHandler = (_, root) => Create(provider, root);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Preferences/SourceGen", priority = 80_00_83_71)]
        [MenuItem("Encosy Tower/SourceGen/Settings", priority = 83_71_00_100)]
        private static void OpenSettings()
        {
            var provider = SourceGenSettings.Instance.GetSettingsProvider(useImgui: false);
            SettingsService.OpenUserPreferences(provider.settingsPath);
        }

        private static void Create(ScriptableObjectSettingsProvider provider, VisualElement root)
        {
            s_instance = new SourceGenSettingsEditor(provider.Settings, provider.SerializedSettings, root);
        }

        private static void Update()
        {
            s_instance?.Update();
        }

        private static void Dispose()
        {
            s_instance?.Save();
            s_instance = null;
        }
    }
}

#endif
