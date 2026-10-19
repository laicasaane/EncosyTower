using System.Collections.Generic;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>
    /// Gives a button its hover and press states: the press inset, the press offset and the color transitions.
    /// </summary>
    /// <remarks>
    /// Two modes. In class mode the colors come from the USS <c>:hover</c> and <c>:active</c> rules, and this
    /// manipulator only runs the inset, the offset and the transitions. In inline mode a page gave the looks with
    /// <see cref="SetLooks"/>, and this manipulator writes the inline background, border and label colors.
    /// The amounts are read when used: from the <see cref="PaletteStyle"/> when there is one, otherwise from the
    /// button's resolved USS custom properties. Call on the Unity main thread.
    /// </remarks>
    internal sealed class ButtonStates : Manipulator
    {
        private const string INSET_CLASS = "tower-book-press-inset";
        private const string INSET_SHOWN_CLASS = "tower-book-press-inset--shown";
        private const float MILLISECONDS_PER_SECOND = 1000f;
        private const int PRIMARY_BUTTON = 0;
        private const int PROPERTY_COUNT = 6;

        private static readonly CustomStyleProperty<float> s_stateSecondsProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.ButtonStateSeconds);

        private static readonly CustomStyleProperty<float> s_snapSecondsProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.ButtonSnapSeconds);

        private static readonly CustomStyleProperty<float> s_pressInsetProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.ButtonPressInset);

        private static readonly CustomStyleProperty<float> s_pressOffsetProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.ButtonPressOffset);

        private static readonly StyleList<StylePropertyName> s_properties = CreateProperties();
        private static readonly StyleList<EasingFunction> s_easings = CreateEasings();
        private static readonly StyleList<TimeValue> s_zeroDurations = new(CreateDurations(0f));

        private readonly PaletteStyle _palette;

        private StyleList<TimeValue> _stateDurations;
        private VisualElement _inset;
        private IVisualElementScheduledItem _snapItem;
        private ButtonLook _normal;
        private ButtonLook _hover;
        private ButtonLook _press;
        private Color _insetColor;
        private StyleLength _originalMinWidth;
        private TextMetrics _textMetrics;
        private float _authoredMinWidth;
        private float _textMinimum;
        private float _durationSeconds;
        private float _stateSeconds;
        private float _snapSeconds;
        private float _pressInset;
        private float _pressOffset;
        private bool _hasDurations;
        private bool _isAmountsRead;
        private bool _isInline;
        private bool _isNormalFresh;
        private bool _isHovered;
        private bool _isPressed;
        private bool _isSnapping;
        private bool _isOffset;
        private bool _hasAuthoredMinWidth;
        private bool _hasTextMetrics;
        private bool _ownsMinWidth;

        /// <summary>Creates the manipulator in class mode.</summary>
        /// <param name="palette">
        /// The page's palette that holds the amounts, or <c>null</c> to read them from the button's resolved USS
        /// custom properties.
        /// </param>
        public ButtonStates(PaletteStyle palette)
        {
            _palette = palette;
        }

        private float StateSeconds
        {
            get
            {
                if (_palette != null)
                {
                    return _palette.Amount(ConfigAmountType.ButtonStateSeconds);
                }

                EnsureAmounts();
                return _stateSeconds;
            }
        }

        private float SnapSeconds
        {
            get
            {
                if (_palette != null)
                {
                    return _palette.Amount(ConfigAmountType.ButtonSnapSeconds);
                }

                EnsureAmounts();
                return _snapSeconds;
            }
        }

        private float PressInset
        {
            get
            {
                if (_palette != null)
                {
                    return _palette.Amount(ConfigAmountType.ButtonPressInset);
                }

                EnsureAmounts();
                return _pressInset;
            }
        }

        private float PressOffset
        {
            get
            {
                if (_palette != null)
                {
                    return _palette.Amount(ConfigAmountType.ButtonPressOffset);
                }

                EnsureAmounts();
                return _pressOffset;
            }
        }

        private bool IsInteractable => target != null && target.enabledInHierarchy;

        /// <summary>
        /// Writes the normal look and keeps it as the look to return to. In inline mode it shows the look of the
        /// current state. Either way the colors snap: no transition runs for them.
        /// </summary>
        /// <param name="look">The normal look.</param>
        public void SetNormal(in ButtonLook look)
        {
            _normal = look;
            _isNormalFresh = true;

            Snap();

            if (_isInline)
            {
                Refresh();
            }
        }

        /// <summary>
        /// Keeps a new border color in the normal look; the button's inline borders are already written.
        /// Does nothing to the looks when no normal look is kept and the manipulator is in class mode.
        /// </summary>
        /// <param name="border">The border color of the normal look.</param>
        public void SetNormalBorder(Color border)
        {
            Snap();

            if (_isNormalFresh == false && _isInline == false)
            {
                return;
            }

            _normal = new ButtonLook(_normal.Fill, border, _normal.Ink);
            _isNormalFresh = true;

            if (_isInline)
            {
                Refresh();
            }
        }

        /// <summary>
        /// Switches to inline mode with the looks a page gave. The normal look is the one <see cref="SetNormal"/> just
        /// kept, or else the button's current inline colors, resolved colors for the ones not set.
        /// </summary>
        /// <param name="hover">The look while the pointer is over the button.</param>
        /// <param name="press">The look while the button is pressed.</param>
        /// <param name="inset">The color of the inset shown while the button is pressed.</param>
        public void SetLooks(in ButtonLook hover, in ButtonLook press, Color inset)
        {
            if (target == null)
            {
                return;
            }

            if (_isNormalFresh == false)
            {
                _normal = ReadNormal();
            }

            _isNormalFresh = false;
            _hover = hover;
            _press = press;
            _insetColor = inset;
            _isInline = true;

            Snap();
            Refresh();
        }

        /// <summary>
        /// Goes back to class mode: the normal look is written once more and the USS rules own the states again.
        /// </summary>
        public void ClearLooks()
        {
            if (_isInline == false)
            {
                return;
            }

            _isNormalFresh = false;

            Snap();
            ApplyLook(in _normal);

            _isInline = false;

            Refresh();
        }

        /// <summary>
        /// Drops the hover and press state at once: the normal look, no inset, no offset, and the colors snap.
        /// </summary>
        public void ResetState()
        {
            _isHovered = false;
            _isPressed = false;

            if (target == null)
            {
                return;
            }

            Snap();

            if (_isInline)
            {
                ApplyLook(in _normal);
            }

            SetPressed(false);
        }

        /// <summary>
        /// Suspends the color transitions for the time of <c>amount.button.snap-seconds</c>, so colors written now
        /// apply at once.
        /// </summary>
        public void Snap()
        {
            if (target == null)
            {
                return;
            }

            RefreshTextMinimum();

            _isSnapping = true;
            target.style.transitionDuration = s_zeroDurations;

            _snapItem ??= target.schedule.Execute(RestoreTransitions);
            _snapItem.ExecuteLater((long)(SnapSeconds * MILLISECONDS_PER_SECOND));
        }

        /// <inheritdoc/>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            target.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            target.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            target.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            _originalMinWidth = target.style.minWidth;
            _hasAuthoredMinWidth = false;
            _hasTextMetrics = false;

            RefreshTextMinimum();

            _inset = new VisualElement { pickingMode = PickingMode.Ignore, focusable = false };
            _inset.AddToClassList(INSET_CLASS);

            target.Add(_inset);

            ApplyTransitions();
        }

        /// <inheritdoc/>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            target.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            target.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            target.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            RestoreTextMinimum();

            _snapItem?.Pause();
            _snapItem = null;

            _inset?.RemoveFromHierarchy();
            _inset = null;
        }

        private static StyleList<StylePropertyName> CreateProperties()
        {
            return new(new List<StylePropertyName>(PROPERTY_COUNT) {
                new("background-color"),
                new("border-top-color"),
                new("border-right-color"),
                new("border-bottom-color"),
                new("border-left-color"),
                new("color"),
            });
        }

        private static StyleList<EasingFunction> CreateEasings()
        {
            var easings = new List<EasingFunction>(PROPERTY_COUNT);

            for (var i = 0; i < PROPERTY_COUNT; i++)
            {
                easings.Add(new EasingFunction(EasingMode.EaseInOut));
            }

            return new(easings);
        }

        private static List<TimeValue> CreateDurations(float seconds)
        {
            var durations = new List<TimeValue>(PROPERTY_COUNT);

            for (var i = 0; i < PROPERTY_COUNT; i++)
            {
                durations.Add(new TimeValue(seconds, TimeUnit.Second));
            }

            return durations;
        }

        private static Color ColorOf(StyleColor inline, Color resolved)
            => inline.keyword == StyleKeyword.Undefined ? inline.value : resolved;

        private ButtonLook ReadNormal()
        {
            var style = target.style;
            var resolved = target.resolvedStyle;

            return new ButtonLook(
                  ColorOf(style.backgroundColor, resolved.backgroundColor)
                , ColorOf(style.borderTopColor, resolved.borderTopColor)
                , ColorOf(style.color, resolved.color)
            );
        }

        private void EnsureAmounts()
        {
            if (_isAmountsRead || target == null)
            {
                return;
            }

            var style = target.customStyle;

            _isAmountsRead = style.TryGetValue(s_stateSecondsProperty, out _stateSeconds)
                && style.TryGetValue(s_snapSecondsProperty, out _snapSeconds)
                && style.TryGetValue(s_pressInsetProperty, out _pressInset)
                && style.TryGetValue(s_pressOffsetProperty, out _pressOffset);
        }

        private void ApplyTransitions()
        {
            var seconds = StateSeconds;

            if (_hasDurations == false || seconds != _durationSeconds)
            {
                _durationSeconds = seconds;
                _hasDurations = true;
                _stateDurations = new StyleList<TimeValue>(CreateDurations(seconds));
            }

            var style = target.style;

            style.transitionProperty = s_properties;
            style.transitionTimingFunction = s_easings;
            style.transitionDuration = _isSnapping ? s_zeroDurations : _stateDurations;
        }

        private void RestoreTransitions()
        {
            _isSnapping = false;

            if (target != null)
            {
                RefreshTextMinimum();
                ApplyTransitions();
            }
        }

        private void RefreshTextMinimum()
        {
            if (target is not Button button || button.panel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(button.text))
            {
                RestoreTextMinimum();
                _hasTextMetrics = false;
                return;
            }

            var resolved = button.resolvedStyle;

            if (resolved.fontSize <= 0f || float.IsNaN(resolved.width) || resolved.width <= 0f)
            {
                return;
            }

            var horizontal = resolved.paddingLeft + resolved.paddingRight
                + resolved.borderLeftWidth + resolved.borderRightWidth;

            var metrics = new TextMetrics(
                  button.text
                , resolved.unityFont
                , resolved.unityFontDefinition
                , resolved.unityFontStyleAndWeight
                , resolved.fontSize
                , resolved.letterSpacing
                , resolved.wordSpacing
                , resolved.unityParagraphSpacing
                , horizontal
            );

            if (_hasTextMetrics && _textMetrics == metrics)
            {
                return;
            }

            var measured = button.MeasureTextSize(
                  textToMeasure: button.text
                , width: 0f
                , widthMode: VisualElement.MeasureMode.Undefined
                , height: 0f
                , heightMode: VisualElement.MeasureMode.Undefined
            ).x;

            if (float.IsNaN(measured) || float.IsInfinity(measured) || measured <= 0f)
            {
                return;
            }

            if (_hasAuthoredMinWidth == false)
            {
                var minimum = resolved.minWidth.value;

                if (float.IsNaN(minimum) || float.IsInfinity(minimum) || minimum < 0f)
                {
                    return;
                }

                _authoredMinWidth = minimum;
                _hasAuthoredMinWidth = true;
            }

            _textMetrics = metrics;
            _hasTextMetrics = true;

            var minimumWidth = Mathf.Max(_authoredMinWidth, Mathf.Ceil(measured + horizontal));

            if (_ownsMinWidth == false || _textMinimum != minimumWidth)
            {
                _textMinimum = minimumWidth;
                _ownsMinWidth = true;
                button.style.minWidth = minimumWidth;
            }
        }

        private void RestoreTextMinimum()
        {
            if (_ownsMinWidth == false)
            {
                return;
            }

            if (target.style.minWidth == new StyleLength(_textMinimum))
            {
                target.style.minWidth = _originalMinWidth;
            }

            _ownsMinWidth = false;
        }

        private void Refresh()
        {
            var isInteractable = IsInteractable;
            var isPressed = _isPressed && _isHovered && isInteractable;

            SetPressed(isPressed);

            if (_isInline == false)
            {
                return;
            }

            if (isPressed)
            {
                ApplyLook(in _press);
            }
            else if (_isHovered && isInteractable)
            {
                ApplyLook(in _hover);
            }
            else
            {
                ApplyLook(in _normal);
            }
        }

        private void ApplyLook(in ButtonLook look)
        {
            var style = target.style;

            style.backgroundColor = look.Fill;
            style.borderTopColor = look.Border;
            style.borderRightColor = look.Border;
            style.borderBottomColor = look.Border;
            style.borderLeftColor = look.Border;
            style.color = look.Ink;
        }

        private void SetPressed(bool isPressed)
        {
            SetInset(isPressed);
            SetOffset(isPressed);
        }

        private void SetInset(bool isShown)
        {
            if (_inset == null)
            {
                return;
            }

            if (isShown == false)
            {
                _inset.EnableInClassList(INSET_SHOWN_CLASS, false);
                return;
            }

            if (_inset.parent != target)
            {
                target.Add(_inset);
            }

            var style = _inset.style;
            var width = PressInset;

            style.borderTopWidth = width;
            style.borderRightWidth = width;
            style.borderBottomWidth = width;
            style.borderLeftWidth = width;

            if (_isInline)
            {
                style.borderTopColor = _insetColor;
                style.borderRightColor = _insetColor;
                style.borderBottomColor = _insetColor;
                style.borderLeftColor = _insetColor;
            }
            else
            {
                style.borderTopColor = StyleKeyword.Null;
                style.borderRightColor = StyleKeyword.Null;
                style.borderBottomColor = StyleKeyword.Null;
                style.borderLeftColor = StyleKeyword.Null;
            }

            _inset.EnableInClassList(INSET_SHOWN_CLASS, true);
        }

        private void SetOffset(bool isShown)
        {
            if (isShown)
            {
                _isOffset = true;
                target.style.translate = new Translate(
                      new Length(0f, LengthUnit.Pixel)
                    , new Length(PressOffset, LengthUnit.Pixel)
                );
            }
            else if (_isOffset)
            {
                _isOffset = false;
                target.style.translate = StyleKeyword.Null;
            }
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            _isHovered = true;

            ApplyTransitions();
            Refresh();
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            _isHovered = false;

            Refresh();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != PRIMARY_BUTTON || IsInteractable == false)
            {
                return;
            }

            _isHovered = true;
            _isPressed = true;

            ApplyTransitions();
            Refresh();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            _isPressed = false;

            if (evt.pointerType != UnityEngine.UIElements.PointerType.mouse)
            {
                _isHovered = false;
            }

            Refresh();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _isPressed = false;

            Refresh();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            _isHovered = false;
            _isPressed = false;

            Refresh();
            RestoreTextMinimum();
            _hasTextMetrics = false;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            RefreshTextMinimum();
            Snap();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshTextMinimum();
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (_palette == null)
            {
                var style = evt.customStyle;

                style.Read(s_stateSecondsProperty, ref _stateSeconds);
                style.Read(s_snapSecondsProperty, ref _snapSeconds);
                style.Read(s_pressInsetProperty, ref _pressInset);
                style.Read(s_pressOffsetProperty, ref _pressOffset);

                _isAmountsRead = true;
            }

            RefreshTextMinimum();
            ApplyTransitions();
        }

        private readonly record struct TextMetrics(
              string Text
            , Font Font
            , FontDefinition FontDefinition
            , FontStyle FontStyle
            , float FontSize
            , float LetterSpacing
            , float WordSpacing
            , float ParagraphSpacing
            , float Horizontal
        );
    }
}
