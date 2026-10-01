using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Debugging;
using EncosyTower.Initialization;
using EncosyTower.Tasks;
using UnityEngine;

namespace EncosyTower.Persistences
{
    using ILogger = Logging.ILogger;

    public abstract class PersistenceBase : IDeinitializable, IDisposable
    {
        private bool _markDirtyBeforeSaving;

        protected abstract IPersistDirectory PersistDirectory { get; }

        public async UnityTask<bool> TryLoadAsync(
              [NotNull] ILogger logger
            , string id
            , SourcePriority priority
            , SaveDestination destination
            , CancellationToken token = default
        )
        {
            ThrowHelper.ThrowIfNull(logger);

            _markDirtyBeforeSaving = true;

            PersistDirectory.Id = id;

            if (string.IsNullOrEmpty(id))
            {
                LogWarningInvalidId(logger);
                return false;
            }

            PersistDirectory.Initialize();

            await PersistDirectory.LoadEntireDirectoryAsync(priority, token);

            if (token.IsCancellationRequested)
            {
                return false;
            }

            PersistDirectory.CreateDataIfNotExist();

            var result = await OnTryLoadAsync(logger, id, priority, destination, token);

            if (result == false || token.IsCancellationRequested)
            {
                return false;
            }

            await SaveAsync(destination, token: token);

            return true;
        }

        public void Deinitialize()
        {
            PersistDirectory.Deinitialize();
            OnDeinitialize();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async UnityTask SaveAsync(SaveDestination destination, CancellationToken token = default)
        {
            if (_markDirtyBeforeSaving)
            {
                _markDirtyBeforeSaving = false;
                PersistDirectory.MarkDirty(true);
            }

            await PersistDirectory.SaveEntireDirectoryAsync(destination, token: token);
        }

        protected virtual UnityTask<bool> OnTryLoadAsync(
              [NotNull] ILogger logger
            , string id
            , SourcePriority priority
            , SaveDestination destination
            , CancellationToken token
        )
        {
            ThrowHelper.ThrowIfNull(logger);

            return UnityTask.FromResult(true);
        }

        protected virtual void OnDeinitialize() { }

        protected virtual void Dispose(bool disposing) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        private static void LogWarningInvalidId(ILogger logger)
        {
            logger.LogWarning("Persist data cannot be loaded because 'id' is invalid.");

        }
    }
}
