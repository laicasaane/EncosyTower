#if UNITY_EDITOR

using System.IO;
using System.Reflection;
using EncosyTower.Editor.Settings;
using EncosyTower.Editor.SourceGen;
using EncosyTower.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.SourceGen
{
    [Category("Editor.SourceGen")]
    public sealed class SourceGenSettingsTests
    {
        [Test]
        public void ReadCurrent_DefaultsFalseAndReturnsStoredValue()
        {
            var settings = ScriptableObject.CreateInstance<SourceGenSettings>();

            try
            {
                var initial = SourceGenSettings.ReadCurrent(_ => settings);
                settings._retainOutput = true;
                var stored = SourceGenSettings.ReadCurrent(_ => settings);
                var missing = SourceGenSettings.ReadCurrent(static _ => null);
                var unreadable = SourceGenSettings.ReadCurrent(
                    static _ => throw new IOException("Unreadable test asset.")
                );

                Assert.That(SourceGenSettings.AssetPath, Is.EqualTo(
                    $"{SettingsAPI.GetSettingsPath(SettingsUsage.EditorUser)}SourceGenSettings.asset"
                ));
                Assert.That(initial.RetainOutput, Is.False);
                Assert.That(stored.RetainOutput, Is.True);
                Assert.That(missing.RetainOutput, Is.False);
                Assert.That(unreadable.RetainOutput, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void Provider_CreatesSourceGenPageWithSingleRetainOutputToggle()
        {
            var factory = typeof(SourceGenSettingsProvider).GetMethod(
                  "GetSettingsProvider"
                , BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.That(factory, Is.Not.Null);

            var provider = factory.Invoke(null, null) as ScriptableObjectSettingsProvider;

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider.settingsPath, Is.EqualTo("Preferences/Encosy Tower/SourceGen"));
            Assert.That(provider.label, Is.EqualTo("SourceGen"));
            Assert.That(provider.Settings, Is.SameAs(SourceGenSettings.Instance));

            var settings = SourceGenSettings.Instance;
            var retainOutput = settings._retainOutput;
            var serializedSettings = provider.SerializedSettings;
            var retainOutputProperty = serializedSettings.FindProperty(nameof(SourceGenSettings._retainOutput));
            var window = ScriptableObject.CreateInstance<SettingsHostWindow>();
            window.position = new Rect(-10000f, -10000f, 1f, 1f);
            window.Show();
            var root = window.rootVisualElement;
            var initialStyleSheetCount = root.styleSheets.count;

            try
            {
                provider.activateHandler(string.Empty, root);

                var container = root.Q<ScrollView>();
                var toggles = root.Query<Toggle>().ToList();

                Assert.That(root.styleSheets.count, Is.EqualTo(initialStyleSheetCount + 2));
                Assert.That(container, Is.Not.Null);
                Assert.That(container.ClassListContains("source-gen"), Is.True);
                Assert.That(toggles, Has.Count.EqualTo(1));
                Assert.That(toggles[0].label, Is.EqualTo("Retain Output"));
                Assert.That(toggles[0].bindingPath, Is.EqualTo(nameof(SourceGenSettings._retainOutput)));

                SetValueAndNotify(toggles[0], true);
                provider.inspectorUpdateHandler();

                Assert.That(settings._retainOutput, Is.True);
            }
            finally
            {
                retainOutputProperty.boolValue = retainOutput;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                provider.inspectorUpdateHandler();
                provider.deactivateHandler();
                window.Close();
            }

            Assert.That(settings._retainOutput, Is.EqualTo(retainOutput));
        }

        private static void SetValueAndNotify(Toggle toggle, bool value)
        {
            var previousValue = toggle.value;
            toggle.SetValueWithoutNotify(value);

            using var evt = ChangeEvent<bool>.GetPooled(previousValue, value);
            evt.target = toggle;
            toggle.SendEvent(evt);
        }

        private sealed class SettingsHostWindow : EditorWindow
        {
        }
    }
}

#endif
