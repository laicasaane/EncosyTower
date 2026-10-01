using System;
using EncosyTower.Logging;

namespace EncosyTower.PubSub.Internals
{
    internal interface IMessageBroker : IDisposable
    {
    }

    internal interface ICompressibleMessageBroker : IMessageBroker
    {
        void Compress(ILogger logger);
    }
}
