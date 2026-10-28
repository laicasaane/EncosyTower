# cbor family — handoff

Status (2026-10-28): research complete; no planning or code yet. Branch `private/wip-cbor`.

## Files

- `cbor-burst-feasibility.md` — Burst-compatible port of System.Formats.Cbor is feasible; blocker map, effort.
- `cbor-json-api-research.md` — RQ1–RQ12: layering, CBOR low-level API, JSON core, CBOR→JSON mapping, unified vs
  specialized (conversion: C; ease-of-use: Hybrid H), defaults, generator, performance, GenJson, dispatch (codec
  witness), packaging/phasing, open questions 1–9. Addendum: **JSON is opt-in** (CBOR primary; JSON in an optional
  assembly; generator emits JSON only on opt-in; CBOR track phased first).
- `cbor-json-api-astra-contrast.md` — independent Codex Astra xhigh pass: corrections C1–C6 and additions A1–A9 adopted;
  disagreements kept with reasons.

## Open Owner decisions (block planning)

1. Ease-of-use API: Hybrid H (recommended) or Specialized S.
2. JSON member naming default; strip `m_`/`_`?
3. Enum JSON style: names (recommended) or numbers.
4. First-release scope.
5. Attribute name.
6. Shared semantic format interface (recommended) or separate codecs.
7. Transcoder default: projection with loss flags (recommended) or strict.
8. Unity/Mathematics JSON shape: objects (recommended) or arrays.
9. JSON opt-in granularity: per assembly by reference (recommended), per type, or both.

## Next step

After the Owner answers, plan the family (plan-task-family, MODE NEW) starting with the CBOR track and Step 0 spike
(Ryu/fast_float kernels, RVA spans, `Nullable<T>`, constrained-generic codec call under Burst and IL2CPP).
