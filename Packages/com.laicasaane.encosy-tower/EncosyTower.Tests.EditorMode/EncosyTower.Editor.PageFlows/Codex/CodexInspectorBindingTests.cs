using System;
using System.Collections;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class CodexInspectorBindingTests
    {
        private const int BINDING_FRAMES = 5;

        private GameObject _gameObject;
        private UnityEditor.Editor _editor;
        private BindingTestWindow _window;

        [TearDown]
        public void TearDown()
        {
            if (_window.IsValid())
            {
                _window.Close();
            }

            if (_editor.IsValid())
            {
                UnityEngine.Object.DestroyImmediate(_editor);
            }

            if (_gameObject.IsValid())
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [UnityTest]
        public IEnumerator UitkPageCodexInspector_BindsInEditorPanelWithoutErrors()
            => BindInspector(typeof(UitkPageCodex));

#if UNITY_UGUI
        [UnityTest]
        public IEnumerator UguiPageCodexInspector_BindsInEditorPanelWithoutErrors()
            => BindInspector(typeof(EncosyTower.PageFlows.UguiPages.UguiPageCodex));
#endif

        private IEnumerator BindInspector(Type codexType)
        {
            _gameObject = EditorUtility.CreateGameObjectWithHideFlags(
                  nameof(CodexInspectorBindingTests)
                , HideFlags.HideAndDontSave
                , codexType
            );

            var codex = _gameObject.GetComponent(codexType);

            using (var serializedObject = new SerializedObject(codex))
            {
                var flows = serializedObject.FindProperty("_flows");
                flows.arraySize = 2;
                flows.GetArrayElementAtIndex(0).FindPropertyRelative("identifier").stringValue = "Screen";
                flows.GetArrayElementAtIndex(1).FindPropertyRelative("identifier").stringValue = "Popup";
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            _editor = UnityEditor.Editor.CreateEditor(codex);
            _window = ScriptableObject.CreateInstance<BindingTestWindow>();
            _window.Show();
            _window.rootVisualElement.Add(new InspectorElement(_editor));

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
