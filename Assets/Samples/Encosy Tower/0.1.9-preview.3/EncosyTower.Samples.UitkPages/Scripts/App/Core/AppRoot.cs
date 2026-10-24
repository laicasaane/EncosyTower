using EncosyTower.Common;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.Views;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>
    /// Binds the codex to the panel's root element: the theme class, the boot class, the keyboard and pointer input
    /// that switches the focus corners, and the Escape key that asks for Back. It rebinds when the root is rebuilt.
    /// </summary>
    internal sealed class AppRoot
    {
        public const string APP_ROOT = "tower-book-app";
        private const string BOOTING_CLASS = "tower-book-app--booting";

        private readonly CodexContext _ctx;
        private FocusCorners _focusCorners;
        private VisualElement _app;
        private VisualElement _keyRoot;
        private int _rootVersion;

        public AppRoot(CodexContext ctx)
        {
            _ctx = ctx;
        }

        public void Bind()
        {
            Unbind();

            _app = _ctx.Codex.PanelRoot.Q(APP_ROOT);

            _rootVersion = _ctx.Codex.RootVersion;
            _keyRoot = _ctx.Codex.PanelRoot;
            _keyRoot.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            _keyRoot.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _keyRoot.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);

            _focusCorners ??= new FocusCorners();
            _focusCorners.Bind(_app);

            ApplyTheme(_ctx.State.Theme);
            _ctx.MarkStatusDirty();
        }

        public void Unbind()
        {
            if (_keyRoot != null)
            {
                _keyRoot.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
                _keyRoot.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
                _keyRoot.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                _keyRoot = null;

                _focusCorners?.Unbind();
            }

            _app = null;
        }

        /// <summary>Rebinds when the panel root was rebuilt.</summary>
        public void RebindIfRootChanged()
        {
            if (_ctx.Codex.RootVersion != _rootVersion)
            {
                Bind();
            }
        }

        public void TickFocus()
        {
            _focusCorners?.Tick();
        }

        public void EndBoot()
        {
            _app?.RemoveFromClassList(BOOTING_CLASS);
        }

        public void ApplyTheme(Theme theme)
        {
            var app = _app ?? _ctx.Codex.PanelRoot.Q(APP_ROOT);

            if (app == null)
            {
                return;
            }

            BookView.SnapAll(app);

            app.EnableInClassList(ThemeAPI.DARK_CLASS, theme == Theme.Dark);

            _focusCorners?.Repick();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                {
                    BookActions.Publish(_ctx.Codex.FlowContext.Publisher, Option.Some(_ctx.Scopes), AppAction.Back);
                    evt.StopPropagation();
                    break;
                }

                case KeyCode.Tab:
                case KeyCode.UpArrow:
                case KeyCode.DownArrow:
                case KeyCode.LeftArrow:
                case KeyCode.RightArrow:
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                case KeyCode.Space:
                {
                    _focusCorners?.SetKeyboardMode(true);
                    break;
                }
            }
        }

        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            _focusCorners?.SetKeyboardMode(true);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            _focusCorners?.SetKeyboardMode(false);
        }
    }
}
