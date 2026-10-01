// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Collections
{
    /// <summary>
    /// Provides specialized collection validation and generated-code exception factories.
    /// </summary>
    /// <remarks>
    /// Conditional guards use the collection symbols declared by
    /// <see cref="EncosyTower.Debugging.ValidationDefines"/>.
    /// </remarks>
    public static class ThrowHelper
    {
        internal enum CollectionType
        {
            Unknown = 0,
            ArrayMap,
            ArrayMapNative,
            ArrayMapUnsafe,
            ArrayMapUnsafeReadOnly,
            ArraySet,
            ArraySetNative,
            ArraySetUnsafe,
            ArraySetUnsafeReadOnly,
            ArrayUnsafe,
            ListFast,
            ListNative,
            ListNativeReadOnly,
            ListProxy,
            ListProxyReadOnly,
            ListUnsafe,
            ListUnsafeReadOnly,
            ReferenceUnsafe,
            SharedArray,
            SharedArrayMap,
            SharedArrayMapReadOnly,
            SharedArrayMapNative,
            SharedArrayMapNativeReadOnly,
            SharedArrayMapUnsafe,
            SharedList,
            SharedListWithNative,
            SharedListWithNativeReadOnly,
            SharedListNative,
            SharedListNativeReadOnly,
            SharedListUnsafe,
            SharedQueue,
            SharedQueueUnsafe,
            SharedStack,
            SharedStackUnsafe,
            QueueUnsafe,
            StackUnsafe,
            SharedArraySet,
            SharedArrayNative,
            SharedArrayReadOnly,
            SharedArrayNativeReadOnly,
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetCollectionTypeName(CollectionType type)
            => type switch {
                CollectionType.ArrayMap => "ArrayMap<TKey, TValue>",
                CollectionType.ArrayMapNative => "ArrayMapNative<TKey, TValue>",
                CollectionType.ArrayMapUnsafe => "ArrayMapUnsafe<TKey, TValue>",
                CollectionType.ArrayMapUnsafeReadOnly => "ArrayMapUnsafe<TKey, TValue>.ReadOnly",
                CollectionType.ArraySet => "ArraySet<T>",
                CollectionType.ArraySetNative => "ArraySetNative<T>",
                CollectionType.ArraySetUnsafe => "ArraySetUnsafe<T>",
                CollectionType.ArraySetUnsafeReadOnly => "ArraySetUnsafe<T>.ReadOnly",
                CollectionType.ArrayUnsafe => "ArrayUnsafe<T>",
                CollectionType.ListFast => "ListFast<T>",
                CollectionType.ListNative => "ListNative<T>",
                CollectionType.ListNativeReadOnly => "ListNative<T>.ReadOnly",
                CollectionType.ListProxy => "ListProxy<TProvider, TBuffer, T>",
                CollectionType.ListProxyReadOnly => "ListProxy<TProvider, TBuffer, T>.ReadOnly",
                CollectionType.ListUnsafe => "ListUnsafe<T>",
                CollectionType.ListUnsafeReadOnly => "ListUnsafe<T>.ReadOnly",
                CollectionType.ReferenceUnsafe => "ReferenceUnsafe<T>",
                CollectionType.SharedArray => "SharedArray<T, TNative>",
                CollectionType.SharedArrayNative => "SharedArrayNative<T>",
                CollectionType.SharedArrayReadOnly => "SharedArray<T, TNative>.ReadOnly",
                CollectionType.SharedArrayNativeReadOnly => "SharedArrayNative<T>.ReadOnly",
                CollectionType.SharedArrayMap => "SharedArrayMap<TKey, TValue, TValueNative>",
                CollectionType.SharedArrayMapReadOnly => "SharedArrayMap<TKey, TValue, TValueNative>.ReadOnly",
                CollectionType.SharedArrayMapNative => "SharedArrayMapNative<TKey, TValue>",
                CollectionType.SharedArrayMapNativeReadOnly => "SharedArrayMapNative<TKey, TValue>.ReadOnly",
                CollectionType.SharedArrayMapUnsafe => "SharedArrayMapUnsafe<TKey, TValue>",
                CollectionType.SharedArraySet => "SharedArraySet<T>",
                CollectionType.SharedList => "SharedList<T>",
                CollectionType.SharedListWithNative => "SharedList<T, TNative>",
                CollectionType.SharedListWithNativeReadOnly => "SharedList<T, TNative>.ReadOnly",
                CollectionType.SharedListNative => "SharedListNative<T>",
                CollectionType.SharedListNativeReadOnly => "SharedListNative<T>.ReadOnly",
                CollectionType.SharedListUnsafe => "SharedListUnsafe<T>",
                CollectionType.SharedQueue => "SharedQueue<T, TNative>",
                CollectionType.SharedQueueUnsafe => "SharedQueueUnsafe<T>",
                CollectionType.SharedStack => "SharedStack<T, TNative>",
                CollectionType.SharedStackUnsafe => "SharedStackUnsafe<T>",
                CollectionType.QueueUnsafe => "QueueUnsafe<T>",
                CollectionType.StackUnsafe => "StackUnsafe<T>",
                _ => "collection",
            };

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfTypesNotEqualSize<T, U>([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"size of type '{typeof(U).FullName}' must be equal to size of type '{typeof(T).FullName}'");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfBucketsAreUninitialized([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.ArraySet => "Set arrays are not correctly initialized (0 size).",
                    CollectionType.ArraySetUnsafe => "Set arrays are not correctly initialized (0 size).",
                    CollectionType.ArraySetUnsafeReadOnly => "Set arrays are not correctly initialized (0 size).",
                    CollectionType.SharedArrayMap => "Map arrays are not correctly initialized (0 size)",
                    CollectionType.SharedArrayMapNative => "Map arrays are not correctly initialized (0 size)",
                    _ => "Map arrays are not correctly initialized (0 size).",
                });
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfSourceCollectionIsNotCreated(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException(CollectionType type)
                => new(
                    type switch {
                        CollectionType.ArraySet => "The source set is not created.",
                        CollectionType.ArraySetNative => "The source set is not created.",
                        CollectionType.ArraySetUnsafe => "The source set is not created.",
                        _ => "The source map is not created.",
                    },
                    "source"
                );
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfNativeSourceCollectionIsNotCreated(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.ArraySetNative => "The source set is not created.",
                    _ => "The source map is not created.",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCapacityIsImmutable([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"the capacity of {GetCollectionTypeName(type)} is immutable and cannot change");
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfUnsafeCollectionIsDisposed([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ObjectDisposedException CreateException(CollectionType type)
                => new(null, $"The {GetCollectionTypeName(type)} is already disposed.");
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfUnsafeCollectionAllocatorIsInvalid(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(
                    $"The {GetCollectionTypeName(type)} can not be Disposed because it was not allocated "
                    + "with a valid allocator."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfUnsafeCollectionTypeIsManaged<T>(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(
                    $"{typeof(T)} used in {GetCollectionTypeName(type)} must be unmanaged "
                    + "(contain no managed types)."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfNativeAliasTypesHaveDifferentSize<T, TNative>(
            [DoesNotReturnIf(false)] bool valid
        )
            where T : unmanaged
            where TNative : unmanaged
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    $"size of native alias type '{typeof(TNative).FullName}' " +
                    $"({UnsafeUtility.SizeOf<TNative>()} bytes) " +
                    $"must be equal to size of source type '{typeof(T).FullName}' ({UnsafeUtility.SizeOf<T>()} bytes)"
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfAllocatorIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("allocator is invalid");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCapacityIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("capacity must be non-negative");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCapacityBelowCount([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("new capacity cannot be smaller than count");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSourceStartIndexIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("sourceStartIndex is outside the range of valid indexes");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSourceLengthIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("length is outside the source range");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSourceCountIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("count is outside the source range");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfDestinationLengthIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("destination is shorter than the requested length");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfIndexIsNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("index is less than 0");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCountIsNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("count is less than 0");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfArgumentIndexIsNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException()
                => new("index", "Index must be non-negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfArgumentCountIsNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException()
                => new("count", "Count must be non-negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfIndexIsNonNegative(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.SharedArrayReadOnly => "Index is less than 0.",
                    CollectionType.SharedArrayNativeReadOnly => "Index is less than 0.",
                    _ => "Index must be non-negative.",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCountIsNonNegative(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.SharedArrayReadOnly => "Count is less than 0.",
                    CollectionType.SharedArrayNativeReadOnly => "Count is less than 0.",
                    _ => "Count must be non-negative.",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfRangeIsWithinArray(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"Index and count do not denote a valid range in {GetCollectionTypeName(type)}.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSectionIsWithinArray(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"Index and count do not specify a valid section in {GetCollectionTypeName(type)}.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfOffsetLengthIsValid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Offset and length do not specify a valid range.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfRemovalIndexIsOutOfRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("out of bound index");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfRemovalRangeIsOutOfRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("out of bound length");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfStartIndexIsOutOfRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("out of bound start index");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfNewCapacityIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("newSize is not greater than the current capacity");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfAmountIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Amount must be greater than 0.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfTypesHaveDifferentSize([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("T and U must have equal size");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfEmpty([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.ListUnsafe => "the list is empty",
                    CollectionType.QueueUnsafe => "the queue is empty",
                    CollectionType.SharedQueue => "the queue is empty",
                    CollectionType.SharedQueueUnsafe => "the queue is empty",
                    CollectionType.StackUnsafe => "the stack is empty",
                    CollectionType.SharedStack => "the stack is empty",
                    CollectionType.SharedStackUnsafe => "the stack is empty",
                    _ => $"the {GetCollectionTypeName(type)} is empty",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfIndexIsOutOfRange([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.SharedArray => "index is outside the range of valid indices for the SharedArray<T>",
                    CollectionType.SharedListWithNativeReadOnly =>
                        "index is outside the range of valid indices for the SharedList<T>.ReadOnly",
                    _ => $"index is outside the range of valid indexes for the {GetCollectionTypeName(type)}",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfInsertionIndexIsOutOfRange([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"index is outside the range of valid indexes for the {GetCollectionTypeName(type)}");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfIndexSectionIsInvalid([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"index and count do not specify a valid section in the {GetCollectionTypeName(type)}");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfFindStartIndexIsOutOfRange([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"startIndex is outside the range of valid indexes for the {GetCollectionTypeName(type)}");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfFindSectionIsInvalid([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new($"startIndex and count do not specify a valid section in the {GetCollectionTypeName(type)}");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfNotUnmanagedType<T>([DoesNotReturnIf(false)] bool isUnmanaged)
        {
            if (isUnmanaged == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new($"{typeof(T)} is not an unmanaged type. Only unmanaged type is supported.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfIndexOutOfRangeException([DoesNotReturnIf(false)] bool withinRange)
        {
            if (withinRange == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static IndexOutOfRangeException CreateException()
                => new("Index must be non-negative and less than the size of the buffer.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowArgumentOutOfRangeException_IfNegative(int value, string paramName)
        {
            if (value < 0)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName)
                => new(paramName, "The value must be non-negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowArgumentOutOfRangeException_IfNegativeZero(int value, string paramName)
        {
            if (value <= 0)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName)
                => new(paramName, "The value must be positive and non-zero.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfArrayIndexIsOutOfRange(
              [DoesNotReturnIf(false)] bool valid
            , string paramName
        )
        {
            if (valid == false)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(string paramName)
                => new(paramName);
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfDestinationArrayIsTooSmall([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => Debugging.ThrowHelper.CreateArgumentException_ArrayPlusOffTooSmall();
        }

        [StackTraceHidden, HideInCallstack, DoesNotReturn]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowArgumentException_ArrayPlusOffTooSmall()
            => throw Debugging.ThrowHelper.CreateArgumentException_ArrayPlusOffTooSmall();

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowInvalidOperationException_ReadOnlyCollectionNotCreated(
            [DoesNotReturnIf(false)] bool isCreated
        )
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The read-only collection is not created.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCollectionWasModified([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Collection was modified after the enumerator was instantiated.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfCollectionWasModified(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.ListFast => "An element in the collection has been modified.",
                    CollectionType.SharedArray => "SharedArray was modified during enumeration.",
                    _ => "Collection was modified after the enumerator was instantiated.",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfEnumeratorOperationIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Enumeration has either not started or has already finished.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfEnumeratorOperationIsInvalid(
              [DoesNotReturnIf(false)] bool valid
            , CollectionType type
        )
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.SharedArray => "Invalid enumerator state: enumeration cannot proceed.",
                    _ => "Enumeration has either not started or has already finished.",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfEnumeratorIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Enumerator is not retrieved via a valid method.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfKeyIsPresent([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Key already present");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSerializedItemIsDuplicate(
              bool valid
            , CollectionType collectionType
            , int serializedIndex
        )
        {
            if (valid == false)
            {
                ReportWarning(collectionType, serializedIndex);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ReportWarning(
                  CollectionType collectionType
                , int serializedIndex
            )
            {
                StaticLogger.LogWarning(
                    $"Duplicate item at serialized index {serializedIndex} was ignored while deserializing "
                    + $"{GetCollectionTypeName(collectionType)}. The first occurrence was kept."
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfMapIsBeingIterated([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot modify a map while it is being iterated");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSetIsBeingIterated([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot modify a set while it is being iterated");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSetCountExceedsStartingCount([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Cannot set a count greater than the starting one");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfKeyIsNotFound([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static KeyNotFoundException CreateException()
                => new("Key not found");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfMissingLinkedListNode([DoesNotReturnIf(false)] bool valid, CollectionType type)
        {
            if (valid == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(CollectionType type)
                => new(type switch {
                    CollectionType.ArrayMap => "The linked-list successor is missing.",
                    CollectionType.ArrayMapUnsafe => "The linked-list successor is missing.",
                    CollectionType.ArraySet => "The linked-list successor is missing.",
                    CollectionType.ArraySetUnsafe => "The linked-list successor is missing.",
                    CollectionType.SharedArrayMap => "This should never happen",
                    CollectionType.SharedArrayMapUnsafe => "This should never happen",
                    _ => "The linked-list successor is missing.",
                });
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfIndexesAreDifferent([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Indexes must be different.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfDepthLimitIsNonNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Depth limit must be non-negative.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfKeysMeetPartitionThreshold([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Key count must meet the introsort partition threshold.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSortIndexIsNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("'index' must be non-negative number");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSortCountIsNegative([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("'count' must be non-negative number");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSortRangeIsInvalid([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Invalid offset length");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfProviderIsNull([DoesNotReturnIf(false)] bool isInitialized)
        {
            if (isInitialized == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("ListProxy<TProvider, TBuffer, T> is not initialized");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfArrayLengthMismatch(
              [DoesNotReturnIf(false)] bool valid
            , int arrayLength
            , int length
        )
        {
            if (valid == false)
            {
                throw CreateException(arrayLength, length);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException(int arrayLength, int length)
                => new($"array.Length ({arrayLength}) does not match the Length of this instance ({length}).", "array");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceWithStrideOffsetAndSizeExceeded([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("SliceWithStride sizeof(U) + offset must be <= sizeof(T)", "offset");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceWithStrideOffsetIsOutOfRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException()
                => new("offset", "SliceWithStride offset must be >= 0");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceConvertSizeMismatch([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("SliceConvert requires that Length * sizeof(T) is a multiple of sizeof(U).");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceConvertOnRestrictedRange([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("SliceConvert may not be used on a restricted range array");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceConvertStrideMismatch([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("SliceConvert requires that stride matches the size of the source type");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceOnRestrictedRange(
              [DoesNotReturnIf(false)] bool valid
            , string paramName
        )
        {
            if (valid == false)
            {
                throw CreateException(paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException(string paramName)
                => new($"Slice may not be used on a restricted range {paramName}", paramName);
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceRangeExceedsLength(
              [DoesNotReturnIf(false)] bool valid
            , int sourceLength
            , int start
            , int length
            , string paramName
        )
        {
            if (valid == false)
            {
                throw CreateException(sourceLength, start, length, paramName);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException(
                  int sourceLength
                , int start
                , int length
                , string paramName
            )
                => new(
                    $"Slice start + length ({start + length}) range must be <= " +
                    $"{paramName}.Length ({sourceLength})"
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceLengthIsNegative(
              [DoesNotReturnIf(false)] bool valid
            , int length
        )
        {
            if (valid == false)
            {
                throw CreateException(length);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(int length)
                => new("length", $"Slice length {length} < 0.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceStartIsNegative(
              [DoesNotReturnIf(false)] bool valid
            , int start
        )
        {
            if (valid == false)
            {
                throw CreateException(start);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException(int start)
                => new("start", $"Slice start {start} < 0.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSliceIntegerOverflow([DoesNotReturnIf(false)] bool valid)
        {
            if (valid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new("Slice start + length ({start + length}) causes an integer overflow");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfSizeNegative([DoesNotReturnIf(false)] bool isZeroOrPositive)
        {
            if (isZeroOrPositive == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("size must be equal or greater than 0");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfFailedToAllocate([DoesNotReturnIf(false)] bool success)
        {
            if (success == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("Failed to allocate.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfByteCountIsNegative([DoesNotReturnIf(true)] bool isNegative, long size)
        {
            if (isNegative)
            {
                throw CreateException(size);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(long size)
                => new($"Attempted to operate on {size} bytes of memory: negative size.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfByteCountExceedsMaximum([DoesNotReturnIf(true)] bool exceedsMaximum, long size)
        {
            if (exceedsMaximum)
            {
                throw CreateException(size);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(long size)
                => new($"Attempted to operate on {size} bytes of memory: size too big.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfAllocatorNotSupported([DoesNotReturnIf(false)] bool isSupported)
        {
            if (isSupported == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentException CreateException()
                => new(
                    "Allocator strategy must resolve to Temp, TempJob, Persistent or a valid custom allocator",
                    "allocator"
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfAllocateLengthNegative([DoesNotReturnIf(false)] bool isZeroOrPositive)
        {
            if (isZeroOrPositive == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentOutOfRangeException CreateException()
                => new("length", "Length must be >= 0");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfReferenceUnsafeSourceIsNotCreated([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The source UnsafeReference is not created.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfReferenceUnsafeDestinationIsNotCreated([DoesNotReturnIf(false)] bool isCreated)
        {
            if (isCreated == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("The destination UnsafeReference is not created.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
        internal static void ThrowIfReferenceUnsafeIndexIsOutOfRange(int index)
        {
            if (index != 0)
            {
                throw CreateException(index);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static IndexOutOfRangeException CreateException(int index)
                => new($"Index {index} is out of range of the UnsafeReference which only contains 1 element.");
        }

        [HideInCallstack, StackTraceHidden]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(COLLECTIONS_CHECKS)]
        [Conditional(UNITY_COLLECTIONS_CHECKS)]
#if UNITY_BURST
        [Unity.Burst.BurstDiscard]
#endif
        internal static void LogWarningIfHashCodeIsNotImplemented<T>()
        {
            try
            {
                var type = typeof(T);
                var method = type.GetMethod(
                      "GetHashCode"
                    , BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                );

                if (method == null)
                {
                    StaticDevLogger.LogWarning(
                          type.Name
                        + " does not implement GetHashCode and will potentially cause unwanted allocations (boxing)"
                    );
                }
            }
            catch
            {
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IndexOutOfRangeException CreateIndexOutOfRangeException_Collection()
            => new("Index was out of range. Must be non-negative and less than the size of the collection.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException CreateInvalidOperationException_CollectionNotCreated()
            => new("Collection was not created.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_CollectionNotCreated(string paramName)
            => new("Collection was not created.", paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_SourceStartIndex_Length()
            => new(
                "The number of elements from 'sourceStartIndex' to the end of the collection "
                + "is lesser than 'length'."
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException CreateArgumentException_DestinationTooShort()
            => new("The destination span is too short to copy the requested number of elements.");

    }
}
