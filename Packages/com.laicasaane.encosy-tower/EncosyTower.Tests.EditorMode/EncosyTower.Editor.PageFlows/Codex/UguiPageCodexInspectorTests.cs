#if UNITY_UGUI

using EncosyTower.Editor.PageFlows;
using EncosyTower.Editor.PageFlows.UguiPages;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.UguiPages.Codex;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Tests.Editor.PageFlows
{
    public sealed class UguiPageCodexInspectorTests
    {
        private const string FLOWS = "_flows";
        private const string IDENTIFIER = "identifier";
        private const string KIND = "kind";
        private const string OVERRIDE_SORTING_LAYER = "overrideSortingLayer";
        private const string SORTING_LAYER = "sortingLayer";
        private const string SORTING_LAYER_VALUE = "value";
        private const string SORTING_ORDER_IN_LAYER = "sortingOrderInLayer";

        private GameObject _gameObject;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject.IsValid())
            {
                UnityEngine.Object.DestroyImmediate(_gameObject);
            }
        }

        [Test]
        public void Sync_AppendedDefinitionIsFresh()
        {
            _gameObject = EditorUtility.CreateGameObjectWithHideFlags(
                  nameof(UguiPageCodexInspectorTests)
                , HideFlags.HideAndDontSave
                , typeof(UguiPageCodex)
            );

            using var serializedObject = new SerializedObject(_gameObject.GetComponent<UguiPageCodex>());
            var flows = serializedObject.FindProperty(FLOWS);
            flows.arraySize = 1;

            var existing = flows.GetArrayElementAtIndex(0);
            existing.FindPropertyRelative(IDENTIFIER).stringValue = "Screen";
            existing.FindPropertyRelative(KIND).enumValueIndex = (int)PageFlowKind.PageStackMulti;
            existing.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue = true;
            existing.FindPropertyRelative(SORTING_LAYER).FindPropertyRelative(SORTING_LAYER_VALUE).intValue = 7;
            existing.FindPropertyRelative(SORTING_ORDER_IN_LAYER).intValue = 5;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var scopes = new[] { "Screen", "Popup" };
            PageFlowScopeCollectionInfo info = new PageFlowScopeCollectionInfo.Found("TestScopes", scopes);

            FlowDefinitionListModel.Create(flows, info).Sync(flows, UguiPageCodexInspector.ResetNewDefinition);
            serializedObject.Update();

            Assert.AreEqual(2, flows.arraySize);

            var first = flows.GetArrayElementAtIndex(0);
            Assert.AreEqual("Screen", first.FindPropertyRelative(IDENTIFIER).stringValue);
            Assert.AreEqual((int)PageFlowKind.PageStackMulti, first.FindPropertyRelative(KIND).enumValueIndex);
            Assert.IsTrue(first.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue);
            Assert.AreEqual(
                  expected: 7
                , actual: first.FindPropertyRelative(SORTING_LAYER).FindPropertyRelative(SORTING_LAYER_VALUE).intValue
            );
            Assert.AreEqual(5, first.FindPropertyRelative(SORTING_ORDER_IN_LAYER).intValue);

            var second = flows.GetArrayElementAtIndex(1);
            Assert.AreEqual("Popup", second.FindPropertyRelative(IDENTIFIER).stringValue);
            Assert.AreEqual((int)PageFlowKind.PageStackSingle, second.FindPropertyRelative(KIND).enumValueIndex);
            Assert.IsFalse(second.FindPropertyRelative(OVERRIDE_SORTING_LAYER).boolValue);
            Assert.AreEqual(
                  expected: 0
                , actual: second.FindPropertyRelative(SORTING_LAYER).FindPropertyRelative(SORTING_LAYER_VALUE).intValue
            );
            Assert.AreEqual(0, second.FindPropertyRelative(SORTING_ORDER_IN_LAYER).intValue);
        }
    }
}

#endif
