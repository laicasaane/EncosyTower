using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.StringIds;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;

namespace EncosyTower.Databases
{
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorAssetIsInvalid(int index, DatabaseAsset context)
        {
            StaticDevLogger.LogError(context, $"Table asset at index {index} is invalid.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void LogWarningAmbiguousTypeAtInitialization(
              int index
            , Type type
            , string name
            , string otherName
            , DatabaseAsset context
        )
        {
            StaticDevLogger.LogWarning(
                  context
                , $"DO NOT use the type '{type}' to get the asset named '{name}' (index {index}) " +
                  $"because that type has already been registered to the asset named '{otherName}'!\n" +
                  $"Please use the name '{name}' to get the correct asset!"
            );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowsDatabaseIsNotInitialized([DoesNotReturnIf(false)] bool initialized)
        {
            if (initialized == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    "The database is not yet initialized. " +
                    "Please call 'Initialize' method once before using."
                );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorCannotFindAsset(StringId id, DatabaseAsset context)
        {
            context.StringVault.TryGetManagedString(id, out var name);
            var info = string.IsNullOrEmpty(name) ? $"id '{id}'" : $"name '{name}'";

            StaticDevLogger.LogError(context, $"Cannot find any table asset by {info}.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorCannotFindAsset(Type type, DatabaseAsset context)
        {
            StaticDevLogger.LogError(context, $"Cannot find any table asset by the type '{type}'.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void LogErrorFoundAssetIsNotValidType<T>(DataTableAssetBase context)
        {
            StaticDevLogger.LogError(context, $"The table asset is not an instance of type '{typeof(T)}'");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void LogWarningAmbiguousTypeAtGetDataTableAsset(
              Type type
            , Dictionary<Type, List<StringId>> typeToIds
            , DatabaseAsset context
        )
        {
            if (typeToIds.TryGetValue(type, out var ids) == false || ids.Count < 2)
            {
                return;
            }

            var id = ids[0];
            context.StringVault.TryGetManagedString(id, out var name);
            var info = string.IsNullOrEmpty(name) ? $"by id '{id}'" : $"named '{name}'";

            StaticDevLogger.LogWarning(
                  context
                , $"It is unreliable to get a table asset by the type '{type}' " +
                  $"because it is the type of multiple assets of different names.\n" +
                  $"The method overload always returns the asset {info} " +
                  $"because it is the first that was registered.\n" +
                  $"Please use the overload that takes asset names into account."
            );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ThrowIfInvalid([DoesNotReturnIf(false)] bool isValid)
        {
            if (isValid == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("DataRef is invalid");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ErrorCannotCast<TData>(object obj, UnityEngine.Object context)
        {
            StaticDevLogger.LogError(context,
                obj == null
                    ? $"Cannot cast null into {typeof(TData[])}"
                    : $"Cannot cast {obj.GetType()} into {typeof(TData[])}"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG), Conditional(RUNTIME_CHECKS)]
        internal static void ErrorDuplicateId(
              string convertedId
            , string id
            , int index
            , [NotNull] UnityEngine.Object context
        )
        {
            StaticDevLogger.LogErrorFormat(
                  context
                , "Id \"{0}\" (converted from \"{1}\") is duplicated at row \"{2}\""
                , convertedId
                , id
                , index
            );
        }
    }
}
