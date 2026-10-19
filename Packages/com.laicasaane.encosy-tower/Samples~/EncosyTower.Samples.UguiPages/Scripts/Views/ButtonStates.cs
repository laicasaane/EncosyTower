using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Views
{

    /// <summary>Gives a button its hover and press look: eased colors, an inset band and a small offset.</summary>
    /// <remarks>
    /// A page writes the normal look to the graphics of the button and the looks of the other states with
    /// <see cref="SetLooks"/>, then calls <see cref="NormalChanged"/>. The look changes only while the button is
    /// interactable. The fade, the press inset and the press offset do not allocate.
    /// </remarks>
    internal sealed class ButtonStates : MonoBehaviour
        , IPointerEnterHandler
        , IPointerExitHandler
        , IPointerDownHandler
        , IPointerUpHandler
    {
        private const string INSET_NAME = "press-inset";

        private static float s_stateSeconds;
        private static float s_pressOffset;

        private Button _button;
        private RectTransform _rect;
        private Image _fill;
        private Image _border;
        private TMP_Text _label;
        private Image _icon;
        private Image _inset;

        private ButtonState _state;
        private ButtonLook _normal;
        private ButtonLook _hover;
        private ButtonLook _press;
        private ButtonLook _current;
        private ButtonLook _from;
        private ButtonLook _target;
        private Color _insetColor;
        private Vector2 _offsetBasePosition;
        private Vector2 _pressedPosition;
        private float _elapsed;
        private bool _hasNormal;
        private bool _hasLooks;
        private bool _isOver;
        private bool _isDown;
        private bool _isAnimating;
        private bool _isOffsetApplied;

        /// <summary>Reads the amounts of the configuration that every button shares.</summary>
        /// <param name="config">The configuration.</param>
        public static void Configure(AppConfig config)
        {
            s_stateSeconds = config.Amount(ConfigAmountType.ButtonStateSeconds);
            s_pressOffset = config.Amount(ConfigAmountType.ButtonPressOffset);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_stateSeconds = 0f;
            s_pressOffset = 0f;
        }

        /// <summary>Stores the hover look, the press look and the color of the press inset.</summary>
        /// <param name="hover">The look while the pointer is over the button.</param>
        /// <param name="press">The look while the button is pressed.</param>
        /// <param name="inset">The color of the band just inside the border while the button is pressed.</param>
        /// <remarks>Call <see cref="NormalChanged"/> after the last color write of the button.</remarks>
        public void SetLooks(in ButtonLook hover, in ButtonLook press, Color inset)
        {
            _hover = hover;
            _press = press;
            _insetColor = inset;
            _hasLooks = true;
        }

        /// <summary>
        /// Reads the normal look from the graphics a page wrote, stops any fade and shows the look of the current state
        /// at once.
        /// </summary>
        public void NormalChanged()
        {
            FindParts();

            var normal = ReadNormal();

            _normal = normal;
            _hasNormal = true;
            _isAnimating = false;
            _current = TargetOf(_state);

            Apply(in _current);
            ApplyInset();
        }

        /// <summary>Stores an explicit normal border even when it equals the current hover or press border.</summary>
        /// <param name="border">The normal border color written by the page.</param>
        public void SetNormalBorder(Color border)
        {
            _normal = new ButtonLook(_normal.Fill, border, _normal.Ink);
            _isAnimating = false;
            _current = TargetOf(_state);

            Apply(in _current);
            ApplyInset();
        }

        /// <summary>Returns the button to its normal state at once.</summary>
        public void ResetState()
        {
            _isOver = false;
            _isDown = false;

            if (_hasNormal == false)
            {
                _state = ButtonState.Normal;
                return;
            }

            SetState(state: ButtonState.Normal, animate: false);
        }

        /// <inheritdoc/>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isOver = true;

            if (CanChange())
            {
                SetState(state: _isDown ? ButtonState.Press : ButtonState.Hover, animate: true);
            }
        }

        /// <inheritdoc/>
        public void OnPointerExit(PointerEventData eventData)
        {
            _isOver = false;

            if (CanChange())
            {
                SetState(state: ButtonState.Normal, animate: true);
            }
        }

        /// <inheritdoc/>
        public void OnPointerDown(PointerEventData eventData)
        {
            _isDown = true;

            if (CanChange())
            {
                SetState(state: ButtonState.Press, animate: true);
            }
        }

        /// <inheritdoc/>
        public void OnPointerUp(PointerEventData eventData)
        {
            _isDown = false;

            if (CanChange())
            {
                SetState(state: _isOver ? ButtonState.Hover : ButtonState.Normal, animate: true);
            }
        }

        private void Update()
        {
            RestoreOffset();

            if (_isAnimating == false)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;

            var fraction = s_stateSeconds > 0f ? Mathf.Clamp01(_elapsed / s_stateSeconds) : 1f;
            var eased = Mathf.SmoothStep(0f, 1f, fraction);

            _current = new ButtonLook(
                  Color.Lerp(_from.Fill, _target.Fill, eased)
                , Color.Lerp(_from.Border, _target.Border, eased)
                , Color.Lerp(_from.Ink, _target.Ink, eased)
            );

            Apply(in _current);

            if (fraction >= 1f)
            {
                _isAnimating = false;
            }
        }

        private void OnDisable()
        {
            UnsubscribeRendering();
            ResetState();
            RestoreOffset();
        }

        private void OnEnable()
        {
            _ = CanvasUpdateRegistry.instance;

            Canvas.preWillRenderCanvases += RestoreOffset;
            Canvas.willRenderCanvases += ApplyOffset;
        }

        private void OnDestroy()
        {
            UnsubscribeRendering();
            RestoreOffset();
        }

        private void UnsubscribeRendering()
        {
            Canvas.preWillRenderCanvases -= RestoreOffset;
            Canvas.willRenderCanvases -= ApplyOffset;
        }

        private bool CanChange()
        {
            return _hasNormal && _hasLooks && _button.IsValid() && _button.IsInteractable();
        }

        private void SetState(ButtonState state, bool animate)
        {
            if (_button.IsInvalid())
            {
                _state = state;
                return;
            }

            var previous = _state;

            _state = state;
            _target = TargetOf(state);

            if (animate && s_stateSeconds > 0f)
            {
                _from = _current;
                _elapsed = 0f;
                _isAnimating = true;
            }
            else
            {
                _isAnimating = false;
                _current = _target;
                Apply(in _current);
            }

            if (previous != state)
            {
                if (state != ButtonState.Press)
                {
                    RestoreOffset();
                }

                ApplyInset();
            }
        }

        private ButtonLook TargetOf(ButtonState state)
        {
            return state switch {
                ButtonState.Hover => _hover,
                ButtonState.Press => _press,
                _ => _normal,
            };
        }

        private void FindParts()
        {
            if (_button.IsInvalid())
            {
                _button = GetComponent<Button>();
                _rect = transform as RectTransform;
            }

            _fill = _button.targetGraphic as Image;
            _label = _button.GetComponentInChildren<TMP_Text>();
            _border = BookView.FindBorder(_button);
            _icon = FindChildImage(BookView.ICON_NAME);
            _inset = FindChildImage(INSET_NAME);
        }

        private Image FindChildImage(string childName)
        {
            var child = transform.Find(childName);

            if (child.IsValid() && child.TryGetComponent<Image>(out var image))
            {
                return image;
            }

            return null;
        }

        private ButtonLook ReadNormal()
        {
            var fill = Pick(_fill, _normal.Fill, _current.Fill);
            var border = Pick(_border, _normal.Border, _current.Border);
            var ink = _label.IsValid()
                ? Pick(_label.color, _normal.Ink, _current.Ink)
                : Pick(_icon, _normal.Ink, _current.Ink);

            return new ButtonLook(fill, border, ink);
        }

        private Color Pick(Graphic graphic, Color normal, Color shown)
        {
            return graphic.IsValid() ? Pick(graphic.color, normal, shown) : normal;
        }

        private Color Pick(Color graphicColor, Color normal, Color shown)
        {
            return _hasNormal && graphicColor == shown ? normal : graphicColor;
        }

        private void Apply(in ButtonLook look)
        {
            if (_fill.IsValid())
            {
                _fill.color = look.Fill;
            }

            if (_border.IsValid())
            {
                _border.color = look.Border;
            }

            if (_label.IsValid())
            {
                _label.color = look.Ink;
            }

            if (_icon.IsValid())
            {
                _icon.color = look.Ink;
            }
        }

        private void ApplyInset()
        {
            if (_inset.IsInvalid())
            {
                return;
            }

            var isPressed = _state == ButtonState.Press;

            var inset = _inset.gameObject;

            _inset.color = _insetColor;

            if (inset.activeSelf != isPressed)
            {
                inset.SetActive(isPressed);
            }
        }

        private void ApplyOffset()
        {
            RestoreOffset();

            if (_rect.IsInvalid() || isActiveAndEnabled == false || CanChange() == false || _state != ButtonState.Press)
            {
                return;
            }

            _offsetBasePosition = _rect.anchoredPosition;

            var position = _offsetBasePosition;

            position.y -= s_pressOffset;

            _rect.anchoredPosition = position;
            _pressedPosition = position;
            _isOffsetApplied = true;
        }

        private void RestoreOffset()
        {
            if (_isOffsetApplied == false)
            {
                return;
            }

            _isOffsetApplied = false;

            if (_rect.IsValid() && _rect.anchoredPosition.Equals(_pressedPosition))
            {
                _rect.anchoredPosition = _offsetBasePosition;
            }
        }

        private enum ButtonState
        {
            Normal,
            Hover,
            Press,
        }
    }
}
