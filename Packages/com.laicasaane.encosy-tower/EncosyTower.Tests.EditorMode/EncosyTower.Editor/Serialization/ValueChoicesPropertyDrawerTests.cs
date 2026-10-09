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
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class ValueChoicesPropertyDrawerTests
    {
        private OverridableChoiceTestAsset _asset;
        private OverridableChoiceTestAsset _secondAsset;
        private PlainChoicesTestAsset _plainAsset;
        private SerializedObject _serializedObject;
        private SerializedObject _plainObject;
        private ChoicesTestWindow _window;
        private int _previousFrameRate;
        private string _previousRegion;
        private TestQuality _previousTierLimit;

        private static string GetPopupText(OverridableChoiceField field)
            => field.Q<TextElement>(className: BasePopupField<string, string>.textUssClassName).text;

        [SetUp]
        public void SetUp()
        {
            _previousFrameRate = TestDefaults.FrameRate;
            _previousRegion = TestDefaults.Region;
            _previousTierLimit = TestDefaults.TierLimit;
            TestDefaults.FrameRate = 60;
            TestDefaults.Region = "asia";
            TestDefaults.TierLimit = TestQuality.High;
            _asset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();
            _serializedObject = new SerializedObject(_asset);
            _window = ScriptableObject.CreateInstance<ChoicesTestWindow>();
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
            _plainObject?.Dispose();
            _plainObject = null;

            if (_plainAsset.IsValid())
            {
                Undo.ClearUndo(_plainAsset);
                Object.DestroyImmediate(_plainAsset);
            }

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

            TestDefaults.FrameRate = _previousFrameRate;
            TestDefaults.Region = _previousRegion;
            TestDefaults.TierLimit = _previousTierLimit;
        }

        [Test]
        public void Region_PopupUsesLabelsAndWritesValuesThenReturnsToDefault()
        {
            var row = BindField(nameof(OverridableChoiceTestAsset.region));
            var popup = row.Q<OverridableChoiceField>();

            Assert.IsNotNull(popup);
            CollectionAssert.AreEqual(new[] { "Default (Asia)", "Asia", "Europe", "North America" }, popup.Choices);
            Assert.IsTrue(popup.HasDefaultChoice);
            Assert.AreEqual(expected: 0, actual: popup.value);
            Assert.IsFalse(_asset.region.isOverridden);

            popup.value = 2;

            Assert.AreEqual("eu", _asset.region.value);
            Assert.IsTrue(_asset.region.isOverridden);
            Assert.AreEqual("Europe", GetPopupText(popup));
            Assert.IsEmpty(popup.InputTooltip);

            popup.value = 0;

            Assert.AreEqual("eu", _asset.region.value);
            Assert.IsFalse(_asset.region.isOverridden);
            Assert.AreEqual("Default (Asia)", GetPopupText(popup));
            Assert.IsNotEmpty(popup.InputTooltip);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void StrictTierLimit_UnlistedValueWarnsUntilAnAllowedChoiceIsSelected()
        {
            var row = BindField(nameof(OverridableChoiceTestAsset.strictTierLimit));
            var popup = row.Q<OverridableChoiceField>();
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            CollectionAssert.AreEqual(new[] { "Default (High)", "Low", "High", "Ultra (Experimental)" }, popup.Choices);
            Assert.AreEqual(expected: -1, actual: popup.value);
            Assert.AreEqual("Medium", GetPopupText(popup));
            Assert.IsFalse(popup.showMixedValue);
            Assert.IsNotNull(warning);
            Assert.AreEqual(DisplayStyle.Flex, warning.style.display.value);
            StringAssert.Contains("Medium", warning.tooltip);
            Assert.AreEqual(TestQuality.Medium, _asset.strictTierLimit.value);

            popup.value = 3;

            Assert.AreEqual(TestQuality.Ultra, _asset.strictTierLimit.value);
            Assert.IsTrue(_asset.strictTierLimit.isOverridden);
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);
            Assert.IsEmpty(warning.tooltip);
            Assert.AreEqual("Ultra (Experimental)", GetPopupText(popup));

            popup.value = 0;

            Assert.IsFalse(_asset.strictTierLimit.isOverridden);
            Assert.AreEqual(TestQuality.Ultra, _asset.strictTierLimit.value);
        }

        [Test]
        public void StrictTierLimit_DefaultSelectionDoesNotWarnAboutInactiveStoredValue()
        {
            _asset.strictTierLimit = new(value: TestQuality.Medium, isOverridden: false);
            _serializedObject.Update();
            var row = BindField(nameof(OverridableChoiceTestAsset.strictTierLimit));
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            Assert.AreEqual(expected: 0, actual: row.Q<OverridableChoiceField>().value);
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);
            Assert.AreEqual(TestQuality.Medium, _asset.strictTierLimit.value);
            Assert.IsFalse(_asset.strictTierLimit.isOverridden);
        }

        [Test]
        public void TierLimit_NonExclusivePopupIncludesEveryEnumValueAndInspectorName()
        {
            var row = BindField(nameof(OverridableChoiceTestAsset.tierLimit));
            var popup = row.Q<OverridableChoiceField>();

            CollectionAssert.AreEqual(
                  new[] { "Default (High)", "Low", "Medium", "High", "Ultra (Experimental)" }
                , popup.Choices
            );

            popup.value = 2;

            Assert.AreEqual(TestQuality.Medium, _asset.tierLimit.value);
            Assert.IsTrue(_asset.tierLimit.isOverridden);
            Assert.IsNull(row.Q<Image>(className: "encosy-overridable__warning"));

            popup.value = 4;

            Assert.AreEqual(TestQuality.Ultra, _asset.tierLimit.value);
            Assert.AreEqual("Ultra (Experimental)", GetPopupText(popup));
        }

        [Test]
        public void RegionCode_PlainPopupWarnsAndWritesTheChoiceValueWithoutADefaultEntry()
        {
            var row = BindField(nameof(OverridableChoiceTestAsset.regionCode));
            var popup = row.Q<OverridableChoiceField>();
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            CollectionAssert.AreEqual(new[] { "Asia", "Europe", "North America" }, popup.Choices);
            Assert.IsFalse(popup.HasDefaultChoice);
            Assert.AreEqual(expected: -1, actual: popup.value);
            Assert.AreEqual("xx", GetPopupText(popup));
            Assert.IsNotNull(warning);
            Assert.AreEqual(DisplayStyle.Flex, warning.style.display.value);
            StringAssert.Contains("xx", warning.tooltip);
            Assert.AreEqual("xx", _asset.regionCode);
            Assert.IsNull(row.Q<Button>(className: "encosy-overridable__more"));

            popup.value = 0;

            Assert.AreEqual("asia", _asset.regionCode);
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);
            Assert.AreEqual("Asia", GetPopupText(popup));
        }

        [Test]
        public void TargetFrameRate_KeepsCustomValueAndNormalEditableFieldWithChoicesButton()
        {
            var row = BindField(nameof(OverridableChoiceTestAsset.targetFrameRate));
            var input = row.Q<IntegerField>();

            Assert.IsNotNull(input);
            Assert.AreEqual(expected: 75, actual: input.value);
            Assert.AreEqual(expected: 75, actual: _asset.targetFrameRate);
            Assert.IsNotNull(row.Q<Button>(className: "encosy-overridable__more"));
            Assert.IsNull(row.Q<OverridableChoiceField>());

            input.value = 91;

            Assert.AreEqual(expected: 91, actual: _asset.targetFrameRate);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(0, 30)]
        [TestCase(1, 144)]
        public void FrameRateSteps_EachElementKeepsItsValueAndChoicesButton(int index, int expected)
        {
            var property = _serializedObject.FindProperty(nameof(OverridableChoiceTestAsset.frameRateSteps));
            var element = property.GetArrayElementAtIndex(index);
            var row = BindField(element.propertyPath);
            var input = row.Q<IntegerField>();

            Assert.IsNotNull(input);
            Assert.AreEqual(expected, input.value);
            Assert.IsNotNull(row.Q<Button>(className: "encosy-overridable__more"));

            input.value = 77;

            Assert.AreEqual(expected: 77, actual: _asset.frameRateSteps[index]);
            Assert.AreEqual(index == 0 ? 144 : 30, _asset.frameRateSteps[1 - index]);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void FrameRate_PresetWritesValueAndOverrideInOneUndoStep()
        {
            _asset.frameRate = new(value: 75, isOverridden: false);
            _serializedObject.Update();
            var builder = CreateBuilder(nameof(OverridableChoiceTestAsset.frameRate));
            var items = builder.GetChoiceMenuItems();

            Assert.AreEqual(expected: 3, actual: items.Length);
            Assert.IsFalse(items[0].IsChecked);
            Assert.IsTrue(items[1].IsChecked);
            Assert.IsFalse(items[2].IsChecked);

            Undo.IncrementCurrentGroup();
            items[2].Callback();

            Assert.AreEqual(expected: 120, actual: _asset.frameRate.value);
            Assert.IsTrue(_asset.frameRate.isOverridden);
            Assert.IsTrue(builder.GetChoiceMenuItems()[2].IsChecked);
            Assert.IsFalse(builder.GetChoiceMenuItems()[1].IsChecked);

            Undo.PerformUndo();
            builder.Refresh();

            Assert.AreEqual(expected: 75, actual: _asset.frameRate.value);
            Assert.IsFalse(_asset.frameRate.isOverridden);
            Assert.IsTrue(builder.GetChoiceMenuItems()[1].IsChecked);
        }

        [Test]
        public void TierLimit_PresetsUseChoiceValuesAndExclusiveMenuHasNoPresets()
        {
            var builder = CreateBuilder(nameof(OverridableChoiceTestAsset.tierLimit));
            var items = builder.GetChoiceMenuItems();

            Assert.AreEqual(expected: 3, actual: items.Length);
            Assert.IsTrue(items[1].IsChecked);
            Assert.AreEqual("Ultra (Experimental)", items[2].Content.text);

            items[2].Callback();

            Assert.AreEqual(TestQuality.Ultra, _asset.tierLimit.value);
            Assert.IsTrue(_asset.tierLimit.isOverridden);
            var strict = CreateBuilder(nameof(OverridableChoiceTestAsset.strictTierLimit));

            Assert.IsEmpty(strict.GetChoiceMenuItems());
            Assert.IsNull(strict.Root.Q<Button>(className: "encosy-overridable__more"));
        }

        [Test]
        public void LayerPresets_ToggleUsesNativeMaskFieldAndAllowsCustomCombinations()
        {
            var row = BindField(nameof(OverridableChoiceTestAsset.layerPresets));
            var toggle = row.Q<OverridableSwitch>();
            var mask = row.Q<EnumFlagsField>();

            Assert.IsNotNull(toggle);
            Assert.IsFalse(toggle.value);
            Assert.IsNotNull(mask);
            Assert.AreEqual(TestLayers.None, mask.value);
            Assert.IsFalse(mask.enabledInHierarchy);
            Assert.IsNull(row.Q<OverridableChoiceField>());
            Assert.IsNotNull(row.Q<Button>(className: "encosy-overridable__more"));

            toggle.value = true;
            mask = row.Q<EnumFlagsField>();

            Assert.IsTrue(mask.enabledInHierarchy);
            mask.value = TestLayers.Ground | TestLayers.Water;

            Assert.AreEqual(TestLayers.Ground | TestLayers.Water, _asset.layerPresets.value);
            Assert.IsTrue(_asset.layerPresets.isOverridden);
            Assert.IsNull(row.Q<Image>(className: "encosy-overridable__warning"));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LayerPresets_MenuChecksExactCombinationAndSelectionHasOneUndoStep()
        {
            _asset.layerPresets = new(value: TestLayers.Water, isOverridden: false);
            _serializedObject.Update();
            var builder = CreateBuilder(nameof(OverridableChoiceTestAsset.layerPresets));
            var items = builder.GetChoiceMenuItems();

            Assert.AreEqual(expected: 3, actual: items.Length);
            Assert.AreEqual("Ground only", items[0].Content.text);
            Assert.AreEqual("Ground and Air", items[1].Content.text);
            Assert.AreEqual("Water", items[2].Content.text);
            Assert.AreEqual(expected: 3, actual: builder.BuildChoiceMenu().GetItemCount());
            Assert.IsFalse(items[0].IsChecked);
            Assert.IsFalse(items[1].IsChecked);
            Assert.IsFalse(items[2].IsChecked);

            Undo.IncrementCurrentGroup();
            items[1].Callback();

            Assert.AreEqual(TestLayers.Ground | TestLayers.Air, _asset.layerPresets.value);
            Assert.IsTrue(_asset.layerPresets.isOverridden);
            items = builder.GetChoiceMenuItems();
            Assert.IsFalse(items[0].IsChecked);
            Assert.IsTrue(items[1].IsChecked);
            Assert.IsFalse(items[2].IsChecked);

            Undo.PerformUndo();
            builder.Refresh();

            Assert.AreEqual(TestLayers.Water, _asset.layerPresets.value);
            Assert.IsFalse(_asset.layerPresets.isOverridden);
            Assert.IsFalse(builder.GetChoiceMenuItems()[2].IsChecked);
        }

        [Test]
        public void StrictLayerPresets_PopupWarnsForAirAndListsOnlyPresetsWithBareDefault()
        {
            var builder = CreateBuilder(nameof(OverridableChoiceTestAsset.strictLayerPresets));
            var row = builder.Root;
            var popup = row.Q<OverridableChoiceField>();
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            Assert.IsNull(row.Q<OverridableSwitch>());
            Assert.IsNull(row.Q<EnumFlagsField>());
            Assert.IsTrue(popup.HasDefaultChoice);
            CollectionAssert.AreEqual(new[] { "Default", "Ground only", "Ground and Air", "Water" }, popup.Choices);
            Assert.AreEqual(expected: -1, actual: popup.value);
            Assert.AreEqual("Air", GetPopupText(popup));
            Assert.AreEqual(DisplayStyle.Flex, warning.style.display.value);
            StringAssert.Contains("Air", warning.tooltip);
            Assert.IsEmpty(builder.GetChoiceMenuItems());
            Assert.AreEqual(expected: 0, actual: builder.BuildChoiceMenu().GetItemCount());
            Assert.IsNull(row.Q<Button>(className: "encosy-overridable__more"));

            popup.value = 2;

            Assert.AreEqual(TestLayers.Ground | TestLayers.Air, _asset.strictLayerPresets.value);
            Assert.IsTrue(_asset.strictLayerPresets.isOverridden);
            Assert.AreEqual("Ground and Air", GetPopupText(popup));
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);

            popup.value = 0;

            Assert.IsFalse(_asset.strictLayerPresets.isOverridden);
            Assert.AreEqual(TestLayers.Ground | TestLayers.Air, _asset.strictLayerPresets.value);
            Assert.AreEqual("Default", GetPopupText(popup));
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void FrameRate_DisabledRowRejectsPreviouslyOpenedPreset()
        {
            var builder = CreateBuilder(nameof(OverridableChoiceTestAsset.frameRate));
            var items = builder.GetChoiceMenuItems();
            builder.Root.SetEnabled(false);
            Assert.AreEqual(expected: 0, actual: builder.BuildChoiceMenu().GetItemCount());
            Assert.AreEqual(expected: 0, actual: builder.BuildContextMenu().GetItemCount());
            items[0].Callback();

            Assert.AreEqual(expected: 0, actual: _asset.frameRate.value);
            Assert.IsFalse(_asset.frameRate.isOverridden);
        }

        [Test]
        public void StrictTierLimit_MixedValuesShowMixedWithoutWarningAndSelectionAppliesToBoth()
        {
            _secondAsset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();
            _secondAsset.strictTierLimit = new(value: TestQuality.Low, isOverridden: true);
            _serializedObject.Dispose();
            _serializedObject = new SerializedObject(new Object[] { _asset, _secondAsset });
            var row = BindField(nameof(OverridableChoiceTestAsset.strictTierLimit));
            var popup = row.Q<OverridableChoiceField>();
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            Assert.IsTrue(popup.showMixedValue);
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);

            popup.value = 2;

            Assert.AreEqual(TestQuality.High, _asset.strictTierLimit.value);
            Assert.AreEqual(TestQuality.High, _secondAsset.strictTierLimit.value);
            Assert.IsTrue(_asset.strictTierLimit.isOverridden);
            Assert.IsTrue(_secondAsset.strictTierLimit.isOverridden);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void PlainEnum_NonExclusivePopupIncludesUnlistedEnumValuesAndChoicesButton()
        {
            var row = BindPlainField(nameof(PlainChoicesTestAsset.quality));
            var popup = row.Q<OverridableChoiceField>();

            CollectionAssert.AreEqual(new[] { "Low", "Medium", "High", "Ultra (Experimental)" }, popup.Choices);
            Assert.IsFalse(popup.HasDefaultChoice);
            Assert.AreEqual(expected: 1, actual: popup.value);
            Assert.IsNotNull(row.Q<Button>(className: "encosy-overridable__more"));
            Assert.IsNull(row.Q<Image>(className: "encosy-overridable__warning"));

            popup.value = 3;

            Assert.AreEqual(TestQuality.Ultra, _plainAsset.quality);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void PlainFlags_NonExclusiveUsesNativeMaskFieldAndChoicesButton()
        {
            var row = BindPlainField(nameof(PlainChoicesTestAsset.layers));
            var mask = row.Q<EnumFlagsField>();

            Assert.IsNotNull(mask);
            Assert.AreEqual(TestLayers.Air, mask.value);
            Assert.IsTrue(mask.enabledInHierarchy);
            Assert.IsNull(row.Q<OverridableChoiceField>());
            Assert.IsNotNull(row.Q<Button>(className: "encosy-overridable__more"));
            Assert.IsNull(row.Q<Image>(className: "encosy-overridable__warning"));

            mask.value = TestLayers.Ground | TestLayers.Water;

            Assert.AreEqual(TestLayers.Ground | TestLayers.Water, _plainAsset.layers);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void PlainFlags_ExclusivePopupWarnsAndWritesAllowedCombination()
        {
            var row = BindPlainField(nameof(PlainChoicesTestAsset.strictLayers));
            var popup = row.Q<OverridableChoiceField>();
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            Assert.IsNull(row.Q<EnumFlagsField>());
            Assert.IsFalse(popup.HasDefaultChoice);
            CollectionAssert.AreEqual(new[] { "Ground only", "Ground and Air", "Water" }, popup.Choices);
            Assert.AreEqual(expected: -1, actual: popup.value);
            Assert.AreEqual("Air", GetPopupText(popup));
            Assert.AreEqual(DisplayStyle.Flex, warning.style.display.value);
            StringAssert.Contains("Air", warning.tooltip);
            Assert.IsNull(row.Q<Button>(className: "encosy-overridable__more"));

            popup.value = 1;

            Assert.AreEqual(TestLayers.Ground | TestLayers.Air, _plainAsset.strictLayers);
            Assert.AreEqual("Ground and Air", GetPopupText(popup));
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(0, 30)]
        [TestCase(1, 144)]
        public void PlainArray_ElementKeepsItsValueAndChoicesButton(int index, int expected)
        {
            var row = BindPlainField($"{nameof(PlainChoicesTestAsset.steps)}.Array.data[{index}]");
            var input = row.Q<IntegerField>();

            Assert.IsNotNull(input);
            Assert.AreEqual(expected, input.value);
            Assert.IsNotNull(row.Q<Button>(className: "encosy-overridable__more"));

            input.value = 87;

            Assert.AreEqual(expected: 87, actual: _plainAsset.steps[index]);
            Assert.AreEqual(index == 0 ? 144 : 30, _plainAsset.steps[1 - index]);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void RegionCode_MixedValuesShowMixedWithoutWarningAndSelectionAppliesToBoth()
        {
            _secondAsset = ScriptableObject.CreateInstance<OverridableChoiceTestAsset>();
            _secondAsset.regionCode = "asia";
            _serializedObject.Dispose();
            _serializedObject = new SerializedObject(new Object[] { _asset, _secondAsset });
            var row = BindField(nameof(OverridableChoiceTestAsset.regionCode));
            var popup = row.Q<OverridableChoiceField>();
            var warning = row.Q<Image>(className: "encosy-overridable__warning");

            Assert.IsTrue(popup.showMixedValue);
            Assert.AreEqual(DisplayStyle.None, warning.style.display.value);

            popup.value = 2;

            Assert.AreEqual("na", _asset.regionCode);
            Assert.AreEqual("na", _secondAsset.regionCode);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void CollectionImguiHeader_UsesDisplayNameAfterNestedPropertyDrawing()
        {
            var asset = ScriptableObject.CreateInstance<CollectionChoicesTestAsset>();

            try
            {
                using var source = new SerializedObject(asset);
                var property = source.FindProperty(nameof(CollectionChoicesTestAsset.wavePlan));
                GUIContent headerLabel = null;
                var drawCount = 0;
                _window.Draw = DrawHeader;

                try
                {
                    _window.SendEvent(new Event { type = EventType.Layout });
                    _window.SendEvent(new Event { type = EventType.Repaint });
                }
                finally
                {
                    _window.Draw = null;
                }

                Assert.IsTrue(drawCount > 0);
                Assert.AreEqual(property.displayName, headerLabel.text);
                CollectionAssert.AreEqual(new[] { 1, 2, 4 }, asset.wavePlan);
                Assert.IsFalse(source.hasModifiedProperties);
                LogAssert.NoUnexpectedReceived();

                void DrawHeader()
                {
                    var position = new Rect(x: 0, y: 0, width: 400, height: EditorGUIUtility.singleLineHeight);

                    headerLabel = ValueChoicesPropertyDrawer.BeginPlainProperty(
                          position
                        , property
                        , label: GUIContent.none
                        , useDisplayName: true
                    );

                    try
                    {
                        EditorGUI.BeginProperty(position, GUIContent.none, property);
                        EditorGUI.EndProperty();
                        drawCount++;
                    }
                    finally
                    {
                        EditorGUI.EndProperty();
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ResolverFailure_LogsOnceAcrossRepeatedDrawsAndResolvesFreshForReusedDrawers()
        {
            ResolverSource.FailureReads = 0;
            ResolverSource.SuccessReads = 0;
            _plainAsset = ScriptableObject.CreateInstance<PlainChoicesTestAsset>();
            _plainObject = new SerializedObject(_plainAsset);
            var drawer = new ValueChoicesPropertyDrawer();
            var sourceType = typeof(ResolverSource);
            var invalid = new ValueChoicesAttribute(sourceType, nameof(ResolverSource.InvalidChoices));
            var property = _plainObject.FindProperty(nameof(PlainChoicesTestAsset.resolverValue));
            var field = typeof(PlainChoicesTestAsset).GetField(property.name);
            var drawCount = 0;

            var warning = $"Value choices member '{sourceType.FullName}.{invalid.MemberName}': "
                + $"unsupported member type '{typeof(int)}'.";

            ConfigureDrawer(drawer, field, invalid);
            _window.Draw = DrawImgui;

            try
            {
                LogAssert.Expect(LogType.Warning, warning);
                DrawRepeatedly();
                Assert.AreEqual(expected: 1, actual: ResolverSource.FailureReads);

                property = _plainObject.FindProperty(nameof(PlainChoicesTestAsset.resolverOverride));
                field = typeof(PlainChoicesTestAsset).GetField(property.name);
                ConfigureDrawer(drawer, field, invalid);
                DrawRepeatedly();
                Assert.AreEqual(expected: 1, actual: ResolverSource.FailureReads);
                Assert.IsFalse(_plainAsset.resolverOverride.isOverridden);

                drawer = new ValueChoicesPropertyDrawer();
                ConfigureDrawer(drawer, field, invalid);
                LogAssert.Expect(LogType.Warning, warning);
                DrawRepeatedly();
                Assert.AreEqual(expected: 2, actual: ResolverSource.FailureReads);

                var valid = new ValueChoicesAttribute(sourceType, nameof(ResolverSource.ValidChoices));
                ConfigureDrawer(drawer, field, valid);
                DrawRepeatedly();
                Assert.AreEqual(expected: 1, actual: ResolverSource.SuccessReads);

                property = _serializedObject.FindProperty(nameof(OverridableChoiceTestAsset.targetFrameRate));
                field = typeof(OverridableChoiceTestAsset).GetField(property.name);
                ConfigureDrawer(drawer, field, valid);
                DrawRepeatedly();
                Assert.AreEqual(expected: 2, actual: ResolverSource.SuccessReads);
                Assert.AreEqual(expected: 75, actual: _asset.targetFrameRate);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                _window.Draw = null;
                OverridableImguiRow.ClearCache();
            }

            void DrawRepeatedly()
            {
                for (var i = 0; i < 3; i++)
                {
                    var root = drawer.CreatePropertyGUI(property);
                    _window.rootVisualElement.Add(root);
                    root.RemoveFromHierarchy();
                    drawer.GetPropertyHeight(property, GUIContent.none);
                    drawCount = 0;
                    _window.SendEvent(new Event { type = EventType.Layout });
                    _window.SendEvent(new Event { type = EventType.Repaint });
                    Assert.IsTrue(drawCount > 0);
                    LogAssert.NoUnexpectedReceived();
                }
            }

            void DrawImgui()
            {
                drawer.OnGUI(
                      position: new Rect(x: 0, y: 0, width: 400, height: EditorGUIUtility.singleLineHeight)
                    , property: property
                    , label: GUIContent.none
                );

                drawCount++;
            }

            static void ConfigureDrawer(
                  ValueChoicesPropertyDrawer drawer
                , FieldInfo field
                , ValueChoicesAttribute attribute
            )
            {
                const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
                var drawerType = typeof(PropertyDrawer);
                drawerType.GetField("m_FieldInfo", FLAGS).SetValue(drawer, field);
                drawerType.GetField("m_Attribute", FLAGS).SetValue(drawer, attribute);
            }
        }

        private PropertyField BindField(string propertyPath)
        {
            var field = new PropertyField(_serializedObject.FindProperty(propertyPath));
            _window.rootVisualElement.Add(field);
            field.Bind(_serializedObject);
            return field;
        }

        private PropertyField BindPlainField(string propertyPath)
        {
            _plainAsset = ScriptableObject.CreateInstance<PlainChoicesTestAsset>();
            _plainObject = new SerializedObject(_plainAsset);
            var field = new PropertyField(_plainObject.FindProperty(propertyPath));
            _window.rootVisualElement.Add(field);
            field.Bind(_plainObject);
            return field;
        }

        private OverridableRowBuilder CreateBuilder(string fieldName)
        {
            var property = _serializedObject.FindProperty(fieldName);
            var fieldInfo = typeof(OverridableChoiceTestAsset).GetField(fieldName);
            var valueType = OverridableEditorAPI.GetValueType(fieldInfo, property);
            var attribute = fieldInfo.GetCustomAttribute<ValueChoicesAttribute>();

            var resolved = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , fieldInfo.DeclaringType
                , valueType
                , out var choices
                , out var error
            );

            Assert.IsTrue(resolved, error.ToMessage());

            var builder = new OverridableRowBuilder(
                  property
                , fieldInfo
                , valueType
                , fieldInfo.GetCustomAttribute<OverridableDefaultAttribute>()
                , fieldName
                , choices
                , attribute
            );

            _window.rootVisualElement.Add(builder.Root);
            builder.Root.Bind(_serializedObject);
            return builder;
        }

        private static class ResolverSource
        {
            public static int FailureReads;
            public static int SuccessReads;

            public static int InvalidChoices
            {
                get
                {
                    FailureReads++;
                    return 3;
                }
            }

            public static int[] ValidChoices
            {
                get
                {
                    SuccessReads++;
                    return new[] { 3, 5 };
                }
            }
        }

        private sealed class ChoicesTestWindow : EditorWindow
        {
            public System.Action Draw { get; set; }

            private void OnGUI()
            {
                Draw?.Invoke();
            }
        }

        private sealed class PlainChoicesTestAsset : ScriptableObject
        {
            public int resolverValue = 0;
            public Overridable<int> resolverOverride = default;

            [ValueChoices(nameof(Qualities))]
            public TestQuality quality = TestQuality.Medium;

            [ValueChoices(typeof(TestChoices), nameof(TestChoices.FrameRates))]
            public int[] steps = { 30, 144 };

            [ValueChoices(nameof(Layers))]
            public TestLayers layers = TestLayers.Air;

            [ValueChoices(nameof(Layers), IsExclusive = true)]
            public TestLayers strictLayers = TestLayers.Air;

            private static TestQuality[] Qualities => new[] { TestQuality.Low, TestQuality.High };

            private static ValueChoice<TestLayers>[] Layers => new ValueChoice<TestLayers>[] {
                new("Ground only", TestLayers.Ground),
                new("Ground and Air", TestLayers.Ground | TestLayers.Air),
                new(TestLayers.Water),
            };
        }
    }
}

#endif
