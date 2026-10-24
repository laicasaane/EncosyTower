using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    internal sealed class HaloVfx
    {
        private readonly Image _image;
        private readonly Image _front;
        private readonly Image _nearest;
        private readonly Sprite _farSprite;
        private readonly Sprite _middleSprite;
        private readonly Sprite _nearestSprite;
        private Vector3 _rotationSpeeds = new(120f, -60f, 0f);

        public HaloVfx(Image image, bool rectangular = false)
        {
            _image = image;
            _image.pickingMode = PickingMode.Ignore;
            _image.scaleMode = ScaleMode.ScaleToFit;
            _image.style.backgroundColor = Color.clear;
            _farSprite = Resources.Load<Sprite>(
                (rectangular ? AssetKeyAPI.CoreHaloVfxFar : AssetKeyAPI.RuneHaloVfxFar).Value);
            _middleSprite = Resources.Load<Sprite>(
                (rectangular ? AssetKeyAPI.CoreHaloVfxNear : AssetKeyAPI.RuneHaloVfxNear).Value);
            _nearestSprite = Resources.Load<Sprite>(
                (rectangular ? AssetKeyAPI.CoreHaloVfxNearest : AssetKeyAPI.RuneHaloVfxNearest).Value);
            _front = CreateLayer("halo-vfx-middle");
            _nearest = CreateLayer("halo-vfx-nearest");
            var nearestFraction = rectangular ? 384f / 409.6f : 1f;

            _nearest.style.left = _nearest.style.top = Length.Percent((1f - nearestFraction) * 50f);
            _nearest.style.width = _nearest.style.height = Length.Percent(nearestFraction * 100f);
            _image.RegisterCallback<AttachToPanelEvent>(OnAttach);
            _image.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            Render(Color.clear, Color.clear, Color.clear, 0f, 0f);
        }

        public void Render(Color nearest, Color middle, Color far, float elapsed, float intensity)
        {
            SetSprites();
            _image.tintColor = Tint(far, intensity);
            _front.tintColor = Tint(middle, intensity);
            _nearest.tintColor = Tint(nearest, intensity);
            _image.style.rotate = new Rotate(new Angle(_rotationSpeeds.z * elapsed, AngleUnit.Degree));
            _front.style.rotate = new Rotate(new Angle(
                (_rotationSpeeds.y - _rotationSpeeds.z) * elapsed, AngleUnit.Degree));
            _nearest.style.rotate = new Rotate(new Angle(
                (_rotationSpeeds.x - _rotationSpeeds.z) * elapsed, AngleUnit.Degree));
        }

        public void Release()
        {
            _image.sprite = null;
            _front.sprite = null;
            _nearest.sprite = null;
        }

        private Image CreateLayer(string name)
        {
            var image = new Image {
                name = name,
                pickingMode = PickingMode.Ignore,
                scaleMode = ScaleMode.ScaleToFit,
            };

            image.style.position = Position.Absolute;
            image.style.left = image.style.top = 0f;
            image.style.width = image.style.height = Length.Percent(100f);
            _image.Add(image);
            return image;
        }

        private void OnAttach(AttachToPanelEvent evt)
        {
            if (ReferenceEquals(evt.target, _image))
            {
                SetSprites();
            }
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            if (ReferenceEquals(evt.target, _image))
            {
                Release();
            }
        }

        private void SetSprites()
        {
            _image.sprite = _farSprite;
            _front.sprite = _middleSprite;
            _nearest.sprite = _nearestSprite;
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
