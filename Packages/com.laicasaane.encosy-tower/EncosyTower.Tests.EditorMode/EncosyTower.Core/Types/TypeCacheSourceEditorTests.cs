#if UNITY_EDITOR && !ENFORCE_ENCOSY_RUNTIME_TYPECACHE

using System;
using System.Linq;
using System.Reflection;
using EncosyTower.Types.Internals;
using NUnit.Framework;
using UnityEditor;

namespace EncosyTower.Tests.Core.Types;

public class TypeCacheSourceEditorTests
{
    private const string UNRELATED_ASSEMBLY = "UnityEngine.CoreModule";

    private static readonly string s_assembly = typeof(TypeCacheSourceEditorTests).Assembly.GetName().Name;

    [Test]
    public void GetTypesDerivedFrom_EveryOverload_ReturnsTheDerivedTypes()
    {
        var source = new TypeCacheSourceEditor();
        var expected = new[] { typeof(ProbeDerivedA), typeof(ProbeDerivedB) };

        CollectionAssert.AreEquivalent(expected, source.GetTypesDerivedFrom<ProbeBase>().ToArray());
        CollectionAssert.AreEquivalent(expected, source.GetTypesDerivedFrom<ProbeBase>(s_assembly).ToArray());
        CollectionAssert.AreEquivalent(expected, source.GetTypesDerivedFrom(typeof(ProbeBase)).ToArray());
        CollectionAssert.AreEquivalent(expected, source.GetTypesDerivedFrom(typeof(ProbeBase), s_assembly).ToArray());
    }

    [Test]
    public void GetTypesDerivedFrom_TypeWithoutDerivedTypes_ReturnsEmpty()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(0, source.GetTypesDerivedFrom<ProbeNoDerived>().Length);
        Assert.AreEqual(0, source.GetTypesDerivedFrom<ProbeNoDerived>(s_assembly).Length);
        Assert.AreEqual(0, source.GetTypesDerivedFrom(typeof(ProbeNoDerived)).Length);
        Assert.AreEqual(0, source.GetTypesDerivedFrom(typeof(ProbeNoDerived), s_assembly).Length);
    }

    [Test]
    public void GetTypesDerivedFrom_AssemblyWithoutTheDerivedTypes_ReturnsEmpty()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(0, source.GetTypesDerivedFrom<ProbeBase>(UNRELATED_ASSEMBLY).Length);
        Assert.AreEqual(0, source.GetTypesDerivedFrom(typeof(ProbeBase), UNRELATED_ASSEMBLY).Length);
    }

    [Test]
    public void GetTypesDerivedFrom_RepeatedCalls_ReturnEqualContent()
    {
        var source = new TypeCacheSourceEditor();

        var first = source.GetTypesDerivedFrom<ProbeBase>().ToArray();
        var second = source.GetTypesDerivedFrom<ProbeBase>().ToArray();

        CollectionAssert.AreEquivalent(first, second);
    }

    [Test]
    public void GetTypesDerivedFrom_Length_MatchesTheTypeCacheCount()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(
              TypeCache.GetTypesDerivedFrom<ProbeBase>().Count
            , source.GetTypesDerivedFrom<ProbeBase>().Length
        );
        Assert.AreEqual(
              TypeCache.GetTypesDerivedFrom<ProbeBase>(s_assembly).Count
            , source.GetTypesDerivedFrom<ProbeBase>(s_assembly).Length
        );
    }

    [Test]
    public void GetTypesWithAttribute_EveryOverload_ReturnsTheAttributedTypes()
    {
        var source = new TypeCacheSourceEditor();
        var expected = new[] { typeof(ProbeAttributed) };

        CollectionAssert.AreEquivalent(expected, source.GetTypesWithAttribute<ProbeAttribute>().ToArray());
        CollectionAssert.AreEquivalent(expected, source.GetTypesWithAttribute<ProbeAttribute>(s_assembly).ToArray());
        CollectionAssert.AreEquivalent(expected, source.GetTypesWithAttribute(typeof(ProbeAttribute)).ToArray());
        CollectionAssert.AreEquivalent(
              expected
            , source.GetTypesWithAttribute(typeof(ProbeAttribute), s_assembly).ToArray()
        );
    }

    [Test]
    public void GetTypesWithAttribute_UnusedAttributeOrUnrelatedAssembly_ReturnsEmpty()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(0, source.GetTypesWithAttribute<ProbeUnusedAttribute>().Length);
        Assert.AreEqual(0, source.GetTypesWithAttribute<ProbeUnusedAttribute>(s_assembly).Length);
        Assert.AreEqual(0, source.GetTypesWithAttribute(typeof(ProbeUnusedAttribute)).Length);
        Assert.AreEqual(0, source.GetTypesWithAttribute(typeof(ProbeUnusedAttribute), s_assembly).Length);
        Assert.AreEqual(0, source.GetTypesWithAttribute<ProbeAttribute>(UNRELATED_ASSEMBLY).Length);
        Assert.AreEqual(0, source.GetTypesWithAttribute(typeof(ProbeAttribute), UNRELATED_ASSEMBLY).Length);
    }

    [Test]
    public void GetTypesWithAttribute_Length_MatchesTheTypeCacheCount()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(
              TypeCache.GetTypesWithAttribute<ProbeAttribute>().Count
            , source.GetTypesWithAttribute<ProbeAttribute>().Length
        );
    }

    [Test]
    public void GetFieldsWithAttribute_EveryOverload_ReturnsTheAttributedFields()
    {
        var source = new TypeCacheSourceEditor();
        var expected = new[] { "ProbeAttributed.probeField" };

        CollectionAssert.AreEquivalent(expected, FieldNames(source.GetFieldsWithAttribute<ProbeAttribute>()));
        CollectionAssert.AreEquivalent(expected, FieldNames(source.GetFieldsWithAttribute<ProbeAttribute>(s_assembly)));
        CollectionAssert.AreEquivalent(expected, FieldNames(source.GetFieldsWithAttribute(typeof(ProbeAttribute))));
        CollectionAssert.AreEquivalent(
              expected
            , FieldNames(source.GetFieldsWithAttribute(typeof(ProbeAttribute), s_assembly))
        );
    }

    [Test]
    public void GetFieldsWithAttribute_UnusedAttributeOrUnrelatedAssembly_ReturnsEmpty()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(0, source.GetFieldsWithAttribute<ProbeUnusedAttribute>().Length);
        Assert.AreEqual(0, source.GetFieldsWithAttribute<ProbeUnusedAttribute>(s_assembly).Length);
        Assert.AreEqual(0, source.GetFieldsWithAttribute(typeof(ProbeUnusedAttribute)).Length);
        Assert.AreEqual(0, source.GetFieldsWithAttribute(typeof(ProbeUnusedAttribute), s_assembly).Length);
        Assert.AreEqual(0, source.GetFieldsWithAttribute<ProbeAttribute>(UNRELATED_ASSEMBLY).Length);
        Assert.AreEqual(0, source.GetFieldsWithAttribute(typeof(ProbeAttribute), UNRELATED_ASSEMBLY).Length);
    }

    [Test]
    public void GetFieldsWithAttribute_Length_MatchesTheTypeCacheCount()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(
              TypeCache.GetFieldsWithAttribute<ProbeAttribute>().Count
            , source.GetFieldsWithAttribute<ProbeAttribute>().Length
        );
    }

    [Test]
    public void GetMethodsWithAttribute_EveryOverload_ReturnsTheAttributedMethods()
    {
        var source = new TypeCacheSourceEditor();
        var expected = new[] { "ProbeAttributed.ProbeMethod" };

        CollectionAssert.AreEquivalent(expected, MethodNames(source.GetMethodsWithAttribute<ProbeAttribute>()));
        CollectionAssert.AreEquivalent(
              expected
            , MethodNames(source.GetMethodsWithAttribute<ProbeAttribute>(s_assembly))
        );
        CollectionAssert.AreEquivalent(expected, MethodNames(source.GetMethodsWithAttribute(typeof(ProbeAttribute))));
        CollectionAssert.AreEquivalent(
              expected
            , MethodNames(source.GetMethodsWithAttribute(typeof(ProbeAttribute), s_assembly))
        );
    }

    [Test]
    public void GetMethodsWithAttribute_UnusedAttributeOrUnrelatedAssembly_ReturnsEmpty()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(0, source.GetMethodsWithAttribute<ProbeUnusedAttribute>().Length);
        Assert.AreEqual(0, source.GetMethodsWithAttribute<ProbeUnusedAttribute>(s_assembly).Length);
        Assert.AreEqual(0, source.GetMethodsWithAttribute(typeof(ProbeUnusedAttribute)).Length);
        Assert.AreEqual(0, source.GetMethodsWithAttribute(typeof(ProbeUnusedAttribute), s_assembly).Length);
        Assert.AreEqual(0, source.GetMethodsWithAttribute<ProbeAttribute>(UNRELATED_ASSEMBLY).Length);
        Assert.AreEqual(0, source.GetMethodsWithAttribute(typeof(ProbeAttribute), UNRELATED_ASSEMBLY).Length);
    }

    [Test]
    public void GetMethodsWithAttribute_Length_MatchesTheTypeCacheCount()
    {
        var source = new TypeCacheSourceEditor();

        Assert.AreEqual(
              TypeCache.GetMethodsWithAttribute<ProbeAttribute>().Count
            , source.GetMethodsWithAttribute<ProbeAttribute>().Length
        );
    }

    private static string[] FieldNames(ReadOnlyMemory<FieldInfo> fields)
        => fields.ToArray().Select(static field => $"{field.DeclaringType.Name}.{field.Name}").ToArray();

    private static string[] MethodNames(ReadOnlyMemory<MethodInfo> methods)
        => methods.ToArray().Select(static method => $"{method.DeclaringType.Name}.{method.Name}").ToArray();

    [AttributeUsage(AttributeTargets.All)]
    private sealed class ProbeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.All)]
    private sealed class ProbeUnusedAttribute : Attribute { }

    private abstract class ProbeBase { }

    private sealed class ProbeDerivedA : ProbeBase { }

    private sealed class ProbeDerivedB : ProbeBase { }

    private sealed class ProbeNoDerived { }

    [Probe]
    private sealed class ProbeAttributed
    {
        [Probe]
        public int probeField = 1;

        [Probe]
        public void ProbeMethod() { }
    }
}

#endif
