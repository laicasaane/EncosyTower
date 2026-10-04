using System.Reflection;
using EncosyTower.Editor.Serialization;
using EncosyTower.Serialization;
using EncosyTower.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableDrawerModelTests
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
        public void GetMode_UsesDropdownForBoolAndPlainEnums()
        {
            Assert.AreEqual(OverridableMode.Dropdown, OverridableDrawerModel.GetMode(typeof(bool)));
            Assert.AreEqual(OverridableMode.Dropdown, OverridableDrawerModel.GetMode(typeof(OverridableTestEnum)));
            Assert.AreEqual(OverridableMode.Toggle, OverridableDrawerModel.GetMode(typeof(OverridableTestFlags)));
            Assert.AreEqual(OverridableMode.Toggle, OverridableDrawerModel.GetMode(typeof(int)));
        }

        [Test]
        public void GetDefaultLabel_PrefersExplicitThenSettingsThenDefault()
        {
            var explicitLabel = new OverridableDefaultAttribute(typeof(OverridableTestDefaults), "BoolValue") {
                Label = "Global",
            };

            Assert.AreEqual("Global", OverridableDrawerModel.GetDefaultLabel(explicitLabel));
            Assert.AreEqual(
                  "Project Settings"
                , OverridableDrawerModel.GetDefaultLabel(new(typeof(TestSettings), nameof(TestSettings.flag)))
            );
            Assert.AreEqual(
                  "Default"
                , OverridableDrawerModel.GetDefaultLabel(new(typeof(OverridableTestDefaults), "BoolValue"))
            );
            Assert.AreEqual("Default", OverridableDrawerModel.GetDefaultLabel(null));
        }

        [Test]
        public void GetDefaultChoiceLabel_AppendsTheDefaultValueWhenKnown()
        {
            var enumDefault = new OverridableDefaultAttribute(
                  typeof(OverridableTestDefaults)
                , nameof(OverridableTestDefaults.EnumValue)
            );

            var boolDefault = new OverridableDefaultAttribute(
                  typeof(OverridableTestDefaults)
                , nameof(OverridableTestDefaults.BoolValue)
            ) {
                Label = "Global",
            };

            Assert.AreEqual(
                  "Default (Third Value)"
                , OverridableDrawerModel.GetDefaultChoiceLabel(enumDefault, typeof(OverridableTestEnum))
            );
            Assert.AreEqual("Global (On)", OverridableDrawerModel.GetDefaultChoiceLabel(boolDefault, typeof(bool)));
            Assert.AreEqual("Default", OverridableDrawerModel.GetDefaultChoiceLabel(null, typeof(bool)));
        }

        [Test]
        public void GetValueChoices_UsesOffOnAndEnumDisplayNames()
        {
            CollectionAssert.AreEqual(new[] { "Off", "On" }, OverridableDrawerModel.GetValueChoices(typeof(bool)));
            CollectionAssert.AreEqual(
                  new[] { "First", "Second Choice", "Third Value" }
                , OverridableDrawerModel.GetValueChoices(typeof(OverridableTestEnum))
            );
            CollectionAssert.IsEmpty(OverridableDrawerModel.GetValueChoices(typeof(int)));
        }

        [Test]
        public void ApplyChoice_SetsOverrideAndValue()
        {
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));

            OverridableDrawerModel.ApplyChoice(property, typeof(OverridableTestEnum), choiceIndex: 2);

            Assert.AreEqual(Overridden(OverridableTestEnum.Second), _first.enumWithDefault);
            Assert.AreEqual(2, OverridableDrawerModel.GetSelectedIndex(property, typeof(OverridableTestEnum)));

            OverridableDrawerModel.ApplyChoice(property, typeof(OverridableTestEnum), choiceIndex: 0);

            Assert.IsFalse(_first.enumWithDefault.isOverridden);
            Assert.AreEqual(OverridableTestEnum.Second, _first.enumWithDefault.value);
            Assert.AreEqual(0, OverridableDrawerModel.GetSelectedIndex(property, typeof(OverridableTestEnum)));
        }

        [Test]
        public void ApplyChoice_SetsBoolValues()
        {
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.boolWithDefault));

            OverridableDrawerModel.ApplyChoice(property, typeof(bool), choiceIndex: 2);

            Assert.IsTrue(_first.boolWithDefault.isOverridden);
            Assert.IsTrue(_first.boolWithDefault.value);
            Assert.AreEqual(2, OverridableDrawerModel.GetSelectedIndex(property, typeof(bool)));

            OverridableDrawerModel.ApplyChoice(property, typeof(bool), choiceIndex: 1);

            Assert.IsFalse(_first.boolWithDefault.value);
            Assert.AreEqual(1, OverridableDrawerModel.GetSelectedIndex(property, typeof(bool)));
        }

        [Test]
        public void Reset_ClearsTheOverride()
        {
            _first.intValue = new Overridable<int>(4, isOverridden: true);

            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));

            Assert.IsTrue(OverridableDrawerModel.CanReset(property));

            OverridableDrawerModel.Reset(property);

            Assert.IsFalse(_first.intValue.isOverridden);
            Assert.AreEqual(4, _first.intValue.value);
            Assert.IsFalse(OverridableDrawerModel.CanReset(property));
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

            Assert.AreEqual("Third Value", OverridableDrawerModel.GetEffectiveText(withDefault, attribute, enumType));
            Assert.AreEqual(string.Empty, OverridableDrawerModel.GetEffectiveText(withoutDefault, null, enumType));

            OverridableDrawerModel.ApplyChoice(withDefault, enumType, choiceIndex: 2);

            Assert.AreEqual("Second Choice", OverridableDrawerModel.GetEffectiveText(withDefault, attribute, enumType));
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

            Assert.IsTrue(OverridableDrawerModel.IsMixed(property));
            Assert.AreEqual(-1, OverridableDrawerModel.GetSelectedIndex(property, enumType));
            Assert.AreEqual("Mixed", OverridableDrawerModel.GetEffectiveText(property, attribute, enumType));
        }

        [Test]
        public void Undo_AfterApplyChoice_RestoresThePreviousValues()
        {
            using var serializedObject = new SerializedObject(_first);
            var property = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));

            Undo.IncrementCurrentGroup();
            OverridableDrawerModel.ApplyChoice(property, typeof(OverridableTestEnum), choiceIndex: 3);

            Assert.AreEqual(Overridden(OverridableTestEnum.ThirdValue), _first.enumWithDefault);

            Undo.PerformUndo();

            Assert.AreEqual(default(Overridable<OverridableTestEnum>), _first.enumWithDefault);
        }

        [Test]
        public void TryGetDefaultValue_ReadsStaticMembersAndRejectsMissingOnes()
        {
            var attribute = GetAttribute(nameof(OverridableTestAsset.boolWithDefault));
            var missing = new OverridableDefaultAttribute(typeof(OverridableTestDefaults), "Missing");

            Assert.IsTrue(OverridableDrawerModel.TryGetDefaultValue(attribute, out var value));
            Assert.AreEqual(true, value);
            Assert.IsFalse(OverridableDrawerModel.TryGetDefaultValue(missing, out _));
            Assert.IsFalse(OverridableDrawerModel.TryGetDefaultValue(null, out _));
        }

        [Test]
        public void OpenGenericDrawer_IsResolvedForOverridableFields()
        {
            using var serializedObject = new SerializedObject(_first);
            var toggleProperty = serializedObject.FindProperty(nameof(OverridableTestAsset.intValue));
            var dropdownProperty = serializedObject.FindProperty(nameof(OverridableTestAsset.enumWithDefault));
            var toggleHeight = 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Assert.AreEqual(toggleHeight, EditorGUI.GetPropertyHeight(toggleProperty), 0.001f);
            Assert.AreEqual(EditorGUIUtility.singleLineHeight, EditorGUI.GetPropertyHeight(dropdownProperty), 0.001f);
        }

        private static Overridable<OverridableTestEnum> Overridden(OverridableTestEnum value)
            => new(value, isOverridden: true);

        private static Overridable<OverridableTestEnum> NotOverridden(OverridableTestEnum value)
            => new(value, isOverridden: false);

        private static OverridableDefaultAttribute GetAttribute(string fieldName)
            => typeof(OverridableTestAsset).GetField(fieldName).GetCustomAttribute<OverridableDefaultAttribute>();

        private abstract class TestSettings : Settings<TestSettings>
        {
            public bool flag;
        }
    }
}
