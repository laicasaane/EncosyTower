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
#endif
                s_initialized = true;
            }
        }

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
