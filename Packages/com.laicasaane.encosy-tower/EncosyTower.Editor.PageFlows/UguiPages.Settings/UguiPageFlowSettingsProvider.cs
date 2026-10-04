#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.PageFlows.UguiPages.Settings.Views;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows.UguiPages;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings
{
    internal static class UguiPageFlowSettingsProvider
    {
        private static UguiPageFlowSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = UguiPageFlowSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = "uGUI Page Flow";
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Project Settings/uGUI Page Flow", priority = 80_77_00_00)]
        private static void OpenSettings()
            => UguiPageFlowSettings.Instance.OpenSettingsWindow();

        private static void Create(ScriptableObjectSettingsProvider provider, VisualElement root)
        {
            s_instance = new UguiPageFlowSettingsEditor(provider.Settings, provider.SerializedSettings, root);
        }

        private static void Update()
        {
            s_instance?.Update();
        }

        private static void Dispose()
        {
            s_instance = null;
        }
    }
}

#endif
