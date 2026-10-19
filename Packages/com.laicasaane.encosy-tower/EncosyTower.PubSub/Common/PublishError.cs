using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PolyEnumStructs;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PubSub
{
    /// <summary>Describes why a publish failed.</summary>
    /// <remarks>
    /// PubSub never logs a failure it returns; the caller logs each failed result it reacts to.
    /// The primary error is the first failure in handler order; the other failures are in <c>SuppressedErrors</c>.
    /// </remarks>
    [PolyEnumFactoryFor(typeof(Error))]
    public readonly partial struct PublishError
    {
        internal readonly Error _error;
        private readonly PublishError[] _suppressed;

        private PublishError(in Error error) : this(error, null)
        {
        }

        private PublishError(in Error error, PublishError[] suppressed)
        {
            _error = error;
            _suppressed = suppressed;
        }

        public System.ReadOnlySpan<PublishError> SuppressedErrors => _suppressed;

        public bool IsSubscriberNotFound => _error.IsSubscriberNotFound;

        public bool TryGetException(out System.Exception exception)
        {
            if (_error.TryGetValue(out Error.HandlerFaulted faulted))
            {
                exception = faulted.Exception;
                return true;
            }

            exception = default;
            return false;
        }

        public bool TryGetStateType(out System.Type stateType)
        {
            if (_error.TryGetValue(out Error.StateUnavailable unavailable))
            {
                stateType = unavailable.StateType;
                return true;
            }

            stateType = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToMessage()
            => _error.ToMessage();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string ToString()
            => ToMessage();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Log([NotNull] ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(logger);
            _error.Log(logger);

            var suppressed = _suppressed;

            if (suppressed == null)
            {
                return;
            }

            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                suppressed[i].Log(logger);
            }
        }

        internal PublishError WithSuppressed(in PublishError error)
        {
            var current = _suppressed;

            if (current == null)
            {
                return new(_error, new[] { error });
            }

            var count = current.Length;
            var suppressed = new PublishError[count + 1];

            System.Array.Copy(current, suppressed, count);
            suppressed[count] = error;

            return new(_error, suppressed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Success<PublishError> Merge(in Success<PublishError> current, in Success<PublishError> next)
        {
            if (next.IsFailure == false)
            {
                return current;
            }

            return MergeFailure(current, next);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PublishError CreateHandlerFaulted(System.Exception exception)
            => HandlerFaulted(exception);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PublishError CreateStateUnavailable(System.Type stateType)
            => StateUnavailable(stateType);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PublishError CreateSubscriberNotFound<TScope, TMessage>()
            => SubscriberNotFound(typeof(TScope), typeof(TMessage));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Success<PublishError> MergeFailure(
              in Success<PublishError> current
            , in Success<PublishError> next
        )
        {
            var failure = next.GetFailureOrThrow();

            if (current.TryGetFailure(out var primary) == false)
            {
                return failure;
            }

            primary = primary.WithSuppressed(new PublishError(failure._error));

            var suppressed = failure._suppressed;

            if (suppressed == null)
            {
                return primary;
            }

            var count = suppressed.Length;

            for (var i = 0; i < count; i++)
            {
                primary = primary.WithSuppressed(suppressed[i]);
            }

            return primary;
        }

        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            partial interface IEnumCase
            {
                bool IsSubscriberNotFound => false;

                string ToMessage();

                void Log(ILogger logger)
                    => logger.LogError(ToMessage());
            }

            public readonly partial struct Undefined
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "An unknown publish error occurred.";
            }

            public readonly partial record struct HandlerFaulted(System.Exception Exception)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"A message handler threw an exception: {Exception?.Message}";

                [MethodImpl(MethodImplOptions.NoInlining)]
                public void Log(ILogger logger)
                    => logger.LogException(Exception);
            }

            public readonly partial record struct StateUnavailable(System.Type StateType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => ThrowHelper.GetStateUnavailableMessage(StateType);
            }

            public readonly partial record struct SubscriberNotFound(System.Type ScopeType, System.Type MessageType)
            {
                public bool IsSubscriberNotFound => true;

                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Found no subscriber for `{MessageType}` inside the scope `{ScopeType}`.";

                [MethodImpl(MethodImplOptions.NoInlining)]
                public void Log(ILogger logger)
                    => logger.LogWarning(ToMessage());
            }
        }
    }
}
