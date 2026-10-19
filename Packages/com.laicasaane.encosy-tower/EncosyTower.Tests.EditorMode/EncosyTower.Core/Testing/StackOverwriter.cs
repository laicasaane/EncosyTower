using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Tests.Core.Testing
{
    public static class StackOverwriter
    {
        private const int SIZE = 16 * 1024;
        private const int STRIDE = 64;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Fill(byte value)
        {
            Span<byte> buffer = stackalloc byte[SIZE];
            buffer.Fill(value);

            var checksum = 0;

            for (var i = 0; i < SIZE; i += STRIDE)
            {
                checksum += buffer[i];
            }

            return checksum;
        }
    }
}
