# EncosyTower Commit Conventions

This document is the single authority for commit boundaries and commit messages
in this repository. It applies to new commits; do not rewrite existing history
only to make old commits conform.

## 1. Commit Boundaries

- Each commit must have one coherent, reviewable purpose and one topic.
- Split unrelated features, tooling, samples, release artifacts, and version
  metadata into separate commits.
- Keep tests, documentation, and generated snapshots with the change they
  directly validate or explain.
- Split runtime and Source Generator changes when each part is independently
  valid and reviewable. When they must change together to preserve one contract,
  use the topic of the primary behavior.
- Keep agent-only guidance separate from runtime code, packages, assets, project
  settings, and build configuration.
- Preserve unrelated tracked and untracked work. Stage explicit paths and inspect
  the staged diff before committing.

## 2. Subject Format

Use this format:

```text
<Topic>: <brief imperative summary>
```

The release-version commit is the only standard exception:

```text
Version <version>
```

Subject rules:

- Maximum length is 70 characters, including the topic and colon.
- Use the most specific stable topic that owns the change.
- Start the summary with a lowercase imperative verb, such as `add`, `fix`,
  `remove`, `rename`, `refactor`, `update`, or `rebuild`.
- Describe the outcome, not the files touched or the work performed.
- Do not end the subject with a period.
- Do not combine topics in the subject.
- `WIP` is acceptable only as a temporary local checkpoint. Reword or squash it
  before the commit becomes part of shared or release history.

Examples:

```text
Core: add custom array allocator support
Entities.Stats: fix invalid component lookup
SourceGen.Mvvm: preserve generated binder contracts
Samples.Persistence: demonstrate save migration
Project: add commit conventions
Agents: link commit conventions
Version 0.1.8-preview.1
```

## 3. Topic Selection

Choose topics by ownership and behavior, not only by directory.

| Topic | Use |
|---|---|
| `Project` | Repository-wide configuration, CI, release tooling, build setup, and general documentation |
| `Agents` | `AGENTS.md`, `.agents/`, and agent-only instruction files; `.github/workflows/` remains `Project` |
| `<Feature>` | Runtime or Unity package behavior owned by one feature |
| `SourceGen` | Shared Source Generator infrastructure, build plumbing, or coordinated shipped-artifact rebuilds |
| `SourceGen.<Feature>` | A feature's Roslyn generator, analyzer, code refactor, or directly supporting tests |
| `Tests` | Cross-feature Unity test infrastructure or test-only work without a narrower owner |
| `SourceGen.Tests` | Cross-feature Source Generator test infrastructure or test-only work without a narrower owner |
| `Samples` | Cross-feature or release-wide sample changes |
| `Samples.<Feature>` | Sample-only changes owned by one feature |
| `Version` | Release version and its directly associated release metadata |

Current feature topics follow the stable module name without the
`EncosyTower.` prefix. Examples include:

- `Bcl.Extensions`, `Core`, and `Data`
- `Databases.Authoring` and `Databases.Settings`
- `Editor`, `Editor.Mvvm`, `Entities`, and `Entities.Stats`
- `Mvvm`, `PageFlows`, and `PageFlows.MonoPages`
- `Persistence`, `Processing`, `PubSub`, and `VisualToolkit`

Apply these rules when choosing between related topics:

- Use `<Feature>` for runtime, package, or feature-owned documentation changes.
- Use `SourceGen.<Feature>` only for Roslyn-side behavior.
- Use a feature topic for its accompanying tests; use `Tests` or
  `SourceGen.Tests` only when the test change is independently cross-feature.
- Use `Samples.<Feature>` for an isolated sample and `Samples` for coordinated
  sample deployment or release updates.
- Use `Project` for root documentation unless a narrower feature or `Agents`
  topic clearly owns it.
- For a new module, derive the topic from its stable module name and apply the
  same `SourceGen.` and `Samples.` prefixes.

## 4. Commit Body

Omit the body when the subject fully explains the commit. When more context is
needed:

- Leave one blank line after the subject.
- Use concise `- <change>` bullets instead of prose paragraphs.
- Describe behavior, intent, compatibility, or a non-obvious reason.
- Do not list filenames or narrate the implementation process.
- Mention breaking behavior explicitly.

Example:

```text
Core: isolate cold-path exception construction

- Keep validation guards inlineable
- Move exception allocation behind no-inline helpers
- Preserve release-build behavior
```

## 5. Release Commit Series

Keep release outputs separate from their source changes. When applicable, use
this order:

1. Commit runtime, Source Generator, tests, and project changes by topic.
2. Commit shipped Roslyn artifact refreshes as
   `SourceGen: rebuild for <version>`.
3. Commit deployed sample refreshes as `Samples: update for <version>`.
4. Commit version and directly associated release metadata as
   `Version <version>`.

Skip a step when the release does not change that output.

