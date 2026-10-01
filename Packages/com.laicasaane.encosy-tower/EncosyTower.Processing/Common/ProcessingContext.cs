using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Logging;

namespace EncosyTower.Processing
{
    public readonly struct ProcessingContext
    {
        private readonly ILogger _logger;

        public ProcessingStrategy Strategy { get; init; }

        public bool WarnNoHandler { get; init; }

        public ILogger Logger
        {
            get => _logger ?? DevLogger.Default;
            init => _logger = value;
        }

        public CallerInfo CallerInfo { get; init; }

        public CancellationToken Token { get; init; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessingContext Default(
              bool warnNoHandler = true
            , ILogger logger = null
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return DropIfNoHandler(warnNoHandler, logger, callerInfo, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessingContext DropIfNoHandler(
              bool warnNoHandler = true
            , ILogger logger = null
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return new ProcessingContext() {
                Strategy = ProcessingStrategy.DropIfNoHandler,
                WarnNoHandler = warnNoHandler,
                Logger = logger,
                CallerInfo = callerInfo,
                Token = token,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ProcessingContext WaitForHandler(
              ILogger logger = null
            , in CallerInfo callerInfo = default
            , CancellationToken token = default
        )
        {
            return new ProcessingContext() {
                Strategy = ProcessingStrategy.WaitForHandler,
                WarnNoHandler = false,
                Logger = logger,
                CallerInfo = callerInfo,
                Token = token,
            };
        }
    }
}
