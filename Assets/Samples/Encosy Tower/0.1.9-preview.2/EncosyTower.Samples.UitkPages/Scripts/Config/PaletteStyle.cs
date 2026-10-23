using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>Reads a page's configured colors and amounts from its resolved USS custom properties.</summary>
    internal sealed class PaletteStyle
    {
        private static readonly CustomStyleProperty<Color>[] s_colorProperties = CreateColorProperties();
        private static readonly CustomStyleProperty<float>[] s_amountProperties = CreateAmountProperties();
        private static readonly CustomStyleProperty<Color>[] s_surfaceProperties = CreateDimensionProperties(false);
        private static readonly CustomStyleProperty<Color>[] s_navigationProperties = CreateDimensionProperties(true);

        private readonly Color[] _colors = new Color[s_colorProperties.Length];
        private readonly float[] _amounts = new float[s_amountProperties.Length];
        private readonly Color[] _surfaces = new Color[s_surfaceProperties.Length];
        private readonly Color[] _navigations = new Color[s_navigationProperties.Length];

        private bool _isDark;
        private bool _isResolved;

        public bool IsResolved => _isResolved;

        public Color Color(ConfigColorType name)
        {
            var index = (int)name;

            if ((uint)index < (uint)_colors.Length)
            {
                return _colors[index];
            }

            EncosyCustomStyleExtensions.ReportUnknown(name);
            return default;
        }

        public float Amount(ConfigAmountType name)
        {
            var index = (int)name;

            if ((uint)index < (uint)_amounts.Length)
            {
                return _amounts[index];
            }

            EncosyCustomStyleExtensions.ReportUnknown(name);
            return default;
        }

        public Color SurfaceOf(int position)
            => (uint)position < (uint)_surfaces.Length ? _surfaces[position] : default;

        public Color NavigationOf(int position)
            => (uint)position < (uint)_navigations.Length ? _navigations[position] : default;

        public ToneStyle Tone(in ToneAmounts amounts)
        {
            var saturation = _isDark ? amounts.SaturationDark : amounts.Saturation;
            var lightness = _isDark ? amounts.LightnessDark : amounts.Lightness;

            return new ToneStyle(Amount(amounts.Hue), Amount(saturation), Amount(lightness));
        }

        public void Read(ICustomStyle style, bool dark)
        {
            _isDark = dark;
            _isResolved = true;

            ReadColors(style, s_colorProperties, _colors);
            ReadColors(style, s_surfaceProperties, _surfaces);
            ReadColors(style, s_navigationProperties, _navigations);

            var amountCount = s_amountProperties.Length;

            for (var i = 0; i < amountCount; i++)
            {
                style.Read(s_amountProperties[i], ref _amounts[i]);
            }
        }

        private static void ReadColors(ICustomStyle style, CustomStyleProperty<Color>[] properties, Color[] colors)
        {
            var count = properties.Length;

            for (var i = 0; i < count; i++)
            {
                style.Read(properties[i], ref colors[i]);
            }
        }

        private static CustomStyleProperty<Color>[] CreateColorProperties()
        {
            var names = ConfigColorTypeExtensions.Values.AsSpan();
            var properties = new CustomStyleProperty<Color>[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                properties[(int)names[i]] = ConfigNameAPI.ColorProperty(names[i]);
            }

            return properties;
        }

        private static CustomStyleProperty<float>[] CreateAmountProperties()
        {
            var names = ConfigAmountTypeExtensions.Values.AsSpan();
            var properties = new CustomStyleProperty<float>[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                properties[(int)names[i]] = ConfigNameAPI.AmountProperty(names[i]);
            }

            return properties;
        }

        private static CustomStyleProperty<Color>[] CreateDimensionProperties(bool navigation)
        {
            var count = DimensionCatalog.OUTER_COUNT + 1;
            var properties = new CustomStyleProperty<Color>[count];

            for (var position = 0; position < count; position++)
            {
                properties[position] = new(ConfigNameAPI.DimensionColorName(position, navigation));
            }

            return properties;
        }
    }
}
