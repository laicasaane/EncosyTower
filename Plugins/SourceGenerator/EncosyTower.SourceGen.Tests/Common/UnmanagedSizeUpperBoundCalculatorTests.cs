using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class UnmanagedSizeUpperBoundCalculatorTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct HostSequential
    {
        public byte A;
        public int B;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HostPacked
    {
        public byte A;
        public short B;
        public int C;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HostNested
    {
        public HostSequential Sequential;
        public byte Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HostGenericSlot<T>
        where T : unmanaged
    {
        public T Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HostGenericFirst
    {
        public byte First;
        public HostGenericSlot<int> Slot;
        public short Last;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HostGenericLast
    {
        public HostGenericSlot<int> Slot;
        public byte First;
        public short Last;
    }

    [TestMethod]
    public void Result_UsesEveryFieldInEqualityAndHashing()
    {
        var value = new UnmanagedSizeUpperBoundResult(true, 8, UnmanagedSizeUpperBoundFailure.None);
        Assert.AreEqual(value, new UnmanagedSizeUpperBoundResult(true, 8, UnmanagedSizeUpperBoundFailure.None));
        Assert.AreEqual(
              value.GetHashCode()
            , new UnmanagedSizeUpperBoundResult(true, 8, UnmanagedSizeUpperBoundFailure.None).GetHashCode()
        );
        Assert.AreNotEqual(
              value
            , new UnmanagedSizeUpperBoundResult(false, 8, UnmanagedSizeUpperBoundFailure.None)
        );
        Assert.AreNotEqual(
              value
            , new UnmanagedSizeUpperBoundResult(true, 9, UnmanagedSizeUpperBoundFailure.None)
        );
        Assert.AreNotEqual(
              value
            , new UnmanagedSizeUpperBoundResult(true, 8, UnmanagedSizeUpperBoundFailure.InvalidLayout)
        );
    }

    [TestMethod]
    public void PrimitiveEnumSequentialExplicitFixedAndClosedGeneric_AreKnown()
    {
        var compilation = Compile(
              """
              public enum E : short { A }
              public struct Empty { }
              public struct Sequential { public byte A; public int B; }
              [System.Runtime.InteropServices.StructLayout(
                  System.Runtime.InteropServices.LayoutKind.Explicit, Size = 5)]
              public struct Explicit
              {
                  [System.Runtime.InteropServices.FieldOffset(0)] public byte A;
                  [System.Runtime.InteropServices.FieldOffset(1)] public int B;
              }
              public unsafe struct Fixed { public fixed int Values[3]; }
              public struct Pair<T> where T : unmanaged { public T A; public T B; }
              public struct Nested { public Pair<int> Pair; public E Value; }
              """
        );

        AssertKnown(compilation, "E", minimum: 2);
        AssertKnown(compilation, "Empty", minimum: 1);
        var sequential = AssertKnown(compilation, "Sequential", Marshal.SizeOf<HostSequential>());
        Assert.IsTrue(sequential.sizeUpperBound >= Marshal.SizeOf<HostSequential>());
        AssertKnown(compilation, "Explicit", minimum: 5);
        AssertKnown(compilation, "Fixed", minimum: 12);
        AssertKnown(
              compilation
            , "Pair`1"
            , minimum: 0
            , typeArgument: compilation.GetSpecialType(SpecialType.System_Int32)
        );
        AssertKnown(compilation, "Nested", minimum: 10);
    }

    [TestMethod]
    public void SupportedPowerOfTwoPacking_HasNoArtificialUpperCap()
    {
        var compilation = Compile(
              """
              [System.Runtime.InteropServices.StructLayout(
                  System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 128)]
              public struct X { public byte A; public long B; }
              """
        );
        AssertKnown(compilation, "X", minimum: 9);
    }

    [TestMethod]
    public void ReferencedStructs_AreOpaqueWhenMetadataImportCanHideStorage()
    {
        var reference = CompileReference(
            """
            public struct ExternalWithHiddenStorage
            {
                public int PublicValue;
                private long hiddenValue;
            }
            public struct ExternalEmpty { }
            """
        );
        var compilation = Compile("public struct X { public ExternalWithHiddenStorage Value; }", reference);
        var external = compilation.GetTypeByMetadataName("ExternalWithHiddenStorage")!;
        var empty = compilation.GetTypeByMetadataName("ExternalEmpty")!;

        AssertFailure(external, UnmanagedSizeUpperBoundFailure.OpaqueExternalStorage);
        AssertFailure(empty, UnmanagedSizeUpperBoundFailure.OpaqueExternalStorage);
    }

    [TestMethod]
    public void MarkerOnlyGeneratedStorage_IsUnknown()
    {
        var compilation = Compile("public struct X { public int Value { get; } }");

        AssertFailure(
              compilation.GetTypeByMetadataName("X")!
            , UnmanagedSizeUpperBoundFailure.GeneratedStorageUnknown
        );
    }

    [TestMethod]
    public void ExplicitOverlap_UsesFixedBufferElementAlignment()
    {
        var compilation = Compile(
            """
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
            public unsafe struct X
            {
                [System.Runtime.InteropServices.FieldOffset(0)] public fixed byte Values[3];
                [System.Runtime.InteropServices.FieldOffset(1)] public int Value;
            }
            """
        );

        var result = AssertKnown(compilation, "X", minimum: 8);
        Assert.AreEqual(8, result.sizeUpperBound);
    }

    [TestMethod]
    public void NestedNonPowerOfTwoSizes_UsePowerOfTwoAlignmentBounds()
    {
        var compilation = Compile(
            """
            public struct ThreeBytes { public byte A; public short B; }
            public struct X { public ThreeBytes First; public byte Second; }
            """
        );

        var result = AssertKnown(compilation, "X", minimum: 8);
        Assert.AreEqual(8, result.sizeUpperBound);
    }

    [TestMethod]
    public void SequentialClosedGenericFieldReordering_UsesOneConservativeUpperBound()
    {
        var compilation = Compile(
            """
            public struct GenericSlot<T> where T : unmanaged { public T Value; }
            public struct GenericFirst { public byte First; public GenericSlot<int> Slot; public short Last; }
            public struct GenericLast { public GenericSlot<int> Slot; public byte First; public short Last; }
            """
        );
        var first = AssertKnown(compilation, "GenericFirst", minimum: 20);
        var last = AssertKnown(compilation, "GenericLast", minimum: 20);

        Assert.AreEqual(20, first.sizeUpperBound);
        Assert.AreEqual(first.sizeUpperBound, last.sizeUpperBound);
        Assert.IsTrue(first.sizeUpperBound >= Marshal.SizeOf<HostGenericFirst>());
        Assert.IsTrue(last.sizeUpperBound >= Marshal.SizeOf<HostGenericLast>());
    }

    [TestMethod]
    public void AcceptedCorpus_UpperBoundsIndependentlyMeasuredHostSizes()
    {
        var compilation = Compile(
            """
            public struct HostSequential { public byte A; public int B; }
            public struct HostPacked { public byte A; public short B; public int C; }
            public struct HostNested { public HostSequential Sequential; public byte Value; }
            """
        );

        Assert.IsTrue(AssertKnown(compilation, "HostSequential", 0).sizeUpperBound >= Marshal.SizeOf<HostSequential>());
        Assert.IsTrue(AssertKnown(compilation, "HostPacked", 0).sizeUpperBound >= Marshal.SizeOf<HostPacked>());
        Assert.IsTrue(AssertKnown(compilation, "HostNested", 0).sizeUpperBound >= Marshal.SizeOf<HostNested>());
    }

    [DataTestMethod]
    [DataRow("public struct X<T> where T : unmanaged { public T Value; }", UnmanagedSizeUpperBoundFailure.OpenGeneric)]
    [DataRow("public struct X { public string Value; }", UnmanagedSizeUpperBoundFailure.ManagedStorage)]
    [DataRow("public ref struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.RefLikeType)]
    [DataRow("public unsafe struct X { public int* Value; }", UnmanagedSizeUpperBoundFailure.PointerStorage)]
    [DataRow("public struct X { public System.IntPtr Value; }", UnmanagedSizeUpperBoundFailure.PointerSizedStorage)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)] " +
        "public struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.AutoLayout)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout(" +
        "System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 3)] " +
        "public struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.UnsupportedPacking)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout(" +
        "System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 256)] " +
        "public struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.UnsupportedPacking)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout((System.Runtime.InteropServices.LayoutKind)99)] " +
        "public struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.InvalidLayout)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout(" +
        "System.Runtime.InteropServices.LayoutKind.Sequential, Size = -1)] " +
        "public struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.InvalidLayout)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)] " +
        "public struct X<T> where T : unmanaged { [System.Runtime.InteropServices.FieldOffset(0)] public T Value; }",
        UnmanagedSizeUpperBoundFailure.InvalidGenericExplicitLayout)]
    [DataRow(
        "[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)] " +
        "public struct X { public int Value; }", UnmanagedSizeUpperBoundFailure.InvalidLayout)]
    [DataRow("public unsafe struct X { public fixed int Values[0]; }", UnmanagedSizeUpperBoundFailure.InvalidLayout)]
    [DataRow("public struct X { public X Value; }", UnmanagedSizeUpperBoundFailure.RecursiveLayout)]
    [DataRow(
        "public unsafe struct X { public fixed int Values[1000000000]; }",
        UnmanagedSizeUpperBoundFailure.SizeOverflow)]
    public void InvalidShapes_ReturnFirstFailure(string source, UnmanagedSizeUpperBoundFailure failure)
    {
        var compilation = Compile(source);
        var result = UnmanagedSizeUpperBoundCalculator.Calculate(
              (compilation.GetTypeByMetadataName("X") ?? compilation.GetTypeByMetadataName("X`1"))!
            , default
        );
        Assert.IsFalse(result.isKnown);
        Assert.AreEqual(0, result.sizeUpperBound);
        Assert.AreEqual(failure, result.failure);
    }

    private static UnmanagedSizeUpperBoundResult AssertKnown(
          CSharpCompilation compilation
        , string metadataName
        , int minimum
        , ITypeSymbol? typeArgument = null
    )
    {
        var type = compilation.GetTypeByMetadataName(metadataName)!;

        if (typeArgument is not null)
        {
            type = type.Construct(typeArgument);
        }

        var result = UnmanagedSizeUpperBoundCalculator.Calculate(type, default);
        Assert.IsTrue(result.isKnown, result.failure.ToString());
        Assert.IsTrue(result.sizeUpperBound >= minimum);
        Assert.AreEqual(UnmanagedSizeUpperBoundFailure.None, result.failure);
        return result;
    }

    private static void AssertFailure(ITypeSymbol type, UnmanagedSizeUpperBoundFailure failure)
    {
        var result = UnmanagedSizeUpperBoundCalculator.Calculate(type, default);
        Assert.IsFalse(result.isKnown);
        Assert.AreEqual(0, result.sizeUpperBound);
        Assert.AreEqual(failure, result.failure);
    }

    private static void AssertExactKnown(ITypeSymbol type, int expected)
    {
        var result = UnmanagedSizeUpperBoundCalculator.Calculate(type, default);

        Assert.IsTrue(result.isKnown, result.failure.ToString());
        Assert.AreEqual(expected, result.sizeUpperBound);
        Assert.AreEqual(UnmanagedSizeUpperBoundFailure.None, result.failure);
    }

    private static CSharpCompilation Compile(string source, params MetadataReference[] references)
        => CSharpCompilation.Create(
              "UpperBoundInput"
            , new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10)) }
            , new MetadataReference[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StructLayoutAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(
                    typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location
                ),
              }.Concat(references)
            , new CSharpCompilationOptions(
                  OutputKind.DynamicallyLinkedLibrary
                , allowUnsafe: true
                , metadataImportOptions: MetadataImportOptions.Public
            )
        );

    private static MetadataReference CompileReference(string source)
    {
        using var stream = new MemoryStream();
        var result = Compile(source).Emit(stream);
        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

}
