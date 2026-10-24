using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>
    /// The four-corner mark that shows which button has the keyboard or gamepad focus.
    /// </summary>
    /// <remarks>
    /// One decoration above the app's pages, tracking the focused button. It shows only in keyboard mode, which a
    /// keyboard or navigation input turns on and a pointer press turns off. Its color is whichever of the two
    /// configured colors has the higher contrast against the surface behind the button. Call on the Unity main
    /// thread.
    /// </remarks>
    internal sealed class FocusCorners
    {
        private const string DECORATION_CLASS = "tower-book-focus-corners";
        private const float OPAQUE_ALPHA = 0.99f;
        private const float LUMINANCE_RED = 0.2126f;
        private const float LUMINANCE_GREEN = 0.7152f;
        private const float LUMINANCE_BLUE = 0.0722f;
        private const float LINEAR_THRESHOLD = 0.04045f;
        private const float LINEAR_DIVISOR = 12.92f;
        private const float GAMMA_OFFSET = 0.055f;
        private const float GAMMA_DIVISOR = 1.055f;
        private const float GAMMA_EXPONENT = 2.4f;
        private const float CONTRAST_OFFSET = 0.05f;

        private static readonly CustomStyleProperty<Color> s_onLightProperty
            = ConfigNameAPI.ColorProperty(ConfigColorType.FocusOnLight);

        private static readonly CustomStyleProperty<Color> s_onDarkProperty
            = ConfigNameAPI.ColorProperty(ConfigColorType.FocusOnDark);

        private static readonly CustomStyleProperty<Color> s_fallbackProperty
            = ConfigNameAPI.ColorProperty(ConfigColorType.PageMainMenu);

        private static readonly CustomStyleProperty<float> s_paddingProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.FocusPadding);

        private static readonly CustomStyleProperty<float> s_strokeProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.FocusStroke);

        private readonly VisualElement _decoration;

        private VisualElement _root;
        private Button _focused;
        private Color _onLight;
        private Color _onDark;
        private Color _fallback;
        private float _padding;
        private float _stroke;
        private bool _isKeyboardMode;
        private bool _isStyleRead;

        /// <summary>Creates the mark, not yet shown on any button.</summary>
        public FocusCorners()
        {
            _decoration = new VisualElement { pickingMode = PickingMode.Ignore, focusable = false };
            _decoration.AddToClassList(DECORATION_CLASS);
            _decoration.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary>Gets whether keyboard mode is on: the mark shows on the focused button.</summary>
        public bool IsKeyboardMode => _isKeyboardMode;

        /// <summary>Starts to follow the focus of every button under a root element.</summary>
        /// <param name="root">The app root above the sample's flows; any earlier root is released first.</param>
        public void Bind(VisualElement root)
        {
            Unbind();

            _root = root;

            if (_root == null)
            {
                return;
            }

            _root.RegisterCallback<FocusInEvent>(OnFocusIn, TrickleDown.TrickleDown);
            _root.RegisterCallback<FocusOutEvent>(OnFocusOut, TrickleDown.TrickleDown);
            _root.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            if (_isKeyboardMode)
            {
                ShowOn(FocusedButton());
            }
        }

        /// <summary>Stops following the focus and takes the mark off its button.</summary>
        public void Unbind()
        {
            if (_root == null)
            {
                return;
            }

            _root.UnregisterCallback<FocusInEvent>(OnFocusIn, TrickleDown.TrickleDown);
            _root.UnregisterCallback<FocusOutEvent>(OnFocusOut, TrickleDown.TrickleDown);
            _root.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            _root = null;
            _isStyleRead = false;

            Hide();
        }

        /// <summary>
        /// Turns keyboard mode on or off. Turning it on shows the mark on the button that has the focus now;
        /// turning it off takes the mark away.
        /// </summary>
        /// <param name="isOn">
        /// <c>true</c> after a keyboard or navigation input; <c>false</c> after a pointer press.
        /// </param>
        public void SetKeyboardMode(bool isOn)
        {
            if (_isKeyboardMode == isOn)
            {
                return;
            }

            _isKeyboardMode = isOn;

            if (isOn)
            {
                ShowOn(FocusedButton());
            }
            else
            {
                Hide();
            }
        }

        /// <summary>Picks the mark's color again, after the theme changed.</summary>
        public void Repick()
        {
            _isStyleRead = false;

            Apply();
        }

        /// <summary>Tracks the focused border box and its page opacity after the app has rendered.</summary>
        public void Tick()
        {
            if (_isKeyboardMode == false)
            {
                return;
            }

            var focused = FocusedButton();

            if (focused != _focused)
            {
                ShowOn(focused);
            }
            else
            {
                Apply();
            }
        }

        private static float Luminance(Color color)
            => (LUMINANCE_RED * Linear(color.r))
            + (LUMINANCE_GREEN * Linear(color.g))
            + (LUMINANCE_BLUE * Linear(color.b));

        private static float Linear(float channel)
            => channel <= LINEAR_THRESHOLD
                ? channel / LINEAR_DIVISOR
                : Mathf.Pow((channel + GAMMA_OFFSET) / GAMMA_DIVISOR, GAMMA_EXPONENT);

        private static float Contrast(float first, float second)
            => first > second
                ? (first + CONTRAST_OFFSET) / (second + CONTRAST_OFFSET)
                : (second + CONTRAST_OFFSET) / (first + CONTRAST_OFFSET);

        private Button FocusedButton()
        {
            var controller = _root?.panel?.focusController;

            var button = controller?.focusedElement as Button;

            return button != null && _root.Contains(button) ? button : null;
        }

        private void ShowOn(Button button)
        {
            if (button == null)
            {
                Hide();
                return;
            }

            _focused = button;
            _isStyleRead = false;

            Apply();
        }

        private void Hide()
        {
            _focused = null;
            _decoration.RemoveFromHierarchy();
        }

        private void Apply()
        {
            if (_focused == null || _root?.panel == null || _focused.panel != _root.panel)
            {
                _decoration.RemoveFromHierarchy();
                return;
            }

            var opacity = 1f;
            var node = (VisualElement)_focused;

            for (; node != null && node != _root; node = node.parent)
            {
                var resolved = node.resolvedStyle;

                if (node.enabledInHierarchy == false
                    || resolved.visibility != Visibility.Visible
                    || resolved.display == DisplayStyle.None
                )
                {
                    _decoration.RemoveFromHierarchy();
                    return;
                }

                opacity *= resolved.opacity;
            }

            if (node != _root)
            {
                _decoration.RemoveFromHierarchy();
                return;
            }

            if (_isStyleRead == false)
            {
                _isStyleRead = TryReadStyle(_focused.customStyle) || TryReadStyle(_root.customStyle);
            }

            if (_isStyleRead == false)
            {
                _decoration.RemoveFromHierarchy();
                return;
            }

            var borderBox = new Rect(Vector2.zero, _focused.layout.size);

            if (float.IsNaN(borderBox.width)
                || float.IsNaN(borderBox.height)
                || borderBox.width <= 0f
                || borderBox.height <= 0f
            )
            {
                _decoration.RemoveFromHierarchy();
                return;
            }

            if (_decoration.parent != _root)
            {
                _root.Add(_decoration);
            }

            if (_root[_root.childCount - 1] != _decoration)
            {
                _decoration.BringToFront();
            }

            var offset = _padding + _stroke;
            var bounds = _focused.ChangeCoordinatesTo(_root, borderBox);
            var rootStyle = _root.resolvedStyle;
            var style = _decoration.style;

            style.left = bounds.xMin - rootStyle.borderLeftWidth - offset;
            style.top = bounds.yMin - rootStyle.borderTopWidth - offset;
            style.width = bounds.width + offset + offset;
            style.height = bounds.height + offset + offset;
            style.opacity = opacity;
            style.unityBackgroundImageTintColor = Pick(SurfaceBehind(_focused));
        }

        private Color SurfaceBehind(VisualElement element)
        {
            for (var ancestor = element.parent; ancestor != null; ancestor = ancestor.parent)
            {
                var fill = ancestor.resolvedStyle.backgroundColor;

                if (fill.a >= OPAQUE_ALPHA)
                {
                    return fill;
                }
            }

            return _fallback;
        }

        private Color Pick(Color surface)
        {
            var surfaceLuminance = Luminance(surface);
            var onLight = Contrast(Luminance(_onLight), surfaceLuminance);
            var onDark = Contrast(Luminance(_onDark), surfaceLuminance);

            return onLight >= onDark ? _onLight : _onDark;
        }

        private bool TryReadStyle(ICustomStyle style)
        {
            return style.TryGetValue(s_onLightProperty, out _onLight)
                && style.TryGetValue(s_onDarkProperty, out _onDark)
                && style.TryGetValue(s_fallbackProperty, out _fallback)
                && style.TryGetValue(s_paddingProperty, out _padding)
                && style.TryGetValue(s_strokeProperty, out _stroke);
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if (_isKeyboardMode)
            {
                ShowOn(evt.target as Button);
            }
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            if (evt.target == _focused)
            {
                Hide();
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _isStyleRead = TryReadStyle(evt.customStyle);

            Apply();
        }
    }
}
