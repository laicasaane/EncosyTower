#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
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
    public sealed class OverridableSpecialTypeTests
    {
        private OverridableSpecialTypeTestAsset _asset;
        private SerializedObject _source;
        private OverridableDefaultHolder _holder;
        private SpecialTypeWindow _window;
        private Texture2D _defaultTexture;
        private Texture2D _storedTexture;
        private CultureInfo _previousCulture;

        public static IEnumerable<string> FieldNames => new[] {
            nameof(OverridableSpecialTypeTestAsset.accentColor),
            nameof(OverridableSpecialTypeTestAsset.summary),
            nameof(OverridableSpecialTypeTestAsset.spawnBudget),
            nameof(OverridableSpecialTypeTestAsset.waveArray),
            nameof(OverridableSpecialTypeTestAsset.fixedSamples),
            nameof(OverridableSpecialTypeTestAsset.referenceGraph),
            nameof(OverridableSpecialTypeTestAsset.collisionMask),
            nameof(OverridableSpecialTypeTestAsset.responseCurve),
            nameof(OverridableSpecialTypeTestAsset.heatGradient),
            nameof(OverridableSpecialTypeTestAsset.viewport),
            nameof(OverridableSpecialTypeTestAsset.spawnBounds),
            nameof(OverridableSpecialTypeTestAsset.gridCell),
            nameof(OverridableSpecialTypeTestAsset.orientation),
            nameof(OverridableSpecialTypeTestAsset.contentHash),
            nameof(OverridableSpecialTypeTestAsset.marker),
            nameof(OverridableSpecialTypeTestAsset.population),
            nameof(OverridableSpecialTypeTestAsset.precision),
            nameof(OverridableSpecialTypeTestAsset.uvScale),
            nameof(OverridableSpecialTypeTestAsset.weights),
            nameof(OverridableSpecialTypeTestAsset.gridPosition),
            nameof(OverridableSpecialTypeTestAsset.tileArea),
            nameof(OverridableSpecialTypeTestAsset.cellBounds),
            nameof(OverridableSpecialTypeTestAsset.exposedMaterial),
            nameof(OverridableSpecialTypeTestAsset.presetMaterial),
            nameof(OverridableSpecialTypeTestAsset.limitList),
        };

        public static IEnumerable<TestCaseData> SwitchCases
        {
            get
            {
                foreach (var field in FieldNames)
                {
                    yield return new TestCaseData(field, false).SetName($"SwitchAndUndo_IMGUI_{field}");
                    yield return new TestCaseData(field, true).SetName($"SwitchAndUndo_UIToolkit_{field}");
                }
            }
        }

        public static IEnumerable<TestCaseData> TooltipCases => new[] {
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.accentColor), "RGBA(0.200, 0.600, 0.900, 0.750)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.summary), "SpecialTypeTestSummary"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.spawnBudget), "Count: 12, Delay: 0.75"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.waveArray), "[2, 5, 9]"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.fixedSamples), "SpecialTypeTestFixedSamples"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.referenceGraph), "SpecialTypeTestReferenceGraph"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.collisionMask), "5"),
            new TestCaseData(
                  nameof(OverridableSpecialTypeTestAsset.responseCurve)
                , "Curve [(0, 0.25), (1, 1)] (ClampForever / PingPong)"
            ),
            new TestCaseData(
                  nameof(OverridableSpecialTypeTestAsset.heatGradient)
                , "Gradient (Blend): colors [0: RGBA(0.200, 0.600, 0.900, 1.000), "
                    + "1: RGBA(1.000, 0.400, 0.100, 1.000)]; alpha [0: 1, 1: 0.25]"
            ),
            new TestCaseData(
                  nameof(OverridableSpecialTypeTestAsset.viewport)
                , "(x:10.00, y:20.00, width:320.00, height:180.00)"
            ),
            new TestCaseData(
                  nameof(OverridableSpecialTypeTestAsset.spawnBounds)
                , "Center: (1.00, 2.00, 3.00), Extents: (2.00, 3.00, 4.00)"
            ),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.gridCell), "(3, -2)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.orientation), "Euler (0.00, 45.00, 0.00)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.contentHash), "0123456789abcdef0123456789abcdef"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.marker), "'Ω'"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.population), "9000000000"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.precision), "0.123456789012345"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.uvScale), "(1.50, 2.50)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.weights), "(0.10, 0.20, 0.30, 0.40)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.gridPosition), "(2, -3, 4)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.tileArea), "(x:2, y:3, width:8, height:6)"),
            new TestCaseData(
                  nameof(OverridableSpecialTypeTestAsset.cellBounds)
                , "Position: (1, 2, 3), Size: (4, 5, 6)"
            ),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.exposedMaterial), "Default texture (Texture2D)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.presetMaterial), "Default texture (Texture2D)"),
            new TestCaseData(nameof(OverridableSpecialTypeTestAsset.limitList), "[5, Default]"),
        };

        private static object ReadDefault(FieldInfo field)
        {
            var attribute = field.GetCustomAttribute<OverridableDefaultAttribute>();
            return typeof(SpecialTypeTestDefaults).GetProperty(attribute.MemberName).GetValue(obj: null);
        }

        private static void AssertLocate(GenericMenu menu)
        {
            var items = (System.Collections.IList)typeof(GenericMenu)
                .GetProperty("menuItems", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menu);
            var last = items[items.Count - 1];
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var content = (GUIContent)last.GetType().GetField("content", flags).GetValue(last);
            Assert.AreEqual("Locate Default Source", content.text);
            Assert.IsNotNull(last.GetType().GetField("func", flags).GetValue(last));
        }

        private static void AssertValue(object expected, object actual)
        {
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.GetType(), actual.GetType());

            switch (expected)
            {
                case AnimationCurve curve:
                {
                    var copy = (AnimationCurve)actual;
                    CollectionAssert.AreEqual(curve.keys, copy.keys);
                    Assert.AreEqual(curve.preWrapMode, copy.preWrapMode);
                    Assert.AreEqual(curve.postWrapMode, copy.postWrapMode);
                    break;
                }

                case Gradient gradient:
                {
                    var copy = (Gradient)actual;
                    CollectionAssert.AreEqual(gradient.colorKeys, copy.colorKeys);
                    CollectionAssert.AreEqual(gradient.alphaKeys, copy.alphaKeys);
                    Assert.AreEqual(gradient.mode, copy.mode);
                    break;
                }

                case SpecialTypeTestSummary summary:
                {
                    Assert.AreEqual(summary.text, ((SpecialTypeTestSummary)actual).text);
                    break;
                }

                case SpecialTypeTestBudget budget:
                {
                    var copy = (SpecialTypeTestBudget)actual;
                    Assert.AreEqual(budget.Count, copy.Count);
                    Assert.AreEqual(budget.Delay, copy.Delay);
                    break;
                }

                case SpecialTypeTestFixedSamples samples:
                {
                    AssertBuffers(samples, (SpecialTypeTestFixedSamples)actual);
                    break;
                }

                case SpecialTypeTestReferenceGraph graph:
                {
                    var copy = (SpecialTypeTestReferenceGraph)actual;
                    Assert.AreEqual(graph.root.number, copy.root.number);
                    Assert.AreEqual(graph.alias.number, copy.alias.number);
                    Assert.AreEqual(ReferenceEquals(graph.root, graph.alias), ReferenceEquals(copy.root, copy.alias));
                    Assert.AreEqual(graph.optional?.number, copy.optional?.number);
                    CollectionAssert.AreEqual(graph.ids, copy.ids);
                    break;
                }

                case ExposedReference<Texture2D> reference:
                {
                    var copy = (ExposedReference<Texture2D>)actual;
                    Assert.AreEqual(reference.exposedName, copy.exposedName);
                    Assert.AreSame(reference.defaultValue, copy.defaultValue);
                    break;
                }

                case List<Overridable<int>> list:
                {
                    CollectionAssert.AreEqual(list, (List<Overridable<int>>)actual);
                    break;
                }

                case int[] array:
                {
                    CollectionAssert.AreEqual(array, (int[])actual);
                    break;
                }

                default:
                {
                    Assert.AreEqual(expected, actual);
                    break;
                }
            }
        }

        private static unsafe void AssertBuffers(
              SpecialTypeTestFixedSamples expected
            , SpecialTypeTestFixedSamples actual
        )
        {
            for (var i = 0; i < 2; i++)
            {
                Assert.AreEqual(expected.signedValues[i], actual.signedValues[i], "signedValues");
                Assert.AreEqual(expected.unsignedValues[i], actual.unsignedValues[i], "unsignedValues");
                Assert.AreEqual(expected.longValues[i], actual.longValues[i], "longValues");
                Assert.AreEqual(expected.ulongValues[i], actual.ulongValues[i], "ulongValues");
                Assert.AreEqual(expected.floatValues[i], actual.floatValues[i], "floatValues");
                Assert.AreEqual(expected.doubleValues[i], actual.doubleValues[i], "doubleValues");
                Assert.AreEqual(expected.boolValues[i], actual.boolValues[i], "boolValues");
                Assert.AreEqual(expected.charValues[i], actual.charValues[i], "charValues");
            }
        }

        [SetUp]
        public void SetUp()
        {
            _previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            _defaultTexture = new Texture2D(width: 1, height: 1) { name = "Default texture" };
            _storedTexture = new Texture2D(width: 1, height: 1) { name = "Stored texture" };
            SpecialTypeTestDefaults.Material = _defaultTexture;
            _asset = ScriptableObject.CreateInstance<OverridableSpecialTypeTestAsset>();
            _source = new SerializedObject(_asset);
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
            UnityEngine.Object.DestroyImmediate(_holder);
            UnityEngine.Object.DestroyImmediate(_asset);
            UnityEngine.Object.DestroyImmediate(_defaultTexture);
            UnityEngine.Object.DestroyImmediate(_storedTexture);
            SpecialTypeTestDefaults.Material = null;
            CultureInfo.CurrentCulture = _previousCulture;
        }

        [TestCaseSource(nameof(FieldNames))]
        public void HolderCopiesDefaultWithoutChangingStoredValue(string fieldName)
        {
            var field = typeof(OverridableSpecialTypeTestAsset).GetField(fieldName);
            var valueType = OverridableEditorAPI.GetValueType(field.FieldType);
            var stored = CreateStoredValue(fieldName);
            SetStored(field, CreateStoredValue(fieldName));
            var expected = ReadDefault(field);
            var property = _source.FindProperty(fieldName).FindPropertyRelative(OverridableEditorAPI.VALUE);
            _holder = OverridableDefaultHolder.Create(valueType);
            var copy = _holder.Write(expected, property);

            AssertValue(expected, SerializedValueCopy.Read(copy, valueType));
            AssertValue(stored, ReadStored(field));
            Assert.IsFalse(ReadOverride(field));

            if (expected is SpecialTypeTestSummary
                or SpecialTypeTestReferenceGraph
                or int[]
                or List<Overridable<int>>
            )
            {
                Assert.AreNotSame(expected, SerializedValueCopy.Read(copy, valueType));
            }

            SerializedValueCopy.Write(copy, stored, valueType);
            copy.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            AssertValue(stored, SerializedValueCopy.Read(copy, valueType));
            AssertValue(ReadDefault(field), expected);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCaseSource(nameof(TooltipCases))]
        public void DefaultTooltipContainsReadableValueAndExactSource(string fieldName, string expectedText)
        {
            var field = typeof(OverridableSpecialTypeTestAsset).GetField(fieldName);
            var attribute = field.GetCustomAttribute<OverridableDefaultAttribute>();
            var valueType = OverridableEditorAPI.GetValueType(field.FieldType);
            var expected = $"Default value: {expectedText}\nFrom: SpecialTypeTestDefaults ▸ {attribute.MemberName}";
            Assert.AreEqual(expected, OverridableEditorAPI.GetDefaultTooltip(attribute, valueType));
        }

        [TestCaseSource(nameof(SwitchCases))]
        public void SwitchAndUndoPreserveValuesAndExposeLocate(string fieldName, bool useToolkit)
        {
            var field = typeof(OverridableSpecialTypeTestAsset).GetField(fieldName);
            var valueType = OverridableEditorAPI.GetValueType(field.FieldType);
            var attribute = field.GetCustomAttribute<OverridableDefaultAttribute>();
            var stored = CreateStoredValue(fieldName);
            var expected = ReadDefault(field);
            SetStored(field, CreateStoredValue(fieldName));
            var property = _source.FindProperty(fieldName);
            property.FindPropertyRelative(OverridableEditorAPI.VALUE).isExpanded = true;
            OverridableRowBuilder builder = null;
            OverridableImguiRow imgui = null;
            OverridableSwitch toggle = null;
            VisualElement valueField = null;

            if (useToolkit)
            {
                var previous = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
                _window = ScriptableObject.CreateInstance<SpecialTypeWindow>();
                _window.Show();
                builder = new OverridableRowBuilder(property, field, valueType, attribute);
                _window.rootVisualElement.Add(builder.Root);
                builder.Root.Bind(_source);
                toggle = builder.Root.Q<OverridableSwitch>();
                valueField = builder.Root.Q<VisualElement>(className: OverridableRowBuilder.VALUE_FIELD_CLASS_NAME);
                Assert.IsNotNull(toggle);
                Assert.IsNotNull(valueField);
                Assert.IsFalse(valueField.enabledSelf);
                Assert.AreEqual("_overridableDefault.value", ((IBindable)valueField).bindingPath);
                Assert.AreEqual(
                      OverridableEditorAPI.GetDefaultTooltip(attribute, valueType)
                    , valueField.parent.tooltip
                );
                AssertLocate(builder.BuildContextMenu());

                var holders = Resources.FindObjectsOfTypeAll<OverridableDefaultHolder>();
                OverridableDefaultHolder preview = null;

                for (var i = 0; i < holders.Length; i++)
                {
                    if (Array.IndexOf(previous, holders[i]) < 0
                        && holders[i].Box.GetType().GetGenericArguments()[0] == valueType
                    )
                    {
                        Assert.IsTrue(preview.IsInvalid(), "A single row should own exactly one preview holder.");
                        preview = holders[i];
                    }
                }

                Assert.IsTrue(preview.IsValid());
                AssertValue(expected, SerializedValueCopy.Read(preview.GetValueProperty(), valueType));

                var header = builder.Root.Q<Foldout>(className: "encosy-overridable__header-label");
                var expectedHeader = fieldName is "summary" or "spawnBudget" or "waveArray"
                    or "fixedSamples" or "referenceGraph" or "limitList";
                Assert.AreEqual(expectedHeader, header != null);
            }
            else
            {
                imgui = OverridableImguiRow.Get(property, field);
                AssertValue(expected, SerializedValueCopy.Read(imgui.GetDrawnProperty(), valueType));
                AssertLocate(imgui.BuildContextMenu(property.displayName));
            }

            AssertValue(stored, ReadStored(field));
            Assert.IsFalse(ReadOverride(field));
            Undo.IncrementCurrentGroup();
            SetOverride(true);
            Assert.IsTrue(ReadOverride(field));
            AssertValue(expected, ReadStored(field));
            Undo.PerformUndo();
            _source.Update();
            builder?.Refresh();
            Assert.IsFalse(ReadOverride(field));
            AssertValue(stored, ReadStored(field));

            SetOverride(true);
            var value = _source.FindProperty(fieldName).FindPropertyRelative(OverridableEditorAPI.VALUE);
            SerializedValueCopy.Write(value, stored, valueType);
            _source.ApplyModifiedPropertiesWithoutUndo();
            builder?.Refresh();
            Undo.ClearUndo(_asset);
            Undo.IncrementCurrentGroup();
            SetOverride(false);
            Assert.IsFalse(ReadOverride(field));
            AssertValue(stored, ReadStored(field));
            Undo.PerformUndo();
            _source.Update();
            builder?.Refresh();
            Assert.IsTrue(ReadOverride(field));
            AssertValue(stored, ReadStored(field));
            LogAssert.NoUnexpectedReceived();

            void SetOverride(bool active)
            {
                if (useToolkit)
                {
                    toggle.value = active;
                    Assert.AreSame(
                          valueField
                        , builder.Root.Q<VisualElement>(className: OverridableRowBuilder.VALUE_FIELD_CLASS_NAME)
                    );
                    Assert.AreEqual(active, valueField.enabledSelf);

                    var expectedPath = active ? fieldName + ".value" : "_overridableDefault.value";
                    Assert.AreEqual(expectedPath, ((IBindable)valueField).bindingPath);
                    var tooltip = active ? string.Empty : OverridableEditorAPI.GetDefaultTooltip(attribute, valueType);
                    Assert.AreEqual(tooltip, valueField.parent.tooltip);
                }
                else
                {
                    imgui.SetOverride(active);
                    var drawn = imgui.GetDrawnProperty();
                    if (active)
                    {
                        Assert.AreSame(_asset, drawn.serializedObject.targetObject);
                    }
                    else
                    {
                        Assert.IsInstanceOf<OverridableDefaultHolder>(drawn.serializedObject.targetObject);
                    }

                    AssertValue(active ? ReadStored(field) : expected, SerializedValueCopy.Read(drawn, valueType));
                }
            }
        }

        [TestCase(nameof(OverridableSpecialTypeTestAsset.summary), false)]
        [TestCase(nameof(OverridableSpecialTypeTestAsset.summary), true)]
        [TestCase(nameof(OverridableSpecialTypeTestAsset.presetMaterial), false)]
        [TestCase(nameof(OverridableSpecialTypeTestAsset.presetMaterial), true)]
        public void PresetsMatchShownDefaultAndUndoRestoresStoredValue(string fieldName, bool useToolkit)
        {
            var field = typeof(OverridableSpecialTypeTestAsset).GetField(fieldName);
            var valueType = OverridableEditorAPI.GetValueType(field.FieldType);
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            var defaultAttribute = field.GetCustomAttribute<OverridableDefaultAttribute>();
            var stored = CreateStoredValue(fieldName);
            SetStored(field, CreateStoredValue(fieldName));
            var property = _source.FindProperty(fieldName);

            Assert.IsTrue(ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , valueType
                , out var choices
                , out var error
            ), error.ToMessage());

            Assert.IsTrue(ValueChoicesEditorAPI.Matches(choices[0].value, ReadDefault(field)));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(choices[0].value, stored));
            GenericMenu menu;

            if (useToolkit)
            {
                _window = ScriptableObject.CreateInstance<SpecialTypeWindow>();
                _window.Show();

                var builder = new OverridableRowBuilder(
                      property: property
                    , fieldInfo: field
                    , valueType: valueType
                    , attribute: defaultAttribute
                    , label: null
                    , choices: choices
                    , choicesAttribute: attribute
                );

                _window.rootVisualElement.Add(builder.Root);
                builder.Root.Bind(_source);
                menu = builder.BuildChoiceMenu();
            }
            else
            {
                menu = OverridableImguiRow.Get(property, field).BuildChoiceMenu(choices, attribute);
            }

            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
            var items = (System.Collections.IList)typeof(GenericMenu).GetProperty("menuItems", FLAGS).GetValue(menu);
            Assert.AreEqual(expected: 2, actual: items.Count);
            var first = items[0];
            var second = items[1];
            Assert.IsTrue((bool)first.GetType().GetField("on").GetValue(first));
            Assert.IsFalse((bool)second.GetType().GetField("on").GetValue(second));
            var callback = (GenericMenu.MenuFunction)second.GetType().GetField("func").GetValue(second);
            Undo.IncrementCurrentGroup();
            callback();
            Assert.IsTrue(ReadOverride(field));

            if (fieldName == nameof(OverridableSpecialTypeTestAsset.summary))
            {
                Assert.AreEqual("On hold", ((SpecialTypeTestSummary)ReadStored(field)).text);
            }
            else
            {
                Assert.IsNull(ReadStored(field));
            }

            Undo.PerformUndo();
            _source.Update();
            Assert.IsFalse(ReadOverride(field));
            AssertValue(stored, ReadStored(field));
            LogAssert.NoUnexpectedReceived();
        }

        private void SetStored(FieldInfo field, object value)
        {
            var wrapper = Activator.CreateInstance(field.FieldType);
            field.FieldType.GetField(OverridableEditorAPI.VALUE).SetValue(wrapper, value);
            field.SetValue(_asset, wrapper);
            _source.Update();
        }

        private object ReadStored(FieldInfo field)
            => field.FieldType.GetField(OverridableEditorAPI.VALUE).GetValue(field.GetValue(_asset));

        private bool ReadOverride(FieldInfo field)
            => (bool)field.FieldType.GetField(OverridableEditorAPI.IS_OVERRIDDEN).GetValue(field.GetValue(_asset));

        private object CreateStoredValue(string fieldName)
            => fieldName switch {
                "accentColor" => Color.magenta,
                "summary" => new SpecialTypeTestSummary { text = "Stored summary" },
                "spawnBudget" => new SpecialTypeTestBudget { Count = 99, Delay = 2.5f },
                "waveArray" => new[] { 91, 82 },
                "fixedSamples" => new SpecialTypeTestFixedSamples(),
                "referenceGraph" => new SpecialTypeTestReferenceGraph {
                    root = new() { number = 99 },
                    alias = new() { number = 88 },
                    optional = new() { number = 77 },
                    ids = Array.Empty<EncosyTower.Common.SerializableGuid>(),
                },
                "collisionMask" => (LayerMask)2,
                "responseCurve" => AnimationCurve.Linear(timeStart: 0f, valueStart: 9f, timeEnd: 2f, valueEnd: 8f),
                "heatGradient" => new Gradient(),
                "viewport" => new Rect(x: -1f, y: -2f, width: 3f, height: 4f),
                "spawnBounds" => new Bounds(Vector3.zero, Vector3.one),
                "gridCell" => new Vector2Int(x: -9, y: 8),
                "orientation" => Quaternion.identity,
                "contentHash" => Hash128.Parse("fedcba9876543210fedcba9876543210"),
                "presetMaterial" => _storedTexture,
                "limitList" => new List<Overridable<int>> { new(value: 91, isOverridden: true) },
                "marker" => 'Z',
                "population" => -9000000001L,
                "precision" => -0.987654321098765d,
                "uvScale" => Vector2.one,
                "weights" => Vector4.one,
                "gridPosition" => Vector3Int.one,
                "tileArea" => new RectInt(xMin: -1, yMin: -2, width: 3, height: 4),
                "cellBounds" => new BoundsInt(Vector3Int.zero, Vector3Int.one),
                "exposedMaterial" => new ExposedReference<Texture2D> {
                    exposedName = new PropertyName("stored-texture"),
                    defaultValue = _storedTexture,
                },
                _ => null,
            };

        private sealed class SpecialTypeWindow : EditorWindow
        {
        }
    }
}

#endif
