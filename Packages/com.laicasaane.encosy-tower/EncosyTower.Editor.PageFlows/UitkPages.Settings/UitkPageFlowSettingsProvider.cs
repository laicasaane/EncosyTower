#if UNITY_EDITOR

using EncosyTower.Editor.PageFlows.UitkPages.Settings.Views;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages.Settings
{
    /// <summary>
    /// Registers the Page Flows UI Toolkit page in Project Settings.
    /// </summary>
    /// <remarks>
    /// Wires a <see cref="ScriptableObjectSettingsProvider"/> to <see cref="UitkPageFlowSettingsEditor"/>.
    /// Activating the page creates the editor, each inspector update calls
    /// <see cref="UitkPageFlowSettingsEditor.Update"/>, and deactivating the page drops the editor. The menu item
    /// <c>Encosy Tower/Project Settings/Page Flows/UI Toolkit</c> opens the page.
    /// </remarks>
    /// <seealso cref="UitkPageFlowSettings"/>
    internal static class UitkPageFlowSettingsProvider
    {
        private static UitkPageFlowSettingsEditor s_instance;

        /// <summary>
        /// Creates the settings provider that Unity discovers through <see cref="SettingsProviderAttribute"/>.
        /// </summary>
        /// <returns>The provider of the UI Toolkit page.</returns>
        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = UitkPageFlowSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = PageFlowsViewResources.Get().Settings.UitkTitle;
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Project Settings/Page Flows/UI Toolkit", priority = 80_77_00_01)]
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
