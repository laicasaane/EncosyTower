#if NEEDLE_CONSOLE

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.DevTools;

[InitializeOnLoad]
internal static class DevToolsForNeedleConsole
{
    private const string SETTINGS_TYPE = "Needle.Console.NeedleConsoleSettings, Needle.Console.Editor";
    private const string INSTANCE_PROPERTY = "instance";

    static DevToolsForNeedleConsole()
    {
        if (TryGetSettings(out var settings))
        {
            settings.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    private static bool TryGetSettings(out UnityEngine.Object settings)
    {
        settings = null;

        var type = Type.GetType(SETTINGS_TYPE, throwOnError: false);
        var property = type?.GetProperty(
              INSTANCE_PROPERTY
            , BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy
        );

        if (property?.GetValue(null) is not UnityEngine.Object instance || instance == false)
        {
            return false;
        }

        settings = instance;
        return true;
    }
}

#endif
