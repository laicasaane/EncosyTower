using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.AssetKeys;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using NUnit.Framework;

namespace EncosyTower.Tests.PageFlows;

public sealed class UitkPageFlowGuardTests
{
    private static async Task<Exception> ShowAndCatchAsync(bool multi, AssetKey assetKey, PageContext context)
    {
        try
        {
            if (multi)
            {
                await new UitkPageStackMulti().ShowPageAsync(assetKey, context, CancellationToken.None);
            }
            else
            {
                await new UitkPageStackSingle().ShowPageAsync(assetKey, context, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }

    private static void AssertReportedException(PageCallReceipt receipt, Exception thrown)
    {
        Assert.IsTrue(receipt.IsReported);
        Assert.IsTrue(receipt.Result.TryGetFailure(out var error));
        Assert.IsTrue(error._error.TryGetValue(out PageFlowError.Error.Exception wrapped));
        Assert.AreSame(thrown, wrapped.Ex);
    }

    [Test]
    public void UninitializedFlowsReadAsEmpty()
    {
        var stackSingle = new UitkPageStackSingle();
        var stackMulti = new UitkPageStackMulti();
        var listSingle = new UitkPageListSingle();
        var listMulti = new UitkPageListMulti();

        Assert.IsFalse(stackSingle.CurrentPage.HasValue);
        Assert.IsFalse(stackMulti.CurrentPage.HasValue);
        Assert.AreEqual(expected: 0, actual: stackMulti.PageCollection.Count);
        Assert.IsFalse(listSingle.CurrentPage.HasValue);
        Assert.AreEqual(expected: 0, actual: listSingle.Pages.Count);
        Assert.AreEqual(expected: 0, actual: listMulti.Pages.Count);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task ShowOnAnUninitializedStackReportsToTheReceiptAsync(bool multi)
    {
        var receipt = new PageCallReceipt();

        var thrown = await ShowAndCatchAsync(multi, "k", new PageContext { Receipt = receipt });

        Assert.IsInstanceOf<InvalidOperationException>(thrown);
        AssertReportedException(receipt, thrown);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task ShowWithAnInvalidKeyReportsToTheReceiptAsync(bool multi, bool empty)
    {
        var receipt = new PageCallReceipt();

        var thrown = await ShowAndCatchAsync(
              multi: multi
            , assetKey: empty ? new AssetKey(string.Empty) : default
            , context: new PageContext { Receipt = receipt }
        );

        Assert.IsInstanceOf<ArgumentNullException>(thrown);
        AssertReportedException(receipt, thrown);
    }
}
