// com.unity.entities © 2024 Unity Technologies
//
// Licensed under the Unity Companion License for Unity-dependent projects
// (see https://unity3d.com/legal/licenses/unity_companion_license).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS”
// BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.
//
// Please review the license for details on these and other terms and conditions.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen
{
    public static class TypeCreationHelpers
    {
        public const string NEWLINE = "\n";

        public static SourceText GenerateSourceText(
              string openingSource
            , string bodySource
            , string closingSource
            , CancellationToken token
            , Printer? overridePrinter = default
        )
        {
            token.ThrowIfCancellationRequested();

            // DO NOT worry about #if directives
            // Because source generators run after preprocessors,
            // every disabled code will be removed from the compilation context.
            // So there might be no generated code to worry about.

            var printer = overridePrinter ?? new Printer(0, 1024 * 16, token);

            printer.PrintLine(openingSource);
            printer.PrintLine(bodySource);
            printer.PrintLine(closingSource);

            token.ThrowIfCancellationRequested();
            var source = SourceText.From(printer.Result, Encoding.UTF8);
            token.ThrowIfCancellationRequested();

            return source.WithIgnoreUnassignedVariableWarning();
        }

        public static void GenerateOpeningAndClosingSource(
              SyntaxNode containingSyntax
            , CancellationToken token
            , out string openingSource
            , out string closingSource
            , Printer? overridePrinter = default
            , PrinterAction printAdditionalUsings = default
        )
        {
            token.ThrowIfCancellationRequested();
            var printer = overridePrinter ?? new Printer(0, 1024 * 16, token);

            var result = WriteOpeningSyntax_AndReturnClosingSyntax(
                  ref printer
                , containingSyntax
                , token
                , printAdditionalUsings
            );

            openingSource = printer.Result;

            printer.ClearAndIndent(printer.IndentDepth);

            var numClosingBraces = result.NumClosingBraces;

            if (numClosingBraces > 0)
            {
                printer.PrintEndLine();
            }

            for (int i = 0; i < numClosingBraces; i++)
            {
                token.ThrowIfCancellationRequested();

                printer = printer.DecreasedIndent();
                printer.PrintLine("}");
            }

            closingSource = printer.Result;
        }

        /// <summary>
        /// Gets the containing type declarations that <see cref="GenerateOpeningAndClosingSource"/> prints
        /// for <paramref name="containingSyntax"/>, outermost first. Namespaces and using directives are excluded.
        /// </summary>
        public static EquatableArray<ContainingTypeSpec> GetContainingTypeSpecs(
              SyntaxNode containingSyntax
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var (openingSyntaxes, _) = GetOpeningSyntaxes(containingSyntax, token);
            using var builder = ImmutableArrayBuilder<ContainingTypeSpec>.Rent();

            foreach (var syntax in openingSyntaxes)
            {
                token.ThrowIfCancellationRequested();

                if (syntax is not TypeDeclarationSyntax typeSyntax)
                {
                    continue;
                }

                var keyword = typeSyntax is RecordDeclarationSyntax recordSyntax
                    ? $"record {recordSyntax.ClassOrStructKeyword.ValueText}"
                    : typeSyntax.Keyword.ValueText;

                builder.Add(new ContainingTypeSpec(
                      keyword
                    , typeSyntax.Identifier.ToString()
                    , typeSyntax.TypeParameterList?.ToString() ?? string.Empty
                    , typeSyntax.ConstraintClauses.ToString()
                ));
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static ClosingSyntax WriteOpeningSyntax_AndReturnClosingSyntax(
              ref Printer printer
            , SyntaxNode containingTypeSyntax
            , CancellationToken token
            , PrinterAction printAdditionUsings
        )
        {
            token.ThrowIfCancellationRequested();

            var (openingSyntaxes, numClosingBraces) = GetOpeningSyntaxes(containingTypeSyntax, token);

            var uniqueUsings = new HashSet<string>();
            var usings = SyntaxFactory.List<UsingDirectiveSyntax>();

            GetUsings(containingTypeSyntax?.SyntaxTree, uniqueUsings, ref usings, token);

            printer.PrintEndLine();

            foreach (var @using in usings)
            {
                token.ThrowIfCancellationRequested();
                printer.PrintLine(@using.ToString());
            }

            printAdditionUsings?.Invoke(ref printer);

            if (usings.Count > 0)
            {
                printer.PrintEndLine();
            }

            foreach (var syntax in openingSyntaxes)
            {
                token.ThrowIfCancellationRequested();

                switch (syntax)
                {
                    case RecordDeclarationSyntax recordSyntax:
                    {
                        // e.g. class/struct
                        var keyword = recordSyntax.ClassOrStructKeyword.ValueText;

                        // e.g. Outer/Generic<T>
                        var typeName = recordSyntax.Identifier.ToString();
                        var typeParameters = recordSyntax.TypeParameterList?.ToString();

                        // e.g. where T: new()
                        var constraint = recordSyntax.ConstraintClauses.ToString();

                        printer.PrintBeginLine("partial record ").Print(keyword).Print(" ")
                            .Print(typeName).Print(typeParameters).Print(" ").Print(constraint).PrintEndLine();
                        printer.PrintLine("{");
                        printer = printer.IncreasedIndent();
                        break;
                    }

                    case TypeDeclarationSyntax typeSyntax:
                    {
                        // e.g. class/struct
                        var keyword = typeSyntax.Keyword.ValueText;

                        // e.g. Outer/Generic<T>
                        var typeName = typeSyntax.Identifier.ToString();
                        var typeParameters = typeSyntax.TypeParameterList?.ToString();

                        // e.g. where T: new()
                        var constraint = typeSyntax.ConstraintClauses.ToString();

                        printer.PrintBeginLine("partial ").Print(keyword).Print(" ")
                            .Print(typeName).Print(typeParameters).Print(" ").Print(constraint).PrintEndLine();
                        printer.PrintLine("{");
                        printer = printer.IncreasedIndent();
                        break;
                    }

                    case BaseNamespaceDeclarationSyntax namespaceSyntax:
                    {
                        printer.PrintBeginLine("namespace ").Print(namespaceSyntax.Name.ToString()).PrintEndLine();
                        printer.PrintLine("{");
                        printer = printer.IncreasedIndent();

                        var namespaceUsings = namespaceSyntax.Usings;

                        for (var i = namespaceUsings.Count - 1; i >= 0; i--)
                        {
                            token.ThrowIfCancellationRequested();
                            printer.PrintLine(namespaceUsings[i].ToString());
                        }

                        break;
                    }
                }
            }

            if (openingSyntaxes.Count > 0)
            {
                printer.PrintEndLine();
            }

            return new(numClosingBraces);

            static void GetUsings(
                  SyntaxTree syntaxTree
                , HashSet<string> uniqueUsings
                , ref SyntaxList<UsingDirectiveSyntax> usings
                , CancellationToken token
            )
            {
                if (syntaxTree == null)
                {
                    return;
                }

                var currentUsings = syntaxTree.GetCompilationUnitRoot(token).Usings;

                foreach (var @using in currentUsings)
                {
                    token.ThrowIfCancellationRequested();

                    if (uniqueUsings.Add(@using.Name.ToString()))
                    {
                        usings = usings.Add(@using);
                    }
                }
            }
        }

        private static OpeningSyntaxes GetOpeningSyntaxes(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var opening = new Stack<SyntaxNode>();
            var numBracesToClose = 0;
            var parentSyntax = node?.Parent;

            while (parentSyntax != null)
            {
                token.ThrowIfCancellationRequested();

                switch (parentSyntax)
                {
                    case RecordDeclarationSyntax:
                    case TypeDeclarationSyntax:
                    case BaseNamespaceDeclarationSyntax:
                    {
                        opening.Push(parentSyntax);
                        numBracesToClose++;
                        break;
                    }
                }

                parentSyntax = parentSyntax.Parent;
            }

            return new(opening, numBracesToClose);
        }

        private record struct ClosingSyntax(int NumClosingBraces);

        private record struct OpeningSyntaxes(Stack<SyntaxNode> Syntaxes, int NumClosingBraces);
    }
}
