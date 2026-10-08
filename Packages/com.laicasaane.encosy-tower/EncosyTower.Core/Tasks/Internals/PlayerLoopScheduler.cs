using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EncosyTower.Tasks
{
    /// <summary>
    /// EncosyTower's player-loop runner: one queue of continuations per <see cref="UnityTaskTiming"/> phase, shared by
    /// both backends.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="EnsureInitialized"/> injects sixteen runner systems into Unity's player loop: at the start of each
    /// phase for the plain timings and at the end for the <c>Last*</c> timings. Re-injection replaces an existing
    /// runner of the same type, so a reload never doubles it.
    /// </para>
    /// <para>
    /// In the Editor outside Play Mode the player loop does not tick, so every queue also runs from
    /// <c>EditorApplication.update</c>. <see cref="Reset"/> runs at subsystem registration, which supports entering
    /// Play Mode without a domain reload.
    /// </para>
    /// </remarks>
    internal static class PlayerLoopScheduler
    {
        private static readonly PhaseQueue[] s_queues = CreateQueues();
        private static readonly object s_initializeLock = new();
        private static bool s_initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeRuntime()
            => EnsureInitialized();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeEditor()
            => EnsureInitialized();
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            lock (s_initializeLock)
            {
                s_initialized = false;
            }

#if UNITY_EDITOR
            EditorApplication.update -= RunEditorLoop;
#endif
        }

        /// <summary>
        /// Queues <paramref name="continuation"/> to run once at the next run of <paramref name="timing"/>. Safe to
        /// call from any thread.
        /// </summary>
        internal static void Schedule(UnityTaskTiming timing, Action continuation)
        {
            EnsureInitialized();
            s_queues[(int)timing].Enqueue(continuation);
        }

        private static PhaseQueue[] CreateQueues()
        {
            var result = new PhaseQueue[16];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new();
            }

            return result;
        }

        private static void EnsureInitialized()
        {
            if (s_initialized)
            {
                return;
            }

            lock (s_initializeLock)
            {
                if (s_initialized)
                {
                    return;
                }

                var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

                Inject(
                      ref playerLoop
                    , typeof(UnityEngine.PlayerLoop.Initialization)
                    , typeof(InitializationRunner)
                    , RunInitialization
                    , false
                );

                Inject(
                      ref playerLoop
                    , typeof(UnityEngine.PlayerLoop.Initialization)
                    , typeof(LastInitializationRunner)
                    , RunLastInitialization
                    , true
                );
                Inject(ref playerLoop, typeof(EarlyUpdate), typeof(EarlyUpdateRunner), RunEarlyUpdate, false);
                Inject(ref playerLoop, typeof(EarlyUpdate), typeof(LastEarlyUpdateRunner), RunLastEarlyUpdate, true);
                Inject(ref playerLoop, typeof(FixedUpdate), typeof(FixedUpdateRunner), RunFixedUpdate, false);
                Inject(ref playerLoop, typeof(FixedUpdate), typeof(LastFixedUpdateRunner), RunLastFixedUpdate, true);
                Inject(ref playerLoop, typeof(PreUpdate), typeof(PreUpdateRunner), RunPreUpdate, false);
                Inject(ref playerLoop, typeof(PreUpdate), typeof(LastPreUpdateRunner), RunLastPreUpdate, true);
                Inject(ref playerLoop, typeof(Update), typeof(UpdateRunner), RunUpdate, false);
                Inject(ref playerLoop, typeof(Update), typeof(LastUpdateRunner), RunLastUpdate, true);
                Inject(ref playerLoop, typeof(PreLateUpdate), typeof(PreLateUpdateRunner), RunPreLateUpdate, false);
                Inject(
                      ref playerLoop
                    , typeof(PreLateUpdate)
                    , typeof(LastPreLateUpdateRunner)
                    , RunLastPreLateUpdate
                    , true
                );
                Inject(ref playerLoop, typeof(PostLateUpdate), typeof(PostLateUpdateRunner), RunPostLateUpdate, false);
                Inject(
                      ref playerLoop
                    , typeof(PostLateUpdate)
                    , typeof(LastPostLateUpdateRunner)
                    , RunLastPostLateUpdate
                    , true
                );
                Inject(ref playerLoop, typeof(TimeUpdate), typeof(TimeUpdateRunner), RunTimeUpdate, false);
                Inject(ref playerLoop, typeof(TimeUpdate), typeof(LastTimeUpdateRunner), RunLastTimeUpdate, true);
                UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);

#if UNITY_EDITOR
                EditorApplication.update -= RunEditorLoop;
                EditorApplication.update += RunEditorLoop;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
                s_initialized = true;
            }
        }

        /// <summary>
        /// Finds the subsystem of <paramref name="targetType"/> anywhere in <paramref name="system"/> and adds a runner
        /// of <paramref name="runnerType"/> as its first or last child, or replaces the update of an existing runner of
        /// that type.
        /// </summary>
        private static bool Inject(
              ref PlayerLoopSystem system
            , Type targetType
            , Type runnerType
            , PlayerLoopSystem.UpdateFunction update
            , bool append
        )
        {
            if (system.type == targetType)
            {
                var source = system.subSystemList ?? Array.Empty<PlayerLoopSystem>();

                for (var i = 0; i < source.Length; i++)
                {
                    if (source[i].type?.FullName == runnerType.FullName)
                    {
                        source[i].updateDelegate = update;
                        system.subSystemList = source;
                        return true;
                    }
                }

                var destination = new PlayerLoopSystem[source.Length + 1];
                var runner = new PlayerLoopSystem
                {
                    type = runnerType,
                    updateDelegate = update,
                };

                if (append)
                {
                    Array.Copy(source, destination, source.Length);
                    destination[source.Length] = runner;
                }
                else
                {
                    destination[0] = runner;
                    Array.Copy(source, 0, destination, 1, source.Length);
                }

                system.subSystemList = destination;
                return true;
            }

            var children = system.subSystemList;

            if (children == null)
            {
                return false;
            }

            for (var i = 0; i < children.Length; i++)
            {
                if (Inject(ref children[i], targetType, runnerType, update, append))
                {
                    system.subSystemList = children;
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private static void RunEditorLoop()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            for (var i = 0; i < s_queues.Length; i++)
            {
                s_queues[i].Run();
            }
        }

        /// <summary>
        /// Drains the queues when the Editor enters or leaves edit mode, as UniTask's <c>PlayerLoopHelper</c> does.
        /// </summary>
        /// <remarks>
        /// Without this, a continuation queued during Play Mode would keep running from <c>EditorApplication.update</c>
        /// after Play Mode ends, and, with domain reload disabled, a continuation queued in one session would run in
        /// the next.
        /// </remarks>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.EnteredEditMode or PlayModeStateChange.ExitingEditMode)
            {
                FlushAndClear();
            }
        }

        /// <summary>
        /// Runs every queued continuation once, then drops whatever those continuations queued again.
        /// </summary>
        /// <remarks>
        /// A promise that reschedules itself is dropped, so a wait still pending at the transition never completes;
        /// its awaiting code stays suspended instead of resuming in the wrong mode.
        /// </remarks>
        internal static void FlushAndClear()
        {
            for (var i = 0; i < s_queues.Length; i++)
            {
                s_queues[i].Run();
                s_queues[i].Clear();
            }
        }
#endif

        private static void RunInitialization()
            => s_queues[0].Run();

        private static void RunLastInitialization()
            => s_queues[1].Run();

        private static void RunEarlyUpdate()
            => s_queues[2].Run();

        private static void RunLastEarlyUpdate()
            => s_queues[3].Run();

        private static void RunFixedUpdate()
            => s_queues[4].Run();

        private static void RunLastFixedUpdate()
            => s_queues[5].Run();

        private static void RunPreUpdate()
            => s_queues[6].Run();

        private static void RunLastPreUpdate()
            => s_queues[7].Run();

        private static void RunUpdate()
            => s_queues[8].Run();

        private static void RunLastUpdate()
            => s_queues[9].Run();

        private static void RunPreLateUpdate()
            => s_queues[10].Run();

        private static void RunLastPreLateUpdate()
            => s_queues[11].Run();

        private static void RunPostLateUpdate()
            => s_queues[12].Run();

        private static void RunLastPostLateUpdate()
            => s_queues[13].Run();

        private static void RunTimeUpdate()
            => s_queues[14].Run();

        private static void RunLastTimeUpdate()
            => s_queues[15].Run();

        /// <summary>
        /// The continuations of one phase.
        /// </summary>
        /// <remarks>
        /// <see cref="Run"/> moves only the continuations queued before it started into a reusable array, then runs
        /// them outside the lock. A continuation queued while they run waits for the next tick, so a promise that
        /// reschedules itself runs once per frame. An exception is logged and does not stop the remaining
        /// continuations.
        /// </remarks>
        private sealed class PhaseQueue
        {
            private readonly object _lock = new();
            private readonly Queue<Action> _incoming = new();
            private Action[] _running = Array.Empty<Action>();

            internal void Enqueue(Action continuation)
            {
                lock (_lock)
                {
                    _incoming.Enqueue(continuation);
                }
            }

#if UNITY_EDITOR
            internal void Clear()
            {
                lock (_lock)
                {
                    _incoming.Clear();
                }
            }
#endif

            internal void Run()
            {
                int count;

                lock (_lock)
                {
                    count = _incoming.Count;

                    if (count == 0)
                    {
                        return;
                    }

                    if (_running.Length < count)
                    {
                        _running = new Action[count];
                    }

                    for (var i = 0; i < count; i++)
                    {
                        _running[i] = _incoming.Dequeue();
                    }
                }

                for (var i = 0; i < count; i++)
                {
                    var continuation = _running[i];
                    _running[i] = null;

                    try
                    {
                        continuation();
                    }
                    catch (Exception exception)
                    {
                        ThrowHelper.LogUnobservedException(exception);
                    }
                }
            }
        }

        private sealed class InitializationRunner { }

        private sealed class LastInitializationRunner { }

        private sealed class EarlyUpdateRunner { }

        private sealed class LastEarlyUpdateRunner { }

        private sealed class FixedUpdateRunner { }

        private sealed class LastFixedUpdateRunner { }

        private sealed class PreUpdateRunner { }

        private sealed class LastPreUpdateRunner { }

        private sealed class UpdateRunner { }

        private sealed class LastUpdateRunner { }

        private sealed class PreLateUpdateRunner { }

        private sealed class LastPreLateUpdateRunner { }

        private sealed class PostLateUpdateRunner { }

        private sealed class LastPostLateUpdateRunner { }

        private sealed class TimeUpdateRunner { }

        private sealed class LastTimeUpdateRunner { }
    }
}
