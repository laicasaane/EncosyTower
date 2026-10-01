#if UNITY_EDITOR

using System;
using EncosyTower.Settings;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.SourceGen
{
    [Settings(SettingsUsage.EditorUser, "Encosy Tower/SourceGen")]
    internal sealed class SourceGenSettings : Settings<SourceGenSettings>
    {
        [SerializeField, InspectorName("Retain Output")]
        internal bool _retainOutput;

        internal static string AssetPath
            => $"{SettingsAPI.GetSettingsPath(SettingsUsage.EditorUser)}SourceGenSettings.asset";

        internal static Snapshot ReadCurrent()
            => ReadCurrent(AssetDatabase.LoadAssetAtPath<SourceGenSettings>);

        internal static Snapshot ReadCurrent(Func<string, SourceGenSettings> loader)
        {
            try
            {
                var settings = loader(AssetPath);
                return settings == null
                    ? default
                    : new Snapshot(settings._retainOutput);
            }
            catch (Exception)
            {
                return default;
            }
        }

        internal readonly record struct Snapshot(bool RetainOutput = false);
    }
}

#endif
