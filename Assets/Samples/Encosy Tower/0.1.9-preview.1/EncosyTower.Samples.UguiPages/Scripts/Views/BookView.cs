using System;
using System.Collections.Generic;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.Views
{
    /// <summary>The edge of a button that shows it is selected.</summary>
    internal enum SelectionEdge
    {
        Top,
        Bottom,
    }

    /// <summary>Helpers that color and label the buttons and images of the pages.</summary>
    internal static class BookView
    {
        private const float PANEL_BORDER_ALPHA = 0.45f;
        private const float DIALOG_BORDER_ALPHA = 0.6f;
        private const string BORDER_NAME = "border";
        internal const string ICON_NAME = "icon";
        private const string SELECTION_BAR_NAME = "selection-bar";

        private static readonly List<Image> s_images = new();
        private static readonly HashSet<int> s_reportedLabelButtons = new();

        private static void LogLabelMissingOnce(Button button)
        {
            if (s_reportedLabelButtons.Add(button.GetInstanceID()))
            {
                ThrowHelper.LogError_ButtonLabelMissing(button);
            }
        }

        public static void SetColors(Button button, Color background, Color ink, AppConfig config)
        {
            if (button.targetGraphic is Image image)
            {
                image.color = background;
            }

            var label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.color = ink;
            }
            else
            {
                LogLabelMissingOnce(button);
            }

            var icon = FindChildImage(button, ICON_NAME);

            if (icon.IsValid())
            {
                icon.color = ink;
            }

            var border = FindBorder(button);

            if (border.IsValid())
            {
                border.color = Color.Lerp(background, ink, config.Amount(ConfigAmountType.ButtonOutlineMix));
            }

            SetGenericLooks(button, ink, config);
        }

        public static void SetGenericLooks(Button button, AppConfig config)
        {
            if (TryGetInk(button, out var ink))
            {
                SetGenericLooks(button, ink, config);
            }
        }

        public static void SetStateLooks(Button button, in ButtonLook hover, in ButtonLook press, Color inset)
        {
            if (button.TryGetComponent<ButtonStates>(out var states) == false)
            {
                states = button.gameObject.AddComponent<ButtonStates>();
            }

            states.SetLooks(in hover, in press, inset);
            states.NormalChanged();
        }

        public static void SetDimensionLooks(
              Button button
            , Color baseColor
            , Color pressBase
            , Color ink
            , AppConfig config
        )
        {
            var hoverFill = Color.Lerp(baseColor, ink, config.Amount(ConfigAmountType.DimensionHoverMix));
            var pressFill = Color.Lerp(pressBase, ink, config.Amount(ConfigAmountType.DimensionPressMix));
            var hover = new ButtonLook(hoverFill, ink, ink);
            var press = new ButtonLook(pressFill, ink, ink);

            SetStateLooks(button, in hover, in press, ink);
        }

        public static void SetInteractable(Button button, bool interactable, AppConfig config)
        {
            SetInteractable(button, interactable, config.Amount(ConfigAmountType.ButtonDisabledAlpha));
        }

        public static void SetInteractable(Button button, bool interactable, float disabledAlpha)
        {
            button.interactable = interactable;

            if (button.TryGetComponent<CanvasGroup>(out var group) == false)
            {
                group = button.gameObject.AddComponent<CanvasGroup>();
            }

            group.alpha = interactable ? 1f : disabledAlpha;

            if (interactable == false && button.TryGetComponent<ButtonStates>(out var states))
            {
                states.ResetState();
            }
        }

        public static void SetBorders(Component root, AppConfig config)
        {
            root.GetComponentsInChildren(includeInactive: true, result: s_images);

            var count = s_images.Count;

            for (var i = 0; i < count; i++)
            {
                var image = s_images[i];

                var imageTransform = image.transform;
                var imageParent = imageTransform.parent;

                if (imageParent == null || imageParent.Find(BORDER_NAME) != imageTransform)
                {
                    continue;
                }

                var alpha = image.color.a;
                ConfigColorType colorName;

                if (Mathf.Approximately(alpha, PANEL_BORDER_ALPHA))
                {
                    colorName = ConfigColorType.BorderTranslucentPanel;
                }
                else if (Mathf.Approximately(alpha, DIALOG_BORDER_ALPHA))
                {
                    colorName = ConfigColorType.BorderTranslucentDialog;
                }
                else
                {
                    continue;
                }

                var parent = image.transform.parent;

                if ((parent.IsValid() && parent.TryGetComponent<Selectable>(out _))
                    || image.GetComponentInParent<RuneView>(includeInactive: true).IsValid()
                )
                {
                    continue;
                }

                image.color = config.Color(colorName);
            }

            s_images.Clear();
        }

        public static void SetBackdrop(GameObject backdrop, AppConfig config)
        {
            if (backdrop.TryGetComponent<Image>(out var image))
            {
                image.color = config.Color(ConfigColorType.Backdrop);
            }
        }

        public static void SetColors(Image[] images, Color color)
        {
            var count = images.Length;

            for (var i = 0; i < count; i++)
            {
                images[i].color = color;
            }
        }

        public static void SetColors(TMP_Text[] texts, Color color)
        {
            var count = texts.Length;

            for (var i = 0; i < count; i++)
            {
                texts[i].color = color;
            }
        }

        public static T[] FindChildren<T>(Component root, string objectName)
            where T : Component
        {
            var found = root.GetComponentsInChildren<T>(includeInactive: true);
            var length = found.Length;
            var count = 0;

            for (var i = 0; i < length; i++)
            {
                var child = found[i];

                if (string.Equals(child.name, objectName, StringComparison.Ordinal))
                {
                    found[count++] = child;
                }
            }

            Array.Resize(ref found, count);
            return found;
        }

        public static void SetSelected(Button button, bool selected)
        {
            if (selected == false || TryGetInk(button, out var ink) == false)
            {
                return;
            }

            var border = FindBorder(button);

            if (border.IsValid())
            {
                border.color = ink;
            }

            NotifyNormalBorderChanged(button, ink);
        }

        public static void SetSelected(Button button, bool selected, SelectionEdge edge, float height)
        {
            var bar = FindChildImage(button, SELECTION_BAR_NAME);

            if (bar.IsInvalid())
            {
                if (selected == false)
                {
                    return;
                }

                bar = CreateSelectionBar(button);
            }

            var rect = (RectTransform)bar.transform;
            var isTop = edge == SelectionEdge.Top;
            var edgeY = isTop ? 1f : 0f;

            rect.anchorMin = new Vector2(0f, edgeY);
            rect.anchorMax = new Vector2(1f, edgeY);
            rect.pivot = new Vector2(0.5f, edgeY);
            var insetLeft = 1f;
            var insetRight = 1f;

            if (button.targetGraphic is Image fill && fill.sprite != null)
            {
                var spriteBorder = fill.sprite.border;

                insetLeft = Mathf.Max(1f, spriteBorder.x - 2f);
                insetRight = Mathf.Max(1f, spriteBorder.z - 2f);
            }

            rect.offsetMin = new Vector2(insetLeft, isTop ? -height : 0f);
            rect.offsetMax = new Vector2(-insetRight, isTop ? 0f : height);

            bar.gameObject.SetActive(selected);

            if (selected == false || TryGetInk(button, out var ink) == false)
            {
                return;
            }

            var barColor = ink;

            barColor.a = 1f;
            bar.color = barColor;

            var border = FindBorder(button);

            if (border.IsValid())
            {
                border.color = ink;
            }

            NotifyNormalBorderChanged(button, ink);
        }

        public static void SetLabel(Button button, string text)
        {
            var label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = text;
            }
            else
            {
                LogLabelMissingOnce(button);
            }
        }

        public static void SetAccessibleName(Button button, string text)
        {
            var label = button.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = text;
                label.enabled = false;
            }
            else
            {
                LogLabelMissingOnce(button);
            }
        }

        internal static Image FindBorder(Component owner)
        {
            return FindChildImage(owner, BORDER_NAME);
        }

        private static Image FindChildImage(Component owner, string childName)
        {
            var child = owner.transform.Find(childName);

            if (child != null && child.TryGetComponent<Image>(out var image))
            {
                return image;
            }

            return null;
        }

        private static void SetGenericLooks(Button button, Color ink, AppConfig config)
        {
            var hover = new ButtonLook(config.Color(ConfigColorType.ButtonHover), ink, ink);
            var press = new ButtonLook(config.Color(ConfigColorType.ButtonPress), ink, ink);

            SetStateLooks(button, in hover, in press, ink);
        }

        private static void NotifyNormalBorderChanged(Button button, Color border)
        {
            if (button.TryGetComponent<ButtonStates>(out var states))
            {
                states.SetNormalBorder(border);
            }
        }

        private static Image CreateSelectionBar(Button button)
        {
            var go = new GameObject(SELECTION_BAR_NAME, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            go.layer = button.gameObject.layer;
            go.transform.SetParent(button.transform, worldPositionStays: false);
            go.transform.SetAsLastSibling();

            var image = go.GetComponent<Image>();

            image.raycastTarget = false;

            return image;
        }

        private static bool TryGetInk(Button button, out Color ink)
        {
            var label = button.GetComponentInChildren<TMP_Text>();

            if (label.IsInvalid())
            {
                LogLabelMissingOnce(button);
                ink = default;
                return false;
            }

            ink = label.color;
            return true;
        }
    }
}
