using System.Threading;
using System.Threading.Tasks;
using EncosyTower.ResourceKeys;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Core.ResourceKeys;

public sealed class ResourceKeyLoadAsyncTests
{
    private const string RESOURCE_KEY = "uitk-test-behaviour-page";

    private static ResourceKey<GameObject> Key => RESOURCE_KEY;

    [Test]
    public async Task LoadOrErrorAsync_TokenCancelledWhileWaiting_ReturnsCancelledRequestAsync()
    {
        using var source = new CancellationTokenSource();
        var task = Key.LoadOrErrorAsync(source.Token);

        source.Cancel();

        var result = await task;

        Assert.IsTrue(result.TryGetError(out var error));
        StringAssert.Contains(
              expected: $"The loading request for the resource key '{RESOURCE_KEY}' was cancelled."
            , actual: error.ToString()
        );
    }

    [Test]
    public async Task LoadOrErrorAsync_TokenNotCancelled_LoadsTheAssetAsync()
    {
        var result = await Key.LoadOrErrorAsync();

        Assert.IsTrue(result.TryGetValue(out var asset));
        Assert.IsTrue(asset);
    }
}
