using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Encryption;
using EncosyTower.Initialization;
using EncosyTower.Logging;
using EncosyTower.StringIds;
using EncosyTower.Tasks;

using ETDBG = EncosyTower.Debugging;

namespace EncosyTower.Persistences
{
    public abstract class PersistSourceBase<TData> : IInitializable
        where TData : IPersist
    {
        protected PersistSourceBase(
              StringId<string> key
            , [NotNull] StringVault stringVault
            , [NotNull] EncryptionBase encryption
            , ILogger logger
            , bool ignoreEncryption
            , PersistSourceArgs _
        )
        {
            ETDBG.ThrowHelper.ThrowIfNull(stringVault);
            ETDBG.ThrowHelper.ThrowIfNull(encryption);

            Key = key;
            StringVault = stringVault;
            Encryption = encryption;
            Logger = logger ?? DevLogger.Default;
            IgnoreEncryption = ignoreEncryption;
        }

        public StringId<string> Key { get; }

        public StringVault StringVault { get; }

        public EncryptionBase Encryption { get; }

        public bool IgnoreEncryption
        {
#if ENFORCE_PERSISTENCE_ENCRYPTION
            get => false;
            set { }
#else
            get;
#endif
        }

        public ILogger Logger { get; }

        public bool IsDirty { get; set; }

        public virtual void Initialize() { }

        public async UnityTask SaveAsync([NotNull] TData data, CancellationToken token)
        {
            ETDBG.ThrowHelper.ThrowIfNullOrUnityObjectInvalid(data);

            if (IsDirty == false)
            {
                return;
            }

            IsDirty = false;

            await OnSaveAsync(data, token);
        }

        protected abstract UnityTask OnSaveAsync([NotNull] TData data, CancellationToken token);

        public abstract UnityTask<Option<TData>> TryLoadAsync(CancellationToken token);

        public abstract Option<TData> TryCloneData(TData source);
    }
}
