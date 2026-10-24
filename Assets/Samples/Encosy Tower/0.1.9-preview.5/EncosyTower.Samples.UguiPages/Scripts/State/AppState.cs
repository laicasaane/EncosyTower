using System;
using System.Collections.Generic;
using EncosyTower.Samples.UguiPages.Attachments;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Persistence;

namespace EncosyTower.Samples.UguiPages.State
{
    /// <summary>What the player left open on a dimension screen: its tab and its cards.</summary>
    internal readonly record struct DimensionView(int Tab, bool ConceptVisible, bool InPracticeVisible);

    /// <summary>
    /// The state of the Book: the current screen, the popups, the discovered runes and the saved views.
    /// </summary>
    /// <remarks>The codex updates it from page navigations; pages render from it and from its events.</remarks>
    internal sealed class AppState
    {
        private readonly AppConfig _config;
        private readonly Rune[] _runeForDimension = new Rune[DimensionCatalog.OUTER_COUNT];
        private readonly List<int> _acquisitionOrder = new(DimensionCatalog.OUTER_COUNT);
        private readonly Dictionary<Dimension, DimensionView> _views = new();

        private BookLocation _currentLocation = BookLocation.MainMenu;
        private Dimension _currentDimension = DimensionCatalog.Outer[0].Id;
        private int _popupDepth;
        private bool _isPopupActive;
        private bool _isMoreOnTop;
        private int _discoveredMask;
        private bool _coreVisited;
        private bool _incantationCast;
        private bool _coreUnlockNoticePending;
        private bool _coreBlink;
        private bool _hasSavedGame;
        private bool _hasUnsavedProgress;

        public AppState(AppConfig config)
        {
            _config = config;
        }

        public event Action Changed;

        public event Action ThemeChanged;

        public AppConfig Config => _config;

        public Theme Theme => _config.ActiveTheme;

        public ScreenPage CurrentScreen => _currentLocation.Screen;

        public BookLocation CurrentLocation => _currentLocation;

        public Dimension CurrentDimension => _currentDimension;

        public PageAttachments Attachments { get; set; }

        public int PopupDepth => _popupDepth;

        public bool IsPopupActive => _isPopupActive;

        public bool IsMoreOnTop => _isMoreOnTop;

        public int DiscoveredMask => _discoveredMask;

        public bool CoreVisited => _coreVisited;

        public bool CoreUnlocked => _discoveredMask == DimensionCatalog.FULL_MASK;

        public bool IncantationCast => _incantationCast;

        public bool CoreUnlockNoticePending => _coreUnlockNoticePending;

        public bool CoreBlink => _coreBlink;

        public bool HasSavedGame => _hasSavedGame;

        public bool HasUnsavedProgress => _hasUnsavedProgress;

        public ReadOnlySpan<Rune> RuneForDimension => _runeForDimension;

        public IReadOnlyList<int> AcquisitionOrder => _acquisitionOrder;

        public DimensionData CurrentDimensionData
        {
            get
            {
                DimensionCatalog.TryGet(_currentDimension, out var data);
                return data;
            }
        }

        public bool IsDiscovered(int dimensionIndex)
            => (uint)dimensionIndex < (uint)DimensionCatalog.OUTER_COUNT
            && (_discoveredMask & (1 << dimensionIndex)) != 0;

        public Rune RuneOf(int dimensionIndex)
            => _runeForDimension[dimensionIndex];

        public int NumberOf(int dimensionIndex)
            => _acquisitionOrder.IndexOf(dimensionIndex) + 1;

        public bool TryGetAcquiredRune(int slot, out Rune rune)
        {
            if ((uint)slot < (uint)_acquisitionOrder.Count)
            {
                rune = _runeForDimension[_acquisitionOrder[slot]];
                return true;
            }

            rune = default;
            return false;
        }

        public bool IsAvailable(BookLocation location)
            => location.Screen switch {
                ScreenPage.CoreDimension => CoreUnlocked,
                ScreenPage.Entrance => true,
                ScreenPage.Dimension => DimensionCatalog.TryGet(location, out _),
                _ => false,
            };

        public DimensionView ViewOf(Dimension dimension)
            => _views.TryGetValue(dimension, out var view) ? view : default;

        public void Load(UserData data, bool hasSavedGame)
        {
            data.RuneForDimension.AsSpan().CopyTo(_runeForDimension);
            _acquisitionOrder.Clear();
            _acquisitionOrder.AddRange(data.AcquisitionOrder);
            _discoveredMask = data.DiscoveredMask;
            _coreVisited = data.CoreVisited;
            _incantationCast = data.IncantationCast;
            _coreUnlockNoticePending = data.CoreUnlockNoticePending;
            _coreBlink = false;
            _hasSavedGame = hasSavedGame;
            _hasUnsavedProgress = false;
            _views.Clear();

            var states = data.ViewStates;
            var count = states.Length;

            for (var i = 0; i < count; i++)
            {
                var state = states[i];
                _views[state.Dimension] = new DimensionView(state.Tab, state.ConceptVisible, state.InPracticeVisible);
            }

            RaiseChanged();
        }

        public UserData CreateProgress()
        {
            var states = new ViewStateData[_views.Count];
            var index = 0;

            foreach (var pair in _views)
            {
                states[index++] = new ViewStateData {
                    Dimension = pair.Key,
                    Tab = pair.Value.Tab,
                    ConceptVisible = pair.Value.ConceptVisible,
                    InPracticeVisible = pair.Value.InPracticeVisible,
                };
            }

            return new UserData {
                RuneForDimension = _runeForDimension.AsSpan().ToArray(),
                AcquisitionOrder = _acquisitionOrder.ToArray(),
                DiscoveredMask = _discoveredMask,
                CoreVisited = _coreVisited,
                IncantationCast = _incantationCast,
                CoreUnlockNoticePending = _coreUnlockNoticePending,
                ViewStates = states,
                Theme = Theme,
            };
        }

        public void ApplyScreen(BookLocation location)
        {
            if (DimensionCatalog.TryGet(location, out var data) && data.Index < DimensionCatalog.OUTER_COUNT)
            {
                SetScreen(location);
                _currentDimension = data.Id;
            }
            else if (location.Screen is ScreenPage.CoreDimension or ScreenPage.Entrance)
            {
                SetScreen(location);
            }
            else
            {
                SetScreen(BookLocation.MainMenu);
            }

            RaiseChanged();
        }

        public void ApplyTab(Dimension dimension, int index)
        {
            var view = ViewOf(dimension);

            if (view.Tab == index && _views.ContainsKey(dimension))
            {
                return;
            }

            _views[dimension] = view with { Tab = index };

            RaiseChanged();
        }

        public void ApplyCard(Dimension dimension, int index, bool visible)
        {
            var view = ViewOf(dimension);

            if (index == 0)
            {
                if (view.ConceptVisible == visible)
                {
                    return;
                }

                _views[dimension] = view with { ConceptVisible = visible };
            }
            else
            {
                if (view.InPracticeVisible == visible)
                {
                    return;
                }

                _views[dimension] = view with { InPracticeVisible = visible };
            }

            RaiseChanged();
        }

        public bool DiscoverDimension(int dimensionIndex)
        {
            if ((uint)dimensionIndex >= (uint)DimensionCatalog.OUTER_COUNT || IsDiscovered(dimensionIndex))
            {
                return false;
            }

            _acquisitionOrder.Add(dimensionIndex);
            _discoveredMask |= 1 << dimensionIndex;
            _hasUnsavedProgress = true;

            if (_discoveredMask == DimensionCatalog.FULL_MASK)
            {
                _coreUnlockNoticePending = true;
            }

            RaiseChanged();
            return true;
        }

        public bool VisitCore()
        {
            if (CoreUnlocked == false || _coreVisited)
            {
                return false;
            }

            _coreVisited = true;
            _hasUnsavedProgress = true;

            RaiseChanged();
            return true;
        }

        public bool CompleteIncantation()
        {
            if (_coreVisited == false || _incantationCast)
            {
                return false;
            }

            _incantationCast = true;
            _hasUnsavedProgress = true;

            RaiseChanged();
            return true;
        }

        public bool ConsumeCoreUnlockNotice()
        {
            if (_coreUnlockNoticePending == false)
            {
                return false;
            }

            _coreUnlockNoticePending = false;
            _hasUnsavedProgress = true;
            _coreBlink = true;

            RaiseChanged();
            return true;
        }

        public void ClearCoreBlink()
        {
            if (_coreBlink == false)
            {
                return;
            }

            _coreBlink = false;
            RaiseChanged();
        }

        public void SetSavedGame(bool value)
        {
            if (_hasSavedGame == value)
            {
                return;
            }

            _hasSavedGame = value;
            RaiseChanged();
        }

        public void SetTheme(Theme theme)
        {
            if (_config.ActiveTheme == theme)
            {
                return;
            }

            _config.ActiveTheme = theme;
            ThemeChanged?.Invoke();
            RaiseChanged();
        }

        public void SetPopupStack(int depth)
        {
            if (_popupDepth == depth)
            {
                return;
            }

            _popupDepth = depth;
            RaiseChanged();
        }

        public void SetPopupActive(bool isActive, bool isMoreOnTop)
        {
            if (_isPopupActive == isActive && _isMoreOnTop == isMoreOnTop)
            {
                return;
            }

            _isPopupActive = isActive;
            _isMoreOnTop = isMoreOnTop;
            RaiseChanged();
        }

        public void SetProgressSaved()
        {
            _hasUnsavedProgress = false;
        }

        private void SetScreen(BookLocation location)
        {
            _currentLocation = location;
        }

        private void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
