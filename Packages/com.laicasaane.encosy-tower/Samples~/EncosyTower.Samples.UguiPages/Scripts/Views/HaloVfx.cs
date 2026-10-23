using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Views
{
    public sealed class HaloVfx : MonoBehaviour
    {
        private Image _image;
        private Image _front;
        private Image _nearest;
        private bool _rectangular;
        private Vector3 _rotationSpeeds = new(120f, -60f, 0f);

        internal static HaloVfx Bind(Image image, bool rectangular = false)
        {
            if (image.TryGetComponent<HaloVfx>(out var vfx) == false)
            {
                vfx = image.gameObject.AddComponent<HaloVfx>();
            }

            var load = vfx._image == null || vfx._rectangular != rectangular;

            vfx._image = image;
            vfx._rectangular = rectangular;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            vfx._front ??= vfx.CreateLayer("halo-vfx-middle");
            vfx._nearest ??= vfx.CreateLayer("halo-vfx-nearest");

            if (load)
            {
                image.sprite = Resources.Load<Sprite>(
                    (rectangular ? AssetKeyAPI.CoreHaloVfxFar : AssetKeyAPI.RuneHaloVfxFar).Value);
                vfx._front.sprite = Resources.Load<Sprite>(
                    (rectangular ? AssetKeyAPI.CoreHaloVfxNear : AssetKeyAPI.RuneHaloVfxNear).Value);
                vfx._nearest.sprite = Resources.Load<Sprite>(
                    (rectangular ? AssetKeyAPI.CoreHaloVfxNearest : AssetKeyAPI.RuneHaloVfxNearest).Value);
            }

            vfx._front.rectTransform.localScale = Vector3.one;
            vfx._nearest.rectTransform.localScale = Vector3.one * (rectangular ? 384f / 409.6f : 1f);

            if (rectangular == false)
            {
                image.rectTransform.sizeDelta = Vector2.one * 204.8f;
            }

            vfx.Render(Color.clear, Color.clear, Color.clear, 0f, 0f);
            return vfx;
        }

        public void Render(Color nearest, Color middle, Color far, float elapsed, float intensity)
        {
            _image.color = Tint(far, intensity);
            _front.color = Tint(middle, intensity);
            _nearest.color = Tint(nearest, intensity);
            transform.localRotation = Quaternion.Euler(0f, 0f, -_rotationSpeeds.z * elapsed);
            // Children inherit the far rotation; compensate to keep each absolute speed independent.
            _front.transform.localRotation = Quaternion.Euler(0f, 0f,
                -(_rotationSpeeds.y - _rotationSpeeds.z) * elapsed);
            _nearest.transform.localRotation = Quaternion.Euler(0f, 0f,
                -(_rotationSpeeds.x - _rotationSpeeds.z) * elapsed);
        }

        private Image CreateLayer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)go.transform;

            go.layer = gameObject.layer;
            rect.SetParent(_image.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();

            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        public void SetRotationSpeeds(Vector3 degreesPerSecond)
        {
            _rotationSpeeds = degreesPerSecond;
        }

        public static float Intensity(float elapsed)
            => elapsed <= 1f ? 1f : Mathf.Exp(-4f * (elapsed - 1f));

        public static Vector2 CoreSize(Vector2 cardSize)
            => Vector2.one * (409.6f / 267f * Mathf.Max(cardSize.x, cardSize.y));

        private static Color Tint(Color color, float intensity)
        {
            var strength = Mathf.Clamp01(intensity);

            Color.RGBToHSV(color, out var hue, out var saturation, out var value);
            var vivid = Mathf.Clamp01(saturation * 1.35f + 0.15f);
            var result = Color.HSVToRGB(hue, Mathf.Lerp(saturation * 0.25f, vivid, strength), value);

            result.a = color.a * 0.85f * strength;
            return result;
        }
    }
}
