// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.Processing.Internals;
using EncosyTower.Types;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;
using ILogger = EncosyTower.Logging.ILogger;

namespace EncosyTower.Processing
{
    /// <summary>
    /// Provides exception helpers for processing validation.
    /// </summary>
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfUnityObjectScope<TScope>(
              TScope scope
            , [CallerArgumentExpression("scope")] string paramName = null
        )
        {
            if (scope is UnityEngine.Object)
            {
                throw CreateArgumentException_UnityObjectScope(paramName);
            }
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfHandlerIsNotFound<TScope, TRequest>(
              [DoesNotReturnIf(false)] bool found
            , TScope scope
            , bool hasCandidate
            , IProcessHandler<TRequest> _
        )
        {
            if (found == false)
            {
                throw CreateException(scope, hasCandidate);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(TScope scope, bool hasCandidate)
            {
                if (hasCandidate)
                {
                    return new InvalidOperationException(
                        $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                        $"inside the scope `{typeof(TScope)}({scope})`, " +
                        $"but it has an invalid type."
                    );
                }

                return new InvalidOperationException(
                    $"Cannot find any process handler for the request `{typeof(TRequest)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`"
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfHandlerIsNotFound<TScope, TRequest, TResult>(
              [DoesNotReturnIf(false)] bool found
            , TScope scope
            , bool hasCandidate
            , IProcessHandler<TRequest, TResult> _
        )
        {
            if (found == false)
            {
                throw CreateException(scope, hasCandidate);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(TScope scope, bool hasCandidate)
            {
                if (hasCandidate)
                {
                    return new InvalidOperationException(
                        $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                        $"inside the scope `{typeof(TScope)}({scope})`, " +
                        $"but it has an invalid type."
                    );
                }

                return new InvalidOperationException(
                    $"Cannot find any process handler for the request `{typeof(TRequest)}` " +
                    $"which returns a `{typeof(TResult)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`"
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfHandlerIsNotFound<TScope, TRequest>(
              [DoesNotReturnIf(false)] bool found
            , TScope scope
            , bool hasCandidate
            , IAsyncProcessHandler<TRequest> _
        )
        {
            if (found == false)
            {
                throw CreateException(scope, hasCandidate);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(TScope scope, bool hasCandidate)
            {
                if (hasCandidate)
                {
                    return new InvalidOperationException(
                        $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                        $"inside the scope `{typeof(TScope)}({scope})`, " +
                        $"but it has an invalid type."
                    );
                }

                return new InvalidOperationException(
                    $"Cannot find any process handler for the request `{typeof(TRequest)}` which returns " +
                    $"a UnityTask " +
                    $"inside the scope `{typeof(TScope)}({scope})`"
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfHandlerIsNotFound<TScope, TRequest, TResult>(
              [DoesNotReturnIf(false)] bool found
            , TScope scope
            , bool hasCandidate
            , IAsyncProcessHandler<TRequest, TResult> _
        )
        {
            if (found == false)
            {
                throw CreateException(scope, hasCandidate);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(TScope scope, bool hasCandidate)
            {
                if (hasCandidate)
                {
                    return new InvalidOperationException(
                        $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                        $"inside the scope `{typeof(TScope)}({scope})`, " +
                        $"but it has an invalid type."
                    );
                }

                return new InvalidOperationException(
                    $"Cannot find any process handler for the request `{typeof(TRequest)}` which returns " +
                    $"a UnityTask<{typeof(TResult)}>" +
                    $"inside the scope `{typeof(TScope)}({scope})`"
                );
            }
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfHandlerResultIsInvalid([DoesNotReturnIf(false)] bool valid, Type handlerType)
        {
            if (valid == false)
            {
                throw CreateException(handlerType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type handlerType)
                => new($"The process handler of type {handlerType} returned an invalid result.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden, DoesNotReturn]
        internal static void ThrowStateUnavailable(Type stateType)
            => throw new InvalidOperationException(GetStateUnavailableMessage(stateType));

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorInvalidHub<TScope>(ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                "Processor.Hub must be retrieved via " +
                $"`{nameof(Processor)}.{nameof(Processor.Scope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorInvalidHub<TScope, TState>(ILogger logger)
            where TState : class
        {
            (logger ?? DevLogger.Default).LogError(
                "Processor.Hub must be retrieved via " +
                $"`{nameof(Processor)}.{nameof(Processor.Scope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorHandlerNotFound<TScope, TRequest>(
              TScope scope
            , bool hasCandidate
            , IProcessHandler<TRequest> _
            , ILogger logger
        )
        {
            var target = logger ?? DevLogger.Default;

            if (hasCandidate)
            {
                target.LogError(
                    $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`, " +
                    $"but it has an invalid type."
                );
                return;
            }

            target.LogError(
                $"Cannot find any process handler for the request `{typeof(TRequest)}` " +
                $"inside the scope `{typeof(TScope)}({scope})`"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorHandlerNotFound<TScope, TRequest, TResult>(
              TScope scope
            , bool hasCandidate
            , IProcessHandler<TRequest, TResult> _
            , ILogger logger
        )
        {
            var target = logger ?? DevLogger.Default;

            if (hasCandidate)
            {
                target.LogError(
                    $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`, " +
                    $"but it has an invalid type."
                );
                return;
            }

            target.LogError(
                $"Cannot find any process handler for the request `{typeof(TRequest)}` " +
                $"which returns a `{typeof(TResult)}` " +
                $"inside the scope `{typeof(TScope)}({scope})`"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorHandlerNotFound<TScope, TRequest>(
              TScope scope
            , bool hasCandidate
            , IAsyncProcessHandler<TRequest> _
            , ILogger logger
        )
        {
            var target = logger ?? DevLogger.Default;

            if (hasCandidate)
            {
                target.LogError(
                    $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`, " +
                    $"but it has an invalid type."
                );
                return;
            }

            target.LogError(
                $"Cannot find any process handler for the request `{typeof(TRequest)}` which returns " +
                $"a UnityTask " +
                $"inside the scope `{typeof(TScope)}({scope})`"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorHandlerNotFound<TScope, TRequest, TResult>(
              TScope scope
            , bool hasCandidate
            , IAsyncProcessHandler<TRequest, TResult> _
            , ILogger logger
        )
        {
            var target = logger ?? DevLogger.Default;

            if (hasCandidate)
            {
                target.LogError(
                    $"Found a candidate process handler for the request `{typeof(TRequest)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`, " +
                    $"but it has an invalid type."
                );
                return;
            }

            target.LogError(
                $"Cannot find any process handler for the request `{typeof(TRequest)}` which returns " +
                $"a UnityTask<{typeof(TResult)}>" +
                $"inside the scope `{typeof(TScope)}({scope})`"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogWarningHandlerAlreadyRegistered(IProcessHandler handler, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogWarning(
                $"A process handler of type {handler.Id.ToType()} has already been registered."
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PROCESSING_CHECKS)]
        internal static void LogErrorStateUnavailable(Type stateType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(GetStateUnavailableMessage(stateType));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetStateUnavailableMessage(Type stateType)
            => $"The state instance of type {stateType} is not alive anymore.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentException CreateArgumentException_UnityObjectScope(string paramName)
            => new("UnityEngine.Object scopes require the `Processor.UnityScope` API.", paramName);
    }
}
