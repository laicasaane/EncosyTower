#pragma warning disable 0219

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__S = global::System;
using g__SC = global::System.Collections;
using g__SCG = global::System.Collections.Generic;
using g__ST = global::System.Threading;
using g__SD = global::System.Diagnostics;
using g__SDCA = global::System.Diagnostics.CodeAnalysis;
using g__SCDC = global::System.CodeDom.Compiler;
using g__SRCS = global::System.Runtime.CompilerServices;
using g__SB = global::System.Buffers;
using g__UE = global::UnityEngine;
using g__ET = global::EncosyTower.Common;
using g__ETP = global::EncosyTower.Persistences;
using g__ETS = global::EncosyTower.StringIds;
using g__ETE = global::EncosyTower.Encryption;
using g__ETC = global::EncosyTower.Collections;
using g__ETDBG = global::EncosyTower.Debugging;
using g__ETDBGVD = global::EncosyTower.Debugging.ValidationDefines;
using g__ETT = global::EncosyTower.Tasks;
using g__ETL = global::EncosyTower.Logging;
using g__ETI = global::EncosyTower.Initialization;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace

namespace TestProject
{


#pragma warning disable

#region    PERSISTENCE
#endregion ===========

    static partial class SavePersistence // Persistence
    {
        /// <summary>
        /// Manages persist stores, accessors, and string ID mappings.
        /// Provides load, save, and lifecycle operations for all persist data associated with an ID.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        internal partial class Persistence : g__ETP.PersistenceBase
        {
            internal readonly PersistDirectory _directory;
            internal readonly AccessorCollection _accessors;
            internal readonly StringIdCollection _stringIds;
            internal readonly string _id;

            internal Persistence(
                  [g__SDCA.NotNull] g__ETS.StringVault stringVault
                , [g__SDCA.NotNull] g__ETE.EncryptionBase encryption
                , [g__SDCA.NotNull] g__ETL.ILogger logger
                , [g__SDCA.NotNull] g__SB.ArrayPool<g__ETT.UnityTask> taskArrayPool
                , string id
            )
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(stringVault);
                g__ETDBG.ThrowHelper.ThrowIfNull(encryption);
                g__ETDBG.ThrowHelper.ThrowIfNull(logger);
                g__ETDBG.ThrowHelper.ThrowIfNull(taskArrayPool);

                _stringIds = new(stringVault);
                _directory = new(stringVault, encryption, logger, taskArrayPool, _stringIds, id);
                _accessors = new(_directory);
                _id = id;
            }

            public StringIdCollection StringIds
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _stringIds;
            }

            public ReadOnlyAccessorCollection Accessors
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _accessors.AsReadOnly();
            }

            public string Id
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _id;
            }

            protected override g__ETP.IPersistDirectory PersistDirectory
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _directory;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            protected sealed override void OnDeinitialize()
            {
                _accessors.Deinitialize();
            }

            protected sealed override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _directory.Dispose();
                }
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            protected sealed override g__ETT.UnityTask<bool> OnTryLoadAsync(
                  [g__SDCA.NotNull] g__ETL.ILogger logger
                , string id
                , g__ETP.SourcePriority priority
                , g__ETP.SaveDestination destination
                , g__ST.CancellationToken token
            )
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(logger);

                _accessors.Initialize();
                return g__ETT.UnityTask.FromResult(true);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public ReadOnlyPersistence AsReadOnly()
            {
                return new ReadOnlyPersistence(this);
            }
        }

    }

#region    READ-ONLY PERSISTENCE
#endregion =====================

    static partial class SavePersistence // ReadOnlyPersistence
    {
        /// <summary>
        /// A read-only view of <see cref="Persistence" /> that exposes safe, non-mutating access
        /// to string IDs, accessors, and save operations.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        public readonly partial struct ReadOnlyPersistence : g__ET.IIsCreated
        {
            internal readonly Persistence _persistence;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal ReadOnlyPersistence([g__SDCA.NotNull] Persistence persistence)
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(persistence);
                _persistence = persistence;
            }

            public bool IsCreated
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _persistence != null;
            }

            public StringIdCollection StringIds
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _persistence.StringIds;
            }

            public ReadOnlyAccessorCollection Accessors
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _persistence.Accessors;
            }

            public string Id
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _persistence.Id;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public g__ETT.UnityTask SaveAsync(g__ETP.SaveDestination destination = default, g__ST.CancellationToken token = default)
            {
                ThrowIfNotCreated(IsCreated);

                return _persistence.SaveAsync(destination, token);
            }

            [g__UE.HideInCallstack, g__SD.StackTraceHidden]
            [g__SD.Conditional(g__ETDBGVD.UNITY_EDITOR), g__SD.Conditional(g__ETDBGVD.DEBUG)]
            [g__SD.Conditional(g__ETDBGVD.RUNTIME_CHECKS), g__SD.Conditional(g__ETDBGVD.PERSISTENCE_CHECKS)]
            private static void ThrowIfNotCreated([g__SDCA.DoesNotReturnIf(false)] bool isCreated)
            {
                if (isCreated == false)
                {
                    throw g__ETDBG.ThrowHelper.CreateInvalidOperationException_TypeNotCreatedCorrectly("SavePersistence+ReadOnlyPersistence");
                }
            }
        }

    }

#region    ID COLLECTION
#endregion =============

    static partial class SavePersistence // StringIdCollection
    {
        /// <summary>
        /// An immutable collection of <see cref="StringId{T}" /> values identifying
        /// each data type stored in the persistence layer.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        public readonly partial struct StringIdCollection : g__ETP.IPersistStringIdCollection, g__ET.IIsCreated
        {
            public readonly g__ETS.StringId<string> SaveData;

            internal StringIdCollection([g__SDCA.NotNull] g__ETS.StringVault stringVault)
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(stringVault);

                SaveData = stringVault.GetOrMakeId(nameof(SaveData));
                IsCreated = true;
            }

            public g__ETS.StringId<string> this[int index] => index switch
            {
                0 => SaveData,
                _ => throw g__ETC.ThrowHelper.CreateIndexOutOfRangeException_Collection()
            };

            public bool IsCreated { get; }

            public int Count
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => 1;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new Enumerator(this);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SCG.IEnumerator<g__ETS.StringId<string>> g__SCG.IEnumerable<g__ETS.StringId<string>>.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SC.IEnumerator g__SC.IEnumerable.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETS.StringId<string>> destination)
                => CopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETS.StringId<string>> destination, int length)
                => CopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETS.StringId<string>> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETS.StringId<string>> destination, int length)
            {
                var count = Count - sourceStartIndex;

                if (length < 0)
                {
                    throw g__ETDBG.ThrowHelper.CreateArgumentOutOfRangeException_LengthNegative();
                }

                if (count < length)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_SourceStartIndex_Length();
                }

                if (destination.Length < length)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_DestinationTooShort();
                }

                destination = destination[..length];

                for (int i = 0; i < length; i++)
                {
                    destination[i] = this[sourceStartIndex + i];
                }
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETS.StringId<string>> destination)
                => TryCopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETS.StringId<string>> destination, int length)
                => TryCopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETS.StringId<string>> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETS.StringId<string>> destination, int length)
            {
                var count = Count - sourceStartIndex;

                if (length < 0 || count < length || destination.Length < length)
                {
                    return false;
                }

                destination = destination[..length];

                for (int i = 0; i < length; i++)
                {
                    destination[i] = this[sourceStartIndex + i];
                }

                return true;
            }

            [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
            public struct Enumerator : g__SCG.IEnumerator<g__ETS.StringId<string>>
            {
                private readonly StringIdCollection _source;
                private int _index;

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public Enumerator(StringIdCollection source)
                {
                    if (source.IsCreated == false)
                    {
                        throw g__ETC.ThrowHelper.CreateArgumentException_CollectionNotCreated("source");
                    }

                    _source = source;
                    _index = -1;
                }

                public readonly g__ETS.StringId<string> Current => _source[_index];

                readonly object g__SC.IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    _index++;
                    return (uint)_index < (uint)_source.Count;
                }

                public void Reset()
                {
                    _index = -1;
                }

            }
        }

    }

#region    ACCESSOR COLLECTION
#endregion ===================

    static partial class SavePersistence // AccessorCollection
    {
        /// <summary>
        /// Holds all typed persist accessors and manages their initialization and deinitialization lifecycle.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        internal partial class AccessorCollection : g__ETP.IPersistAccessorCollection
        {
            internal AccessorCollection([g__SDCA.NotNull] PersistDirectory directory)
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(directory);

                SaveAccessor = new(directory.SaveData);

            }

            public g__ETP.IPersistAccessor this[int index] => index switch
            {
                0 => SaveAccessor,
                _ => throw g__ETC.ThrowHelper.CreateIndexOutOfRangeException_Collection()
            };

            public int Count
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => 1;
            }

            public global::TestProject.SaveAccessor SaveAccessor { get; }

            public void Initialize()
            {
            }

            public void Deinitialize()
            {
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public AccessorEnumerator GetEnumerator()
                => new AccessorEnumerator(new ReadOnlyAccessorCollection(this));

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SCG.IEnumerator<g__ETP.IPersistAccessor> g__SCG.IEnumerable<g__ETP.IPersistAccessor>.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SC.IEnumerator g__SC.IEnumerable.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETP.IPersistAccessor> destination)
                => CopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETP.IPersistAccessor> destination, int length)
                => CopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination, int length)
            {
                var count = Count - sourceStartIndex;

                if (length < 0)
                {
                    throw g__ETDBG.ThrowHelper.CreateArgumentOutOfRangeException_LengthNegative();
                }

                if (count < length)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_SourceStartIndex_Length();
                }

                if (destination.Length < length)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_DestinationTooShort();
                }

                destination = destination[..length];

                for (int i = 0; i < length; i++)
                {
                    destination[i] = this[sourceStartIndex + i];
                }
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETP.IPersistAccessor> destination)
                => TryCopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETP.IPersistAccessor> destination, int length)
                => TryCopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination, int length)
            {
                var count = Count - sourceStartIndex;

                if (length < 0 || count < length || destination.Length < length)
                {
                    return false;
                }

                destination = destination[..length];

                for (int i = 0; i < length; i++)
                {
                    destination[i] = this[sourceStartIndex + i];
                }

                return true;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public ReadOnlyAccessorCollection AsReadOnly()
            {
                return new ReadOnlyAccessorCollection(this);
            }
        }

    }

#region    READ-ONLY ACCESSOR COLLECTION
#endregion =============================

    static partial class SavePersistence // ReadOnlyAccessorCollection
    {
        /// <summary>
        /// A read-only view of <see cref="AccessorCollection" /> that provides immutable,
        /// enumerable access to all typed persist accessors.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        public readonly partial struct ReadOnlyAccessorCollection : g__ETP.IPersistAccessorReadOnlyCollection, g__ET.IIsCreated
        {
            internal readonly AccessorCollection _accessors;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal ReadOnlyAccessorCollection([g__SDCA.NotNull] AccessorCollection accessors)
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(accessors);
                _accessors = accessors;
            }

            public bool IsCreated
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _accessors != null;
            }

            public g__ETP.IPersistAccessor this[int index] => index switch
            {
                0 => SaveAccessor,
                _ => throw g__ETC.ThrowHelper.CreateIndexOutOfRangeException_Collection()
            };

            public int Count
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => 1;
            }

            public global::TestProject.SaveAccessor SaveAccessor
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _accessors.SaveAccessor;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public AccessorEnumerator GetEnumerator()
                => new AccessorEnumerator(this);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SCG.IEnumerator<g__ETP.IPersistAccessor> g__SCG.IEnumerable<g__ETP.IPersistAccessor>.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SC.IEnumerator g__SC.IEnumerable.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETP.IPersistAccessor> destination)
                => CopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETP.IPersistAccessor> destination, int length)
                => CopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination, int length)
                => _accessors.CopyTo(sourceStartIndex, destination, length);
            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETP.IPersistAccessor> destination)
                => TryCopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETP.IPersistAccessor> destination, int length)
                => TryCopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersistAccessor> destination, int length)
                => _accessors.TryCopyTo(sourceStartIndex, destination, length);

        }

    }

#region    ACCESSOR ENUMERATOR
#endregion ===================

    static partial class SavePersistence // AccessorEnumerator
    {
        /// <summary>
        /// Provides forward-only iteration over the <see cref="g__ETP.IPersistAccessor" /> elements
        /// held in a <see cref="ReadOnlyAccessorCollection" />.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        public partial struct AccessorEnumerator : g__SCG.IEnumerator<g__ETP.IPersistAccessor>
        {
            private readonly ReadOnlyAccessorCollection _source;
            private int _index;

            internal AccessorEnumerator(ReadOnlyAccessorCollection source)
            {
                if (source.IsCreated == false)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_CollectionNotCreated("source");
                }

                _source = source;
                _index = -1;
            }

            public readonly g__ETP.IPersistAccessor Current => _source[_index];

            readonly object g__SC.IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                _index++;
                return (uint)_index < (uint)_source.Count;
            }

            public void Reset()
            {
                _index = -1;
            }

        }

    }

#region    PERSIST DIRECTORY
#endregion =================

    static partial class SavePersistence // PersistDirectory
    {
        /// <summary>
        /// Manages the underlying persist stores for all persist data types, coordinating
        /// load, save, and clone operations across the stores belonging to a single ID.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        internal partial class PersistDirectory : g__ETP.IPersistDirectory, g__S.IDisposable
        {
            private readonly g__ETS.StringVault _stringVault;
            private readonly g__ETE.EncryptionBase _encryption;
            private readonly g__ETL.ILogger _logger;
            private readonly g__SB.ArrayPool<g__ETT.UnityTask> _taskArrayPool;
            private readonly StringIdCollection _stringIds;

            private string _id;

            internal PersistDirectory(
                  [g__SDCA.NotNull] g__ETS.StringVault stringVault
                , [g__SDCA.NotNull] g__ETE.EncryptionBase encryption
                , [g__SDCA.NotNull] g__ETL.ILogger logger
                , [g__SDCA.NotNull] g__SB.ArrayPool<g__ETT.UnityTask> taskArrayPool
                , StringIdCollection stringIds
                , string id
            )
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(stringVault);
                g__ETDBG.ThrowHelper.ThrowIfNull(encryption);
                g__ETDBG.ThrowHelper.ThrowIfNull(logger);
                g__ETDBG.ThrowHelper.ThrowIfNull(taskArrayPool);

                _stringVault = stringVault;
                _encryption = encryption;
                _logger = logger;
                _taskArrayPool = taskArrayPool;
                _stringIds = stringIds;
                _id = id;

                bool ignoreEncryption = false;

#if !FORCE_PERSIST_ENCRYPTION
                GetIgnoreEncryption(ref ignoreEncryption);
#endif

                {
                    g__S.Func<global::TestProject.SaveData> createFunc = static () => new global::TestProject.SaveData();
                    var args = GetStoreArgs<global::TestProject.SaveData, global::EncosyTower.Persistences.PersistStoreDefault<global::TestProject.SaveData>>(createFunc);
                    SaveData = new(_stringIds.SaveData, stringVault, encryption, logger, ignoreEncryption, args) { Id = id };
                }
            }

            public global::EncosyTower.Persistences.PersistStoreDefault<global::TestProject.SaveData> SaveData { get; }

            public string Id
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get
                {
                    return _id;
                }

                set
                {
                    _id = value;

                    SaveData.Id = value;
                }
            }

            static partial void GetIgnoreEncryption(ref bool ignoreEncryption);

            private static partial g__ETP.PersistStoreArgs GetStoreArgs<TData, TStore>(g__S.Func<TData> createDataFunc)
                where TData : g__ETP.IPersist
                where TStore : g__ETP.PersistStoreBase<TData>;

            public void Initialize()
            {
                SaveData.Initialize();
            }

            public void Deinitialize()
            {
                SaveData.Deinitialize();
            }

            public void Dispose()
            {
                _encryption.Dispose();
            }

            public void CreateDataIfNotExist()
            {
                if (SaveData.IsDataValid == false)
                {
                    SaveData.CreateData();
                }

            }

            public void CreateData()
            {
                SaveData.CreateData();
            }

            public void MarkDirty(bool isDirty)
            {
                SaveData.MarkDirty(isDirty);
            }

            public void SetIdAndVersion(
                  string id
                , int version
                , bool includeSaveData = true
            )
            {
                if (includeSaveData)
                {
                    SaveData.SetIdAndVersion(id, version);
                }

            }

            public g__ETT.UnityTask LoadEntireDirectoryAsync(g__ETP.SourcePriority priority = default, g__ST.CancellationToken token = default)
                => LoadAsync(priority, token);

            public async g__ETT.UnityTask LoadAsync(
                  g__ETP.SourcePriority priority
                , g__ST.CancellationToken token = default
                , bool includeSaveData = true
            )
            {
                if (includeSaveData)
                {
                    await SaveData.LoadAsync(priority, token);
                }

            }

            public g__ETT.UnityTask SaveEntireDirectoryAsync(g__ETP.SaveDestination destination = default, g__ST.CancellationToken token = default)
                => SaveAsync(destination, token);

            public async g__ETT.UnityTask SaveAsync(
                  g__ETP.SaveDestination destination = default
                , g__ST.CancellationToken token = default
                , bool includeSaveData = true
            )
            {
                if (includeSaveData)
                {
                    await SaveData.SaveAsync(destination, token);
                }

            }

        }

    }

#region    PERSIST COLLECTION
#endregion ==================

    static partial class SavePersistence // PersistCollection
    {
        /// <summary>
        /// A serializable, value-type snapshot of all persist data instances belonging to a single ID.
        /// Supports copying to and from the <see cref="PersistDirectory" />.
        /// </summary>
        [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
        [g__S.Serializable]
        public partial struct PersistCollection : g__ETP.IPersistCollection, g__ET.IIsCreated
        {
            [g__UE.SerializeField] internal string _id;
            [g__UE.SerializeField] internal global::TestProject.SaveData _saveData;

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public PersistCollection(
                  [g__SDCA.NotNull] string id
                , PersistCollection source
            )
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(id);

                if (source.IsCreated == false)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_CollectionNotCreated("source");
                }

                _id = id;
                _saveData = source._saveData;

                IsCreated = true;
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            internal PersistCollection(
                  [g__SDCA.NotNull] string id
                , global::TestProject.SaveData saveData
            )
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(id);

                _id = id;
                _saveData = saveData;

                IsCreated = true;
            }

            public g__ETP.IPersist this[int index] => index switch
            {
                0 => _saveData,
                _ => throw g__ETC.ThrowHelper.CreateIndexOutOfRangeException_Collection()
            };

            public bool IsCreated { get; }

            public int Count
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => 1;
            }

            public string Id
            {
                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                get => _id;
            }

            public static PersistCollection GetFrom(ReadOnlyPersistence persistence, g__ETP.SourcePriority priority = default)
            {
                if (persistence.IsCreated == false)
                {
                    throw g__ETDBG.ThrowHelper.CreateArgumentNullException("persistence");
                }

                var directory = persistence._persistence._directory;

                return new PersistCollection(
                      directory.Id
                    , directory.SaveData.GetData(priority)
                );
            }

            public static PersistCollection CloneFrom(ReadOnlyPersistence persistence, g__ETP.SourcePriority priority = default)
            {
                if (persistence.IsCreated == false)
                {
                    throw g__ETDBG.ThrowHelper.CreateArgumentNullException("persistence");
                }

                var directory = persistence._persistence._directory;

                return new PersistCollection(
                      directory.Id
                    , directory.SaveData.TryCloneData(priority).GetValueOrDefault()
                );
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public PersistCollection ChangeId([g__SDCA.NotNull] string id)
            {
                g__ETDBG.ThrowHelper.ThrowIfNull(id);
                return new PersistCollection(id, this);
            }

            public void SetTo(ReadOnlyPersistence persistence, bool allowNull = false)
            {
                if (IsCreated == false)
                {
                    throw g__ETC.ThrowHelper.CreateInvalidOperationException_CollectionNotCreated();
                }

                if (persistence.IsCreated == false)
                {
                    throw g__ETDBG.ThrowHelper.CreateArgumentNullException("persistence");
                }

                var directory = persistence._persistence._directory;

                directory.SaveData.SetData(_saveData, allowNull);
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new Enumerator(this);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SCG.IEnumerator<g__ETP.IPersist> g__SCG.IEnumerable<g__ETP.IPersist>.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            g__SC.IEnumerator g__SC.IEnumerable.GetEnumerator()
                => GetEnumerator();

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETP.IPersist> destination)
                => CopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(g__S.Span<g__ETP.IPersist> destination, int length)
                => CopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersist> destination)
                => CopyTo(sourceStartIndex, destination, destination.Length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public void CopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersist> destination, int length)
            {
                var count = Count - sourceStartIndex;

                if (length < 0)
                {
                    throw g__ETDBG.ThrowHelper.CreateArgumentOutOfRangeException_LengthNegative();
                }

                if (count < length)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_SourceStartIndex_Length();
                }

                if (destination.Length < length)
                {
                    throw g__ETC.ThrowHelper.CreateArgumentException_DestinationTooShort();
                }

                destination = destination[..length];

                for (int i = 0; i < length; i++)
                {
                    destination[i] = this[sourceStartIndex + i];
                }
            }

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETP.IPersist> destination)
                => TryCopyTo(0, destination);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(g__S.Span<g__ETP.IPersist> destination, int length)
                => TryCopyTo(0, destination, length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersist> destination)
                => TryCopyTo(sourceStartIndex, destination, destination.Length);

            [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
            public bool TryCopyTo(int sourceStartIndex, g__S.Span<g__ETP.IPersist> destination, int length)
            {
                var count = Count - sourceStartIndex;

                if (length < 0 || count < length || destination.Length < length)
                {
                    return false;
                }

                destination = destination[..length];

                for (int i = 0; i < length; i++)
                {
                    destination[i] = this[sourceStartIndex + i];
                }

                return true;
            }

            [g__SDCA.ExcludeFromCodeCoverage][g__SCDC.GeneratedCode("EncosyTower.Persistence.Generators.PersistenceGenerator", "0.1.7-preview.3")]
            public struct Enumerator : g__SCG.IEnumerator<g__ETP.IPersist>
            {
                private readonly PersistCollection _source;
                private int _index;

                [g__SRCS.MethodImpl(g__SRCS.MethodImplOptions.AggressiveInlining)]
                public Enumerator(PersistCollection source)
                {
                    if (source.IsCreated == false)
                    {
                        throw g__ETC.ThrowHelper.CreateArgumentException_CollectionNotCreated("source");
                    }

                    _source = source;
                    _index = -1;
                }

                public readonly g__ETP.IPersist Current => _source[_index];

                readonly object g__SC.IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    _index++;
                    return (uint)_index < (uint)_source.Count;
                }

                public void Reset()
                {
                    _index = -1;
                }

            }
        }
    }

#region    INTERNALS
#endregion =========

    static partial class SavePersistence // Internals
    {
        private const string GENERATOR = "EncosyTower.Persistence.Generators.PersistenceGenerator";

        [g__UE.HideInCallstack, g__SD.StackTraceHidden]
        private static void LogErrorCyclicDependency(g__ETL.ILogger logger, string name)
        {
            logger.LogError($"Detect cyclic dependency in the constructor of type '{name}'");

        }

    }



}
