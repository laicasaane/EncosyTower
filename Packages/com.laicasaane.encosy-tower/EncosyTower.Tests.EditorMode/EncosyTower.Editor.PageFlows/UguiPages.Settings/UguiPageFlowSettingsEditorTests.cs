#if UNITY_EDITOR && UNITY_UGUI

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.Editor.PageFlows.UguiPages.Settings
{
    public sealed class UguiPageFlowSettingsEditorTests
    {
        private const string TYPE_NAME =
            "EncosyTower.Editor.PageFlows.UguiPages.Settings.Views.UguiPageFlowSettingsEditor";

        [Test]
        public void EnumValueChange_NullEventLeavesStateUnchanged()
        {
            var fixture = new EditorFixture();

            fixture.Invoke(evt: null);

            Assert.IsFalse(fixture.ValueUpdated);
        }

        [Test]
        public void EnumValueChange_EqualValuesLeaveStateUnchanged()
        {
            var fixture = new EditorFixture();
            using var evt = ChangeEvent<Enum>.GetPooled(TestEnum.First, TestEnum.First);

            fixture.Invoke(evt);

            Assert.IsFalse(fixture.ValueUpdated);
        }

        [TestCase(null, TestEnum.First, true)]
        [TestCase(TestEnum.First, null, true)]
        [TestCase(TestEnum.First, TestEnum.Second, true)]
        public void EnumValueChange_DifferentValuesMarkStateUpdated(Enum previous, Enum current, bool expected)
        {
            var fixture = new EditorFixture();
            using var evt = ChangeEvent<Enum>.GetPooled(previous, current);

            fixture.Invoke(evt);

            Assert.AreEqual(expected, fixture.ValueUpdated);
        }

        private enum TestEnum
        {
            First,
            Second,
        }

        private sealed class EditorFixture
        {
            private readonly object _editor;
            private readonly FieldInfo _valueUpdated;

            public EditorFixture()
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .Single(static candidate => candidate.GetName().Name == "EncosyTower.Editor.PageFlows");
                var type = assembly.GetType(TYPE_NAME, throwOnError: true);
                _editor = FormatterServices.GetUninitializedObject(type);
                _valueUpdated = type.GetField("_valueUpdated", BindingFlags.Instance | BindingFlags.NonPublic);
                Method = type.GetMethod(
                      "OnValueChanged"
                    , BindingFlags.Instance | BindingFlags.NonPublic
                    , binder: null
                    , new[] { typeof(ChangeEvent<Enum>) }
                    , modifiers: null
                );

                Assert.That(_valueUpdated, Is.Not.Null);
                Assert.That(Method, Is.Not.Null);
            }

            public MethodInfo Method { get; }
            public bool ValueUpdated => (bool)_valueUpdated.GetValue(_editor);

            public void Invoke(ChangeEvent<Enum> evt)
            {
                Method.Invoke(_editor, new object[] { evt });
            }
        }
    }
}

#endif
