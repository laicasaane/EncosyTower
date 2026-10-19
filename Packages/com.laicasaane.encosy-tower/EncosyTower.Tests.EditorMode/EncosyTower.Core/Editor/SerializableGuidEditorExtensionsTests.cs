#if UNITY_EDITOR

using System;
using EncosyTower.Common;
using EncosyTower.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Tests.Editor
{
    public class SerializableGuidEditorExtensionsTests
    {
        private SerializableGuidHost _host;

        [SetUp]
        public void SetUp()
        {
            _host = ScriptableObject.CreateInstance<SerializableGuidHost>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_host);
            _host = null;
        }

        [Test]
        public void TryCopyFrom_SerializableGuidProperty_CopiesEverySerializedByte()
        {
            var expected = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            _host._guid = new SerializableGuid(expected);

            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(SerializableGuidHost._guid));
            SerializableGuid actual = default;

            var copied = actual.TryCopyFrom(property);

            Assert.IsTrue(copied);
            Assert.AreEqual(expected, (Guid)actual);
        }

        [Test]
        public void TryCopyFrom_GuidTarget_CopiesEverySerializedByte()
        {
            var expected = new Guid("fedcba98-7654-4321-8fed-cba987654321");
            _host._guid = new SerializableGuid(expected);

            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(SerializableGuidHost._guid));
            var actual = Guid.Empty;

            var copied = actual.TryCopyFrom(property);

            Assert.IsTrue(copied);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TryCopyFrom_ByteValuesAroundSignBoundary_RoundTripBitExact()
        {
            var bytes = new byte[] { 0, 1, 127, 128, 129, 254, 255, 0, 255, 128, 127, 1, 0, 255, 2, 253 };
            _host._guid = new SerializableGuid(bytes);

            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(SerializableGuidHost._guid));
            SerializableGuid actual = default;

            var copied = actual.TryCopyFrom(property);

            Assert.IsTrue(copied);
            CollectionAssert.AreEqual(bytes, actual.AsReadOnlySpan().ToArray());
        }

        [Test]
        public void TryCopyFrom_DefaultGuidProperty_CopiesSixteenZeroBytes()
        {
            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(SerializableGuidHost._guid));
            var actual = new SerializableGuid(new Guid("00112233-4455-6677-8899-aabbccddeeff"));

            var copied = actual.TryCopyFrom(property);

            Assert.IsTrue(copied);
            Assert.AreEqual(Guid.Empty, (Guid)actual);
        }

        [Test]
        public void TryCopyFrom_PropertyOfAnotherType_ReturnsFalseAndResetsSerializableGuid()
        {
            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(SerializableGuidHost._notAGuid));
            var actual = new SerializableGuid(new Guid("00112233-4455-6677-8899-aabbccddeeff"));

            var copied = actual.TryCopyFrom(property);

            Assert.IsFalse(copied);
            Assert.AreEqual(Guid.Empty, (Guid)actual);
        }

        [Test]
        public void TryCopyFrom_PropertyOfAnotherType_ReturnsFalseAndKeepsGuid()
        {
            using var serializedObject = new SerializedObject(_host);
            var property = serializedObject.FindProperty(nameof(SerializableGuidHost._notAGuid));
            var expected = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            var actual = expected;

            var copied = actual.TryCopyFrom(property);

            Assert.IsFalse(copied);
            Assert.AreEqual(expected, actual);
        }
    }
}

#endif
