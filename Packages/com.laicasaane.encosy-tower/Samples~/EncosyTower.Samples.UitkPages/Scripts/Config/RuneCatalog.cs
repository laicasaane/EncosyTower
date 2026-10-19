using System;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.UnityExtensions;
using UnityEngine;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>The runes, built from the resources, and the permutation of a new game.</summary>
    internal static class RuneCatalog
    {
        public const int COUNT = DimensionCatalog.OUTER_COUNT;

        private static readonly Texture2D[] s_textures = new Texture2D[COUNT];

        private static string[] s_letters = Array.Empty<string>();

        private static string[] s_collectedTooltips = Array.Empty<string>();

        private static string s_emptyTooltip = string.Empty;

        private static AppViewResources s_resources;

        public static string EmptyTooltip
            => s_emptyTooltip;

        public static bool Initialize(in AppViewResources resources)
        {
            var letters = new string[COUNT];
            var collectedTooltips = new string[COUNT];
            var seen = 0;

            for (var i = 0; i < COUNT; i++)
            {
                var text = resources.Get(i, RuneDetail.Id);

                if (RuneExtensions.TryParse(text, out var rune) == false
                    || (uint)rune >= (uint)COUNT
                    || (seen & (1 << (int)rune)) != 0
                )
                {
                    ThrowHelper.LogError_ConfigInvalid($"the rune {i} has the unknown or repeated id '{text}'");
                    return false;
                }

                seen |= 1 << (int)rune;
                letters[(int)rune] = resources.Get(i, RuneDetail.Letter);
                collectedTooltips[i] = resources.Format(ConfigTextType.RuneTooltipCollected, i + 1);
            }

            s_letters = letters;
            s_collectedTooltips = collectedTooltips;
            s_emptyTooltip = resources.Get(ConfigTextType.RuneTooltipEmpty);
            s_resources = resources;
            Array.Clear(s_textures, 0, s_textures.Length);

            return true;
        }

        public static void Reset()
        {
            s_letters = Array.Empty<string>();
            s_collectedTooltips = Array.Empty<string>();
            s_emptyTooltip = string.Empty;
            s_resources = default;
            Array.Clear(s_textures, 0, s_textures.Length);
        }

        public static string LetterOf(Rune rune)
            => s_letters[(int)rune];

        public static string CollectedTooltip(int number)
            => (uint)(number - 1) < (uint)s_collectedTooltips.Length
                ? s_collectedTooltips[number - 1]
                : s_resources.Format(ConfigTextType.RuneTooltipCollected, number);

        public static Texture2D TextureOf(Rune rune)
        {
            ref var texture = ref s_textures[(int)rune];

            if (texture.IsInvalid())
            {
                texture = Resources.Load<Texture2D>(AssetKeyAPI.Key(rune).Value);
            }

            return texture;
        }

        public static Rune[] NewPermutation(System.Random random)
        {
            var values = new Rune[COUNT] { Rune.Ehwaz, Rune.Naudiz, Rune.Kaun, Rune.Othala, Rune.Sowilo, Rune.Isa };

            for (var i = COUNT - 1; i >= 1; i--)
            {
                var j = random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }

            return values;
        }
    }
}
