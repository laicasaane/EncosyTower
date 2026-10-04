#if UNITY_UGUI

using System.Collections;
using System.Text.RegularExpressions;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageCodexInitializerTests
    {
        private GameObject _gameObject;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject.IsValid())
            {
                Object.Destroy(_gameObject);
            }
        }

        [UnityTest]
        public IEnumerator MatchingDefinitions_CallTheHookOnceWithTheScopes()
        {
            var (codex, initializer) = CreateCodex("Screen", "Popup");

            codex.Initialize();
            yield return null;

            Assert.AreEqual(1, initializer.CallCount);
            Assert.AreSame(codex, initializer.Codex);
            Assert.IsTrue(codex.TryGetFlowScope("Screen", out var screen));
            Assert.IsTrue(codex.TryGetFlowScope("Popup", out var popup));
            Assert.AreEqual(screen, initializer.Scopes.Screen);
            Assert.AreEqual(popup, initializer.Scopes.Popup);
        }

        [UnityTest]
        public IEnumerator MissingDefinition_LogsTheMismatchAndSkipsTheHook()
        {
            var (codex, initializer) = CreateCodex("Screen");

            LogAssert.Expect(LogType.Error, new Regex("do not match the properties"));
            codex.Initialize();
            yield return null;

            Assert.AreEqual(0, initializer.CallCount);
        }

        private (UguiPageCodex, RecordingInitializer) CreateCodex(params string[] identifiers)
        {
            _gameObject = new GameObject(nameof(UguiPageCodexInitializerTests), typeof(RectTransform));

            var codex = _gameObject.AddComponent<UguiPageCodex>();
            var initializer = _gameObject.AddComponent<RecordingInitializer>();
            var flows = new UguiPageCodex.FlowDefinition[identifiers.Length];

            for (var i = 0; i < identifiers.Length; i++)
            {
                flows[i] = new UguiPageCodex.FlowDefinition {
                    identifier = identifiers[i],
                    kind = PageFlowKind.SinglePageStack,
                };
            }

            codex._flows = flows;
            return (codex, initializer);
        }

        private sealed class RecordingInitializer : UguiPageCodexInitializer<InitializerTestScopes>
        {
            public int CallCount { get; private set; }

            public UguiPageCodex Codex { get; private set; }

            public InitializerTestScopes Scopes { get; private set; }

            protected override UnityTask OnInitializeAsync(UguiPageCodex codex, InitializerTestScopes scopes)
            {
                CallCount++;
                Codex = codex;
                Scopes = scopes;
                return UnityTask.CompletedTask;
            }
        }
    }

    [PageFlowScopeCollection]
    internal partial struct InitializerTestScopes
    {
        public PageFlowScope Screen { get; private set; }

        public PageFlowScope Popup { get; private set; }
    }
}

#endif
