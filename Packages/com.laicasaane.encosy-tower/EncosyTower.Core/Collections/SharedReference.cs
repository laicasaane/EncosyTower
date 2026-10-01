// https://github.com/stella3d/SharedArray

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Buffers;
using EncosyTower.Collections.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections
{
    [Serializable]
    public sealed partial class SharedReference<T> : SharedReference<T, T>
        where T : unmanaged
    {
        public SharedReference() : base()
        {
        }

        public SharedReference(T value) : base(value)
        {
        }
    }

    [Serializable]
    public partial class SharedReference<T, TNative> : IDisposable
        , IAsSpan<T>, IAsReadOnlySpan<T>, IAsMemory<T>, IAsReadOnlyMemory<T>
        , IAsNativeArray<TNative>, IAsNativeSlice<TNative>
        where T : unmanaged
        where TNative : unmanaged
    {
        [NonSerialized] internal BufferShared<T, TNative> _buffer;
        [NonSerialized] internal int _version;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [NonSerialized] private AtomicSafetyHandle _safety;
#endif

        protected SharedReference()
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());
            Initialize(default);
        }

        public SharedReference(T value)
        {
            ThrowHelper.ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(UnsafeAPI.AreTypesEqualSize<T, TNative>());
            Initialize(value);
        }

        ~SharedReference()
        {
            // SAFETY: Finalization owns the remaining buffer and no managed caller can use this instance afterward.
            unsafe
            {
                Dispose();
            }
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                return _buffer.Capacity;
            }
        }

        /// <safety>The returned reference must not outlive this owner.</safety>
        public unsafe ref T ValueRW
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckWrite();
                _version++;
                // SAFETY: This owner keeps its one-element pin live while the reference is used.
                unsafe
                {
                    return ref _buffer[0];
                }
            }
        }

        /// <safety>The returned reference must not outlive this owner.</safety>
        public unsafe ref readonly T ValueRO
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckRead();
                // SAFETY: This owner keeps its one-element pin live while the reference is used.
                unsafe
                {
                    return ref _buffer[0];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T[]([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsManagedArray();
        }

        /// <safety>The returned span must not outlive the source owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator Span<T>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: The caller accepts the returned span's borrowed owner lifetime.
            unsafe
            {
                return self.AsSpan();
            }
        }

        /// <safety>The returned span must not outlive the source owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ReadOnlySpan<T>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: The caller accepts the returned span's borrowed owner lifetime.
            unsafe
            {
                return self.AsReadOnlySpan();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return self.AsReadOnlyMemory();
        }

        /// <safety>The returned span must not outlive the source owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator Span<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: The caller accepts the returned span's borrowed owner lifetime.
            unsafe
            {
                return self.AsSpanNative();
            }
        }

        /// <safety>The returned span must not outlive the source owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ReadOnlySpan<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: The caller accepts the returned span's borrowed owner lifetime.
            unsafe
            {
                return self.AsReadOnlySpanNative();
            }
        }

        /// <safety>The returned native array must not outlive the source owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator NativeArray<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: The caller accepts the returned alias's borrowed owner lifetime.
            unsafe
            {
                return self.AsNativeArray();
            }
        }

        /// <safety>The returned native slice must not outlive the source owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator NativeSlice<TNative>([NotNull] SharedReference<T, TNative> self)
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            // SAFETY: The caller accepts the returned alias's borrowed owner lifetime.
            unsafe
            {
                return self.AsNativeSlice();
            }
        }

        public void Dispose()
        {
            if (_buffer.IsCreated == false)
            {
                return;
            }

            _version++;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckDeallocateAndThrow(_safety);
#endif

            // SAFETY: This class owns the live pin and has passed its deallocation checks.
            unsafe
            {
                _buffer.Dispose();
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.Release(_safety);
            _safety = default;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] AsManagedArray()
        {
            CheckWrite();
            return _buffer.AsManagedArray();
        }

        /// <safety>The returned span must not outlive this owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span<T> AsSpan()
        {
            CheckWrite();
            // SAFETY: This owner keeps the one-element buffer pinned for the returned span.
            unsafe
            {
                return _buffer.AsSpan();
            }
        }

        /// <safety>The returned span must not outlive this owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlySpan<T> AsReadOnlySpan()
        {
            CheckRead();
            // SAFETY: This owner keeps the one-element buffer pinned for the returned span.
            unsafe
            {
                return _buffer.AsReadOnlySpan();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsMemory()
        {
            CheckWrite();
            return _buffer.AsMemory();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
            CheckRead();
            return _buffer.AsReadOnlyMemory();
        }

        /// <safety>The returned span must not outlive this owner.</safety>
        public unsafe Span<TNative> AsSpanNative()
        {
            CheckWrite();

            // SAFETY: Owner keeps the one-element buffer pinned for the returned mutable span.
            unsafe
            {
                return new Span<TNative>(_buffer.GetUnsafeBufferPointer(), 1);
            }
        }

        /// <safety>The returned span must not outlive this owner.</safety>
        public unsafe ReadOnlySpan<TNative> AsReadOnlySpanNative()
        {
            CheckRead();

            // SAFETY: Owner keeps the one-element buffer pinned for the returned read-only span.
            unsafe
            {
                return new ReadOnlySpan<TNative>(_buffer.GetUnsafeBufferPointer(), 1);
            }
        }

        /// <safety>The returned native array must not outlive this owner.</safety>
        public unsafe NativeArray<TNative> AsNativeArray()
        {
            CheckNativeAliasAccess();
            NativeArray<TNative> alias;

            // SAFETY: This owner keeps the one-element buffer pinned for the returned native array.
            unsafe
            {
                alias = _buffer.AsNativeArray();
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var aliasSafety = _safety;
            AtomicSafetyHandle.UseSecondaryVersion(ref aliasSafety);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref alias, aliasSafety);
#endif
            return alias;
        }

        /// <safety>The returned native slice must not outlive this owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeSlice<TNative> AsNativeSlice()
        {
            // SAFETY: The slice borrows the native array returned under the same owner lifetime.
            unsafe
            {
                return new(AsNativeArray());
            }
        }

        /// <safety>The returned pointer must not outlive this owner.</safety>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe TNative* GetUnsafeBufferPointer()
        {
            CheckWrite();
            // SAFETY: The write check validates this owner's live pinned buffer.
            unsafe
            {
                return _buffer.GetUnsafeBufferPointer();
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal AtomicSafetyHandle GetSafetyHandle()
        {
            CheckRead();
            return _safety;
        }
#endif

        private void Initialize(T value)
        {
            _version++;
            var buffer = new BufferShared<T, TNative>(new[] { value });

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            var safety = default(AtomicSafetyHandle);
#endif

            try
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                safety = AtomicSafetyHandle.Create();
                AtomicSafetyHandle.SetBumpSecondaryVersionOnScheduleWrite(safety, true);
#endif
                _buffer = buffer;
                buffer = default;

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                _safety = safety;
                safety = default;
#endif
            }
            finally
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
                if (AtomicSafetyHandle.IsDefaultValue(safety) == false)
                {
                    AtomicSafetyHandle.Release(safety);
                }
#endif
                // SAFETY: Any unpublished temporary buffer remains solely owned by this initializer.
                unsafe
                {
                    buffer.Dispose();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckRead()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(_buffer);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckReadAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckWrite()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(_buffer);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckWriteAndThrow(_safety);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckNativeAliasAccess()
        {
            DebuggingThrowHelper.ThrowIfNotCreated(_buffer);

#if ENABLE_UNITY_COLLECTIONS_CHECKS && !DISABLE_SHAREDARRAY_SAFETY
            AtomicSafetyHandle.CheckGetSecondaryDataPointerAndThrow(_safety);
#endif
        }
    }
}
