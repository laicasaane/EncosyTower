using System;
using System.Collections.Generic;
using EncosyTower.Collections.Unsafe;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class StackUnsafeReadOnlyExtensions
    {
        public static bool Contains<T>(this in StackUnsafe<T>.ReadOnly self, T item)
            where T : unmanaged, IEquatable<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            foreach (var x in self)
            {
                if (x.Equals(item))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains<T, TComparer>(
              this in StackUnsafe<T>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TComparer : unmanaged, IEqualityComparer<T>
        {
            DebuggingThrowHelper.ThrowIfNotCreated(self);
            foreach (var x in self)
            {
                if (comparer.Equals(x, item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
