using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Variants;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace EncosyTower.Tests.Variants
{
    public class VariantTests
    {
        [Test]
        public void StringValues_PreserveNullEmptyNonemptyAndRejectWrongType()
        {
            var nullValue = new Variant((string)null);
            Assert.IsTrue(nullValue.TryGetValue(out string nullResult));
            Assert.IsNull(nullResult);
            var destination = "preserved";
            Assert.IsTrue(nullValue.TrySetValueTo(ref destination));
            Assert.IsNull(destination);

            var emptyValue = new Variant(string.Empty);
            Assert.IsTrue(emptyValue.TryGetValue(out string emptyResult));
            Assert.AreEqual(string.Empty, emptyResult);

            var textValue = new Variant("message");
            Assert.IsTrue(textValue.TryGetValue(out string textResult));
            Assert.AreEqual("message", textResult);

            var wrongValue = new Variant(1);
            destination = "preserved";
            Assert.IsFalse(wrongValue.TryGetValue(out string _));
            Assert.IsFalse(wrongValue.TrySetValueTo(ref destination));
            Assert.AreEqual("preserved", destination);
        }

        [Test, Performance]
        public void Variant_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("Variant");
                {
                    var u = new Variant(i);
                    Process(u);
                    i++;
                }
                Profiler.EndSample();
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void VariantBig_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("VariantBig");
                {
                    var u = new VariantBig(i);
                    Process(u);
                    i++;
                }
                Profiler.EndSample();
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in Variant v)
        {
            _ = v.Int;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in VariantBig v)
        {
            _ = v.Int;
        }

        [StructLayout(LayoutKind.Explicit, Size = 9996)]
        private struct Big { }

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct VariantBig
        {
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly VariantBase Base;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly Big Big;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly bool Bool;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly byte Byte;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly sbyte SByte;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly char Char;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly double Double;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly float Float;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly int Int;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly uint UInt;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly long Long;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly ulong ULong;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly short Short;
            // TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.
            [FieldOffset(0)] public readonly ushort UShort;

            public VariantBig(int intValue) : this()
            {
                Int = intValue;
            }
        }
    }
}
