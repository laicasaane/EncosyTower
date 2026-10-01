#if UNITY_EDITOR

using System.IO;
using System.Reflection;
using EncosyTower.Editor.CodeGen;
using EncosyTower.Editor.Settings;
using EncosyTower.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.CodeGen
{
    [Category("Editor.CodeGen")]
    public sealed class SettingsTests
    {
        [Test]
        public void Snapshot_RetainDotnetSolutions_IsIndependentFromAutomaticDotnetBackend()
        {
            var settings = ScriptableObject.CreateInstance<CodeGenSettings>();

            try
            {
                settings._retainDotnetSolutions = true;
                var unityBackendSnapshot = CodeGenSettings.ReadCurrent(_ => settings);

                settings._runGeneratorsInDotnetSolution = true;
                var dotnetBackendSnapshot = CodeGenSettings.ReadCurrent(_ => settings);

                Assert.That(unityBackendSnapshot.AutomaticallyGenerateCode, Is.False);
                Assert.That(unityBackendSnapshot.RunGeneratorsInDotnetSolution, Is.False);
                Assert.That(unityBackendSnapshot.RetainDotnetSolutions, Is.True);
                Assert.That(dotnetBackendSnapshot.RetainDotnetSolutions, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void DirectRead_MissingOrUnreadableAsset_ReturnsFalseDefaults()
        {
            string loadedPath = null;
            var missing = CodeGenSettings.ReadCurrent(path =>
            {
                loadedPath = path;
                return null;
            });
            var unreadable = CodeGenSettings.ReadCurrent(
                static _ => throw new IOException("Unreadable test asset.")
            );

            Assert.That(loadedPath, Is.EqualTo(CodeGenSettings.AssetPath));
            Assert.That(
                  CodeGenSettings.AssetPath
                , Is.EqualTo($"{SettingsAPI.GetSettingsPath(SettingsUsage.EditorUser)}CodeGenSettings.asset")
            );
            Assert.That(missing.AutomaticallyGenerateCode, Is.False);
            Assert.That(missing.RunGeneratorsInDotnetSolution, Is.False);
            Assert.That(missing.RetainDotnetSolutions, Is.False);
            Assert.That(unreadable.AutomaticallyGenerateCode, Is.False);
            Assert.That(unreadable.RunGeneratorsInDotnetSolution, Is.False);
            Assert.That(unreadable.RetainDotnetSolutions, Is.False);
        }

        [Test]
        public void Provider_UsesSharedUiToolkitLifecycleAndExactPreferencesPage()
        {
            var factory = typeof(CodeGenSettingsProvider).GetMethod(
                  "GetSettingsProvider"
                , BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.That(factory, Is.Not.Null);

            var provider = factory.Invoke(null, null) as ScriptableObjectSettingsProvider;

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider.settingsPath, Is.EqualTo("Preferences/Encosy Tower/CodeGen"));
            Assert.That(provider.label, Is.EqualTo("CodeGen"));
            Assert.That(provider.Settings, Is.SameAs(CodeGenSettings.Instance));
            Assert.That(provider.activateHandler, Is.Not.Null);
            Assert.That(provider.inspectorUpdateHandler, Is.Not.Null);
            Assert.That(provider.deactivateHandler, Is.Not.Null);

            var settings = CodeGenSettings.Instance;
            var automaticallyGenerateCode = settings._automaticallyGenerateCode;
            var runGeneratorsInDotnetSolution = settings._runGeneratorsInDotnetSolution;
            var retainDotnetSolutions = settings._retainDotnetSolutions;
            var serializedSettings = provider.SerializedSettings;
            var automaticallyGenerateCodeProperty
                = serializedSettings.FindProperty(nameof(CodeGenSettings._automaticallyGenerateCode));
            var runGeneratorsInDotnetSolutionProperty
                = serializedSettings.FindProperty(nameof(CodeGenSettings._runGeneratorsInDotnetSolution));
            var retainDotnetSolutionsProperty
                = serializedSettings.FindProperty(nameof(CodeGenSettings._retainDotnetSolutions));
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
                Assert.That(container.ClassListContains("code-gen"), Is.True);
                Assert.That(toggles, Has.Count.EqualTo(3));
                Assert.That(toggles[0].label, Is.EqualTo("Automatically Generate Code"));
                Assert.That(
                      toggles[0].bindingPath
                    , Is.EqualTo(nameof(CodeGenSettings._automaticallyGenerateCode))
                );
                Assert.That(toggles[1].label, Is.EqualTo("Run Generators in .NET Solution"));
                Assert.That(
                      toggles[1].bindingPath
                    , Is.EqualTo(nameof(CodeGenSettings._runGeneratorsInDotnetSolution))
                );
                Assert.That(toggles[2].label, Is.EqualTo("Retain .NET Solutions"));
                Assert.That(toggles[2].bindingPath, Is.EqualTo(nameof(CodeGenSettings._retainDotnetSolutions)));
                Assert.That(toggles[2].enabledSelf, Is.True);

                SetValueAndNotify(toggles[1], false);
                SetValueAndNotify(toggles[2], true);

                Assert.That(toggles[2].enabledSelf, Is.True);
                Assert.That(toggles[2].value, Is.True);
            }
            finally
            {
                automaticallyGenerateCodeProperty.boolValue = automaticallyGenerateCode;
                runGeneratorsInDotnetSolutionProperty.boolValue = runGeneratorsInDotnetSolution;
                retainDotnetSolutionsProperty.boolValue = retainDotnetSolutions;
                serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                provider.inspectorUpdateHandler();
                provider.deactivateHandler();
                window.Close();
            }

            Assert.That(settings._automaticallyGenerateCode, Is.EqualTo(automaticallyGenerateCode));
            Assert.That(settings._runGeneratorsInDotnetSolution, Is.EqualTo(runGeneratorsInDotnetSolution));
            Assert.That(settings._retainDotnetSolutions, Is.EqualTo(retainDotnetSolutions));
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
