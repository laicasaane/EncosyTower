using EncosyTower.Samples.UitkPages.Config;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>An element that draws the lines between the nodes of the map.</summary>
    /// <remarks>It reads the node positions and the line style from its USS custom properties.</remarks>
    [UxmlElement]
    public partial class MapLines : VisualElement
    {
        private readonly MapStyle _map = new();

        /// <summary>Creates the element. It ignores pointer input.</summary>
        public MapLines()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private static Vector2 Scale(Vector2 percent, Rect rect)
            => new(percent.x * rect.width / 100f, percent.y * rect.height / 100f);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _map.Read(evt.customStyle);
            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            var rect = contentRect;
            var count = DimensionCatalog.OUTER_COUNT;
            var center = Scale(_map.Center, rect);

            painter.strokeColor = _map.LineColor;
            painter.lineWidth = _map.LineWidth;

            for (var i = 0; i < count; i++)
            {
                painter.BeginPath();
                painter.MoveTo(center);
                painter.LineTo(Scale(_map.PositionOf(i), rect));
                painter.Stroke();
            }

            painter.BeginPath();
            painter.MoveTo(Scale(_map.PositionOf(0), rect));

            for (var i = 1; i < count; i++)
            {
                painter.LineTo(Scale(_map.PositionOf(i), rect));
            }

            painter.ClosePath();
            painter.Stroke();
        }
    }
}
