using System;
using System.Collections.Generic;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Views
{
    /// <summary>
    /// Shows four corner marks around the control that keyboard or gamepad navigation has selected, and keeps the
    /// selection inside the top page of the app.
    /// </summary>
    /// <remarks>
    /// The marks are the child named <c>focus-corners</c> of the selected control. They show only in keyboard mode,
    /// which a key or gamepad input turns on and a pointer press turns off. The mark color is the configured color
    /// with the higher contrast against the surface behind the control. It does not allocate after the first use.
    /// </remarks>
    internal sealed class FocusCorners
    {
        private const string CORNERS_NAME = "focus-corners";
        private const float OPAQUE_ALPHA = 0.99f;
        private const float SRGB_LINEAR_LIMIT = 0.04045f;
        private const float SRGB_LINEAR_DIVISOR = 12.92f;
        private const float SRGB_OFFSET = 0.055f;
        private const float SRGB_SCALE = 1.055f;
        private const float SRGB_EXPONENT = 2.4f;
        private const float LUMINANCE_RED = 0.2126f;
        private const float LUMINANCE_GREEN = 0.7152f;
        private const float LUMINANCE_BLUE = 0.0722f;
        private const float CONTRAST_OFFSET = 0.05f;

        private static readonly HierarchyComparer s_comparer = new();

        private readonly AppConfig _config;
        private readonly List<Selectable> _group = new();
        private readonly List<Canvas> _canvases = new();
        private readonly List<Canvas> _focusCanvases = new();

        private Selectable[] _buffer = Array.Empty<Selectable>();
        private Selectable _shown;
        private RectTransform _shownCorners;
        private Image _shownImage;
        private Canvas _shownCanvas;
        private Transform _canvasRoot;
        private int _popupSortingOrder;
        private bool _isKeyboardMode;
        private bool _isRecolorPending;

        /// <summary>Initializes the marks with the configuration that holds their colors and sizes.</summary>
        /// <param name="config">The configuration.</param>
        public FocusCorners(AppConfig config)
        {
            _config = config;
        }

        /// <summary>Sets the sorting order of the popup flow; flows above it take the focus last.</summary>
        /// <param name="popupSortingOrder">The sorting order of the canvas of the popup flow.</param>
        public void Initialize(int popupSortingOrder)
        {
            _popupSortingOrder = popupSortingOrder;
            _isKeyboardMode = false;

            Hide();
        }

        /// <summary>Turns the keyboard mode on or off; the marks show only in keyboard mode.</summary>
        /// <param name="enabled">Whether the keyboard mode is on.</param>
        public void SetKeyboardMode(bool enabled)
        {
            _isKeyboardMode = enabled;

            if (enabled == false)
            {
                Hide();
            }
        }

        /// <summary>Asks the marks to pick their color again on the next <see cref="Tick"/>.</summary>
        public void ThemeChanged()
        {
            _isRecolorPending = true;
        }

        /// <summary>Selects the next or the previous control of the top page, and wraps around its ends.</summary>
        /// <param name="backwards">Whether to select the previous control instead of the next one.</param>
        public void MoveNext(bool backwards)
        {
            var eventSystem = EventSystem.current;

            if (eventSystem.IsInvalid())
            {
                return;
            }

            var count = FillBuffer();

            if (TryFindTopPage(count: count, sortingOrder: out _, page: out var page) == false)
            {
                return;
            }

            BuildGroup(count, page);

            var length = _group.Count;

            if (length == 0)
            {
                return;
            }

            var index = -1;
            var selected = eventSystem.currentSelectedGameObject;

            if (selected.IsValid() && selected.TryGetComponent<Selectable>(out var current))
            {
                index = _group.IndexOf(current);
            }

            int next;

            if (index < 0)
            {
                next = backwards ? length - 1 : 0;
            }
            else
            {
                next = backwards ? index - 1 : index + 1;
            }

            if (next < 0)
            {
                next = length - 1;
            }
            else if (next >= length)
            {
                next = 0;
            }

            eventSystem.SetSelectedGameObject(_group[next].gameObject);
        }

        /// <summary>
        /// Keeps the selection inside the top page and shows the marks on the selected control. Call it once per frame
        /// after the pages have rendered.
        /// </summary>
        /// <param name="eventSystem">The event system that holds the selection.</param>
        public void Tick(EventSystem eventSystem)
        {
            if (_isKeyboardMode == false || eventSystem.IsInvalid())
            {
                Hide();
                return;
            }

            var current = EnsureSelection(eventSystem);

            if (current.IsInvalid())
            {
                Hide();
                return;
            }

            if (ReferenceEquals(current, _shown) == false)
            {
                Show(current);
            }
            else if (_isRecolorPending)
            {
                Recolor();
            }

            _isRecolorPending = false;

            UpdateSorting();
        }

        private Selectable EnsureSelection(EventSystem eventSystem)
        {
            var count = FillBuffer();

            if (TryFindTopPage(count: count, sortingOrder: out _, page: out var page) == false)
            {
                return null;
            }

            var selected = eventSystem.currentSelectedGameObject;

            if (selected.IsValid()
                && selected.TryGetComponent<Selectable>(out var current)
                && IsCandidate(current)
                && TryGetPage(selectable: current, sortingOrder: out _, page: out var currentPage)
                && currentPage == page
            )
            {
                return current;
            }

            BuildGroup(count, page);

            if (_group.Count == 0)
            {
                return null;
            }

            var first = _group[0];

            eventSystem.SetSelectedGameObject(first.gameObject);
            return first;
        }

        private int FillBuffer()
        {
            var count = Selectable.allSelectableCount;

            if (_buffer.Length < count)
            {
                Array.Resize(ref _buffer, count);
            }

            return Selectable.AllSelectablesNoAlloc(_buffer);
        }

        private bool TryFindTopPage(int count, out int sortingOrder, out Transform page)
        {
            var found = false;

            sortingOrder = default;
            page = null;

            for (var i = 0; i < count; i++)
            {
                var candidate = _buffer[i];

                if (IsCandidate(candidate) == false
                    || TryGetPage(candidate, out var order, out var candidatePage) == false
                )
                {
                    continue;
                }

                if (found == false || IsBetter(order, candidatePage, sortingOrder, page))
                {
                    found = true;
                    sortingOrder = order;
                    page = candidatePage;
                }
            }

            return found;
        }

        private void BuildGroup(int count, Transform page)
        {
            _group.Clear();

            for (var i = 0; i < count; i++)
            {
                var candidate = _buffer[i];

                if (IsCandidate(candidate)
                    && TryGetPage(selectable: candidate, sortingOrder: out _, page: out var candidatePage)
                    && candidatePage == page
                )
                {
                    _group.Add(candidate);
                }
            }

            _group.Sort(s_comparer);
        }

        private static bool IsCandidate(Selectable selectable)
        {
            return selectable.IsValid() && selectable.isActiveAndEnabled && selectable.IsInteractable();
        }

        private bool IsBetter(int order, Transform page, int bestOrder, Transform bestPage)
        {
            var qualifies = order <= _popupSortingOrder;
            var bestQualifies = bestOrder <= _popupSortingOrder;

            if (qualifies != bestQualifies)
            {
                return qualifies;
            }

            if (order != bestOrder)
            {
                return qualifies ? order > bestOrder : order < bestOrder;
            }

            return page.GetSiblingIndex() > bestPage.GetSiblingIndex();
        }

        private static bool TryGetPage(Selectable selectable, out int sortingOrder, out Transform page)
        {
            var node = selectable.transform;
            Transform child = null;

            while (node.IsValid())
            {
                if (node.TryGetComponent<Canvas>(out var canvas) && canvas.overrideSorting)
                {
                    sortingOrder = canvas.sortingOrder;
                    page = child;
                    return child.IsValid();
                }

                child = node;
                node = node.parent;
            }

            sortingOrder = default;
            page = null;
            return false;
        }

        private void Show(Selectable selectable)
        {
            Hide();

            _shown = selectable;

            var corners = selectable.transform.Find(CORNERS_NAME);

            if (corners.IsInvalid() || corners.TryGetComponent<Image>(out var image) == false)
            {
                return;
            }

            _shownCorners = corners as RectTransform;
            _shownImage = image;

            if (_shownCorners.IsInvalid())
            {
                return;
            }

            var inset = _config.Amount(ConfigAmountType.FocusPadding) + _config.Amount(ConfigAmountType.FocusStroke);

            _shownCorners.offsetMin = new Vector2(-inset, -inset);
            _shownCorners.offsetMax = new Vector2(inset, inset);

            _shownImage.maskable = false;
            _shownImage.raycastTarget = false;
            _shownCorners.SetAsLastSibling();
            _shownCorners.gameObject.SetActive(true);

            if (_shownCorners.TryGetComponent<Canvas>(out _shownCanvas) == false)
            {
                _shownCanvas = _shownCorners.gameObject.AddComponent<Canvas>();
            }

            if (_focusCanvases.Contains(_shownCanvas) == false)
            {
                _focusCanvases.Add(_shownCanvas);
            }

            _shownCanvas.overrideSorting = true;
            _canvasRoot = null;

            for (var node = selectable.transform; node.IsValid(); node = node.parent)
            {
                if (node.TryGetComponent<Canvas>(out _))
                {
                    _canvasRoot = node;
                }
            }

            Recolor();
            UpdateSorting();
        }

        private void Hide()
        {
            if (_shownCorners.IsValid())
            {
                _shownCorners.gameObject.SetActive(false);
            }

            _shown = null;
            _shownCorners = null;
            _shownImage = null;
            _shownCanvas = null;
            _canvasRoot = null;
        }

        private void UpdateSorting()
        {
            if (_shownCanvas.IsInvalid() || _canvasRoot.IsInvalid())
            {
                return;
            }

            _canvases.Clear();
            _canvasRoot.GetComponentsInChildren(includeInactive: true, result: _canvases);

            var layerId = 0;
            var layerValue = int.MinValue;
            var order = int.MinValue;
            var count = _canvases.Count;

            for (var i = 0; i < count; i++)
            {
                var canvas = _canvases[i];

                if (canvas.IsInvalid() || _focusCanvases.Contains(canvas))
                {
                    continue;
                }

                var value = SortingLayer.GetLayerValueFromID(canvas.sortingLayerID);

                if (value > layerValue || (value == layerValue && canvas.sortingOrder > order))
                {
                    layerId = canvas.sortingLayerID;
                    layerValue = value;
                    order = canvas.sortingOrder;
                }
            }

            if (_shownCanvas.sortingLayerID != layerId)
            {
                _shownCanvas.sortingLayerID = layerId;
            }

            var focusOrder = order + 1;

            if (_shownCanvas.sortingOrder != focusOrder)
            {
                _shownCanvas.sortingOrder = focusOrder;
            }
        }

        private void Recolor()
        {
            if (_shownImage.IsInvalid() || _shown.IsInvalid())
            {
                return;
            }

            var surface = FindSurface(_shown.transform);
            var onLight = _config.Color(ConfigColorType.FocusOnLight);
            var onDark = _config.Color(ConfigColorType.FocusOnDark);

            _shownImage.color = Contrast(onLight, surface) >= Contrast(onDark, surface) ? onLight : onDark;
        }

        private Color FindSurface(Transform control)
        {
            var node = control.parent;

            while (node.IsValid())
            {
                if (node.TryGetComponent<Image>(out var image) && image.enabled && image.color.a >= OPAQUE_ALPHA)
                {
                    return image.color;
                }

                node = node.parent;
            }

            return _config.Color(ConfigColorType.PageMainMenu);
        }

        private static float Contrast(Color first, Color second)
        {
            var firstLuminance = Luminance(first);
            var secondLuminance = Luminance(second);
            var lighter = Mathf.Max(firstLuminance, secondLuminance);
            var darker = Mathf.Min(firstLuminance, secondLuminance);

            return (lighter + CONTRAST_OFFSET) / (darker + CONTRAST_OFFSET);
        }

        private static float Luminance(Color color)
        {
            return (LUMINANCE_RED * Linear(color.r))
                + (LUMINANCE_GREEN * Linear(color.g))
                + (LUMINANCE_BLUE * Linear(color.b));
        }

        private static float Linear(float channel)
        {
            return channel <= SRGB_LINEAR_LIMIT
                ? channel / SRGB_LINEAR_DIVISOR
                : Mathf.Pow((channel + SRGB_OFFSET) / SRGB_SCALE, SRGB_EXPONENT);
        }

        private sealed class HierarchyComparer : IComparer<Selectable>
        {
            /// <inheritdoc/>
            public int Compare(Selectable x, Selectable y)
            {
                var first = x.transform;
                var second = y.transform;

                if (first == second)
                {
                    return 0;
                }

                var firstDepth = DepthOf(first);
                var secondDepth = DepthOf(second);
                var depth = firstDepth;
                var otherDepth = secondDepth;

                while (depth > otherDepth)
                {
                    first = first.parent;
                    depth--;
                }

                while (otherDepth > depth)
                {
                    second = second.parent;
                    otherDepth--;
                }

                if (first == second)
                {
                    return firstDepth < secondDepth ? -1 : 1;
                }

                while (first.parent != second.parent)
                {
                    first = first.parent;
                    second = second.parent;
                }

                return first.GetSiblingIndex().CompareTo(second.GetSiblingIndex());
            }

            private static int DepthOf(Transform transform)
            {
                var depth = 0;

                while (transform.parent.IsValid())
                {
                    transform = transform.parent;
                    depth++;
                }

                return depth;
            }
        }
    }
}
