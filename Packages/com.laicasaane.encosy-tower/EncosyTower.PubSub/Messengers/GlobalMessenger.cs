using System.Buffers;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub
{
    public static partial class GlobalMessenger
    {
        private static Messenger s_instance;

        private static Messenger Instance => s_instance ??= new(ArrayPool<UnityTask>.Shared);

        public static MessagePublisher Publisher => Instance.Publisher;

        public static MessageSubscriber Subscriber => Instance.Subscriber;

        public static MessageInterceptors Interceptors => Instance.Interceptors;
    }
}

#if UNITY_EDITOR

namespace EncosyTower.PubSub
{
    using UnityEditor;
    using UnityEngine.Scripting;

    partial class GlobalMessenger
    {
        [InitializeOnEnterPlayMode, Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_instance?.Dispose();
            s_instance = new(ArrayPool<UnityTask>.Shared);
        }
    }
}

#endif
