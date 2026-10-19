using System;
using EncosyTower.Collections;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using SliceAPI = EncosyTower.Collections.Unsafe.NativeSliceReadOnlyUnsafeUtility;

namespace EncosyTower.Tests.Core.Collections.Extensions.Unsafe
{
    public partial class NativeSliceReadOnlyUnsafeUtilityTests
    {
        [Test]
        public void UnsafePointers_MatchSourceArrayAndAliasStorage()
        {
            using var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var slice = new NativeSliceReadOnly<int>(array);
            var writePointer = IntPtr.Zero;
            var readPointer = IntPtr.Zero;
            var arrayPointer = IntPtr.Zero;

            // SAFETY: The array owns the live allocation for the entire pointer comparison and write.
            unsafe
            {
                writePointer = (IntPtr)SliceAPI.GetUnsafePtr(slice);
                readPointer = (IntPtr)SliceAPI.GetUnsafeReadOnlyPtr(slice);
                arrayPointer = (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(array);
                *(int*)writePointer = 99;
            }

            Assert.AreNotEqual(IntPtr.Zero, writePointer);
            Assert.AreEqual(arrayPointer, writePointer);
            Assert.AreEqual(arrayPointer, readPointer);
            Assert.AreEqual(99, array[0]);
        }

        [Test]
        public void UnsafePointers_SubSlice_PointAtTheStartOfTheSubSlice()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Persistent);

            try
            {
                var tail = new NativeSliceReadOnly<int>(array, 2);
                var middle = new NativeSliceReadOnly<int>(array, 1, 2);

                // SAFETY: The array owns the live four-int allocation; every pointer stays below its end and is only
                // compared with the array address or read once.
                unsafe
                {
                    var origin = (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(array);

                    Assert.AreEqual(origin + (2 * sizeof(int)), (IntPtr)SliceAPI.GetUnsafePtr(tail));
                    Assert.AreEqual(origin + (2 * sizeof(int)), (IntPtr)SliceAPI.GetUnsafeReadOnlyPtr(tail));
                    Assert.AreEqual(origin + sizeof(int), (IntPtr)SliceAPI.GetUnsafePtr(middle));
                    Assert.AreEqual(origin + sizeof(int), (IntPtr)SliceAPI.GetUnsafeReadOnlyPtr(middle));
                    Assert.AreEqual(3, *(int*)SliceAPI.GetUnsafeReadOnlyPtr(tail));
                    Assert.AreEqual(2, *(int*)SliceAPI.GetUnsafeReadOnlyPtr(middle));
                }
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        public void UnsafePointers_EmptySlice_DoNotThrow()
        {
            var array = new NativeArray<int>(0, Allocator.Persistent);

            try
            {
                var slice = new NativeSliceReadOnly<int>(array);

                // SAFETY: The pointers of an empty slice are only produced and never dereferenced.
                unsafe
                {
                    Assert.AreEqual(0, slice.Length);
                    Assert.DoesNotThrow(() => SliceAPI.GetUnsafePtr(slice));
                    Assert.DoesNotThrow(() => SliceAPI.GetUnsafeReadOnlyPtr(slice));
                }
            }
            finally
            {
                array.Dispose();
            }
        }

        [Test]
        [TestRequiresCollectionChecks]
        public void UnsafePointers_AfterSourceDispose_Throw()
        {
            var array = new NativeArray<int>(new[] { 1, 2 }, Allocator.Persistent);
            var slice = new NativeSliceReadOnly<int>(array);

            array.Dispose();

            // SAFETY: Both calls reject the disposed source before a pointer is returned, so no pointer is used.
            unsafe
            {
                Assert.Catch(() => SliceAPI.GetUnsafePtr(slice));
                Assert.Catch(() => SliceAPI.GetUnsafeReadOnlyPtr(slice));
            }
        }

        [Test]
        public void UnsafePointers_WriteThroughTheWritablePointer_IsSeenByTheSliceAndTheSourceArray()
        {
            var array = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Persistent);

            try
            {
                var slice = new NativeSliceReadOnly<int>(array, 1);

                // SAFETY: The array owns the live three-int allocation and the write stays inside the sub-slice.
                unsafe
                {
                    ((int*)SliceAPI.GetUnsafePtr(slice))[1] = 30;
                }

                Assert.AreEqual(30, slice[1]);
                Assert.AreEqual(30, array[2]);
            }
            finally
            {
                array.Dispose();
            }
        }
    }
}
