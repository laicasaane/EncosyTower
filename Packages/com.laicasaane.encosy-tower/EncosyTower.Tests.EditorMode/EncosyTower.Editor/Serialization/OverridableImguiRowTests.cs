#if UNITY_EDITOR

using System.Reflection;
using EncosyTower.Editor;
using EncosyTower.Editor.Internals;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Serialization;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableImguiRowTests
    {
        private OverridableTestAsset _asset;
        private OverridableTestAsset _secondAsset;
        private OverridableChoiceTestAsset _choicesAsset;
        private ImguiCustomDrawerAsset _customAsset;
        private SerializedObject _source;
        private SerializedObject _secondSource;
        private SerializedObject _choicesSource;
        private SerializedObject _customSource;
        private int _previousDefault;

        [SetUp]
        public void SetUp()
        {
            OverridableImguiRow.ClearCache();
            _previousDefault = OverridableTestDefaults.IntValue;
            OverridableTestDefaults.IntValue = 3;
            _asset = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _asset.retryCount = new(value: 91, isOverridden: false);
            _asset.intValue = new(value: 17, isOverridden: false);
            _asset.payloadValue = new(new() { number = 9, text = "Stored" }, isOverridden: false);
            _asset.nestedList = new(new(), isOverridden: false);
            _source = new SerializedObject(_asset);
        }

        [TearDown]
        public void TearDown()
        {
            OverridableImguiRow.ClearCache();
            _source?.Dispose();
            _secondSource?.Dispose();
            _choicesSource?.Dispose();
            _customSource?.Dispose();

            if (_asset.IsValid())
            {
                Undo.ClearUndo(_asset);
                Object.DestroyImmediate(_asset);
            }

            if (_secondAsset.IsValid())
            {
                Undo.ClearUndo(_secondAsset);
                Object.DestroyImmediate(_secondAsset);
            }

            if (_choicesAsset.IsValid())
            {
                Undo.ClearUndo(_choicesAsset);
                Object.DestroyImmediate(_choicesAsset);
            }

            if (_customAsset.IsValid())
            {
                Object.DestroyImmediate(_customAsset);
            }

            _customAsset = null;
            _customSource = null;
            _secondAsset = null;
            _secondSource = null;
            _choicesAsset = null;
            _choicesSource = null;
            OverridableTestDefaults.IntValue = _previousDefault;
        }

        [Test]
        public void Cache_ReusesHolderForPropertyCopiesAndSeparatesPaths()
        {
            var property = _source.FindProperty(nameof(OverridableTestAsset.retryCount));
            var field = typeof(OverridableTestAsset).GetField(property.name);
            var row = OverridableImguiRow.Get(property, field);
            var first = row.GetDrawnProperty();
            var repeated = OverridableImguiRow.Get(property.Copy(), field);
            var other = GetRow(nameof(OverridableTestAsset.intValue)).GetDrawnProperty();

            Assert.AreSame(row, repeated);
            Assert.AreSame(
                  first.serializedObject.targetObject
                , repeated.GetDrawnProperty().serializedObject.targetObject
            );
            Assert.AreNotSame(first.serializedObject.targetObject, other.serializedObject.targetObject);
            Assert.AreEqual(expected: 3, actual: first.intValue);
            Assert.AreEqual(expected: 0, actual: other.intValue);
            Assert.AreEqual(expected: 91, actual: _asset.retryCount.value);
            Assert.AreEqual(expected: 17, actual: _asset.intValue.value);
        }

        [Test]
        public void Cache_SeparatesSerializedObjectInstancesForTheSameTarget()
        {
            _secondSource = new SerializedObject(_asset);
            var property = _secondSource.FindProperty(nameof(OverridableTestAsset.retryCount));
            var field = typeof(OverridableTestAsset).GetField(property.name);
            var first = GetRow(property.name).GetDrawnProperty();
            var second = OverridableImguiRow.Get(property, field).GetDrawnProperty();

            Assert.AreNotSame(first.serializedObject.targetObject, second.serializedObject.targetObject);
            Assert.AreEqual(expected: 3, actual: first.intValue);
            Assert.AreEqual(expected: 3, actual: second.intValue);
        }

        [Test]
        public void Default_RefreshesInTheSameHolderWithoutChangingStoredData()
        {
            var row = GetRow(nameof(OverridableTestAsset.retryCount));
            var holder = row.GetDrawnProperty().serializedObject.targetObject;
            OverridableTestDefaults.IntValue = 12;
            var updated = row.GetDrawnProperty();

            Assert.AreSame(holder, updated.serializedObject.targetObject);
            Assert.AreEqual(expected: 12, actual: updated.intValue);
            Assert.AreEqual(expected: 91, actual: _asset.retryCount.value);
            Assert.IsFalse(_asset.retryCount.isOverridden);
        }

        [Test]
        public void ClearCache_DestroysEveryHolderAndRejectsStaleActions()
        {
            var row = GetRow(nameof(OverridableTestAsset.retryCount));
            var first = row.GetDrawnProperty().serializedObject.targetObject;
            var second = GetRow(nameof(OverridableTestAsset.intValue)).GetDrawnProperty().serializedObject.targetObject;
            OverridableImguiRow.ClearCache();

            Assert.IsTrue(first.IsInvalid());
            Assert.IsTrue(second.IsInvalid());
            row.SetOverride(true);
            row.ApplyPreset(27);
            Assert.AreEqual(expected: 91, actual: _asset.retryCount.value);
            Assert.IsFalse(_asset.retryCount.isOverridden);
            var replacement = GetRow(nameof(OverridableTestAsset.retryCount));
            Assert.AreNotSame(row, replacement);
            Assert.IsTrue(replacement.GetDrawnProperty().serializedObject.targetObject.IsValid());
        }

        [Test]
        public void SwitchOn_CopiesTheCurrentDefaultAndUndoRestoresBothMembers()
        {
            var row = GetRow(nameof(OverridableTestAsset.retryCount));
            row.GetDrawnProperty();
            OverridableTestDefaults.IntValue = 12;
            Undo.IncrementCurrentGroup();
            row.SetOverride(true);

            Assert.IsTrue(_asset.retryCount.isOverridden);
            Assert.AreEqual(expected: 12, actual: _asset.retryCount.value);
            Assert.AreSame(_source, row.GetDrawnProperty().serializedObject);
            Undo.PerformUndo();
            _source.Update();
            Assert.IsFalse(_asset.retryCount.isOverridden);
            Assert.AreEqual(expected: 91, actual: _asset.retryCount.value);
            Assert.AreEqual(expected: 12, actual: row.GetDrawnProperty().intValue);
        }

        [Test]
        public void SwitchOff_PreservesStoredValueAndUndoRestoresOverride()
        {
            _asset.retryCount = new(value: 19, isOverridden: true);
            _source.Update();
            var row = GetRow(nameof(OverridableTestAsset.retryCount));
            Undo.IncrementCurrentGroup();
            row.SetOverride(false);

            Assert.IsFalse(_asset.retryCount.isOverridden);
            Assert.AreEqual(expected: 19, actual: _asset.retryCount.value);
            Assert.AreEqual(expected: 3, actual: row.GetDrawnProperty().intValue);
            Undo.PerformUndo();
            _source.Update();
            Assert.IsTrue(_asset.retryCount.isOverridden);
            Assert.AreEqual(expected: 19, actual: _asset.retryCount.value);
        }

        [Test]
        public void Preset_WritesValueAndOverrideInOneUndoStep()
        {
            var row = GetRow(nameof(OverridableTestAsset.retryCount));
            Undo.IncrementCurrentGroup();
            row.ApplyPreset(60);

            Assert.AreEqual(expected: 60, actual: _asset.retryCount.value);
            Assert.IsTrue(_asset.retryCount.isOverridden);
            Undo.PerformUndo();
            _source.Update();
            Assert.AreEqual(expected: 91, actual: _asset.retryCount.value);
            Assert.IsFalse(_asset.retryCount.isOverridden);
        }

        [Test]
        public void MixedOverride_UsesRealMixedPropertyAndSwitchAppliesToBothTargets()
        {
            _secondAsset = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _secondAsset.retryCount = new(value: 24, isOverridden: true);
            _secondSource = new SerializedObject(new Object[] { _asset, _secondAsset });
            var property = _secondSource.FindProperty(nameof(OverridableTestAsset.retryCount));
            var row = OverridableImguiRow.Get(property, typeof(OverridableTestAsset).GetField(property.name));
            var drawn = row.GetDrawnProperty();

            Assert.AreSame(_secondSource, drawn.serializedObject);
            Assert.IsTrue(drawn.hasMultipleDifferentValues);
            Assert.IsFalse(OverridableImguiRow.IsLocked(property));
            row.SetOverride(true);
            Assert.IsTrue(_asset.retryCount.isOverridden);
            Assert.IsTrue(_secondAsset.retryCount.isOverridden);
            Assert.AreEqual(expected: 3, actual: _asset.retryCount.value);
            Assert.AreEqual(expected: 3, actual: _secondAsset.retryCount.value);
        }

        [Test]
        public void NestedDefault_LocksRowsAndUsesEachElementsOwnEffectiveValue()
        {
            var outer = GetRow(nameof(OverridableTestAsset.nestedList));
            var drawn = outer.GetDrawnProperty();
            var field = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.nestedList));
            var first = drawn.GetArrayElementAtIndex(0);
            var second = drawn.GetArrayElementAtIndex(1);
            var firstRow = OverridableImguiRow.Get(first, field);
            var secondRow = OverridableImguiRow.Get(second, field);

            Assert.IsTrue(OverridableImguiRow.IsLocked(first));
            Assert.IsTrue(OverridableImguiRow.IsLocked(second));
            Assert.AreEqual(expected: 0, actual: firstRow.BuildContextMenu(first.displayName).GetItemCount());
            Assert.AreEqual(expected: 0, actual: secondRow.BuildContextMenu(second.displayName).GetItemCount());
            Assert.AreEqual(expected: 2, actual: firstRow.GetDrawnProperty().intValue);
            Assert.AreEqual(expected: 0, actual: secondRow.GetDrawnProperty().intValue);
            firstRow.SetOverride(false);
            secondRow.ApplyPreset(44);
            Assert.IsTrue(first.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.IsFalse(second.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.AreEqual(expected: 9, actual: second.FindPropertyRelative(OverridableEditorAPI.VALUE).intValue);
            Assert.IsEmpty(_asset.nestedList.value);
        }

        [Test]
        public void NestedStoredRow_RejectsActionsWhileOuterDefaultIsActive()
        {
            _asset.nestedList.value.Add(new Overridable<int>(value: 21, isOverridden: true));
            _source.Update();
            var outer = _source.FindProperty(nameof(OverridableTestAsset.nestedList));
            var element = outer.FindPropertyRelative(OverridableEditorAPI.VALUE).GetArrayElementAtIndex(0);
            var field = typeof(OverridableTestAsset).GetField(outer.name);
            var row = OverridableImguiRow.Get(element, field);

            Assert.IsTrue(OverridableImguiRow.IsLocked(element));
            row.SetOverride(false);
            row.ApplyPreset(44);
            Assert.AreEqual(expected: 21, actual: _asset.nestedList.value[0].value);
            Assert.IsTrue(_asset.nestedList.value[0].isOverridden);
            Assert.IsFalse(_asset.nestedList.isOverridden);
        }

        [Test]
        public void NestedOverride_CopiesStatesAndInnerRowsDoNotInheritTheOuterDefault()
        {
            var outer = GetRow(nameof(OverridableTestAsset.nestedList));
            outer.SetOverride(true);
            var drawn = outer.GetDrawnProperty();
            var field = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.nestedList));
            var second = drawn.GetArrayElementAtIndex(1);
            var secondRow = OverridableImguiRow.Get(second, field);

            Assert.IsTrue(_asset.nestedList.value[0].isOverridden);
            Assert.AreEqual(expected: 2, actual: _asset.nestedList.value[0].value);
            Assert.IsFalse(_asset.nestedList.value[1].isOverridden);
            Assert.AreEqual(expected: 9, actual: _asset.nestedList.value[1].value);
            Assert.IsFalse(OverridableImguiRow.IsLocked(second));
            Assert.AreEqual(expected: 0, actual: secondRow.GetDrawnProperty().intValue);
            secondRow.SetOverride(true);
            Assert.AreEqual(expected: 0, actual: _asset.nestedList.value[1].value);
            Assert.IsTrue(_asset.nestedList.value[1].isOverridden);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Class_SelectsDefaultCopyOrStoredProperty(bool isOverridden)
        {
            _asset.payloadValue.isOverridden = isOverridden;
            _source.Update();
            var property = _source.FindProperty(nameof(OverridableTestAsset.payloadValue));
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var drawn = GetRow(property.name).GetDrawnProperty();

            Assert.AreEqual(isOverridden, SerializedProperty.EqualContents(value, drawn));
            Assert.AreEqual(isOverridden == false, drawn.serializedObject.targetObject is OverridableDefaultHolder);
            Assert.AreEqual(isOverridden ? 9 : 0, drawn.FindPropertyRelative("number").intValue);
            Assert.AreEqual(isOverridden ? "Stored" : string.Empty, drawn.FindPropertyRelative("text").stringValue);
            Assert.AreEqual(expected: 9, actual: _asset.payloadValue.value.number);
            Assert.AreEqual("Stored", _asset.payloadValue.value.text);
        }

        [Test]
        public void Collection_UsesDefaultCountUntilOverridden()
        {
            var property = _source.FindProperty(nameof(OverridableTestAsset.nestedList));
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var row = GetRow(property.name);
            var drawn = row.GetDrawnProperty();
            Assert.IsInstanceOf<OverridableDefaultHolder>(drawn.serializedObject.targetObject);
            Assert.AreEqual(expected: 2, actual: drawn.arraySize);

            value.arraySize = 8;
            _source.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(expected: 2, actual: row.GetDrawnProperty().arraySize);
            Assert.AreEqual(expected: 8, actual: _asset.nestedList.value.Count);

            property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue = true;
            _source.ApplyModifiedPropertiesWithoutUndo();
            drawn = row.GetDrawnProperty();
            Assert.IsTrue(SerializedProperty.EqualContents(value, drawn));
            Assert.AreEqual(expected: 8, actual: drawn.arraySize);
        }

        [TestCase(nameof(OverridableTestAsset.nestedList), false)]
        [TestCase(nameof(OverridableTestAsset.nestedList), true)]
        [TestCase(nameof(OverridableTestAsset.arrayValue), false)]
        [TestCase(nameof(OverridableTestAsset.arrayValue), true)]
        public void Collection_SelectsDefaultCopyOrStoredProperty(string fieldName, bool isOverridden)
        {
            _asset.nestedList = new(value: new() { new(value: 7, isOverridden: true) }, isOverridden: isOverridden);
            _asset.arrayValue = new(value: new[] { 8, 9 }, isOverridden: isOverridden);
            _source.Update();
            var property = _source.FindProperty(fieldName);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var drawn = GetRow(fieldName).GetDrawnProperty();

            Assert.AreEqual(isOverridden, SerializedProperty.EqualContents(value, drawn));
            Assert.AreEqual(isOverridden == false, drawn.serializedObject.targetObject is OverridableDefaultHolder);

            if (fieldName == nameof(OverridableTestAsset.nestedList))
            {
                Assert.AreEqual(isOverridden ? 1 : 2, drawn.arraySize);
                var first = drawn.GetArrayElementAtIndex(0).FindPropertyRelative(OverridableEditorAPI.VALUE);
                Assert.AreEqual(isOverridden ? 7 : 2, first.intValue);
            }
            else
            {
                Assert.AreEqual(isOverridden ? 2 : 0, drawn.arraySize);
            }

            Assert.AreEqual(
                  isOverridden
                , property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue
            );

            Assert.AreEqual(expected: 1, actual: _asset.nestedList.value.Count);
            CollectionAssert.AreEqual(new[] { 8, 9 }, _asset.arrayValue.value);
        }

        [TestCase(nameof(OverridableChoiceTestAsset.layerPresets), false, 3)]
        [TestCase(nameof(OverridableChoiceTestAsset.strictLayerPresets), true, 0)]
        public void FlagsChoices_ModeAndMoreMenuRespectExclusivity(string fieldName, bool dropdown, int menuItemCount)
        {
            _choicesAsset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();
            _choicesSource = new SerializedObject(_choicesAsset);
            var property = _choicesSource.FindProperty(fieldName);
            var field = typeof(OverridableChoiceTestAsset).GetField(fieldName);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            var row = OverridableImguiRow.Get(property, field);

            Assert.IsTrue(ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , typeof(TestLayers)
                , out var choices
                , out var error
            ), error.ToMessage());

            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
            var mode = typeof(OverridableImguiRow).GetMethod("UsesDropdown", FLAGS);
            Assert.AreEqual(dropdown, mode.Invoke(row, new object[] { choices, attribute }));
            Assert.IsFalse((bool)mode.Invoke(row, new object[] { null, attribute }));
            var menu = row.BuildChoiceMenu(choices, attribute);
            Assert.AreEqual(menuItemCount, menu.GetItemCount());

            if (dropdown)
            {
                var popupChoices = ValueChoicesPropertyDrawer.GetPopupChoices(
                      attribute
                    , typeof(TestLayers)
                    , choices
                );

                var storedValue = _choicesAsset.strictLayerPresets.value;
                Assert.AreSame(choices, popupChoices);

                Assert.AreEqual(
                      expected: -1
                    , actual: ValueChoicesPropertyDrawer.GetChoiceIndex(popupChoices, storedValue)
                );
            }
        }

        [Test]
        public void FlagsPreset_WritesCombinationAndOverrideWithOneUndoStep()
        {
            _choicesAsset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();
            _choicesAsset.layerPresets = new(value: TestLayers.Water, isOverridden: false);
            _choicesSource = new SerializedObject(_choicesAsset);
            var property = _choicesSource.FindProperty(nameof(OverridableChoiceTestAsset.layerPresets));
            var field = typeof(OverridableChoiceTestAsset).GetField(property.name);
            var row = OverridableImguiRow.Get(property, field);
            var drawn = row.GetDrawnProperty();

            Assert.IsTrue(drawn.serializedObject.targetObject is OverridableDefaultHolder);
            Assert.AreEqual(expected: (int)TestLayers.None, actual: drawn.intValue);
            Undo.IncrementCurrentGroup();
            row.ApplyPreset(TestLayers.Ground | TestLayers.Air);

            Assert.IsTrue(_choicesAsset.layerPresets.isOverridden);
            Assert.AreEqual(TestLayers.Ground | TestLayers.Air, _choicesAsset.layerPresets.value);
            Assert.AreSame(_choicesSource, row.GetDrawnProperty().serializedObject);

            Undo.PerformUndo();
            _choicesSource.Update();

            Assert.IsFalse(_choicesAsset.layerPresets.isOverridden);
            Assert.AreEqual(TestLayers.Water, _choicesAsset.layerPresets.value);
            Assert.AreEqual(expected: (int)TestLayers.None, actual: row.GetDrawnProperty().intValue);
        }

        [TestCase(nameof(ImguiCustomDrawerAsset.layers))]
        [TestCase(nameof(ImguiCustomDrawerAsset.strictLayers))]
        public void PlainFlagsChoices_ResolvePresetsWithoutChangingStoredValue(string fieldName)
        {
            _customAsset = ScriptableObject.CreateInstance<ImguiCustomDrawerAsset>();
            _customSource = new SerializedObject(_customAsset);
            var field = typeof(ImguiCustomDrawerAsset).GetField(fieldName);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            var property = _customSource.FindProperty(fieldName);

            Assert.IsTrue(ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , typeof(TestLayers)
                , out var choices
                , out var error
            ), error.ToMessage());

            Assert.AreEqual(expected: 3, actual: choices.Count);
            Assert.AreEqual(TestLayers.Ground, choices[0].value);
            Assert.AreEqual(TestLayers.Ground | TestLayers.Air, choices[1].value);
            Assert.AreEqual(TestLayers.Water, choices[2].value);
            Assert.AreEqual(expected: (int)TestLayers.Air, actual: property.intValue);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CustomClass_SelectsDefaultCopyOrStoredProperty(bool isOverridden)
        {
            _customAsset = ScriptableObject.CreateInstance<ImguiCustomDrawerAsset>();
            _customAsset.payload = new(value: new() { number = 21 }, isOverridden: isOverridden);
            _customSource = new SerializedObject(_customAsset);
            var property = _customSource.FindProperty(nameof(ImguiCustomDrawerAsset.payload));
            var field = typeof(ImguiCustomDrawerAsset).GetField(property.name);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var drawn = OverridableImguiRow.Get(property, field).GetDrawnProperty();

            Assert.AreEqual(isOverridden, SerializedProperty.EqualContents(value, drawn));
            Assert.AreEqual(isOverridden == false, drawn.serializedObject.targetObject is OverridableDefaultHolder);
            Assert.AreEqual(isOverridden ? 21 : 0, drawn.FindPropertyRelative("number").intValue);
            Assert.AreEqual(isOverridden, _customAsset.payload.isOverridden);
            Assert.AreEqual(expected: 21, actual: _customAsset.payload.value.number);
        }

        private OverridableImguiRow GetRow(string fieldName)
            => OverridableImguiRow.Get(
                  _source.FindProperty(fieldName)
                , typeof(OverridableTestAsset).GetField(fieldName)
            );
    }

    [System.Serializable]
    public sealed class ImguiCustomDrawerPayload
    {
        public int number;
    }

    public sealed class ImguiCustomDrawerAsset : ScriptableObject
    {
        public Overridable<ImguiCustomDrawerPayload> payload;

        [ValueChoices(nameof(LayerChoices))]
        public TestLayers layers = TestLayers.Air;

        [ValueChoices(nameof(LayerChoices), IsExclusive = true)]
        public TestLayers strictLayers = TestLayers.Air;

        private static TestLayers[] LayerChoices
            => new[] { TestLayers.Ground, TestLayers.Ground | TestLayers.Air, TestLayers.Water };
    }

    [CustomPropertyDrawer(typeof(ImguiCustomDrawerPayload))]
    internal sealed class ImguiCustomDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
            => new PropertyField(property.FindPropertyRelative(nameof(ImguiCustomDrawerPayload.number)));

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight * 3f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(
                  position: position
                , property: property.FindPropertyRelative(nameof(ImguiCustomDrawerPayload.number))
                , label: label
            );
        }
    }
}

#endif
