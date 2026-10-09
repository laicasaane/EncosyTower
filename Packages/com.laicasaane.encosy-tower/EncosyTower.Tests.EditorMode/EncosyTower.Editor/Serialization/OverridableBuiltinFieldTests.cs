#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
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
    public sealed class OverridableBuiltinFieldTests
    {
        private OverridableSpecialTypeTestAsset _asset;
        private SerializedObject _source;
        private BuiltinFieldTestWindow _window;

        public static IEnumerable<TestCaseData> BuiltinCases
        {
            get
            {
                var fields = new (string Name, Type Control, SerializedPropertyType Property)[] {
                    ("accentColor", typeof(ColorField), SerializedPropertyType.Color),
                    ("uvScale", typeof(Vector2Field), SerializedPropertyType.Vector2),
                    ("direction", typeof(Vector3Field), SerializedPropertyType.Vector3),
                    ("weights", typeof(Vector4Field), SerializedPropertyType.Vector4),
                    ("gridCell", typeof(Vector2IntField), SerializedPropertyType.Vector2Int),
                    ("gridPosition", typeof(Vector3IntField), SerializedPropertyType.Vector3Int),
                    ("viewport", typeof(RectField), SerializedPropertyType.Rect),
                    ("tileArea", typeof(RectIntField), SerializedPropertyType.RectInt),
                    ("spawnBounds", typeof(BoundsField), SerializedPropertyType.Bounds),
                    ("cellBounds", typeof(BoundsIntField), SerializedPropertyType.BoundsInt),
                    ("orientation", typeof(OverridableQuaternionField), SerializedPropertyType.Quaternion),
                    ("contentHash", typeof(Hash128Field), SerializedPropertyType.Hash128),
                };

                var count = fields.Length;

                for (var i = 0; i < count; i++)
                {
                    var field = fields[i];
                    yield return new TestCaseData(new object[] { field.Name, field.Control, field.Property, false });
                    yield return new TestCaseData(new object[] { field.Name, field.Control, field.Property, true });
                }
            }
        }

        private static void AssumeNoCustomDrawer(Type controlType)
        {
            if (controlType == typeof(OverridableQuaternionField))
            {
                Assume.That(
                      OverridableImguiRow.HasCustomDrawer(typeof(Quaternion))
                    , Is.False
                    , "A registered Quaternion drawer (for example SRP Core's) takes precedence."
                );
            }
        }

        private static VisualElement GetValueField(VisualElement root)
            => root.Q<VisualElement>(className: OverridableRowBuilder.VALUE_FIELD_CLASS_NAME);

        private static VisualElement GetSingleControl(VisualElement root, Type controlType)
        {
            var descendants = root.Query<VisualElement>().ToList();
            VisualElement result = null;
            var count = descendants.Count;

            for (var i = 0; i < count; i++)
            {
                var element = descendants[i];

                if (element.GetType() == controlType)
                {
                    Assert.IsNull(result, $"More than one {controlType.Name}.");
                    result = element;
                }
            }

            Assert.IsNotNull(result, $"Missing {controlType.Name}.");
            return result;
        }

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<OverridableSpecialTypeTestAsset>();
            _source = new SerializedObject(_asset);
            _window = ScriptableObject.CreateInstance<BuiltinFieldTestWindow>();
            _window.Show();
            OverridableImguiRow.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window.IsValid())
            {
                _window.Close();
            }

            OverridableImguiRow.ClearCache();
            _source?.Dispose();
            Undo.ClearUndo(_asset);
            UnityEngine.Object.DestroyImmediate(_asset);
        }

        [TestCaseSource(nameof(BuiltinCases))]
        public void BuiltinValuesBindOneTypedControlWithoutFoldouts(
              string fieldName
            , Type controlType
            , SerializedPropertyType propertyType
            , bool expanded
        )
        {
            AssumeNoCustomDrawer(controlType);
            var field = typeof(OverridableSpecialTypeTestAsset).GetField(fieldName);
            var valueType = OverridableEditorAPI.GetValueType(field.FieldType);
            var attribute = field.GetCustomAttribute<OverridableDefaultAttribute>();
            var property = _source.FindProperty(fieldName);
            var stored = property.FindPropertyRelative(OverridableEditorAPI.VALUE);
            stored.isExpanded = expanded;
            var builder = new OverridableRowBuilder(property, field, valueType, attribute);
            _window.rootVisualElement.Add(builder.Root);
            builder.Root.Bind(_source);
            var input = GetValueField(builder.Root);
            var control = GetSingleControl(builder.Root, controlType);
            var expected = typeof(SpecialTypeTestDefaults).GetProperty(attribute.MemberName).GetValue(obj: null);

            Assert.IsFalse(OverridableImguiRow.UsesHeader(stored, valueType));
            Assert.AreEqual(propertyType, stored.propertyType);
            Assert.IsNull(builder.Root.Q<Foldout>());
            Assert.IsFalse(input.enabledSelf);
            Assert.AreEqual("_overridableDefault.value", ((IBindable)input).bindingPath);
            Assert.AreEqual(expected, controlType.GetProperty("value").GetValue(control));
            Assert.IsFalse(property.FindPropertyRelative(OverridableEditorAPI.IS_OVERRIDDEN).boolValue);

            var imgui = OverridableImguiRow.Get(property, field);
            var preview = imgui.GetDrawnProperty();
            Assert.AreEqual(propertyType, preview.propertyType);
            Assert.IsFalse(OverridableImguiRow.UsesHeader(preview, valueType));
            Assert.AreEqual(expected, SerializedValueCopy.Read(preview, valueType));

            builder.Root.Q<OverridableSwitch>().value = true;
            Assert.AreSame(input, GetValueField(builder.Root));
            control = GetSingleControl(builder.Root, controlType);
            Assert.IsNull(builder.Root.Q<Foldout>());
            Assert.IsTrue(input.enabledSelf);
            Assert.AreEqual(fieldName + ".value", ((IBindable)input).bindingPath);
            Assert.AreEqual(expected, stored.boxedValue);
            Assert.AreEqual(expected, controlType.GetProperty("value").GetValue(control));
            Assert.AreEqual(stored.propertyPath, imgui.GetDrawnProperty().propertyPath);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Vector4InlineFieldEditsPreservesStoredValueAndUndoes(bool expanded)
        {
            var fieldName = nameof(OverridableSpecialTypeTestAsset.weights);
            var property = _source.FindProperty(fieldName);
            property.FindPropertyRelative(OverridableEditorAPI.VALUE).isExpanded = expanded;
            var builder = CreateRow(fieldName);
            var input = builder.Root.Q<Vector4Field>();
            Assert.IsNotNull(input);
            Assert.IsNull(builder.Root.Q<Foldout>());
            Assert.AreEqual(SpecialTypeTestDefaults.Weights, input.value);
            var components = input.Query<FloatField>().ToList();
            var count = components.Count;
            Assert.AreEqual(expected: 4, actual: count);

            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(SpecialTypeTestDefaults.Weights[i], components[i].value);
            }

            Assert.AreEqual(Vector4.zero, _asset.weights.value);
            Assert.IsFalse(input.enabledInHierarchy);
            var toggle = builder.Root.Q<OverridableSwitch>();
            toggle.value = true;
            Assert.AreSame(input, builder.Root.Q<Vector4Field>());
            Assert.AreEqual("weights.value", ((IBindable)input).bindingPath);
            Assert.IsTrue(input.enabledInHierarchy);
            var edited = new Vector4(x: 2f, y: 4f, z: 6f, w: 8f);
            Undo.IncrementCurrentGroup();
            input.value = edited;
            Assert.AreEqual(edited, _asset.weights.value);
            Undo.IncrementCurrentGroup();
            toggle.value = false;
            Assert.IsFalse(_asset.weights.isOverridden);
            Assert.AreEqual(edited, _asset.weights.value);
            Assert.AreSame(input, builder.Root.Q<Vector4Field>());
            Assert.AreEqual("_overridableDefault.value", ((IBindable)input).bindingPath);
            Assert.AreEqual(SpecialTypeTestDefaults.Weights, input.value);
            Assert.IsFalse(input.enabledInHierarchy);
            Undo.PerformUndo();
            _source.Update();
            builder.Refresh();
            Assert.IsTrue(_asset.weights.isOverridden);
            Assert.AreEqual(edited, _asset.weights.value);
            Undo.PerformUndo();
            _source.Update();
            Assert.IsTrue(_asset.weights.isOverridden);
            Assert.AreEqual(SpecialTypeTestDefaults.Weights, _asset.weights.value);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void QuaternionInlineEulerFieldWritesQuaternionAndSupportsMixedValues()
        {
            AssumeNoCustomDrawer(typeof(OverridableQuaternionField));
            var builder = CreateRow(nameof(OverridableSpecialTypeTestAsset.orientation));
            var input = builder.Root.Q<OverridableQuaternionField>();
            Assert.IsNotNull(input);
            var euler = input.Q<Vector3Field>();
            Assert.IsNotNull(euler);
            Assert.IsNull(builder.Root.Q<Foldout>());
            Assert.AreEqual(SpecialTypeTestDefaults.Orientation, input.value);
            var defaultAngles = new Vector3(x: 0f, y: 45f, z: 0f);
            Assert.That(Vector3.Distance(defaultAngles, euler.value), Is.LessThan(0.001f));
            var components = euler.Query<FloatField>().ToList();
            var count = components.Count;
            Assert.AreEqual(expected: 3, actual: count);

            for (var i = 0; i < count; i++)
            {
                Assert.That(components[i].value, Is.EqualTo(defaultAngles[i]).Within(0.001f));
            }

            builder.Root.Q<OverridableSwitch>().value = true;
            var changed = 0;
            input.RegisterValueChangedCallback(OnChanged);
            var angles = new Vector3(x: 10f, y: 20f, z: 30f);
            Undo.IncrementCurrentGroup();
            euler.value = angles;
            Assert.AreEqual(expected: 1, actual: changed);
            Assert.AreEqual(Quaternion.Euler(angles), _asset.orientation.value);
            Assert.IsTrue(_asset.orientation.isOverridden);
            input.showMixedValue = true;
            Assert.IsTrue(euler.showMixedValue);
            input.showMixedValue = false;
            Assert.IsFalse(euler.showMixedValue);
            Undo.PerformUndo();
            _source.Update();
            Assert.AreEqual(SpecialTypeTestDefaults.Orientation, _asset.orientation.value);
            Assert.IsTrue(_asset.orientation.isOverridden);
            LogAssert.NoUnexpectedReceived();

            void OnChanged(ChangeEvent<Quaternion> evt)
            {
                changed++;
            }
        }

        private OverridableRowBuilder CreateRow(string fieldName)
        {
            var property = _source.FindProperty(fieldName);
            var field = typeof(OverridableSpecialTypeTestAsset).GetField(fieldName);

            var builder = new OverridableRowBuilder(
                  property
                , field
                , OverridableEditorAPI.GetValueType(field.FieldType)
                , field.GetCustomAttribute<OverridableDefaultAttribute>()
            );

            _window.rootVisualElement.Add(builder.Root);
            builder.Root.Bind(_source);
            return builder;
        }

        private sealed class BuiltinFieldTestWindow : EditorWindow
        {
        }
    }
}

#endif
