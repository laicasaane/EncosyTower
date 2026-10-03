using System;
using System.Text.RegularExpressions;
using EncosyTower.Vaults;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EncosyTower.Tests.Core.Vaults;

public class SingletonVaultTests
{
    [Test]
    public void TryGetOrAdd_ReturnsStoredInstance()
    {
        var vault = new SingletonVault<object>();

        Assert.IsTrue(vault.TryGetOrAdd<Probe>(out var first));
        Assert.IsTrue(vault.TryGetOrAdd<Probe>(out var second));
        Assert.AreSame(first, second);
    }

    [Test]
    public void TryAdd_Existing_ReturnsFalse()
    {
        var vault = new SingletonVault<object>();

        Assert.IsTrue(vault.TryAdd<Probe>());

        LogAssert.Expect(LogType.Error, new Regex("already"));

        Assert.IsFalse(vault.TryAdd<Probe>());
    }

    [Test]
    public void Dispose_DisposesAndRemoves()
    {
        var vault = new SingletonVault<object>();
        var first = new DisposableProbe();
        var second = new OtherDisposableProbe();

        vault.TryAdd(first);
        vault.TryAdd(second);
        vault.Dispose();

        Assert.IsTrue(first.IsDisposed);
        Assert.IsTrue(second.IsDisposed);
        Assert.IsFalse(vault.Contains<DisposableProbe>());
        Assert.IsFalse(vault.Contains<OtherDisposableProbe>());
    }

    private sealed class Probe { }

    private sealed class DisposableProbe : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class OtherDisposableProbe : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
