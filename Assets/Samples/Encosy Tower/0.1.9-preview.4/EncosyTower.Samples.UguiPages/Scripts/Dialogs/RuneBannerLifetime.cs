using UnityEngine;

namespace EncosyTower.Samples.UguiPages.Dialogs
{
    internal sealed class RuneBannerLifetime
    {
        private float _start;
        private float _duration;
        private bool _running;

        public bool IsRunning => _running;

        public static float Duration(float configured)
            => float.IsNaN(configured) || float.IsInfinity(configured) ? 5f : Mathf.Max(2f, configured);

        public void Start(float now, float configured)
        {
            _start = now;
            _duration = Duration(configured);
            _running = true;
        }

        public void Stop()
        {
            _running = false;
        }

        public bool TryExpire(float now)
        {
            if (_running == false || now - _start < _duration)
            {
                return false;
            }

            _running = false;
            return true;
        }

        public float Elapsed(float now)
            => Mathf.Max(0f, now - _start);

        public float HaloIntensity(float now)
            => _running ? Views.HaloVfx.Intensity(Elapsed(now)) : 0f;
    }
}
