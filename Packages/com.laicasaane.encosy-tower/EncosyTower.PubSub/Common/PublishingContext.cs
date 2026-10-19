using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        private readonly ILogger _logger;

        public PublishingStrategy Strategy { get; init; }

        public ILogger Logger
        {
            get => _logger ?? DevLogger.Default;
            init => _logger = value;
        }

        public CallerInfo CallerInfo { get; init; }

        public CancellationToken Token { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext Default(
              ILogger logger = default
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return DropIfNoSubscriber(logger, callerInfo, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext DropIfNoSubscriber(
              ILogger logger = default
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return new PublishingContext() {
                Strategy = PublishingStrategy.DropIfNoSubscriber,
                Logger = logger,
                CallerInfo = callerInfo,
                Token = token,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext WaitForSubscriber(
              ILogger logger = default
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return new PublishingContext() {
                Strategy = PublishingStrategy.WaitForSubscriber,
                Logger = logger,
                CallerInfo = callerInfo,
                Token = token
            };
        }
    }
}
