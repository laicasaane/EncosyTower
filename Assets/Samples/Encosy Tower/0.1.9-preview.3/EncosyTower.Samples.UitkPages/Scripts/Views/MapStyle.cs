using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>The node positions and the line style of the map, read from USS custom properties.</summary>
    internal sealed class MapStyle
    {
        private static readonly ConfigAmountType[] s_nodeXNames = {
            ConfigAmountType.MapNode0X,
            ConfigAmountType.MapNode1X,
            ConfigAmountType.MapNode2X,
            ConfigAmountType.MapNode3X,
            ConfigAmountType.MapNode4X,
            ConfigAmountType.MapNode5X,
        };
        private static readonly ConfigAmountType[] s_nodeYNames = {
            ConfigAmountType.MapNode0Y,
            ConfigAmountType.MapNode1Y,
            ConfigAmountType.MapNode2Y,
            ConfigAmountType.MapNode3Y,
            ConfigAmountType.MapNode4Y,
            ConfigAmountType.MapNode5Y,
        };
        private static readonly CustomStyleProperty<float>[] s_nodeX = CreateProperties(s_nodeXNames);
        private static readonly CustomStyleProperty<float>[] s_nodeY = CreateProperties(s_nodeYNames);
        private static readonly CustomStyleProperty<float> s_centerX
            = ConfigNameAPI.AmountProperty(ConfigAmountType.MapCenterX);
        private static readonly CustomStyleProperty<float> s_centerY
            = ConfigNameAPI.AmountProperty(ConfigAmountType.MapCenterY);
        private static readonly CustomStyleProperty<float> s_lineWidth
            = ConfigNameAPI.AmountProperty(ConfigAmountType.MapLineWidth);
        private static readonly CustomStyleProperty<Color> s_lineColor
            = ConfigNameAPI.ColorProperty(ConfigColorType.MapLine);

        private readonly Vector2[] _positions = new Vector2[DimensionCatalog.OUTER_COUNT];

        private Vector2 _center;
        private Color _lineColor;
        private float _lineWidth;

        public Vector2 Center => _center;

        public Color LineColor => _lineColor;

        public float LineWidth => _lineWidth;

        public Vector2 PositionOf(int index)
            => index < DimensionCatalog.OUTER_COUNT ? _positions[index] : _center;

        public void Read(ICustomStyle style)
        {
            for (var i = 0; i < _positions.Length; i++)
            {
                var position = _positions[i];

                style.Read(s_nodeX[i], ref position.x);
                style.Read(s_nodeY[i], ref position.y);

                _positions[i] = position;
            }

            style.Read(s_centerX, ref _center.x);
            style.Read(s_centerY, ref _center.y);
            style.Read(s_lineWidth, ref _lineWidth);
            style.Read(s_lineColor, ref _lineColor);
        }

        private static CustomStyleProperty<float>[] CreateProperties(ConfigAmountType[] names)
        {
            var count = names.Length;
            var properties = new CustomStyleProperty<float>[count];

            for (var i = 0; i < count; i++)
            {
                properties[i] = ConfigNameAPI.AmountProperty(names[i]);
            }

            return properties;
        }
    }
}
