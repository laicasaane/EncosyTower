using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Tests.Common;

[TestClass]
public sealed class TypeCreationHelpersTests
{
    [TestMethod]
    public void GenerateOpeningAndClosingSource_NestedGenericRecord_PreservesExactText()
    {
        const string SOURCE = """
            using System;
            using System.Collections.Generic;

            namespace Example
            {
                using Example.Zeta;
                using Example.Alpha;

                partial class Container<T>
                    where T : class
                {
                    partial record struct Entry<TValue>
                        where TValue : struct
                    {
                        private void Target()
                        {
                        }
                    }
                }
            }

            """;
        const string EXPECTED_OPENING = """

            using System;
            using System.Collections.Generic;

            namespace Example
            {
                using Example.Alpha;
                using Example.Zeta;
                partial class Container<T> where T : class
                {
                    partial record struct Entry<TValue> where TValue : struct
                    {


            """;
        const string EXPECTED_CLOSING = """

                    }
                }
            }

            """;

        var root = CSharpSyntaxTree.ParseText(SOURCE).GetCompilationUnitRoot();
        var target = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        TypeCreationHelpers.GenerateOpeningAndClosingSource(
              target
            , CancellationToken.None
            , out var openingSource
            , out var closingSource
        );

        Assert.AreEqual(EXPECTED_OPENING, openingSource);
        Assert.AreEqual(EXPECTED_CLOSING, closingSource);
    }
}
