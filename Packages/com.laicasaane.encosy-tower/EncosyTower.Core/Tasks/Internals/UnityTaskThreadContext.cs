using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    internal enum UnityTaskThreadAffinity
    {
        None,
        MainThread,
        ThreadPool,
    }

    internal static class UnityTaskThreadContext
    {
        private static int s_mainThreadId;
        private static SynchronizationContext s_mainContext;

        internal static UnityTaskThreadAffinity CurrentAffinity
        {
            get
            {
                var mainThreadId = Volatile.Read(ref s_mainThreadId);

                if (mainThreadId == 0)
                {
                    return UnityTaskThreadAffinity.None;
                }

                return Thread.CurrentThread.ManagedThreadId == mainThreadId
                    ? UnityTaskThreadAffinity.MainThread
                    : UnityTaskThreadAffinity.ThreadPool;
            }
        }

        internal static bool Matches(UnityTaskThreadAffinity affinity)
            => affinity == UnityTaskThreadAffinity.None || affinity == CurrentAffinity;

        internal static void Run(UnityTaskThreadAffinity affinity, Action<object> continuation, object state)
        {
            if (Matches(affinity))
            {
                continuation(state);
                return;
            }

            if (affinity == UnityTaskThreadAffinity.MainThread)
            {
                var context = s_mainContext;

                if (context == null)
                {
                    continuation(state);
                    return;
                }

                context.Post(
                      static box => ((ContinuationBox)box).InvokeAndReturn()
                    , ContinuationBox.Rent(continuation, state)
                );

                return;
            }

            ThreadPool.UnsafeQueueUserWorkItem(
                  static box => ((ContinuationBox)box).InvokeAndReturn()
                , ContinuationBox.Rent(continuation, state)
            );
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CaptureOnRuntimeLoad()
        {
            Capture();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void CaptureOnEditorLoad()
        {
            Capture();
        }
#endif

        private static void Capture()
        {
            s_mainContext = SynchronizationContext.Current;
            Volatile.Write(ref s_mainThreadId, Thread.CurrentThread.ManagedThreadId);
        }

        private sealed class ContinuationBox
        {
            private const int MAX_POOL_SIZE = 256;

            private static readonly object s_poolLock = new();
            private static readonly Stack<ContinuationBox> s_pool = new();

            private Action<object> _continuation;
            private object _state;

            internal static ContinuationBox Rent(Action<object> continuation, object state)
            {
                ContinuationBox box;

                lock (s_poolLock)
                {
                    box = s_pool.Count > 0 ? s_pool.Pop() : new();
                }

                box._continuation = continuation;
                box._state = state;
                return box;
            }

            internal void InvokeAndReturn()
            {
                var continuation = _continuation;
                var state = _state;

                _continuation = null;
                _state = null;

                lock (s_poolLock)
                {
                    if (s_pool.Count < MAX_POOL_SIZE)
                    {
                        s_pool.Push(this);
                    }
                }

                continuation(state);
            }
        }
    }
}
