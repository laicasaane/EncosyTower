using EncosyTower.Common;
using EncosyTower.Tasks;
using EncosyTower.Types;

namespace EncosyTower.Processing.Internals
{
    internal interface IProcessHandler
    {
        TypeId Id { get; }
    }

    internal interface IProcessHandler<in TRequest> : IProcessHandler
    {
        Result<Success, StateUnavailableError> Process(TRequest request, ProcessingContext context);
    }

    internal interface IProcessHandler<in TRequest, TResult> : IProcessHandler
    {
        Result<TResult, StateUnavailableError> Process(TRequest request, ProcessingContext context);
    }

    internal interface IAsyncProcessHandler<in TRequest> : IProcessHandler
    {
        Result<UnityTask, StateUnavailableError>
        ProcessAsync(TRequest request, ProcessingContext context);
    }

    internal interface IAsyncProcessHandler<in TRequest, TResult> : IProcessHandler
    {
        Result<UnityTask<TResult>, StateUnavailableError>
        ProcessAsync(TRequest request, ProcessingContext context);
    }
}
