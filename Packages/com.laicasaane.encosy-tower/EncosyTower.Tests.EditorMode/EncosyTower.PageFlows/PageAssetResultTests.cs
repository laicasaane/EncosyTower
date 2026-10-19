using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.PageFlows.Loading;
#if UNITY_ADDRESSABLES
using EncosyTower.AddressableKeys;
#endif
using EncosyTower.AssetKeys;
using EncosyTower.Logging;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.ResourceKeys;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class PageAssetResultTests
{
    [Test]
    public async Task AssetLoadErrorsDescribeTheirReasonsAsync()
    {
        var resourceResult = await new ResourceKey("").LoadOrErrorAsync<GameObject>();
        Assert.IsTrue(resourceResult.TryGetError(out var resourceError));

        Assert.AreEqual(
              expected: "Resources: " + resourceError
            , actual: PageAssetLoadError.Resource(resourceError).ToMessage()
        );

#if UNITY_ADDRESSABLES
        var addressableResult = await new AddressableKey("").LoadGetHandleOrErrorAsync<GameObject>();
        Assert.IsTrue(addressableResult.TryGetError(out var addressableError));

        Assert.AreEqual(
              expected: "Addressables: " + addressableError
            , actual: PageAssetLoadError.Addressable(addressableError).ToMessage()
        );
#endif

        Assert.AreEqual(
              expected: "The asset loaded by the key 'k' is a 'UnityEngine.TextAsset'. "
                + "A UI Toolkit page must be a VisualTreeAsset or a GameObject with a UitkPageBehaviour."
            , actual: PageAssetLoadError.UnsupportedAsset("k", typeof(TextAsset)).ToMessage()
        );

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task LoaderWithTheResourcesStrategyLoadsThroughAResourceKeyAsync()
    {
        ResourceKey<GameObject> typedKey = default(AssetKey);
        var typedResult = await typedKey.LoadOrErrorAsync();
        Assert.IsTrue(typedResult.TryGetError(out var typedError));

        var result = await PageAssetLoader.LoadAsync<GameObject>(
              strategy: PageLoaderStrategy.Resources
            , assetKey: default
            , token: CancellationToken.None
        );

        Assert.IsTrue(result.TryGetError(out var error));
        Assert.IsTrue(error._error.TryGetValue(out PageAssetLoadError.Error.Resource resource));
        Assert.AreEqual(expected: typedError.ToString(), actual: resource.KeyError.ToString());

        LogAssert.NoUnexpectedReceived();
    }

#if UNITY_ADDRESSABLES
    [Test]
    public async Task LoaderWithTheAddressablesStrategyLoadsThroughAnAddressableKeyAsync()
    {
        AddressableKey<GameObject> typedKey = default(AssetKey);
        var typedResult = await typedKey.LoadGetHandleOrErrorAsync();
        Assert.IsTrue(typedResult.TryGetError(out var typedError));

        var result = await PageAssetLoader.LoadAsync<GameObject>(
              strategy: PageLoaderStrategy.Addressables
            , assetKey: default
            , token: CancellationToken.None
        );

        Assert.IsTrue(result.TryGetError(out var error));
        Assert.IsTrue(error._error.TryGetValue(out PageAssetLoadError.Error.Addressable addressable));
        Assert.AreEqual(expected: typedError.ToString(), actual: addressable.KeyError.ToString());

        LogAssert.NoUnexpectedReceived();
    }
#endif

    [Test]
    public void UitkFlowReportsTheResourcesFallbackOnlyWhenAddressablesIsMissing()
    {
        var logger = DevLogger.Default;

        LogAssert.Expect(LogType.Error, new Regex("UI Toolkit Page Loader will use Resources"));

        UitkPageFlow.ErrorIfAddressablesNotInstalled(
              strategy: PageLoaderStrategy.Addressables
            , isAddressablesAvailable: false
            , logger: logger
        );

        UitkPageFlow.ErrorIfAddressablesNotInstalled(
              strategy: PageLoaderStrategy.Addressables
            , isAddressablesAvailable: true
            , logger: logger
        );

        UitkPageFlow.ErrorIfAddressablesNotInstalled(
              strategy: PageLoaderStrategy.Resources
            , isAddressablesAvailable: false
            , logger: logger
        );

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public void FlowAssetAndPoolErrorsDescribeTheirReasons()
    {
        var page = new TestPage("P");
        var reason = PageAssetLoadError.UnsupportedAsset("k", typeof(TextAsset));
        var amount = 0;

        Assert.AreEqual(
              expected: "Cannot load the page asset with key 'k': " + reason.ToMessage()
            , actual: PageFlowError.AssetLoadFailed("k", reason).ToMessage()
        );

        Assert.AreEqual(
              expected: "The page 'P' is not a UnityEngine.Component."
            , actual: PageFlowError.PageNotComponent(page).ToMessage()
        );

        Assert.AreEqual(
              expected: "The page 'P' has no UguiPageIdentifier; it was not created by a page flow pool."
            , actual: PageFlowError.PageIdentifierMissing(page).ToMessage()
        );

        Assert.AreEqual(
              expected: "The page 'P' has been destroyed or was not created by a page flow pool."
            , actual: PageFlowError.PageDestroyed(page).ToMessage()
        );

        Assert.AreEqual(
              expected: "The page 'P' is not an active page of this flow."
            , actual: PageFlowError.PageNotActive(page).ToMessage()
        );

        Assert.AreEqual(
              expected: "Cannot find the pool for asset key 'k'."
            , actual: PageFlowError.PoolNotFound("k").ToMessage()
        );

        Assert.AreEqual(
              expected: "The prepool amount '0' must be greater than 0."
            , actual: PageFlowError.InvalidPrepoolAmount(amount).ToMessage()
        );

        LogAssert.NoUnexpectedReceived();
    }
}
