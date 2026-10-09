#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Common;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Serialization;
using EncosyTower.UIElements;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridablePropertyDrawerBindingTests
    {
        private static readonly List<int> s_collectionDefault = new();
        private static readonly OverridableTestPayload s_payloadDefault = new();

        private OverridableTestAsset _asset;
        private OverridableTestAsset _secondAsset;
        private OverridableSettingsTestAsset _settingsAsset;
        private OverridableDefaultHolder _outerHolder;
        private SerializedObject _serializedObject;
        private SerializedObject _settingsObject;
        private BindingTestWindow _window;
        private OverridableRowBuilder _builder;
        private VisualElement _row;
        private int _previousDefault;

        public static List<int> CollectionDefault => s_collectionDefault;

        public static OverridableTestPayload PayloadDefault => s_payloadDefault;

        private static OverridableDefaultHolder GetNewHolder(OverridableDefaultHolder[] previousHolders)
        {
            var holders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
            var count = holders.Length;

            for (var i = 0; i < count; i++)
            {
                var holder = holders[i];

                if (System.Array.IndexOf(previousHolders, holder) < 0)
                {
                    return holder;
                }
            }

            Assert.Fail("The default row did not create a holder.");
            return null;
        }

        private static OverridableDefaultAttribute GetDefaultAttribute(string fieldName)
            => typeof(OverridableTestAsset).GetField(fieldName).GetCustomAttribute<OverridableDefaultAttribute>();

        [SetUp]
        public void SetUp()
        {
            s_collectionDefault.Clear();
            s_payloadDefault.number = 7;
            s_payloadDefault.text = "Default";
            _previousDefault = OverridableTestDefaults.IntValue;
            OverridableTestDefaults.IntValue = 3;
            _asset = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _asset.retryCount = new(value: 9, isOverridden: false);
            _serializedObject = new SerializedObject(_asset);
            _window = ScriptableObject.CreateInstance<BindingTestWindow>();
            _window.Show();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window.IsValid())
            {
                _window.Close();
            }

            _serializedObject?.Dispose();
            _settingsObject?.Dispose();
            Object.DestroyImmediate(_asset);
            Object.DestroyImmediate(_secondAsset);
            Object.DestroyImmediate(_settingsAsset);
            Object.DestroyImmediate(_outerHolder);
            s_collectionDefault.Clear();
            OverridableTestDefaults.IntValue = _previousDefault;
        }

        [Test]
        public void BindInEditorPanel_WithSettingsDefault_DoesNotThrow()
        {
            _settingsAsset = ScriptableObject.CreateInstance<OverridableSettingsTestAsset>();
            _settingsObject = new SerializedObject(_settingsAsset);
            var property = _settingsObject.FindProperty(nameof(OverridableSettingsTestAsset.warnNoSubscriber));
            var root = _window.rootVisualElement;
            root.Add(new PropertyField(property));

            Assert.DoesNotThrow(BindSettings);
            Assert.IsNotNull(root.Q<OverridableSwitch>());
            LogAssert.NoUnexpectedReceived();

            void BindSettings()
            {
                root.Bind(_settingsObject);
            }
        }

        [TestCase(nameof(OverridableChoiceTestAsset.limitSteps))]
        [TestCase(nameof(OverridableChoiceTestAsset.spawnWaves))]
        public void Bind_CollectionsBuildWithoutErrors(string fieldName)
        {
            using var settingsScope = fieldName == nameof(OverridableChoiceTestAsset.limitSteps)
                ? new OverridableTestSettingsScope()
                : null;

            var asset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();

            try
            {
                using var serializedObject = new SerializedObject(asset);
                var property = serializedObject.FindProperty(fieldName);
                Assert.IsNotNull(property);

                var root = _window.rootVisualElement;
                var field = new PropertyField(property);
                root.Add(field);

                try
                {
                    Assert.DoesNotThrow(BindCollections);

                    var toggle = field.Q<OverridableSwitch>();
                    var size = field.Q<IntegerField>(className: "encosy-overridable__collection-size");

                    Assert.IsNotNull(toggle);
                    Assert.IsNotNull(field.Q<Foldout>(className: "encosy-overridable__header-label"));
                    Assert.IsNotNull(size);
                    Assert.IsTrue(size.isDelayed);
                    Assert.IsFalse(size.enabledSelf);

                    var expectedSize = fieldName == nameof(OverridableChoiceTestAsset.limitSteps) ? 2 : 3;
                    Assert.AreEqual(expectedSize, size.value);
                    Assert.IsNotNull(field.Q<ListView>());
                    Assert.IsFalse(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
                }
                finally
                {
                    field.RemoveFromHierarchy();
                }

                LogAssert.NoUnexpectedReceived();

                void BindCollections()
                {
                    root.Bind(serializedObject);
                }
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void Bind_DefaultIntShowsDefaultWithoutChangingAsset()
        {
            var dirtyCount = EditorUtility.GetDirtyCount(_asset);
            CreateRow(nameof(OverridableTestAsset.retryCount));

            Assert.DoesNotThrow(BindRow);

            var valueField = _row.Q<PropertyField>();
            var input = valueField.Q<IntegerField>();
            var buttons = _row.Query<Button>().ToList();

            Assert.IsNotNull(_row.Q<OverridableSwitch>());
            Assert.IsFalse(buttons.Exists(static button => button.text == "Reset"));
            Assert.IsNull(_row.Q<Button>(className: "encosy-overridable__more"));
            Assert.IsFalse(valueField.enabledSelf);
            Assert.IsNotNull(input);
            Assert.AreEqual(3, input.value);
            Assert.AreEqual(9, _asset.retryCount.value);
            Assert.IsFalse(_asset.retryCount.isOverridden);
            Assert.AreEqual(dirtyCount, EditorUtility.GetDirtyCount(_asset));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Refresh_AfterBeginOverrideEnablesTheSameValueField()
        {
            CreateRow(nameof(OverridableTestAsset.retryCount));
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var property = _serializedObject.FindProperty(nameof(OverridableTestAsset.retryCount));
            var attribute = GetDefaultAttribute(nameof(OverridableTestAsset.retryCount));

            OverridableEditorAPI.BeginOverride(property, attribute, typeof(int));
            _builder.Refresh();

            Assert.AreSame(valueField, _row.Q<PropertyField>());
            Assert.IsTrue(valueField.enabledSelf);
            Assert.AreEqual(3, valueField.Q<IntegerField>().value);
            Assert.AreEqual(3, _asset.retryCount.value);
            Assert.IsTrue(_asset.retryCount.isOverridden);
            Assert.IsTrue(_row.Q<OverridableSwitch>().value);
            Assert.IsEmpty(valueField.parent.tooltip);
        }

        [Test]
        public void Switch_UsesShownDefaultAndPreservesStoredValueWhenTurnedOff()
        {
            CreateRow(nameof(OverridableTestAsset.retryCount));
            BindRow();
            var control = _row.Q<OverridableSwitch>();
            control.value = true;

            Assert.AreEqual(3, _asset.retryCount.value);
            Assert.IsTrue(_asset.retryCount.isOverridden);

            var value = _serializedObject.FindProperty(nameof(OverridableTestAsset.retryCount));
            value.FindPropertyRelative(OverridableEditorAPI.VALUE).intValue = 12;
            _serializedObject.ApplyModifiedProperties();
            control.value = false;

            Assert.AreEqual(12, _asset.retryCount.value);
            Assert.IsFalse(_asset.retryCount.isOverridden);
            Assert.IsFalse(_row.Q<PropertyField>().enabledSelf);
            Assert.AreEqual(3, _row.Q<IntegerField>().value);

            _row.RemoveFromHierarchy();
            CreateRow(nameof(OverridableTestAsset.guidValue));
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var guidInput = valueField.Q<SerializableGuidField>();
            control = _row.Q<OverridableSwitch>();
            Assert.IsNotNull(guidInput);
            Assert.AreEqual(OverridableTestDefaults.GuidValue, guidInput.value);
            Assert.AreEqual(SerializableGuid.Empty, _asset.guidValue.value);
            Assert.IsFalse(_asset.guidValue.isOverridden);
            var newV4 = guidInput.Q<Button>(className: SerializableGuidField.NewButtonUssClassName);
            var newV7 = guidInput.Q<Button>(className: SerializableGuidField.NewV7ButtonUssClassName);
            Assert.IsNotNull(newV4);
            Assert.IsNotNull(newV7);
            Assert.IsFalse(guidInput.enabledInHierarchy);
            Assert.IsFalse(newV4.enabledInHierarchy);
            Assert.IsFalse(newV7.enabledInHierarchy);

            Assert.AreEqual(
                  "Default value: 01234567-89ab-4cde-8f01-23456789abcd\nFrom: OverridableTestDefaults ▸ GuidValue"
                , valueField.parent.tooltip
            );

            Undo.IncrementCurrentGroup();
            control.value = true;
            guidInput = valueField.Q<SerializableGuidField>();
            Assert.AreSame(valueField, _row.Q<PropertyField>());
            Assert.AreEqual(OverridableTestDefaults.GuidValue, _asset.guidValue.value);
            Assert.IsTrue(_asset.guidValue.isOverridden);
            newV4 = guidInput.Q<Button>(className: SerializableGuidField.NewButtonUssClassName);
            newV7 = guidInput.Q<Button>(className: SerializableGuidField.NewV7ButtonUssClassName);
            Assert.IsNotNull(newV4);
            Assert.IsNotNull(newV7);
            Assert.IsTrue(guidInput.enabledInHierarchy);
            Assert.IsTrue(newV4.enabledInHierarchy);
            Assert.IsTrue(newV7.enabledInHierarchy);

            Undo.PerformUndo();
            _serializedObject.Update();
            _builder.Refresh();
            Assert.AreEqual(SerializableGuid.Empty, _asset.guidValue.value);
            Assert.IsFalse(_asset.guidValue.isOverridden);

            control.value = true;
            guidInput = valueField.Q<SerializableGuidField>();
            var customGuid = new SerializableGuid(new System.Guid("fedcba98-7654-4321-8fed-cba987654321"));
            guidInput.value = customGuid;
            Assert.AreEqual(customGuid, _asset.guidValue.value);
            control.value = false;

            Assert.AreEqual(customGuid, _asset.guidValue.value);
            Assert.IsFalse(_asset.guidValue.isOverridden);
            Assert.AreEqual(OverridableTestDefaults.GuidValue, valueField.Q<SerializableGuidField>().value);
            Assert.IsFalse(valueField.enabledSelf);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void StaticDefaultTick_UpdatesSameHolderWithoutManualRefresh()
        {
            var previousHolders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
            CreateRow(nameof(OverridableTestAsset.retryCount));
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var input = valueField.Q<IntegerField>();
            var holder = GetNewHolder(previousHolders);
            using var holderObject = new SerializedObject(holder);
            var boundProperty = holderObject.FindProperty(valueField.bindingPath);
            _row.Bind(_serializedObject);

            Assert.AreEqual(3, input.value);
            Assert.AreEqual(3, boundProperty.intValue);

            OverridableTestDefaults.IntValue = 8;
            _builder.TickDefaultSource();
            holderObject.Update();

            Assert.AreSame(input, valueField.Q<IntegerField>());
            Assert.AreSame(holder, GetNewHolder(previousHolders));
            Assert.AreEqual(8, boundProperty.intValue);
            Assert.AreEqual(9, _asset.retryCount.value);
            StringAssert.Contains("Default value: 8", valueField.parent.tooltip);

            OverridableTestDefaults.IntValue = 3;
            _builder.TickDefaultSource();
            holderObject.Update();

            Assert.AreEqual(3, boundProperty.intValue);
            Assert.AreSame(input, valueField.Q<IntegerField>());
        }

        [Test]
        public void StaticDefaultTick_LeavesOverrideUntouchedAndResumesAfterSwitchOff()
        {
            var previousHolders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
            CreateRow(nameof(OverridableTestAsset.retryCount));
            BindRow();
            var control = _row.Q<OverridableSwitch>();
            var holder = GetNewHolder(previousHolders);
            control.value = true;
            OverridableTestDefaults.IntValue = 8;

            _builder.TickDefaultSource();

            Assert.IsTrue(_asset.retryCount.isOverridden);
            Assert.AreEqual(expected: 3, actual: _asset.retryCount.value);
            Assert.AreEqual(expected: 3, actual: _row.Q<IntegerField>().value);
            Assert.IsEmpty(_row.Q<PropertyField>().parent.tooltip);

            control.value = false;
            OverridableTestDefaults.IntValue = 12;
            _builder.TickDefaultSource();

            var valueField = _row.Q<PropertyField>();
            using var holderObject = new SerializedObject(holder);

            Assert.AreEqual(expected: 12, actual: holderObject.FindProperty(valueField.bindingPath).intValue);
            Assert.IsFalse(_asset.retryCount.isOverridden);
            Assert.AreEqual(expected: 3, actual: _asset.retryCount.value);
            StringAssert.Contains("Default value: 12", valueField.parent.tooltip);
        }

        [Test]
        public void StaticDefaultTick_RefreshesClassMembersWhenDisplayTextIsUnchanged()
        {
            var previousHolders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();

            var attribute = new OverridableDefaultAttribute(
                  typeof(OverridablePropertyDrawerBindingTests)
                , nameof(PayloadDefault)
            );

            CreateRow(nameof(OverridableTestAsset.payloadValue), attribute);
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var holder = GetNewHolder(previousHolders);
            using var holderObject = new SerializedObject(holder);
            var value = holderObject.FindProperty(valueField.bindingPath);
            Assert.AreEqual(expected: 7, actual: value.FindPropertyRelative("number").intValue);
            var previousText = PayloadDefault.ToString();

            PayloadDefault.number = 12;
            _builder.TickDefaultSource();
            holderObject.Update();

            Assert.AreEqual(previousText, PayloadDefault.ToString());
            Assert.AreEqual(expected: 12, actual: value.FindPropertyRelative("number").intValue);
            Assert.AreSame(holder, GetNewHolder(previousHolders));
            Assert.IsFalse(_asset.payloadValue.isOverridden);
        }

        [Test]
        public void StaticDefaultTick_RefreshesChangedCollectionSizeAndElements()
        {
            s_collectionDefault.Add(1);
            var attribute = new OverridableDefaultAttribute(
                  typeof(OverridablePropertyDrawerBindingTests)
                , nameof(CollectionDefault)
            );

            CreateRow(nameof(OverridableTestAsset.listValue), attribute);
            BindRow();
            var valueField = _row.Q<PropertyField>();
            AssertCollectionBinding(valueField: valueField, count: 1, boundToValue: false);

            s_collectionDefault[0] = 4;
            s_collectionDefault.Add(7);
            _builder.TickDefaultSource();

            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: false);
            var items = valueField.Q<ListView>().itemsSource;
            Assert.AreEqual(expected: 4, actual: ((SerializedProperty)items[0]).intValue);
            Assert.AreEqual(expected: 7, actual: ((SerializedProperty)items[1]).intValue);
            Assert.IsFalse(_asset.listValue.isOverridden);
            StringAssert.Contains("Default value: [4, 7]", valueField.parent.tooltip);
        }

        [Test]
        public void Bind_MixedOverridesShowsMixedSwitchAndValue()
        {
            _secondAsset = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _secondAsset.retryCount = new(value: 4, isOverridden: true);
            _serializedObject.Dispose();
            _serializedObject = new SerializedObject(new Object[] { _asset, _secondAsset });
            CreateRow(nameof(OverridableTestAsset.retryCount));

            Assert.DoesNotThrow(BindRow);
            Assert.IsTrue(_row.Q<OverridableSwitch>().ShowMixedValue);
            Assert.IsTrue(_row.Q<IntegerField>().showMixedValue);
            Assert.IsTrue(_row.Q<PropertyField>().enabledSelf);
            Assert.AreEqual(9, _asset.retryCount.value);
            Assert.AreEqual(4, _secondAsset.retryCount.value);
        }

        [Test]
        public void Bind_EnumWithoutDefaultShowsBareDefault()
        {
            _asset.enumWithoutDefault = new(value: OverridableTestEnum.ThirdValue, isOverridden: false);
            _serializedObject.Update();
            CreateRow(nameof(OverridableTestAsset.enumWithoutDefault));
            BindRow();
            var choice = _row.Q<OverridableChoiceField>();
            var text = choice.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName);

            Assert.IsNotNull(text);
            Assert.AreEqual(0, choice.value);
            Assert.AreEqual("Default", text.text);
            StringAssert.Contains("Default value: First", choice.InputTooltip);
            Assert.AreEqual(OverridableTestEnum.ThirdValue, _asset.enumWithoutDefault.value);
            Assert.IsFalse(_asset.enumWithoutDefault.isOverridden);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Bind_EnumTooltipFollowsDefaultSelection()
        {
            CreateRow(nameof(OverridableTestAsset.enumWithDefault));
            BindRow();
            var choice = _row.Q<OverridableChoiceField>();
            var text = choice.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName);

            Assert.IsNotNull(text);
            Assert.AreEqual(0, choice.value);
            Assert.AreEqual("Default (Third Value)", text.text);
            StringAssert.Contains("Default value: Third Value", choice.tooltip);
            Assert.IsNull(_row.Q<Button>(className: "encosy-overridable__more"));

            choice.value = 1;

            Assert.IsTrue(_asset.enumWithDefault.isOverridden);
            Assert.AreEqual(OverridableTestEnum.First, _asset.enumWithDefault.value);
            Assert.IsEmpty(choice.tooltip);
            Assert.IsEmpty(choice.InputTooltip);

            choice.value = 0;

            Assert.IsFalse(_asset.enumWithDefault.isOverridden);
            StringAssert.Contains("Default value: Third Value", choice.tooltip);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ClassHeader_ExpandsAndBindsMembersWithoutChangingStoredValue(bool isOverridden)
        {
            _asset.payloadValue = new(new OverridableTestPayload { number = 9, text = "Stored" }, isOverridden);
            _serializedObject.Update();
            var property = _serializedObject.FindProperty(nameof(OverridableTestAsset.payloadValue));
            var valueProperty = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            valueProperty.isExpanded = false;
            var previousHolders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
            var fieldInfo = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.payloadValue));

            var attribute = new OverridableDefaultAttribute(
                  typeof(OverridableTestDefaults)
                , nameof(OverridableTestDefaults.PayloadValue)
            );

            _builder = new OverridableRowBuilder(property, fieldInfo, typeof(OverridableTestPayload), attribute);
            _row = _builder.Root;
            BindRow();
            var header = _row.Q<Foldout>(className: "encosy-overridable__header-label");
            var valueField = _row.Q<PropertyField>();

            Assert.IsNotNull(header);
            Assert.IsTrue(header.enabledInHierarchy);
            Assert.AreEqual(isOverridden, valueField.enabledSelf);
            Assert.IsFalse(header.value);

            header.value = true;
            var valueFoldout = valueField.Q<Foldout>();

            Assert.IsTrue(valueProperty.isExpanded);
            Assert.IsTrue(valueFoldout.value);

            var members = valueFoldout.Query<PropertyField>().ToList();
            var count = members.Count;
            Assert.AreEqual(2, count);

            Object bindingTarget = isOverridden ? _asset : GetNewHolder(previousHolders);
            using var bindingObject = new SerializedObject(bindingTarget);

            for (var i = 0; i < count; i++)
            {
                var member = members[i];
                Assert.IsNotNull(member.panel);
                member.Bind(bindingObject);
            }

            var number = valueField.Q<IntegerField>();
            var text = valueField.Q<TextField>();

            Assert.IsNotNull(number);
            Assert.IsNotNull(text);
            Assert.AreEqual(isOverridden ? 9 : 7, number.value);
            Assert.AreEqual(isOverridden ? "Stored" : "Default", text.value);
            Assert.AreEqual(isOverridden, number.enabledInHierarchy);
            Assert.AreEqual(isOverridden, text.enabledInHierarchy);
            Assert.AreEqual(9, _asset.payloadValue.value.number);
            Assert.AreEqual("Stored", _asset.payloadValue.value.text);
            Assert.AreEqual(isOverridden, _asset.payloadValue.isOverridden);

            header.value = false;

            Assert.IsFalse(valueProperty.isExpanded);
            Assert.IsFalse(valueFoldout.value);

            _row.RemoveFromHierarchy();
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ClassHeader_CustomDrawerWithoutFoldoutTogglesBody(bool isOverridden)
        {
            var asset = ScriptableObject.CreateInstance<ImguiCustomDrawerAsset>();

            try
            {
                asset.payload = new(value: new() { number = 9 }, isOverridden: isOverridden);
                using var serializedObject = new SerializedObject(asset);
                var property = serializedObject.FindProperty(nameof(ImguiCustomDrawerAsset.payload));
                var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
                value.isExpanded = true;
                var field = typeof(ImguiCustomDrawerAsset).GetField(property.name);

                _builder = new OverridableRowBuilder(
                      property: property
                    , fieldInfo: field
                    , valueType: typeof(ImguiCustomDrawerPayload)
                    , attribute: null
                );

                _row = _builder.Root;
                _window.rootVisualElement.Add(_row);

                try
                {
                    _row.Bind(serializedObject);
                    var header = _row.Q<Foldout>(className: "encosy-overridable__header-label");
                    var valueField = _row.Q<PropertyField>();
                    Assert.IsNull(valueField.Q<Foldout>());
                    Assert.IsNotNull(valueField.Q<IntegerField>());
                    Assert.AreEqual(isOverridden, valueField.enabledSelf);
                    Assert.AreEqual(DisplayStyle.Flex, valueField.parent.style.display.value);

                    header.value = false;

                    Assert.IsFalse(value.isExpanded);
                    Assert.AreEqual(DisplayStyle.None, valueField.parent.style.display.value);

                    header.value = true;

                    Assert.IsTrue(value.isExpanded);
                    Assert.AreEqual(DisplayStyle.Flex, valueField.parent.style.display.value);
                    Assert.AreEqual(isOverridden, valueField.enabledSelf);
                    Assert.AreEqual(expected: 9, actual: asset.payload.value.number);
                    Assert.AreEqual(isOverridden, asset.payload.isOverridden);
                }
                finally
                {
                    _row.RemoveFromHierarchy();
                }

                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [TestCase(nameof(OverridableTestAsset.listValue), nameof(OverridableTestDefaults.ListValue))]
        [TestCase(nameof(OverridableTestAsset.arrayValue), nameof(OverridableTestDefaults.ArrayValue))]
        public void CollectionHeader_BindsDefaultSizeThenEditsStoredCollection(string fieldName, string defaultName)
        {
            _asset.listValue = new(value: new() { 9, 8, 7 }, isOverridden: false);
            _asset.arrayValue = new(value: new[] { 9, 8, 7 }, isOverridden: false);
            _serializedObject.Update();
            var property = _serializedObject.FindProperty(fieldName);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            value.isExpanded = false;
            var attribute = new OverridableDefaultAttribute(typeof(OverridableTestDefaults), defaultName);
            CreateRow(fieldName, attribute);
            BindRow();
            var header = _row.Q<Foldout>(className: "encosy-overridable__header-label");
            var size = _row.Q<IntegerField>(className: "encosy-overridable__collection-size");
            var valueField = _row.Q<PropertyField>();
            var listView = valueField.Q<ListView>();
            var control = _row.Q<OverridableSwitch>();

            Assert.IsNotNull(header);
            Assert.IsNotNull(size);
            Assert.IsNotNull(listView);
            Assert.AreEqual(2, size.value);
            Assert.IsFalse(size.enabledSelf);
            Assert.IsFalse(valueField.enabledSelf);
            Assert.IsFalse(listView.showFoldoutHeader);
            Assert.IsFalse(listView.showBoundCollectionSize);
            Assert.AreEqual(3, value.arraySize);
            Assert.AreEqual(9, value.GetArrayElementAtIndex(0).intValue);
            StringAssert.Contains("Default value: [3, 5]", valueField.parent.tooltip);

            header.value = true;

            Assert.IsTrue(value.isExpanded);

            control.value = true;

            Assert.AreSame(valueField, _row.Q<PropertyField>());
            Assert.IsTrue(size.enabledSelf);
            Assert.IsTrue(valueField.enabledSelf);
            Assert.AreEqual(2, size.value);
            Assert.AreEqual(2, value.arraySize);
            Assert.AreEqual(3, value.GetArrayElementAtIndex(0).intValue);
            Assert.AreEqual(5, value.GetArrayElementAtIndex(1).intValue);
            Assert.IsEmpty(valueField.parent.tooltip);

            size.value = 1;
            _serializedObject.Update();

            Assert.AreEqual(1, value.arraySize);
            Assert.AreEqual(3, value.GetArrayElementAtIndex(0).intValue);

            control.value = false;

            Assert.AreEqual(2, size.value);
            Assert.IsFalse(size.enabledSelf);
            Assert.IsFalse(valueField.enabledSelf);
            Assert.AreEqual(1, value.arraySize);

            header.value = false;

            Assert.IsFalse(value.isExpanded);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(nameof(OverridableTestAsset.listValue), nameof(OverridableTestDefaults.ListValue))]
        [TestCase(nameof(OverridableTestAsset.arrayValue), nameof(OverridableTestDefaults.ArrayValue))]
        public void CollectionSwitch_UndoRestoresStoredCollectionAndDefaultBinding(string fieldName, string defaultName)
        {
            _asset.listValue = new(value: new() { 9, 8, 7 }, isOverridden: false);
            _asset.arrayValue = new(value: new[] { 9, 8, 7 }, isOverridden: false);
            _serializedObject.Update();
            var attribute = new OverridableDefaultAttribute(typeof(OverridableTestDefaults), defaultName);
            CreateRow(fieldName, attribute);
            BindRow();
            var control = _row.Q<OverridableSwitch>();
            Undo.IncrementCurrentGroup();
            control.value = true;

            var property = _serializedObject.FindProperty(fieldName);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);

            Assert.IsTrue(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.AreEqual(2, value.arraySize);
            Assert.AreEqual(3, value.GetArrayElementAtIndex(0).intValue);

            Undo.PerformUndo();
            _serializedObject.Update();
            _builder.Refresh();

            Assert.IsFalse(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.AreEqual(3, value.arraySize);
            Assert.AreEqual(9, value.GetArrayElementAtIndex(0).intValue);
            Assert.AreEqual(8, value.GetArrayElementAtIndex(1).intValue);
            Assert.AreEqual(7, value.GetArrayElementAtIndex(2).intValue);
            Assert.IsFalse(control.value);
            Assert.IsFalse(_row.Q<PropertyField>().enabledSelf);
            Assert.AreEqual(2, _row.Q<IntegerField>(className: "encosy-overridable__collection-size").value);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NestedElement_IgnoresOuterDefaultAndOverridesOnlyItsOwnValue(bool usesValueFieldInfo)
        {
            _asset.nestedList = new(
                  value: new() {
                      new(value: 2, isOverridden: true),
                      new(value: 9, isOverridden: false),
                  }
                , isOverridden: true
            );

            _serializedObject.Update();
            var fieldInfo = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.nestedList));
            var outer = _serializedObject.FindProperty(fieldInfo.Name);
            var element = outer.FindPropertyRelative(OverridableEditorAPI.VALUE).GetArrayElementAtIndex(1);
            var attribute = GetDefaultAttribute(fieldInfo.Name);

            if (usesValueFieldInfo)
            {
                fieldInfo = fieldInfo.FieldType.GetField(OverridableEditorAPI.VALUE);
            }

            Assert.IsTrue(OverridableEditorAPI.IsOverridableListElement(element, fieldInfo));
            Assert.AreEqual(typeof(int), OverridableEditorAPI.GetValueType(fieldInfo, element));

            _builder = new OverridableRowBuilder(element, fieldInfo, typeof(int), attribute);
            _row = _builder.Root;
            BindRow();
            var control = _row.Q<OverridableSwitch>();
            var valueField = _row.Q<PropertyField>();

            Assert.AreEqual(0, valueField.Q<IntegerField>().value);
            Assert.IsFalse(valueField.enabledSelf);
            Assert.IsTrue(control.enabledSelf);
            Assert.IsNull(_row.Q<Button>(className: "encosy-overridable__more"));
            StringAssert.Contains("No [OverridableDefault]", valueField.parent.tooltip);
            Assert.AreEqual(9, _asset.nestedList.value[1].value);
            Assert.IsFalse(_asset.nestedList.value[1].isOverridden);

            control.value = true;

            Assert.IsTrue(_asset.nestedList.isOverridden);
            Assert.AreEqual(2, _asset.nestedList.value[0].value);
            Assert.IsTrue(_asset.nestedList.value[0].isOverridden);
            Assert.AreEqual(0, _asset.nestedList.value[1].value);
            Assert.IsTrue(_asset.nestedList.value[1].isOverridden);
            Assert.IsTrue(valueField.enabledSelf);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void NestedCollectionSwitch_CopiesElementFlagsAndUndoRestoresStoredValues()
        {
            _asset.nestedList = new(value: new() { new(value: 12, isOverridden: false) }, isOverridden: false);
            _serializedObject.Update();
            CreateRow(nameof(OverridableTestAsset.nestedList));
            BindRow();
            var control = _row.Q<OverridableSwitch>();
            var valueField = _row.Q<PropertyField>();

            Assert.IsFalse(valueField.enabledSelf);
            StringAssert.Contains("Default value: [2, Default]", valueField.parent.tooltip);
            Assert.AreEqual(1, _asset.nestedList.value.Count);
            Assert.AreEqual(12, _asset.nestedList.value[0].value);

            Undo.IncrementCurrentGroup();
            control.value = true;

            Assert.IsTrue(valueField.enabledSelf);
            Assert.IsTrue(_asset.nestedList.isOverridden);
            Assert.AreEqual(2, _asset.nestedList.value.Count);
            Assert.AreEqual(2, _asset.nestedList.value[0].value);
            Assert.IsTrue(_asset.nestedList.value[0].isOverridden);
            Assert.AreEqual(9, _asset.nestedList.value[1].value);
            Assert.IsFalse(_asset.nestedList.value[1].isOverridden);
            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: true);

            Undo.PerformUndo();
            _serializedObject.Update();

            Assert.IsFalse(_asset.nestedList.isOverridden);
            Assert.AreEqual(1, _asset.nestedList.value.Count);
            Assert.AreEqual(12, _asset.nestedList.value[0].value);
            Assert.IsFalse(_asset.nestedList.value[0].isOverridden);
            Assert.IsFalse(control.value);
            Assert.IsFalse(valueField.enabledSelf);
            Assert.AreEqual(2, _row.Q<IntegerField>(className: "encosy-overridable__collection-size").value);
            Assert.AreSame(valueField, _row.Q<PropertyField>());
            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: false);

            Undo.PerformRedo();
            _serializedObject.Update();

            Assert.IsTrue(_asset.nestedList.isOverridden);
            Assert.AreEqual(expected: 2, actual: _asset.nestedList.value.Count);
            Assert.AreEqual(expected: 2, actual: _asset.nestedList.value[0].value);
            Assert.IsTrue(_asset.nestedList.value[0].isOverridden);
            Assert.AreEqual(expected: 9, actual: _asset.nestedList.value[1].value);
            Assert.IsFalse(_asset.nestedList.value[1].isOverridden);
            Assert.IsTrue(control.value);
            Assert.IsTrue(valueField.enabledSelf);
            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: true);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(nameof(OverridableTestAsset.listValue))]
        [TestCase(nameof(OverridableTestAsset.arrayValue))]
        public void CollectionResize_UndoRedoRefreshesElementsWithoutChangingOverride(string fieldName)
        {
            _asset.listValue = new(value: new() { 9, 8, 7 }, isOverridden: true);
            _asset.arrayValue = new(value: new[] { 9, 8, 7 }, isOverridden: true);
            _serializedObject.Update();
            CreateRow(fieldName);
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var property = _serializedObject.FindProperty(fieldName);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            AssertCollectionBinding(valueField: valueField, count: 3, boundToValue: true);
            Undo.IncrementCurrentGroup();
            value.arraySize = 1;
            _serializedObject.ApplyModifiedProperties();
            _builder.Refresh();

            AssertCollectionBinding(valueField: valueField, count: 1, boundToValue: true);
            Assert.AreEqual(expected: 9, actual: value.GetArrayElementAtIndex(0).intValue);

            Undo.PerformUndo();
            _serializedObject.Update();

            Assert.IsTrue(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.AreSame(valueField, _row.Q<PropertyField>());
            AssertCollectionBinding(valueField: valueField, count: 3, boundToValue: true);
            Assert.AreEqual(expected: 9, actual: value.GetArrayElementAtIndex(0).intValue);
            Assert.AreEqual(expected: 8, actual: value.GetArrayElementAtIndex(1).intValue);
            Assert.AreEqual(expected: 7, actual: value.GetArrayElementAtIndex(2).intValue);

            Undo.PerformRedo();
            _serializedObject.Update();

            Assert.IsTrue(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            AssertCollectionBinding(valueField: valueField, count: 1, boundToValue: true);
            Assert.AreEqual(expected: 9, actual: value.GetArrayElementAtIndex(0).intValue);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void CollectionDefaultResize_RefreshesElementsInTheSameHolder()
        {
            s_collectionDefault.AddRange(new[] { 3, 5, 7 });
            _asset.listValue = new(value: new() { 91 }, isOverridden: false);
            _serializedObject.Update();

            var attribute = new OverridableDefaultAttribute(
                  typeof(OverridablePropertyDrawerBindingTests)
                , nameof(CollectionDefault)
            );

            CreateRow(nameof(OverridableTestAsset.listValue), attribute);
            BindRow();
            var valueField = _row.Q<PropertyField>();
            AssertCollectionBinding(valueField: valueField, count: 3, boundToValue: false);
            var list = valueField.Q<ListView>();
            var holder = ((SerializedProperty)list.itemsSource[0]).serializedObject.targetObject;
            s_collectionDefault.RemoveRange(1, 2);
            _builder.Refresh();

            Assert.AreSame(valueField, _row.Q<PropertyField>());
            AssertCollectionBinding(valueField: valueField, count: 1, boundToValue: false);
            list = valueField.Q<ListView>();
            var element = (SerializedProperty)list.itemsSource[0];
            Assert.AreSame(holder, element.serializedObject.targetObject);
            Assert.AreEqual(expected: 3, actual: element.intValue);
            var size = _row.Q<IntegerField>(className: "encosy-overridable__collection-size");
            Assert.AreEqual(expected: 1, actual: size.value);
            Assert.IsFalse(_asset.listValue.isOverridden);
            CollectionAssert.AreEqual(new[] { 91 }, _asset.listValue.value);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void CollectionDetach_UndoThenReattachRestoresLiveBindingsForRedo()
        {
            _asset.nestedList = new(value: new() { new(value: 12, isOverridden: false) }, isOverridden: false);
            _serializedObject.Update();
            CreateRow(nameof(OverridableTestAsset.nestedList));
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var control = _row.Q<OverridableSwitch>();
            Undo.IncrementCurrentGroup();
            control.value = true;
            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: true);
            _row.RemoveFromHierarchy();

            Undo.PerformUndo();
            _serializedObject.Update();

            Assert.IsNull(_row.panel);
            Assert.IsFalse(_asset.nestedList.isOverridden);
            Assert.AreEqual(expected: 1, actual: _asset.nestedList.value.Count);
            Assert.AreEqual(expected: 12, actual: _asset.nestedList.value[0].value);

            BindRow();

            Assert.AreSame(valueField, _row.Q<PropertyField>());
            Assert.IsFalse(control.value);
            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: false);

            Undo.PerformRedo();
            _serializedObject.Update();

            Assert.IsTrue(control.value);
            Assert.IsTrue(_asset.nestedList.isOverridden);
            AssertCollectionBinding(valueField: valueField, count: 2, boundToValue: true);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(0, 2, true)]
        [TestCase(1, 0, false)]
        public void NestedHolderElement_PreservesFlagsAndLocksItsControls(int index, int shownValue, bool isOverridden)
        {
            _outerHolder = OverridableDefaultHolder.Create(typeof(Overridable<int>[]));

            var defaults = new[] {
                new Overridable<int>(value: 2, isOverridden: true),
                new Overridable<int>(value: 9, isOverridden: false),
            };

            var list = _outerHolder.Write(value: defaults, expandedSource: null);
            var element = list.GetArrayElementAtIndex(index);
            var fieldInfo = typeof(OverridableDefaultBox<Overridable<int>[]>).GetField(OverridableEditorAPI.VALUE);
            var valueType = OverridableEditorAPI.GetValueType(fieldInfo, element);

            _builder = new OverridableRowBuilder(
                  property: element
                , fieldInfo: fieldInfo
                , valueType: valueType
                , attribute: null
            );

            _row = _builder.Root;
            _window.rootVisualElement.Add(_row);
            var control = _row.Q<OverridableSwitch>();
            var more = _row.Q<Button>(className: "encosy-overridable__more");
            var valueField = _row.Q<PropertyField>();
            var input = valueField.Q<IntegerField>();

            Assert.IsNotNull(input);
            Assert.AreEqual(shownValue, input.value);

            _row.Bind(list.serializedObject);

            Assert.AreSame(input, valueField.Q<IntegerField>());

            _window.rootVisualElement.Bind(list.serializedObject);

            Assert.AreSame(input, valueField.Q<IntegerField>());
            Assert.AreEqual(isOverridden, control.value);
            Assert.AreEqual(shownValue, input.value);
            Assert.IsFalse(input.enabledInHierarchy);
            Assert.IsFalse(control.enabledSelf);
            Assert.IsNull(more);
            Assert.AreEqual(expected: 0, actual: _builder.BuildContextMenu().GetItemCount());
            Assert.IsFalse(valueField.enabledSelf);
            Assert.IsEmpty(control.tooltip);
            Assert.IsEmpty(valueField.parent.tooltip);

            control.value = isOverridden == false;
            list.serializedObject.Update();

            Assert.AreEqual(isOverridden, element.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.AreEqual(defaults[index].value, element.FindPropertyRelative(OverridableEditorAPI.VALUE).intValue);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(nameof(OverridableTestAsset.ordinaryArray))]
        [TestCase(nameof(OverridableTestAsset.ordinaryList))]
        public void OrdinaryCollectionElement_KeepsFieldDefault(string fieldName)
        {
            _asset.ordinaryArray = new[] { new Overridable<int>(value: 9, isOverridden: false) };
            _asset.ordinaryList = new() { new(value: 9, isOverridden: false) };
            _serializedObject.Update();
            var element = _serializedObject.FindProperty(fieldName).GetArrayElementAtIndex(0);
            var fieldInfo = typeof(OverridableTestAsset).GetField(fieldName);

            _builder = new OverridableRowBuilder(
                  property: element
                , fieldInfo: fieldInfo
                , valueType: typeof(int)
                , attribute: null
            );

            _row = _builder.Root;
            BindRow();
            var valueField = _row.Q<PropertyField>();

            Assert.AreEqual(3, valueField.Q<IntegerField>().value);
            Assert.IsFalse(valueField.enabledSelf);
            Assert.IsNull(_row.Q<Button>(className: "encosy-overridable__more"));
            StringAssert.Contains("Default value: 3", valueField.parent.tooltip);

            _row.Q<OverridableSwitch>().value = true;

            Assert.IsTrue(element.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            Assert.AreEqual(3, element.FindPropertyRelative(OverridableEditorAPI.VALUE).intValue);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Bind_HolderTargetLocksControlsAndHasNoOwnTooltips()
        {
            _outerHolder = OverridableDefaultHolder.Create(typeof(Overridable<int>));

            var property = _outerHolder.Write(
                  value: new Overridable<int>(value: 9, isOverridden: true)
                , expandedSource: null
            );

            var fieldInfo = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.intValue));

            _builder = new OverridableRowBuilder(
                  property: property
                , fieldInfo: fieldInfo
                , valueType: typeof(int)
                , attribute: null
            );

            _row = _builder.Root;
            _window.rootVisualElement.Add(_row);
            _row.Bind(property.serializedObject);
            var control = _row.Q<OverridableSwitch>();
            var more = _row.Q<Button>(className: "encosy-overridable__more");
            var valueField = _row.Q<PropertyField>();

            Assert.IsFalse(control.enabledSelf);
            Assert.IsFalse(valueField.enabledSelf);
            Assert.IsNull(more);
            Assert.AreEqual(expected: 0, actual: _builder.BuildContextMenu().GetItemCount());
            Assert.IsEmpty(control.tooltip);
            Assert.IsEmpty(valueField.parent.tooltip);
            Assert.AreEqual(9, valueField.Q<IntegerField>().value);
        }

        [Test]
        public void Detach_DestroysHolderAndReattachCreatesFreshDefaultBinding()
        {
            var previousHolders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
            CreateRow(nameof(OverridableTestAsset.retryCount));
            BindRow();
            var valueField = _row.Q<PropertyField>();
            var previousHolder = GetNewHolder(previousHolders);
            _row.RemoveFromHierarchy();

            Assert.IsTrue(previousHolder.IsInvalid());
            OverridableTestDefaults.IntValue = 8;
            _builder.TickDefaultSource();
            Assert.AreEqual(previousHolders.Length, Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>().Length);

            BindRow();
            var holder = GetNewHolder(previousHolders);

            Assert.IsTrue(holder.IsValid());
            Assert.AreNotSame(previousHolder, holder);
            Assert.AreEqual(expected: 8, actual: valueField.Q<IntegerField>().value);
        }

        private void AssertCollectionBinding(PropertyField valueField, int count, bool boundToValue)
        {
            var list = valueField.Q<ListView>();
            Assert.IsNotNull(list);
            Assert.IsNotNull(list.itemsSource);
            Assert.AreEqual(count, list.itemsSource.Count);

            for (var i = 0; i < count; i++)
            {
                var element = list.itemsSource[i] as SerializedProperty;
                Assert.IsNotNull(element);
                Assert.AreEqual($"{valueField.bindingPath}.Array.data[{i}]", element.propertyPath);

                if (boundToValue)
                {
                    Assert.AreSame(_asset, element.serializedObject.targetObject);
                }
                else
                {
                    Assert.IsInstanceOf<OverridableDefaultHolder>(element.serializedObject.targetObject);
                }
            }
        }

        private void CreateRow(string fieldName, OverridableDefaultAttribute attribute = null)
        {
            var property = _serializedObject.FindProperty(fieldName);
            var fieldInfo = typeof(OverridableTestAsset).GetField(fieldName);

            _builder = new OverridableRowBuilder(
                  property
                , fieldInfo
                , OverridableEditorAPI.GetValueType(fieldInfo, property)
                , attribute ?? GetDefaultAttribute(fieldName)
            );

            _row = _builder.Root;
        }

        private void BindRow()
        {
            _window.rootVisualElement.Add(_row);
            _row.Bind(_serializedObject);
        }

        private sealed class BindingTestWindow : EditorWindow
        {
        }
    }
}

#endif
