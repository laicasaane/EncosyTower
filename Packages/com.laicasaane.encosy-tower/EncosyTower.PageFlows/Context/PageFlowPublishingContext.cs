#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV
#define __PUBSUB_INCLUDE_CALLER_INFO_DEV__
#endif
#endif

namespace EncosyTower.PageFlows
{
    /// <summary>The caller info setting of the messages that page flows publish.</summary>
    public readonly struct PageFlowPublishingContext
    {
        /// <summary>Gets whether the flows add the caller info of their publish calls to messages.</summary>
        /// <value>
        /// <see cref="PubSubCallerInfoOption.ForDevelopment"/> in the Editor and in development builds when the
        /// scripting define <c>ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV</c> is set; otherwise
        /// <see cref="PubSubCallerInfoOption.Always"/> when <c>ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO</c> is
        /// set; otherwise <see cref="PubSubCallerInfoOption.Never"/>.
        /// </value>
        public PubSubCallerInfoOption CallerInfoOption
        {
            get
            {
#if __PUBSUB_INCLUDE_CALLER_INFO_DEV__
                return PubSubCallerInfoOption.ForDevelopment;
#elif ENCOSY_PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO
                return PubSubCallerInfoOption.Always;
#else
                return PubSubCallerInfoOption.Never;
#endif
            }
        }
    }

    /// <summary>When page flows add the caller info of their publish calls to messages.</summary>
    public enum PubSubCallerInfoOption : byte
    {
        /// <summary>
        /// Do not include caller info.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Include caller info only for Editor and Development builds.
        /// </summary>
        ForDevelopment,

        /// <summary>
        /// Include caller info for all environments.
        /// </summary>
        Always,
    }
}
