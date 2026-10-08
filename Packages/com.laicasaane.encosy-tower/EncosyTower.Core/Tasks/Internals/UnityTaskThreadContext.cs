using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// The kind of thread on which a task resumes its awaiter.
    /// </summary>
    internal enum UnityTaskThreadAffinity
    {
        /// <summary>Resume inline on the completing thread; also reported before the main thread is captured.</summary>
        None,
        /// <summary>Resume on Unity's main thread.</summary>
        MainThread,
        /// <summary>Resume on any thread-pool thread.</summary>
        ThreadPool,
    }

    /// <summary>
    /// Records Unity's main thread and dispatches continuations to a task creator's thread kind.
    /// </summary>
    /// <remarks>
    /// The main thread id and Unity's <see cref="SynchronizationContext"/> are captured at Editor load and at runtime
    /// subsystem registration. Every thread that is not the main thread counts as a thread-pool thread.
    /// </remarks>
    internal static class UnityTaskThreadContext
    {
        private static int s_mainThreadId;
        private static SynchronizationContext s_mainContext;

        /// <summary>
        /// Gets the thread kind of the calling thread, or <see cref="UnityTaskThreadAffinity.None"/> before the main
        /// thread is captured.
        /// </summary>
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

        /// <summary>
        /// Returns whether a continuation for <paramref name="affinity"/> may run inline on the calling thread.
        /// </summary>
        internal static bool Matches(UnityTaskThreadAffinity affinity)
            => affinity == UnityTaskThreadAffinity.None || affinity == CurrentAffinity;

        /// <summary>
        /// Runs <paramref name="continuation"/> inline when the calling thread matches <paramref name="affinity"/>;
        /// otherwise posts it to Unity's main <see cref="SynchronizationContext"/> (inline when none was captured) or
        /// queues it to the <see cref="ThreadPool"/>. Posted work is wrapped in a pooled <see cref="ContinuationBox"/>
        /// so no closure is allocated.
        /// </summary>
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

        /// <summary>
        /// Pooled holder of a continuation and its state for one posted call. <see cref="InvokeAndReturn"/> clears and
        /// returns the box before invoking, so the box can be reused while the continuation runs.
        /// </summary>
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
