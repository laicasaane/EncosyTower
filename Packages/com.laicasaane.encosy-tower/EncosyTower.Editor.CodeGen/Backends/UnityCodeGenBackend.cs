#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.CodeGen;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Discovers and runs code generators already loaded in the Unity Editor process.
    /// </summary>
    internal sealed class UnityCodeGenBackend
    {
        private const string INVALID_TYPE_CODE = "ENCOSY_CODEGEN_UNITY_0001";
        private const string INVALID_SOURCE_PATH_CODE = "ENCOSY_CODEGEN_UNITY_0002";
        private const string GENERATOR_FAILED_CODE = "ENCOSY_CODEGEN_UNITY_0003";

        /// <summary>
        /// Discovers loaded Unity generator types and returns their complete result batch.
        /// </summary>
        internal Task<GeneratedCodeBatch> GenerateAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var types = new List<Type>();
            types.AddRange(TypeCache.GetTypesWithAttribute<CodeGeneratorAttribute>());
            types.AddRange(TypeCache.GetTypesDerivedFrom<ICodeGenerator>());

            return Task.FromResult(Generate(types, token));
        }

        /// <summary>
        /// Validates, de-duplicates, and deterministically orders Unity generator candidates.
        /// </summary>
        internal static DiscoveryResult Discover(IEnumerable<Type> types)
        {
            var candidates = new List<Candidate>();
            var diagnostics = new List<DiagnosticEntry>();
            var visitedTypes = new HashSet<Type>();

            foreach (var type in types)
            {
                if (type is null || visitedTypes.Add(type) == false)
                {
                    continue;
                }

                CodeGeneratorAttribute attribute;

                try
                {
                    attribute = type.GetCustomAttribute<CodeGeneratorAttribute>(inherit: false);
                }
                catch (Exception exception)
                {
                    var typeName = GetTypeName(type);
                    diagnostics.Add(new(
                          typeName
                        , string.Empty
                        , CreateError(
                              INVALID_TYPE_CODE
                            , $"Cannot read {nameof(CodeGeneratorAttribute)} from '{typeName}': "
                                + exception.GetBaseException().Message
                            , string.Empty
                          )
                    ));
                    continue;
                }

                if (attribute is null)
                {
                    continue;
                }

                var name = GetTypeName(type);
                var valid = true;
                var sourcePath = string.Empty;
                var sourcePathValid = TryGetCanonicalSourcePath(attribute.FilePath, out sourcePath);

                if (sourcePathValid == false)
                {
                    valid = false;
                    diagnostics.Add(new(
                          name
                        , sourcePath
                        , CreateError(
                              INVALID_SOURCE_PATH_CODE
                            , $"Generator type '{name}' must declare an existing physical source "
                                + "file inside the current project Assets or Packages directory."
                            , sourcePath
                          )
                    ));
                }

                if (typeof(ICodeGenerator).IsAssignableFrom(type) == false)
                {
                    valid = false;
                    diagnostics.Add(new(
                          name
                        , sourcePath
                        , CreateError(
                              INVALID_TYPE_CODE
                            , $"Annotated type '{name}' must implement {typeof(ICodeGenerator).FullName}."
                            , sourcePath
                          )
                    ));
                }

                if (type.IsAbstract)
                {
                    valid = false;
                    diagnostics.Add(new(
                          name
                        , sourcePath
                        , CreateError(INVALID_TYPE_CODE, $"Annotated type '{name}' must be non-abstract.", sourcePath)
                    ));
                }

                if (type.ContainsGenericParameters)
                {
                    valid = false;
                    diagnostics.Add(new(
                          name
                        , sourcePath
                        , CreateError(INVALID_TYPE_CODE, $"Annotated type '{name}' must be closed.", sourcePath)
                    ));
                }

                if (CanConstruct(type) == false)
                {
                    valid = false;
                    diagnostics.Add(new(
                          name
                        , sourcePath
                        , CreateError(
                              INVALID_TYPE_CODE
                            , $"Annotated type '{name}' must have a parameterless " + "instance constructor."
                            , sourcePath
                          )
                    ));
                }

                if (valid)
                {
                    candidates.Add(new(type, sourcePath));
                }
            }

            candidates.Sort(static (left, right) => {
                var pathOrder = StringComparer.Ordinal.Compare(left.SourcePath, right.SourcePath);
                return pathOrder != 0
                    ? pathOrder
                    : StringComparer.Ordinal.Compare(GetTypeName(left.Type), GetTypeName(right.Type));
            });

            diagnostics.Sort(static (left, right) =>
            {
                var pathOrder = StringComparer.Ordinal.Compare(left.SourcePath, right.SourcePath);

                if (pathOrder != 0)
                {
                    return pathOrder;
                }

                var typeOrder = StringComparer.Ordinal.Compare(left.TypeName, right.TypeName);
                return typeOrder != 0
                    ? typeOrder
                    : StringComparer.Ordinal.Compare(left.Diagnostic.Message, right.Diagnostic.Message);
            });

            var resultDiagnostics = new CodeGenDiagnostic[diagnostics.Count];

            for (var i = 0; i < diagnostics.Count; i++)
            {
                resultDiagnostics[i] = diagnostics[i].Diagnostic;
            }

            return new(candidates.ToArray(), resultDiagnostics);
        }

        /// <summary>
        /// Runs the valid Unity generator candidates and collects per-generator failures.
        /// </summary>
        internal static GeneratedCodeBatch Generate(IEnumerable<Type> types, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var discovery = Discover(types);
            var generatedCodes = new List<GeneratedCode>();
            var diagnostics = new List<CodeGenDiagnostic>(discovery.Diagnostics);

            for (var i = 0; i < discovery.Candidates.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                var candidate = discovery.Candidates[i];

                try
                {
                    var instance = Activator.CreateInstance(candidate.Type, nonPublic: true);

                    if (instance is not ICodeGenerator generator)
                    {
                        diagnostics.Add(CreateError(
                              GENERATOR_FAILED_CODE
                            , $"Cannot construct Unity generator '{GetTypeName(candidate.Type)}'."
                            , candidate.SourcePath
                        ));
                        continue;
                    }

                    var result = generator.Generate();

                    if (result is null)
                    {
                        diagnostics.Add(CreateError(
                              GENERATOR_FAILED_CODE
                            , $"Unity generator '{GetTypeName(candidate.Type)}' " + "returned a null result."
                            , candidate.SourcePath
                        ));
                        continue;
                    }

                    generatedCodes.AddRange(result);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    diagnostics.Add(CreateError(
                          GENERATOR_FAILED_CODE
                        , $"Unity generator '{GetTypeName(candidate.Type)}' failed: "
                            + exception.GetBaseException().Message
                        , candidate.SourcePath
                    ));
                }
            }

            return new(generatedCodes.ToArray(), diagnostics.ToArray(), AllCandidatesSkipped: false);
        }

        private static bool CanConstruct(Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }

            return type.IsClass
                && type.GetConstructor(
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    , binder: null
                    , Type.EmptyTypes
                    , modifiers: null
                ) is not null
                ;
        }

        private static CodeGenDiagnostic CreateError(string code, string message, string filePath)
            => new(CodeGenDiagnosticSeverity.Error, code, message, filePath, Line: 0, Column: 0);

        private static string GetTypeName(Type type)
            => type.FullName ?? type.Name;

        private static bool HasReparsePoint(string filePath, string projectRoot)
        {
            if ((File.GetAttributes(filePath) & FileAttributes.ReparsePoint) != 0)
            {
                return true;
            }

            var current = Directory.GetParent(filePath);

            while (current is not null)
            {
                if ((current.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    return true;
                }

                if (PathEquals(current.FullName, projectRoot))
                {
                    return false;
                }

                current = current.Parent;
            }

            return true;
        }

        private static bool IsInsideRoot(string filePath, string rootPath)
        {
            var rootWithSeparator = rootPath.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? rootPath
                : rootPath + Path.DirectorySeparatorChar;

            return filePath.StartsWith(rootWithSeparator, GetPathComparison());
        }

        private static bool PathEquals(string left, string right)
            => string.Equals(left, right, GetPathComparison());

        private static StringComparison GetPathComparison()
            => Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        private static bool TryGetCanonicalSourcePath(string filePath, out string canonicalPath)
        {
            canonicalPath = string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                var assetsRoot = Path.GetFullPath(Path.Combine(projectRoot, "Assets"));
                var packagesRoot = Path.GetFullPath(Path.Combine(projectRoot, "Packages"));
                var fullPath = Path.GetFullPath(filePath);
                canonicalPath = fullPath;

                if (File.Exists(fullPath) == false)
                {
                    return false;
                }

                if (IsInsideRoot(fullPath, assetsRoot) == false
                    && IsInsideRoot(fullPath, packagesRoot) == false
                )
                {
                    return false;
                }

                if (HasReparsePoint(fullPath, projectRoot))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Identifies a validated Unity generator type and its canonical declaration path.
        /// </summary>
        /// <param name="Type">The concrete generator type.</param>
        /// <param name="SourcePath">The canonical path to its declaration file.</param>
        internal readonly record struct Candidate(Type Type, string SourcePath);

        /// <summary>
        /// Contains the ordered candidates and diagnostics produced by Unity discovery.
        /// </summary>
        /// <param name="Candidates">The candidates that can be instantiated.</param>
        /// <param name="Diagnostics">The diagnostics for rejected declarations.</param>
        internal readonly record struct DiscoveryResult(
              Candidate[] Candidates
            , CodeGenDiagnostic[] Diagnostics
        );

        /// <summary>
        /// Associates a discovery diagnostic with the type and path used for deterministic sorting.
        /// </summary>
        /// <param name="TypeName">The generator type name.</param>
        /// <param name="SourcePath">The generator declaration path.</param>
        /// <param name="Diagnostic">The diagnostic to report.</param>
        private readonly record struct DiagnosticEntry(
              string TypeName
            , string SourcePath
            , CodeGenDiagnostic Diagnostic
        );
    }
}

#endif
