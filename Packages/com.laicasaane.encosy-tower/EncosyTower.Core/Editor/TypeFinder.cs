#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EncosyTower.Common;
using EncosyTower.Core;
using EncosyTower.IO;
using UnityEditor;
using UnityEditor.Compilation;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace EncosyTower.Editor
{
    /// <summary>A physical C# source file and the one-based line containing its type declaration.</summary>
    [ApiForEditor]
    public readonly struct ScriptFileInfo
    {
        [ApiForEditor]
        public ScriptFileInfo(string absolutePath, int lineNumber)
        {
            AbsolutePath = absolutePath;
            LineNumber = lineNumber;
        }

        [ApiForEditor]
        public string AbsolutePath { get; }

        [ApiForEditor]
        public int LineNumber { get; }
    }

    /// <summary>
    /// Finds declarations in C# files known to Unity without opening an editor or displaying a dialog.
    /// </summary>
    /// <remarks>
    /// Call on the editor main thread. Names are case-sensitive. Nested names use '+' and generic names use
    /// metadata arity, for example Outer`1+Inner`2. Omitted namespace and assembly arguments match any value;
    /// an empty namespace matches the global namespace. Assembly names may include a .dll suffix or full identity.
    /// Ambiguous queries matching distinct type identities return None. For partial declarations of the same type,
    /// exact filenames take precedence, then paths and physical line numbers in ordinal order. Generated or
    /// precompiled types without a source file return None. Generic names must include their metadata arity.
    /// </remarks>
    [ApiForEditor]
    public static class TypeFinder
    {
        private static readonly Dictionary<string, ParsedFile> s_files = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, SourceFile> s_sources = new(StringComparer.Ordinal);
        private static readonly Dictionary<string, RootPath> s_packageRoots = new(StringComparer.Ordinal);
        private static readonly Dictionary<Query, Option<ScriptFileInfo>> s_results = new();

        private static bool s_hasSources;

        static TypeFinder()
        {
            EditorApplication.projectChanged -= ClearCache;
            EditorApplication.projectChanged += ClearCache;

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        [ApiForEditor]
        public static Option<ScriptFileInfo> Find(string typeName)
            => Find(typeName, null, null);

        [ApiForEditor]
        public static Option<ScriptFileInfo> Find(string typeName, string typeNamespace)
            => Find(typeName, typeNamespace, null);

        [ApiForEditor]
        public static Option<ScriptFileInfo> Find(string typeName, string typeNamespace, string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return Option.None;
            }

            var query = new Query(typeName, typeNamespace, assemblyName);

            if (s_results.TryGetValue(query, out var cached))
            {
                if (cached.TryGetValue(out var script) == false)
                {
                    return cached;
                }

                var info = new FileInfo(script.AbsolutePath);

                if (info.Exists && s_files.TryGetValue(script.AbsolutePath, out var parsed)
                    && info.LastWriteTimeUtc == parsed.LastWriteTime && info.Length == parsed.Length)
                {
                    return cached;
                }
            }

            var result = FindUncached(query);
            s_results[query] = result;
            return result;
        }

        private static Option<ScriptFileInfo> FindUncached(Query query)
        {
            EnsureSources();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var candidates = new List<SourceFile>();
            var guids = AssetDatabase.FindAssets($"{query.FileName} t:MonoScript");

            // The asset name is only a hint. Every result must contain the requested declaration.
            for (var i = 0; i < guids.Length; i++)
            {
                AddCandidate(AssetDatabase.GUIDToAssetPath(guids[i]), query, candidates, visited);
            }

            var result = query.IsQualified ? FindInCandidates(candidates, query) : Option<ScriptFileInfo>.None;

            if (result.HasValue)
            {
                return result;
            }

            // Compilation ownership avoids scanning other assemblies when the complete identity is known.
            foreach (var source in s_sources.Values)
            {
                if (query.MatchesAssembly(source.AssemblyName) && visited.Add(source.AbsolutePath))
                {
                    candidates.Add(source);
                }
            }

            result = query.IsQualified ? FindInCandidates(candidates, query) : Option<ScriptFileInfo>.None;

            if (result.HasValue)
            {
                return result;
            }

            guids = AssetDatabase.FindAssets("t:MonoScript");

            // Includes package assets and scripts outside the active compilation snapshot.
            for (var i = 0; i < guids.Length; i++)
            {
                AddCandidate(AssetDatabase.GUIDToAssetPath(guids[i]), query, candidates, visited);
            }

            return FindInCandidates(candidates, query);
        }

        private static void OnCompilationFinished(object context)
            => ClearCache();

        private static void ClearCache()
        {
            s_files.Clear();
            s_sources.Clear();
            s_packageRoots.Clear();
            s_results.Clear();
            s_hasSources = false;
        }

        private static void EnsureSources()
        {
            if (s_hasSources)
            {
                return;
            }

            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);

            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                var paths = assembly.sourceFiles;

                for (var j = 0; j < paths.Length; j++)
                {
                    var path = GetAbsolutePath(paths[j]);

                    if (string.IsNullOrEmpty(path) == false)
                    {
                        s_sources[path] = new SourceFile(path, assembly.name, assembly.defines);
                    }
                }
            }

            s_hasSources = true;
        }

        private static void AddCandidate(
              string assetPath
            , Query query
            , List<SourceFile> candidates
            , HashSet<string> visited
        )
        {
            var path = GetAbsolutePath(assetPath);

            if (string.IsNullOrEmpty(path) || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == false)
            {
                return;
            }

            if (s_sources.TryGetValue(path, out var source) == false)
            {
                var assembly = CompilationPipeline.GetAssemblyNameFromScriptPath(assetPath);
                source = new SourceFile(path, NormalizeAssembly(assembly), Array.Empty<string>());
            }

            if (query.MatchesAssembly(source.AssemblyName) && visited.Add(path))
            {
                candidates.Add(source);
            }
        }

        private static Option<ScriptFileInfo> FindInCandidates(List<SourceFile> candidates, Query query)
        {
            candidates.Sort((left, right) => {
                var leftExact = Path.GetFileNameWithoutExtension(left.AbsolutePath) == query.FileName;
                var rightExact = Path.GetFileNameWithoutExtension(right.AbsolutePath) == query.FileName;
                var order = rightExact.CompareTo(leftExact);
                return order != 0 ? order : string.CompareOrdinal(left.AbsolutePath, right.AbsolutePath);
            });

            var identity = string.Empty;
            Option<ScriptFileInfo> result = Option.None;

            for (var i = 0; i < candidates.Count; i++)
            {
                var source = candidates[i];
                var declarations = ReadDeclarations(source);

                for (var j = 0; j < declarations.Length; j++)
                {
                    var declaration = declarations[j];

                    if (query.Matches(declaration))
                    {
                        var nextIdentity = source.AssemblyName + "\0" + declaration.Namespace + "\0" + declaration.Name;

                        if (result.HasValue && identity != nextIdentity)
                        {
                            return Option.None;
                        }

                        if (result.HasValue == false)
                        {
                            identity = nextIdentity;
                            result = new ScriptFileInfo(source.AbsolutePath, declaration.LineNumber);
                        }

                        if (query.IsQualified)
                        {
                            return result;
                        }
                    }
                }
            }

            return result;
        }

        private static Declaration[] ReadDeclarations(SourceFile source)
        {
            try
            {
                var info = new FileInfo(source.AbsolutePath);

                if (info.Exists == false)
                {
                    s_files.Remove(source.AbsolutePath);
                    return Array.Empty<Declaration>();
                }

                if (s_files.TryGetValue(source.AbsolutePath, out var cached)
                    && cached.LastWriteTime == info.LastWriteTimeUtc && cached.Length == info.Length)
                {
                    return cached.Declarations;
                }

                var text = File.ReadAllText(source.AbsolutePath);
                var declarations = ParseDeclarations(text, source.Defines);
                s_files[source.AbsolutePath] = new ParsedFile(info.LastWriteTimeUtc, info.Length, declarations);
                return declarations;
            }
            catch (IOException)
            {
                return Array.Empty<Declaration>();
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<Declaration>();
            }
        }

        private static string GetAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            path = path.Replace('\\', '/');

            if (path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                var separator = path.IndexOf('/', "Packages/".Length);

                if (separator >= 0)
                {
                    var packagePath = path[..separator];

                    if (s_packageRoots.TryGetValue(packagePath, out var packageRoot) == false)
                    {
                        var package = PackageInfo.FindForAssetPath(path);

                        if (package != null && string.IsNullOrEmpty(package.resolvedPath) == false)
                        {
                            packageRoot = package.resolvedPath;
                            s_packageRoots[packagePath] = packageRoot;
                        }
                    }

                    if (packageRoot.IsValid)
                    {
                        return packageRoot.GetFileAbsolutePath(path[(separator + 1)..]);
                    }
                }
            }

            RootPath projectRoot = EditorAPI.ProjectPath;
            return projectRoot.GetFileAbsolutePath(path);
        }

        private static string NormalizeAssembly(string name)
        {
            if (name == null)
            {
                return null;
            }

            var comma = name.IndexOf(',');
            name = (comma < 0 ? name : name[..comma]).Trim();
            return name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                ? name[..^4]
                : name;
        }

        private static Declaration[] ParseDeclarations(string text, string[] defines)
        {
            var tokens = Tokenize(text, defines);
            var declarations = new List<Declaration>();
            var index = 0;
            ParseScope(tokens, ref index, string.Empty, string.Empty, declarations);
            return declarations.ToArray();
        }

        private static void ParseScope(
              List<Token> tokens
            , ref int index
            , string typeNamespace
            , string parentName
            , List<Declaration> declarations
        )
        {
            while (index < tokens.Count)
            {
                var token = tokens[index++];

                if (token.Text == "}")
                {
                    return;
                }

                if (token.Text == "namespace" && token.IsEscaped == false && parentName.Length == 0)
                {
                    var builder = new StringBuilder();

                    while (index < tokens.Count && tokens[index].Text != "{" && tokens[index].Text != ";")
                    {
                        builder.Append(tokens[index++].Text);
                    }

                    var nextNamespace = JoinName(typeNamespace, builder.ToString(), ".");

                    if (index < tokens.Count && tokens[index++].Text == "{")
                    {
                        ParseScope(tokens, ref index, nextNamespace, string.Empty, declarations);
                    }
                    else
                    {
                        typeNamespace = nextNamespace;
                    }

                    continue;
                }

                if (token.IsEscaped == false
                    && token.Text is "class" or "struct" or "interface" or "enum" or "record" or "delegate")
                {
                    if (token.Text == "record" && index < tokens.Count && tokens[index].Text is "class" or "struct")
                    {
                        index++;
                    }

                    if (token.Text == "delegate")
                    {
                        if (index < tokens.Count && tokens[index].Text == "*")
                        {
                            continue;
                        }

                        // The last identifier before the parameter list (or its type parameters) is the name.
                        var nameIndex = -1;
                        var depth = 0;

                        while (index < tokens.Count)
                        {
                            var value = tokens[index].Text;

                            if (value == "(" && depth == 0)
                            {
                                if (nameIndex >= 0 && tokens[nameIndex].Text is not ("ref" or "readonly"))
                                {
                                    break;
                                }

                                // A tuple return type precedes the delegate's name and parameter list.
                                index++;
                                SkipBlock(tokens, ref index, "(", ")");
                                nameIndex = -1;
                                continue;
                            }

                            if (value == "<")
                            {
                                depth++;
                            }
                            else if (value == ">")
                            {
                                depth--;
                            }
                            else if (depth == 0 && tokens[index].IsIdentifier)
                            {
                                nameIndex = index;
                            }

                            if (value is ";" or "{" or "}")
                            {
                                break;
                            }

                            index++;
                        }

                        if (nameIndex < 0)
                        {
                            continue;
                        }

                        index = nameIndex;
                    }

                    if (index >= tokens.Count || tokens[index].IsIdentifier == false)
                    {
                        continue;
                    }

                    var nameToken = tokens[index++];
                    var name = nameToken.Text;

                    if (index < tokens.Count && tokens[index].Text == "<")
                    {
                        var arity = 1;
                        var depth = 1;
                        index++;

                        while (index < tokens.Count && depth > 0)
                        {
                            var value = tokens[index++].Text;
                            depth += value == "<" ? 1 : value == ">" ? -1 : 0;

                            if (value == "," && depth == 1)
                            {
                                arity++;
                            }
                        }

                        name += "`" + arity;
                    }

                    name = JoinName(parentName, name, "+");
                    declarations.Add(new Declaration(typeNamespace, name, nameToken.LineNumber));
                    SkipHeader(tokens, ref index);

                    if (index < tokens.Count && tokens[index].Text == "{")
                    {
                        index++;

                        if (token.Text == "enum")
                        {
                            SkipBlock(tokens, ref index, "{", "}");
                        }
                        else
                        {
                            ParseScope(tokens, ref index, typeNamespace, name, declarations);
                        }
                    }
                    else if (index < tokens.Count && tokens[index].Text == ";")
                    {
                        index++;
                    }

                    continue;
                }

                // Attribute arguments and executable/member bodies cannot contain type declarations.
                if (token.Text is "[" or "(" or "{")
                {
                    SkipBlock(tokens, ref index, token.Text, token.Text == "[" ? "]" : token.Text == "(" ? ")" : "}");
                }
            }
        }

        private static string JoinName(string prefix, string name, string separator)
            => prefix.Length == 0 ? name : prefix + separator + name;

        private static void SkipHeader(List<Token> tokens, ref int index)
        {
            while (index < tokens.Count)
            {
                var value = tokens[index].Text;

                if (value is "{" or ";" or "}")
                {
                    return;
                }

                index++;

                if (value is "(" or "[")
                {
                    SkipBlock(tokens, ref index, value, value == "(" ? ")" : "]");
                }
            }
        }

        private static void SkipBlock(List<Token> tokens, ref int index, string open, string close)
        {
            var depth = 1;

            while (index < tokens.Count && depth > 0)
            {
                var value = tokens[index++].Text;
                depth += value == open ? 1 : value == close ? -1 : 0;
            }
        }

        private static List<Token> Tokenize(string text, string[] defines)
        {
            var tokens = new List<Token>();
            var symbols = new HashSet<string>(defines ?? Array.Empty<string>(), StringComparer.Ordinal);
            var branches = new Stack<Branch>();
            var active = true;
            var line = 1;

            for (var i = 0; i < text.Length;)
            {
                var ch = text[i];

                if (ch == '\n')
                {
                    line++;
                    i++;
                    continue;
                }

                if (char.IsWhiteSpace(ch))
                {
                    i++;
                    continue;
                }

                if (ch == '#')
                {
                    var start = ++i;

                    while (i < text.Length && text[i] != '\n')
                    {
                        i++;
                    }

                    ProcessDirective(text[start..i], symbols, branches, ref active);
                    continue;
                }

                if (active == false)
                {
                    i++;
                    continue;
                }

                if (ch == '/' && i + 1 < text.Length && text[i + 1] == '/')
                {
                    while (i < text.Length && text[i] != '\n')
                    {
                        i++;
                    }

                    continue;
                }

                if (ch == '/' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    i += 2;

                    while (i < text.Length)
                    {
                        if (text[i] == '*' && i + 1 < text.Length && text[i + 1] == '/')
                        {
                            i += 2;
                            break;
                        }

                        line += text[i++] == '\n' ? 1 : 0;
                    }

                    continue;
                }

                if (ch is '"' or '\'' || (ch is '@' or '$' && IsStringPrefix(text, i)))
                {
                    SkipString(text, ref i, ref line);
                    continue;
                }

                var startIndex = i;

                if (ch == '@' || TryIdentifierCharacter(text, i, out _, out _))
                {
                    var builder = new StringBuilder();
                    var escaped = ch == '@';

                    if (ch == '@')
                    {
                        i++;
                    }

                    while (TryIdentifierCharacter(text, i, out var letter, out var length))
                    {
                        escaped |= text[i] == '\\';

                        if (char.GetUnicodeCategory(letter) != UnicodeCategory.Format)
                        {
                            builder.Append(letter);
                        }

                        i += length;
                    }

                    // Malformed escaped identifiers must not prevent the scanner from advancing.
                    i = Math.Max(i, startIndex + 1);
                    tokens.Add(new Token(builder.ToString(), line, true, escaped));
                }
                else
                {
                    tokens.Add(new Token(text[i++].ToString(), line, false, false));
                }
            }

            return tokens;
        }

        private static bool TryIdentifierCharacter(string text, int index, out char letter, out int length)
        {
            letter = default;
            length = 1;

            if (index >= text.Length)
            {
                return false;
            }

            letter = text[index];

            if (letter == '\\' && index + 1 < text.Length && text[index + 1] is 'u' or 'U')
            {
                var digits = text[index + 1] == 'u' ? 4 : 8;

                if (index + digits + 2 > text.Length
                    || uint.TryParse(text.Substring(index + 2, digits), NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture, out var value) == false || value > char.MaxValue)
                {
                    return false;
                }

                letter = (char)value;
                length = digits + 2;
            }

            return char.GetUnicodeCategory(letter) is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
                or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter
                or UnicodeCategory.LetterNumber or UnicodeCategory.DecimalDigitNumber
                or UnicodeCategory.ConnectorPunctuation
                or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.Format;
        }

        private static bool IsStringPrefix(string text, int index)
        {
            while (index < text.Length && text[index] is '@' or '$')
            {
                index++;
            }

            return index < text.Length && text[index] == '"';
        }

        private static void SkipString(string text, ref int index, ref int line)
        {
            var verbatim = false;
            var interpolated = false;

            while (index < text.Length && text[index] is '@' or '$')
            {
                verbatim |= text[index] == '@';
                interpolated |= text[index++] == '$';
            }

            var quote = text[index++];
            var quotes = 1;

            while (verbatim == false && quote == '"' && index < text.Length && text[index] == '"')
            {
                quotes++;
                index++;
            }

            if (quotes == 2)
            {
                return;
            }

            while (index < text.Length)
            {
                var ch = text[index++];
                line += ch == '\n' ? 1 : 0;

                if (ch == quote)
                {
                    if (quotes >= 3)
                    {
                        var endQuotes = 1;

                        while (index < text.Length && text[index] == quote)
                        {
                            endQuotes++;
                            index++;
                        }

                        if (endQuotes >= quotes)
                        {
                            return;
                        }
                    }
                    else if (verbatim && index < text.Length && text[index] == quote)
                    {
                        index++;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (ch == '\\' && verbatim == false && quotes < 3 && index < text.Length)
                {
                    line += text[index++] == '\n' ? 1 : 0;
                }
                else if (ch == '{' && interpolated && quotes < 3)
                {
                    if (index < text.Length && text[index] == '{')
                    {
                        index++;
                    }
                    else
                    {
                        SkipInterpolation(text, ref index, ref line);
                    }
                }
            }
        }

        private static void SkipInterpolation(string text, ref int index, ref int line)
        {
            var depth = 1;

            while (index < text.Length && depth > 0)
            {
                var ch = text[index];

                if (ch is '"' or '\'' || (ch is '@' or '$' && IsStringPrefix(text, index)))
                {
                    SkipString(text, ref index, ref line);
                    continue;
                }

                if (ch == '/' && index + 1 < text.Length && text[index + 1] == '/')
                {
                    while (index < text.Length && text[index] != '\n')
                    {
                        index++;
                    }

                    continue;
                }

                if (ch == '/' && index + 1 < text.Length && text[index + 1] == '*')
                {
                    index += 2;

                    while (index < text.Length)
                    {
                        if (text[index] == '*' && index + 1 < text.Length && text[index + 1] == '/')
                        {
                            index += 2;
                            break;
                        }

                        line += text[index++] == '\n' ? 1 : 0;
                    }

                    continue;
                }

                line += ch == '\n' ? 1 : 0;
                depth += ch == '{' ? 1 : ch == '}' ? -1 : 0;
                index++;
            }
        }

        private static void ProcessDirective(
              string text
            , HashSet<string> symbols
            , Stack<Branch> branches
            , ref bool active
        )
        {
            text = text.Trim();
            var split = 0;

            while (split < text.Length && char.IsLetter(text[split]))
            {
                split++;
            }

            var directive = text[..split];
            var expression = text[split..].Trim();

            if (directive == "if")
            {
                var condition = new Condition(expression, symbols).Evaluate();
                branches.Push(new Branch(active, condition));
                active &= condition;
            }
            else if (directive is "elif" or "else" && branches.Count > 0)
            {
                var branch = branches.Pop();
                var condition = directive == "else" || new Condition(expression, symbols).Evaluate();
                active = branch.ParentActive && branch.Taken == false && condition;
                branches.Push(new Branch(branch.ParentActive, branch.Taken || condition));
            }
            else if (directive == "endif" && branches.Count > 0)
            {
                active = branches.Pop().ParentActive;
            }
            else if (active && directive == "define")
            {
                var words = expression.Split(new[] { ' ', '\t', '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 0)
                {
                    symbols.Add(words[0]);
                }
            }
            else if (active && directive == "undef")
            {
                var words = expression.Split(new[] { ' ', '\t', '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (words.Length > 0)
                {
                    symbols.Remove(words[0]);
                }
            }
        }

        private readonly record struct Query
        {
            public Query(string name, string typeNamespace, string assemblyName)
            {
                Name = name.Trim();
                Namespace = typeNamespace;
                AssemblyName = NormalizeAssembly(assemblyName);
                var nested = Name.LastIndexOf('+');
                var leaf = Name[(nested + 1)..];
                var arity = leaf.IndexOf('`');
                FileName = arity < 0 ? leaf : leaf[..arity];
            }

            public string Name { get; }
            public string Namespace { get; }
            public string AssemblyName { get; }
            public string FileName { get; }

            public bool IsQualified => Namespace != null && AssemblyName != null;

            public bool MatchesAssembly(string name)
                => AssemblyName == null || AssemblyName == name;

            public bool Matches(Declaration declaration)
            {
                if (Namespace != null && Namespace != declaration.Namespace)
                {
                    return false;
                }

                var candidate = declaration.Name;

                if (IsQualified == false && Name.IndexOf('+') < 0)
                {
                    candidate = candidate[(candidate.LastIndexOf('+') + 1)..];
                }

                return Name == candidate;
            }
        }

        private readonly record struct Token(string Text, int LineNumber, bool IsIdentifier, bool IsEscaped);

        private readonly record struct Declaration(string Namespace, string Name, int LineNumber);

        private readonly record struct SourceFile(string AbsolutePath, string AssemblyName, string[] Defines);

        private readonly record struct ParsedFile(DateTime LastWriteTime, long Length, Declaration[] Declarations);

        private readonly record struct Branch(bool ParentActive, bool Taken);

        private sealed class Condition
        {
            private readonly string _text;
            private readonly HashSet<string> _symbols;

            private int _index;

            public Condition(string text, HashSet<string> symbols)
            {
                _text = text;
                _symbols = symbols;
            }

            public bool Evaluate()
                => ParseOr();

            private bool ParseOr()
            {
                var value = ParseAnd();

                while (Consume("||"))
                {
                    value |= ParseAnd();
                }

                return value;
            }

            private bool ParseAnd()
            {
                var value = ParseEquality();

                while (Consume("&&"))
                {
                    value &= ParseEquality();
                }

                return value;
            }

            private bool ParseEquality()
            {
                var value = ParseUnary();

                while (true)
                {
                    if (Consume("=="))
                    {
                        value = value == ParseUnary();
                    }
                    else if (Consume("!="))
                    {
                        value = value != ParseUnary();
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            private bool ParseUnary()
            {
                if (Consume("!"))
                {
                    return ParseUnary() == false;
                }

                if (Consume("("))
                {
                    var value = ParseOr();
                    Consume(")");
                    return value;
                }

                var start = _index;

                while (_index < _text.Length && (_text[_index] == '_' || char.IsLetterOrDigit(_text[_index])))
                {
                    _index++;
                }

                var symbol = _text[start.._index ];
                return symbol == "true" || (symbol != "false" && _symbols.Contains(symbol));
            }

            private bool Consume(string token)
            {
                while (_index < _text.Length && char.IsWhiteSpace(_text[_index]))
                {
                    _index++;
                }

                if (_index + token.Length <= _text.Length
                    && string.CompareOrdinal(_text, _index, token, 0, token.Length) == 0)
                {
                    _index += token.Length;
                    return true;
                }

                return false;
            }
        }
    }
}

#endif
