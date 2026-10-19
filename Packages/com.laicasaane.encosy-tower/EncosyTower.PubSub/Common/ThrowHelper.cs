// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
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

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfPublisherNotCreated([DoesNotReturnIf(false)] bool created, Type type)
        {
            if (created == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type type)
                => new(
                    $"{type} is not created. " +
                    "Get a publisher from MessagePublisher.Global() or MessagePublisher.Scope()."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfCachedPublisherNotCreated([DoesNotReturnIf(false)] bool created, Type type)
        {
            if (created == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type type)
                => new(
                    $"{type} is not created. " +
                    "Get a cached publisher from MessagePublisher.GlobalCache(), MessagePublisher.Cache(), " +
                    "MessagePublisher.UnityCache() or Publisher.Cache()."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfUnityPublisherNotCreated([DoesNotReturnIf(false)] bool created, Type type)
        {
            if (created == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type type)
                => new(
                    $"{type.Name} is not created. " +
                    "Get a Unity publisher from MessagePublisher.UnityScope()."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfSubscriberNotCreated([DoesNotReturnIf(false)] bool created, Type type)
        {
            if (created == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type type)
                => new(
                    $"{type} is not created. " +
                    "Get a subscriber from MessageSubscriber.Global() or MessageSubscriber.Scope()."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfStatefulSubscriberNotCreated([DoesNotReturnIf(false)] bool created, Type type)
        {
            if (created == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type type)
                => new(
                    $"{type.Name} is not created. " +
                    "Get a stateful subscriber from Subscriber.WithState()."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfUnitySubscriberNotCreated([DoesNotReturnIf(false)] bool created, Type type)
        {
            if (created == false)
            {
                throw CreateException(type);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException(Type type)
                => new(
                    $"{type.Name} is not created. " +
                    "Get a Unity subscriber from MessageSubscriber.UnityScope()."
                );
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfInterceptorsNotCreated([DoesNotReturnIf(false)] bool created)
        {
            if (created == false)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static InvalidOperationException CreateException()
                => new("MessageInterceptors is not created. Get it from Messenger.Interceptors.");
        }

        [HideInCallstack, StackTraceHidden]
        [Conditional(UNITY_EDITOR), Conditional(DEBUG)]
        [Conditional(RUNTIME_CHECKS), Conditional(PUBSUB_CHECKS)]
        internal static void ThrowIfScopeNull<TScope>(TScope scope)
        {
            if (scope is null)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentNullException CreateException()
                => new("scope", "The scope must not be null.");
        }

        [HideInCallstack, StackTraceHidden]
        internal static void ThrowIfMessageNull<TMessage>(TMessage message)
        {
            if (message is null)
            {
                throw CreateException();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ArgumentNullException CreateException()
                => new("message", "The message must not be null.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [HideInCallstack, StackTraceHidden]
        internal static void LogDisplacedFailure(in Success<PublishError> result, ILogger logger)
        {
            if (result.TryGetFailure(out var error))
            {
                error.Log(logger);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string GetStateUnavailableMessage(Type stateType)
            => $"The state instance of type {stateType} is not alive anymore.";
    }
}
