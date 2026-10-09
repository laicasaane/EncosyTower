#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.ProjectAuditor.Editor;
using Unity.ProjectAuditor.Editor.Core;
using UnityEditor.Compilation;

namespace EncosyTower.Editor.AuditorRules
{
    [ApiForEditor]
    public sealed class ApiForAuthoringInstructionAnalyzer : CodeModuleInstructionAnalyzer
    {
        internal const string MARKER = "EncosyTower.Core.ApiForAuthoringAttribute";

        private const string DIAGNOSTIC_ID = "EAP0002";
        private const string AUTHORING_SYMBOL = "ENCOSY_INCLUDE_AUTHORING";

        private static readonly Descriptor s_descriptor = new(
              id: DIAGNOSTIC_ID
            , title: "Authoring API used outside an authoring build"
            , areas: Areas.Quality
            , description: "Use authoring APIs only in allowed builds or from an authoring or editor marked API."
            , recommendation: "Guard the use with UNITY_EDITOR or ENCOSY_INCLUDE_AUTHORING, or mark its caller."
        ) {
            DefaultSeverity = Severity.Error,
            MessageFormat = "API '{0}' requires UNITY_EDITOR or ENCOSY_INCLUDE_AUTHORING.",
        };

        private static readonly string[] s_exemptionMarkers = {
            ApiForEditorInstructionAnalyzer.MARKER,
            MARKER,
        };

        private readonly HashSet<string> _authoringAssemblies = new(StringComparer.Ordinal);

        [ApiForEditor]
        public override IReadOnlyCollection<OpCode> opCodes => ApiMarkerAnalyzerAPI.InstructionOpCodes;

        [ApiForEditor]
        public override void Initialize(Action<Descriptor> registerDescriptor)
        {
            registerDescriptor(s_descriptor);
            _authoringAssemblies.Clear();

            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            var count = assemblies.Length;

            for (var i = 0; i < count; i++)
            {
                var assembly = assemblies[i];

                if (Array.IndexOf(assembly.defines, AUTHORING_SYMBOL) >= 0)
                {
                    _authoringAssemblies.Add(assembly.name);
                }
            }
        }

        [ApiForEditor]
        public override IEnumerable<ReportItemBuilder> Analyze(InstructionAnalysisContext context)
        {
            if (context.Params.CompilationMode is CompilationMode.Editor or CompilationMode.EditorPlayMode
                || context.Instruction.Operand is not MemberReference target
            )
            {
                yield break;
            }

            var caller = context.MethodDefinition;
            var authoringAllowed = _authoringAssemblies.Contains(caller.Module.Assembly.Name.Name);

            if (TryGetViolation(caller, target, authoringAllowed, out var api))
            {
                yield return ApiMarkerAnalyzerAPI.CreateIssue(context, s_descriptor, api);
            }
        }

        internal bool TryGetViolation(
              MethodDefinition caller
            , MemberReference target
            , bool authoringAllowed
            , out MemberReference api
        )
        {
            if (authoringAllowed || ApiForEditorInstructionAnalyzer.TryFindViolation(caller, target, out _))
            {
                api = default;
                return false;
            }

            return ApiMarkerAnalyzerAPI.TryFindViolation(
                  caller: caller
                , target: target
                , marker: MARKER
                , exemptionMarkers: s_exemptionMarkers
                , api: out api
            );
        }
    }
}

#endif
