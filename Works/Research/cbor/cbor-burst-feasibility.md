# CBOR in Burst-compatible code — feasibility research

- Family: `cbor` (research)
- Date: 2026-10-28
- Upstream studied: `dotnet/dotnet` @ `cd0373999f56e21da8bad8e12cb140622e2d6f4f` (last commit touching
  `src/runtime/src/libraries/System.Formats.Cbor`, 2026-10-23), folder
  `src/runtime/src/libraries/System.Formats.Cbor/src/System/Formats/Cbor` — 30 files, 5,434 lines. License MIT
  (port allowed with copyright + license notice kept).
- Target toolchain (from `Packages/manifest.json`): Burst 1.8.31, Collections 2.7.1, Mathematics 1.3.3,
  Entities 1.5.0.
- Burst rules cited from the installed Burst docs (`Library/PackageCache/com.unity.burst@*/Documentation~`):
  `csharp-type-support.md`, `csharp-hpc-overview.md`, `csharp-string-support.md`, `csharp-system-support.md`.

## Verdict

**Yes, feasible — as a port, not a reuse.** The CBOR wire format itself (RFC 8949) is pure byte/bit work and maps
cleanly to Burst. The upstream *algorithms* (state machine, conformance checks, canonical key sorting, indefinite-length
handling, half-float conversion) carry over almost line-for-line. The upstream *API shape and storage* do not: both
`CborReader` and `CborWriter` are managed classes built on managed collections, managed strings, exceptions and
`try/catch`. Roughly 60–70 % of the code is portable logic; the rest is re-plumbing (storage, error model, strings) plus
a small set of features that must stay managed-only.

Recommended shape: `CborWriter` / `CborReader` as unmanaged structs over `NativeList<byte>` / `UnsafeList<byte>` (writer)
and `byte*`+length or `NativeArray<byte>.ReadOnly` (reader), an `UnsafeList<Frame>` nesting stack, `CborError`
return codes instead of exceptions, UTF-8 bytes / `FixedStringN` instead of `string`, and a separate managed
extension layer for `string`, `BigInteger`, `decimal`, `DateTimeOffset`.

## Upstream structure (what exists)

| Area | Files | Notes |
|---|---|---|
| Core enums / helpers | `CborInitialByte.cs`, `CborTag.cs`, `CborConformanceLevel.cs`, `CborReaderState.cs`, `HalfHelpers.netstandard.cs` | Pure value logic; `CborConformanceModeHelpers` also owns two `static readonly UTF8Encoding` and key compare/hash. |
| Writer | `Writer/CborWriter*.cs` (9 files) | `class`; `byte[] _buffer` from `ArrayPool<byte>`; `Stack<StackFrame>`; `List<KeyValuePairEncodingRange>` + `HashSet<(int,int)>` per map; `List.Sort(IComparer)` for canonical maps. |
| Reader | `Reader/CborReader*.cs` (11 files) | `class`; `ReadOnlyMemory<byte> _data`; `Stack<StackFrame>`; `HashSet<(int,int)>` key dedup; `List<(int,int)>` indefinite-string chunk ranges; checkpoint/rollback via `try/catch`; new streaming `SlideData` (partial-buffer) support. |

Construct counts across the folder (grep, approximate):

| Construct | Hits | Burst status |
|---|---:|---|
| `class` declarations | 35 lines (reader, writer, options, comparers, exception, helpers) | Managed classes not allowed → structs / static classes |
| `throw` / `new ...Exception(` | 125 / 127 | Throw = editor-only, **abort in players**; BC1370 warning unless under `[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]` |
| `SR.` resource strings | 104 | Managed string formatting → error codes |
| `catch` / `finally` | 10 sites (`SkipValue`, `String`, `Tag`, `Writer.WriteEncodedValue`, `Writer.String`) | `catch` unsupported; `finally` skipped on exception |
| `Stack<` / `List<` / `HashSet<` | 10 / 24 / 19 | Managed → `UnsafeList<T>` / `NativeHashSet<T>` or linear scan |
| `byte[]` / `ArrayPool` / `Memory<` | 17 / 1 / 13 | Managed → native buffers, `byte*` |
| `Encoding` / `string` / `string.Create` | 228 / 55 | Managed → own strict UTF-8 validator, UTF-8 byte APIs, `FixedStringN` |
| `BigInteger` / `decimal` / `DateTimeOffset` | 14 / 22 / 13 | Managed / unsupported (`decimal` is explicitly unsupported by Burst) |
| `Span<`, `ReadOnlySpan<` | 71 | **Supported inside Burst**, not across the entry-point boundary |
| `BinaryPrimitives.*BigEndian` | 21 calls | Replace with `math.reversebytes` / shifts (portable, guaranteed Burst) |
| `HashCode.AddBytes` | 1 | Supported by Burst (results differ from Mono — fine, only used for in-process dedup) |
| `Half` | 65 | `HalfHelpers.netstandard.cs` is pure bit math → portable; or `Unity.Mathematics.half` |
| `int?` / `CborMajorType?` fields | many | `Nullable<T>` struct; simpler and safer to use `-1` sentinels |

## Blocker-by-blocker mapping

### 1. Reader/Writer are classes → unmanaged structs

State is small and already value-typed (`_offset`, `_frameOffset`, `_itemsRead`, `_definiteLength`, `_isTagContext`,
`_currentMajorType`, `_currentKeyOffset`, …). `StackFrame` and `Checkpoint` are already `readonly struct`s. Port as
`struct CborWriter : IDisposable` / `struct CborReader` with:

- Writer buffer: `NativeList<byte>` (owned, growable; replaces `byte[] + ArrayPool` and `EnsureWriteCapacity`), or an
  `UnsafeList<byte>*` variant for embedding inside other unmanaged containers.
- Reader input: `byte*` + `int length` (or `NativeArray<byte>.ReadOnly`); `ReadOnlyMemory<byte>` returns become
  `(offset, length)` slices or `ReadOnlySpan<byte>` *inside* Burst code.
- Nesting stack: `UnsafeList<StackFrame>` (or `FixedList512Bytes<StackFrame>` when `MaxDepth` is bounded — avoids an
  allocator for typical depths).
- Options classes (`CborReaderOptions`, `CborWriterOptions`) → plain structs / ctor params.

Allocator choice must be explicit (`Allocator.Temp` inside jobs, `Persistent` / `TempJob` otherwise) and the writer
needs `Dispose`.

### 2. Exceptions and `try/catch` → error codes

Burst: throw works in the Editor but aborts player builds; `catch` is unsupported; `finally` does not run on throw.
CBOR decoding of untrusted data fails routinely, so exceptions cannot be the error channel.

- Introduce `enum CborError : byte { None, EndOfData, InvalidMajorType, NonCanonicalInteger, InvalidUtf8,
  DuplicateKey, KeysNotSorted, DepthExceeded, UnexpectedBreak, DefiniteLengthMismatch, ... }` mapped from the 104 `SR.*`
  sites.
- API: `CborError TryReadInt32(out int value)` style, plus a thin `[Conditional]`-guarded check layer for programmer
  errors (e.g. writing past a definite length) that throws only in the Editor / with collection checks.
- The `catch`-based rollback sites become explicit checkpoint restore on non-`None` results:
  - `SkipValue` (`catch` + `finally` restoring conformance-check flag and checkpoint),
  - `ReadDateTimeOffset` / `ReadUnixTimeSeconds` / `ReadBigInteger` / `ReadDecimal` (rollback on bad payload),
  - UTF-8 `DecoderFallbackException` / `EncoderFallbackException` sites,
  - `WriteEncodedValue` validation (`catch (CborContentException)` → `ArgumentException`).
  All are local and mechanical; upstream already models them as checkpoint + restore.
- A "sticky error" field on the reader/writer (first error wins, later calls no-op) keeps call sites short in jobs.

### 3. Map key uniqueness and canonical sorting

- **Reader dedup** (`HashSet<(int Offset,int Length)>` with a comparer that hashes key bytes): replace with either a
  `NativeHashSet<KeyRange>` whose `KeyRange` stores a precomputed `xxHash3` / `HashCode` of the key bytes plus offset
  and length (equality then re-compares bytes through the reader's base pointer), or a per-map `UnsafeList<KeyRange>`
  with linear scan for small maps. Note: `NativeHashSet<T>` needs `T : IEquatable<T>` without a captured context, so
  equality must either carry the pointer in the element or fall back to hash + byte compare in a custom open-addressing
  set. Canonical / Ctap2 modes already check sorted order against the *previous* key only (`_previousKeyEncodingRange`),
  so uniqueness there is O(1) and needs no set.
- **Writer canonical sort** (`List.Sort(IComparer)` + `ArrayPool` temp buffer): replace with
  `UnsafeList<KeyValuePairEncodingRange>` + `NativeSortExtension.Sort<T, U>(U comparer)` where the comparer is an
  unmanaged struct holding `byte*` to the writer buffer and the mode. `CompareKeyEncodings` (length-first then
  lexicographic; Ctap2 major-type first) is pure span logic. Temp copy buffer from `Allocator.Temp`.
- The upstream per-map pooling (`Stack<HashSet<...>>`, `Stack<List<...>>`) becomes one reusable `UnsafeList` per depth
  level or a single shared list with per-frame start indices — simpler than upstream.

### 4. Strings (major type 3)

Burst has no `string`, no `char` (explicitly unsupported), no `System.Text.Encoding`.

- Burst-side API exposes text as UTF-8 bytes: `WriteTextString(ReadOnlySpan<byte> utf8)` (inside Burst),
  `WriteTextString(in FixedStringN)`, and `TryReadTextString(ref FixedStringN)` / `ReadDefiniteLengthTextStringBytes()`
  returning a slice. `FixedStringN` is UTF-8 already, so no transcoding.
- Strict-mode UTF-8 validation (upstream uses `UTF8Encoding(throwOnInvalidBytes: true)`) → **own validator**
  (overlongs, surrogates, > U+10FFFF, truncation). *Updated 2026-10-28:* Collections `Unicode.Utf8ToUcs` is not strict:
  `Unicode.IsValidCodePoint` has its surrogate check commented out (`Unicode.cs:286-287`).
- Indefinite-length text/byte strings: chunk ranges into `UnsafeList<Range>` (replaces `List<(int,int)>`), then
  concatenate into a caller-provided buffer / `NativeList<byte>`. `string.Create` path goes to the managed layer.
- `ReadTextString()` returning `System.String` stays managed-only (extension method).

### 5. Numbers

- Integers (`CborReader.Integer.cs`, `CborWriter.Integer.cs`): pure arithmetic with `checked`/`unchecked` — fully
  portable. Replace `checked` overflow exceptions with explicit range checks returning `CborError`.
- Big-endian I/O: 21 `BinaryPrimitives` calls → small `CborBinary` helpers using `math.reversebytes` (or shifts) and
  unaligned loads. Avoids depending on whether Unity's BCL `BinaryPrimitives` IL is Burst-clean.
- Half / float / double (`CborReader.Simple*.cs`, `CborWriter.Simple*.cs`): `HalfHelpers.netstandard.cs` is pure bit
  manipulation (ported from .NET `Half`) → portable as-is; alternatively `Unity.Mathematics.half` / `math.f16tof32` /
  `f32tof16`. Precision-preserving downcast checks (`(float)d == d`, NaN handling) port unchanged. Confirm rounding of
  `math.f32tof16` vs `HalfHelpers` (round-to-nearest-even) if switching — canonical encoders depend on exact results.

### 6. Tag helpers that stay managed

| Upstream API | Why not Burst | Plan |
|---|---|---|
| `ReadBigInteger` / `WriteBigInteger` (tags 2/3) | `System.Numerics.BigInteger` managed | Burst: expose raw tag + byte-string magnitude (`ReadTag` + bytes). Managed extension builds `BigInteger`. Optional: `UInt128`-like unmanaged struct for ≤128-bit. |
| `ReadDecimal` / `WriteDecimal` (tag 4) | `decimal` unsupported by Burst | Managed extension only; Burst side reads `[exponent, mantissa]` array as `long`s. |
| `ReadDateTimeOffset` / `WriteDateTimeOffset` (tag 0, RFC 3339 text) | `DateTimeOffset.TryParseExact`, `CultureInfo`, `string` | Managed extension. Burst can offer tag 1 (`WriteUnixTimeSeconds(long/double)`, `ReadUnixTimeSeconds` → `long`/`double`) — pure numbers. |
| `CborContentException` | managed exception type | Managed layer maps `CborError` → exception for ergonomic managed APIs. |

### 7. Everything else ports directly

`CborInitialByte`, `CborTag`, `CborSimpleValue`, `CborReaderState` + `PeekState` state machine, `Array.cs`,
`SkipValue` depth walk (after error-code rewrite), `ConvertIndefiniteLengthEncodings` patch-back in the writer
(in-place memmove on the native buffer — `UnsafeUtility.MemMove`), conformance-mode switch tables (replace `default:
throw new ArgumentOutOfRangeException` with validation at construction).

`SlideData` (streaming reader over partial buffers) ports fine: it only rebases offsets.

## Generics, Burst boundary, and API notes

- `Span<T>` / `ReadOnlySpan<T>` OK inside Burst methods/jobs, **not** in `[BurstCompile]` entry-point signatures. Jobs
  hold `NativeArray`/`UnsafeList` fields and build spans internally.
- Avoid interfaces on hot paths except constrained generics (`where TComparer : unmanaged, IComparer<T>`), which Burst
  devirtualizes.
- Value tuples allowed only internally (LayoutKind.Auto); prefer named structs (`KeyRange`) for anything stored in
  native containers.
- Keep reader `ref`-passed (`ref CborReader`) — struct copies would fork state.
- Allocation rule for hot paths: allocate only on map-with-dedup / canonical-sort / indefinite-string paths, never
  for scalars.

## Effort estimate (rough)

| Slice | Size | Risk |
|---|---|---|
| Core types + binary helpers + half | S | Low |
| Writer (ints, floats, simple, arrays, strings-as-bytes, tags 1/24) | M | Low |
| Writer canonical maps (sort + dedup) | M | Medium (comparer + temp buffer) |
| Reader state machine + PeekState + scalars | M | Medium (error-code rewrite of ~125 throw sites) |
| Reader maps dedup, indefinite strings, SkipValue rollback | M | Medium |
| UTF-8 strict validation parity | S | Medium (behavior parity with .NET strict decoder) |
| Managed extension layer (string, BigInteger, decimal, DateTimeOffset, exceptions) | S | Low |
| Tests: port upstream `System.Formats.Cbor.Tests` vectors (RFC 8949 Appendix A) in EditMode, plus a Burst-compiled job smoke test | M | Low |

Total: medium-sized feature, roughly 3–4k lines of ported code plus tests.

## Open questions for the Owner

1. Target surface: Burst-only core with a managed extension layer (recommended), or two separate implementations?
2. Buffer ownership: `NativeList<byte>`-owned writer (simple) vs. caller-supplied `UnsafeList<byte>*` (embeddable in
   ECS components) — or both?
3. Conformance modes: port all four (Lax, Strict, Canonical, Ctap2Canonical) or start with Lax + Canonical?
4. Placement: new assembly (e.g. `EncosyTower.Cbor`) vs. inside `EncosyTower.Core`; and whether a source generator for
   struct (de)serialization is a later goal (affects API naming now).

## Unverified / to check before planning

- ~~`Unicode.Utf8ToUcs` strictness~~ resolved: not strict (accepts surrogates), so an own validator is needed.
- `math.f32tof16` rounding parity with `HalfHelpers` for canonical float shortening.
- `NativeSortExtension.Sort` with a pointer-holding comparer under safety checks (should be fine with
  `[NativeDisableUnsafePtrRestriction]`-free plain struct, but confirm no AtomicSafety complaints).
- No code was compiled in this research; all Burst conclusions come from the installed Burst 1.8.31 docs and the
  upstream source.
