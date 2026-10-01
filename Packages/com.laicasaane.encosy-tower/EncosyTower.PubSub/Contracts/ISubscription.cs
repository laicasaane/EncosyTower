using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.PubSub
{
    public interface ISubscription : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unsubscribe()
            => Dispose();
    }
}
