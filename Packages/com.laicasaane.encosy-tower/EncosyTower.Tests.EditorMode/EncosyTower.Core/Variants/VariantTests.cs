using System;
using EncosyTower.Types;
using EncosyTower.Variants;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;

namespace EncosyTower.Tests.Core.Variants
{
    public class VariantTests
    {
        [Test]
        public void Default_IsUndefinedWithoutTypeIdAndObject()
        {
            var variant = default(Variant);

            Assert.AreEqual(VariantTypeKind.Undefined, variant.TypeKind);
            Assert.AreEqual(TypeId.Undefined, variant.TypeId);
            Assert.IsNull(variant.Object);
            Assert.AreEqual(0UL, variant.Base.Meta);
            Assert.AreEqual(0L, variant.Long);
            Assert.AreEqual(string.Empty, variant.ToString());
        }

        [Test]
        public void Undefined_EqualsTheDefaultVariantLayout()
        {
            Assert.AreEqual(VariantTypeKind.Undefined, Variant.Undefined.TypeKind);
            Assert.AreEqual(0UL, Variant.Undefined.Base.Meta);
            Assert.IsTrue(Variant.Undefined.TypeEquals(default(Variant)));
        }

        [Test]
        public void TryGetValue_DefaultVariant_ReturnsFalseForEveryTarget()
        {
            var variant = default(Variant);

            Assert.IsFalse(variant.TryGetValue(out bool _));
            Assert.IsFalse(variant.TryGetValue(out byte _));
            Assert.IsFalse(variant.TryGetValue(out sbyte _));
            Assert.IsFalse(variant.TryGetValue(out char _));
            Assert.IsFalse(variant.TryGetValue(out double _));
            Assert.IsFalse(variant.TryGetValue(out float _));
            Assert.IsFalse(variant.TryGetValue(out int _));
            Assert.IsFalse(variant.TryGetValue(out uint _));
            Assert.IsFalse(variant.TryGetValue(out long _));
            Assert.IsFalse(variant.TryGetValue(out ulong _));
            Assert.IsFalse(variant.TryGetValue(out short _));
            Assert.IsFalse(variant.TryGetValue(out ushort _));
            Assert.IsFalse(variant.TryGetValue(out string _));
            Assert.IsFalse(variant.TryGetValue(out object _));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Constructor_Bool_StoresKindTypeIdAndValue(bool value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Bool, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<bool>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.Bool);
            Assert.IsTrue(variant.TryGetValue(out bool result));
            Assert.AreEqual(value, result);
        }

        [TestCase((byte)0)]
        [TestCase((byte)1)]
        [TestCase((byte)255)]
        public void Constructor_Byte_StoresKindTypeIdAndValue(byte value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Byte, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<byte>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.Byte);
            Assert.IsTrue(variant.TryGetValue(out byte result));
            Assert.AreEqual(value, result);
        }

        [TestCase((sbyte)-128)]
        [TestCase((sbyte)-1)]
        [TestCase((sbyte)127)]
        public void Constructor_SByte_StoresKindTypeIdAndValue(sbyte value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.SByte, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<sbyte>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.SByte);
            Assert.IsTrue(variant.TryGetValue(out sbyte result));
            Assert.AreEqual(value, result);
        }

        [TestCase('\0')]
        [TestCase('a')]
        [TestCase('\u00FC')]
        [TestCase('\u2713')]
        public void Constructor_Char_StoresKindTypeIdAndValue(char value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Char, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<char>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.Char);
            Assert.IsTrue(variant.TryGetValue(out char result));
            Assert.AreEqual(value, result);
        }

        [TestCase((short)-32768)]
        [TestCase((short)-1)]
        [TestCase((short)32767)]
        public void Constructor_Short_StoresKindTypeIdAndValue(short value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Short, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<short>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.Short);
            Assert.IsTrue(variant.TryGetValue(out short result));
            Assert.AreEqual(value, result);
        }

        [TestCase((ushort)0)]
        [TestCase((ushort)1)]
        [TestCase((ushort)65535)]
        public void Constructor_UShort_StoresKindTypeIdAndValue(ushort value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.UShort, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<ushort>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.UShort);
            Assert.IsTrue(variant.TryGetValue(out ushort result));
            Assert.AreEqual(value, result);
        }

        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(int.MaxValue)]
        public void Constructor_Int_StoresKindTypeIdAndValue(int value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Int, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<int>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.Int);
            Assert.IsTrue(variant.TryGetValue(out int result));
            Assert.AreEqual(value, result);
        }

        [TestCase(0u)]
        [TestCase(1u)]
        [TestCase(uint.MaxValue)]
        public void Constructor_UInt_StoresKindTypeIdAndValue(uint value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.UInt, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<uint>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.UInt);
            Assert.IsTrue(variant.TryGetValue(out uint result));
            Assert.AreEqual(value, result);
        }

        [TestCase(long.MinValue)]
        [TestCase(-1L)]
        [TestCase(0L)]
        [TestCase(long.MaxValue)]
        public void Constructor_Long_StoresKindTypeIdAndValue(long value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Long, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<long>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.Long);
            Assert.IsTrue(variant.TryGetValue(out long result));
            Assert.AreEqual(value, result);
        }

        [TestCase(0UL)]
        [TestCase(1UL)]
        [TestCase(ulong.MaxValue)]
        public void Constructor_ULong_StoresKindTypeIdAndValue(ulong value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.ULong, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<ulong>.Id, variant.TypeId);
            Assert.AreEqual(value, variant.ULong);
            Assert.IsTrue(variant.TryGetValue(out ulong result));
            Assert.AreEqual(value, result);
        }

        [TestCase(float.MinValue)]
        [TestCase(-0f)]
        [TestCase(float.Epsilon)]
        [TestCase(float.MaxValue)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_Float_StoresTheExactBits(float value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Float, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<float>.Id, variant.TypeId);
            Assert.AreEqual(BitConverter.SingleToInt32Bits(value), BitConverter.SingleToInt32Bits(variant.Float));
            Assert.IsTrue(variant.TryGetValue(out float result));
            Assert.AreEqual(BitConverter.SingleToInt32Bits(value), BitConverter.SingleToInt32Bits(result));
        }

        [TestCase(double.MinValue)]
        [TestCase(-0d)]
        [TestCase(double.Epsilon)]
        [TestCase(double.MaxValue)]
        [TestCase(double.NaN)]
        [TestCase(double.NegativeInfinity)]
        public void Constructor_Double_StoresTheExactBits(double value)
        {
            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Double, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<double>.Id, variant.TypeId);
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(value), BitConverter.DoubleToInt64Bits(variant.Double));
            Assert.IsTrue(variant.TryGetValue(out double result));
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(value), BitConverter.DoubleToInt64Bits(result));
        }

        [Test]
        public void Constructor_CharMaxValue_StoresTheExactValue()
        {
            var variant = new Variant(char.MaxValue);

            Assert.AreEqual(char.MaxValue, variant.Char);
            Assert.AreEqual(ushort.MaxValue, variant.UShort);
            Assert.IsTrue(variant.TryGetValue(out char result));
            Assert.AreEqual(char.MaxValue, result);
        }

        [Test]
        public void Constructor_String_StoresTheReferenceAndLeavesTheDataZero()
        {
            const string TEXT = "variant text";

            var variant = new Variant(TEXT);

            Assert.AreEqual(VariantTypeKind.String, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<string>.Id, variant.TypeId);
            Assert.AreSame(TEXT, variant.Object);
            Assert.AreEqual(0L, variant.Long);
            Assert.IsTrue(variant.TryGetValue(out string result));
            Assert.AreSame(TEXT, result);
        }

        [Test]
        public void Constructor_NullString_StoresNoObject()
        {
            var variant = new Variant((string)null);

            Assert.AreEqual(VariantTypeKind.String, variant.TypeKind);
            Assert.IsNull(variant.Object);
            Assert.AreEqual(string.Empty, variant.ToString());
        }

        [Test]
        public void Constructor_Object_StoresTheReferenceAndLeavesTheDataZero()
        {
            var value = new object();

            var variant = new Variant(value);

            Assert.AreEqual(VariantTypeKind.Object, variant.TypeKind);
            Assert.AreEqual((TypeId)Type<object>.Id, variant.TypeId);
            Assert.AreSame(value, variant.Object);
            Assert.AreEqual(0L, variant.Long);
            Assert.IsTrue(variant.TryGetValue(out object result));
            Assert.AreSame(value, result);
        }

        [Test]
        public void Constructor_TypeIdAndObject_StoresTheObjectKindWithThatTypeId()
        {
            var typeId = (TypeId)Type<Uri>.Id;
            var value = new Uri("https://example.invalid/");

            var variant = new Variant(typeId, value);

            Assert.AreEqual(VariantTypeKind.Object, variant.TypeKind);
            Assert.AreEqual(typeId, variant.TypeId);
            Assert.AreSame(value, variant.Object);
        }

        [Test]
        public void Constructor_Long_OverlaysTheNarrowerFieldsOnTheLowBytes()
        {
            const long VALUE = 0x1122334455667788L;

            var variant = new Variant(VALUE);

            Assert.AreEqual(unchecked((int)0x55667788), variant.Int);
            Assert.AreEqual(0x55667788u, variant.UInt);
            Assert.AreEqual(unchecked((short)0x7788), variant.Short);
            Assert.AreEqual((ushort)0x7788, variant.UShort);
            Assert.AreEqual((byte)0x88, variant.Byte);
            Assert.AreEqual(unchecked((sbyte)0x88), variant.SByte);
            Assert.AreEqual(unchecked((ulong)VALUE), variant.ULong);
            Assert.AreEqual(VALUE, BitConverter.DoubleToInt64Bits(variant.Double));
        }

        [Test]
        public void Constructor_Int_LeavesTheUpperDataBytesZero()
        {
            var variant = new Variant(int.MinValue);

            Assert.AreEqual((long)unchecked((uint)int.MinValue), variant.Long);
            Assert.AreEqual((ulong)unchecked((uint)int.MinValue), variant.ULong);
        }

        [Test]
        public void Constructor_Byte_LeavesTheOtherDataBytesZero()
        {
            var variant = new Variant((byte)0xFF);

            Assert.AreEqual(0xFFL, variant.Long);
            Assert.AreEqual(0xFF, variant.Int);
            Assert.AreEqual((short)0xFF, variant.Short);
        }

        [Test]
        public void Constructor_KindAndTypeId_OverlayTheMetaBytes()
        {
            var typeId = new TypeId(0xA1B2C3D4u);

            var variant = new Variant(VariantTypeKind.Long, typeId);

            Assert.AreEqual(VariantTypeKind.Long, variant.TypeKind);
            Assert.AreEqual(typeId, variant.TypeId);
            Assert.AreEqual((ulong)VariantTypeKind.Long | (0xA1B2C3D4UL << 8), variant.Base.Meta);
        }

        [Test]
        public void Constructor_Base_ReadsKindAndTypeIdFromTheMeta()
        {
            var meta = (ulong)VariantTypeKind.Short | (0x01020304UL << 8);
            var @base = new VariantBase(meta: meta, data: default);

            var variant = new Variant(@base);

            Assert.AreEqual(VariantTypeKind.Short, variant.TypeKind);
            Assert.AreEqual(new TypeId(0x01020304u), variant.TypeId);
            Assert.AreEqual(meta, variant.Base.Meta);
        }

        [Test]
        public void Constructor_BaseKindAndTypeId_ReplacesOnlyTheKindAndTypeIdBytesOfTheMeta()
        {
            var @base = new VariantBase(meta: ulong.MaxValue, data: default);
            var typeId = new TypeId(0x01020304u);

            var variant = new Variant(@base, VariantTypeKind.UInt, typeId);

            var expected = 0xFFFFFF0000000000UL | (ulong)VariantTypeKind.UInt | (0x01020304UL << 8);

            Assert.AreEqual(VariantTypeKind.UInt, variant.TypeKind);
            Assert.AreEqual(typeId, variant.TypeId);
            Assert.AreEqual(expected, variant.Base.Meta);
        }

        [Test]
        public void TypeEquals_SameKindDifferentValue_ReturnsTrue()
        {
            var first = new Variant(1);
            var second = new Variant(2);
            var other = new Variant(1L);

            Assert.IsTrue(first.TypeEquals(second));
            Assert.IsFalse(first.TypeEquals(other));
        }

        [Test]
        public void TryGetValue_WidensSmallerKindsAndRejectsNarrowing()
        {
            var negative = new Variant(int.MinValue);
            var maximum = new Variant(ulong.MaxValue);
            var fractional = new Variant(1.5f);

            Assert.IsTrue(negative.TryGetValue(out double widened));
            Assert.AreEqual((double)int.MinValue, widened);
            Assert.IsTrue(maximum.TryGetValue(out double widenedMaximum));
            Assert.AreEqual((double)ulong.MaxValue, widenedMaximum);
            Assert.IsFalse(fractional.TryGetValue(out int _));
            Assert.IsFalse(negative.TryGetValue(out byte _));
        }

        [Test]
        public void Layout_DataFields_StartAtTheBaseDataOffset()
        {
            Assert.AreEqual(0, VariantBase.META_OFFSET);
            Assert.AreEqual(8, VariantBase.META_SIZE);
            Assert.AreEqual(8, VariantBase.OBJECT_OFFSET);
            Assert.AreEqual(16, VariantBase.DATA_OFFSET);
            Assert.AreEqual(VariantBase.DATA_OFFSET + VariantData.BYTE_COUNT, UnsafeUtility.SizeOf<VariantBase>());
        }
    }
}
