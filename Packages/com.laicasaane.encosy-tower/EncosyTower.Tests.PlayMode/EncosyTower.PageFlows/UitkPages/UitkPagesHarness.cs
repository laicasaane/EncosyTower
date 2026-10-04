using System;
using System.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Tests.PageFlows.UitkPages
{
    internal sealed class UitkPagesHarness : IDisposable
    {
        public const string ELEMENT_PAGE = "uitk-test-element-page";
        public const string TWO_PAGES = "uitk-test-two-pages";
        public const string BEHAVIOUR_PAGE = "uitk-test-behaviour-page";
        public const string NOT_A_PAGE = "uitk-test-not-a-page";
        public const string LAYOUT = "uitk-test-layout";
        public const string TRANSITIONS = "uitk-test-transitions";

        public static readonly PageContext ZeroDuration = new() {
            ShowOptions = PageTransitionOptions.ZeroDuration,
            HideOptions = PageTransitionOptions.ZeroDuration,
        };

        private readonly PanelSettings _panelSettings;

        private UitkPagesHarness(
              GameObject gameObject
            , UitkPageCodex codex
            , UitkTestInitializer initializer
            , PanelSettings panelSettings
        )
        {
            GameObject = gameObject;
            Codex = codex;
            Initializer = initializer;
            _panelSettings = panelSettings;
        }

        public GameObject GameObject { get; }

        public UitkPageCodex Codex { get; }

        public UitkTestInitializer Initializer { get; }

        public static async Task<UitkPagesHarness> CreateAsync(
              VisualTreeAsset layoutAsset
            , params (string Identifier, PageFlowKind Kind, string ContainerName)[] definitions
        )
        {
            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            var gameObject = new GameObject(nameof(UitkPagesHarness));
            gameObject.SetActive(false);

            var codex = gameObject.AddComponent<UitkPageCodex>();
            var initializer = gameObject.AddComponent<UitkTestInitializer>();
            var flows = new UitkPageCodex.FlowDefinition[definitions.Length];

            for (var i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];

                flows[i] = new UitkPageCodex.FlowDefinition {
                    identifier = definition.Identifier,
                    kind = definition.Kind,
                    containerName = definition.ContainerName,
                };
            }

            codex._flows = flows;
            codex._panelSettings = panelSettings;
            codex._layoutAsset = layoutAsset;
            codex._flowContext.autoInitializeOnAwake = true;
            codex._flowContext.loadStrategy = new(value: PageLoaderStrategy.Resources, isOverridden: true);

            gameObject.SetActive(true);

            var harness = new UitkPagesHarness(gameObject, codex, initializer, panelSettings);
            await WaitUntilAsync(() => codex.PanelRoot != null);
            return harness;
        }

        public static Task<UitkPagesHarness> CreateAsync(
              params (string Identifier, PageFlowKind Kind, string ContainerName)[] definitions
        )
            => CreateAsync(null, definitions);

        public static async Task WaitUntilAsync(Func<bool> condition, int maxFrames = 300)
        {
            for (var i = 0; i < maxFrames; i++)
            {
                if (condition())
                {
                    return;
                }

                await UnityTask.NextFrameAsync();
            }

            Assert.Fail($"The condition was not met within {maxFrames} frames.");
        }

        public static async Task WaitFramesAsync(int frames)
        {
            for (var i = 0; i < frames; i++)
            {
                await UnityTask.NextFrameAsync();
            }
        }

        public T GetFlow<T>(string identifier)
            where T : UitkPageFlow
        {
            Assert.IsTrue(Codex.GetFlow(identifier).TryGetValue(out var flow), $"No flow '{identifier}'.");
            return (T)flow;
        }

        public static TemplateContainer GetSlot(IUitkPage page)
            => page switch {
                UitkPageElement element => element.Slot,
                UitkPageBehaviour behaviour => behaviour.Slot,
                _ => null,
            };

        public static bool IsDisplayed(VisualElement element)
            => element.style.display.value != DisplayStyle.None;

        public void Dispose()
        {
            UnityEngine.Object.Destroy(GameObject);
            UnityEngine.Object.Destroy(_panelSettings);
        }
    }
}
