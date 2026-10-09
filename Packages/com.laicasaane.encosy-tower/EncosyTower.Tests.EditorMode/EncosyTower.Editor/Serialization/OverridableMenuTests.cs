#if UNITY_EDITOR

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
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableMenuTests
    {
        private const BindingFlags STATIC_FLAGS = BindingFlags.Static | BindingFlags.NonPublic;

        private OverridableTestAsset _asset;
        private OverridableChoiceTestAsset _choicesAsset;
        private SerializedObject _source;
        private SerializedObject _choicesSource;
        private MenuTestWindow _window;

        private static MethodInfo FindNativeMenuBuilder()
            => typeof(EditorGUI).GetMethod(
                  name: "FillPropertyContextMenu"
                , bindingAttr: STATIC_FLAGS
                , binder: null
                , types: new[] {
                      typeof(SerializedProperty),
                      typeof(bool),
                      typeof(bool),
                      typeof(SerializedProperty),
                      typeof(GenericMenu),
                      typeof(VisualElement),
                  }
                , modifiers: null
            ) ?? typeof(EditorGUI).GetMethod(
                  name: "FillPropertyContextMenu"
                , bindingAttr: STATIC_FLAGS
                , binder: null
                , types: new[] { typeof(SerializedProperty), typeof(SerializedProperty), typeof(GenericMenu) }
                , modifiers: null
            );

        private static (string Text, bool Separator, GenericMenu.MenuFunction Callback)[] ReadMenu(GenericMenu menu)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
            var items = (IList)typeof(GenericMenu).GetProperty("menuItems", FLAGS).GetValue(menu);
            var result = new (string Text, bool Separator, GenericMenu.MenuFunction Callback)[items.Count];

            for (var i = 0; i < result.Length; i++)
            {
                var item = items[i];
                var type = item.GetType();
                result[i] = (
                      ((GUIContent)type.GetField("content").GetValue(item)).text
                    , (bool)type.GetField("separator").GetValue(item)
                    , (GenericMenu.MenuFunction)type.GetField("func").GetValue(item)
                );
            }

            return result;
        }

        private static string[] GetTexts((string Text, bool Separator, GenericMenu.MenuFunction Callback)[] items)
        {
            var result = new string[items.Length];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = items[i].Text;
            }

            return result;
        }

        private static GenericMenu ReturnNullMenu(
              SerializedProperty property
            , SerializedProperty linked
            , GenericMenu menu
        )
            => null;

        private static GenericMenu ReturnEmptyMenu(
              SerializedProperty property
            , SerializedProperty linked
            , GenericMenu menu
        )
            => menu;

        private static GenericMenu FailToFillMenu(
              SerializedProperty property
            , SerializedProperty linked
            , GenericMenu menu
        )
        {
            menu.AddDisabledItem(new GUIContent("Partial menu"));
            int.Parse("Invalid menu");
            return menu;
        }

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _choicesAsset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();
            _source = new SerializedObject(_asset);
            _choicesSource = new SerializedObject(_choicesAsset);
            _window = ScriptableObject.CreateInstance<MenuTestWindow>();
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
            _choicesSource.Dispose();
            Undo.ClearUndo(_asset);
            Undo.ClearUndo(_choicesAsset);
            UnityEngine.Object.DestroyImmediate(_asset);
            UnityEngine.Object.DestroyImmediate(_choicesAsset);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ContextMenu_UsesOuterPropertyAndAppendsLocateAfterUnityItems(bool imgui, bool overridden)
        {
            _asset.retryCount = new(value: 91, isOverridden: overridden);
            _source.Update();
            var property = _source.FindProperty(nameof(OverridableTestAsset.retryCount));
            var field = typeof(OverridableTestAsset).GetField(property.name);
            var nativeBuilder = FindNativeMenuBuilder();
            var callbacks = 0;
            string observedPath = null;
            UnityEngine.Object observedTarget = null;
            EditorApplication.contextualPropertyMenu += OnPropertyMenu;

            try
            {
                var nativeMenu = new GenericMenu();

                if (nativeBuilder != null)
                {
                    var arguments = nativeBuilder.GetParameters().Length == 6
                        ? new object[] { property, false, true, null, nativeMenu, null }
                        : new object[] { property, null, nativeMenu };

                    nativeMenu = nativeBuilder.Invoke(obj: null, parameters: arguments) as GenericMenu ?? nativeMenu;
                    callbacks = 0;
                }

                var menu = imgui
                    ? OverridableImguiRow.Get(property, field).BuildContextMenu(property.displayName)
                    : CreateBuilder(property, field).BuildContextMenu();

                var items = ReadMenu(menu);
                Assert.AreEqual("Locate Default Source", items[^1].Text);
                Assert.IsFalse(items[^1].Separator);
                Assert.IsNotNull(items[^1].Callback);

                if (nativeBuilder != null)
                {
                    Assert.AreEqual(expected: 1, actual: callbacks);
                    Assert.AreEqual(property.propertyPath, observedPath);
                    Assert.AreSame(_asset, observedTarget);
                    Assert.IsTrue(items[^2].Separator);
                    var nativeItems = ReadMenu(nativeMenu);
                    Assert.Greater(nativeItems.Length, 1);
                    Assert.AreEqual(nativeItems.Length + 2, items.Length);
                    CollectionAssert.Contains(GetTexts(nativeItems), "Test property action");

                    for (var i = 0; i < nativeItems.Length; i++)
                    {
                        Assert.AreEqual(nativeItems[i].Text, items[i].Text);
                        Assert.AreEqual(nativeItems[i].Separator, items[i].Separator);
                    }
                }
                else
                {
                    Assert.AreEqual(expected: 1, actual: items.Length);
                }

                Assert.AreEqual(expected: 91, actual: _asset.retryCount.value);
                Assert.AreEqual(overridden, _asset.retryCount.isOverridden);
            }
            finally
            {
                EditorApplication.contextualPropertyMenu -= OnPropertyMenu;
            }

            void OnPropertyMenu(GenericMenu menu, SerializedProperty current)
            {
                callbacks++;
                observedPath = current.propertyPath;
                observedTarget = current.serializedObject.targetObject;
                menu.AddDisabledItem(new GUIContent("Test property action"));
            }
        }

        [TestCase(null)]
        [TestCase(nameof(ReturnNullMenu))]
        [TestCase(nameof(ReturnEmptyMenu))]
        [TestCase(nameof(FailToFillMenu))]
        public void ContextMenu_UnavailableOrFailingUnityBuilderKeepsLocateUsable(string methodName)
        {
            var property = _source.FindProperty(nameof(OverridableTestAsset.retryCount));
            var native = methodName == null ? null : typeof(OverridableMenuTests).GetMethod(methodName, STATIC_FLAGS);

            var builder = typeof(OverridableEditorAPI).GetMethod(
                  name: "BuildPropertyContextMenu"
                , bindingAttr: STATIC_FLAGS
                , binder: null
                , types: new[] {
                      typeof(SerializedProperty),
                      typeof(GenericMenu.MenuFunction),
                      typeof(MethodInfo),
                      typeof(bool),
                  }
                , modifiers: null
            );

            var located = false;
            GenericMenu.MenuFunction locate = Locate;
            var menu = (GenericMenu)builder.Invoke(
                  obj: null
                , parameters: new object[] { property, locate, native, false }
            );
            var items = ReadMenu(menu);
            CollectionAssert.AreEqual(new[] { "Locate Default Source" }, GetTexts(items));
            Assert.IsFalse(items[0].Separator);
            items[0].Callback();
            Assert.IsTrue(located);

            void Locate()
            {
                located = true;
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ContextMenu_StoredElementInInactiveOuterListHasNoMenu(bool imgui)
        {
            _asset.nestedList.value = new List<Overridable<int>> { new(value: 21, isOverridden: true) };
            _asset.nestedList.isOverridden = false;
            _source.Update();
            var outer = _source.FindProperty(nameof(OverridableTestAsset.nestedList));
            var property = outer.FindPropertyRelative(OverridableEditorAPI.VALUE).GetArrayElementAtIndex(0);
            var field = typeof(OverridableTestAsset).GetField(outer.name);

            var menu = imgui
                ? OverridableImguiRow.Get(property, field).BuildContextMenu(property.displayName)
                : CreateBuilder(property, field).BuildContextMenu();

            Assert.AreEqual(expected: 0, actual: menu.GetItemCount());
            Assert.AreEqual(expected: 21, actual: _asset.nestedList.value[0].value);
            Assert.IsTrue(_asset.nestedList.value[0].isOverridden);
        }

        [TestCase(nameof(OverridableTestAsset.retryCount))]
        [TestCase(nameof(OverridableTestAsset.enumWithDefault))]
        [TestCase(nameof(OverridableTestAsset.payloadValue))]
        [TestCase(nameof(OverridableTestAsset.nestedList))]
        [TestCase(nameof(OverridableTestAsset.arrayValue))]
        public void RowsWithoutChoices_HaveNoChoicesButtonOrChoiceMenu(string fieldName)
        {
            var property = _source.FindProperty(fieldName);
            var field = typeof(OverridableTestAsset).GetField(fieldName);
            var builder = CreateBuilder(property, field);

            Assert.IsNull(builder.Root.Q<Button>(className: "encosy-overridable__more"));
            Assert.AreEqual(expected: 0, actual: builder.BuildChoiceMenu().GetItemCount());

            Assert.AreEqual(
                  expected: 0
                , actual: OverridableImguiRow.Get(property, field).BuildChoiceMenu(
                      choices: null
                    , choicesAttribute: null
                ).GetItemCount()
            );
        }

        [TestCase(nameof(OverridableChoiceTestAsset.frameRate), false)]
        [TestCase(nameof(OverridableChoiceTestAsset.frameRate), true)]
        [TestCase(nameof(OverridableChoiceTestAsset.tierLimit), false)]
        [TestCase(nameof(OverridableChoiceTestAsset.tierLimit), true)]
        [TestCase(nameof(OverridableChoiceTestAsset.layerPresets), false)]
        [TestCase(nameof(OverridableChoiceTestAsset.layerPresets), true)]
        [TestCase(nameof(OverridableChoiceTestAsset.region), false)]
        [TestCase(nameof(OverridableChoiceTestAsset.region), true)]
        [TestCase(nameof(OverridableChoiceTestAsset.strictTierLimit), false)]
        [TestCase(nameof(OverridableChoiceTestAsset.strictTierLimit), true)]
        [TestCase(nameof(OverridableChoiceTestAsset.strictLayerPresets), false)]
        [TestCase(nameof(OverridableChoiceTestAsset.strictLayerPresets), true)]
        public void ChoiceMenus_OfferOnlyNonExclusivePresets(string fieldName, bool imgui)
        {
            var property = _choicesSource.FindProperty(fieldName);
            var field = typeof(OverridableChoiceTestAsset).GetField(fieldName);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            var valueType = OverridableEditorAPI.GetValueType(field, property);

            Assert.IsTrue(ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , valueType
                , out var choices
                , out var error
            ), error.ToMessage());

            GenericMenu menu;

            if (imgui)
            {
                menu = OverridableImguiRow.Get(property, field).BuildChoiceMenu(choices, attribute);
            }
            else
            {
                var builder = CreateBuilder(property, field, choices, attribute);
                menu = builder.BuildChoiceMenu();
                var button = builder.Root.Q<Button>(className: "encosy-overridable__more");
                Assert.AreEqual(attribute.IsExclusive == false, button != null);
            }

            var items = ReadMenu(menu);
            var expected = new string[attribute.IsExclusive ? 0 : choices.Count];

            for (var i = 0; i < expected.Length; i++)
            {
                expected[i] = choices[i].label;
                Assert.IsFalse(items[i].Separator);
                Assert.IsNotNull(items[i].Callback);
            }

            CollectionAssert.AreEqual(expected, GetTexts(items));
        }

        [TestCase(nameof(OverridableChoiceTestAsset.targetFrameRate))]
        [TestCase("frameRateSteps.Array.data[1]")]
        public void PlainChoiceMenu_ContainsOnlyChoicesAndSelectionUndoes(string propertyPath)
        {
            var property = _choicesSource.FindProperty(propertyPath);
            var fieldName = propertyPath.Split('.')[0];
            var field = typeof(OverridableChoiceTestAsset).GetField(fieldName);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();

            Assert.IsTrue(ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , typeof(int)
                , out var choices
                , out var error
            ), error.ToMessage());

            var builder = typeof(ValueChoicesPropertyDrawer).GetMethod("BuildPlainMenu", STATIC_FLAGS);

            var menu = (GenericMenu)builder.Invoke(
                  obj: null
                , parameters: new object[] { property, typeof(int), choices }
            );

            var items = ReadMenu(menu);
            CollectionAssert.AreEqual(new[] { "30", "60", "120" }, GetTexts(items));

            for (var i = 0; i < items.Length; i++)
            {
                Assert.IsFalse(items[i].Separator);
            }

            var previous = property.intValue;
            Undo.IncrementCurrentGroup();
            items[0].Callback();
            _choicesSource.Update();
            Assert.AreEqual(expected: 30, actual: property.intValue);
            Undo.PerformUndo();
            _choicesSource.Update();
            Assert.AreEqual(previous, property.intValue);
        }

        [Test]
        public void EmptyChoices_HaveNoChoicesButtonOrMenu()
        {
            var property = _choicesSource.FindProperty(nameof(OverridableChoiceTestAsset.frameRate));
            var field = typeof(OverridableChoiceTestAsset).GetField(property.name);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            var choices = System.Array.Empty<(string label, object value)>();
            var builder = CreateBuilder(property, field, choices, attribute);

            Assert.IsNull(builder.Root.Q<Button>(className: "encosy-overridable__more"));
            Assert.AreEqual(expected: 0, actual: builder.BuildChoiceMenu().GetItemCount());

            Assert.AreEqual(
                  expected: 0
                , actual: OverridableImguiRow.Get(property, field).BuildChoiceMenu(choices, attribute).GetItemCount()
            );
        }

        [Test]
        public void DisabledRow_HasNoContextMenu()
        {
            var property = _source.FindProperty(nameof(OverridableTestAsset.retryCount));
            var field = typeof(OverridableTestAsset).GetField(property.name);
            var builder = CreateBuilder(property, field);
            builder.Root.SetEnabled(false);

            Assert.AreEqual(expected: 0, actual: builder.BuildContextMenu().GetItemCount());
        }

        private OverridableRowBuilder CreateBuilder(
              SerializedProperty property
            , FieldInfo field
            , IReadOnlyList<(string label, object value)> choices = null
            , ValueChoicesAttribute attribute = null
        )
        {
            var builder = new OverridableRowBuilder(
                  property: property
                , fieldInfo: field
                , valueType: OverridableEditorAPI.GetValueType(field, property)
                , attribute: field.GetCustomAttribute<OverridableDefaultAttribute>()
                , label: null
                , choices: choices
                , choicesAttribute: attribute
            );

            _window.rootVisualElement.Add(builder.Root);
            return builder;
        }

        private sealed class MenuTestWindow : EditorWindow
        {
        }
    }
}

#endif
