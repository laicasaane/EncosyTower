using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Config
{
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
        public static DimensionTones Analogous(AppConfig config, Color surface)
        {
            Color.RGBToHSV(surface, out var hue, out _, out _);

            var degrees = Mathf.Floor(hue * 360f + 0.5f);

            return new DimensionTones(
                  Button: FromTone(
                      config
                    , degrees
                    , ConfigAmountType.ToneButtonHue
                    , ConfigAmountType.ToneButtonSaturation
                    , ConfigAmountType.ToneButtonLightness
                    , ConfigAmountType.ToneButtonSaturationDark
                    , ConfigAmountType.ToneButtonLightnessDark
                  )
                , NearA: FromTone(
                      config
                    , degrees
                    , ConfigAmountType.ToneNearAHue
                    , ConfigAmountType.ToneNearASaturation
                    , ConfigAmountType.ToneNearALightness
                    , ConfigAmountType.ToneNearASaturationDark
                    , ConfigAmountType.ToneNearALightnessDark
                  )
                , NearB: FromTone(
                      config
                    , degrees
                    , ConfigAmountType.ToneNearBHue
                    , ConfigAmountType.ToneNearBSaturation
                    , ConfigAmountType.ToneNearBLightness
                    , ConfigAmountType.ToneNearBSaturationDark
                    , ConfigAmountType.ToneNearBLightnessDark
                  )
                , ComplementA: FromTone(
                      config
                    , degrees
                    , ConfigAmountType.ToneComplementAHue
                    , ConfigAmountType.ToneComplementASaturation
                    , ConfigAmountType.ToneComplementALightness
                    , ConfigAmountType.ToneComplementASaturationDark
                    , ConfigAmountType.ToneComplementALightnessDark
                  )
                , ComplementB: FromTone(
                      config
                    , degrees
                    , ConfigAmountType.ToneComplementBHue
                    , ConfigAmountType.ToneComplementBSaturation
                    , ConfigAmountType.ToneComplementBLightness
                    , ConfigAmountType.ToneComplementBSaturationDark
                    , ConfigAmountType.ToneComplementBLightnessDark
                  )
            );
        }

        private static Color FromTone(
              AppConfig config
            , float degrees
            , ConfigAmountType hue
            , ConfigAmountType saturation
            , ConfigAmountType lightness
            , ConfigAmountType saturationDark
            , ConfigAmountType lightnessDark
        )
        {
            var isDark = config.ActiveTheme == Theme.Dark;

            return FromHsl(
                  degrees + config.Amount(hue)
                , config.Amount(isDark ? saturationDark : saturation)
                , config.Amount(isDark ? lightnessDark : lightness)
            );
        }

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
