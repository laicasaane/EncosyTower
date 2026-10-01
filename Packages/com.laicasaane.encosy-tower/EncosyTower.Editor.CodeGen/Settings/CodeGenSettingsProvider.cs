#if UNITY_EDITOR

using EncosyTower.Editor.CodeGen.Settings.Views;
using EncosyTower.Editor.Settings;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Registers the CodeGen preferences UI in Unity's Settings window.
    /// </summary>
    internal static class CodeGenSettingsProvider
    {
        private static CodeGenSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = CodeGenSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = "CodeGen";
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Preferences/CodeGen", priority = 80_00_67_71)]
        [MenuItem("Encosy Tower/CodeGen/Settings", priority = 67_71_00_100)]
        private static void OpenSettings()
        {
            var provider = CodeGenSettings.Instance.GetSettingsProvider(useImgui: false);
            SettingsService.OpenUserPreferences(provider.settingsPath);
        }

        private static void Create(ScriptableObjectSettingsProvider provider, VisualElement root)
        {
            s_instance = new CodeGenSettingsEditor(provider.Settings, provider.SerializedSettings, root);
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
