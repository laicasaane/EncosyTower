// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.PubSub.Internals;
using UnityEngine;

using static EncosyTower.Debugging.ValidationDefines;
using ILogger = EncosyTower.Logging.ILogger;

namespace EncosyTower.PubSub
{
    /// <summary>
    /// Provides exception helpers for PubSub validation.
    /// </summary>
    internal static class ThrowHelper
    {
        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfFailedToRegisterBroker<TMessage>([DoesNotReturnIf(false)] bool succeeded)
        {
            if (succeeded == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new(
                    $"Failed to register interceptor broker for message type '{typeof(TMessage)}'. " +
                    $"This should never happen!"
                );
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
                => new($"The message handler of type {handlerType} returned an invalid result.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogException(ILogger logger, Exception exception)
        {
            (logger ?? DevLogger.Default).LogException(exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogExceptionInvalidScope(ILogger logger)
        {
            (logger ?? DevLogger.Default).LogException(new NullReferenceException("Scope"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogExceptionMessageIsNull(ILogger logger)
        {
            (logger ?? DevLogger.Default).LogException(new ArgumentNullException("message"));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidPublisherAccess(Type publisherType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{publisherType} must be retrieved via " +
                $"`{nameof(MessagePublisher)}.{nameof(MessagePublisher.Scope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidCachedPublisherAccess(Type publisherType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{publisherType} must be retrieved via " +
                $"`{nameof(MessagePublisher)}.{nameof(MessagePublisher.Cache)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidUnityPublisherAccess(Type publisherType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{publisherType.Name} must be retrieved via " +
                $"`{nameof(MessagePublisher)}.{nameof(MessagePublisher.UnityScope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidSubscriberAccess(Type subscriberType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{subscriberType} must be retrieved via " +
                $"`{nameof(MessageSubscriber)}.{nameof(MessageSubscriber.Scope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidStatefulSubscriberAccess(Type subscriberType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{subscriberType.Name} must be retrieved via " +
                $"`{nameof(MessageSubscriber)}.{nameof(MessageSubscriber.Scope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidUnitySubscriberAccess(Type subscriberType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{subscriberType.Name} must be retrieved via " +
                $"`{nameof(MessageSubscriber)}.{nameof(MessageSubscriber.UnityScope)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorInvalidInterceptorsAccess(ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                $"{nameof(MessageInterceptors)} must be retrieved via " +
                $"`{nameof(Messenger)}.{nameof(Messenger.Interceptors)}` API"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorUnexpectedBrokerRegistration<TScope, TMessage>(ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(
                "Something went wrong when registering a new instance of " +
                $"{typeof(MessageBroker<TScope, TMessage>)}!"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogWarningNoSubscriber<TScope, TMessage>(TScope scope, PublishingContext context)
        {
            if (context.WarnNoSubscriber)
            {
                context.Logger.LogWarning(
                    $"Found no subscription for `{typeof(TMessage)}` " +
                    $"inside the scope `{typeof(TScope)}({scope})`."
                );
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorFailedWaitThenPublish<TScope, TMessage>(TScope scope, PublishingContext context)
        {
            context.Logger.LogError(
                $"Failed to wait then publish: No subscriber for message type `{typeof(TMessage)}` " +
                $"inside the scope `{typeof(TScope)}({scope})`. This should never happen!"
            );
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogErrorStateUnavailable(Type stateType, ILogger logger)
        {
            (logger ?? DevLogger.Default).LogError(GetStateUnavailableMessage(stateType));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void LogHandlerError(in StateUnavailableError error, ILogger logger)
        {
            LogErrorStateUnavailable(error.StateType, logger);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetStateUnavailableMessage(Type stateType)
            => $"The state instance of type {stateType} is not alive anymore.";

    }
}
