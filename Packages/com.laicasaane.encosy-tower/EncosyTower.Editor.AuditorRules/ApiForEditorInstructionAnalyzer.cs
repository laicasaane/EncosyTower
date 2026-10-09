#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.ProjectAuditor.Editor;
using Unity.ProjectAuditor.Editor.Core;

namespace EncosyTower.Editor.AuditorRules
{
    [ApiForEditor]
    public sealed class ApiForEditorInstructionAnalyzer : CodeModuleInstructionAnalyzer
    {
        internal const string MARKER = "EncosyTower.Core.ApiForEditorAttribute";

        private const string DIAGNOSTIC_ID = "EAP0001";

        private static readonly Descriptor s_descriptor = new(
              id: DIAGNOSTIC_ID
            , title: "Editor API used outside an editor build"
            , areas: Areas.Quality
            , description: "Use editor APIs only in editor builds or from an [ApiForEditor] member or containing type."
            , recommendation: "Guard the use with UNITY_EDITOR or mark its caller with [ApiForEditor]."
        ) {
            DefaultSeverity = Severity.Error,
            MessageFormat = "API '{0}' requires UNITY_EDITOR.",
        };

        private static readonly string[] s_exemptionMarkers = {
            MARKER,
        };

        [ApiForEditor]
        public override IReadOnlyCollection<OpCode> opCodes => ApiMarkerAnalyzerAPI.InstructionOpCodes;

        internal static bool TryFindViolation(MethodDefinition caller, MemberReference target, out MemberReference api)
            => ApiMarkerAnalyzerAPI.TryFindViolation(
                  caller: caller
                , target: target
                , marker: MARKER
                , exemptionMarkers: s_exemptionMarkers
                , api: out api
            );

        [ApiForEditor]
        public override void Initialize(Action<Descriptor> registerDescriptor)
        {
            registerDescriptor(s_descriptor);
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

            if (TryFindViolation(context.MethodDefinition, target, out var api))
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
            => TryFindViolation(caller, target, out api);
    }
}

#endif
