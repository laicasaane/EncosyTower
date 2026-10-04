using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EncosyTower.Tasks
{
    internal readonly struct ThreadPoolSwitch
    {
        public Awaiter GetAwaiter()
            => default;

        internal readonly struct Awaiter : ICriticalNotifyCompletion
        {
            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
                => ThreadPool.UnsafeQueueUserWorkItem(static state => ((Action)state)(), continuation);

            public void UnsafeOnCompleted(Action continuation)
                => ThreadPool.UnsafeQueueUserWorkItem(static state => ((Action)state)(), continuation);
        }
    }
}
