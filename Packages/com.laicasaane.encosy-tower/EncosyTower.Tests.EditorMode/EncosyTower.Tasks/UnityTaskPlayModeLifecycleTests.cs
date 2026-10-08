using System;
using System.Collections;
using System.Collections.Generic;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Tasks
{
    /// <summary>
    /// Enters and exits Play Mode with domain reload disabled, and checks that UnityTask waits pending at each
    /// transition never resume, and that the module keeps working in both modes afterward.
    /// </summary>
    public class UnityTaskPlayModeLifecycleTests
    {
        private const int MAX_WAIT_FRAMES = 600;
        private const int SETTLE_FRAMES = 10;

        private readonly List<string> _log = new();

        private bool _optionsEnabled;
        private EnterPlayModeOptions _options;

        [SetUp]
        public void SetUp()
        {
            _log.Clear();
            _optionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            _options = EditorSettings.enterPlayModeOptions;

            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = _options | EnterPlayModeOptions.DisableDomainReload;
        }

        [TearDown]
        public void TearDown()
        {
            EditorSettings.enterPlayModeOptionsEnabled = _optionsEnabled;
            EditorSettings.enterPlayModeOptions = _options;
        }

        [UnityTest]
        public IEnumerator PendingWaits_NeverResumeAfterTransitions_AndModuleKeepsWorking()
        {
            var editPolls = new int[1];
            var playPolls = new int[1];

            // Edit mode: waits that must be dropped when entering Play Mode.
            Watch("edit WaitUntil", UnityTask.WaitUntil(() => ++editPolls[0] < 0));
            Watch("edit DelayFrameAsync", UnityTask.DelayFrameAsync(1_000_000));
            Watch("edit Delay", UnityTask.Delay(TimeSpan.FromMinutes(10), UnityTaskDelayType.Realtime));

            yield return new EnterPlayMode(expectDomainReload: false);

            Assert.IsTrue(Application.isPlaying);

            var editPollsAtEnter = editPolls[0];

            yield return Settle();

            Assert.AreEqual(editPollsAtEnter, editPolls[0], "Edit-mode WaitUntil kept polling in Play Mode.");
            Assert.IsEmpty(_log, "An edit-mode wait resumed in Play Mode.");

            // Play mode: the module works.
            yield return AssertShortWaitsComplete("play");

            // Play mode: waits that must be dropped when exiting Play Mode.
            var pendingSource = new AwaitableCompletionSource();

            Watch("play WaitUntil", UnityTask.WaitUntil(() => ++playPolls[0] < 0));
            Watch("play DelayFrameAsync", UnityTask.DelayFrameAsync(1_000_000));
            Watch("play Delay", UnityTask.Delay(TimeSpan.FromMinutes(10)));
            Watch("play NextFrame then Delay", NextFrameThenDelayAsync());
            Watch("play relay WaitForSecondsAsync", Awaitable.WaitForSecondsAsync(600f).AsUnityTask());
            Watch("play relay pending source", pendingSource.Awaitable.AsUnityTask());

            yield return null;
            yield return null;

            Assert.Greater(playPolls[0], 0, "Play-mode WaitUntil did not poll in Play Mode.");

            yield return new ExitPlayMode();

            Assert.IsFalse(Application.isPlaying);

            var playPollsAtExit = playPolls[0];

            yield return Settle();

            Assert.AreEqual(playPollsAtExit, playPolls[0], "Play-mode WaitUntil kept polling in Edit Mode.");
            Assert.IsEmpty(_log, "A play-mode wait resumed in Edit Mode.");

            // Edit mode: the module works again.
            yield return AssertShortWaitsComplete("edit");

            // Play mode again: churn Unity's Awaitable pool. A relay still registered on a dropped Awaitable must not
            // be resumed by a recycled Awaitable instance.
            yield return new EnterPlayMode(expectDomainReload: false);

            var churned = 0;

            for (var frame = 0; frame < 20; frame++)
            {
                for (var i = 0; i < 300; i++)
                {
                    Count(Awaitable.NextFrameAsync().AsUnityTask()).Forget();
                }

                yield return null;
            }

            yield return WaitFor(() => churned == 20 * 300);

            Assert.AreEqual(20 * 300, churned, "Not all churned relays completed.");
            Assert.IsEmpty(_log, "A dropped wait resumed after Awaitable churn.");

            yield return AssertShortWaitsComplete("play again");
            yield return new ExitPlayMode();

            yield return Settle();

            Assert.IsEmpty(_log, "A dropped wait resumed after the last transition.");

            async UnityTaskVoid Count(UnityTask task)
            {
                await task;
                churned++;
            }

            static async UnityTask NextFrameThenDelayAsync()
            {
                await UnityTask.NextFrameAsync();
                await UnityTask.Delay(TimeSpan.FromMinutes(10));
            }
        }

        [UnityTest]
        public IEnumerator RelayOfPendingSource_CompletesWhenSourceCompletesInSameMode()
        {
            yield return new EnterPlayMode(expectDomainReload: false);

            var source = new AwaitableCompletionSource<int>();
            var results = new List<int>();

            Capture(source.Awaitable.AsUnityTask()).Forget();

            yield return null;

            Assert.IsEmpty(results);

            source.SetResult(42);

            yield return WaitFor(() => results.Count > 0);

            CollectionAssert.AreEqual(new[] { 42 }, results);

            // The relay went back to the pool; a fresh relay must not observe the old result.
            var next = new AwaitableCompletionSource<int>();

            Capture(next.Awaitable.AsUnityTask()).Forget();

            yield return null;

            CollectionAssert.AreEqual(new[] { 42 }, results);

            next.SetResult(7);

            yield return WaitFor(() => results.Count > 1);

            CollectionAssert.AreEqual(new[] { 42, 7 }, results);

            yield return new ExitPlayMode();

            async UnityTaskVoid Capture(UnityTask<int> task)
                => results.Add(await task);
        }

        private void Watch(string name, UnityTask task)
        {
            RunAsync().Forget();

            async UnityTaskVoid RunAsync()
            {
                try
                {
                    await task;
                    _log.Add($"{name}: completed, playing={Application.isPlaying}");
                }
                catch (Exception exception)
                {
                    _log.Add($"{name}: {exception.GetType().Name}, playing={Application.isPlaying}");
                }
            }
        }

        private static IEnumerator AssertShortWaitsComplete(string mode)
        {
            var done = new bool[1];
            var step = new int[1];

            RunAsync().Forget();

            yield return WaitFor(() => done[0]);

            Assert.IsTrue(done[0], $"Short waits did not complete in {mode} mode; stopped after step {step[0]}.");

            async UnityTaskVoid RunAsync()
            {
                await UnityTask.Yield();
                step[0]++;
                await UnityTask.NextFrameAsync();
                step[0]++;
                await UnityTask.DelayFrameAsync(2);
                step[0]++;
                await UnityTask.Delay(TimeSpan.FromMilliseconds(10), UnityTaskDelayType.Realtime);
                step[0]++;
                await UnityTask.WaitUntil(static () => true);
                step[0]++;
                // Unity resumes frame-based Awaitables only while the player loop runs, so outside Play Mode the
                // relay is checked with a completion source instead.
                if (Application.isPlaying)
                {
                    await Awaitable.NextFrameAsync().AsUnityTask();
                }
                else
                {
                    var awaitableSource = new AwaitableCompletionSource();

                    UnityTask.Post(awaitableSource.SetResult);
                    await awaitableSource.Awaitable.AsUnityTask();
                }

                step[0]++;
                await UnityTask.RunOnThreadPool(static () => { });
                step[0]++;
                await UnityTask.SwitchToMainThreadAsync();
                step[0]++;

                var source = new UnityTaskCompletionSource<int>();

                UnityTask.Post(() => source.TrySetResult(1));

                Assert.AreEqual(1, await source.Task);
                step[0]++;

                done[0] = true;
            }
        }

        private static IEnumerator WaitFor(Func<bool> condition)
        {
            for (var frame = 0; frame < MAX_WAIT_FRAMES && condition() == false; frame++)
            {
                yield return null;
            }
        }

        private static IEnumerator Settle()
        {
            for (var frame = 0; frame < SETTLE_FRAMES; frame++)
            {
                yield return null;
            }
        }
    }
}
