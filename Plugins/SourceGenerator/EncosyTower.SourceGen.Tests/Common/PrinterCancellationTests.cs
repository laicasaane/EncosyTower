namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class PrinterCancellationTests
{
    [TestMethod]
    public void Printer_CancellationRequestedBeforeNextWrite_Throws()
    {
        using var cancellationSource = new CancellationTokenSource();
        var printer = new Printer(0, 1024, cancellationSource.Token);
        printer.Print("before");
        var newCopy = Printer.NewCopy(printer);
        var relativeIndent = printer.WithRelativeIndent(1);
        var increasedIndent = printer.WithIncreasedIndent();
        var decreasedIndent = increasedIndent.WithDecreasedIndent();
        cancellationSource.Cancel();

        AssertMatchingCancellation(() => printer.Print("after"), cancellationSource.Token);
        AssertMatchingCancellation(() => _ = printer.Result, cancellationSource.Token);
        AssertMatchingCancellation(() => newCopy.Print("after"), cancellationSource.Token);
        AssertMatchingCancellation(() => relativeIndent.Print("after"), cancellationSource.Token);
        AssertMatchingCancellation(() => increasedIndent.Print("after"), cancellationSource.Token);
        AssertMatchingCancellation(() => decreasedIndent.Print("after"), cancellationSource.Token);
    }

    private static void AssertMatchingCancellation(Action action, CancellationToken expectedToken)
    {
        OperationCanceledException? exception = null;

        try
        {
            action();
        }
        catch (OperationCanceledException caught)
        {
            exception = caught;
        }

        Assert.IsNotNull(exception, "Operation completed after cancellation was requested.");
        Assert.AreEqual(expectedToken, exception.CancellationToken);
    }
}
