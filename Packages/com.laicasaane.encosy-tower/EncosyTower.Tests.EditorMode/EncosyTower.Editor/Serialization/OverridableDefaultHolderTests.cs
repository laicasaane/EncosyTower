#if UNITY_EDITOR

using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Editor;
using EncosyTower.Editor.Serialization.Internals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public sealed class OverridableDefaultHolderTests
    {
        private const string VALUE_PATH = "_overridableDefault.value";

        private OverridableDefaultHolder _holder;
        private OverridableTestAsset _asset;
        private SerializedObject _source;
        private PropertyField _field;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<OverridableTestAsset>();
            _asset.intValue = new(value: 9, isOverridden: false);
            _asset.payloadValue = new(value: new() { number = 9, text = "Stored" }, isOverridden: false);
            _asset.listValue = new(value: new() { 9, 8, 7 }, isOverridden: false);
            _asset.arrayValue = new(value: new[] { 9, 8, 7 }, isOverridden: false);
            _source = new SerializedObject(_asset);
        }

        [TearDown]
        public void TearDown()
        {
            _field?.Unbind();
            _source?.Dispose();
            Object.DestroyImmediate(_holder);
            Object.DestroyImmediate(_asset);
        }

        [Test]
        public void Write_IntPersistsDefaultWithoutChangingSource()
        {
            _holder = OverridableDefaultHolder.Create(typeof(int));
            var source = GetSource(nameof(OverridableTestAsset.intValue));
            var property = _holder.Write(value: 3, expandedSource: source);

            Assert.AreEqual(HideFlags.DontSave, _holder.hideFlags);
            Assert.AreEqual(VALUE_PATH, property.propertyPath);
            Assert.AreEqual(expected: 3, actual: property.intValue);

            using var serializedObject = new SerializedObject(_holder);

            Assert.AreEqual(expected: 3, actual: serializedObject.FindProperty(VALUE_PATH).intValue);
            Assert.AreEqual(expected: 9, actual: _asset.intValue.value);
            Assert.IsFalse(_asset.intValue.isOverridden);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Write_ClassCopiesMembersAndExpansion(bool expanded)
        {
            _holder = OverridableDefaultHolder.Create(typeof(OverridableTestPayload));
            var source = GetSource(nameof(OverridableTestAsset.payloadValue));
            source.isExpanded = expanded;
            var value = new OverridableTestPayload { number = 7, text = "Default" };
            var property = _holder.Write(value, source);

            Assert.AreEqual(expanded, property.isExpanded);

            using var serializedObject = new SerializedObject(_holder);
            var stored = serializedObject.FindProperty(VALUE_PATH);

            Assert.AreEqual(
                  expected: 7
                , actual: stored.FindPropertyRelative(nameof(OverridableTestPayload.number)).intValue
            );

            Assert.AreEqual("Default", stored.FindPropertyRelative(nameof(OverridableTestPayload.text)).stringValue);

            property.FindPropertyRelative(nameof(OverridableTestPayload.number)).intValue = 11;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(expected: 7, actual: value.number);
            Assert.AreEqual(expected: 9, actual: _asset.payloadValue.value.number);
            Assert.AreEqual("Stored", _asset.payloadValue.value.text);
        }

        [Test]
        public void Write_ListCopiesElementsAndRefreshesExistingHolder()
        {
            _holder = OverridableDefaultHolder.Create(typeof(List<int>));
            var source = GetSource(nameof(OverridableTestAsset.listValue));
            source.isExpanded = true;
            var value = new List<int> { 3, 5 };
            var property = _holder.Write(value, source);
            var serializedObject = property.serializedObject;

            using var readback = new SerializedObject(_holder);
            var stored = readback.FindProperty(VALUE_PATH);

            Assert.AreEqual(expected: 2, actual: stored.arraySize);
            Assert.AreEqual(expected: 3, actual: stored.GetArrayElementAtIndex(0).intValue);
            Assert.AreEqual(expected: 5, actual: stored.GetArrayElementAtIndex(1).intValue);
            Assert.IsTrue(property.isExpanded);

            property.GetArrayElementAtIndex(0).intValue = 11;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            CollectionAssert.AreEqual(new[] { 3, 5 }, value);

            source.isExpanded = false;
            property = _holder.Write(new List<int> { 4 }, source);

            Assert.AreSame(serializedObject, property.serializedObject);
            Assert.AreEqual(VALUE_PATH, property.propertyPath);
            Assert.AreEqual(expected: 1, actual: property.arraySize);
            Assert.AreEqual(expected: 4, actual: property.GetArrayElementAtIndex(0).intValue);
            Assert.IsFalse(property.isExpanded);
            CollectionAssert.AreEqual(new[] { 9, 8, 7 }, _asset.listValue.value);
        }

        [Test]
        public void Write_ArrayCopiesElementsAndNullClearsCollection()
        {
            _holder = OverridableDefaultHolder.Create(typeof(int[]));
            var source = GetSource(nameof(OverridableTestAsset.arrayValue));
            var property = _holder.Write(new[] { 3, 5 }, source);

            using var serializedObject = new SerializedObject(_holder);
            var stored = serializedObject.FindProperty(VALUE_PATH);

            Assert.AreEqual(expected: 2, actual: stored.arraySize);
            Assert.AreEqual(expected: 3, actual: stored.GetArrayElementAtIndex(0).intValue);
            Assert.AreEqual(expected: 5, actual: stored.GetArrayElementAtIndex(1).intValue);

            property = _holder.Write(value: null, expandedSource: source);

            Assert.AreEqual(expected: 0, actual: property.arraySize);
            CollectionAssert.AreEqual(new[] { 9, 8, 7 }, _asset.arrayValue.value);
        }

        [Test]
        public void Write_NullObjectDefaultKeepsDeclaredType()
        {
            _holder = OverridableDefaultHolder.Create(typeof(GameObject));
            var source = GetSource(nameof(OverridableTestAsset.objectValue));
            var property = _holder.Write(value: null, expandedSource: source);

            Assert.AreEqual(SerializedPropertyType.ObjectReference, property.propertyType);
            Assert.IsNull(property.objectReferenceValue);
            Assert.AreEqual(VALUE_PATH, property.propertyPath);
        }

        [Test]
        public void Write_ReturnedPropertySurvivesBindPass()
        {
            _holder = OverridableDefaultHolder.Create(typeof(int));
            var source = GetSource(nameof(OverridableTestAsset.intValue));
            var property = _holder.Write(value: 3, expandedSource: source);
            _field = new PropertyField(property);

            Assert.DoesNotThrow(BindField);
            Assert.AreEqual(VALUE_PATH, property.propertyPath);
            Assert.AreEqual(VALUE_PATH, _field.bindingPath);
            Assert.AreSame(_holder, property.serializedObject.targetObject);
            Assert.AreEqual(expected: 3, actual: property.intValue);
            Assert.AreEqual(expected: 9, actual: _asset.intValue.value);

            void BindField()
            {
                _field.Bind(property.serializedObject);
                _field.Bind(_source);
            }
        }

        [Test]
        public void Write_GuidCopiesFixedBufferAndKeepsTheSourceValue()
        {
            var original = new SerializableGuid(new System.Guid("fedcba98-7654-4321-8fed-cba987654321"));
            _asset.guidValue = new(value: original, isOverridden: false);
            _source.Update();
            _holder = OverridableDefaultHolder.Create(typeof(SerializableGuid));
            var source = GetSource(nameof(OverridableTestAsset.guidValue));
            var expected = OverridableTestDefaults.GuidValue;
            var property = _holder.Write(expected, source);
            SerializableGuid actual = default;

            Assert.IsTrue(actual.TryCopyFrom(property));
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, ValueChoicesPropertyDrawer.ReadValue(property, typeof(SerializableGuid)));
            Assert.AreEqual(original, _asset.guidValue.value);
            Assert.IsFalse(_asset.guidValue.isOverridden);

            ValueChoicesPropertyDrawer.WriteValue(property, original);
            Assert.IsTrue(property.serializedObject.hasModifiedProperties);
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            Assert.AreEqual(original, ValueChoicesPropertyDrawer.ReadValue(property, typeof(SerializableGuid)));
            Assert.AreEqual(original, _asset.guidValue.value);
        }

        [Test]
        public void Write_GuidListCopiesElementsAndReadBackIsIndependent()
        {
            var original = new List<SerializableGuid> { SerializableGuid.Empty };
            _asset.guidList = new(value: original, isOverridden: false);
            _source.Update();
            _holder = OverridableDefaultHolder.Create(typeof(List<SerializableGuid>));
            var source = GetSource(nameof(OverridableTestAsset.guidList));
            var expected = OverridableTestDefaults.GuidListValue;
            var property = _holder.Write(expected, source);
            var readBack = (List<SerializableGuid>)ValueChoicesPropertyDrawer.ReadValue(
                  property
                , typeof(List<SerializableGuid>)
            );

            CollectionAssert.AreEqual(expected, readBack);
            Assert.AreNotSame(expected, readBack);
            readBack[0] = SerializableGuid.Empty;
            CollectionAssert.AreEqual(
                  expected
                , (List<SerializableGuid>)ValueChoicesPropertyDrawer.ReadValue(property, typeof(List<SerializableGuid>))
            );

            using var element = property.GetArrayElementAtIndex(0);
            ValueChoicesPropertyDrawer.WriteValue(element, SerializableGuid.Empty);
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            SerializableGuid actual = default;
            Assert.IsTrue(actual.TryCopyFrom(element));
            Assert.AreEqual(SerializableGuid.Empty, actual);
            Assert.AreEqual(OverridableTestDefaults.GuidValue, expected[0]);
            CollectionAssert.AreEqual(new[] { SerializableGuid.Empty }, _asset.guidList.value);
            Assert.IsFalse(_asset.guidList.isOverridden);
        }

        [Test]
        public void Write_NestedBuffersDeepCopyManagedReferencesAndPreserveObjectReferences()
        {
            var target = new GameObject("Fixed buffer reference");

            try
            {
                var guid = OverridableTestDefaults.GuidValue;
                var node = new OverridableFixedBufferNode(number: 7) { id = guid, target = target };
                node.next = node;

                var value = new OverridableFixedBufferPayload {
                    envelope = new() { Id = guid, Count = 11 },
                    array = new[] { guid, SerializableGuid.Empty },
                    list = new() { SerializableGuid.Empty, guid },
                    target = target,
                    node = node,
                    alias = node,
                };

                _holder = OverridableDefaultHolder.Create(typeof(OverridableFixedBufferPayload));
                var property = _holder.Write(value: value, expandedSource: null);
                using var storedTarget = property.FindPropertyRelative(nameof(OverridableFixedBufferPayload.target));
                using var writtenNode = property.FindPropertyRelative(nameof(OverridableFixedBufferPayload.node));
                using var nodeTarget = writtenNode.FindPropertyRelative(nameof(OverridableFixedBufferNode.target));
                Assert.AreSame(target, storedTarget.objectReferenceValue, "Holder write lost the root reference.");
                Assert.AreSame(target, nodeTarget.objectReferenceValue, "Holder write lost the node reference.");

                var copy = (OverridableFixedBufferPayload)ValueChoicesPropertyDrawer.ReadValue(
                      property
                    , typeof(OverridableFixedBufferPayload)
                );

                Assert.AreNotSame(value, copy);
                Assert.AreEqual(guid, copy.envelope.Id);
                Assert.AreEqual(expected: 11, actual: copy.envelope.Count);
                CollectionAssert.AreEqual(value.array, copy.array);
                CollectionAssert.AreEqual(value.list, copy.list);
                Assert.AreNotSame(value.array, copy.array);
                Assert.AreNotSame(value.list, copy.list);
                Assert.AreSame(target, copy.target, "Read-back lost the root reference.");
                Assert.AreNotSame(node, copy.node);
                Assert.AreSame(copy.node, copy.alias);
                Assert.AreSame(copy.node, copy.node.next);
                Assert.AreSame(target, copy.node.target, "Read-back lost the node reference.");
                Assert.IsNull(copy.absent);
                Assert.AreEqual(expected: 7, actual: copy.node.number);

                using var storedNode = property.FindPropertyRelative(nameof(OverridableFixedBufferPayload.node));
                using var storedGuid = storedNode.FindPropertyRelative(nameof(OverridableFixedBufferNode.id));
                SerializableGuid.Empty.TryCopyTo(storedGuid);
                Assert.IsTrue(property.serializedObject.hasModifiedProperties);

                var pending = (OverridableFixedBufferPayload)ValueChoicesPropertyDrawer.ReadValue(
                      property
                    , typeof(OverridableFixedBufferPayload)
                );

                Assert.AreEqual(SerializableGuid.Empty, pending.node.id);
                Assert.AreSame(pending.node, pending.alias);
                Assert.AreSame(pending.node, pending.node.next);
                Assert.IsTrue(property.serializedObject.hasModifiedProperties);
                Assert.AreEqual(guid, node.id);
                property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                Assert.AreEqual(guid, node.id);
                pending.array[0] = SerializableGuid.Empty;
                pending.list[1] = SerializableGuid.Empty;
                Assert.AreEqual(guid, value.array[0]);
                Assert.AreEqual(guid, value.list[1]);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        private SerializedProperty GetSource(string fieldName)
            => _source.FindProperty(fieldName).FindPropertyRelative(OverridableEditorAPI.VALUE);
    }
}

#endif
