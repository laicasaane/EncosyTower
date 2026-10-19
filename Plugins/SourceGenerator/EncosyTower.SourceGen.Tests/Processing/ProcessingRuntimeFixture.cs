namespace EncosyTower.SourceGen.Tests.Processing;

internal static class ProcessingRuntimeFixture
{
    internal const string MarkerSource = """
        global using EncosyTower.CodeGen;

        namespace EncosyTower.CodeGen
        {
            public enum ApiMode
            {
                Sync = 0,
                Async = 1,
                Both = 2,
            }

            public enum StateMode
            {
                Stateless = 0,
                Stateful = 1,
                Both = 2,
            }
        }

        namespace EncosyTower.Processing
        {
            [System.AttributeUsage(
                  System.AttributeTargets.Class | System.AttributeTargets.Struct
                , AllowMultiple = true
                , Inherited = false
            )]
            public sealed class ProcessingAttribute : System.Attribute
            {
                public ProcessingAttribute(ApiMode mode)
                {
                    Mode = mode;
                }

                public ApiMode Mode { get; }

                public StateMode State { get; set; } = StateMode.Both;

                public System.Type Scope { get; set; }
            }
        }
        """;

    internal const string RuntimeSource = """
        namespace EncosyTower.Common
        {
            public readonly struct Option<T> { }

            public readonly struct Success<TFailure> { }

            public readonly struct Result<TValue, TError> { }

            public readonly struct GlobalScope { }
        }

        namespace EncosyTower.Logging
        {
            public interface ILogger { }
        }

        namespace EncosyTower.Tasks
        {
            public readonly struct UnityTask { }

            public readonly struct UnityTask<T> { }
        }

        namespace EncosyTower.Processing
        {
            using System;
            using EncosyTower.Common;
            using EncosyTower.Logging;
            using EncosyTower.Tasks;

            public interface IRequest { }

            public interface IRequest<TResult> { }

            public interface IAsyncRequest { }

            public interface IAsyncRequest<TResult> { }

            public readonly struct ProcessingContext { }

            public readonly struct ProcessRegistry { }

            public readonly struct ProcessError { }

            public sealed class SkipSourceGeneratorsForAssemblyAttribute : Attribute { }

            public static class GlobalProcessor
            {
                public static Processor Instance { get; } = new();
            }

            public static class ProcessorExtensions
            {
                public static Processor.Hub<TScope, TState> WithState<TScope, TState>(
                      in Processor.Hub<TScope> hub
                    , TState state
                )
                    where TState : class
                    => default;

                public static Processor.UnityHub<TScope, TState> WithState<TScope, TState>(
                      in Processor.UnityHub<TScope> hub
                    , TState state
                )
                    where TScope : UnityEngine.Object
                    where TState : class
                    => default;
            }

            public sealed class Processor
            {
                public Hub<GlobalScope> Global()
                    => default;

                public Hub<TScope> Scope<TScope>(TScope scope)
                    => default;

                public readonly struct Hub<TScope>
                {
                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TRequest> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TRequest, ProcessingContext> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, ProcessingContext, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TRequest, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TRequest, ProcessingContext, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, ProcessingContext, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;

                    public void Process<TRequest>(TRequest request, ProcessingContext context = default) { }

                    public Success<ProcessError> TryProcess<TRequest>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public TResult Process<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public Result<TResult, ProcessError> TryProcess<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask ProcessAsync<TRequest>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask<Success<ProcessError>> TryProcessAsync<TRequest>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask<TResult> ProcessAsync<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask<Result<TResult, ProcessError>> TryProcessAsync<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;
                }

                public readonly struct Hub<TScope, TState>
                    where TState : class
                {
                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TState, TRequest> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TState, TRequest, ProcessingContext> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, ProcessingContext, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TState, TRequest, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TState, TRequest, ProcessingContext, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, ProcessingContext, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;
                }

                public readonly struct UnityHub<TScope>
                    where TScope : UnityEngine.Object
                {
                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TRequest> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TRequest, ProcessingContext> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, ProcessingContext, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TRequest, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TRequest, ProcessingContext, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TRequest, ProcessingContext, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;

                    public void Process<TRequest>(TRequest request, ProcessingContext context = default) { }

                    public Success<ProcessError> TryProcess<TRequest>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public TResult Process<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public Result<TResult, ProcessError> TryProcess<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask ProcessAsync<TRequest>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask<Success<ProcessError>> TryProcessAsync<TRequest>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask<TResult> ProcessAsync<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;

                    public UnityTask<Result<TResult, ProcessError>> TryProcessAsync<TRequest, TResult>(
                          TRequest request
                        , ProcessingContext context = default
                    )
                        => default;
                }

                public readonly struct UnityHub<TScope, TState>
                    where TScope : UnityEngine.Object
                    where TState : class
                {
                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TState, TRequest> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Action<TState, TRequest, ProcessingContext> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, ProcessingContext, TResult> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TState, TRequest, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest>(
                          Func<TState, TRequest, ProcessingContext, UnityTask> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;

                    public Result<ProcessRegistry, ProcessError> Register<TRequest, TResult>(
                          Func<TState, TRequest, ProcessingContext, UnityTask<TResult>> process
                        , ILogger logger = null
                    )
                        => default;
                }
            }
        }
        """;

    internal const string ScopeSource = """
        namespace TestProject
        {
            public interface IOrderScope { }

            public abstract class AbstractScope { }

            public enum EnumScope { Default }

            public sealed class NoDefaultConstructorScope
            {
                public NoDefaultConstructorScope(int value) { }
            }

            public sealed class ViewScope : UnityEngine.Object { }

            public static class StaticScope { }

            public ref struct RefScope { }

            public class OpenScope<T> { }
        }
        """;
}
