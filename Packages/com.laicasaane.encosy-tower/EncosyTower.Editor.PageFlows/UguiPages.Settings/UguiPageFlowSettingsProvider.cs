#if UNITY_EDITOR && UNITY_UGUI

using EncosyTower.Editor.PageFlows.UguiPages.Settings.Views;
using EncosyTower.Editor.Settings;
using EncosyTower.PageFlows;
using UnityEditor;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UguiPages.Settings
{
    /// <summary>
    /// Registers the Page Flows uGUI page in Project Settings.
    /// </summary>
    /// <remarks>
    /// Wires a <see cref="ScriptableObjectSettingsProvider"/> to <see cref="UguiPageFlowSettingsEditor"/>.
    /// Activating the page creates the editor, each inspector update calls
    /// <see cref="UguiPageFlowSettingsEditor.Update"/>, and deactivating the page drops the editor. The menu item
    /// <c>Encosy Tower/Project Settings/Page Flows/uGUI</c> opens the page.
    /// </remarks>
    /// <seealso cref="UguiPageFlowSettings"/>
    internal static class UguiPageFlowSettingsProvider
    {
        private static UguiPageFlowSettingsEditor s_instance;

        /// <summary>
        /// Creates the settings provider that Unity discovers through <see cref="SettingsProviderAttribute"/>.
        /// </summary>
        /// <returns>The provider of the uGUI page.</returns>
        [SettingsProvider, Preserve]
        private static SettingsProvider GetSettingsProvider()
        {
            var provider = UguiPageFlowSettings.Instance.GetSettingsProvider(useImgui: false);
            provider.label = PageFlowsViewResources.Get().Settings.UguiTitle;
            provider.activateHandler = (_, r) => Create(provider, r);
            provider.inspectorUpdateHandler = Update;
            provider.deactivateHandler = Dispose;

            return provider;
        }

        [MenuItem("Encosy Tower/Project Settings/Page Flows/uGUI", priority = 80_77_00_00)]
        private static void OpenSettings()
            => UguiPageFlowSettings.Instance.OpenSettingsWindow();

        private static void Create(ScriptableObjectSettingsProvider provider, VisualElement root)
        {
            s_instance = new UguiPageFlowSettingsEditor(provider.SerializedSettings, root);
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
