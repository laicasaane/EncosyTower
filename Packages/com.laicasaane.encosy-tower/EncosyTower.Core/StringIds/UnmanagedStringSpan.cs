using System;
using System.Runtime.CompilerServices;
using EncosyTower.SystemExtensions;

namespace EncosyTower.StringIds
{
    internal readonly ref struct UnmanagedStringSpan
    {
        private readonly ReadOnlySpan<Range> _ranges;
        private readonly ReadOnlySpan<byte> _buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedStringSpan(ReadOnlySpan<Range> ranges, ReadOnlySpan<byte> buffer)
        {
            _ranges = ranges;
            _buffer = buffer;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ranges.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnmanagedStringSpan Slice(int start, int length)
            => new(_ranges.Slice(start, length), _buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySlice(int start, int length, out UnmanagedStringSpan span)
        {
            if ((uint)start > (uint)Length || (uint)length > (uint)(Length - start))
            {
                span = this;
                return false;
            }

            span = Slice(start, length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<UnmanagedString> destination)
            => CopyTo(destination, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<UnmanagedString> destination, int length)
            => CopyTo(0, destination, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int sourceStartIndex, Span<UnmanagedString> destination, int length)
            => Copy(Slice(sourceStartIndex, length), destination[..length]);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<UnmanagedString> destination)
            => TryCopyTo(destination, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<UnmanagedString> destination, int length)
            => TryCopyTo(0, destination, length);

        public bool TryCopyTo(int sourceStartIndex, Span<UnmanagedString> destination, int length)
        {
            if ((uint)sourceStartIndex > (uint)Length || (uint)length > (uint)(Length - sourceStartIndex))
            {
                return false;
            }

            if ((uint)length > (uint)destination.Length)
            {
                return false;
            }

            return TryCopy(Slice(sourceStartIndex, length), destination[..length]);
        }

        private static void Copy(UnmanagedStringSpan source, Span<UnmanagedString> dest)
        {
            var ranges = source._ranges;
            var buffer = source._buffer;
            var length = ranges.Length;

            for (var i = 0; i < length; i++)
            {
                var resultOpt = UnmanagedString.FromBufferAt(ranges[i], buffer);
                dest[i] = resultOpt.GetValueOrThrow();
            }
        }

        private static bool TryCopy(UnmanagedStringSpan source, Span<UnmanagedString> dest)
        {
            var ranges = source._ranges;
            var buffer = source._buffer;
            var length = ranges.Length;

            for (var i = 0; i < length; i++)
            {
                if (ranges[i].TryGetOffsetAndLength(buffer.Length).HasValue == false)
                {
                    return false;
                }
            }

            Copy(source, dest);
            return true;
        }
    }
}
