using System;

namespace EncosyTower.Tasks
{
    static class ThrowHelper
    {
        internal static void ThrowIfCountOutOfRange(int count, int length)
        {
            if ((uint)count > (uint)length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }
    }
}
