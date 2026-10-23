using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>The hue offset, saturation and lightness of one tone.</summary>
    internal readonly record struct ToneStyle(float HueOffset, float Saturation, float Lightness);

    /// <summary>The configuration amount types that identify the HSL components of one tone.</summary>
    internal readonly record struct ToneAmounts(
          ConfigAmountType Hue
        , ConfigAmountType Saturation
        , ConfigAmountType Lightness
        , ConfigAmountType SaturationDark
        , ConfigAmountType LightnessDark
    );

    /// <summary>The button colors that a dimension screen derives from its surface color.</summary>
    internal readonly record struct DimensionTones(
          Color Button
        , Color NearA
        , Color NearB
        , Color ComplementA
        , Color ComplementB
    );

    /// <summary>Derives the colors of a dimension screen from its surface color.</summary>
    internal static class ToneMath
    {
        private static readonly ToneAmounts s_button = new(
              ConfigAmountType.ToneButtonHue
            , ConfigAmountType.ToneButtonSaturation
            , ConfigAmountType.ToneButtonLightness
            , ConfigAmountType.ToneButtonSaturationDark
            , ConfigAmountType.ToneButtonLightnessDark
        );

        private static readonly ToneAmounts s_nearA = new(
              ConfigAmountType.ToneNearAHue
            , ConfigAmountType.ToneNearASaturation
            , ConfigAmountType.ToneNearALightness
            , ConfigAmountType.ToneNearASaturationDark
            , ConfigAmountType.ToneNearALightnessDark
        );

        private static readonly ToneAmounts s_nearB = new(
              ConfigAmountType.ToneNearBHue
            , ConfigAmountType.ToneNearBSaturation
            , ConfigAmountType.ToneNearBLightness
            , ConfigAmountType.ToneNearBSaturationDark
            , ConfigAmountType.ToneNearBLightnessDark
        );

        private static readonly ToneAmounts s_complementA = new(
              ConfigAmountType.ToneComplementAHue
            , ConfigAmountType.ToneComplementASaturation
            , ConfigAmountType.ToneComplementALightness
            , ConfigAmountType.ToneComplementASaturationDark
            , ConfigAmountType.ToneComplementALightnessDark
        );

        private static readonly ToneAmounts s_complementB = new(
              ConfigAmountType.ToneComplementBHue
            , ConfigAmountType.ToneComplementBSaturation
            , ConfigAmountType.ToneComplementBLightness
            , ConfigAmountType.ToneComplementBSaturationDark
            , ConfigAmountType.ToneComplementBLightnessDark
        );

        public static DimensionTones Analogous(Color color, PaletteStyle palette)
        {
            Color.RGBToHSV(color, out var hue, out _, out _);

            var degrees = hue * 360f;

            return new DimensionTones(
                  Button: FromTone(degrees, palette.Tone(in s_button))
                , NearA: FromTone(degrees, palette.Tone(in s_nearA))
                , NearB: FromTone(degrees, palette.Tone(in s_nearB))
                , ComplementA: FromTone(degrees, palette.Tone(in s_complementA))
                , ComplementB: FromTone(degrees, palette.Tone(in s_complementB))
            );
        }

        private static Color FromTone(float degrees, ToneStyle tone)
            => FromHsl(Mathf.Round(degrees + tone.HueOffset), tone.Saturation, tone.Lightness);

        private static Color FromHsl(float degrees, float saturation, float lightness)
        {
            var hue = (degrees % 360f + 360f) % 360f;
            var chroma = (1f - Mathf.Abs(2f * lightness - 1f)) * saturation;
            var sector = hue / 60f;
            var second = chroma * (1f - Mathf.Abs(sector % 2f - 1f));
            var offset = lightness - chroma / 2f;
            float red;
            float green;
            float blue;

            switch ((int)sector)
            {
                case 0:
                {
                    (red, green, blue) = (chroma, second, 0f);
                    break;
                }

                case 1:
                {
                    (red, green, blue) = (second, chroma, 0f);
                    break;
                }

                case 2:
                {
                    (red, green, blue) = (0f, chroma, second);
                    break;
                }

                case 3:
                {
                    (red, green, blue) = (0f, second, chroma);
                    break;
                }

                case 4:
                {
                    (red, green, blue) = (second, 0f, chroma);
                    break;
                }

                default:
                {
                    (red, green, blue) = (chroma, 0f, second);
                    break;
                }
            }

            return new Color(red + offset, green + offset, blue + offset, 1f);
        }
    }
}
