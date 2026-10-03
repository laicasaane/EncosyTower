using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Ids;
using EncosyTower.Vaults;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Vaults;

public class ValueVaultTests
{
    private const int ID = 1;
    private const int VALUE = 42;

    [Test]
    public void TryAdd_KeepsExistingValue()
    {
        var vault = new ValueVault<int, int>();

        Assert.IsTrue(vault.TryAdd(ID, VALUE));
        Assert.IsFalse(vault.TryAdd(id: ID, value: 7));
        Assert.IsTrue(vault.TryGet(ID, out var value));
        Assert.AreEqual(VALUE, value);
    }

    [Test]
    public async Task TryGetAsync_PresentEntry_ReturnsSome()
    {
        var vault = new ValueVault<int, int>();

        vault.TryAdd(ID, VALUE);

        var result = await vault.TryGetAsync(ID);

        Assert.AreEqual(VALUE, result.GetValueOrThrow());

        await vault.WaitUntil(ID, VALUE);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task CancelledWaits_Throw(bool entryPresent)
    {
        var vault = new ValueVault<int, int>();
        var globalId = new Id2(x: 201, y: 202);
        var token = new CancellationToken(canceled: true);

        if (entryPresent)
        {
            vault.TryAdd(ID, VALUE);
            GlobalValueVault<int>.TryAdd(globalId, VALUE);
        }

        await CaptureExpectedExceptionAsync<OperationCanceledException>(WaitUntilContainsAsync);
        await CaptureExpectedExceptionAsync<OperationCanceledException>(WaitUntilAsync);
        await CaptureExpectedExceptionAsync<OperationCanceledException>(TryGetAsync);
        await CaptureExpectedExceptionAsync<OperationCanceledException>(GlobalTryGetAsync);

        GlobalValueVault<int>.TryRemove(globalId, out _);

        async Task WaitUntilContainsAsync()
        {
            await vault.WaitUntilContains(ID, token);
        }

        async Task WaitUntilAsync()
        {
            await vault.WaitUntil(ID, VALUE, token);
        }

        async Task TryGetAsync()
        {
            await vault.TryGetAsync(ID, token);
        }

        async Task GlobalTryGetAsync()
        {
            await GlobalValueVault<int>.TryGetAsync(globalId, token);
        }
    }

    private static async Task<TException> CaptureExpectedExceptionAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException exception)
        {
            return exception;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).FullName}.");
        return null;
    }
}
