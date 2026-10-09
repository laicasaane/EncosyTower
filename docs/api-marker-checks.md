# Editor and authoring API checks

`ApiForEditorAnalyzer` and `ApiForAuthoringAnalyzer` in the Core Roslyn assembly, plus
`ApiForEditorInstructionAnalyzer` and `ApiForAuthoringInstructionAnalyzer` in the Editor Project
Auditor module, check APIs marked with
`EncosyTower.Core.ApiForEditorAttribute` and `EncosyTower.Core.ApiForAuthoringAttribute`.
The attributes remain in player assemblies. The package depends on Project Auditor 3.1.1.

| Marker | Allowed compilation symbols | Roslyn error | Project Auditor error |
|---|---|---|---|
| `[ApiForEditor]` | `UNITY_EDITOR` | `SG_API_MARKER_0001` | `EAP0001` |
| `[ApiForAuthoring]` | `UNITY_EDITOR` or `ENCOSY_INCLUDE_AUTHORING` | `SG_API_MARKER_0002` | `EAP0002` |

A marker on a type covers all its members and nested types, including members without their own
attribute. Public members of marked types should still carry their individual markers. An editor
API use is exempt inside an editor marked calling member or containing type. An authoring API use
is exempt inside either an authoring or editor marked calling member or containing type.
An authoring marker never exempts an editor API use. Each Roslyn analyzer reports only its own
marker, so a dual-marked use can produce both IDs when both analyzers are enabled. Project Auditor
retains editor-error precedence for an instruction subject to both restrictions.

Roslyn reads the preprocessor symbols of each use's syntax tree. It checks bound invocations,
method groups, fields, properties, events, constructors, indexers, operators, attribute applications,
and type references, including `typeof`, inheritance and generic arguments. It analyzes generated
source too. `nameof` expressions and inactive `#if` code are excluded. Qualifying a marked member
with its marked type does not produce a duplicate report for the same restriction.

Messages identify the API and say either `requires UNITY_EDITOR` or
`requires UNITY_EDITOR or ENCOSY_INCLUDE_AUTHORING`.

## Run Project Auditor

Open **Window > Analysis > Project Auditor**, choose the player compilation mode and analyze Code.
Search the resulting issues for `EAP0001` or `EAP0002`. The module registers automatically through
Project Auditor's `CodeModuleInstructionAnalyzer` discovery; no settings panel is required.
`ApiForEditorInstructionAnalyzer` registers `EAP0001` for editor APIs;
`ApiForAuthoringInstructionAnalyzer` registers `EAP0002` for authoring APIs. They share Cecil
inspection, the opcode list and per-assembly define snapshot handling.
Its assembly and namespace are `EncosyTower.Editor.AuditorRules`, in
`Packages/com.laicasaane.encosy-tower/EncosyTower.Editor.AuditorRules/`.
`Player` (the default) and `DevelopmentPlayer` are checked. `Editor` and `EditorPlayMode` are exempt.

The module snapshots per-assembly player compilation defines during initialization, before Project
Auditor starts its analysis thread. This includes assembly version defines as well as scripting
defines. Create a fresh `Unity.ProjectAuditor.Editor.ProjectAuditor` instance after changing defines.
For precompiled assemblies with no matching Unity compilation assembly, the original preprocessor
symbols are unavailable; caller markers still provide the exemption.

The IL check covers `call`, `callvirt`, `newobj`, field loads/addresses/stores, method pointers,
`ldtoken`, `box`, `newarr`, `castclass` and `isinst`. It resolves member and containing-type markers,
property/event accessor associations, generic operands, and the source member of compiler generated
state machines and lambdas. It skips references that Cecil cannot resolve. IL does not retain all
source type references, attribute applications, constant accesses or initializer ownership, so the
Roslyn check supplies that source-level coverage. A clean audit is not a substitute for compilation.

## Validation

From the repository root, build only the standalone generator workspace:

~~~powershell
dotnet build Plugins/SourceGenerator/EncosyTower.SourceGen.slnx --configuration Debug --nologo
dotnet test Plugins/SourceGenerator/EncosyTower.SourceGen.Tests/EncosyTower.SourceGen.Tests.csproj --configuration Debug --no-build --filter "FullyQualifiedName~EncosyTower.SourceGen.Tests.Core.ApiMarkers" --nologo
~~~

`ApiForEditorAnalyzerTests` and `ApiForAuthoringAnalyzerTests` assert exact IDs, error severity,
locations and messages with Microsoft.CodeAnalysis.Testing. They cover player/editor/authoring
profiles, member/type exemptions, type uses, `nameof`, accessors, aliases, metadata references,
generated source and inferred `var` types. Each suite also verifies that its analyzer ignores APIs
marked only with the other attribute.

Refresh the shipped DLL through the existing Release copy target:

~~~powershell
dotnet build Plugins/SourceGenerator/EncosyTower.Core.Generators/EncosyTower.Core.Generators.csproj --configuration Release --nologo
~~~

The output goes to
`Packages/com.laicasaane.encosy-tower/EncosyTower.Core/SourceGenerators/EncosyTower.Core.Generators.dll`.
Import/recompile in Unity afterward; preserve existing DLL importer metadata. If the copy fails
because the DLL is locked, close the Unity Editor, retry the copy and reopen the project.

With `ENCOSY_TESTS_ENABLED`, run the EditMode `ApiForEditorInstructionAnalyzerTests` and
`ApiForAuthoringInstructionAnalyzerTests` suites in namespace `EncosyTower.Tests.Editor.AuditorRules`.
Their source is under
`Packages/com.laicasaane.encosy-tower/EncosyTower.Tests.EditorMode/EncosyTower.Editor.AuditorRules/`.
Their in-memory Cecil fixtures exercise marker resolution, exemptions, authoring defines and
editor compilation modes without a full audit. Each suite verifies that the analyzer ignores the
other marker and preserves editor-error precedence for dual-marked APIs.
For integration verification,
construct a fresh Project Auditor and call `Audit` with `AnalysisParams.Categories` containing
`IssueCategory.Code`, `AssemblyNames` containing the fixture assembly name, and
`CompilationMode.Player`. Confirm both IDs and error severities, then repeat with authoring enabled
and with editor compilation. A fixture that intentionally violates the Roslyn errors must be a
precompiled assembly or suppress those two diagnostics locally so its IL can reach the auditor.
Keep Unity compilation, EditMode execution and the batch audit as separate results.
