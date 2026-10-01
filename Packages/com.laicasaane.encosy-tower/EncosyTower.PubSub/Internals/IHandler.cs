using System;
using EncosyTower.Common;
using EncosyTower.Tasks;

namespace EncosyTower.PubSub.Internals
{
    internal interface IHandler<in TMessage> : IDisposable
    {
        DelegateId Id { get; }

        Result<UnityTask, StateUnavailableError> Handle(TMessage message, PublishingContext context);
    }
}
