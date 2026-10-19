#if UNITY_UGUI

using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.Loading;
using EncosyTower.Pooling;
using UnityEngine;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows
{
    public sealed class UguiPageFlowContextTests
    {
        private UguiPageFlowSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<UguiPageFlowSettings>();
            _settings.warnNoSubscriber = true;
            _settings.rejectDuplicatePages = true;
            _settings.loaderStrategy = PageLoaderStrategy.Resources;
            _settings.poolRentingStrategy = RentingStrategy.Activate;
            _settings.poolReturningStrategy = ReturningStrategy.Deactivate;
            _settings.messageScope = UguiMessageScope.Component;
            _settings.logEnvironment = LogEnvironment.Runtime;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Initialize_WithSettings_UsesSettingsUnlessOverridden()
        {
            var context = CreatePartlyOverriddenContext();

            context.Initialize(settings: _settings);

            Assert.IsFalse(context.WarnNoSubscriber);
            Assert.IsTrue(context.RejectDuplicatePages);
            Assert.AreEqual(PageLoaderStrategy.Resources, context.LoadStrategy);
            Assert.AreEqual(RentingStrategy.DoNothing, context.PoolRentingStrategy);
            Assert.AreEqual(ReturningStrategy.Deactivate, context.PoolReturningStrategy);
            Assert.AreEqual(UguiMessageScope.Component, context.MessageScope);
            Assert.AreEqual(LogEnvironment.Runtime, context.LogEnvironment);
        }

        [Test]
        public void Initialize_WithoutSettings_UsesSettingsDefaultsUnlessOverridden()
        {
            var context = CreatePartlyOverriddenContext();

            context.Initialize(settings: null);

            Assert.IsFalse(context.WarnNoSubscriber);
            Assert.IsFalse(context.RejectDuplicatePages);
            Assert.AreEqual(default(PageLoaderStrategy), context.LoadStrategy);
            Assert.AreEqual(RentingStrategy.DoNothing, context.PoolRentingStrategy);
            Assert.AreEqual(default(ReturningStrategy), context.PoolReturningStrategy);
            Assert.AreEqual(default(UguiMessageScope), context.MessageScope);
            Assert.AreEqual(default(LogEnvironment), context.LogEnvironment);
        }

        [Test]
        public void CloneWithoutOwner_KeepsOverridesAndEffectiveValues()
        {
            var context = CreatePartlyOverriddenContext();
            context.Initialize(settings: _settings);

            var clone = context.CloneWithoutOwner();

            Assert.AreEqual(context.warnNoSubscriber, clone.warnNoSubscriber);
            Assert.AreEqual(context.rejectDuplicatePages, clone.rejectDuplicatePages);
            Assert.AreEqual(context.poolRentingStrategy, clone.poolRentingStrategy);
            Assert.AreEqual(context.loadStrategy, clone.loadStrategy);
            Assert.AreEqual(context.WarnNoSubscriber, clone.WarnNoSubscriber);
            Assert.AreEqual(context.RejectDuplicatePages, clone.RejectDuplicatePages);
            Assert.AreEqual(context.LoadStrategy, clone.LoadStrategy);
            Assert.AreEqual(context.PoolRentingStrategy, clone.PoolRentingStrategy);
            Assert.AreEqual(context.PoolReturningStrategy, clone.PoolReturningStrategy);
            Assert.AreEqual(context.MessageScope, clone.MessageScope);
            Assert.AreEqual(context.LogEnvironment, clone.LogEnvironment);
        }

        [Test]
        public void Initialize_RejectDuplicatePagesOverride_WinsOverSettings()
        {
            var context = new UguiPageFlowContext {
                rejectDuplicatePages = new(value: false, isOverridden: true),
            };

            context.Initialize(settings: _settings);

            Assert.IsFalse(context.RejectDuplicatePages);
        }

        private static UguiPageFlowContext CreatePartlyOverriddenContext()
            => new() {
                warnNoSubscriber = new(value: false, isOverridden: true),
                poolRentingStrategy = new(value: RentingStrategy.DoNothing, isOverridden: true),
                loadStrategy = new(value: PageLoaderStrategy.Addressables, isOverridden: false),
            };
    }
}

#endif
