using System;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>The runes, built from the definitions in the configuration, and the new-game permutation.</summary>
    internal static class RuneCatalog
    {
        public const int COUNT = DimensionCatalog.OUTER_COUNT;

        private static readonly Sprite[] s_sprites = new Sprite[COUNT];

        private static string[] s_letters = Array.Empty<string>();

        private static AppConfig s_config;

        public static Color DefaultInk => s_config.Color(ConfigColorType.RuneDefaultInk);

        public static float FrameAlpha => s_config.Amount(ConfigAmountType.RuneFrameAlpha);

        public static float BorderAlpha => s_config.Amount(ConfigAmountType.RuneBorderAlpha);

        public static float BorderAlphaBlank => s_config.Amount(ConfigAmountType.RuneBorderAlphaBlank);

        public static bool IsValid(AppConfig config)
        {
            var definitions = config.Runes;

            if (definitions.Length != COUNT)
            {
                return false;
            }

            var seen = 0;

            for (var i = 0; i < COUNT; i++)
            {
                var id = (int)definitions[i].Id;

                if ((uint)id >= (uint)COUNT || (seen & (1 << id)) != 0)
                {
                    return false;
                }

                seen |= 1 << id;
            }

            return true;
        }

        public static void Initialize(AppConfig config)
        {
            var runes = config.Runes;
            var letters = new string[COUNT];

            for (var i = 0; i < runes.Length; i++)
            {
                letters[(int)runes[i].Id] = runes[i].letter;
            }

            s_letters = letters;
            s_config = config;
            Array.Clear(s_sprites, 0, s_sprites.Length);
        }

        public static void Reset()
        {
            s_letters = Array.Empty<string>();
            s_config = null;
            Array.Clear(s_sprites, 0, s_sprites.Length);
        }

        public static string LetterOf(Rune rune)
            => s_letters[(int)rune];

        public static Sprite SpriteOf(Rune rune)
        {
            ref var sprite = ref s_sprites[(int)rune];

            if (sprite.IsInvalid())
            {
                sprite = Resources.Load<Sprite>(AssetKeyAPI.Key(rune).Value);
            }

            return sprite;
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
