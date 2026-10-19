using System;
using System.IO;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Encryption;
using EncosyTower.IO;
using EncosyTower.Logging;
using EncosyTower.Persistences;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.StringIds;
using EncosyTower.Tasks;
using UnityEngine;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.Persistence
{

    /// <summary>Loads and saves the user data of the Book, the progress and the theme, as one local file.</summary>
    internal static class UserStorage
    {
        private const string FOLDER = "PageFlowsSamples";
        private const string SYSTEM = "uGUI";
        private const string USER_DATA_SLOT = "tower-book-user-data-v4";
        private const string PLAYER_ID = "player";
        private const string EXTENSION = "json";

        // The progress and the theme share one file, so the writes must not interleave.
        private static readonly SemaphoreSlim s_gate = new(1, 1);

        public static async UnityTask<Result<UserData, BookLoadError>> LoadProgressAsync(
            CancellationToken token
        )
        {
            try
            {
                using var encryption = new PlainTextEncryption();
                var store = CreateStore(encryption);

                if (File.Exists(store.FilePath) == false)
                {
                    return BookLoadError.Missing;
                }

                await store.LoadAsync(SourcePriority.LocalThenRemote, token);

                var data = store.GetData(SourcePriority.LocalThenRemote);

                if (data == null)
                {
                    return BookLoadError.Unreadable;
                }

                return data.HasProgress ? data.Clone() : BookLoadError.Missing;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ThrowHelper.LogError_StorageFailed("loading progress", ex);
                return BookLoadError.Unavailable;
            }
        }

        public static async UnityTask<bool> SaveProgressAsync(UserData progress, CancellationToken token)
        {
            await s_gate.WaitAsync(token);

            try
            {
                using var encryption = new PlainTextEncryption();
                var store = CreateStore(encryption);

                store.CreateData();
                store.SetData(progress.Clone());
                await store.SaveAsync(SaveDestination.Local, token);

                var reader = CreateStore(encryption);

                await reader.LoadAsync(SourcePriority.LocalThenRemote, token);
                return progress.ContentEquals(reader.GetData(SourcePriority.LocalThenRemote));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ThrowHelper.LogError_StorageFailed("saving progress", ex);
                return false;
            }
            finally
            {
                s_gate.Release();
            }
        }

        public static Theme LoadTheme()
        {
            string reason;

            try
            {
                using var encryption = new PlainTextEncryption();
                var path = CreateStore(encryption).FilePath;

                if (File.Exists(path) == false)
                {
                    return Theme.Light;
                }

                if (TryReadRaw(path, out var data))
                {
                    return data.Theme;
                }

                reason = "the content is not valid";
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                reason = ex.Message;
            }

            ThrowHelper.LogWarning_SettingsFailed("read", reason);
            return Theme.Light;
        }

        public static async UnityTask<bool> SaveThemeAsync(Theme theme, CancellationToken token)
        {
            await s_gate.WaitAsync(token);

            try
            {
                using var encryption = new PlainTextEncryption();
                var store = CreateStore(encryption);
                var data = new UserData();

                if (File.Exists(store.FilePath) && TryReadRaw(store.FilePath, out data) == false)
                {
                    ThrowHelper.LogWarning_SettingsFailed("saved", "the stored file is not readable");
                    return false;
                }

                data.Theme = theme;

                store.CreateData();
                store.SetData(data);
                await store.SaveAsync(SaveDestination.Local, token);

                var reader = CreateStore(encryption);

                await reader.LoadAsync(SourcePriority.LocalThenRemote, token);

                var saved = reader.GetData(SourcePriority.LocalThenRemote);

                if (saved != null && saved.Theme == theme)
                {
                    return true;
                }

                ThrowHelper.LogWarning_SettingsFailed("saved", "the stored file does not match");
                return false;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ThrowHelper.LogError_StorageFailed("saving settings", ex);
                return false;
            }
            finally
            {
                s_gate.Release();
            }
        }

        private static bool TryReadRaw(string path, out UserData data)
        {
            data = JsonUtility.FromJson<UserData>(File.ReadAllText(path));

            if (data != null && data.Theme is Theme.Light or Theme.Dark)
            {
                return true;
            }

            data = null;
            return false;
        }

        private static string RootDirectory()
            => Path.Combine(Application.persistentDataPath, FOLDER, SYSTEM);

        private static PersistStoreDefault<UserData> CreateStore(EncryptionBase encryption)
        {
            var vault = StringVault.Default;
            var source = new PersistSourceLocal<UserData>.Args(
                  new RootPath(RootDirectory())
                , UserData.TrySerialize
                , UserData.TryDeserialize
                , EXTENSION
            );

            var args = new PersistStoreDefault<UserData>.Args(static () => new UserData(), source);

            var store = new PersistStoreDefault<UserData>(
                  vault.GetOrMakeId(USER_DATA_SLOT)
                , vault
                , encryption
                , DevLogger.Default
                , ignoreEncryption: true
                , args
            );

            store.Id = PLAYER_ID;
            store.Initialize();
            return store;
        }

        private sealed class PlainTextEncryption : EncryptionBase
        {
            public PlainTextEncryption() : base(DevLogger.Default)
            {
            }

            public override string Encrypt(string plain)
                => plain ?? string.Empty;

            public override string Decrypt(string encrypted)
                => encrypted ?? string.Empty;
        }
    }
}
