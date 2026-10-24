using System;
using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Dialogs
{
    [UxmlElement]
    public partial class RuneBanner : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAfterShow
        , IPageOnBeforeHide
        , IPageOnReturnToPool
    {
        private readonly PaletteStyle _palette = new();
        private readonly RuneBannerLifetime _lifetime = new();
        private readonly DialogBackdrop _dismissal = new();
        private VisualElement _root;
        private Image _glow;
        private HaloVfx _haloVfx;
        private RuneView _rune;
        private Label _label;
        private AppState _state;
        private Action _dismiss;
        private IVisualElementScheduledItem _tick;

        public RuneBanner()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnStyleResolved);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        public override PageOptions PageOptions => PopupPageOptions.KeepLowerPage;

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

            if (_root == null)
            {
                _root = this.Q("root");
                _glow = this.Q<Image>("glow");
                _haloVfx = new HaloVfx(_glow);
                _rune = this.Q<RuneView>("rune");
                _label = this.Q<Label>("label-discovery");
                _tick = schedule.Execute(Tick).Every(16);
                _tick.Pause();
            }

            _state = data.State;
            _dismiss = data.Dismiss;
            _dismissal.Bind(this, _dismiss);
            EnableInClassList("tower-book-popup--nested", _state.PopupDepth > 0);
            _label.text = _state.Resources.Format(ConfigTextType.ToastRuneDiscovered, dimension.Title);
            _rune.SetRune(_state.RuneOf(dimension.Index), decoded: false, number: _state.NumberOf(dimension.Index));
            Render();
            return UnityTask.FromResult(true);
        }

        public void OnAfterShow(PageContext context)
        {
            if (_state != null)
            {
                _lifetime.Start(Time.unscaledTime, _state.Timings.RuneBannerSeconds);
                _tick.Resume();
            }
        }

        public void OnBeforeHide(PageContext context)
        {
            Stop();
        }

        public void OnReturnToPool(PageContext context)
        {
            Release();
        }

        protected override IPageTransition CreateTransition(VisualElement slot)
            => new PopupTransition(slot, _state.Timings.PopupTransitionMs);

        private void Tick()
        {
            var now = Time.unscaledTime;

            _haloVfx?.SetRotationSpeeds(_state.Timings.RuneVfxSpeeds);
            _haloVfx.Render(_palette.Color(ConfigColorType.RuneVfxNearest),
                _palette.Color(ConfigColorType.RuneVfxMiddle), _palette.Color(ConfigColorType.RuneVfxFar),
                _lifetime.Elapsed(now), _lifetime.HaloIntensity(now));

            if (_lifetime.TryExpire(now))
            {
                Stop();
                _dismiss?.Invoke();
            }
        }

        private void OnStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            if (_state != null)
            {
                Render();
            }
        }

        private void Render()
        {
            var ink = _palette.Color(ConfigColorType.Ink);

            _root.style.backgroundColor = _palette.Color(ConfigColorType.ToastInfo);
            _label.style.color = ink;
            _rune.style.backgroundColor = _palette.Color(ConfigColorType.PageEcho);
            _rune.SetInk(ink);
            _haloVfx?.SetRotationSpeeds(_state.Timings.RuneVfxSpeeds);
            _haloVfx.Render(_palette.Color(ConfigColorType.RuneVfxNearest),
                _palette.Color(ConfigColorType.RuneVfxMiddle), _palette.Color(ConfigColorType.RuneVfxFar),
                _lifetime.Elapsed(Time.unscaledTime), _lifetime.HaloIntensity(Time.unscaledTime));
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            Stop();
        }

        private void Stop()
        {
            _lifetime.Stop();
            _tick?.Pause();

            if (_glow != null)
            {
                _haloVfx?.Render(Color.clear, Color.clear, Color.clear, 0f, 0f);
            }
        }

        private void Release()
        {
            Stop();
            _haloVfx?.Release();
            _dismissal.Unbind();
            _dismiss = null;
            _state = null;
        }
    }
}
