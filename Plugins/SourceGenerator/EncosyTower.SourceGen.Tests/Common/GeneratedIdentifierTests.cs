using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class GeneratedIdentifierTests
{
    [DataTestMethod]
    [DataRow("A.B_C", "I_A_x002EB_uC")]
    [DataRow("A_B.C", "I_A_uB_x002EC")]
    [DataRow("Game.Client", "I_Game_x002EClient")]
    [DataRow("A B:C-D", "I_A_x0020B_x003AC_x002DD")]
    [DataRow("Å", "I__x00C5")]
    [DataRow("😀", "I__xD83D_xDE00")]
    public void ToValidIdentifier_PreservesV1Encoding(string input, string expected)
    {
        Assert.AreEqual(expected, input.ToValidIdentifier());
    }

    [TestMethod]
    public void ToValidIdentifier_BuilderReuse_PreservesV1Contract()
    {
        var builder = new StringBuilder("retained");

        Assert.AreEqual(string.Empty, ((string)null!).ToValidIdentifier(builder));
        Assert.AreEqual("retained", builder.ToString());
        Assert.AreEqual(string.Empty, string.Empty.ToValidIdentifier(builder));
        Assert.AreEqual("retained", builder.ToString());
        Assert.AreEqual("I_A_uB", "A_B".ToValidIdentifier(builder));
        Assert.AreEqual("I_A_uB", builder.ToString());
    }

    [DataTestMethod]
    [DataRow("AZaz09_", "AZaz09_")]
    [DataRow("class", "I_class")]
    [DataRow("record", "record")]
    [DataRow("123", "I_123")]
    [DataRow(" ", "__u0000020__")]
    [DataRow(":", "__sc__")]
    [DataRow("?", "__q__")]
    [DataRow("*", "__a__")]
    [DataRow(".", "___")]
    [DataRow("-", "__ds__")]
    [DataRow("+", "__p__")]
    [DataRow("`", "__bt__")]
    [DataRow("<", "__lt__")]
    [DataRow(">", "__gt__")]
    [DataRow("A.B_C", "A___B_C")]
    [DataRow("A B:C-D", "A__u0000020__B__sc__C__ds__D")]
    [DataRow("Å", "Å")]
    [DataRow("ж", "ж")]
    [DataRow("变量", "变量")]
    [DataRow("A\u0301\u203F", "A\u0301\u203F")]
    [DataRow("ᐧжᑌ", "ᐧжᑌ")]
    [DataRow("___", "___")]
    [DataRow("😀", "__u001F600__")]
    [DataRow("A[]", "A__u000005B____u000005D__")]
    [DataRow("A<B>+C`1?*&", "A__lt__B__gt____p__C__bt__1__q____a____u0000026__")]
    public void ToValidIdentifierV2_EncodesVisualMapping(string input, string expected)
    {
        Assert.AreEqual(expected, input.ToValidIdentifierV2());
    }

    [TestMethod]
    public void ToValidIdentifierV2_EncodesSurrogatesAndReservedSymbols()
    {
        Assert.AreEqual("__u1D800__", new string('\uD800', 1).ToValidIdentifierV2());
        Assert.AreEqual("__u1DC00__", new string('\uDC00', 1).ToValidIdentifierV2());
        Assert.AreEqual("ж", "ж".ToValidIdentifierV2());
        Assert.AreEqual("I_class", "I_class".ToValidIdentifierV2());
        Assert.AreEqual("I_class", "class".ToValidIdentifierV2());
        Assert.AreEqual("___", ".".ToValidIdentifierV2());
        Assert.AreEqual("___", "___".ToValidIdentifierV2());
        Assert.AreEqual("__u0000020__", " ".ToValidIdentifierV2());
        Assert.AreEqual("__u0000020__", "__u0000020__".ToValidIdentifierV2());
        Assert.AreEqual("A__u1D800__B", "A\uD800B".ToValidIdentifierV2());
        Assert.AreEqual("__u1DC00____u1D800__", "\uDC00\uD800".ToValidIdentifierV2());
        Assert.AreEqual("__u1D800____u1D800__", "\uD800\uD800".ToValidIdentifierV2());
        Assert.AreEqual("__u0000000__", "\0".ToValidIdentifierV2());
        Assert.AreEqual("__u000FFFF__", "\uFFFF".ToValidIdentifierV2());
        AssertSupplementaryIdentifierHandling(0x10000, "__u0010000__");
        AssertSupplementaryIdentifierHandling(0x10400, "__u0010400__");
        AssertSupplementaryIdentifierHandling(0x10FFFF, "__u010FFFF__");
        Assert.AreEqual("I_\u0301Value", "\u0301Value".ToValidIdentifierV2());
    }

    [TestMethod]
    public void ToValidIdentifierV2_BuilderAndRoslynCompilationRemainValid()
    {
        var builder = new StringBuilder("retained");

        Assert.AreEqual(string.Empty, ((string)null!).ToValidIdentifierV2(builder));
        Assert.AreEqual("retained", builder.ToString());
        Assert.AreEqual(string.Empty, string.Empty.ToValidIdentifierV2(builder));
        Assert.AreEqual("retained", builder.ToString());
        Assert.AreEqual("A___B_C", "A.B_C".ToValidIdentifierV2(builder));
        Assert.AreEqual("A___B_C", builder.ToString());

        var inputs = new[] {
            "class", "record", "123", "Int32[,][]", "Å", "ж", "变量", "😀", "<", char.ConvertFromUtf32(0x10400),
        };

        foreach (var input in inputs)
        {
            var identifier = input.ToValidIdentifierV2();
            var source = $$"""
                internal sealed class GeneratedIdentifierInput
                {
                    private int {{identifier}};

                    internal int Read()
                    {
                        int {{identifier}} = 0;
                        return {{identifier}} + this.{{identifier}};
                    }
                }
                """;

            CreateCompilation(source, out _);
        }
    }


    [TestMethod]
    public void IdentifierHelpers_NullAndEmpty_ReturnEmpty()
    {
        ITypeSymbol symbol = null!;

        Assert.AreEqual(string.Empty, ((string)null!).ToValidIdentifier());
        Assert.AreEqual(string.Empty, string.Empty.ToValidIdentifier());
        Assert.AreEqual(string.Empty, symbol.ToMetadataIdentity());
        Assert.AreEqual(string.Empty, symbol.ToValidIdentifier());
        Assert.AreEqual(string.Empty, ((string)null!).ToValidIdentifierV2());
    }

    [TestMethod]
    public void NormalizeAuthoredIdentifier_PreservesAuthoredIdentifierRules()
    {
        Assert.AreEqual("A_B_C__D\u1438int\u1433Array", "global::A B:C-D<int>[]".NormalizeAuthoredIdentifier());
        Assert.AreEqual("@class", "class".EscapeCSharpIdentifier());
        Assert.AreEqual("@record", "record".EscapeCSharpIdentifier());
        Assert.AreEqual("Value", "Value".EscapeCSharpIdentifier());
    }

    [TestMethod]
    public void ToMetadataIdentity_NormalizesBoundTypeShapes()
    {
        const string source = """
            #nullable enable

            namespace TestTypes
            {
                using Alias = A.B_C;

                namespace A
                {
                    public sealed class B_C { }
                }

                namespace A_B
                {
                    public sealed class C { }
                }

                namespace @namespace
                {
                    public sealed class KeywordNamespaceType { }
                }

                public sealed class Outer<T>
                {
                    public sealed class Inner<U> { }
                }

                public sealed class @class { }

                public unsafe sealed class Holder
                {
                    public Alias Alias = null!;
                    public A.B_C Direct = null!;
                    public A_B.C Collision = null!;
                    public Outer<int>.Inner<string> Nested = null!;
                    public int[,] Matrix = null!;
                    public int? Nullable;
                    public (int Id, string Name) NamedTuple;
                    public (int, string) UnnamedTuple;
                    public dynamic Dynamic = null!;
                    public string? NullableReference;
                    public string PlainReference = null!;
                    public @class Keyword = null!;
                    public @namespace.KeywordNamespaceType KeywordNamespace = null!;
                    public int* Pointer;
                }
            }
            """;

        var fields = GetFieldTypes(source);

        Assert.AreEqual("TestTypes.A.B_C", fields["Alias"].ToMetadataIdentity());
        Assert.AreEqual(fields["Alias"].ToMetadataIdentity(), fields["Direct"].ToMetadataIdentity());
        Assert.AreNotEqual(fields["Direct"].ToValidIdentifier(), fields["Collision"].ToValidIdentifier());
        Assert.AreEqual(
              "TestTypes.Outer`1<System.Int32>+Inner`1<System.String>"
            , fields["Nested"].ToMetadataIdentity()
        );
        Assert.AreEqual("System.Int32[,]", fields["Matrix"].ToMetadataIdentity());
        Assert.AreEqual("System.Nullable`1<System.Int32>", fields["Nullable"].ToMetadataIdentity());
        Assert.AreEqual(fields["NamedTuple"].ToMetadataIdentity(), fields["UnnamedTuple"].ToMetadataIdentity());
        Assert.AreEqual("System.ValueTuple`2<System.Int32,System.String>", fields["NamedTuple"].ToMetadataIdentity());
        Assert.AreEqual("System.Object", fields["Dynamic"].ToMetadataIdentity());
        Assert.AreEqual(
              fields["NullableReference"].ToMetadataIdentity()
            , fields["PlainReference"].ToMetadataIdentity()
        );
        Assert.AreEqual("TestTypes.class", fields["Keyword"].ToMetadataIdentity());
        Assert.AreEqual("TestTypes.namespace.KeywordNamespaceType", fields["KeywordNamespace"].ToMetadataIdentity());
        Assert.AreEqual("System.Int32*", fields["Pointer"].ToMetadataIdentity());
    }

    [TestMethod]
    public void ToMetadataIdentity_TypeParametersIncludeDeclaringOwnerAndOrdinal()
    {
        const string source = """
            namespace TestTypes
            {
                public sealed class Owner<TFirst, TSecond>
                {
                    public void Method<TMethod>(TMethod value) { }
                }
            }
            """;

        var compilation = CreateCompilation(source, out var tree);
        var model = compilation.GetSemanticModel(tree);
        var root = tree.GetRoot();
        var owner = (INamedTypeSymbol)model.GetDeclaredSymbol(
            root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single()
        )!;
        var method = (IMethodSymbol)model.GetDeclaredSymbol(
            root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single()
        )!;

        Assert.AreEqual("TestTypes.Owner`2!0", owner.TypeParameters[0].ToMetadataIdentity());
        Assert.AreEqual("TestTypes.Owner`2!1", owner.TypeParameters[1].ToMetadataIdentity());
        Assert.AreEqual("TestTypes.Owner`2+Method``1!!0", method.TypeParameters[0].ToMetadataIdentity());
    }

    private static Dictionary<string, ITypeSymbol> GetFieldTypes(string source)
    {
        var compilation = CreateCompilation(source, out var tree);
        var model = compilation.GetSemanticModel(tree);

        return tree.GetRoot()
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .ToDictionary(
                  static declaration => declaration.Identifier.ValueText
                , declaration => ((IFieldSymbol)model.GetDeclaredSymbol(declaration)!).Type
                , StringComparer.Ordinal
            );
    }

    private static void AssertSupplementaryIdentifierHandling(int scalar, string fallback)
    {
        var value = char.ConvertFromUtf32(scalar);
        var tree = CSharpSyntaxTree.ParseText(
              $"internal sealed class C {{ private int I_{value}Value; }}"
            , new CSharpParseOptions(LanguageVersion.CSharp10)
        );
        var isIdentifierPart = tree.GetDiagnostics().Any(
            static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error
        ) == false;
        var expected = isIdentifierPart ? value : fallback;
        Assert.AreEqual(expected, value.ToValidIdentifierV2());

        if (isIdentifierPart)
        {
            CreateCompilation(tree.ToString(), out _);
        }
    }

    private static CSharpCompilation CreateCompilation(string source, out SyntaxTree tree)
    {
        tree = CSharpSyntaxTree.ParseText(
              source
            , new CSharpParseOptions(LanguageVersion.CSharp10)
            , path: "GeneratedIdentifierInput.cs"
        );

        var compilation = CSharpCompilation.Create(
              "GeneratedIdentifierTests"
            , new[] { tree }
            , new[] {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(
                    typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location
                ),
            }
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        Assert.IsFalse(errors.Any(), string.Join("\n", errors));
        return compilation;
    }
}
