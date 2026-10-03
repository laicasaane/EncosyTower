using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Vaults;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Vaults;

public class ObjectVaultTests
{
    private const int ID = 1;

    [Test]
    public void TryAdd_Existing_ReturnsFalse()
    {
        var vault = new ObjectVault<int>();
        var first = new Probe();
        var second = new Probe();

        Assert.IsTrue(vault.TryAdd(ID, first));
        Assert.IsFalse(vault.TryAdd(ID, second));
        Assert.IsTrue(vault.TryGet<Probe>(ID, out var stored));
        Assert.AreSame(first, stored.GetValueOrThrow());
    }

    [Test]
    public void TryRemoveExpected_RemovesOnlySameInstance()
    {
        var vault = new ObjectVault<int>();
        var stored = new Probe();
        var other = new Probe();

        vault.TryAdd(ID, stored);

        Assert.IsFalse(vault.TryRemove(ID, other));
        Assert.IsTrue(vault.TryGet<Probe>(ID, out var remaining));
        Assert.AreSame(stored, remaining.GetValueOrThrow());

        Assert.IsTrue(vault.TryRemove(ID, stored));
        Assert.IsFalse(vault.Contains(ID));
    }

    [Test]
    public void DestroyedObject_LogsOnce()
    {
        var vault = new ObjectVault<int>();
        var gameObject = new GameObject(nameof(DestroyedObject_LogsOnce));

        vault.TryAdd(ID, gameObject);
        UnityEngine.Object.DestroyImmediate(gameObject);

        LogAssert.Expect(LogType.Error, new Regex("is null"));

        Assert.IsTrue(vault.TryGet<GameObject>(ID, out var result));
        Assert.IsFalse(result.HasValue);

        LogAssert.NoUnexpectedReceived();
    }

    [Test]
    public async Task TryGetAsync_PresentEntry_ReturnsSome()
    {
        var vault = new ObjectVault<int>();
        var stored = new Probe();

        vault.TryAdd(ID, stored);

        var result = await vault.TryGetAsync<Probe>(ID, context: null, token: CancellationToken.None);
        var untyped = await vault.TryGetAsync(ID, context: null, token: CancellationToken.None);

        Assert.AreSame(stored, result.GetValueOrThrow());
        Assert.AreSame(stored, untyped.GetValueOrThrow());
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task CancelledWaits_Throw(bool entryPresent)
    {
        var vault = new ObjectVault<int>();
        var token = new CancellationToken(canceled: true);

        if (entryPresent)
        {
            vault.TryAdd(ID, new Probe());
        }

        await CaptureExpectedExceptionAsync<OperationCanceledException>(WaitUntilContainsAsync);
        await CaptureExpectedExceptionAsync<OperationCanceledException>(TryGetTypedAsync);
        await CaptureExpectedExceptionAsync<OperationCanceledException>(TryGetUntypedAsync);

        async Task WaitUntilContainsAsync()
        {
            await vault.WaitUntilContains(ID, token);
        }

        async Task TryGetTypedAsync()
        {
            await vault.TryGetAsync<Probe>(ID, context: null, token: token);
        }

        async Task TryGetUntypedAsync()
        {
            await vault.TryGetAsync(ID, context: null, token: token);
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

    private sealed class Probe { }
}
