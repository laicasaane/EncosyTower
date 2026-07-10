using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using NUnit.Framework;

namespace EncosyTower.Tests.EncosyTower.Collections
{
    public class BufferProvider<T> : IBufferProvider<ManagedBuffer<T>, T>
    {
        private ManagedBuffer<T> _buffer = new(4);
        private int _count;
        private int _version;

        public ref ManagedBuffer<T> Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer;
        }

        public ref int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _count;
        }

        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Capacity;
        }

        public ref int Version
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _version;
        }
    }

    public class ListProxyTests
    {
        [Test]
        public void StatelessList_Tests()
        {
            var buffer = new BufferProvider<int>();
            var list = new ListProxy<BufferProvider<int>, ManagedBuffer<int>, int>(buffer);

            Assert.AreEqual(true, list.IsCreated);
            Assert.AreEqual(4, list.Capacity);
            Assert.AreEqual(0, list.Count);

            list.Add(5);
            list.Add(8);
            list.Add(2);
            list.Add(0);

            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(5, list[0]);
            Assert.AreEqual(8, list[1]);
            Assert.AreEqual(2, list[2]);
            Assert.AreEqual(0, list[3]);

            list.Remove(8);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2, list[1]);
        }
    }
}
