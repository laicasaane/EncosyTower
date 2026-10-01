using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen.Tests.Databases.Authoring;

[TestClass]
public sealed class DatabaseAuthoringPublicAttributeTests
{
    [TestMethod]
    public async Task TransposeAttribute_CompilesWithoutBakingSheetReference()
    {
        var root = FindRepositoryRoot();
        var attributePath = Path.Combine(
              root
            , "Packages"
            , "com.laicasaane.encosy-tower"
            , "EncosyTower.Data"
            , "Databases.Authoring"
            , "Annotations"
            , "TransposeAttribute.cs"
        );
        var attributeSource = await File.ReadAllTextAsync(attributePath);
        var references = await TestReferenceHelper.ResolveCompilationReferencesAsync();
        var filteredReferences = references
            .Where(static reference => IsForbiddenReference(reference.Display ?? string.Empty) == false)
            .ToArray();
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);
        var syntaxTrees = new[] {
            CSharpSyntaxTree.ParseText(
                  SourceText.From(attributeSource, Encoding.UTF8)
                , parseOptions
                , path: "TransposeAttribute.cs"
            ),
            CSharpSyntaxTree.ParseText(
                  SourceText.From(
                      """
                      using EncosyTower.Databases.Authoring;

                      public sealed class Database
                      {
                          [Transpose]
                          public int Table { get; }
                      }
                      """
                    , Encoding.UTF8
                  )
                , parseOptions
                , path: "Consumer.cs"
            ),
        };
        var compilation = CSharpCompilation.Create(
              "TransposeAttributeConsumer"
            , syntaxTrees
            , filteredReferences
            , new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var diagnostics = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Warning or DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(
              0
            , diagnostics.Length
            , string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.ToString()))
        );
    }

    private static bool IsForbiddenReference(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var name = Path.GetFileNameWithoutExtension(path);
        return name.StartsWith("BakingSheet", StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, "EncosyTower.Data", StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent)
        {
            var marker = Path.Combine(
                  directory.FullName
                , "Plugins"
                , "SourceGenerator"
                , "EncosyTower.SourceGen.slnx"
            );

            if (File.Exists(marker))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException($"Could not locate the repository from '{AppContext.BaseDirectory}'.");
    }
}
