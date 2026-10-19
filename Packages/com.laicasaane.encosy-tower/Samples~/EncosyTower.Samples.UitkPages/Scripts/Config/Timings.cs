using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>The durations and counts that the Book reads from USS custom properties.</summary>
    internal struct Timings
    {
        private const float MILLISECONDS_PER_SECOND = 1000f;

        private static readonly CustomStyleProperty<float> s_reopenSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.NavigationBarReopenSeconds);
        private static readonly CustomStyleProperty<float> s_toastSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.ToastSeconds);
        private static readonly CustomStyleProperty<float> s_blinkSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.EntranceBlinkSeconds);
        private static readonly CustomStyleProperty<float> s_coreGlowPulseSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.EntranceGlowPulseSeconds);
        private static readonly CustomStyleProperty<float> s_historyEntries
            = ConfigNameAPI.AmountProperty(ConfigAmountType.HistoryEntries);
        private static readonly CustomStyleProperty<float> s_logEntries
            = ConfigNameAPI.AmountProperty(ConfigAmountType.HistoryLogEntries);
        private static readonly CustomStyleProperty<float> s_screenTransitionSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.TransitionScreenSeconds);
        private static readonly CustomStyleProperty<float> s_auxiliaryTransitionSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.TransitionAuxiliarySeconds);
        private static readonly CustomStyleProperty<float> s_popupTransitionSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.TransitionPopupSeconds);
        private static readonly CustomStyleProperty<float> s_echoTransitionSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.TransitionEchoSeconds);
        private static readonly CustomStyleProperty<float> s_incantationCastSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationCastSeconds);
        private static readonly CustomStyleProperty<float> s_incantationFadeSeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationFadeSeconds);
        private static readonly CustomStyleProperty<float> s_demoAbortDelaySeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoAbortDelaySeconds);
        private static readonly CustomStyleProperty<float> s_demoQuickAbortDelaySeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoQuickAbortDelaySeconds);
        private static readonly CustomStyleProperty<float> s_demoIncantationAbortDelaySeconds
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoIncantationAbortDelaySeconds);
        private static readonly CustomStyleProperty<float> s_demoPrimaryDimension
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoPrimaryDimensionIndex);
        private static readonly CustomStyleProperty<float> s_demoSwitchDimension
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoSwitchDimensionIndex);
        private static readonly CustomStyleProperty<float> s_demoNewestFirstDimension
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoNewestFirstDimensionIndex);
        private static readonly CustomStyleProperty<float> s_demoNewestSecondDimension
            = ConfigNameAPI.AmountProperty(ConfigAmountType.DemoNewestSecondDimensionIndex);

        private float _reopenSeconds;
        private float _toastSeconds;
        private float _blinkSeconds;
        private int _coreGlowPulseMs;
        private int _historyEntries;
        private int _logEntries;
        private int _screenTransitionMs;
        private int _auxiliaryTransitionMs;
        private int _popupTransitionMs;
        private int _echoTransitionMs;
        private int _incantationCastMs;
        private int _incantationFadeMs;
        private int _demoAbortDelayMs;
        private int _demoQuickAbortDelayMs;
        private int _demoIncantationAbortDelayMs;
        private int _demoPrimaryDimension;
        private int _demoSwitchDimension;
        private int _demoNewestFirstDimension;
        private int _demoNewestSecondDimension;

        public readonly float ReopenSeconds => _reopenSeconds;

        public readonly float ToastSeconds => _toastSeconds;

        public readonly float BlinkSeconds => _blinkSeconds;

        public readonly int CoreGlowPulseMs => _coreGlowPulseMs;

        public readonly int HistoryEntries => _historyEntries;

        public readonly int LogEntries => _logEntries;

        public readonly int ScreenTransitionMs => _screenTransitionMs;

        public readonly int AuxiliaryTransitionMs => _auxiliaryTransitionMs;

        public readonly int PopupTransitionMs => _popupTransitionMs;

        public readonly int EchoTransitionMs => _echoTransitionMs;

        public readonly int IncantationCastMs => _incantationCastMs;

        public readonly int IncantationFadeMs => _incantationFadeMs;

        public readonly int DemoAbortDelayMs => _demoAbortDelayMs;

        public readonly int DemoQuickAbortDelayMs => _demoQuickAbortDelayMs;

        public readonly int DemoIncantationAbortDelayMs => _demoIncantationAbortDelayMs;

        public readonly int DemoPrimaryDimension => _demoPrimaryDimension;

        public readonly int DemoSwitchDimension => _demoSwitchDimension;

        public readonly int DemoNewestFirstDimension => _demoNewestFirstDimension;

        public readonly int DemoNewestSecondDimension => _demoNewestSecondDimension;

        public void Read(ICustomStyle style)
        {
            style.Read(s_reopenSeconds, ref _reopenSeconds);
            style.Read(s_toastSeconds, ref _toastSeconds);
            style.Read(s_blinkSeconds, ref _blinkSeconds);
            ReadMilliseconds(style, s_coreGlowPulseSeconds, ref _coreGlowPulseMs);
            ReadCount(style, s_historyEntries, ref _historyEntries);
            ReadCount(style, s_logEntries, ref _logEntries);
            ReadMilliseconds(style, s_screenTransitionSeconds, ref _screenTransitionMs);
            ReadMilliseconds(style, s_auxiliaryTransitionSeconds, ref _auxiliaryTransitionMs);
            ReadMilliseconds(style, s_popupTransitionSeconds, ref _popupTransitionMs);
            ReadMilliseconds(style, s_echoTransitionSeconds, ref _echoTransitionMs);
            ReadMilliseconds(style, s_incantationCastSeconds, ref _incantationCastMs);
            ReadMilliseconds(style, s_incantationFadeSeconds, ref _incantationFadeMs);
            ReadMilliseconds(style, s_demoAbortDelaySeconds, ref _demoAbortDelayMs);
            ReadMilliseconds(style, s_demoQuickAbortDelaySeconds, ref _demoQuickAbortDelayMs);
            ReadMilliseconds(style, s_demoIncantationAbortDelaySeconds, ref _demoIncantationAbortDelayMs);
            ReadCount(style, s_demoPrimaryDimension, ref _demoPrimaryDimension);
            ReadCount(style, s_demoSwitchDimension, ref _demoSwitchDimension);
            ReadCount(style, s_demoNewestFirstDimension, ref _demoNewestFirstDimension);
            ReadCount(style, s_demoNewestSecondDimension, ref _demoNewestSecondDimension);
        }

        private static void ReadMilliseconds(
              ICustomStyle style
            , CustomStyleProperty<float> property
            , ref int milliseconds
        )
        {
            if (style.TryGetValue(property, out var seconds))
            {
                milliseconds = Mathf.RoundToInt(seconds * MILLISECONDS_PER_SECOND);
            }
            else
            {
                EncosyCustomStyleExtensions.ReportMissing(property.name);
            }
        }

        private static void ReadCount(ICustomStyle style, CustomStyleProperty<float> property, ref int count)
        {
            if (style.TryGetValue(property, out var value))
            {
                count = Mathf.RoundToInt(value);
            }
            else
            {
                EncosyCustomStyleExtensions.ReportMissing(property.name);
            }
        }
    }
}
