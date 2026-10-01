using System;
using System.Collections.Generic;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Collections.Extensions
{
    public static class SharedQueueReadOnlyExtensions
    {
        public static bool Contains<T, TNative>(
              this in SharedQueue<T, TNative>.ReadOnly self
            , T item
        )
            where T : unmanaged, IEquatable<T>
            where TNative : unmanaged
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

        public static bool Contains<T, TNative, TComparer>(
              this in SharedQueue<T, TNative>.ReadOnly self
            , T item
            , TComparer comparer
        )
            where T : unmanaged
            where TNative : unmanaged
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
