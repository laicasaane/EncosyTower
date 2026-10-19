#if UNITY_EDITOR

using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Editor.Internals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Editor.Common
{
    public sealed class StringEnumEditorAPITests
    {
        private StringEnumTestAsset _asset;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<StringEnumTestAsset>();
            _serializedObject = new SerializedObject(_asset);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            Undo.ClearUndo(_asset);
            Object.DestroyImmediate(_asset);
        }

        [Test]
        public void TryGetStructType_PlainField()
        {
            var field = typeof(StringEnumTestAsset).GetField(nameof(StringEnumTestAsset.plain));

            Assert.IsTrue(StringEnumEditorAPI.TryGetStructType(field, out var type));
            Assert.AreEqual(typeof(StringEnum<StringEnumTestKind>), type);
        }

        [Test]
        public void TryGetStructType_ArrayAndList()
        {
            var array = typeof(StringEnumTestAsset).GetField(nameof(StringEnumTestAsset.list));
            var list = typeof(StringEnumTestAsset).GetField(nameof(StringEnumTestAsset.convertedList));

            Assert.IsTrue(StringEnumEditorAPI.TryGetStructType(array, out var arrayType));
            Assert.AreEqual(typeof(StringEnum<StringEnumTestKind>), arrayType);

            Assert.IsTrue(StringEnumEditorAPI.TryGetStructType(list, out var listType));
            Assert.AreEqual(typeof(StringEnum<StringEnumTestKind, StringEnumTestConverter>), listType);
        }

        [Test]
        public void TryGetStructType_NotAStringEnum()
        {
            var field = typeof(NotStringEnum).GetField(nameof(NotStringEnum.number));

            Assert.IsFalse(StringEnumEditorAPI.TryGetStructType(field, out var type));
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetChoices_Plain()
        {
            Assert.IsTrue(StringEnumEditorAPI.TryGetChoices(typeof(StringEnum<StringEnumTestKind>), out var choices));
            AssertChoices(choices, new[] { "None", "Fire", "Cold Water", "Wind" }, new[] { "None", "Fire", "Water", "Wind" });
        }

        [Test]
        public void TryGetChoices_Converter()
        {
            var type = typeof(StringEnum<StringEnumTestKind, StringEnumTestConverter>);

            Assert.IsTrue(StringEnumEditorAPI.TryGetChoices(type, out var choices));
            AssertChoices(choices, new[] { "None", "Fire", "Cold Water", "Wind" }, new[] { "none", "fire", "water", "wind" });
        }

        [Test]
        public void TryGetChoices_AliasesAreListedOnce()
        {
            Assert.IsTrue(StringEnumEditorAPI.TryGetChoices(typeof(StringEnum<StringEnumTestAlias>), out var choices));
            Assert.AreEqual(2, choices.Count);
        }

        [Test]
        public void TryGetChoices_FailingConverterLogsOnce()
        {
            StringEnumEditorAPI.ClearCache();
            var type = typeof(StringEnum<StringEnumTestKind, StringEnumTestThrowingConverter>);

            LogAssert.Expect(
                  LogType.Warning
                , $"StringEnum drawer cannot read the choices of {type.Name}: test failure"
            );

            Assert.IsFalse(StringEnumEditorAPI.TryGetChoices(type, out var first));
            Assert.IsFalse(StringEnumEditorAPI.TryGetChoices(type, out var second));
            Assert.AreEqual(0, first.Count);
            Assert.AreEqual(0, second.Count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void IndexAndText()
        {
            StringEnumEditorAPI.TryGetChoices(typeof(StringEnum<StringEnumTestKind>), out var choices);

            Assert.AreEqual(1, StringEnumEditorAPI.GetChoiceIndex(choices, "Fire"));
            Assert.AreEqual(-1, StringEnumEditorAPI.GetChoiceIndex(choices, ""));
            Assert.AreEqual(-1, StringEnumEditorAPI.GetChoiceIndex(choices, null));
            Assert.AreEqual(-1, StringEnumEditorAPI.GetChoiceIndex(choices, "Lava"));

            Assert.AreEqual("Fire", StringEnumEditorAPI.GetShownText(choices, "Fire"));
            Assert.AreEqual("(Not set)", StringEnumEditorAPI.GetShownText(choices, ""));
            Assert.AreEqual("(Not set)", StringEnumEditorAPI.GetShownText(choices, null));
            Assert.AreEqual("Lava", StringEnumEditorAPI.GetShownText(choices, "Lava"));

            Assert.IsFalse(StringEnumEditorAPI.IsUnlisted(choices, "Fire"));
            Assert.IsFalse(StringEnumEditorAPI.IsUnlisted(choices, ""));
            Assert.IsFalse(StringEnumEditorAPI.IsUnlisted(choices, null));
            Assert.IsTrue(StringEnumEditorAPI.IsUnlisted(choices, "Lava"));
        }

        [Test]
        public void TryWrite_WritesValue()
        {
            Assert.IsTrue(StringEnumEditorAPI.TryWrite(_serializedObject, nameof(StringEnumTestAsset.plain), "Fire"));
            Assert.AreEqual("Fire", _asset.plain.ToString());
        }

        [Test]
        public void TryWrite_IsOneUndoStep()
        {
            _asset.plain = new StringEnum<StringEnumTestKind>("Fire");
            _serializedObject.Update();

            Undo.IncrementCurrentGroup();
            Assert.IsTrue(StringEnumEditorAPI.TryWrite(_serializedObject, nameof(StringEnumTestAsset.plain), "Wind"));
            Assert.AreEqual("Wind", _asset.plain.ToString());

            Undo.PerformUndo();
            Assert.AreEqual("Fire", _asset.plain.ToString());
        }

        [Test]
        public void TryWrite_LockedRowRejectsWrite()
        {
            const string PATH = "overridableHolder.value.kind";

            Assert.IsFalse(StringEnumEditorAPI.TryWrite(_serializedObject, PATH, "Fire"));
            Assert.IsTrue(string.IsNullOrEmpty(_asset.overridableHolder.value.kind.ToString()));

            _asset.overridableHolder = new(value: default, isOverridden: true);
            _serializedObject.Update();

            Assert.IsTrue(StringEnumEditorAPI.TryWrite(_serializedObject, PATH, "Fire"));
            Assert.AreEqual("Fire", _asset.overridableHolder.value.kind.ToString());
        }

        [Test]
        public void CreateMenuItems_CheckAndSelect()
        {
            StringEnumEditorAPI.TryGetChoices(typeof(StringEnum<StringEnumTestKind>), out var choices);
            var path = nameof(StringEnumTestAsset.plain);

            var items = StringEnumEditorAPI.CreateMenuItems(_serializedObject, path, choices, "Fire", isMixed: false);

            Assert.AreEqual(4, items.Length);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(i == 1, items[i].IsChecked);
            }

            var mixed = StringEnumEditorAPI.CreateMenuItems(_serializedObject, path, choices, "Fire", isMixed: true);

            foreach (var item in mixed)
            {
                Assert.IsFalse(item.IsChecked);
            }

            items[3].Callback();
            Assert.AreEqual("Wind", _asset.plain.ToString());
        }

        private static void AssertChoices(IReadOnlyList<ValueChoice> choices, string[] labels, string[] values)
        {
            Assert.AreEqual(labels.Length, choices.Count);

            for (var i = 0; i < labels.Length; i++)
            {
                Assert.AreEqual(labels[i], choices[i].Label);
                Assert.AreEqual(values[i], choices[i].Value);
            }
        }

        private sealed class NotStringEnum
        {
            public int number;
        }
    }
}

#endif
