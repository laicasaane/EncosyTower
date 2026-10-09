#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
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
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Serialization
{
    internal sealed class ValueChoicesCollectionTests
    {
        private CollectionChoicesTestAsset _asset;
        private SerializedObject _source;
        private CollectionTestWindow _window;

        private static (string Text, bool Checked, bool Separator, GenericMenu.MenuFunction Select)[] ReadMenu(
              GenericMenu menu
        )
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
            var items = (IList)typeof(GenericMenu).GetProperty("menuItems", FLAGS).GetValue(menu);
            var result = new (string Text, bool Checked, bool Separator, GenericMenu.MenuFunction Select)[items.Count];

            for (var i = 0; i < result.Length; i++)
            {
                var item = items[i];
                var type = item.GetType();
                result[i] = (
                      ((GUIContent)type.GetField("content").GetValue(item)).text
                    , (bool)type.GetField("on").GetValue(item)
                    , (bool)type.GetField("separator").GetValue(item)
                    , (GenericMenu.MenuFunction)type.GetField("func").GetValue(item)
                );
            }

            return result;
        }

        private static IReadOnlyList<(string label, object value)> Resolve(FieldInfo field, Type valueType)
        {
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();

            Assert.IsTrue(ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , valueType
                , out var choices
                , out var error
            ), error.ToMessage());

            return choices;
        }

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<CollectionChoicesTestAsset>();
            _source = new SerializedObject(_asset);
            _window = ScriptableObject.CreateInstance<CollectionTestWindow>();
            _window.Show();
        }

        [TearDown]
        public void TearDown()
        {
            OverridableImguiRow.ClearCache();

            if (_window.IsValid())
            {
                _window.Close();
            }

            _source.Dispose();
            Undo.ClearUndo(_asset);
            UnityEngine.Object.DestroyImmediate(_asset);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Attribute_ApplyToCollectionControlsUnityRouting(bool applyToCollection, bool explicitSource)
        {
            var attribute = explicitSource
                ? new ValueChoicesAttribute(typeof(CollectionChoicesTestAsset), "ListPresets", applyToCollection)
                : new ValueChoicesAttribute("ListPresets", applyToCollection);

            Assert.AreEqual(applyToCollection, attribute.applyToCollection);
            Assert.AreEqual("ListPresets", attribute.MemberName);
            Assert.AreEqual(explicitSource ? typeof(CollectionChoicesTestAsset) : null, attribute.SourceType);
            Assert.IsFalse(attribute.IsExclusive);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Attribute_OmittedBooleanKeepsElementRouting(bool explicitSource)
        {
            var attribute = explicitSource
                ? new ValueChoicesAttribute(typeof(CollectionChoicesTestAsset), "ElementPresets")
                : new ValueChoicesAttribute("ElementPresets");

            Assert.IsFalse(attribute.applyToCollection);
        }

        [TestCase(nameof(CollectionChoicesTestAsset.wavePlan), typeof(List<int>))]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPlan), typeof(int[]))]
        public void Resolver_CollectionChoicesKeepLabelsAndWholeValues(string fieldName, Type valueType)
        {
            var field = typeof(CollectionChoicesTestAsset).GetField(fieldName);
            var choices = Resolve(field, valueType);
            Assert.AreEqual(expected: 3, actual: choices.Count);
            Assert.AreEqual("Gentle", choices[0].label);
            Assert.AreEqual("Standard", choices[1].label);
            Assert.AreEqual("Rush", choices[2].label);
            CollectionAssert.AreEqual(new[] { 1, 2, 4 }, (IEnumerable)choices[1].value);
            Assert.AreEqual(valueType, choices[1].value.GetType());
            Assert.IsFalse(ValueChoicesEditorAPI.UsesDropdown(
                  field.GetCustomAttribute<ValueChoicesAttribute>()
                , valueType
            ));
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Matches_CollectionsCompareCountOrderAndElements(bool leftArray, bool rightArray)
        {
            IList left = leftArray ? new[] { 1, 2, 4 } : new List<int> { 1, 2, 4 };
            IList right = rightArray ? new[] { 1, 2, 4 } : new List<int> { 1, 2, 4 };
            Assert.IsTrue(ValueChoicesEditorAPI.Matches(left, right));
            right[2] = 8;
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(left, right));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(left, new[] { 1, 2 }));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(left, new[] { 1, 4, 2 }));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: left, b: null));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(left, new object()));
            Assert.IsTrue(ValueChoicesEditorAPI.Matches(Array.Empty<int>(), new List<int>()));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: Array.Empty<int>(), b: null));
        }

        [TestCase("ListPresets", typeof(int), true, ValueChoicesError.Type.TargetTypeMismatch)]
        [TestCase("ElementPresets", typeof(List<int>), true, ValueChoicesError.Type.ChoiceTargetTypeMismatch)]
        [TestCase("ListPresets", typeof(int), false, ValueChoicesError.Type.ChoiceTargetTypeMismatch)]
        [TestCase("ListPresets", typeof(int[]), true, ValueChoicesError.Type.ChoiceTargetTypeMismatch)]
        [TestCase("WrongErasedPresets", typeof(List<int>), true, ValueChoicesError.Type.ChoiceTargetTypeMismatch)]
        public void Resolver_TargetAndChoiceTypeMismatchReturnsTypedFailure(
              string memberName
            , Type valueType
            , bool applyToCollection
            , ValueChoicesError.Type expected
        )
        {
            var attribute = new ValueChoicesAttribute(memberName, applyToCollection);

            var resolved = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(CollectionChoicesTestAsset)
                , valueType
                , out var choices
                , out var error
            );

            Assert.IsFalse(resolved);
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(expected), error.ToMessage());

            var expectedChoiceType = memberName switch {
                "ElementPresets" => typeof(int[]),
                "WrongErasedPresets" => typeof(int),
                _ => typeof(ValueChoice<List<int>>[]),
            };

            var choiceKind = applyToCollection ? "Collection" : "Element";

            var reason = expected == ValueChoicesError.Type.TargetTypeMismatch
                ? $"collection choices require an array or list value type, not '{valueType}'."
                : $"{choiceKind} choices require values "
                    + $"assignable to '{valueType}', not '{expectedChoiceType}'.";

            Assert.AreEqual(
                  $"Value choices member '{typeof(CollectionChoicesTestAsset).FullName}.{memberName}': {reason}"
                , error.ToMessage()
            );
        }

        [Test]
        public void Resolver_OmittedBooleanKeepsPreviouslySupportedValueTypes()
        {
            var attribute = new ValueChoicesAttribute(nameof(CollectionChoicesTestAsset.ListPresets));

            var resolved = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(CollectionChoicesTestAsset)
                , typeof(List<int>)
                , out var choices
                , out var error
            );

            Assert.IsTrue(resolved, error.ToMessage());
            Assert.AreEqual(expected: 3, actual: choices.Count);
            CollectionAssert.AreEqual(new[] { 1, 2, 4 }, (IEnumerable)choices[1].value);
        }

        [Test]
        public void Resolver_ExclusiveCollectionReturnsTypedFailure()
        {
            var attribute = new ValueChoicesAttribute(memberName: "ListPresets", applyToCollection: true) {
                IsExclusive = true,
            };

            var resolved = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(CollectionChoicesTestAsset)
                , typeof(List<int>)
                , out var choices
                , out var error
            );

            Assert.IsFalse(resolved);
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(ValueChoicesError.Type.ExclusiveCollection));

            Assert.AreEqual(
                  $"Value choices member '{typeof(CollectionChoicesTestAsset).FullName}.ListPresets': "
                    + "collection choices must be non-exclusive."
                , error.ToMessage()
            );
        }

        [TestCase(nameof(CollectionChoicesTestAsset.wavePlan))]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPlan))]
        public void PlainCollection_BindsWholeListWithOneChoicesButton(string fieldName)
        {
            var property = _source.FindProperty(fieldName);
            property.isExpanded = true;
            var root = new PropertyField(property);
            _window.rootVisualElement.Add(root);
            root.Bind(_source);
            var list = root.Q<ListView>("unity-list-" + fieldName);

            Assert.IsNotNull(list);
            Assert.AreEqual(expected: 3, actual: list.itemsSource.Count);
            var buttons = root.Query<Button>(className: "encosy-overridable__more").ToList();
            Assert.AreEqual(expected: 1, actual: buttons.Count);
            Assert.IsNull(root.Q<OverridableChoiceField>());
            CollectionAssert.AreEqual(new[] { 1, 2, 4 }, (IEnumerable)ValueChoicesPropertyDrawer.ReadValue(
                  property
                , typeof(CollectionChoicesTestAsset).GetField(fieldName).FieldType
            ));

            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(nameof(CollectionChoicesTestAsset.wavePlan))]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPlan))]
        public void PlainCollection_MenuChecksElementContentInsteadOfCollectionIdentity(string fieldName)
        {
            var property = _source.FindProperty(fieldName);
            var field = typeof(CollectionChoicesTestAsset).GetField(fieldName);
            var choices = Resolve(field, field.FieldType);
            const BindingFlags FLAGS = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ValueChoicesPropertyDrawer).GetMethod("BuildPlainMenu", FLAGS);

            var menu = (GenericMenu)method.Invoke(
                  obj: null
                , parameters: new object[] { property, field.FieldType, choices }
            );

            var items = ReadMenu(menu);
            Assert.AreEqual(expected: 3, actual: items.Length);
            Assert.AreEqual("Gentle", items[0].Text);
            Assert.AreEqual("Standard", items[1].Text);
            Assert.AreEqual("Rush", items[2].Text);
            Assert.IsFalse(items[0].Checked);
            Assert.IsTrue(items[1].Checked);
            Assert.IsFalse(items[2].Checked);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.IsFalse(items[i].Separator);
            }

            property.GetArrayElementAtIndex(2).intValue = 8;
            _source.ApplyModifiedPropertiesWithoutUndo();

            menu = (GenericMenu)method.Invoke(
                  obj: null
                , parameters: new object[] { property, field.FieldType, choices }
            );

            items = ReadMenu(menu);
            Assert.IsFalse(items[0].Checked);
            Assert.IsFalse(items[1].Checked);
            Assert.IsFalse(items[2].Checked);
        }

        [TestCase(nameof(CollectionChoicesTestAsset.wavePlan))]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPlan))]
        public void ApplyToCollection_ElementsKeepNormalEditingWithoutPresetButtons(string fieldName)
        {
            var collection = _source.FindProperty(fieldName);
            var element = collection.GetArrayElementAtIndex(1);
            var root = new PropertyField(element);
            _window.rootVisualElement.Add(root);
            root.Bind(_source);
            var input = root.Q<IntegerField>();

            Assert.IsNotNull(input);
            Assert.AreEqual(expected: 2, actual: input.value);
            Assert.IsNull(root.Q<Button>(className: "encosy-overridable__more"));
            input.value = 7;
            Assert.AreEqual(expected: 7, actual: collection.GetArrayElementAtIndex(1).intValue);
            Assert.AreEqual(expected: 1, actual: collection.GetArrayElementAtIndex(0).intValue);
            Assert.AreEqual(expected: 4, actual: collection.GetArrayElementAtIndex(2).intValue);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(nameof(CollectionChoicesTestAsset.wavePlan), false)]
        [TestCase(nameof(CollectionChoicesTestAsset.wavePlan), true)]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPlan), false)]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPlan), true)]
        public void PlainCollection_PresetReplacesSizeAndElementsWithOneUndo(string fieldName, bool imgui)
        {
            var property = _source.FindProperty(fieldName);
            var field = typeof(CollectionChoicesTestAsset).GetField(fieldName);
            var choices = Resolve(field, field.FieldType);
            property.arraySize = 4;
            property.GetArrayElementAtIndex(3).intValue = 99;
            _source.ApplyModifiedPropertiesWithoutUndo();
            Undo.IncrementCurrentGroup();

            if (imgui)
            {
                const BindingFlags FLAGS = BindingFlags.Static | BindingFlags.NonPublic;
                var method = typeof(ValueChoicesPropertyDrawer).GetMethod("BuildPlainMenu", FLAGS);

                var menu = (GenericMenu)method.Invoke(
                      obj: null
                    , parameters: new object[] { property, field.FieldType, choices }
                );

                ReadMenu(menu)[2].Select();
            }
            else
            {
                var type = typeof(ValueChoicesPropertyDrawer).GetNestedType("PlainField", BindingFlags.NonPublic);

                var plain = Activator.CreateInstance(
                      type
                    , new object[] {
                          property,
                          field.FieldType,
                          field.GetCustomAttribute<ValueChoicesAttribute>(),
                          choices,
                          property.displayName,
                      }
                );

                var method = type.GetMethod("ApplyChoice", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(plain, new[] { choices[2].value });
            }

            _source.Update();
            CollectionAssert.AreEqual(new[] { 2, 4, 8 }, (IEnumerable)ValueChoicesPropertyDrawer.ReadValue(
                  property
                , field.FieldType
            ));

            Undo.PerformUndo();
            _source.Update();
            CollectionAssert.AreEqual(new[] { 1, 2, 4, 99 }, (IEnumerable)ValueChoicesPropertyDrawer.ReadValue(
                  property
                , field.FieldType
            ));
        }

        [TestCase(nameof(CollectionChoicesTestAsset.wavePresets), false)]
        [TestCase(nameof(CollectionChoicesTestAsset.wavePresets), true)]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPresets), false)]
        [TestCase(nameof(CollectionChoicesTestAsset.arrayPresets), true)]
        public void OverridableCollection_MenuChecksShownDefaultAndPresetUndoesBothMembers(string fieldName, bool imgui)
        {
            var property = _source.FindProperty(fieldName);
            var field = typeof(CollectionChoicesTestAsset).GetField(fieldName);
            var valueType = OverridableEditorAPI.GetValueType(field, property);
            var choices = Resolve(field, valueType);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            OverridableRowBuilder builder = null;
            GenericMenu menu;

            if (imgui)
            {
                menu = OverridableImguiRow.Get(property, field).BuildChoiceMenu(choices, attribute);
            }
            else
            {
                builder = new OverridableRowBuilder(
                      property
                    , field
                    , valueType
                    , field.GetCustomAttribute<OverridableDefaultAttribute>()
                    , property.displayName
                    , choices
                    , attribute
                );

                _window.rootVisualElement.Add(builder.Root);
                menu = builder.BuildChoiceMenu();
            }

            var items = ReadMenu(menu);
            Assert.AreEqual(expected: 3, actual: items.Length);
            Assert.AreEqual("Gentle", items[0].Text);
            Assert.AreEqual("Standard", items[1].Text);
            Assert.AreEqual("Rush", items[2].Text);
            Assert.IsFalse(items[0].Checked);
            Assert.IsTrue(items[1].Checked);
            Assert.IsFalse(items[2].Checked);

            for (var i = 0; i < items.Length; i++)
            {
                Assert.IsFalse(items[i].Separator);
            }

            Undo.IncrementCurrentGroup();
            items[2].Select();
            _source.Update();
            Assert.IsTrue(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            var value = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            CollectionAssert.AreEqual(
                  new[] { 2, 4, 8 }
                , (IEnumerable)ValueChoicesPropertyDrawer.ReadValue(value, valueType)
            );

            var selectedMenu = imgui
                ? OverridableImguiRow.Get(property, field).BuildChoiceMenu(choices, attribute)
                : builder.BuildChoiceMenu();

            Assert.IsTrue(ReadMenu(selectedMenu)[2].Checked);
            Assert.IsFalse(ReadMenu(selectedMenu)[1].Checked);
            Undo.PerformUndo();
            _source.Update();
            builder?.Refresh();
            Assert.IsFalse(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);
            CollectionAssert.AreEqual(
                  new[] { 99 }
                , (IEnumerable)ValueChoicesPropertyDrawer.ReadValue(value, valueType)
            );
        }

        private sealed class CollectionTestWindow : EditorWindow
        {
        }
    }
}

#endif
