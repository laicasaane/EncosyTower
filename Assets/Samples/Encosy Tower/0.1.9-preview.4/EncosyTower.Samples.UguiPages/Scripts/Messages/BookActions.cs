using EncosyTower.Common;
using EncosyTower.PubSub;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Config;

namespace EncosyTower.Samples.UguiPages.Messages
{
    /// <summary>Publishes a <see cref="AppActionMessage"/> from a page to the Screen flow scope.</summary>
    /// <remarks>Pages publish their button clicks this way, so the codex is the only code that acts on them.</remarks>
    internal static class BookActions
    {
        public static void Publish(MessagePublisher publisher, Option<AppFlowScopes> scopes, AppAction action)
            => Publish(publisher, scopes, new AppActionMessage(action, default, 0));

        public static void Publish(
              MessagePublisher publisher
            , Option<AppFlowScopes> scopes
            , AppAction action
            , BookLocation location
        )
            => Publish(publisher, scopes, new AppActionMessage(action, location, 0));

        public static void Publish(
              MessagePublisher publisher
            , Option<AppFlowScopes> scopes
            , AppAction action
            , int index
        )
            => Publish(publisher, scopes, new AppActionMessage(action, default, index));

        private static void Publish(
              MessagePublisher publisher
            , Option<AppFlowScopes> scopes
            , AppActionMessage message
        )
        {
            if (scopes.TryGetValue(out var value) == false)
            {
                return;
            }

            var scoped = publisher.Scope(value.Screen);
            AppActionMessage.Publish(in scoped, message);
        }
    }
}
