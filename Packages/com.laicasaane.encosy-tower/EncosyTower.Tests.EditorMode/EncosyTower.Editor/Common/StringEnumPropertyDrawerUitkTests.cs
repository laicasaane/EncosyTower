#if UNITY_EDITOR

using EncosyTower.Editor;
using EncosyTower.Common;
using EncosyTower.Editor.Internals;
using EncosyTower.Editor.Serialization.Internals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Common
{
    public sealed class StringEnumPropertyDrawerUitkTests
    {
        private static readonly string[] s_kindLabels = { "None", "Fire", "Cold Water", "Wind" };

        private StringEnumTestAsset _asset;
        private SerializedObject _serializedObject;
        private StringEnumTestWindow _window;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<StringEnumTestAsset>();
            _serializedObject = new SerializedObject(_asset);
            _window = ScriptableObject.CreateInstance<StringEnumTestWindow>();
            _window.Show();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window != null)
            {
                _window.Close();
            }

            _serializedObject.Dispose();
            Undo.ClearUndo(_asset);
            Object.DestroyImmediate(_asset);
        }

        [Test]
        public void UnsetValue_ShowsNotSetWithoutWarning()
        {
            var root = Bind(nameof(StringEnumTestAsset.plain));
            var popup = root.Q<ChoicePopupField>();

            Assert.IsNotNull(popup);
            CollectionAssert.AreEqual(s_kindLabels, popup.Choices);
            Assert.AreEqual("(Not set)", GetText(popup));
            Assert.IsFalse(IsWarningShown(root));
        }

        [Test]
        public void Pick_WritesValue()
        {
            var root = Bind(nameof(StringEnumTestAsset.plain));
            var popup = root.Q<ChoicePopupField>();

            popup.value = 1;

            Assert.AreEqual("Fire", _asset.plain.ToString());
            Assert.AreEqual("Fire", GetText(popup));
            Assert.IsFalse(IsWarningShown(root));
        }

        [Test]
        public void Pick_WithConverterWritesConvertedText()
        {
            var root = Bind(nameof(StringEnumTestAsset.converted));
            var popup = root.Q<ChoicePopupField>();

            popup.value = 2;

            Assert.AreEqual("water", _asset.converted.ToString());
            Assert.AreEqual("Cold Water", GetText(popup));
        }

        [Test]
        public void UnlistedValue_WarnsUntilAChoiceIsPicked()
        {
            _asset.plain = new StringEnum<StringEnumTestKind>("Lava");
            _serializedObject.Update();

            var root = Bind(nameof(StringEnumTestAsset.plain));
            var popup = root.Q<ChoicePopupField>();

            Assert.AreEqual("Lava", GetText(popup));
            Assert.IsTrue(IsWarningShown(root));

            popup.value = 1;

            Assert.IsFalse(IsWarningShown(root));
        }

        [Test]
        public void Pick_IsOneUndoStep()
        {
            _asset.plain = new StringEnum<StringEnumTestKind>("Fire");
            _serializedObject.Update();

            var root = Bind(nameof(StringEnumTestAsset.plain));
            var popup = root.Q<ChoicePopupField>();

            Undo.IncrementCurrentGroup();
            popup.value = 3;
            Assert.AreEqual("Wind", _asset.plain.ToString());

            Undo.PerformUndo();
            Assert.AreEqual("Fire", _asset.plain.ToString());
        }

        [Test]
        public void MixedValues_ShowMixedWithoutWarningAndApplyToBoth()
        {
            var second = ScriptableObject.CreateInstance<StringEnumTestAsset>();

            try
            {
                _asset.plain = new StringEnum<StringEnumTestKind>("Fire");
                second.plain = new StringEnum<StringEnumTestKind>("Wind");

                using var multi = new SerializedObject(new Object[] { _asset, second });
                var field = new PropertyField(multi.FindProperty(nameof(StringEnumTestAsset.plain)));
                _window.rootVisualElement.Add(field);
                field.Bind(multi);

                var popup = field.Q<ChoicePopupField>();

                Assert.IsTrue(popup.showMixedValue);
                Assert.IsFalse(IsWarningShown(field));

                popup.value = 0;

                Assert.AreEqual("None", _asset.plain.ToString());
                Assert.AreEqual("None", second.plain.ToString());
            }
            finally
            {
                Undo.ClearUndo(second);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void ListElements_KeepTheirOwnValues()
        {
            _asset.list[0] = new StringEnum<StringEnumTestKind>("Fire");
            _asset.list[1] = new StringEnum<StringEnumTestKind>("Wind");
            _asset.convertedList[1] = new StringEnum<StringEnumTestKind, StringEnumTestConverter>("water");
            _serializedObject.Update();

            Assert.AreEqual("Fire", GetText(Bind("list.Array.data[0]").Q<ChoicePopupField>()));
            Assert.AreEqual("Wind", GetText(Bind("list.Array.data[1]").Q<ChoicePopupField>()));
            Assert.AreEqual("Cold Water", GetText(Bind("convertedList.Array.data[1]").Q<ChoicePopupField>()));
        }

        [Test]
        public void FailingConverter_DrawsTheRawStringField()
        {
            _asset.failing = new StringEnum<StringEnumTestKind, StringEnumTestThrowingConverter>("x");
            _serializedObject.Update();

            StringEnumEditorAPI.ClearCache();
            var type = typeof(StringEnum<StringEnumTestKind, StringEnumTestThrowingConverter>);
            LogAssert.Expect(LogType.Warning, $"StringEnum drawer cannot read the choices of {type.Name}: test failure");

            var root = Bind(nameof(StringEnumTestAsset.failing));

            Assert.IsNull(root.Q<ChoicePopupField>());
            Assert.AreEqual("x", root.Q<TextField>().value);

            Bind(nameof(StringEnumTestAsset.failing));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LockedRow_IsDisabled()
        {
            var root = Bind("overridableHolder.value.kind");
            var popup = root.Q<ChoicePopupField>();

            Assert.IsNotNull(popup);
            Assert.IsFalse(popup.enabledInHierarchy);
        }

        private static string GetText(ChoicePopupField field)
            => field.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName).text;

        private static bool IsWarningShown(VisualElement root)
        {
            var icon = root.Q<Image>(className: UitkAPI.WARNING_ICON_CLASS_NAME);
            return icon != null && icon.resolvedStyle.display != DisplayStyle.None && icon.style.display != DisplayStyle.None;
        }

        private PropertyField Bind(string propertyPath)
        {
            var field = new PropertyField(_serializedObject.FindProperty(propertyPath));
            _window.rootVisualElement.Add(field);
            field.Bind(_serializedObject);
            return field;
        }
    }
}

#endif
