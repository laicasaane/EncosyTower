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

    private static void AssertReportedException(PageCallReport report, Exception thrown)
    {
        Assert.IsTrue(report.IsReported);
        Assert.IsTrue(report.Result.TryGetFailure(out var error));
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
    public async Task ShowOnAnUninitializedStackReportsToTheReportAsync(bool multi)
    {
        var report = new PageCallReport();

        var thrown = await ShowAndCatchAsync(multi, "k", new PageContext { Report = report });

        Assert.IsInstanceOf<InvalidOperationException>(thrown);
        AssertReportedException(report, thrown);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task ShowWithAnInvalidKeyReportsToTheReportAsync(bool multi, bool empty)
    {
        var report = new PageCallReport();

        var thrown = await ShowAndCatchAsync(
              multi: multi
            , assetKey: empty ? new AssetKey(string.Empty) : default
            , context: new PageContext { Report = report }
        );

        Assert.IsInstanceOf<ArgumentNullException>(thrown);
        AssertReportedException(report, thrown);
    }
}
