using System;
using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Dialogs
{
    public sealed class RuneBanner : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAfterShow
        , IPageOnBeforeHide
        , IPageOnReturnToPool
    {
        [SerializeField] private Image _backdrop;
        [SerializeField] private Image _panel;
        [SerializeField] private Image _glow;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private RuneView _rune;
        [SerializeField] private Image _runeSurface;

        private readonly RuneBannerLifetime _lifetime = new();
        private AppState _state;
        private Action _dismiss;
        private Color _glowColor;
        private Color _glowMiddleColor;
        private Color _glowFarColor;
        private HaloVfx _haloVfx;

        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || DimensionCatalog.TryGet(data.Location, out var dimension) == false
                || dimension.Index >= DimensionCatalog.OUTER_COUNT
            )
            {
                return UnityTask.FromResult(false);
            }

            Release();
            _state = data.State;
            _dismiss = data.Dismiss;
            _state.ThemeChanged += Render;
            _label.text = string.Format(_state.Config.Text(ConfigTextType.ToastRuneDiscovered), dimension.Title);
            _rune.SetRune(_state.RuneOf(dimension.Index), decoded: false, number: _state.NumberOf(dimension.Index));
            DialogBackdrop.Bind(_backdrop.gameObject, _dismiss, _state.PopupDepth > 0);
            _haloVfx = HaloVfx.Bind(_glow);
            Render();
            return UnityTask.FromResult(true);
        }

        public void OnAfterShow(PageContext context)
        {
            if (_state != null)
            {
                _lifetime.Start(Time.unscaledTime, _state.Config.Amount(ConfigAmountType.RuneBannerSeconds));
            }
        }

        public void OnBeforeHide(PageContext context)
        {
            _lifetime.Stop();
            SetGlow(0f);
        }

        public void OnReturnToPool(PageContext context)
        {
            Release();
        }

        private void Update()
        {
            if (_lifetime.IsRunning == false)
            {
                return;
            }

            var now = Time.unscaledTime;

            SetGlow(_lifetime.HaloIntensity(now));

            if (_lifetime.TryExpire(now))
            {
                SetGlow(0f);
                _dismiss?.Invoke();
            }
        }

        private void Render()
        {
            var config = _state.Config;
            var ink = config.Color(ConfigColorType.Ink);

            _panel.color = config.Color(ConfigColorType.ToastInfo);
            _label.color = ink;
            _runeSurface.color = config.Color(ConfigColorType.PageEcho);
            _rune.SetInk(ink);
            _glowColor = config.Color(ConfigColorType.RuneVfxNearest);
            _glowMiddleColor = config.Color(ConfigColorType.RuneVfxMiddle);
            _glowFarColor = config.Color(ConfigColorType.RuneVfxFar);
            BookView.SetBackdrop(_backdrop.gameObject, config);
            SetGlow(_lifetime.HaloIntensity(Time.unscaledTime));
        }

        private void SetGlow(float alpha)
        {
            if (_state != null)
            {
                _haloVfx?.SetRotationSpeeds(new Vector3(
                      _state.Config.Amount(ConfigAmountType.RuneVfxNearestDegreesPerSecond)
                    , _state.Config.Amount(ConfigAmountType.RuneVfxMiddleDegreesPerSecond)
                    , _state.Config.Amount(ConfigAmountType.RuneVfxFarDegreesPerSecond)
                ));
            }

            _haloVfx?.Render(_glowColor, _glowMiddleColor, _glowFarColor,
                _lifetime.Elapsed(Time.unscaledTime), alpha);
        }

        private void Release()
        {
            _lifetime.Stop();
            _dismiss = null;

            if (_state != null)
            {
                _state.ThemeChanged -= Render;
                _state = null;
            }
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
