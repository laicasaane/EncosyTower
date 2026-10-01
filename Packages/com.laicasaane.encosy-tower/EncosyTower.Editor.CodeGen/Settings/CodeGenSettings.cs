#if UNITY_EDITOR

using System;
using EncosyTower.Settings;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.CodeGen
{
    /// <summary>
    /// Stores per-user Editor preferences for automatic code generation.
    /// </summary>
    [Settings(SettingsUsage.EditorUser, "Encosy Tower/CodeGen")]
    internal sealed class CodeGenSettings : Settings<CodeGenSettings>
    {
        [SerializeField, InspectorName("Automatically Generate Code")]
        internal bool _automaticallyGenerateCode;

        [SerializeField, InspectorName("Run Generators in .NET Solution")]
        internal bool _runGeneratorsInDotnetSolution;

        [SerializeField, InspectorName("Retain .NET Solutions")]
        internal bool _retainDotnetSolutions;

        /// <summary>
        /// Gets the project-relative path of the per-user settings asset.
        /// </summary>
        internal static string AssetPath
            => $"{SettingsAPI.GetSettingsPath(SettingsUsage.EditorUser)}CodeGenSettings.asset";

        /// <summary>
        /// Reads an immutable snapshot of the current settings,
        /// falling back to defaults on failure.
        /// </summary>
        internal static Snapshot ReadCurrent()
            => ReadCurrent(AssetDatabase.LoadAssetAtPath<CodeGenSettings>);

        /// <summary>
        /// Reads the current settings through a supplied loader for deterministic testing.
        /// </summary>
        internal static Snapshot ReadCurrent(Func<string, CodeGenSettings> loader)
        {
            try
            {
                var settings = loader(AssetPath);
                return settings == null
                    ? default
                    : new Snapshot(
                          settings._automaticallyGenerateCode
                        , settings._runGeneratorsInDotnetSolution
                        , settings._retainDotnetSolutions
                    );
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Captures the automatic-run and backend preferences used by the coordinator.
        /// </summary>
        /// <param name="AutomaticallyGenerateCode">
        /// Whether generation is queued after eligible Editor events.
        /// </param>
        /// <param name="RunGeneratorsInDotnetSolution">
        /// Whether automatic runs use the isolated .NET backend.
        /// </param>
        internal readonly record struct Snapshot(
              bool AutomaticallyGenerateCode
            , bool RunGeneratorsInDotnetSolution
            , bool RetainDotnetSolutions = false
        );
    }
}

#endif
