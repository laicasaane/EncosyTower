using System.Runtime.InteropServices;
using EncosyTower.Variants;
using EncosyTower.Variants.Converters;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Variants
{
    public class VariantConverterTests
    {
        [Test]
        public void CanStore_SmallPrimitiveTypes_ReturnsTrue()
        {
            Assert.IsTrue(VariantConverter.CanStore<byte>());
            Assert.IsTrue(VariantConverter.CanStore<short>());
            Assert.IsTrue(VariantConverter.CanStore<int>());
            Assert.IsTrue(VariantConverter.CanStore<long>());
            Assert.IsTrue(VariantConverter.CanStore<double>());
        }

        [Test]
        public void CanStore_StructSmallerThanTheDataSize_ReturnsTrue()
        {
            Assert.IsTrue(VariantConverter.CanStore<OneByte>());
            Assert.IsTrue(VariantConverter.CanStore<Empty>());
        }

        [Test]
        public void CanStore_StructExactlyTheDataSize_ReturnsTrue()
        {
            Assert.AreEqual(VariantData.BYTE_COUNT, Marshal.SizeOf<DataSized>());
            Assert.IsTrue(VariantConverter.CanStore<DataSized>());
        }

        [Test]
        public void CanStore_StructOneByteLargerThanTheDataSize_ReturnsFalse()
        {
            Assert.AreEqual(VariantData.BYTE_COUNT + 1, Marshal.SizeOf<OneBeyondDataSized>());
            Assert.IsFalse(VariantConverter.CanStore<OneBeyondDataSized>());
        }

        [Test]
        public void CanStore_StructMuchLargerThanTheDataSize_ReturnsFalse()
        {
            Assert.IsFalse(VariantConverter.CanStore<TwiceDataSized>());
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct OneByte
        {
        }

        private struct Empty
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = VariantData.BYTE_COUNT)]
        private struct DataSized
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = VariantData.BYTE_COUNT + 1)]
        private struct OneBeyondDataSized
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = VariantData.BYTE_COUNT * 2)]
        private struct TwiceDataSized
        {
        }
    }
}
