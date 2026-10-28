# Independent review contrast — Astra vs primary research

- Primary report: [cbor-json-api-research.md](cbor-json-api-research.md) (Claude Opus 5.5).
- Independent pass: Codex `gpt-6-astra`, reasoning effort `xhigh` (both read from the Codex session log), read-only
  sandbox, 2026-10-28. It received the requirements R1–R10, the Owner's definition of "less customization", RQ1–RQ12,
  and pointers to the local evidence: upstream CBOR source, GenJson clone, Burst and Collections sources, SourceGen.Common.
  It did **not** receive the primary report's answers.
- Raw Astra output (about 3,160 words) is kept in the session scratchpad, not in the repo. This file records the
  substance.
- Verification: the Astra claims marked ✔ below were re-checked by grep or read against the local sources before
  adoption.

## Summary

The two passes agree on the architecture: an unmanaged CBOR core plus a new unmanaged JSON engine; one implementation
with specialized adapters for the conversion layer (RQ5); a hybrid for the ease-of-use API (RQ6); Ryu and fast_float
for numbers; status codes instead of exceptions; and spans kept off Burst entry points.

They differ in four places:

- **Shared format interface.** Primary: one semantic `IFormatWriter`/`IFormatReader` used by a single generated method
  per type. Astra: separate JSON and CBOR codecs.
- **Conversion strictness.** Primary: RFC 8949 projection by default. Astra: reject by default, and offer separate
  lossy and lossless operations.
- **Some defaults:** NaN, enums, duplicate keys, byte-string alphabet, Unity type shape, member order.
- **Dispatch.** Astra adds a codec-witness mechanism and context roots, which also cover third-party types.

Astra also found **six factual errors or gaps** in the primary report. All are adopted.

## Corrections adopted from Astra

| # | Topic | Primary said | Astra finding | Check | Change in primary report |
|---|---|---|---|---|---|
| C1 | Strict UTF-8 | Use `Unicode.Utf8ToUcs` for validation; verify strictness | `IsValidCodePoint` has its surrogate rejection commented out, so it is not a strict validator | ✔ `Unicode.cs:286-287` | Own UTF-8 validator required (RQ3, RQ7) |
| C2 | Size precompute | `UnmanagedSizeUpperBoundCalculator` can help pre-size wire output | It computes unmanaged memory layout (`char`, `decimal`, alignment), not encoded size | ✔ `UnmanagedSizeUpperBoundCalculator.cs:268-276` | Removed; a wire-size function is generated separately, for fields only (RQ8, RQ9) |
| C3 | Overloads | Unified vs managed overloads separated by generic constraint | C# signatures exclude constraints; overloads must differ by parameter types | ✔ C# spec | RQ6 Option H now separates overloads by parameter and return types only |
| C4 | Canonical mode | Port four upstream modes as they are | Upstream `Canonical` uses the RFC 7049 §3.9 length-first order. RFC 8949 §4.2.1 Core Deterministic uses bytewise lexicographic order. Key equality compares encoded bytes, not semantics | ✔ `CborConformanceLevel.cs:214-226` | Add a `CoreDeterministic` mode, and name `Canonical` as legacy RFC 7049 (RQ2) |
| C5 | Negative ints | `TryReadInt64` style | CBOR major type 1 reaches −2⁶⁴, beyond `long` | ✔ RFC 8949 §3.1 | Keep upstream's `ReadCborNegativeIntegerRepresentation` / `WriteCborNegativeIntegerRepresentation` (`CborReader.Integer.cs:95`, `CborWriter.Integer.cs:79`) as `TryRead…`/`Write…`; the transcoder writes exact digits down to −2⁶⁴ (RQ2, RQ4) |
| C6 | Floats | "Ryu d2s/f2s" | Use f2s for typed `float` (shortest *binary32*). For generic projection, widen binary16/32 exactly, then format as binary64 | — (design) | Rule stated in RQ3/RQ4 |

## Additions adopted

| # | Addition | Where |
|---|---|---|
| A1 | **Codec witness** (`struct XCodec : ICodec<X>`, `where TCodec : unmanaged, ICodec<T>`, `T` unconstrained). It works for classes, structs and third-party types without boxing | RQ11 |
| A2 | **Context roots** (`[EncosySerializableRoot(typeof(Foo))]` on a `static partial` class) for types the user does not own and for closed generic roots. This is in addition to the attribute on the user's own types | RQ8 |
| A3 | Transcoder **loss reporting**: the projection returns `CborJsonLoss` flags (NaN replaced, bytes encoded, tag dropped, …), plus a strict mode (reject on any loss) and a **lossless JSON envelope** with an unambiguous grammar, not `$type` sniffing | RQ4 |
| A4 | Duplicate members of **known** names rejected by default with a bitset (cheap). Full I-JSON duplicate detection for unknown names in the strict profile (needs hashing plus decoded compare) | RQ7 |
| A5 | No measure-then-write over arbitrary getters (double evaluation, side effects). The size pass is limited to fields and auto-properties; otherwise use a growable buffer | RQ9 |
| A6 | Schema-free **document access**: a managed DOM, and in Burst a read-only token index over the buffer. Later phase | RQ12 |
| A7 | GenJson extra pitfalls: most-parameterized constructor heuristic (`GenJsonSourceGenerator.cs:289`), required-ness coupled to nullability (`:3136`, `:3328`), successful `null` treated as failure (`:2929`), skip-value not validating literals and numbers (`GenJsonParser.cs:2211`) | RQ10 |
| A8 | Cycle detection by active-path identity, so repeated acyclic references still serialize | RQ7 |
| A9 | Signed zero preserved (`-0.0`) in typed floats and in the transcoder | RQ7 |

## Disagreements — kept primary position (with reason)

| Topic | Primary | Astra | Decision |
|---|---|---|---|
| Shared format interface | One semantic `IFormatWriter`/`IFormatReader`; one generated `Write<TWriter>` per type for both formats | Separate JSON and CBOR codec interfaces; shared semantics only in the generator | **Kept primary, with mitigation.** One generated body halves generated code and keeps the two formats from drifting. The interface is semantic (`WriteBytes` becomes a CBOR byte string or JSON base64; the container count is always passed and JSON ignores it). Format-specific paths can use `typeof(TWriter) == typeof(CborWriter)` checks, which Burst and the JIT fold away. Open question O6 if the Owner prefers Astra's split |
| NaN/Infinity, typed | Write `"NaN"`/`"Infinity"` strings, read them back | Reject in ordinary JSON | **Kept primary.** It follows the "less customization" rule: a game state with NaN must not need a user setting to serialize. The output stays valid JSON and round-trips. Strict profile rejects |
| Transcoder default | RFC 8949 §6.1 projection | Reject unsupported values; lossy only on request | **Merged.** Default is the projection, now with loss flags (A3). Strict and lossless modes are built in. Owner question O7 |
| Enum JSON default | Name strings | Underlying integer, which keeps flags and unnamed values | **Kept primary, made automatic for edge cases.** Unnamed values fall back to a number and flags become `"A, B"`. The reader accepts both. Still an open question (O3) |
| `byte[]` JSON alphabet | base64 standard, padded | base64url, no padding | **Kept primary** for System.Text.Json and Newtonsoft interop, which Unity users meet most. The reader accepts both. The transcoder uses base64url (RFC 8949) |
| Member order | Declaration order | Ordinal wire-name order | **Kept primary.** Readable output, and fast in-order matching on read. CBOR deterministic modes sort independently |
| Unity / Mathematics shape | Objects `{"x":…}` | Arrays `[x,y,z]` | **Kept primary** to match `JsonUtility`. Arrays are smaller; listed as open question O8 |
| `DateTime` with unspecified kind | Write without offset | Reject | **Kept primary** (less customization); strict profile rejects |

## Agreements (both passes)

- Layers: primitives, then format engines, then transcoder, then generated bindings, then adapters. Everything except
  adapters is Burst-compatible, and the Burst boundary follows reachable code, not assembly names.
- Conversion layer: one unmanaged engine callable from both contexts, plus thin managed and native adapters (RQ5).
- Ease-of-use API: hybrid, not pure unified and not pure specialized (RQ6).
- JSON numbers: port Ryu and fast_float. Collections' float formatting (7 digits, `FixedStringUtils.cs:137`) and
  parsing (`FixedStringParseMethods.cs:152`) are not acceptable.
- Status-code errors with rollback; managed wrappers throw.
- Invariant culture everywhere; UTF-8 without a BOM; reject invalid Unicode; depth limit 64; 64-bit integers written
  as exact numbers (a string mode only per declared type, never decided by value).
- No reflection fallback; source generation only; analyzers own the diagnostics.
- Reuse `EquatableArray`, `NamingPolicy` and `Printer` from SourceGen.Common.
- Top risks: numeric correctness, Unicode validation, unsafe lifetimes, generated code size, real Burst and IL2CPP
  behavior.

## New Owner questions raised by the contrast

- O6 — Shared semantic format interface (primary) or separate JSON and CBOR codecs (Astra)?
- O7 — Transcoder default: RFC 8949 projection with loss flags (primary) or reject-unless-asked (Astra)?
- O8 — Unity and Mathematics JSON shape: objects (primary) or arrays (Astra)?
