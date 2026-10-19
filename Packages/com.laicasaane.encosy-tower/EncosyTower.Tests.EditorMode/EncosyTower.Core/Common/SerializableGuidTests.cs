using System;
using EncosyTower.Common;
using EncosyTower.SystemExtensions;
using EncosyTower.Tests.Core.Testing;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Common
{
    public class SerializableGuidTests
    {
        [Test]
        public void SerializableGuid_Test_Implicit_Operator()
        {
            var guid = new Guid("b23b48fcb15d994c939451ce1a44b3fd");
            SerializableGuid serializedGuid = guid;
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void GuidAsReadOnlySpan_EqualsToByteArray()
        {
            var guid = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            var view = guid.AsReadOnlySpan();

            StackOverwriter.Fill(value: 0xCD);

            Assert.AreEqual(16, view.Length);
            CollectionAssert.AreEqual(guid.ToByteArray(), view.ToArray());
        }

        [Test]
        public void GuidAsReadOnlySpan_ViewsTheVariable_SeesLaterWrites()
        {
            var guid = new Guid("00112233-4455-6677-8899-aabbccddeeff");
            var view = guid.AsReadOnlySpan();

            guid = Guid.Empty;

            CollectionAssert.AreEqual(new byte[16], view.ToArray());
        }

        [Test]
        public void AsReadOnlySpan_Default_IsSixteenZeroBytes()
        {
            var value = default(SerializableGuid);
            var view = value.AsReadOnlySpan();

            Assert.AreEqual(SerializableGuid.SIZE, view.Length);
            CollectionAssert.AreEqual(new byte[SerializableGuid.SIZE], view.ToArray());
        }

        [Test]
        public void AsReadOnlySpan_FromBytes_ReturnsTheSameBytes()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 255 };
            var value = new SerializableGuid(bytes);

            var view = value.AsReadOnlySpan();

            CollectionAssert.AreEqual(bytes, view.ToArray());
        }

        [Test]
        public void AsReadOnlySpan_ViewsTheValue_SeesLaterWrites()
        {
            var value = new SerializableGuid(new Guid("00112233-4455-6677-8899-aabbccddeeff"));
            var view = value.AsReadOnlySpan();

            value = default;
            StackOverwriter.Fill(value: 0xCD);

            Assert.AreEqual(SerializableGuid.SIZE, view.Length);
            CollectionAssert.AreEqual(new byte[SerializableGuid.SIZE], view.ToArray());
        }

#if UNITY_COLLECTIONS
        [Test]
        public void SerializableGuid_Test_Conversion_D()
        {
            const string STR = "b23b48fc-b15d-994c-9394-51ce1a44b3fd";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void SerializableGuid_Test_Conversion_N()
        {
            const string STR = "b23b48fcb15d994c939451ce1a44b3fd";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void SerializableGuid_Test_Conversion_B()
        {
            const string STR = "{b23b48fc-b15d-994c-9394-51ce1a44b3fd}";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }

        [Test]
        public void SerializableGuid_Test_Conversion_X()
        {
            const string STR = "{0xb23b48fc,0xb15d,0x994c,{0x93,0x94,0x51,0xce,0x1a,0x44,0xb3,0xfd}}";

            var guid = new Guid(STR);
            var serializedGuid = new SerializableGuid(STR);
            Assert.IsTrue(serializedGuid == guid);
        }
#endif
    }
}
