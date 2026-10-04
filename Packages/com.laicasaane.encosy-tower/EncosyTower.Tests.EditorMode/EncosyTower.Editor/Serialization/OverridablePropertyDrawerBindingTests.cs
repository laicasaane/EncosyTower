using System.Collections;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridablePropertyDrawerBindingTests
    {
        private const int BINDING_FRAMES = 5;

        private OverridableSettingsTestAsset _asset;
        private SerializedObject _serializedObject;
        private BindingTestWindow _window;

        [TearDown]
        public void TearDown()
        {
            if (_window.IsValid())
            {
                _window.Close();
            }

            _serializedObject?.Dispose();
            Object.DestroyImmediate(_asset);
        }

        [UnityTest]
        public IEnumerator BindInEditorPanel_WithSettingsDefault_DoesNotThrow()
        {
            _asset = ScriptableObject.CreateInstance<OverridableSettingsTestAsset>();
            _serializedObject = new SerializedObject(_asset);
            _window = ScriptableObject.CreateInstance<BindingTestWindow>();
            _window.Show();

            var property = _serializedObject.FindProperty(nameof(OverridableSettingsTestAsset.warnNoSubscriber));
            var root = _window.rootVisualElement;

            root.Add(new PropertyField(property));
            root.Bind(_serializedObject);

            for (var i = 0; i < BINDING_FRAMES; i++)
            {
                _window.Repaint();
                yield return null;
            }

            LogAssert.NoUnexpectedReceived();
        }

        private sealed class BindingTestWindow : EditorWindow
        {
        }
    }
}
