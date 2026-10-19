#if UNITY_EDITOR && UNITY_UGUI

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using EncosyTower.Editor.PageFlows;
using EncosyTower.Editor.PageFlows.UguiPages.Settings.Views;
using EncosyTower.PageFlows;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.PageFlows.UguiPages.Settings
{
    public sealed class UguiPageFlowSettingsEditorTests
    {
        private const string TYPE_NAME =
            "EncosyTower.Editor.PageFlows.UguiPages.Settings.Views.UguiPageFlowSettingsEditor";

        private const int BINDING_FRAMES = 5;

        private SettingsTestWindow _window;
        private SerializedObject _serializedSettings;
        private UguiPageFlowSettings _settings;

        [TearDown]
        public void TearDown()
        {
            if (_window.IsValid())
            {
                _window.Close();
            }

            _serializedSettings?.Dispose();

            if (_settings.IsValid())
            {
                UnityEngine.Object.DestroyImmediate(_settings);
            }
        }

        [UnityTest]
        public IEnumerator CallerInfoFoldout_ToggleDoesNotMarkSettingsChanged()
        {
            _settings = ScriptableObject.CreateInstance<UguiPageFlowSettings>();
            _serializedSettings = new SerializedObject(_settings);
            _window = ScriptableObject.CreateInstance<SettingsTestWindow>();
            _window.Show();

            var root = new VisualElement();
            _window.rootVisualElement.Add(root);

            var editor = new UguiPageFlowSettingsEditor(_serializedSettings, root);

            for (var i = 0; i < BINDING_FRAMES; i++)
            {
                _window.Repaint();
                yield return null;
            }

            var valueUpdated = typeof(UguiPageFlowSettingsEditor)
                .GetField("_valueUpdated", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(valueUpdated, Is.Not.Null);
            valueUpdated.SetValue(editor, false);

            var texts = PageFlowsViewResources.Get();
            var heading = texts.CallerInfo.Heading;
            var warnNoSubscriber = texts.Settings.WarnNoSubscriber;
            var foldout = root.Query<Foldout>().Where(candidate => candidate.text == heading).First();
            var toggle = root.Query<Toggle>().Where(candidate => candidate.label == warnNoSubscriber).First();

            Assert.That(foldout, Is.Not.Null);
            Assert.That(toggle, Is.Not.Null);

            foldout.value = foldout.value == false;

            yield return null;

            Assert.IsFalse((bool)valueUpdated.GetValue(editor));

            toggle.value = toggle.value == false;

            yield return null;

            Assert.IsTrue((bool)valueUpdated.GetValue(editor));
        }

        [Test]
        public void EnumValueChange_NullEventLeavesStateUnchanged()
        {
            var fixture = new EditorFixture();

            fixture.Invoke(evt: null);

            Assert.IsFalse(fixture.ValueUpdated);
        }

        [Test]
        public void EnumValueChange_EqualValuesLeaveStateUnchanged()
        {
            var fixture = new EditorFixture();
            using var evt = ChangeEvent<Enum>.GetPooled(TestEnum.First, TestEnum.First);

            fixture.Invoke(evt);

            Assert.IsFalse(fixture.ValueUpdated);
        }

        [TestCase(null, TestEnum.First, true)]
        [TestCase(TestEnum.First, null, true)]
        [TestCase(TestEnum.First, TestEnum.Second, true)]
        public void EnumValueChange_DifferentValuesMarkStateUpdated(Enum previous, Enum current, bool expected)
        {
            var fixture = new EditorFixture();
            using var evt = ChangeEvent<Enum>.GetPooled(previous, current);

            fixture.Invoke(evt);

            Assert.AreEqual(expected, fixture.ValueUpdated);
        }

        private enum TestEnum
        {
            First,
            Second,
        }

        private sealed class SettingsTestWindow : EditorWindow
        {
        }

        private sealed class EditorFixture
        {
            private readonly object _editor;
            private readonly FieldInfo _valueUpdated;

            public EditorFixture()
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(static candidate => candidate.GetName().Name == "EncosyTower.Editor.PageFlows");
                var type = assembly.GetType(TYPE_NAME, throwOnError: true);
                _editor = FormatterServices.GetUninitializedObject(type);
                _valueUpdated = type.GetField("_valueUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
                Method = type.GetMethod(
                      "OnValueChanged"
                    , BindingFlags.Instance | BindingFlags.NonPublic
                    , binder: null
                    , new[] { typeof(ChangeEvent<Enum>) }
                    , modifiers: null
                );

                Assert.That(_valueUpdated, Is.Not.Null);
                Assert.That(Method, Is.Not.Null);
            }

            public MethodInfo Method { get; }
            public bool ValueUpdated => (bool)_valueUpdated.GetValue(_editor);

            public void Invoke(ChangeEvent<Enum> evt)
            {
                Method.Invoke(_editor, new object[] { evt });
            }
        }
    }
}

#endif
