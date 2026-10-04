using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Types;
using NUnit.Framework;

namespace EncosyTower.Tests.Core.Types;

public sealed partial class TypeFlagTests
{
    [SetUp]
    public void SetUp()
    {
        ResetState();
    }

    [TearDown]
    public void TearDown()
    {
        ResetState();
    }

    [Test]
    public void GeneratedMembersBindToOwnerFlag()
    {
        Assert.IsTrue(ClassOwner.Enable());
        Assert.IsTrue(ClassOwner.TypeFlag.IsEnabled);
        Assert.AreEqual(Type<ClassOwner>.Id, ClassOwner.TypeFlag.TypeId);
    }

    [Test]
    public void AllViewsShareState()
    {
        var byDefault = default(TypeFlag<ClassOwner>);
        var byNew = new TypeFlag<ClassOwner>();
        TypeFlag<ClassOwner>.ReadOnly readOnly = byNew;

        ClassOwner.Enable();

        Assert.IsTrue(byDefault.IsEnabled);
        Assert.IsTrue(byNew.IsEnabled);
        Assert.IsTrue(readOnly.IsEnabled);
        Assert.IsTrue(ClassOwner.TypeFlag.IsEnabled);
        Assert.IsTrue(ClassOwner.IsEnabledThroughWriter());

        byNew.Disable();

        Assert.IsFalse(byDefault.IsEnabled);
        Assert.IsFalse(byNew.IsEnabled);
        Assert.IsFalse(readOnly.IsEnabled);
        Assert.IsFalse(ClassOwner.TypeFlag.IsEnabled);
        Assert.IsFalse(ClassOwner.IsEnabledThroughWriter());
    }

    [Test]
    public void EnableDisableReportChanges()
    {
        var flag = default(TypeFlag<OwnerB>);

        Assert.IsTrue(flag.Enable());
        Assert.IsFalse(flag.Enable());
        Assert.IsTrue(flag.Disable());
        Assert.IsFalse(flag.Disable());
    }

    [Test]
    public void FlagsAreIndependentPerType()
    {
        default(TypeFlag<OwnerB>).Enable();

        Assert.IsTrue(default(TypeFlag<OwnerB>).IsEnabled);
        Assert.IsFalse(default(TypeFlag<SettingsA>).IsEnabled);
        Assert.IsFalse(ClassOwner.TypeFlag.IsEnabled);
        Assert.IsFalse(ValueOwner.TypeFlag.IsEnabled);
    }

    [Test]
    public void TryRegisterStoresThenEnables()
    {
        var first = new ClassOwner();
        var second = new ClassOwner();

        Assert.IsTrue(ClassOwner.TryRegister(first));
        Assert.IsTrue(ClassOwner.TypeFlag.IsEnabled);
        Assert.AreSame(first, ClassOwner.TypeFlag.GetInstanceOrThrow());

        Assert.IsFalse(ClassOwner.TryRegister(second));
        Assert.AreSame(first, ClassOwner.TypeFlag.GetInstanceOrThrow());
    }

    [Test]
    public void TryUnregisterRemovesOnlySameInstance()
    {
        var stored = new ClassOwner();
        var other = new ClassOwner();

        ClassOwner.TryRegister(stored);

        Assert.IsFalse(ClassOwner.TryUnregister(other));
        Assert.IsTrue(ClassOwner.TypeFlag.IsEnabled);
        Assert.AreSame(stored, ClassOwner.TypeFlag.GetInstanceOrThrow());

        Assert.IsTrue(ClassOwner.TryUnregister(stored));
        Assert.IsFalse(ClassOwner.TypeFlag.IsEnabled);
        Assert.IsFalse(ClassOwner.TypeFlag.TryGetInstance(out _));
    }

    [Test]
    public void ValueOwnerSelfStorage()
    {
        ValueOwner.TypeFlag.SetValue(new ValueOwner(value: 1));
        ValueOwner.TypeFlag.SetValue(new ValueOwner(value: 2));

        Assert.IsTrue(ValueOwner.TypeFlag.TryGetValue(out var value));
        Assert.AreEqual(2, value.Value);
        Assert.AreEqual(2, ValueOwner.TypeFlag.GetValueOrThrow().Value);

        Assert.IsTrue(ValueOwner.TypeFlag.TryRemoveValue(out var removed));
        Assert.AreEqual(2, removed.Value);
        Assert.IsFalse(ValueOwner.TypeFlag.TryGetValue(out _));
    }

    [Test]
    public void LinksKeyByOwnerAndLinkedType()
    {
        var owner = default(TypeFlag<OwnerB>);
        var settingsA = new SettingsA();
        var settingsB = new SettingsB();

        Assert.IsTrue(owner.GetLink<SettingsA>().TryAddObject(settingsA));
        Assert.IsTrue(owner.GetLink<SettingsB>().TryAddObject(settingsB));

        Assert.AreSame(settingsA, owner.GetLink<SettingsA>().GetObjectOrThrow());
        Assert.AreSame(settingsB, owner.GetLink<SettingsB>().GetObjectOrThrow());

        Assert.IsFalse(ClassOwner.TypeFlag.TryGetObject<SettingsA>(out _));
        Assert.IsFalse(ClassOwner.TypeFlag.TryGetObject<SettingsB>(out _));
        Assert.IsFalse(default(TypeFlag<SettingsA>).GetLink<SettingsA>().TryGetObject(out _));
        Assert.IsFalse(default(TypeFlag<SettingsB>).GetLink<SettingsB>().TryGetObject(out _));
        Assert.IsFalse(owner.GetLink<OwnerB>().TryGetObject(out _));
    }

    [Test]
    public void PhaseMarkerState()
    {
        Assert.IsTrue(Phase.TypeFlag.Enable());
        Assert.IsFalse(Phase.TypeFlag.Enable());
        Assert.IsTrue(Phase.TypeFlag.IsEnabled);
        Assert.IsTrue(Phase.TypeFlag.Disable());
        Assert.IsFalse(Phase.TypeFlag.Disable());
        Assert.IsFalse(Phase.TypeFlag.IsEnabled);
    }

    [Test]
    public void OrThrowThrowsWhenAbsent()
    {
        var flag = default(TypeFlag<OwnerB>);
        TypeFlag<OwnerB>.ReadOnly readOnly = flag;
        var link = flag.GetLink<SettingsA>();
        var valueFlag = default(TypeFlag<Config>);
        TypeFlag<Config>.ReadOnly valueReadOnly = valueFlag;

        Assert.Throws<InvalidOperationException>(static () => ClassOwner.TypeFlag.GetInstanceOrThrow());
        Assert.Throws<InvalidOperationException>(static () => ValueOwner.TypeFlag.GetValueOrThrow());
        Assert.Throws<InvalidOperationException>(static () => ClassOwner.TypeFlag.GetObjectOrThrow<SettingsA>());
        Assert.Throws<InvalidOperationException>(static () => ClassOwner.TypeFlag.GetValueOrThrow<Config>());
        Assert.Throws<InvalidOperationException>(() => flag.GetInstanceOrThrow());
        Assert.Throws<InvalidOperationException>(() => readOnly.GetInstanceOrThrow());
        Assert.Throws<InvalidOperationException>(() => link.GetObjectOrThrow());
        Assert.Throws<InvalidOperationException>(() => valueFlag.GetValueOrThrow());
        Assert.Throws<InvalidOperationException>(() => valueReadOnly.GetValueOrThrow());

        var owner = new OwnerB();
        var settings = new SettingsA();
        var classOwner = new ClassOwner();

        flag.TryRegister(owner);
        link.TryAddObject(settings);
        valueFlag.SetValue(new Config(value: 3));
        ClassOwner.TryRegister(classOwner);
        ValueOwner.TypeFlag.SetValue(new ValueOwner(value: 5));
        ClassOwner.TryAddObject(settings);
        ClassOwner.SetValue(new Config(value: 7));

        Assert.AreSame(classOwner, ClassOwner.TypeFlag.GetInstanceOrThrow());
        Assert.AreEqual(5, ValueOwner.TypeFlag.GetValueOrThrow().Value);
        Assert.AreSame(settings, ClassOwner.TypeFlag.GetObjectOrThrow<SettingsA>());
        Assert.AreEqual(7, ClassOwner.TypeFlag.GetValueOrThrow<Config>().Value);
        Assert.AreSame(owner, flag.GetInstanceOrThrow());
        Assert.AreSame(owner, readOnly.GetInstanceOrThrow());
        Assert.AreSame(settings, link.GetObjectOrThrow());
        Assert.AreEqual(3, valueFlag.GetValueOrThrow().Value);
        Assert.AreEqual(3, valueReadOnly.GetValueOrThrow().Value);
    }

    [Test]
    public void StorageAndStateAreIndependent()
    {
        var flag = default(TypeFlag<OwnerB>);
        var owner = new OwnerB();

        flag.GetLink<OwnerB>().TryAddObject(owner);

        Assert.IsFalse(flag.IsEnabled);

        flag.GetLink<OwnerB>().TryRemoveObject(owner);
        flag.Enable();

        Assert.IsTrue(flag.IsEnabled);
        Assert.IsFalse(flag.TryGetInstance(out _));
    }

    [Test]
    public void UseExtensionsOwner()
    {
        var owner = new ExtensionOwner();

        Assert.IsTrue(ExtensionOwner.Register(owner));
        ExtensionOwner.SetConfig(new Config(value: 9));

        Assert.IsTrue(ExtensionOwner.TypeFlag.IsEnabled);
        Assert.IsTrue(ExtensionOwner.TypeFlag.TryGetInstance(out var instance));
        Assert.AreSame(owner, instance);
        Assert.AreEqual(9, ExtensionOwner.TypeFlag.GetLink<Config>().GetValueOrThrow().Value);
    }

    [Test]
    public void GeneratedAndExtensionApisShareStorage()
    {
        ClassOwner.SetValue(new Config(value: 11));

        Assert.AreEqual(11, default(TypeFlag<ClassOwner>).GetLink<Config>().GetValueOrThrow().Value);
    }

    [Test]
    public async Task WaitUntilEnabledAsyncCompletesWhenEnabled()
    {
        ClassOwner.Enable();
        default(TypeFlag<OwnerB>).Enable();

        await ClassOwner.TypeFlag.WaitUntilEnabledAsync();
        await default(TypeFlag<OwnerB>).WaitUntilEnabledAsync();

        Assert.IsTrue(ClassOwner.TypeFlag.IsEnabled);
    }

    [Test]
    public async Task WaitUntilEnabledAsyncThrowsWhenCancelled()
    {
        var token = new CancellationToken(canceled: true);

        await CaptureExpectedExceptionAsync<OperationCanceledException>(WaitAsync);

        async Task WaitAsync()
        {
            await ClassOwner.TypeFlag.WaitUntilEnabledAsync(token);
        }
    }

    [Test]
    public async Task GetInstanceAsyncReturnsStoredInstance()
    {
        var token = new CancellationToken(canceled: true);

        await CaptureExpectedExceptionAsync<OperationCanceledException>(GetAbsentAsync);

        var stored = new ClassOwner();

        ClassOwner.TryRegister(stored);

        Assert.AreSame(stored, await ClassOwner.TypeFlag.GetInstanceAsync());

        async Task GetAbsentAsync()
        {
            await ClassOwner.TypeFlag.GetInstanceAsync(token);
        }
    }

    [Test]
    public async Task GetValueAsyncReturnsStoredValue()
    {
        ValueOwner.TypeFlag.SetValue(new ValueOwner(value: 4));

        Assert.AreEqual(4, (await ValueOwner.TypeFlag.GetValueAsync()).Value);
    }

    private static void ResetState()
    {
        default(TypeFlag<ClassOwner>).Disable();
        default(TypeFlag<ValueOwner>).Disable();
        default(TypeFlag<Phase>).Disable();
        default(TypeFlag<ExtensionOwner>).Disable();
        default(TypeFlag<OwnerB>).Disable();
        default(TypeFlag<SettingsA>).Disable();
        default(TypeFlag<SettingsB>).Disable();
        default(TypeFlag<Config>).Disable();

        RemoveObject<ClassOwner, ClassOwner>();
        RemoveObject<ClassOwner, SettingsA>();
        RemoveObject<ClassOwner, SettingsB>();
        RemoveObject<ExtensionOwner, ExtensionOwner>();
        RemoveObject<OwnerB, OwnerB>();
        RemoveObject<OwnerB, SettingsA>();
        RemoveObject<OwnerB, SettingsB>();
        RemoveObject<SettingsA, SettingsA>();
        RemoveObject<SettingsB, SettingsB>();

        default(TypeFlag<ValueOwner>).TryRemoveValue(out _);
        default(TypeFlag<Config>).TryRemoveValue(out _);
        default(TypeFlag<ClassOwner>).GetLink<Config>().TryRemoveValue(out _);
        default(TypeFlag<ExtensionOwner>).GetLink<Config>().TryRemoveValue(out _);
    }

    private static void RemoveObject<TOwner, TObject>()
        where TObject : class
    {
        var link = default(TypeFlag<TOwner>).GetLink<TObject>();

        if (link.TryGetObject(out var obj))
        {
            link.TryRemoveObject(obj);
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

    [TypeFlag]
    private sealed partial class ClassOwner
    {
        public static bool Enable()
            => s_typeFlag.Enable();

        public static bool IsEnabledThroughWriter()
            => s_typeFlag.IsEnabled;

        public static bool TryRegister(ClassOwner instance)
            => s_typeFlag.TryRegister(instance);

        public static bool TryUnregister(ClassOwner instance)
            => s_typeFlag.TryUnregister(instance);

        public static bool TryAddObject(SettingsA settings)
            => s_typeFlag.TryAddObject(settings);

        public static void SetValue(Config config)
            => s_typeFlag.SetValue(config);
    }

    [TypeFlag(WriteAccess = TypeFlagAccess.Public)]
    private readonly partial struct ValueOwner
    {
        public readonly int Value;

        public ValueOwner(int value)
        {
            Value = value;
        }
    }

    [TypeFlag(WriteAccess = TypeFlagAccess.Internal, Api = TypeFlagApi.State)]
    private readonly partial struct Phase { }

    [TypeFlag(UseExtensions = true)]
    private sealed partial class ExtensionOwner
    {
        public static bool Register(ExtensionOwner instance)
            => s_typeFlag.TryRegister(instance);

        public static void SetConfig(Config config)
            => s_typeFlag.GetLink<Config>().SetValue(config);
    }

    private sealed class OwnerB { }

    private sealed class SettingsA { }

    private sealed class SettingsB { }

    private readonly struct Config
    {
        public readonly int Value;

        public Config(int value)
        {
            Value = value;
        }
    }
}
