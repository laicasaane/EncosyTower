using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using EncosyTower.UnityExtensions;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENCOSY_SAMPLES_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EncosyTower.Samples.UguiPages.App
{
    /// <summary>
    /// Binds the codex to the canvas and the input: the camera background of the theme, the keyboard and pointer
    /// input that switches the focus corners, and the Escape key that asks for Back.
    /// </summary>
    internal sealed class AppRoot
    {
        private readonly CodexContext _ctx;
        private readonly ScreenController _screens;
        private readonly Component _host;
        private FocusCorners _focusCorners;

        public AppRoot(CodexContext ctx, ScreenController screens, Component host)
        {
            _ctx = ctx;
            _screens = screens;
            _host = host;
        }

        /// <summary>Creates the focus corners above the popup canvas.</summary>
        public void Bind()
        {
            _focusCorners = new FocusCorners(_ctx.Config);
            _focusCorners.Initialize(_ctx.Popup.Canvas.sortingOrder);
        }

        public void ApplyCameraBackground()
        {
            var canvas = _host.GetComponent<Canvas>();

            if (canvas.IsValid() && canvas.worldCamera.IsValid())
            {
                canvas.worldCamera.backgroundColor = _ctx.Config.Color(ConfigColorType.PageMainMenu);
            }
        }

        public void ApplyTheme()
        {
            ApplyCameraBackground();
            _focusCorners?.ThemeChanged();
        }

        public void TickFocus()
        {
            _focusCorners.Tick(EventSystem.current);
        }

        /// <summary>Reads the keyboard and the pointer: switches the focus mode and turns Escape into Back.</summary>
        public void PollInput()
        {
            UpdateKeyboardMode();

            if (IsEscapePressed())
            {
                _screens.BackAsync().Forget();
            }
        }

        private void UpdateKeyboardMode()
        {
            if (IsTabPressed(out var backwards))
            {
                _focusCorners.SetKeyboardMode(true);
                _focusCorners.MoveNext(backwards);
            }
            else if (IsNavigationPressed())
            {
                _focusCorners.SetKeyboardMode(true);
            }
            else if (IsPointerPressed())
            {
                _focusCorners.SetKeyboardMode(false);
            }
        }

        private static bool IsTabPressed(out bool backwards)
        {
#if ENCOSY_SAMPLES_INPUT_SYSTEM
            var keyboard = Keyboard.current;

            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
            {
                backwards = keyboard.shiftKey.isPressed;
                return true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                backwards = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                return true;
            }
#endif
            backwards = false;
            return false;
        }

        private static bool IsNavigationPressed()
        {
#if ENCOSY_SAMPLES_INPUT_SYSTEM
            return IsKeyboardNavigationPressed(Keyboard.current) || IsGamepadNavigationPressed(Gamepad.current);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.DownArrow)
                || Input.GetKeyDown(KeyCode.LeftArrow)
                || Input.GetKeyDown(KeyCode.RightArrow)
                || Input.GetKeyDown(KeyCode.W)
                || Input.GetKeyDown(KeyCode.A)
                || Input.GetKeyDown(KeyCode.S)
                || Input.GetKeyDown(KeyCode.D)
                || Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        private static bool IsPointerPressed()
        {
#if ENCOSY_SAMPLES_INPUT_SYSTEM
            return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

#if ENCOSY_SAMPLES_INPUT_SYSTEM
        private static bool IsKeyboardNavigationPressed(Keyboard keyboard)
        {
            return keyboard != null
                && (keyboard.upArrowKey.wasPressedThisFrame
                    || keyboard.downArrowKey.wasPressedThisFrame
                    || keyboard.leftArrowKey.wasPressedThisFrame
                    || keyboard.rightArrowKey.wasPressedThisFrame
                    || keyboard.wKey.wasPressedThisFrame
                    || keyboard.aKey.wasPressedThisFrame
                    || keyboard.sKey.wasPressedThisFrame
                    || keyboard.dKey.wasPressedThisFrame
                    || keyboard.enterKey.wasPressedThisFrame
                    || keyboard.numpadEnterKey.wasPressedThisFrame
                    || keyboard.spaceKey.wasPressedThisFrame
                );
        }

        private static bool IsGamepadNavigationPressed(Gamepad gamepad)
        {
            return gamepad != null
                && (gamepad.dpad.up.wasPressedThisFrame
                    || gamepad.dpad.down.wasPressedThisFrame
                    || gamepad.dpad.left.wasPressedThisFrame
                    || gamepad.dpad.right.wasPressedThisFrame
                    || gamepad.leftStick.up.wasPressedThisFrame
                    || gamepad.leftStick.down.wasPressedThisFrame
                    || gamepad.leftStick.left.wasPressedThisFrame
                    || gamepad.leftStick.right.wasPressedThisFrame
                    || gamepad.buttonSouth.wasPressedThisFrame
                );
        }
#endif

        private static bool IsEscapePressed()
        {
#if ENCOSY_SAMPLES_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
    }
}
