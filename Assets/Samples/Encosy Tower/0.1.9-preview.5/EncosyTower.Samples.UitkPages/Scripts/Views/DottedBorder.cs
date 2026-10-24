using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>An element that draws a dotted rounded border around itself.</summary>
    [UxmlElement]
    public partial class DottedBorder : VisualElement
    {
        private const string CLASS_NAME = "tower-book-dotted-border";

        private Color _color = Color.black;
        private float _width = 1f;
        private float _radius;
        private float _dash = 1f;
        private float _gap = 1f;

        /// <summary>Creates the border. It ignores pointer input.</summary>
        public DottedBorder()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(CLASS_NAME);

            generateVisualContent += OnGenerateVisualContent;
        }

        /// <summary>Gets or sets the color of the dots.</summary>
        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>Gets or sets the stroke width of the border.</summary>
        public float Width
        {
            get => _width;
            set
            {
                if (_width == value)
                {
                    return;
                }

                _width = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>Gets or sets the corner radius of the border.</summary>
        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius == value)
                {
                    return;
                }

                _radius = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>Gets or sets the length of one dot.</summary>
        public float Dash
        {
            get => _dash;
            set
            {
                if (_dash == value)
                {
                    return;
                }

                _dash = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>Gets or sets the length of the gap between two dots.</summary>
        public float Gap
        {
            get => _gap;
            set
            {
                if (_gap == value)
                {
                    return;
                }

                _gap = value;
                MarkDirtyRepaint();
            }
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (_width <= 0f || _dash <= 0f || _color.a <= 0f)
            {
                return;
            }

            var path = new RoundedPath(contentRect, _width, _radius);

            if (path.IsValid == false)
            {
                return;
            }

            var painter = context.painter2D;
            var length = path.Length;

            painter.strokeColor = _color;
            painter.lineWidth = _width;
            painter.lineCap = LineCap.Butt;

            if (_gap <= 0f)
            {
                painter.BeginPath();
                path.Trace(painter, 0f, length);
                painter.ClosePath();
                painter.Stroke();
                return;
            }

            var period = _dash + _gap;

            painter.BeginPath();

            for (var start = 0f; start < length; start += period)
            {
                path.Trace(painter, start, Mathf.Min(start + _dash, length));
            }

            painter.Stroke();
        }

        private readonly struct RoundedPath
        {
            private const int SEGMENT_COUNT = 8;
            private const float QUARTER_TURN = Mathf.PI * 0.5f;
            private const float ODD_WIDTH_TOLERANCE = 0.001f;

            private readonly float _left;
            private readonly float _top;
            private readonly float _width;
            private readonly float _height;
            private readonly float _radius;
            private readonly float _horizontal;
            private readonly float _vertical;
            private readonly bool _snapsToPixels;

            public RoundedPath(Rect rect, float strokeWidth, float radius)
            {
                var half = strokeWidth * 0.5f;
                var limit = Mathf.Min(rect.width, rect.height) * 0.5f;

                _left = rect.x + half;
                _top = rect.y + half;
                _width = rect.width - strokeWidth;
                _height = rect.height - strokeWidth;
                _radius = Mathf.Max(0f, Mathf.Min(radius, limit) - half);
                _horizontal = Mathf.Max(0f, _width - (2f * _radius));
                _vertical = Mathf.Max(0f, _height - (2f * _radius));
                _snapsToPixels = Mathf.Abs((strokeWidth % 2f) - 1f) < ODD_WIDTH_TOLERANCE;
            }

            public bool IsValid => _width > 0f && _height > 0f;

            public float Length => (2f * (_horizontal + _vertical)) + (4f * QUARTER_TURN * _radius);

            public void Trace(Painter2D painter, float from, float to)
            {
                var cursor = 0f;
                var isStarted = false;

                for (var i = 0; i < SEGMENT_COUNT && cursor < to; i++)
                {
                    var length = SegmentLength(i);

                    if (length <= 0f)
                    {
                        continue;
                    }

                    var segmentStart = cursor;

                    cursor += length;

                    if (cursor <= from)
                    {
                        continue;
                    }

                    var start = Mathf.Max(from, segmentStart) - segmentStart;
                    var end = Mathf.Min(to, cursor) - segmentStart;
                    var isStraight = (i & 1) == 0;

                    if (isStraight && _snapsToPixels)
                    {
                        start = Mathf.Floor(start + 0.5f);
                        end = Mathf.Floor(end + 0.5f);

                        if (end <= start)
                        {
                            continue;
                        }
                    }

                    if (isStarted == false)
                    {
                        painter.MoveTo(PointAt(i, start));
                        isStarted = true;
                    }

                    if (isStraight)
                    {
                        painter.LineTo(PointAt(i, end));
                        continue;
                    }

                    var startDegrees = ArcStartDegrees(i);
                    var degreesPerUnit = Mathf.Rad2Deg / _radius;

                    painter.Arc(
                          CenterOf(i)
                        , _radius
                        , Angle.Degrees(startDegrees + (start * degreesPerUnit))
                        , Angle.Degrees(startDegrees + (end * degreesPerUnit))
                        , ArcDirection.Clockwise
                    );
                }
            }

            private static float ArcStartDegrees(int segment)
                => (270f + (90f * (segment / 2))) % 360f;

            private float SegmentLength(int segment)
            {
                if ((segment & 1) == 1)
                {
                    return QUARTER_TURN * _radius;
                }

                return (segment & 2) == 0 ? _horizontal : _vertical;
            }

            private Vector2 CenterOf(int segment)
                => segment switch {
                    1 => new Vector2(_left + _width - _radius, _top + _radius),
                    3 => new Vector2(_left + _width - _radius, _top + _height - _radius),
                    5 => new Vector2(_left + _radius, _top + _height - _radius),
                    _ => new Vector2(_left + _radius, _top + _radius),
                };

            private Vector2 PointAt(int segment, float distance)
            {
                if ((segment & 1) == 1)
                {
                    var radians = (ArcStartDegrees(segment) * Mathf.Deg2Rad) + (distance / _radius);
                    var center = CenterOf(segment);

                    return new Vector2(
                          center.x + (_radius * Mathf.Cos(radians))
                        , center.y + (_radius * Mathf.Sin(radians))
                    );
                }

                return segment switch {
                    0 => new Vector2(_left + _radius + distance, _top),
                    2 => new Vector2(_left + _width, _top + _radius + distance),
                    4 => new Vector2(_left + _width - _radius - distance, _top + _height),
                    _ => new Vector2(_left, _top + _height - _radius - distance),
                };
            }
        }
    }
}
