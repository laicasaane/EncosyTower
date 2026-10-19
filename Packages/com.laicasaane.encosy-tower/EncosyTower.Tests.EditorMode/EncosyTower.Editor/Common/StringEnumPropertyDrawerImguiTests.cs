#if UNITY_EDITOR

using System.Reflection;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Editor.Internals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Editor.Common
{
    public sealed class StringEnumPropertyDrawerImguiTests
    {
        private StringEnumTestAsset _asset;
        private SerializedObject _serializedObject;
        private StringEnumTestWindow _window;
        private int _drawCount;

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
            _window.Draw = null;

            if (_window != null)
            {
                _window.Close();
            }

            _serializedObject.Dispose();
            Undo.ClearUndo(_asset);
            Object.DestroyImmediate(_asset);
        }

        [Test]
        public void Height_IsOneLine()
        {
            foreach (var name in new[] {
                nameof(StringEnumTestAsset.plain),
                nameof(StringEnumTestAsset.failing),
                nameof(StringEnumTestAsset.alias),
            })
            {
                var property = _serializedObject.FindProperty(name);
                var drawer = CreateDrawer(name);

                Assert.AreEqual(
                      EditorGUIUtility.singleLineHeight
                    , drawer.GetPropertyHeight(property, GUIContent.none)
                );
            }
        }

        [Test]
        public void Draw_DoesNotChangeValuesOrLog()
        {
            _asset.converted = new StringEnum<StringEnumTestKind, StringEnumTestConverter>("fire");
            _serializedObject.Update();

            DrawBoth(nameof(StringEnumTestAsset.plain), expected: string.Empty);

            _asset.plain = new StringEnum<StringEnumTestKind>("Lava");
            _serializedObject.Update();
            DrawBoth(nameof(StringEnumTestAsset.plain), expected: "Lava");
            DrawBoth(nameof(StringEnumTestAsset.converted), expected: "fire");

            var second = ScriptableObject.CreateInstance<StringEnumTestAsset>();

            try
            {
                second.plain = new StringEnum<StringEnumTestKind>("Wind");

                using var multi = new SerializedObject(new Object[] { _asset, second });
                var property = multi.FindProperty(nameof(StringEnumTestAsset.plain));
                Draw(CreateDrawer(nameof(StringEnumTestAsset.plain)), property);

                Assert.AreEqual("Lava", _asset.plain.ToString());
                Assert.AreEqual("Wind", second.plain.ToString());
            }
            finally
            {
                Object.DestroyImmediate(second);
            }

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Draw_FailingConverterFallsBackToRawField()
        {
            StringEnumEditorAPI.ClearCache();
            var type = typeof(StringEnum<StringEnumTestKind, StringEnumTestThrowingConverter>);
            LogAssert.Expect(LogType.Warning, $"StringEnum drawer cannot read the choices of {type.Name}: test failure");

            DrawBoth(nameof(StringEnumTestAsset.failing), expected: string.Empty);
            DrawBoth(nameof(StringEnumTestAsset.failing), expected: string.Empty);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Draw_ArrayElement()
        {
            _asset.list[1] = new StringEnum<StringEnumTestKind>("Fire");
            _serializedObject.Update();

            var property = _serializedObject.FindProperty("list.Array.data[1]");
            Draw(CreateDrawer(nameof(StringEnumTestAsset.list)), property);

            Assert.AreEqual("Fire", _asset.list[1].ToString());
            LogAssert.NoUnexpectedReceived();
        }

        private void DrawBoth(string fieldName, string expected)
        {
            var property = _serializedObject.FindProperty(fieldName);
            Draw(CreateDrawer(fieldName), property);

            var stored = property.FindPropertyRelative(StringEnumEditorAPI.VALUE);
            Assert.AreEqual(expected, stored.stringValue ?? string.Empty);
        }

        private void Draw(StringEnumPropertyDrawer drawer, SerializedProperty property)
        {
            _window.Draw = () => {
                drawer.OnGUI(
                      new Rect(x: 0, y: 0, width: 400, height: EditorGUIUtility.singleLineHeight)
                    , property
                    , GUIContent.none
                );

                _drawCount++;
            };

            _drawCount = 0;
            _window.SendEvent(new Event { type = EventType.Layout });
            _window.SendEvent(new Event { type = EventType.Repaint });
            Assert.IsTrue(_drawCount > 0);
        }

        private static StringEnumPropertyDrawer CreateDrawer(string fieldName)
        {
            const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;
            var drawer = new StringEnumPropertyDrawer();
            var field = typeof(StringEnumTestAsset).GetField(fieldName);
            typeof(PropertyDrawer).GetField("m_FieldInfo", FLAGS).SetValue(drawer, field);
            return drawer;
        }
    }
}

#endif
