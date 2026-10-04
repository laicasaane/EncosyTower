#if UNITY_EDITOR

using EncosyTower.Editor.PageFlows.UitkPages.Settings.Views;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows.UitkPages;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages.Settings
{
    internal static class UitkPageFlowSettingsProvider
    {
        private static UitkPageFlowSettingsEditor s_instance;

        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = UitkPageFlowSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = "UI Toolkit Page Flow";
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Project Settings/UI Toolkit Page Flow", priority = 80_77_00_01)]
        private static void OpenSettings()
            => UitkPageFlowSettings.Instance.OpenSettingsWindow();

        private static void Create(ScriptableObjectSettingsProvider provider, VisualElement root)
        {
            s_instance = new UitkPageFlowSettingsEditor(provider.SerializedSettings, root);
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
