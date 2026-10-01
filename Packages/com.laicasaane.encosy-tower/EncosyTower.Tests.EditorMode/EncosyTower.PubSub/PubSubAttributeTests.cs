using System.Buffers;
using System.Collections.Generic;
using EncosyTower.CodeGen;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using NUnit.Framework;

namespace EncosyTower.Tests.PubSub
{
    public partial class PubSubAttributeTests
    {
        [Test]
        public void Constructor_PreservesModeWithDefaultStateAndScope()
        {
            var attribute = new PubSubAttribute((ApiMode)99);

            Assert.AreEqual((ApiMode)99, attribute.Mode);
            Assert.AreEqual(StateMode.Both, attribute.State);
            Assert.IsNull(attribute.Scope);
        }

        [Test]
        public void Properties_PreserveValues()
        {
            var attribute = new PubSubAttribute(ApiMode.Both)
            {
                State = (StateMode)99,
                Scope = typeof(ScopeA),
            };

            Assert.AreEqual(ApiMode.Both, attribute.Mode);
            Assert.AreEqual((StateMode)99, attribute.State);
            Assert.AreEqual(typeof(ScopeA), attribute.Scope);
        }

        [Test]
        public void SyncEnabledFixtures_UseExistingGenericPubSubApis()
        {
            using var messenger = new Messenger(ArrayPool<UnityTask>.Shared);
            var subscriptions = new List<ISubscription>();
            var syncCount = 0;
            var bothCount = 0;

            try
            {
                subscriptions.Add(
                    messenger.Subscriber.Global()
                        .Subscribe<SyncMessage>(() => syncCount++)
                );
                subscriptions.Add(
                    messenger.Subscriber.Global()
                        .Subscribe<BothMessage>(() => bothCount++)
                );

                var context = PublishingContext.Default(warnNoSubscriber: false);
                messenger.Publisher.Global().Publish(new SyncMessage(), context);
                messenger.Publisher.Global().Publish(new BothMessage(), context);

                Assert.AreEqual(1, syncCount);
                Assert.AreEqual(1, bothCount);
            }
            finally
            {
                subscriptions.Unsubscribe();
            }
        }

        [PubSub(ApiMode.Sync, State = StateMode.Both)]
        private readonly partial struct SyncMessage : IMessage
        {
        }

        [PubSub(ApiMode.Both, State = StateMode.Both)]
        private readonly partial struct BothMessage
        {
        }

        private readonly struct ScopeA
        {
        }
    }
}
