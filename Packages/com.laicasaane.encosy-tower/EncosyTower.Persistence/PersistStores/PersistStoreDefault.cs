using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Common;
using EncosyTower.Debugging;
using EncosyTower.Encryption;
using EncosyTower.Initialization;
using EncosyTower.StringIds;
using EncosyTower.Tasks;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Persistences
{
    public sealed class PersistStoreDefault<TData> : PersistStoreBase<TData>, IIsInitialized
        where TData : IPersist
    {
        private readonly PersistSourceLocal<TData> _source;
        private readonly Func<TData> _createDataFunc;
        private readonly bool _isValueType;

        private string _id;
        private TData _data;

        public PersistStoreDefault(
              StringId<string> key
            , [NotNull] StringVault stringVault
            , [NotNull] EncryptionBase encryption
            , EncosyTower.Logging.ILogger logger
            , bool ignoreEncryption
            , [NotNull] PersistStoreArgs args
        )
            : base(key, stringVault, encryption, logger, ignoreEncryption, args)
        {
            ThrowHelper.ThrowIfNull(args);

            ThrowIfArgsIsNotInstanceOfType(args is Args);

            var (createDataFunc, sourceArgs) = (Args)args;

            _id = string.Empty;
            _createDataFunc = createDataFunc;
            _isValueType = typeof(TData).IsValueType;
            _source = new PersistSourceLocal<TData>(key, stringVault, encryption, logger, ignoreEncryption, sourceArgs);
        }

        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.IsInitialized;
        }

        public string FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.FilePath;
        }

        public override string Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _id;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.Id = _id = value ?? string.Empty;
        }

        public TData Data
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }

        public bool IsDataValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Data is not null;
        }

        public bool IsDataDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.IsDirty;
        }

        public override void Initialize()
        {
            _source.Initialize();
        }

        public override void Deinitialize()
        {
        }

        public override void CreateData()
        {
            var data = _createDataFunc();
            data.Id = _id ?? string.Empty;
            _data = data;
        }

        public override TData GetData(SourcePriority priority = default)
        {
            return _data;
        }

        /// <summary>
        /// Gets <typeparamref name="TData"/> by reference when it is a value type.
        /// </summary>
        /// <remarks>
        /// Should be used as an optimization when copy-by-value is not desirable.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Throws when <typeparamref name="TData"/> is not a value type.
        /// </exception>
        public ref TData GetDataByRef()
        {
            ThrowInvalidOperationIfNotValueType(_isValueType);
            return ref _data;
        }

        public override void SetData(TData data, bool allowNull = false)
        {
            if (data != null || allowNull)
            {
                _data = data;
                _source.IsDirty = true;
            }
        }

        public override void SetIdAndVersion(string id, int version)
        {
            ref var data = ref _data;
            data.Id = _id = id ?? string.Empty;
            data.Version = version;
        }

        public override void MarkDirty(bool isDirty = true)
        {
            _source.IsDirty = isDirty;
        }

        public override async UnityTask LoadAsync(SourcePriority priority = default, CancellationToken token = default)
        {
            var dataOpt = await _source.TryLoadAsync(token);

            if (dataOpt.TryGetValue(out var data))
            {
                _data = data;
            }
        }

        public override async UnityTask SaveAsync(
              SaveDestination destination = default
            , CancellationToken token = default
        )
        {
            if (IsDataValid && destination.Contains(SaveDestination.Local))
            {
                await _source.SaveAsync(Data, token);
            }
        }

        public override Option<TData> TryCloneData(SourcePriority priority = default)
        {
            return IsDataValid && _source.TryCloneData(GetData(priority)).TryGetValue(out var clone)
                ? Option.Some(clone)
                : Option.None;
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PERSISTENCE_CHECKS)]
        private static void ThrowIfArgsIsNotInstanceOfType([DoesNotReturnIf(false)] bool isInstance)
        {
            if (isInstance == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new($"'args' must be an instance of '{typeof(Args).FullName}'.");
        }

        [UnityEngine.HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PERSISTENCE_CHECKS)]
        private static void ThrowInvalidOperationIfNotValueType(
              [DoesNotReturnIf(false)] bool check
            , [CallerMemberName] string memberName = ""
        )
        {
            if (check == false)
            {
                throw CreateException(memberName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(string memberName)
                => new($"Cannot use 'PersistStoreDefault<{typeof(TData)}>.{memberName}' " +
                    $"because {typeof(TData)} is not a value type."
                );
        }

        public sealed record class Args(
              [NotNull] Func<TData> CreateDataFunc
            , [NotNull] PersistSourceArgs SourceArgs
        ) : PersistStoreArgs
        {
            public Func<TData> CreateDataFunc { get; init; }
                = GetNotNull(CreateDataFunc, nameof(CreateDataFunc));

            public PersistSourceArgs SourceArgs { get; init; }
                = GetNotNull(SourceArgs, nameof(SourceArgs));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Func<TData> GetNotNull(Func<TData> value, string paramName)
            {
                ThrowHelper.ThrowIfNull(value, paramName);
                return value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static PersistSourceArgs GetNotNull(PersistSourceArgs value, string paramName)
            {
                ThrowHelper.ThrowIfNull(value, paramName);
                return value;
            }
        }
    }
}
