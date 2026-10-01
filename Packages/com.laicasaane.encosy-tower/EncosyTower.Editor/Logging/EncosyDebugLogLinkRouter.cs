#if UNITY_EDITOR

using System;
using UnityEditor;

namespace EncosyTower.Editor.Logging
{
    [InitializeOnLoad]
    internal static class EncosyDebugLogLinkRouter
    {
        static EncosyDebugLogLinkRouter()
        {
            EditorGUI.hyperLinkClicked -= OnDebugLogLinkClicked;
            EditorGUI.hyperLinkClicked += OnDebugLogLinkClicked;
        }

        private static void OnDebugLogLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            if (args.hyperLinkData.TryGetValue("href", out string href) == false
                || args.hyperLinkData.TryGetValue("router", out string router) == false
                || string.Equals(router, "encosy-tower", StringComparison.OrdinalIgnoreCase) == false
                || href.StartsWith("\\", StringComparison.OrdinalIgnoreCase) == false
            )
            {
                return;
            }

            // Route: Project Settings sub-pages (e.g., "Project/Player")
            if (href.StartsWith("\\open:Project/", StringComparison.Ordinal))
            {
                var path = href[6..];
                SettingsService.OpenProjectSettings(path);
            }
            // Route: User Preferences sub-pages (e.g., "Preferences/External Tools")
            else if (href.StartsWith("\\open:Preferences/", StringComparison.Ordinal))
            {
                var path = href[6..];
                SettingsService.OpenUserPreferences(path);
            }
            // Route: [MenuItem] (e.g., "Encosy Tower/Project Settings/Features")
            else if (href.StartsWith("\\menu:", StringComparison.Ordinal))
            {
                var path = href[6..];
                var success = EditorApplication.ExecuteMenuItem(path);

                if (success == false)
                {
                    ThrowHelper.LogWarningUndefinedMenuPath(path);
                }
            }
            else
            {
                ThrowHelper.LogWarningUnsupportedRoute(href);
            }
        }

    }
}

#endif
