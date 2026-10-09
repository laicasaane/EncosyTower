using System;
using System.Collections.Generic;
using System.Reflection;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Editor.Serialization;
using EncosyTower.Editor.Serialization.Internals;
using EncosyTower.Serialization;
using EncosyTower.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableEditorAPITests
    {
        private OverridableTestAsset _first;
        private OverridableTestAsset _second;

        [SetUp]
        public void SetUp()
        {
            _first = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _second = ScriptableObject.CreateInstance<OverridableTestAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
        }

        [Test]
        public void GetMode_UsesDropdownOnlyForPlainEnums()
        {
            Assert.AreEqual(OverridableMode.Toggle, OverridableEditorAPI.GetMode(typeof(bool)));
            Assert.AreEqual(OverridableMode.Dropdown, OverridableEditorAPI.GetMode(typeof(OverridableTestEnum)));
            Assert.AreEqual(OverridableMode.Toggle, OverridableEditorAPI.GetMode(typeof(OverridableTestFlags)));
            Assert.AreEqual(OverridableMode.Toggle, OverridableEditorAPI.GetMode(typeof(int)));
        }

        [Test]
        public void GetDefaultLabel_PrefersExplicitThenSettingsThenDefault()
        {
            var explicitLabel = new OverridableDefaultAttribute(
                  typeof(OverridableTestDefaults)
                , nameof(OverridableTestDefaults.EnumValue)
            ) {
                Label = "Global",
            };

            Assert.AreEqual("Global", OverridableEditorAPI.GetDefaultLabel(explicitLabel));
            Assert.AreEqual(
                  "Project Settings"
                , OverridableEditorAPI.GetDefaultLabel(new(typeof(TestSettings), nameof(TestSettings.flag)))
            );

            explicitLabel.Label = null;
            Assert.AreEqual("Default", OverridableEditorAPI.GetDefaultLabel(explicitLabel));
            Assert.AreEqual("Default", OverridableEditorAPI.GetDefaultLabel(null));
        }

        [Test]
        public void GetDefaultChoiceLabel_AppendsTheDefaultValueWhenKnown()
        {
            var enumDefault = new OverridableDefaultAttribute(
                  typeof(OverridableTestDefaults)
                , nameof(OverridableTestDefaults.EnumValue)
            ) {
                Label = "Global",
            };

            Assert.AreEqual(
                  "Global (Third Value)"
                , OverridableEditorAPI.GetDefaultChoiceLabel(enumDefault, typeof(OverridableTestEnum))
            );

            Assert.AreEqual(
                  "Default"
                , OverridableEditorAPI.GetDefaultChoiceLabel(attribute: null, valueType: typeof(OverridableTestEnum))
            );
        }

        [Test]
        public void GetValueChoices_UsesEnumDisplayNames()
        {
            CollectionAssert.AreEqual(
                  new[] { "First", "Second Choice", "Third Value" }
                , OverridableEditorAPI.GetValueChoices(typeof(OverridableTestEnum))
            );
            CollectionAssert.IsEmpty(OverridableEditorAPI.GetValueChoices(typeof(int)));
        }

        [Test]
        public void ApplyChoice_SetsOverrideAndValue()
        {
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));

            OverridableEditorAPI.ApplyChoice(
                  property: property
                , valueType: typeof(OverridableTestEnum)
                , choiceIndex: 2
            );

            Assert.AreEqual(Overridden(OverridableTestEnum.Second), _first.enumWithDefault);

            Assert.AreEqual(
                  expected: 2
                , actual: OverridableEditorAPI.GetSelectedIndex(property, typeof(OverridableTestEnum))
            );

            OverridableEditorAPI.ApplyChoice(
                  property: property
                , valueType: typeof(OverridableTestEnum)
                , choiceIndex: 0
            );

            Assert.IsFalse(_first.enumWithDefault.isOverridden);
            Assert.AreEqual(OverridableTestEnum.Second, _first.enumWithDefault.value);

            Assert.AreEqual(
                  expected: 0
                , actual: OverridableEditorAPI.GetSelectedIndex(property, typeof(OverridableTestEnum))
            );
        }

        [Test]
        public void EndOverride_ClearsOnlyTheOverride()
        {
            _first.intValue = new Overridable<int>(value: 4, isOverridden: true);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));

            Assert.IsTrue(OverridableEditorAPI.CanReset(property));

            OverridableEditorAPI.EndOverride(property);

            Assert.IsFalse(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 4, actual: _first.intValue.value);
            Assert.IsFalse(OverridableEditorAPI.CanReset(property));
        }

        [Test]
        public void GetEffectiveText_ShowsOverrideDefaultOrNothing()
        {
            _first.enumWithoutDefault = NotOverridden(OverridableTestEnum.First);

            using var serializedObject = new SerializedObject(_first);
            var withDefault = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));
            var withoutDefault = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithoutDefault));
            var attribute = GetAttribute(nameof(OverridableTestAsset.enumWithDefault));
            var enumType = typeof(OverridableTestEnum);

            Assert.AreEqual("Third Value", OverridableEditorAPI.GetEffectiveText(withDefault, attribute, enumType));

            Assert.AreEqual(
                  string.Empty
                , OverridableEditorAPI.GetEffectiveText(
                        property: withoutDefault
                      , attribute: null
                      , valueType: enumType
                  )
            );

            OverridableEditorAPI.ApplyChoice(property: withDefault, valueType: enumType, choiceIndex: 2);

            Assert.AreEqual("Second Choice", OverridableEditorAPI.GetEffectiveText(withDefault, attribute, enumType));
        }

        [Test]
        public void MultipleObjects_WithDifferentOverrides_AreMixed()
        {
            _first.enumWithDefault = Overridden(OverridableTestEnum.First);
            _second.enumWithDefault = Overridden(OverridableTestEnum.Second);

            using var serializedObject = new SerializedObject(new Object[] { _first, _second });
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));
            var attribute = GetAttribute(nameof(OverridableTestAsset.enumWithDefault));
            var enumType = typeof(OverridableTestEnum);

            Assert.IsTrue(OverridableEditorAPI.IsMixed(property));
            Assert.AreEqual(expected: -1, actual: OverridableEditorAPI.GetSelectedIndex(property, enumType));
            Assert.AreEqual("Mixed", OverridableEditorAPI.GetEffectiveText(property, attribute, enumType));
        }

        [Test]
        public void Undo_AfterApplyChoice_RestoresThePreviousValues()
        {
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));

            Undo.IncrementCurrentGroup();

            OverridableEditorAPI.ApplyChoice(
                  property: property
                , valueType: typeof(OverridableTestEnum)
                , choiceIndex: 3
            );

            Assert.AreEqual(Overridden(OverridableTestEnum.ThirdValue), _first.enumWithDefault);

            Undo.PerformUndo();

            Assert.AreEqual(default(Overridable<OverridableTestEnum>), _first.enumWithDefault);
        }

        [Test]
        public void TryGetDefaultValue_ReadsStaticMembersAndRejectsMissingOnes()
        {
            var attribute = GetAttribute(nameof(OverridableTestAsset.boolWithDefault));
            var missing = new OverridableDefaultAttribute(typeof(OverridableTestDefaults), "Missing");

            Assert.IsTrue(OverridableEditorAPI.TryGetDefaultValue(attribute, out var value));
            Assert.AreEqual(expected: true, actual: value);
            Assert.IsFalse(OverridableEditorAPI.TryGetDefaultValue(missing, out _));
            Assert.IsFalse(OverridableEditorAPI.TryGetDefaultValue(attribute: null, value: out _));
        }

        [TestCase(typeof(ThrowingDefaultSource))]
        [TestCase(typeof(UnavailableSettings))]
        public void TryGetDefaultValue_FailedAcquisitionReturnsFalse(Type sourceType)
        {
            var attribute = new OverridableDefaultAttribute(sourceType, "IntValue");

            Assert.IsFalse(OverridableEditorAPI.TryGetDefaultValue(attribute, out var value));
            Assert.IsNull(value);
        }

        [TestCase(typeof(ThrowingDefaultSource))]
        [TestCase(typeof(UnavailableSettings))]
        public void GetShownDefault_FailedAcquisitionUsesTypeFallback(Type sourceType)
        {
            var attribute = new OverridableDefaultAttribute(sourceType, "IntValue");

            Assert.AreEqual(expected: 0, actual: OverridableEditorAPI.GetShownDefault(attribute, typeof(int)));
        }

        [TestCase(typeof(ThrowingDefaultSource), "From: ThrowingDefaultSource ▸ IntValue")]
        [TestCase(typeof(UnavailableSettings), "From: Project Settings > UnavailableSettings > Int Value")]
        public void GetDefaultTooltip_FailedAcquisitionShowsFallback(Type sourceType, string sourceLine)
        {
            var attribute = new OverridableDefaultAttribute(sourceType, "IntValue");

            Assert.AreEqual(
                  $"Default value: 0\n{sourceLine}"
                , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(int))
            );
        }

        [TestCase(typeof(ThrowingDefaultSource))]
        [TestCase(typeof(UnavailableSettings))]
        public void BeginOverride_FailedAcquisitionCopiesFallbackAndUndoRestoresStoredValue(Type sourceType)
        {
            _first.intValue = new(value: 9, isOverridden: false);
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));
            var attribute = new OverridableDefaultAttribute(sourceType, "IntValue");

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(int));

            Assert.IsTrue(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 0, actual: _first.intValue.value);

            Undo.PerformUndo();

            Assert.IsFalse(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 9, actual: _first.intValue.value);
        }

        [Test]
        public void OpenGenericDrawer_IsResolvedForOverridableFields()
        {
            using var serializedObject = new SerializedObject(_first);
            var toggleProperty = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));
            var dropdownProperty = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));
            var utility = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
            Assert.IsNotNull(utility);
            var getHandler = utility.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(getHandler);

            Assert.IsInstanceOf<OverridablePropertyDrawer>(GetDrawer(toggleProperty));
            Assert.IsInstanceOf<OverridablePropertyDrawer>(GetDrawer(dropdownProperty));

            object GetDrawer(SerializedProperty property)
            {
                var handler = getHandler.Invoke(obj: null, parameters: new object[] { property });
                Assert.IsNotNull(handler);

                var drawer = handler.GetType().GetProperty(
                      "propertyDrawer"
                    , BindingFlags.Instance | BindingFlags.NonPublic
                );

                Assert.IsNotNull(drawer);
                return drawer.GetValue(handler);
            }
        }

        [Test]
        public void GetDisplayText_FormatsScalarsAndNull()
        {
            Assert.AreEqual("True", OverridableEditorAPI.GetDisplayText(valueType: typeof(bool), value: true));
            Assert.AreEqual("False", OverridableEditorAPI.GetDisplayText(valueType: typeof(bool), value: false));
            Assert.AreEqual(
                  "Second Choice"
                , OverridableEditorAPI.GetDisplayText(typeof(OverridableTestEnum), OverridableTestEnum.Second)
            );

            var flags = OverridableTestFlags.A | OverridableTestFlags.B;
            Assert.AreEqual(
                  flags.ToString()
                , OverridableEditorAPI.GetDisplayText(typeof(OverridableTestFlags), flags)
            );
            Assert.AreEqual(
                  string.Empty
                , OverridableEditorAPI.GetDisplayText(valueType: typeof(string), value: null)
            );
        }

        [Test]
        public void GetDisplayText_FormatsListsAndOverridableElements()
        {
            var values = new List<Overridable<int>> {
                new(value: 99, isOverridden: false),
                new(value: 3, isOverridden: true),
            };

            var waves = TestDefaults.SpawnWaves;
            var steps = OverridableTestDefaults.NestedArrayValue;
            Assert.AreEqual("[1, 2, 4]", OverridableEditorAPI.GetDisplayText(waves.GetType(), waves));
            Assert.AreEqual("[2, Default]", OverridableEditorAPI.GetDisplayText(steps.GetType(), steps));

            Assert.AreEqual("[Default, 3]", OverridableEditorAPI.GetDisplayText(values.GetType(), values));
            Assert.AreEqual(
                  "[True, False]"
                , OverridableEditorAPI.GetDisplayText(typeof(bool[]), new[] { true, false })
            );
            Assert.AreEqual("[]", OverridableEditorAPI.GetDisplayText(typeof(int[]), Array.Empty<int>()));
            Assert.AreEqual("Default", OverridableEditorAPI.GetDisplayText(typeof(Overridable<int>), values[0]));
            Assert.AreEqual("3", OverridableEditorAPI.GetDisplayText(typeof(Overridable<int>), values[1]));
        }

        [Test]
        public void GetShownDefault_ReadsMembersAndCreatesTypeFallbacks()
        {
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.IntValue));
            var missing = DefaultAttribute("Missing");

            Assert.AreEqual(expected: 3, actual: OverridableEditorAPI.GetShownDefault(attribute, typeof(int)));
            Assert.AreEqual(expected: 0, actual: OverridableEditorAPI.GetShownDefault(missing, typeof(int)));

            Assert.AreEqual(
                  expected: false
                , actual: OverridableEditorAPI.GetShownDefault(attribute: null, valueType: typeof(bool))
            );

            Assert.AreEqual(
                  string.Empty
                , OverridableEditorAPI.GetShownDefault(attribute: null, valueType: typeof(string))
            );

            Assert.IsNull(OverridableEditorAPI.GetShownDefault(attribute: null, valueType: typeof(GameObject)));
            Assert.IsInstanceOf<OverridableTestPayload>(OverridableEditorAPI.GetShownDefault(
                  attribute: null
                , valueType: typeof(OverridableTestPayload)
            ));

            var list = OverridableEditorAPI.GetShownDefault(attribute: null, valueType: typeof(List<int>));
            var array = OverridableEditorAPI.GetShownDefault(attribute: null, valueType: typeof(int[]));
            Assert.IsInstanceOf<List<int>>(list);
            Assert.IsInstanceOf<int[]>(array);
            CollectionAssert.IsEmpty((List<int>)list);
            CollectionAssert.IsEmpty((int[])array);
            Assert.AreNotSame(
                  list
                , OverridableEditorAPI.GetShownDefault(attribute: null, valueType: typeof(List<int>))
            );
        }

        [Test]
        public void GetDefaultTooltip_FormatsStaticSettingsAndMissingSources()
        {
            var attribute = new OverridableDefaultAttribute(
                  typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
            );
            Assert.AreEqual(
                  "Default value: 3\nFrom: TestDefaults ▸ RetryCount"
                , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(int))
            );

            using var settingsScope = new OverridableTestSettingsScope();
            var settings = settingsScope.Settings;
            var previous = settings.maxRetries;
            settings.maxRetries = 5;

            try
            {
                attribute = new(typeof(OverridableTestSettings), nameof(OverridableTestSettings.maxRetries));

                Assert.AreEqual(
                      "Default value: 5\nFrom: Project Settings > Encosy Tower > Tests > Overridables > Max Retries"
                    , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(int))
                );
            }
            finally
            {
                settings.maxRetries = previous;
            }

            Assert.AreEqual(
                  "Default value: 0\nNo [OverridableDefault]: uses the type's default value."
                , OverridableEditorAPI.GetDefaultTooltip(attribute: null, valueType: typeof(int))
            );
        }

        [Test]
        public void GetDefaultTooltip_FormatsStringsObjectsClassesAndLists()
        {
            var attribute = new OverridableDefaultAttribute(
                  typeof(TestDefaults)
                , nameof(TestDefaults.DisplayName)
            );

            Assert.AreEqual(
                  "Default value: \"Player\"\nFrom: TestDefaults ▸ DisplayName"
                , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(string))
            );

            Assert.AreEqual(
                  "Default value: None\nNo [OverridableDefault]: uses the type's default value."
                , OverridableEditorAPI.GetDefaultTooltip(attribute: null, valueType: typeof(GameObject))
            );

            attribute = DefaultAttribute(nameof(OverridableTestDefaults.PayloadValue));

            Assert.AreEqual(
                  "Default value: OverridableTestPayload\nFrom: OverridableTestDefaults ▸ PayloadValue"
                , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(OverridableTestPayload))
            );

            attribute = DefaultAttribute(nameof(OverridableTestDefaults.ListValue));

            Assert.AreEqual(
                  "Default value: [3, 5]\nFrom: OverridableTestDefaults ▸ ListValue"
                , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(List<int>))
            );

            var defaultObject = new GameObject("Default Object");
            OverridableTestDefaults.ObjectValue = defaultObject;

            try
            {
                attribute = DefaultAttribute(nameof(OverridableTestDefaults.ObjectValue));

                Assert.AreEqual(
                      "Default value: Default Object (GameObject)\nFrom: OverridableTestDefaults ▸ ObjectValue"
                    , OverridableEditorAPI.GetDefaultTooltip(attribute, typeof(GameObject))
                );
            }
            finally
            {
                OverridableTestDefaults.ObjectValue = null;
                Object.DestroyImmediate(defaultObject);
            }
        }

        [Test]
        public void BeginOverride_IntCopiesDefaultAndUndoRestoresBothFields()
        {
            _first.intValue = new(value: 9, isOverridden: false);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.IntValue));

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(int));

            Assert.IsTrue(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 3, actual: _first.intValue.value);

            Undo.PerformUndo();

            Assert.IsFalse(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 9, actual: _first.intValue.value);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BeginOverride_GuidCopiesDefaultAndUndoRestoresEachTarget(bool multipleTargets)
        {
            var firstStored = new SerializableGuid(new Guid("11111111-2222-4333-8444-555555555555"));
            var secondStored = new SerializableGuid(new Guid("aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeeeeeee"));
            _first.guidValue = new(value: firstStored, isOverridden: false);
            _second.guidValue = new(value: secondStored, isOverridden: false);

            using var serializedObject = multipleTargets
                ? new SerializedObject(new Object[] { _first, _second })
                : new SerializedObject(_first);

            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.guidValue));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.GuidValue));
            var expected = OverridableTestDefaults.GuidValue;
            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(SerializableGuid));

            Assert.IsTrue(_first.guidValue.isOverridden);
            Assert.AreEqual(expected, _first.guidValue.value);
            Assert.AreEqual(multipleTargets, _second.guidValue.isOverridden);
            Assert.AreEqual(multipleTargets ? expected : secondStored, _second.guidValue.value);

            using var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            SerializableGuid actual = default;
            Assert.IsTrue(actual.TryCopyFrom(value));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, ValueChoicesPropertyDrawer.ReadValue(value, typeof(SerializableGuid)));
            Assert.AreEqual(expected.ToString(), OverridableEditorAPI.GetEffectiveText(
                  property
                , attribute
                , typeof(SerializableGuid)
            ));

            Undo.PerformUndo();
            serializedObject.Update();
            Assert.IsFalse(_first.guidValue.isOverridden);
            Assert.AreEqual(firstStored, _first.guidValue.value);
            Assert.IsFalse(_second.guidValue.isOverridden);
            Assert.AreEqual(secondStored, _second.guidValue.value);

            using var writableValue = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            Undo.IncrementCurrentGroup();
            ValueChoicesPropertyDrawer.WriteValue(writableValue, expected);
            Assert.IsTrue(serializedObject.hasModifiedProperties);
            Assert.AreEqual(firstStored, _first.guidValue.value);
            serializedObject.ApplyModifiedProperties();
            Assert.AreEqual(expected, _first.guidValue.value);
            Assert.IsFalse(_first.guidValue.isOverridden);
            Assert.AreEqual(multipleTargets ? expected : secondStored, _second.guidValue.value);
            Undo.PerformUndo();
            serializedObject.Update();
            Assert.AreEqual(firstStored, _first.guidValue.value);
            Assert.AreEqual(secondStored, _second.guidValue.value);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BeginOverride_GuidListCopiesDefaultsAndUndoRestoresEachCollection(bool multipleTargets)
        {
            var firstStored = new[] { SerializableGuid.Empty };
            var secondStored = new[] { SerializableGuid.Empty, SerializableGuid.Empty, SerializableGuid.Empty };
            _first.guidList = new(value: new(firstStored), isOverridden: false);
            _second.guidList = new(value: new(secondStored), isOverridden: false);

            using var serializedObject = multipleTargets
                ? new SerializedObject(new Object[] { _first, _second })
                : new SerializedObject(_first);

            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.guidList));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.GuidListValue));
            var expected = OverridableTestDefaults.GuidListValue;
            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(List<SerializableGuid>));

            Assert.IsTrue(_first.guidList.isOverridden);
            CollectionAssert.AreEqual(expected, _first.guidList.value);
            Assert.AreEqual(multipleTargets, _second.guidList.isOverridden);
            CollectionAssert.AreEqual(multipleTargets ? expected.ToArray() : secondStored, _second.guidList.value);

            using var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var readBack = (List<SerializableGuid>)ValueChoicesPropertyDrawer.ReadValue(
                  value
                , typeof(List<SerializableGuid>)
            );

            CollectionAssert.AreEqual(expected, readBack);
            Assert.AreNotSame(_first.guidList.value, readBack);
            readBack[0] = SerializableGuid.Empty;
            CollectionAssert.AreEqual(expected, _first.guidList.value);

            Undo.PerformUndo();
            serializedObject.Update();
            Assert.IsFalse(_first.guidList.isOverridden);
            CollectionAssert.AreEqual(firstStored, _first.guidList.value);
            Assert.IsFalse(_second.guidList.isOverridden);
            CollectionAssert.AreEqual(secondStored, _second.guidList.value);

            Undo.PerformRedo();
            serializedObject.Update();
            Assert.IsTrue(_first.guidList.isOverridden);
            CollectionAssert.AreEqual(expected, _first.guidList.value);
            Assert.AreEqual(multipleTargets, _second.guidList.isOverridden);
            CollectionAssert.AreEqual(multipleTargets ? expected.ToArray() : secondStored, _second.guidList.value);
            Undo.PerformUndo();
            serializedObject.Update();
            Assert.IsFalse(_first.guidList.isOverridden);
            CollectionAssert.AreEqual(firstStored, _first.guidList.value);
            Assert.IsFalse(_second.guidList.isOverridden);
            CollectionAssert.AreEqual(secondStored, _second.guidList.value);

            using var writableValue = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            Undo.IncrementCurrentGroup();
            ValueChoicesPropertyDrawer.WriteValue(writableValue, expected);
            Assert.IsTrue(serializedObject.hasModifiedProperties);
            Assert.AreEqual(firstStored[0], _first.guidList.value[0]);
            Assert.AreEqual(secondStored[0], _second.guidList.value[0]);

            CollectionAssert.AreEqual(
                  expected
                , (List<SerializableGuid>)ValueChoicesPropertyDrawer.ReadValue(
                      writableValue
                    , typeof(List<SerializableGuid>)
                )
            );

            Assert.IsTrue(serializedObject.hasModifiedProperties);
            serializedObject.ApplyModifiedProperties();
            CollectionAssert.AreEqual(expected, _first.guidList.value);
            Assert.IsFalse(_first.guidList.isOverridden);
            CollectionAssert.AreEqual(multipleTargets ? expected.ToArray() : secondStored, _second.guidList.value);
            Undo.PerformUndo();
            serializedObject.Update();
            CollectionAssert.AreEqual(firstStored, _first.guidList.value);
            CollectionAssert.AreEqual(secondStored, _second.guidList.value);
            Undo.PerformRedo();
            serializedObject.Update();
            Assert.IsFalse(_first.guidList.isOverridden);
            CollectionAssert.AreEqual(expected, _first.guidList.value);
            Assert.IsFalse(_second.guidList.isOverridden);
            CollectionAssert.AreEqual(multipleTargets ? expected.ToArray() : secondStored, _second.guidList.value);
            Undo.PerformUndo();
            serializedObject.Update();
            CollectionAssert.AreEqual(firstStored, _first.guidList.value);
            CollectionAssert.AreEqual(secondStored, _second.guidList.value);

            var shorter = new List<SerializableGuid> { expected[0] };
            using var shorterValue = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            Undo.IncrementCurrentGroup();
            ValueChoicesPropertyDrawer.WriteValue(shorterValue, shorter);
            serializedObject.ApplyModifiedProperties();
            CollectionAssert.AreEqual(shorter, _first.guidList.value);
            CollectionAssert.AreEqual(multipleTargets ? shorter.ToArray() : secondStored, _second.guidList.value);
            Undo.PerformUndo();
            serializedObject.Update();
            CollectionAssert.AreEqual(firstStored, _first.guidList.value);
            CollectionAssert.AreEqual(secondStored, _second.guidList.value);
        }

        [Test]
        public void BeginOverride_ObjectCopiesReferenceAndUndoRestoresBothFields()
        {
            var originalObject = new GameObject("Original Object");
            var defaultObject = new GameObject("Default Object");
            _first.objectValue = new(value: originalObject, isOverridden: false);
            OverridableTestDefaults.ObjectValue = defaultObject;

            try
            {
                using var serializedObject = new SerializedObject(_first);
                var property = serializedObject.FindProperty(nameof(OverridableTestAsset.objectValue));
                var attribute = DefaultAttribute(nameof(OverridableTestDefaults.ObjectValue));

                Undo.IncrementCurrentGroup();
                OverridableEditorAPI.BeginOverride(property, attribute, typeof(GameObject));

                Assert.IsTrue(_first.objectValue.isOverridden);
                Assert.AreSame(defaultObject, _first.objectValue.value);

                Undo.PerformUndo();

                Assert.IsFalse(_first.objectValue.isOverridden);
                Assert.AreSame(originalObject, _first.objectValue.value);
            }
            finally
            {
                OverridableTestDefaults.ObjectValue = null;
                Object.DestroyImmediate(originalObject);
                Object.DestroyImmediate(defaultObject);
            }
        }

        [Test]
        public void BeginOverride_ClassCopiesDefaultAndUndoRestoresBothFields()
        {
            _first.payloadValue = new(value: new() { number = 9, text = "Stored" }, isOverridden: false);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.payloadValue));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.PayloadValue));

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(OverridableTestPayload));

            Assert.IsTrue(_first.payloadValue.isOverridden);
            Assert.AreEqual(expected: 7, actual: _first.payloadValue.value.number);
            Assert.AreEqual("Default", _first.payloadValue.value.text);

            Undo.PerformUndo();

            Assert.IsFalse(_first.payloadValue.isOverridden);
            Assert.AreEqual(expected: 9, actual: _first.payloadValue.value.number);
            Assert.AreEqual("Stored", _first.payloadValue.value.text);
        }

        [Test]
        public void BeginOverride_ListCopiesElementsAndUndoRestoresBothFields()
        {
            _first.listValue = new(value: new() { 9, 8, 7 }, isOverridden: false);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.listValue));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.ListValue));

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(List<int>));

            Assert.IsTrue(_first.listValue.isOverridden);
            CollectionAssert.AreEqual(new[] { 3, 5 }, _first.listValue.value);

            Undo.PerformUndo();

            Assert.IsFalse(_first.listValue.isOverridden);
            CollectionAssert.AreEqual(new[] { 9, 8, 7 }, _first.listValue.value);
        }

        [Test]
        public void BeginOverride_ArrayCopiesElementsAndUndoRestoresBothFields()
        {
            _first.arrayValue = new(value: new[] { 9, 8, 7 }, isOverridden: false);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.arrayValue));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.ArrayValue));

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(int[]));

            Assert.IsTrue(_first.arrayValue.isOverridden);
            CollectionAssert.AreEqual(new[] { 3, 5 }, _first.arrayValue.value);

            Undo.PerformUndo();

            Assert.IsFalse(_first.arrayValue.isOverridden);
            CollectionAssert.AreEqual(new[] { 9, 8, 7 }, _first.arrayValue.value);
        }

        [Test]
        public void BeginOverride_NestedArrayCopiesElementValuesAndFlagsAndUndoRestoresStoredArray()
        {
            _first.nestedIntArray = new(
                  value: new[] { new Overridable<int>(value: 12, isOverridden: true) }
                , isOverridden: false
            );

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.nestedIntArray));
            var attribute = DefaultAttribute(nameof(OverridableTestDefaults.NestedArrayValue));

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property, attribute, typeof(Overridable<int>[]));

            Assert.IsTrue(_first.nestedIntArray.isOverridden);
            Assert.AreEqual(expected: 2, actual: _first.nestedIntArray.value.Length);
            Assert.AreEqual(expected: 2, actual: _first.nestedIntArray.value[0].value);
            Assert.IsTrue(_first.nestedIntArray.value[0].isOverridden);
            Assert.AreEqual(expected: 9, actual: _first.nestedIntArray.value[1].value);
            Assert.IsFalse(_first.nestedIntArray.value[1].isOverridden);

            Undo.PerformUndo();

            Assert.IsFalse(_first.nestedIntArray.isOverridden);
            Assert.AreEqual(expected: 1, actual: _first.nestedIntArray.value.Length);
            Assert.AreEqual(expected: 12, actual: _first.nestedIntArray.value[0].value);
            Assert.IsTrue(_first.nestedIntArray.value[0].isOverridden);
        }

        [Test]
        public void IsOverridableListElement_DetectsNestedListsAndResolvesElementTypes()
        {
            _first.nestedList = new(value: new() { new(value: 4, isOverridden: true) }, isOverridden: true);
            var values = new[] { new Overridable<bool>(value: true, isOverridden: false) };
            _first.nestedArray = new(value: values, isOverridden: false);

            using var serializedObject = new SerializedObject(_first);
            var listField = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.nestedList));
            var arrayField = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.nestedArray));
            var listProperty = serializedObject.FindProperty(listField.Name);
            var arrayProperty = serializedObject.FindProperty(arrayField.Name);
            var listElement = listProperty.FindPropertyRelative(OverridableEditorAPI.VALUE).GetArrayElementAtIndex(0);
            var arrayValue = arrayProperty.FindPropertyRelative(OverridableEditorAPI.VALUE);
            var arrayElement = arrayValue.GetArrayElementAtIndex(0);

            Assert.IsTrue(OverridableEditorAPI.IsOverridableListElement(listElement, listField));
            Assert.IsTrue(OverridableEditorAPI.IsOverridableListElement(arrayElement, arrayField));
            Assert.AreEqual(typeof(int), OverridableEditorAPI.GetValueType(listField, listElement));
            Assert.AreEqual(typeof(bool), OverridableEditorAPI.GetValueType(arrayField, arrayElement));
            Assert.IsFalse(OverridableEditorAPI.IsOverridableListElement(listProperty, listField));
            Assert.IsFalse(OverridableEditorAPI.IsOverridableListElement(arrayProperty, arrayField));
            Assert.AreEqual(
                  typeof(List<Overridable<int>>)
                , OverridableEditorAPI.GetValueType(listField, listProperty)
            );
            Assert.AreEqual(
                  typeof(Overridable<bool>[])
                , OverridableEditorAPI.GetValueType(arrayField, arrayProperty)
            );
            Assert.IsFalse(OverridableEditorAPI.IsOverridableListElement(
                  listElement.FindPropertyRelative(OverridableEditorAPI.VALUE)
                , listField
            ));
        }

        [Test]
        public void IsOverridableListElement_RejectsPlainCollectionsAndPreservesOrdinaryElementTypes()
        {
            _first.listValue = new(value: new() { 4 }, isOverridden: false);
            _first.ordinaryArray = new[] { new Overridable<int>(value: 4, isOverridden: false) };
            _first.ordinaryList = new() { new(value: 4, isOverridden: false) };

            using var serializedObject = new SerializedObject(_first);
            var field = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.listValue));
            var outer = serializedObject.FindProperty(field.Name);
            var element = outer.FindPropertyRelative(OverridableEditorAPI.VALUE).GetArrayElementAtIndex(0);

            Assert.IsFalse(OverridableEditorAPI.IsOverridableListElement(element, field));
            Assert.AreEqual(typeof(List<int>), OverridableEditorAPI.GetValueType(field, outer));
            Assert.AreEqual(typeof(List<int>), OverridableEditorAPI.GetValueType(field, element));

            field = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.ordinaryArray));
            element = serializedObject.FindProperty(field.Name).GetArrayElementAtIndex(0);

            Assert.IsFalse(OverridableEditorAPI.IsOverridableListElement(element, field));
            Assert.AreEqual(typeof(int), OverridableEditorAPI.GetValueType(field, element));

            field = typeof(OverridableTestAsset).GetField(nameof(OverridableTestAsset.ordinaryList));
            element = serializedObject.FindProperty(field.Name).GetArrayElementAtIndex(0);

            Assert.IsFalse(OverridableEditorAPI.IsOverridableListElement(element, field));
            Assert.AreEqual(typeof(int), OverridableEditorAPI.GetValueType(field, element));
        }

        [Test]
        public void TryFindSourceScript_FindsTestDefaultsInDifferentlyNamedFile()
        {
            Assert.IsTrue(OverridableEditorAPI.TryFindSourceScript(
                  typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var script
                , out var line
            ));

            StringAssert.EndsWith("/OverridableChoiceTestAsset.cs", AssetDatabase.GetAssetPath(script));
            Assert.AreEqual("public static int RetryCount = 3;", script.text.Split('\n')[line - 1].Trim());
        }

        [TestCase(typeof(GenericDefaults<int>), "Int32")]
        [TestCase(typeof(GenericDefaults<string>), "String")]
        [TestCase(typeof(GenericDefaults<int>.Nested<string>), "Int32/String")]
        [TestCase(typeof(GenericDefaults<string>.Nested<int>), "String/Int32")]
        public void TryFindSourceScript_ConstructedGenericsKeepTheirDefaultReadingType(Type sourceType, string expected)
        {
            var attribute = new OverridableDefaultAttribute(sourceType, nameof(GenericDefaults<int>.TypeName));

            Assert.IsTrue(OverridableEditorAPI.TryFindSourceScript(
                  sourceType
                , attribute.MemberName
                , out var script
                , out var line
            ));

            StringAssert.EndsWith("/OverridableEditorAPITests.cs", AssetDatabase.GetAssetPath(script));
            StringAssert.StartsWith("public static string TypeName =>", script.text.Split('\n')[line - 1].Trim());
            Assert.IsTrue(OverridableEditorAPI.TryGetDefaultValue(attribute, out var value));
            Assert.AreEqual(expected, value);
            Assert.AreEqual(expected, OverridableEditorAPI.GetShownDefault(attribute, typeof(string)));
        }

        [TestCase(typeof(GenericDefaults<int>))]
        [TestCase(typeof(GenericDefaults<string>))]
        public void TryGetDeclaredMemberLine_ConstructedGenericMatchesItsDeclaration(Type sourceType)
        {
            var text = $"namespace {sourceType.Namespace}\n"
                + "{\n"
                + "    class OverridableEditorAPITests\n"
                + "    {\n"
                + "        class Other { public static string TypeName => null; }\n"
                + "        class GenericDefaults<T>\n"
                + "        {\n"
                + "            public static string TypeName => typeof(T).Name;\n"
                + "        }\n"
                + "    }\n"
                + "}";

            Assert.IsTrue(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , sourceType
                , nameof(GenericDefaults<int>.TypeName)
                , out var line
            ));

            Assert.AreEqual(expected: 8, actual: line);
        }

        [TestCase(typeof(GenericDefaults<int>.Nested<string>))]
        [TestCase(typeof(GenericDefaults<string>.Nested<int>))]
        public void TryGetDeclaredMemberLine_ConstructedNestedGenericKeepsItsDeclaringScope(Type sourceType)
        {
            var text = $"namespace {sourceType.Namespace}\n"
                + "{\n"
                + "    class OverridableEditorAPITests\n"
                + "    {\n"
                + "        class Other<T> { class Nested<U> { public static string TypeName => null; } }\n"
                + "        class GenericDefaults<T>\n"
                + "        {\n"
                + "            public static string TypeName => null;\n"
                + "            class Nested<U>\n"
                + "            {\n"
                + "                public static string TypeName => typeof(U).Name;\n"
                + "            }\n"
                + "        }\n"
                + "    }\n"
                + "}";

            Assert.IsTrue(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , sourceType
                , nameof(GenericDefaults<int>.Nested<string>.TypeName)
                , out var line
            ));

            Assert.AreEqual(expected: 11, actual: line);
        }

        [Test]
        public void TryGetDeclaredMemberLine_ConstructedNestedGenericRejectsAnAdjacentScope()
        {
            var sourceType = typeof(GenericDefaults<int>.Nested<string>);
            var text = $"namespace {sourceType.Namespace}\n"
                + "{\n"
                + "    class OverridableEditorAPITests\n"
                + "    {\n"
                + "        class GenericDefaults<T> { class Nested<U> { } }\n"
                + "        class Other<T> { class Nested<U> { public static string TypeName => null; } }\n"
                + "    }\n"
                + "}";

            Assert.IsFalse(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , sourceType
                , nameof(GenericDefaults<int>.Nested<string>.TypeName)
                , out var line
            ));

            Assert.AreEqual(expected: 0, actual: line);
        }

        [TestCase("class")]
        [TestCase("struct")]
        [TestCase("record")]
        [TestCase("record class")]
        [TestCase("record struct")]
        [TestCase("interface")]
        public void TryGetDeclaredMemberLine_FindsSecondTypeAndSkipsEarlierMember(string declaration)
        {
            var text = $"namespace {typeof(TestDefaults).Namespace}\n"
                + "{\n"
                + "    class Other { public static int RetryCount = 1; }\n"
                + $"    public {declaration} TestDefaults\n"
                + "    {\n"
                + "        public static int RetryCount = 3;\n"
                + "    }\n"
                + "}";

            Assert.IsTrue(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 6, actual: line);
        }

        [Test]
        public void TryGetDeclaredMemberLine_AcceptsFileScopedNamespace()
        {
            var text = $"namespace {typeof(TestDefaults).Namespace};\n"
                + "public static class TestDefaults { public static int RetryCount = 3; }";

            Assert.IsTrue(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 2, actual: line);
        }

        [TestCase("Other.Namespace", "class TestDefaults", "RetryCount")]
        [TestCase("EncosyTower.Tests.Editor.Serialization.Extra", "class TestDefaults", "RetryCount")]
        [TestCase("EncosyTower.Tests.Editor.Serialization", "class TestDefaultsExtra", "RetryCount")]
        [TestCase("EncosyTower.Tests.Editor.Serialization", "class Other", "RetryCount")]
        [TestCase("EncosyTower.Tests.Editor.Serialization", "class TestDefaults", "RetryCountExtra")]
        public void TryGetDeclaredMemberLine_RejectsOtherNamespaceTypeOrMember(
              string sourceNamespace
            , string declaration
            , string memberName
        )
        {
            var text = $"namespace {sourceNamespace}\n{{\n"
                + $"    public {declaration} {{ public static int {memberName} = 3; }}\n"
                + "}";

            Assert.IsFalse(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 0, actual: line);
        }

        [Test]
        public void TryGetDeclaredMemberLine_RejectsMemberInAdjacentType()
        {
            var text = $"namespace {typeof(TestDefaults).Namespace}\n"
                + "{\n"
                + "    class TestDefaults { public static int Other = 1; }\n"
                + "    class Other { public static int RetryCount = 3; }\n"
                + "}";

            Assert.IsFalse(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 0, actual: line);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TryGetDeclaredMemberLine_KeepsNamespaceAndTypeTogether(bool matchingNamespaceFirst)
        {
            var matching = $"namespace {typeof(TestDefaults).Namespace} {{ class Other {{ }} }}\n";
            const string OTHER = "namespace Other { class TestDefaults { public static int RetryCount = 3; } }\n";
            var text = matchingNamespaceFirst ? matching + OTHER : OTHER + matching;

            Assert.IsFalse(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 0, actual: line);
        }

        [Test]
        public void TryGetDeclaredMemberLine_FindsMemberInLaterPartialDeclaration()
        {
            var text = $"namespace {typeof(TestDefaults).Namespace}\n"
                + "{\n"
                + "    partial class TestDefaults { public static int Other = 1; }\n"
                + "    class Other { public static int RetryCount = 2; }\n"
                + "    partial class TestDefaults\n"
                + "    {\n"
                + "        public static int RetryCount = 3;\n"
                + "    }\n"
                + "}";

            Assert.IsTrue(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 7, actual: line);
        }

        [Test]
        public void TryGetDeclaredMemberLine_SkipsNestedTypeCommentsAndLiteralBraces()
        {
            var text = $"namespace {typeof(TestDefaults).Namespace}\n"
                + "{\n"
                + "    class TestDefaults\n"
                + "    {\n"
                + "        class Other { public static int RetryCount = 1; }\n"
                + "        /* } public static int RetryCount = 2; */\n"
                + "        public static string Text = \"} public static int RetryCount = 2;\";\n"
                + "        public static int RetryCount = 3;\n"
                + "    }\n"
                + "}";

            Assert.IsTrue(OverridableEditorAPI.TryGetDeclaredMemberLine(
                  text
                , typeof(TestDefaults)
                , nameof(TestDefaults.RetryCount)
                , out var line
            ));

            Assert.AreEqual(expected: 8, actual: line);
        }

        [Test]
        public void GetNoDefaultMessage_ExplainsHowToSetSpeedDefault()
        {
            const string EXPECTED = "Speed: defaults to 0 (type default)\n"
                + "To set a custom default, annotate Speed with [OverridableDefault].";

            Assert.AreEqual(EXPECTED, OverridableEditorAPI.GetNoDefaultMessage("Speed", typeof(int)));
        }

        [TestCase(typeof(bool), "False")]
        [TestCase(typeof(string), "empty text")]
        [TestCase(typeof(GameObject), "None")]
        [TestCase(typeof(OverridableTestEnum), "First")]
        [TestCase(typeof(OverridableTestFlags), "None")]
        [TestCase(typeof(Vector3), "(0, 0, 0)")]
        [TestCase(typeof(Color), "transparent black")]
        [TestCase(typeof(int[]), "[]")]
        [TestCase(typeof(List<int>), "[]")]
        public void GetNoDefaultMessage_UsesFriendlyTypeDefault(Type valueType, string friendlyValue)
        {
            var expected = $"Value: defaults to {friendlyValue} (type default)\n"
                + "To set a custom default, annotate Value with [OverridableDefault].";

            Assert.AreEqual(expected, OverridableEditorAPI.GetNoDefaultMessage("Value", valueType));
        }

        [TestCase(typeof(int), "0")]
        [TestCase(typeof(bool), "False")]
        [TestCase(typeof(string), "empty text")]
        [TestCase(typeof(GameObject), "None")]
        [TestCase(typeof(OverridableTestEnum), "First")]
        [TestCase(typeof(OverridableTestFlags), "None")]
        [TestCase(typeof(Vector3), "(0, 0, 0)")]
        [TestCase(typeof(Color), "transparent black")]
        [TestCase(typeof(int[]), "[]")]
        [TestCase(typeof(List<int>), "[]")]
        public void GetElementNoDefaultMessage_UsesFriendlyTypeDefault(Type valueType, string friendlyValue)
        {
            var expected = $"Tiers Element 2: defaults to {friendlyValue} (type default)\n"
                + "Elements have no default of their own; [OverridableDefault] on Tiers sets the whole array.";

            Assert.AreEqual(expected, OverridableEditorAPI.GetElementNoDefaultMessage(
                  fieldLabel: "Tiers"
                , valueType: valueType
                , elementIndex: 2
            ));
        }

        [Test]
        public void GetEffectiveText_BoolUsesTrueAndFalse()
        {
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.boolWithDefault));
            var attribute = GetAttribute(nameof(OverridableTestAsset.boolWithDefault));

            Assert.AreEqual("True", OverridableEditorAPI.GetEffectiveText(property, attribute, typeof(bool)));

            _first.boolWithDefault = new(value: false, isOverridden: true);
            serializedObject.Update();

            Assert.AreEqual("False", OverridableEditorAPI.GetEffectiveText(property, attribute, typeof(bool)));
        }

        [Test]
        public void BeginOverride_WithoutAttributeCopiesTypeDefault()
        {
            _first.intValue = new(value: 9, isOverridden: false);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));

            Undo.IncrementCurrentGroup();
            OverridableEditorAPI.BeginOverride(property: property, attribute: null, valueType: typeof(int));

            Assert.IsTrue(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 0, actual: _first.intValue.value);

            Undo.PerformUndo();

            Assert.IsFalse(_first.intValue.isOverridden);
            Assert.AreEqual(expected: 9, actual: _first.intValue.value);
        }

        private static OverridableDefaultAttribute DefaultAttribute(string memberName)
            => new(typeof(OverridableTestDefaults), memberName);

        private static Overridable<OverridableTestEnum> Overridden(OverridableTestEnum value)
            => new(value: value, isOverridden: true);

        private static Overridable<OverridableTestEnum> NotOverridden(OverridableTestEnum value)
            => new(value: value, isOverridden: false);

        private static OverridableDefaultAttribute GetAttribute(string fieldName)
            => typeof(OverridableTestAsset).GetField(fieldName).GetCustomAttribute<OverridableDefaultAttribute>();

        private static class GenericDefaults<T>
        {
            public static string TypeName => typeof(T).Name;

            public static class Nested<TNested>
            {
                public static string TypeName => $"{typeof(T).Name}/{typeof(TNested).Name}";
            }
        }

        private static class ThrowingDefaultSource
        {
            public static int IntValue => int.Parse("Unreadable default");
        }

        private abstract class UnavailableSettings : Settings<UnavailableSettings>
        {
            public new static UnavailableSettings Instance
                => (UnavailableSettings)Convert.ChangeType("Unavailable settings", typeof(UnavailableSettings));

            public int IntValue => 5;
        }

        private abstract class TestSettings : Settings<TestSettings>
        {
            public bool flag;
        }
    }
}
