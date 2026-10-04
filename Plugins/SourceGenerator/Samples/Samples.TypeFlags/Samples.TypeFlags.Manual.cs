using EncosyTower.Types;
using UnityEngine;

namespace Samples.TypeFlags
{
    public abstract class SceneMarker<T> : MonoBehaviour
        where T : SceneMarker<T>
    {
        public static TypeFlag<T>.ReadOnly TypeFlag => default;

        protected void Awake()
        {
            default(TypeFlag<T>).Enable();
        }

        protected void OnDestroy()
        {
            default(TypeFlag<T>).Disable();
        }
    }

    public sealed class SpawnPoint : SceneMarker<SpawnPoint> { }

    public static class MarkerConsumers
    {
        public static bool IsSpawnPointReady()
            => SpawnPoint.TypeFlag.IsEnabled;
    }

    public sealed class QualitySettingsProfile { }

    public sealed class LegacyService
    {
        private static readonly TypeFlag<LegacyService> s_typeFlag = default;

        public static TypeFlag<LegacyService>.ReadOnly TypeFlag => default;

        public void Start(QualitySettingsProfile profile)
        {
            s_typeFlag.TryRegister(this);
            s_typeFlag.GetLink<QualitySettingsProfile>().TryAddObject(profile);
        }

        public void Stop(QualitySettingsProfile profile)
        {
            var link = default(TypeFlagLink<LegacyService, QualitySettingsProfile>);
            TypeFlagLinkExtensions.TryRemoveObject(link, profile);
            s_typeFlag.TryUnregister(this);
        }

        public static QualitySettingsProfile GetProfile()
            => TypeFlag.GetLink<QualitySettingsProfile>().GetObjectOrThrow();
    }
}
