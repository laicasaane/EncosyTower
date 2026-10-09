#if UNITY_EDITOR

using System;
using System.IO;
using EncosyTower.Serialization;
using EncosyTower.Settings;
using EncosyTower.UnityExtensions;
using UnityEditor;

namespace EncosyTower.Tests.Editor.Serialization
{
    [Settings(SettingsUsage.EditorProject, "Encosy Tower/Tests/Overridables")]
    public sealed class OverridableTestSettings : Settings<OverridableTestSettings>
    {
        public int maxRetries = 5;
        public Overridable<int>[] limitSteps = { new(value: 2, isOverridden: true), default };
    }

    internal sealed class OverridableTestSettingsScope : IDisposable
    {
        private readonly string _assetPath;
        private readonly string _settingsFolder;
        private readonly string _usageFolder;
        private readonly bool _createdAsset;
        private readonly bool _createdSettingsFolder;
        private readonly bool _createdUsageFolder;
        private readonly int _previousMaxRetries;
        private readonly Overridable<int>[] _previousLimitSteps;

        public OverridableTestSettingsScope()
        {
            var attribute = OverridableTestSettings.Attribute;
            _usageFolder = SettingsAPI.GetSettingsPath(attribute.Usage).TrimEnd('/');
            _settingsFolder = Path.GetDirectoryName(_usageFolder).Replace('\\', '/');
            var filename = attribute.Filename ?? nameof(OverridableTestSettings);
            _assetPath = $"{_usageFolder}/{filename}.asset";
            _createdSettingsFolder = Directory.Exists(_settingsFolder) == false;
            _createdUsageFolder = Directory.Exists(_usageFolder) == false;
            _createdAsset = AssetDatabase.LoadAssetAtPath<OverridableTestSettings>(_assetPath).IsInvalid();
            Settings = OverridableTestSettings.Instance;
            _previousMaxRetries = Settings.maxRetries;
            _previousLimitSteps = Settings.limitSteps;
            Settings.maxRetries = 5;
            Settings.limitSteps = new Overridable<int>[] { new(value: 2, isOverridden: true), default };
        }

        public OverridableTestSettings Settings { get; }

        public void Dispose()
        {
            if (Settings.IsValid())
            {
                Settings.maxRetries = _previousMaxRetries;
                Settings.limitSteps = _previousLimitSteps;
            }

            if (_createdAsset)
            {
                AssetDatabase.DeleteAsset(_assetPath);
            }

            if (_createdUsageFolder
                && Directory.Exists(_usageFolder)
                && Directory.GetFileSystemEntries(_usageFolder).Length == 0
            )
            {
                AssetDatabase.DeleteAsset(_usageFolder);
            }

            if (_createdSettingsFolder
                && Directory.Exists(_settingsFolder)
                && Directory.GetFileSystemEntries(_settingsFolder).Length == 0
            )
            {
                AssetDatabase.DeleteAsset(_settingsFolder);
            }
        }
    }
}

#endif
