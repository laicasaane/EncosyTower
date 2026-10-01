#if UNITY_EDITOR

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using EncosyTower.Common;
using EncosyTower.Editor;
using NUnit.Framework;

namespace EncosyTower.Tests.Editor
{
    public class TypeFinderTests
    {
        [Test]
        public void Find_AllOverloadsLocateSecondaryTypeInPackageFile()
        {
            var type = typeof(TypeFinderPackageFixture);
            var expected = FindDeclarationLine(type.Name);
            AssertScript(TypeFinder.Find(type.Name), expected);
            AssertScript(TypeFinder.Find(type.Name, type.Namespace), expected);
            AssertScript(TypeFinder.Find(type.Name, type.Namespace, type.Assembly.GetName().Name), expected);
        }

        [Test]
        public void Find_MissingOrWrongIdentityReturnsNone()
        {
            Assert.IsFalse(TypeFinder.Find("EncosyTypeFinderMissing_598A179BC763").HasValue);
            Assert.IsFalse(TypeFinder.Find(nameof(TypeFinderPackageFixture), "Wrong.Namespace").HasValue);
            Assert.IsFalse(TypeFinder.Find(nameof(TypeFinderPackageFixture), "", "Missing.Assembly").HasValue);
            Assert.IsFalse(TypeFinder.Find(null).HasValue);
            Assert.IsFalse(TypeFinder.Find(string.Empty).HasValue);
        }

        [Test]
        public void Find_NestedGenericUsesFullMetadataName()
        {
            var type = typeof(TypeFinderPackageFixture.Nested<>);
            var name = nameof(TypeFinderPackageFixture) + "+" + type.Name;
            var result = TypeFinder.Find(name, type.Namespace, type.Assembly.FullName);
            AssertScript(result, FindDeclarationLine("Nested<T>"));
        }

        [Test]
        public void Parser_TracksNamespacesNestedGenericsAndPhysicalLines()
        {
            const string SOURCE = "namespace One.Two\r\n{\r\n"
                + "partial class Outer<T>\r\n{\r\npublic struct Inner<T1,T2> {}\r\n}\r\n}\r\n"
                + "namespace Three;\r\npublic interface Other {}\r\n";
            var declarations = Parse(SOURCE);
            Assert.AreEqual(3, declarations.Length);
            AssertDeclaration(declarations, 0, "One.Two", "Outer`1", 3);
            AssertDeclaration(declarations, 1, "One.Two", "Outer`1+Inner`2", 5);
            AssertDeclaration(declarations, 2, "Three", "Other", 9);
        }

        [Test]
        public void Parser_IgnoresCommentsStringsAttributesAndMemberBodies()
        {
            const string SOURCE = "// class Comment {}\n/* namespace Wrong { class Comment {} } */\n"
                + "[Example(\"class Attribute {}\")] class Real\n{\n"
                + "string ordinary = \"class String {}\";\n"
                + "string verbatim = @\"class String {}\";\n"
                + "string interpolated = $\"{string.Join(\"class String {}\", values)}\";\n"
                + "string raw = \"\"\"\nclass Raw {}\n\"\"\";\n"
                + "void Method() { var fake = \"class Fake {}\"; }\n"
                + "public class Nested {}\n}\n";
            var declarations = Parse(SOURCE);
            Assert.AreEqual(2, declarations.Length);
            AssertDeclaration(declarations, 0, "", "Real", 3);
            AssertDeclaration(declarations, 1, "", "Real+Nested", 12);
        }

        [Test]
        public void Parser_EvaluatesNestedConditionalCompilationAndLocalDefines()
        {
            const string SOURCE = "#define LOCAL\n#if FEATURE && (!OTHER || false)\n"
                + "#if LOCAL == true\nclass Active {}\n#else\nclass Inactive {}\n#endif\n"
                + "#elif OTHER\nclass Inactive {}\n#else\nclass Inactive {}\n#endif\n"
                + "#undef LOCAL\n#if LOCAL\nclass Inactive {}\n#endif\nclass Last {}\n";
            var declarations = Parse(SOURCE, "FEATURE");
            Assert.AreEqual(2, declarations.Length);
            AssertDeclaration(declarations, 0, "", "Active", 4);
            AssertDeclaration(declarations, 1, "", "Last", 17);
        }

        [Test]
        public void Parser_RecordsEnumsDelegatesAndEscapedNames()
        {
            const string SOURCE = "namespace @event;\nrecord struct Item(int Value);\n"
                + "delegate System.Func<int, string> Factory<T>(T value);\n"
                + "enum Choice { First, Second }\nclass @class {}\n";
            var declarations = Parse(SOURCE);
            Assert.AreEqual(4, declarations.Length);
            AssertDeclaration(declarations, 0, "event", "Item", 2);
            AssertDeclaration(declarations, 1, "event", "Factory`1", 3);
            AssertDeclaration(declarations, 2, "event", "Choice", 4);
            AssertDeclaration(declarations, 3, "event", "class", 5);
        }

        [Test]
        public void Parser_EscapedKeywordsInMembersAndUsingAliasesAreNotDeclarations()
        {
            const string SOURCE = "using @namespace = System;\nclass Real\n{\n"
                + "int @class;\nint @struct;\npublic class Nested {}\n}\n";
            var declarations = Parse(SOURCE);
            Assert.AreEqual(2, declarations.Length);
            AssertDeclaration(declarations, 0, "", "Real", 2);
            AssertDeclaration(declarations, 1, "", "Real+Nested", 6);
        }

        [Test]
        public void Parser_DecodesUnicodeEscapesAndCombiningCharactersInIdentifiers()
        {
            var declarations = Parse("namespace Sc\\u006Fpe; class \\u0054ype {} class Cafe\u0301 {}\n");
            Assert.AreEqual(2, declarations.Length);
            AssertDeclaration(declarations, 0, "Scope", "Type", 1);
            AssertDeclaration(declarations, 1, "Scope", "Cafe\u0301", 1);
        }

        [TestCase("@\"\"\"hello\"")]
        [TestCase("$@\"\"\"hello\"")]
        [TestCase("@$\"\"\"hello\"")]
        public void Parser_VerbatimLeadingEscapedQuotePreservesFollowingDeclarations(string literal)
        {
            var declarations = Parse("class Outer { string Text = " + literal + "; public class Nested {} }");
            Assert.AreEqual(2, declarations.Length);
            AssertDeclaration(declarations, 0, "", "Outer", 1);
            AssertDeclaration(declarations, 1, "", "Outer+Nested", 1);
        }

        [TestCase("(int X, int Y)")]
        [TestCase("ref readonly (int X, (int Y, int Z) Other)")]
        [TestCase("System.Func<(int X, int Y)>")]
        public void Parser_DelegateTupleReturnTypesPreserveDelegateName(string returnType)
        {
            var declarations = Parse("delegate " + returnType + " Factory<T>();");
            Assert.AreEqual(1, declarations.Length);
            AssertDeclaration(declarations, 0, "", "Factory`1", 1);
        }

        [Test]
        public void Parser_FunctionPointerFieldsAreNotDelegateDeclarations()
        {
            var declarations = Parse("unsafe class Outer { delegate*<void> Callback; public class Nested {} }");
            Assert.AreEqual(2, declarations.Length);
            AssertDeclaration(declarations, 0, "", "Outer", 1);
            AssertDeclaration(declarations, 1, "", "Outer+Nested", 1);
        }

        [TestCase("First", "Second", "Same", "Same")]
        [TestCase("Same", "Same", "First", "Second")]
        public void Candidates_DistinctNamespaceOrAssemblyIsAmbiguous(
              string firstNamespace
            , string secondNamespace
            , string firstAssembly
            , string secondAssembly
        )
        {
            WithFiles((folder, firstPath, secondPath) => {
                File.WriteAllText(firstPath, $"namespace {firstNamespace} {{ class Candidate {{}} }}");
                File.WriteAllText(secondPath, $"namespace {secondNamespace} {{ class Candidate {{}} }}");
                var sources = Sources((firstPath, firstAssembly), (secondPath, secondAssembly));
                Assert.IsFalse(FindCandidates(sources, "Candidate", null).HasValue);

                if (firstNamespace == secondNamespace)
                {
                    Assert.IsFalse(FindCandidates(sources, "Candidate", firstNamespace).HasValue);
                }
                else
                {
                    var result = FindCandidates(sources, "Candidate", firstNamespace).GetValueOrThrow();
                    Assert.AreEqual(firstPath, result.AbsolutePath);
                }
            });
        }

        [Test]
        public void Candidates_PartialDeclarationsPreferExactFilenameThenOrdinalPath()
        {
            WithFiles((folder, firstPath, secondPath) => {
                File.WriteAllText(firstPath, "namespace Scope { partial class Candidate {} }");
                File.WriteAllText(secondPath, "\nnamespace Scope { partial class Candidate {} }");
                var sources = Sources((secondPath, "Assembly"), (firstPath, "Assembly"));
                var result = FindCandidates(sources, "Candidate", "Scope");
                Assert.AreEqual(firstPath, result.GetValueOrThrow().AbsolutePath);
                Assert.AreEqual(1, result.GetValueOrThrow().LineNumber);

                var exact = Path.Combine(folder, "Candidate.cs");
                File.WriteAllText(exact, "\n\nnamespace Scope { partial class Candidate {} }");
                sources = Sources((firstPath, "Assembly"), (exact, "Assembly"));
                result = FindCandidates(sources, "Candidate", "Scope");
                Assert.AreEqual(exact, result.GetValueOrThrow().AbsolutePath);
                Assert.AreEqual(3, result.GetValueOrThrow().LineNumber);
            });
        }

        [Test]
        public void Candidates_RechecksEditedAndDeletedFiles()
        {
            WithFiles((folder, firstPath, secondPath) => {
                File.WriteAllText(firstPath, "class Candidate {}");
                var sources = Sources((firstPath, "Assembly"));
                Assert.AreEqual(1, FindCandidates(sources, "Candidate", "").GetValueOrThrow().LineNumber);
                File.WriteAllText(firstPath, "\n\nclass Candidate {}");
                Assert.AreEqual(3, FindCandidates(sources, "Candidate", "").GetValueOrThrow().LineNumber);
                File.Delete(firstPath);
                Assert.IsFalse(FindCandidates(sources, "Candidate", "").HasValue);
            });
        }

        private static Array Parse(string text, params string[] defines)
            => (Array)GetMethod("ParseDeclarations").Invoke(null, new object[] { text, defines });

        private static void AssertDeclaration(Array declarations, int index, string scope, string name, int line)
        {
            var declaration = declarations.GetValue(index);
            var type = declaration.GetType();
            Assert.AreEqual(scope, type.GetProperty("Namespace").GetValue(declaration));
            Assert.AreEqual(name, type.GetProperty("Name").GetValue(declaration));
            Assert.AreEqual(line, type.GetProperty("LineNumber").GetValue(declaration));
        }

        private static IList Sources(params (string Path, string Assembly)[] files)
        {
            var sourceType = typeof(TypeFinder).GetNestedType("SourceFile", BindingFlags.NonPublic);
            var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(sourceType);
            var sources = (IList)Activator.CreateInstance(listType);

            foreach (var file in files)
            {
                sources.Add(Activator.CreateInstance(sourceType, file.Path, file.Assembly, Array.Empty<string>()));
            }

            return sources;
        }

        private static Option<ScriptFileInfo> FindCandidates(IList sources, string name, string typeNamespace)
        {
            var queryType = typeof(TypeFinder).GetNestedType("Query", BindingFlags.NonPublic);
            var query = Activator.CreateInstance(queryType, name, typeNamespace, null);
            return (Option<ScriptFileInfo>)GetMethod("FindInCandidates").Invoke(null, new[] { sources, query });
        }

        private static MethodInfo GetMethod(string name)
            => typeof(TypeFinder).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        private static void WithFiles(Action<string, string, string> action)
        {
            var folder = Path.Combine(Path.GetTempPath(), "EncosyTypeFinderTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);

            try
            {
                action(folder, Path.Combine(folder, "First.cs"), Path.Combine(folder, "Second.cs"));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        private static int FindDeclarationLine(string declarationName)
        {
            const string RELATIVE_PATH = "EncosyTower.Tests.EditorMode/EncosyTower.Core/Editor/TypeFinderTests.cs";
            const string ASSET_PATH = "Packages/com.laicasaane.encosy-tower/" + RELATIVE_PATH;
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(ASSET_PATH);
            var path = package == null
                ? Path.Combine(EditorAPI.ProjectPath, ASSET_PATH)
                : Path.Combine(package.resolvedPath, RELATIVE_PATH);
            var lines = File.ReadAllLines(path);

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().StartsWith("public class " + declarationName, StringComparison.Ordinal))
                {
                    return i + 1;
                }
            }

            Assert.Fail("Fixture declaration is missing.");
            return 0;
        }

        private static void AssertScript(Option<ScriptFileInfo> result, int line)
        {
            Assert.IsTrue(result.TryGetValue(out var script));
            Assert.IsTrue(Path.IsPathRooted(script.AbsolutePath));
            Assert.IsTrue(File.Exists(script.AbsolutePath));
            Assert.AreEqual("TypeFinderTests.cs", Path.GetFileName(script.AbsolutePath));
            Assert.AreEqual(line, script.LineNumber);
        }
    }

    public class TypeFinderPackageFixture
    {
        public class Nested<T> { }
    }
}

#endif
