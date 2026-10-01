#if !(UNITY_EDITOR || DEBUG || ENCOSY_RUNTIME_CHECKS || ENCOSY_PROCESSING_RUNTIME_CHECKS) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Types;
using EncosyTower.UnityExtensions;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Processing
{
    public static partial class ProcessorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.UnityHub<TUnityScope> WithUnityScope<TScope, TUnityScope>(
              this in Processor.Hub<TScope> hub
            , [NotNull] TUnityScope scope
        )
            where TUnityScope : UnityEngine.Object
        {
            DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);
            return new(hub.WithScope((UnityEntityId<TUnityScope>)scope));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.UnityHub<TScope, TState> WithState<TScope, TState>(
              this in Processor.UnityHub<TScope> hub
            , [NotNull] TState state
        )
            where TScope : UnityEngine.Object
            where TState : class
        {
            DebuggingThrowHelper.ThrowIfNullOrUnityObjectInvalid(state);
            return new(hub, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.UnityHub<TScope> WithRegistries<TScope>(
              this in Processor.UnityHub<TScope> hub
            , ICollection<ProcessRegistry> registries
        )
            where TScope : UnityEngine.Object
        {
            return new(hub._hub.WithRegistries(registries));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Processor.UnityHub<TScope, TState> WithRegistries<TScope, TState>(
              this in Processor.UnityHub<TScope, TState> hub
            , ICollection<ProcessRegistry> registries
        )
            where TScope : UnityEngine.Object
            where TState : class
        {
            return new(hub._hub.WithRegistries(registries), hub.State);
        }
    }

    partial class Processor
    {
        public readonly partial struct UnityHub<TScope> : IIsCreated
            where TScope : UnityEngine.Object
        {
            internal readonly Hub<UnityEntityId<TScope>> _hub;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnityHub([NotNull] Processor processor, [NotNull] TScope scope)
            {
                DebuggingThrowHelper.ThrowIfNull(processor);
                DebuggingThrowHelper.ThrowIfUnityObjectInvalid(scope);

                _hub = processor.Scope((UnityEntityId<TScope>)scope);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnityHub(in Hub<UnityEntityId<TScope>> hub)
            {
                _hub = hub;
            }

            public bool IsCreated => _hub.IsCreated;

            public UnityEntityId<TScope> Scope => _hub.Scope;

            public ICollection<ProcessRegistry> Registries => _hub.Registries;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope> WithScope<TNewScope>()
                where TNewScope : struct
                => _hub.WithScope<TNewScope>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<TNewScope> WithScope<TNewScope>(TNewScope scope)
                => _hub.WithScope(scope);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Hub<GlobalScope> WithGlobalScope()
                => _hub.WithGlobalScope();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnityHub<TNewScope> WithUnityScope<TNewScope>([NotNull] TNewScope scope)
                where TNewScope : UnityEngine.Object
                => _hub.WithUnityScope(scope);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Action<TRequest> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest>(
                  [NotNull] Action<TRequest, ProcessingContext> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ProcessRegistry Register<TRequest, TResult>(
                  [NotNull] Func<TRequest, ProcessingContext, TResult> process
                , ILogger logger = null
            )
#if !ENCOSY_PROCESSING_RELAX_MODE
                where TRequest : IRequest<TResult>
#endif
                => _hub.Register(process, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Unregister(TypeId id, ILogger logger = null)
                => _hub.Unregister(id, logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear(ILogger logger = null)
                => _hub.Clear(logger);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Process<TRequest>(TRequest request, ProcessingContext context = default)
                => _hub.Process(request, context);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryProcess<TRequest>(TRequest request, ProcessingContext context = default)
                => _hub.TryProcess(request, context);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TResult Process<TRequest, TResult>(TRequest request, ProcessingContext context = default)
                => _hub.Process<TRequest, TResult>(request, context);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Option<TResult> TryProcess<TRequest, TResult>(
                  TRequest request
                , ProcessingContext context = default
            )
                => _hub.TryProcess<TRequest, TResult>(request, context);
        }
    }
}
