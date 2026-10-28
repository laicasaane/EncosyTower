# cbor family — handoff

Status (2026-10-28): research complete; design consolidated; no planning or code yet. Branch `private/wip-cbor`.

## Files

- `handoff.md` — this file: Owner decisions, open points, next step.
- [cbor-burst-feasibility.md](cbor-burst-feasibility.md) — Burst-compatible port of `System.Formats.Cbor` is feasible;
  blocker map, effort.
- [cbor-serialization-design.md](cbor-serialization-design.md) — current design: requirements, layering (CBOR backend;
  CBOR and JSON frontends), low-level CBOR API, JSON text routines, D8 transcoder, typed defaults, generator,
  dispatch, Hybrid H facade shape, performance, phasing, Step 0 spike, risks, open points.
- [cbor-review-record.md](cbor-review-record.md) — independent review passes (Opus and Codex Astra), corrections
  adopted, positions per question and the decisions they led to, open disagreements, name candidates and conflicts.

## Owner decisions (2026-10-28)

- D1 (O1) Ease-of-use API: **Hybrid H**. API names are not decided. No `Encosy` prefix on API names.
- D2 Module layout:
  - Low-level formats: module `EncosyTower.Formatting`, folder `EncosyTower.Core/Formatting`. Low-level CBOR in
    namespace `EncosyTower.Formatting.Cbor`. `EncosyTower.Formatting` itself holds higher-level formatting API if any
    is needed.
  - High-level JSON and CBOR serialization: module `Serializing`, namespace `EncosyTower.Serializing`, folder
    `EncosyTower.Core/Serializing`, with sub-namespaces `EncosyTower.Serializing.Cbor` and
    `EncosyTower.Serializing.Json`. The existing `EncosyTower.Core/Serialization` folder (namespace
    `EncosyTower.Serialization`) is renamed to `Serializing`.
- D3 (O5) Attribute and API naming rules (names still open):
  1. Fit terms used across EncosyTower.
  2. No alien wording.
  3. No unnecessarily repeated words.
  4. No unnecessary prefix or suffix. No `Encosy` prefix, except where a name would be ambiguous or conflict with
     another library or package (the prefix is currently used on extension classes for that reason).
- D4 (O4) First-release scope: **everything** — CBOR and JSON, both directions, typed serialization, generator JSON
  opt-in. CBOR track first inside the release.
- D5 (O6) Shared format interface vs separate codecs: re-investigated by an Opus pass and a Codex `gpt-6-astra`
  pass; resolved by D11.
- D6 (O2) JSON member names: as declared, strip `m_`/`_`; user can override.
- D7 (O3) Enum JSON: names by default; numbers when the user asks for them.
- D8 (O7) Transcoder: projection with loss flags by default; strict mode is opt-in.
- D9 (O8) Unity/Mathematics JSON shape: objects.
- D10 (O9) Opt-in granularity: per assembly + per type + per member (field/property), depending on context. Per
  assembly means an assembly-level attribute or configuration (D12).
- D11 (O6) **Split**: one shared member model in the generator, separately emitted CBOR and JSON bodies, no public
  shared format writer/reader interfaces. Step 0 still measures the shared-body candidate.
- D12 Two separate concepts:
  - **Formatting backend = CBOR.** The low-level foundation is built for CBOR.
  - **Serialization frontends = CBOR and/or JSON.** Both depend on the CBOR backend. They are not mutually exclusive
    and are optional to each other.
  - Users opt into a frontend by attributes or configuration, never by adding an assembly reference.
- D13 The location of CBOR and JSON code is never a design concern. They may live in the same assembly or in
  different assemblies; no decision or opt-in rule is based on assembly placement.
- D14 JSON frontend path **C1**: the generator emits a direct JSON body on the CBOR foundation; typed JSON never goes
  through CBOR bytes plus the D8 projection. Step 0 also measures a streaming C2 (JSON body → CBOR-model events → JSON
  sink); switch only on a material measured gain.
- D15 Frontends are independent (D-a); JSON-only types are allowed. A bare type marker means **CBOR by default**;
  assembly/type/member settings change it.
- D16 Nested member types without a codec for the selected format: the generator **emits codecs on demand** for
  accessible nested types when the root policy allows; diagnostics only at explicit exclusions or unsupported types.

## Open points for the Owner

Details and positions: [cbor-serialization-design.md](cbor-serialization-design.md) (section 14) and
[cbor-review-record.md](cbor-review-record.md).

1. Facade style and all API names (D3 rules apply; candidates and known conflicts in the review record).
2. Namespace of the low-level JSON text routines and the transcoder (D2 names only `EncosyTower.Formatting.Cbor`; both
   passes proposed `EncosyTower.Formatting.Json`).
3. CBOR shape of Unity/Mathematics types: array or map.
4. One wire name for CBOR text keys and JSON, or a separate CBOR key and JSON name.
5. Default member selection (public members only, or also `[SerializeField]` private fields) and prefix-stripping
   details (one `m_`, else one `_`; repeated prefixes; "as declared" vs `NameCasing.Pascal = 0`).
6. D10 precedence details: assembly/type defaults, member-only inclusion, explicit-members mode, hard exclusions,
   on-demand discovery defaults for unmarked nested types.
7. Typed JSON policy edges: NaN/Infinity, base64 flavour, `[Flags]` and unnamed enum values, exact-number limits.
8. Optional write-only projection fallback for CBOR-only custom codecs on JSON-enabled types.
9. Migration of `EncosyTower.Serialization` and `Serialization.NewtonsoftJson` under the D2 rename.

## Next step

1. Settle the names and the open points above with the Owner.
2. Then plan the family (plan-task-family, MODE NEW), CBOR track first, starting with the Step 0 spike (design
   section 12).
