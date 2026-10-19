using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.PolyEnumStructs;
using EncosyTower.Processing.Internals;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    /// <summary>Describes why a processing operation failed.</summary>
    /// <remarks>
    /// Processing never logs a failure it returns; the caller logs each failed result it reacts to.
    /// </remarks>
    [PolyEnumFactoryFor(typeof(Error))]
    public readonly partial struct ProcessError
    {
        internal readonly Error _error;

        private ProcessError(in Error error)
        {
            _error = error;
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
            logger.LogError(ToMessage());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ProcessError CreateLookupFailed<TScope, TRequest>(IProcessHandler candidate)
        {
            if (candidate == null)
            {
                System.Type resultType = null;
                return HandlerNotFound(typeof(TScope), typeof(TRequest), resultType);
            }

            return HandlerTypeMismatch(typeof(TScope), typeof(TRequest), candidate.GetType());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ProcessError CreateLookupFailed<TScope, TRequest, TResult>(IProcessHandler candidate)
        {
            if (candidate == null)
            {
                return HandlerNotFound(typeof(TScope), typeof(TRequest), typeof(TResult));
            }

            return HandlerTypeMismatch(typeof(TScope), typeof(TRequest), candidate.GetType());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ProcessError CreateStateUnavailable(System.Type stateType)
            => StateUnavailable(stateType);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ProcessError CreateAlreadyRegistered(IProcessHandler handler)
            => HandlerAlreadyRegistered(handler.GetType());

        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            partial interface IEnumCase
            {
                string ToMessage();
            }

            public readonly partial struct Undefined
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "An unknown process error occurred.";
            }

            public readonly partial record struct HandlerNotFound(
                  System.Type ScopeType
                , System.Type RequestType
                , System.Type ResultType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                {
                    if (ResultType == null)
                    {
                        return $"Cannot find any process handler for the request `{RequestType}` " +
                            $"inside the scope `{ScopeType}`.";
                    }

                    return $"Cannot find any process handler for the request `{RequestType}` " +
                        $"which returns `{ResultType}` inside the scope `{ScopeType}`.";
                }
            }

            public readonly partial record struct HandlerTypeMismatch(
                  System.Type ScopeType
                , System.Type RequestType
                , System.Type HandlerType
            )
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Found a candidate process handler `{HandlerType}` for the request `{RequestType}` " +
                        $"inside the scope `{ScopeType}`, but it has an invalid type.";
            }

            public readonly partial record struct StateUnavailable(System.Type StateType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => ThrowHelper.GetStateUnavailableMessage(StateType);
            }

            public readonly partial record struct HandlerAlreadyRegistered(System.Type HandlerType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"A process handler of type `{HandlerType}` has already been registered.";
            }
        }
    }
}
