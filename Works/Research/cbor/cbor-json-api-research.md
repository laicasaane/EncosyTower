# CBOR ⇄ JSON conversion and ease-of-use serialization API — research

- Family: `cbor` (research). Date: 2026-10-28.
- Builds on: [cbor-burst-feasibility.md](cbor-burst-feasibility.md) (Burst port of `System.Formats.Cbor`, upstream
  `dotnet/dotnet@cd03739`).
- Reference studied: [PereViader/GenJson](https://github.com/PereViader/GenJson) @ `b4a181c` (v1.11.0, MIT).
- Toolchain facts (verified in repo): Unity `6000.3.27f1`, C# `-langversion:10`
  (`Packages/com.laicasaane.encosy-tower/EncosyTower.Core/csc.rsp`), Burst 1.8.31, Collections 2.7.1, Mathematics 1.3.3.
  C# 10 means **no static abstract interface members** and no `required` keyword.
- Existing repo assets worth reusing: `EncosyTower.SourceGen.Common` (`Printer`, `EquatableArray`, `NamingPolicy`,
  `UnmanagedSizeUpperBoundCalculator`), runtime `EncosyTower.Core/Naming/NamingPolicy.cs` (camel/kebab/snake policies),
  `EncosyTower.Core/Serialization.NewtonsoftJson` (current JSON path, AOT helper generator).
- Status: research only. Nothing compiled. Claims about Burst come from installed Burst docs; claims about GenJson were
  spot-checked against its source (whitespace limitation README.md:503-505, zero `ReportDiagnostic` calls in the
  generator, `Info<T>` registry GenJsonGenericRegistry.cs:13, throwing writes generator line 1847).

## Owner requirements (restated)

| # | Requirement |
|---|---|
| R1 | CBOR → JSON conversion works in managed context |
| R2 | ... and in Burst context |
| R3 | Offer both a unified API (one API for all contexts) and specialized APIs (managed / Burst) |
| R4 | Core CBOR (ported) stays a single unmanaged, Burst-compatible API |
| R5 | Core exposes a low-level public API for specialization |
| R6 | Ease-of-use API for JSON and CBOR: ready to use with "less customization" |
| R7 | High-performance ready |
| R8 | Roslyn source generators wherever possible |
| R9 | Study GenJson as reference |
| R10 | For the ease-of-use API, research both options: unified vs specialized |

**"Less customization" (Owner definition):** users never have to configure anything to get output and input that
conforms to JSON standards. Correctness and performance are the defaults. Contrast: Newtonsoft.Json needs converters,
settings, contract resolvers and AOT hints for correctness and speed. Configuration exists only for *preference*
(naming style, enum style), never for *correctness*.

## Research questions

- RQ1 — Layering: what layers, and which are Burst?
- RQ2 — Core CBOR low-level API shape (R4, R5).
- RQ3 — Burst-compatible JSON core: writer, reader, number formatting and parsing.
- RQ4 — CBOR → JSON conversion design (R1, R2): mapping rules and lossy cases.
- RQ5 — Unified vs specialized for the **conversion / low-level** layer (R3).
- RQ6 — Unified vs specialized for the **ease-of-use** API (R10).
- RQ7 — "Less customization" defaults: what the library decides automatically for JSON-standards parity (R6).
- RQ8 — Source generator design (R8): what users write, what is generated, Burst eligibility, analyzers.
- RQ9 — Performance strategy (R7).
- RQ10 — GenJson: what to borrow, what to avoid (R9).
- RQ11 — Generic dispatch under C# 10 without reflection or static abstracts.
- RQ12 — Packaging, assemblies, phasing, risks.

---

## RQ1 — Layering

```
L4  Ease-of-use facade        EncosyJson / EncosyCbor static APIs (managed + Burst overloads)
L3  Generated serializers     [Serializable-like attribute] partial types → Write<TWriter>/Read<TReader>
L2  Format-agnostic contracts IFormatWriter / IFormatReader (struct-constrained generics)
    Transcoders               Transcode<TReader, TWriter>  (CBOR→JSON, JSON→CBOR)
L1  Format cores (Burst)      CborWriter / CborReader (port)   JsonWriter / JsonReader (new)
L0  Primitives (Burst)        ByteBuffer (UnsafeList<byte>), Utf8 validation, base64, Ryu/fast_float numbers
--  Managed adapters          string / byte[] / Stream / IBufferWriter<byte> / class & collection support
```

- L0–L3 are **one codebase, Burst-compatible**, and also run as ordinary IL under Mono/IL2CPP. "Burst-compatible" code is
  valid managed code; the inverse is not true. This is the key fact behind the unified option.
- Managed adapters are additive and live in the same assembly (behind no conditional), so managed users never touch
  allocators unless they want to.

## RQ2 — Core CBOR low-level API

Keep the upstream surface almost verbatim (users of `System.Formats.Cbor` feel at home) but unmanaged:

```csharp
public struct CborWriter : IDisposable                 // Burst + managed
{
    public CborWriter(Allocator allocator, CborConformanceMode mode = CborConformanceMode.Strict,
                      bool convertIndefiniteLengthEncodings = false, int initialCapacity = 256);
    public CborWriter(byte* buffer, int capacity, ...);  // fixed external buffer, no growth (zero-copy)
    public CborError Error { get; }                      // sticky first error
    public void WriteInt32(int v); WriteInt64; WriteUInt64; WriteSingle; WriteDouble; WriteHalf(half v);
    public void WriteTextString(ReadOnlySpan<byte> utf8); WriteTextString<T>(in T fs) where T : unmanaged, IUTF8Bytes;
    public void WriteByteString(ReadOnlySpan<byte> bytes);
    public void WriteStartArray(int? length); WriteEndArray(); WriteStartMap(int? length); WriteEndMap();
    public void WriteTag(CborTag tag); WriteSimpleValue(CborSimpleValue v); WriteBoolean; WriteNull;
    public void WriteEncodedValue(ReadOnlySpan<byte> encoded);
    public ReadOnlySpan<byte> AsSpan();  public NativeArray<byte> AsNativeArray();
}

public struct CborReader                                // Burst + managed; pass by ref
{
    public CborReader(byte* data, int length, CborConformanceMode mode = Strict, bool allowMultipleRoots = false);
    public CborReaderState PeekState();
    public CborError TryReadInt32(out int v); ... TryReadTextString(out ReadOnlySpan<byte> utf8) ...
    public CborError TryReadStartMap(out int? length); ... SkipValue(); SlideData(...);
}
```

- Errors: `CborError` codes + sticky error (see feasibility report). Managed extensions convert to
  `CborContentException`.
- Integers: keep upstream `ReadCborNegativeIntegerRepresentation` / `WriteCborNegativeIntegerRepresentation`
  (major type 1 reaches −2⁶⁴, beyond `long`) as `TryRead…`/`Write…` *(Astra C5)*.
- Conformance modes: upstream `Canonical` is the legacy RFC 7049 §3.9 length-first order, and key equality compares
  encoded bytes (`CborConformanceLevel.cs:214-226`). Add `CoreDeterministic` (RFC 8949 §4.2.1, bytewise lexicographic)
  and document `Canonical` as legacy *(Astra C4)*.
- Low-level escape hatches for specialization (R5): `WriteEncodedValue`, raw initial-byte peek
  (`PeekInitialByte`), `ReadEncodedValue` (returns the raw slice of the next item), checkpoint/restore,
  and public `CborBinary` helpers (big-endian, half). These let users write custom converters without library changes.
- Managed-only extension methods (same namespace, separate static class): `WriteTextString(string)`,
  `ReadTextString() → string`, `BigInteger`, `decimal`, `DateTimeOffset`, `ToArray() → byte[]`.

## RQ3 — Burst-compatible JSON core (new code)

No existing piece fits: `System.Text.Json` is not in Unity's BCL, `Utf8Formatter` / `double.TryParse` are not Burst,
GenJson's runtime is managed (strings, `Span<char>`, `Utf8Formatter`, exceptions).

| Piece | Design | Notes |
|---|---|---|
| `JsonWriter` struct | UTF-8 only, over the same L0 `ByteBuffer`; `WriteStartObject/EndObject/StartArray/EndArray`, `WritePropertyName(utf8)`, `WritePropertyNameRaw(preEncoded)`, `WriteString(utf8)`, numbers, bool, null, `WriteRawValue`; optional indentation | Tracks a small container stack (bit stack like STJ) for comma/colon insertion and structural validation |
| String escaping | Scan for `"`, `\`, `< 0x20` (and optionally `0x7F`, `<`/`>`/`&` for HTML-safe mode); fast copy when clean; `\uXXXX` for controls | Burst auto-vectorizes the scan loop; optional SIMD via Burst intrinsics later |
| Strict UTF-8 validation | **Own validator** (overlongs, surrogates U+D800–DFFF, > U+10FFFF, truncation; escaped surrogate pairs on read) | Collections `Unicode.IsValidCodePoint` has its surrogate check commented out (`Unicode.cs:286-287`), so `Utf8ToUcs` is not strict *(Astra C1, verified)* |
| Double/float → text | **Port Ryu** (`d2s`, `f2s`): shortest round-trip, invariant. Typed `float` uses `f2s` (shortest binary32); generic projection widens binary16/32 exactly then `d2s` *(Astra C6)* | Needs 64×64→128 multiply (hand-written; `Math.BigMul(ulong…)` absent in Unity BCL) and ~10 KB tables as `static readonly ulong[]` (Burst-supported read-only arrays). Alternatives: Schubfach / Dragonbox (smaller tables). Collections' `FixedString.Append(float)` is not round-trip and has no `double` overload (checked `FixedStringAppendMethods.cs:195`) |
| Integer → text | Digit-count + two-digits-at-a-time table | Trivial |
| Text → double | **Port fast_float (Eisel–Lemire)** + slow-path fallback (big-decimal) | Needed by `JsonReader` for JSON→CBOR and typed JSON deserialization. Fallback path is the expensive part to port correctly |
| `JsonReader` struct | Pull tokenizer over UTF-8 (`TokenType`, `ValueSpan`, `HasEscapes`), RFC 8259 validating, whitespace-tolerant, depth limit, `TrySkip()` | GenJson's "no whitespace, no validation" parser is explicitly **not** acceptable |
| Base64 / base64url / base16 | Encode/decode helpers | For byte strings (RFC 8949 §6.1) |

## RQ4 — CBOR → JSON conversion design

Generic pull/push loop: `Transcode<TReader, TWriter>(ref TReader r, ref TWriter w, in Options o)` built on the L2
contracts; the CBOR→JSON specialization is just `Transcode<CborReader, JsonWriter>`. No intermediate tree, single pass,
Burst-compilable, O(depth) memory.

Mapping defaults (RFC 8949 §6.1 + I-JSON RFC 7493):

| CBOR | JSON default | Lossy? / option |
|---|---|---|
| int (major 0/1), within ±(2^53−1) | number | — |
| int beyond ±(2^53−1), up to 64-bit | number (exact digits) | Option: string for I-JSON interop |
| float16/32/64 | shortest round-trip number | width lost |
| NaN / ±Infinity | `null` (RFC 8949 §6.1) | Option: `"NaN"`/`"Infinity"` strings, or error |
| byte string | base64url, no padding (RFC 8949) | Tag 22 → base64, tag 23 → base16 |
| text string | string (escaped) | Invalid UTF-8 → error (strict) |
| array / map, definite or indefinite | array / object | indefiniteness lost |
| map key not a text string | int/float/bool keys stringified; other keys → diagnostic-notation string | Option: error |
| duplicate keys | pass through in Lax; error in Strict/Canonical | I-JSON forbids duplicates |
| false / true / null | same | — |
| undefined, other simple values | `null` | — |
| tag 2/3 bignum | base64url string (RFC 8949) | Option: decimal digits string |
| tag 0 (RFC 3339 string) | string | — |
| tag 1 (epoch) | number | — |
| other tags | inner value, tag dropped | Option: `{"tag":n,"value":…}` envelope (lossless, non-standard) |

Loss reporting and modes *(merged from Astra A3)*: the projection returns `CborJsonLoss` flags (NaN replaced,
bytes encoded, tag dropped, key stringified, …), so callers see what was lost without configuring anything. Built-in
modes: `Project` (default, the table above), `Strict` (fail on any loss), and `Lossless`: a versioned JSON envelope with
an unambiguous grammar for every node (no `$type` sniffing that would collide with user data). Signed zero is kept.
Astra recommends `Strict` as the default; that is Owner question O7.

Lossless alternative for debugging: CBOR diagnostic notation (RFC 8949 §8) writer — same transcoder skeleton, different
writer. Cheap to add and useful for an Editor inspector.

Reverse direction (JSON → CBOR) uses the same skeleton: `Transcode<JsonReader, CborWriter>`. Numbers: integers → CBOR
ints, non-integers → smallest lossless float width in Canonical mode, else double.

## RQ5 — Unified vs specialized: conversion / low-level layer

| Option | Shape | Pros | Cons |
|---|---|---|---|
| **A. Unified only** | One set of methods taking/returning unmanaged buffers (`ReadOnlySpan<byte>` in, `ref JsonWriter`/`NativeList<byte>` out). Managed callers pin `byte[]` | One code path, one test matrix | Managed callers deal with allocators/pinning; no `string` output |
| **B. Specialized only** | Separate managed (`string`, `byte[]`, `Stream`) and Burst (`NativeArray`, `FixedString`) APIs, each with own implementation | Idiomatic per context | Double implementation, divergence risk, double tests |
| **C. Unified core + thin specialized adapters (recommended)** | Option A is the implementation and is public; managed adapters (`CborJson.ToJsonString(byte[])`, `ToJson(ReadOnlySpan<byte>, IBufferWriter<byte>)`, `Stream`) are 5–20-line wrappers that pin/allocate and call A; Burst adapters add `FixedString` output and an `IJob` helper | One implementation; both contexts idiomatic; specialized API cost is tiny | Two API layers to document |

Recommendation: **C**. It satisfies R3 literally — the core *is* the unified API, the adapters *are* the specialized
APIs — without a second implementation.

Burst boundary rule: `Span<T>` cannot cross a `[BurstCompile]` entry-point signature. Expose function-pointer-friendly
entry points with `byte*` + `int` (e.g. `CborJson.Transcode(byte* src, int len, UnsafeList<byte>* dst, …)`) for users
who call Burst-compiled static methods from managed code.

## RQ6 — Unified vs specialized: ease-of-use API (R10)

Three real options. All assume generated per-type code (RQ8).

### Option U — Unified (one API surface for all contexts)

```csharp
[EncosySerializable] public partial struct PlayerState { public int Hp; public float3 Pos; public FixedString64Bytes Name; }

// identical calls in managed code and inside Burst jobs:
var buf = new NativeList<byte>(Allocator.Temp);
EncosyCbor.Serialize(in state, ref buf);
EncosyJson.Serialize(in state, ref buf);
CborError e = EncosyCbor.Deserialize(buf.AsArray().AsReadOnlySpan(), out PlayerState s);
```

- Generic static methods `where T : struct, IEncosySerializable` → constrained calls, Burst devirtualizes.
- **Works only for Burst-eligible types** (unmanaged structs, `FixedString*`, `FixedList*`, Mathematics types, enums,
  nested eligible structs). Classes, `string`, `List<T>` cannot be in a unified API by definition.
- Managed users must handle `NativeList` + allocator + `Dispose`. Not "ready to use" for typical managed data
  (save files, configs, network DTOs with strings).

### Option S — Specialized (two API surfaces)

```csharp
// Managed API — any type (classes, string, List<T>, Dictionary<string,T>, records)
string json  = EncosyJson.Serialize(player);
byte[] cbor  = EncosyCbor.Serialize(player);
Player p     = EncosyJson.Deserialize<Player>(json);      // throws EncosyJsonException
bool ok      = EncosyJson.TryDeserialize(json, out Player p2);
EncosyCbor.Serialize(player, stream); EncosyCbor.Serialize(player, bufferWriter);

// Burst API — Burst-eligible types only
BurstCbor.Serialize(in state, ref nativeList);
CborError e = BurstJson.TryDeserialize(span, out state);
```

- Each surface idiomatic: managed gets strings/exceptions/streams; Burst gets error codes/native containers.
- Risk: two generated method sets per type and two runtime implementations — unless both sit on the same L2 generic
  core (then "specialized" is only the facade).

### Option H — Hybrid: one generated core, unified facade names, specialized overloads (recommended)

- Generated per type (once): `void Write<TWriter>(ref TWriter w) where TWriter : struct, IFormatWriter` and
  `CborError Read<TReader>(ref TReader r) where TReader : struct, IFormatReader`. Same code serves CBOR and JSON,
  managed and Burst.
- Facade: **one static class per format** (`EncosyJson`, `EncosyCbor`) with overloads by context:
  - unified overloads (native buffers, error codes) — callable everywhere, the only ones legal in Burst;
  - managed overloads (`string`, `byte[]`, `Stream`, `IBufferWriter<byte>`, exceptions, class types) — 1–10-line
    wrappers that rent a `Temp`/pooled native buffer, call the unified overload, copy out.
- Unified and managed overloads differ by **parameter/return types** (`ref NativeList<byte>` / `ref JsonWriter` vs
  `string` / `byte[]` / `Stream`), never by constraints alone. C# signatures exclude constraints *(Astra C3)*. A
  Burst-ineligible type used with a unified overload is reported by an analyzer (ENCSER003) at compile time, and Burst
  would reject it anyway.
- Generated closed overloads per type or context root (Astra A1/A2) hide the codec witness:
  `EncosyJson.Serialize(in state, ref buf)` resolves to `Serialize<PlayerState, PlayerStateCodec>` without the user
  naming the codec.
- Users learn one name per operation (`EncosyJson.Serialize`), overload resolution picks the context. Advanced users
  drop to L2/L1.

| Criterion | U | S | H |
|---|---|---|---|
| Implementations to maintain | 1 | 2 (or 1 if shared core) | 1 |
| Managed ergonomics (strings, classes) | poor | good | good |
| Burst usability | good | good | good |
| API names to learn | 1 set | 2 sets | 1 set + overloads |
| Discoverability of "why not in Burst" | n/a | clear (different class) | needs analyzer |
| Risk of accidental managed call in Burst | none | low | low (Burst compile error + analyzer) |

Recommendation: **H**. If the Owner prefers stronger separation, S built on the same shared core costs only facade
code; switching between H and S later is cheap because the generated core is identical.

## RQ7 — "Less customization": automatic JSON-standards parity defaults

Target: every output is valid RFC 8259 **and** I-JSON (RFC 7493) unless the data itself cannot be represented; every
input that is valid RFC 8259 is accepted. No settings needed for that.

| Concern | Default (automatic) | Rationale |
|---|---|---|
| Encoding | UTF-8 output, no BOM; reader skips a leading BOM | RFC 8259 §8.1 |
| Escaping | Minimal escaping (`"`, `\`, controls); non-ASCII written raw | Valid, compact; HTML-safe escaping only as option |
| Invalid UTF-16 / UTF-8 in input strings | Error (managed: exception; Burst: `InvalidUtf8`); own strict validator | I-JSON §2.1 forbids; no silent corruption. The raw-token API can still expose lone-surrogate escapes, which are grammatical under RFC 8259 |
| Signed zero | `-0.0` preserved | Round-trip fidelity *(Astra A9)* |
| Whitespace in input | Accepted anywhere legal | GenJson's main gap |
| Culture | Always invariant; no culture setting exists | Removes a classic Newtonsoft/ToString bug class |
| float/double | Shortest round-trip (Ryu) | Exact round trip with zero settings |
| NaN/Infinity (typed serializer) | Write `"NaN"`, `"Infinity"`, `"-Infinity"` strings; read them back | Valid JSON, lossless round trip; matches STJ's named-literal convention and GenJson |
| NaN/Infinity (transcoder) | `null` per RFC 8949 §6.1 | Standard rule for generic conversion |
| long/ulong | Number with exact digits; reader parses exactly | Valid JSON; our reader is exact. I-JSON string mode as option |
| decimal (managed) | Number with exact digits | Matches STJ |
| byte[] / NativeArray<byte> (typed) | base64 (standard, padded) string | Matches STJ/Newtonsoft; reader also accepts base64url |
| Enums | JSON: name string; CBOR: integer; reader accepts both; `[Flags]` JSON as `"A, B"` like STJ | Readable JSON, compact CBOR, tolerant read. (GenJson and STJ default to numbers — open question) |
| Member names | As declared (C# name), with Unity prefix rules for `[SerializeField]` fields (`m_`/`_` stripped)? | **Open question** — decide once, assembly-level override available |
| Member order | Declaration order | Deterministic output, fast in-order reads |
| Missing members on read | Keep default value | Tolerant |
| Unknown members on read | Skip | Forward compatibility |
| Required members | Opt-in `[Required]` (C# 10 has no `required`) | — |
| Duplicate keys on read | Duplicate **known** member → error (bitset, near-free); unknown names not tracked by default. Strict/I-JSON profile tracks all names (hash plus decoded compare) | RFC 8259 SHOULD-unique, I-JSON MUST; *(changed after Astra A4, was "last wins")* |
| Null references | Write `null`; read `null` into reference/nullable | — |
| Nullable<T> | `null` or value | — |
| DateTime / DateTimeOffset | RFC 3339 string in JSON; CBOR tag 0 string (Canonical: tag 1 epoch option) | I-JSON §4.3 |
| TimeSpan | ISO 8601 duration or `c` format string | Pick one; open question |
| Guid | Canonical `D` string in JSON; CBOR tag 37 byte string | RFC 9562 / CBOR registry |
| Unity / Mathematics structs | Objects with component names (`{"x":…,"y":…}`), built-in converters shipped | Parity with `JsonUtility`; zero user converters |
| Dictionaries | `Dictionary<string,T>` → object; primitive/enum keys stringified; other keys → array of pairs | Matches STJ behavior for primitive keys |
| Polymorphism | Off unless `[Polymorphic]`; discriminator `"$type"` first | Avoid surprising output |
| Depth | Limit 64, error beyond | Protects against stack/DoS |
| Cycles | Error via active-path identity tracking (managed classes only); repeated acyclic references serialize repeatedly | Simple, predictable *(Astra A8)* |
| Enum edge cases | Unnamed values fall back to a number; reader accepts names or numbers | Automatic, no fallback attribute needed |

Configuration that remains (preference only): naming policy, enum style, indentation, HTML-safe escaping,
long-as-string, NaN policy, strictness. Set per type by attribute or per assembly by assembly attribute (generation-time,
zero runtime cost), plus a runtime `Options` struct for the few runtime choices (indentation, depth).

## RQ8 — Source generator design

User code:

```csharp
[EncosySerializable]                        // single attribute; CBOR always; JSON only when opted in (see Addendum)
public partial class SaveData { public string Name; public List<Item> Items; public DateTimeOffset At; }

[EncosySerializable]
public partial struct NetState { public int Tick; public float3 Pos; public FixedString32Bytes Tag; }   // Burst-eligible
```

Generated per type:

- `IEncosySerializable` (or managed variant) implementation: `Write<TWriter>`, `Read<TReader>`.
- Pre-encoded keys: per member, a static RVA blob with **both** encodings — CBOR (`0x6N` header + UTF-8) and JSON
  (`"name":`) — consumed by `TWriter.WritePropertyNameRaw(in PropertyKey)` so each key is one `memcpy` in either format.
- Read-side key matching: GenJson's in-declaration-order fall-through chain (fast when input order matches, which is
  always true for our own output), falling back to a length-bucketed `switch` + `ulong` compares for out-of-order keys.
  All Burst-friendly.
- Generated wire-size upper-bound function, **for fields and auto-properties only** (no double evaluation of arbitrary
  getters, Astra A5), to pre-size buffers and allow external fixed buffers. Note: `UnmanagedSizeUpperBoundCalculator`
  in SourceGen.Common computes memory layout, not encoded size, so it is **not** reusable for this *(Astra C2,
  verified)*.
- Context roots *(Astra A2)*: `[EncosySerializableRoot(typeof(ThirdPartyType))]` on a `static partial` class generates
  codecs for types the user does not own (Unity, other libraries) and for closed generic roots. Own types use the
  attribute directly.
- Burst eligibility computed transitively per type; eligible types also get the unified-overload interface. Ineligible
  types get only the managed interface.
- Root collections (`List<T>`, `T[]`, `Dictionary<string,T>` at the top level): handled by generic runtime helpers that
  call the element's generated code — no extra attribute needed (GenJson needs `[GenJsonSerializable]`; we can avoid
  it because our generic facade dispatches on `T`).
- Custom converters: duck-typed static class `Write<TWriter>(ref TWriter, in T)` / `Read<TReader>(ref TReader, out T)`
  registered by attribute (`[EncosyConverter(typeof(MyConv))]` on member, type, or assembly). Analyzer checks the shape
  and a code fix stubs it (GenJson GENJSON006 pattern). Converters written against the generic contracts work for both
  formats and both contexts at once.
- Generator implementation: split emitters, use `Printer`, equatable models, snapshot tests — per
  `build-generators-analyzers` conventions and EncosyTower's existing generator projects.

Analyzer diagnostics (proposed):

| Id | Meaning |
|---|---|
| ENCSER001 | Type not partial / is static |
| ENCSER002 | Unsupported member type (with reason) |
| ENCSER003 | Type used from Burst but contains managed member `X` (path shown) |
| ENCSER004 | Converter shape invalid (code fix: implement) |
| ENCSER005 | Duplicate serialized name after naming policy |
| ENCSER006 | Polymorphic derived type not registered / discriminator clash |
| ENCSER007 | Readonly member without matching constructor parameter (will not deserialize) |

## RQ9 — Performance strategy

- No reflection, no boxing, no delegates on hot paths; constrained generics only.
- Single growable `UnsafeList<byte>` per operation (Temp in jobs, pooled `Persistent` in managed adapters); optional
  external fixed buffer when size is known (exact-size path) for zero-copy into pinned `byte[]`.
- Pre-encoded keys (one memcpy), in-order key matching, ASCII fast paths, Burst-vectorized escape scan.
- Ryu + fast_float: fastest known correct algorithms; one formatting pass (GenJson formats floats twice to size them —
  avoid; size floats by upper bound 24/16 bytes instead).
- CBOR: definite-length maps/arrays when count is known at generation time (struct members) → no patching; canonical
  sort only in Canonical modes.
- Managed `string` output: write UTF-8 into native buffer, then one `Encoding.UTF8.GetString` (or `string.Create` with
  transcoding) — one final allocation.
- Benchmarks to set baselines: vs Newtonsoft (existing feature), vs GenJson, vs Unity `JsonUtility`, Burst vs Mono for
  the same unified path.

## RQ10 — GenJson lessons

Borrow:

1. Exact-size precompute → single write (adapt: upper bounds for floats).
2. Generation-time key bytes as RVA `ReadOnlySpan<byte>` (verify Burst accepts this form; fallback `static readonly
   byte[]`).
3. In-declaration-order key matching chain + skip-unknown.
4. Cursor + `bool Try…` parsing, no exceptions on hot path.
5. Duck-typed static converters + analyzer + code fix.
6. Attribute set: naming policy, rename, ignore, enum text/number + fallback, polymorphic discriminator (string or int),
   private-member opt-in, constructor/record binding.
7. `Info<T>` static-generic cache pattern for managed generic dispatch (no dictionaries).
8. Collection presizing (CBOR gives counts natively; JSON pre-count optional).

Avoid:

1. Parser that rejects whitespace and does not validate (README.md:503-505).
2. Formatting floats twice to compute size.
3. Duplicated char and byte implementations — we are UTF-8 only, managed `string` via adapter.
4. 4,255-line single-file `StringBuilder` generator with `Contains`-only tests and zero generator diagnostics.
5. `null` as the only error signal, throwing in write paths, non-standard `$Prop` count fields.
6. *(Astra A7)* Most-parameterized-constructor heuristic (`GenJsonSourceGenerator.cs:289`); required-ness tied to
   nullability (`:3136`, `:3328`); successful `null` treated as failure in `TryFromJson` (`:2929`); skip-value that does
   not validate literals and numbers (`GenJsonParser.cs:2211`).

## RQ11 — Generic dispatch under C# 10

Primary mechanism *(adopted from Astra A1)*: a generated **codec witness** per type.

```csharp
public interface ICodec<T>
{
    CborError Write<TWriter>(ref TWriter w, in T value) where TWriter : struct, IFormatWriter;
    CborError Read<TReader>(ref TReader r, out T value) where TReader : struct, IFormatReader;
}
public struct PlayerStateCodec : ICodec<PlayerState> { /* generated, fieldless → unmanaged */ }

public static CborError Serialize<T, TCodec>(in T value, ref NativeList<byte> dst)
    where TCodec : unmanaged, ICodec<T>  => default(TCodec).Write(ref /*writer*/, in value);
```

`T` is unconstrained, so it works for classes, structs and third-party types. A fieldless codec is unmanaged even when
`T` is a class, and there is no boxing. Generated closed overloads hide `TCodec` from users (RQ6). The instance-method
form below remains as sugar on the user's own types.

- Structs: `T : struct, IEncosySerializable` with instance `Write<TWriter>(ref TWriter)`; read via
  `T value = default; value.Read(ref reader);` (mutating `ref this` on a local). Constrained calls → no boxing, Burst OK.
- Classes (managed): `T : class, IEncosySerializableManaged, new()` or generated static factory cached in
  `EncosySerializer<T>.Read` (GenJson `Info<T>` pattern) populated by the type's static constructor / generated
  `[RuntimeInitializeOnLoadMethod]` registrar — needed when `T` comes from a generic context without constraints.
- Burst entry points from managed: generated `[BurstCompile]` static methods taking `byte*`/`UnsafeList<byte>*` per
  eligible type (opt-in, `[EncosySerializable(BurstEntryPoints = true)]`), callable via direct call.

## RQ12 — Packaging, phasing, risks

Assemblies (proposal): `EncosyTower.Serialization.Core` (L0–L2, Burst), `EncosyTower.Serialization.Cbor`,
`EncosyTower.Serialization.Json`, managed adapters inside each; generator project
`EncosyTower.Serialization.Generators` under `Plugins/SourceGenerator` following existing layout. Could later back
`EncosyTower.Persistence` as a storage format (not evaluated).

Phasing:

1. L0 + CBOR core port (feasibility report plan) + RFC 8949 Appendix A tests.
2. JSON writer + Ryu + transcoder CBOR→JSON (delivers R1/R2) + diagnostic notation.
3. JSON reader + fast_float + JSON→CBOR.
4. L2 contracts + generator (structs, primitives, Unity types) + analyzers.
5. Managed type support (classes, string, collections, polymorphism) + facade overloads.
6. Schema-free document access: managed DOM, Burst read-only token index *(Astra A6)*.
7. Benchmarks, IL2CPP/platform validation, fuzzing (JSONTestSuite, RFC 8949 vectors, Ryu/fast_float vectors), docs.

Step 0 (from the contrast): a spike that compiles Ryu/fast_float kernels, RVA `ReadOnlySpan<byte>` keys,
`Nullable<T>` fields and a constrained-generic codec call under Burst *and* IL2CPP, before any API freeze.

Risks: Ryu/fast_float port correctness (mitigate with upstream test vectors); Burst acceptance of RVA spans and
`Nullable<T>` (verify early with a spike); generator scope creep (keep attribute surface small); API naming decisions
must be fixed before step 4 (Owner rule: settle names first).

## Open questions for the Owner

1. Ease-of-use API: Hybrid H (recommended) or Specialized S?
2. Default JSON member naming: as-declared vs camelCase; strip Unity `m_`/`_` prefixes?
3. Default enum JSON style: names (recommended) vs numbers (STJ/GenJson default)?
4. Scope of first release: CBOR→JSON only, or both directions plus typed serialization?
5. Attribute name: `[EncosySerializable]`, or format-specific `[Cbor]`/`[Json]`?
6. Shared semantic format interface, one generated body (primary), or separate JSON/CBOR codecs (Astra)?
7. Transcoder default: RFC 8949 projection with loss flags (primary) or strict reject-unless-asked (Astra)?
8. Unity/Mathematics JSON shape: objects `{"x":…}` (primary, `JsonUtility` parity) or arrays `[x,y,z]` (Astra)?
9. JSON opt-in granularity: per assembly by referencing `EncosyTower.Serialization.Json` (recommended), per type
   (`[EncosySerializable(Formats = Cbor | Json)]`), or both (assembly turns it on, type narrows)? See Addendum.

## Independent review

An independent Codex pass (`gpt-6-astra`, `xhigh`, verified from the session log) answered RQ1–RQ12 without seeing
this report. Its six factual corrections (C1–C6) and nine additions (A1–A9) are merged above and marked *(Astra …)*.
Remaining disagreements and the reasons for each decision are in
[cbor-json-api-astra-contrast.md](cbor-json-api-astra-contrast.md).

## Addendum — JSON is opt-in (Owner clarification, 2026-10-28)

The sections above treated JSON as a co-equal format generated for every type by default (RQ8 sketch, RQ12 phasing).
That is wrong: **CBOR is the primary format; JSON, including CBOR→JSON conversion, is opt-in.** Changes:

| Area | Before | Opt-in revision |
|---|---|---|
| Assemblies (RQ12) | Core, Cbor, Json side by side | `Core` + `Cbor` are the base. `EncosyTower.Serialization.Json` is a separate, optional assembly that references Core + Cbor and owns `JsonWriter`/`JsonReader`, Ryu, fast_float, base64, the transcoder and the `EncosyJson` facade. A project that never references it ships none of that code |
| Generator (RQ8) | Emits CBOR and JSON members per type | CBOR members always. JSON-only output (JSON key blobs, `EncosyJson` overloads, JSON wire-size function) only when the type's assembly references the JSON assembly (generator checks `Compilation.ReferencedAssemblyNames`), optionally narrowed per type (O9). No JSON opt-in → no JSON code, smaller IL and Burst output |
| Pre-encoded keys | One blob with both encodings | CBOR blob always; JSON blob emitted only on opt-in |
| Shared format interface (RQ11, O6) | Argued for halving generated code | Still fits: `IFormatWriter`/`IFormatReader` live in Core; `JsonWriter` implements them from the optional assembly, so the one generated `Write<TWriter>` body costs nothing extra when JSON is off. Interface semantics are designed around CBOR; JSON maps onto them (counts ignored, bytes→base64). Astra's separate codecs also fit opt-in (generate the JSON codec only on opt-in), at the cost of a second body |
| Transcoder (RQ4, RQ5) | Core deliverable | Lives in the JSON assembly; always an explicit call (`CborJson.Transcode…`), never implicit |
| "Less customization" defaults (RQ7) | Mostly JSON concerns | JSON defaults apply only once opted in. CBOR needs its own automatic defaults: deterministic encoding off by default (Lax/Strict reader), tags for dates (tag 0/1), UUID (tag 37), bignum (tags 2/3), half/float/double shortest exact width, typed arrays (RFC 8746) for unmanaged arrays — no user settings needed |
| Phasing (RQ12) | JSON in phases 2–3, before the generator | CBOR track first: (1) L0 + CBOR core, (2) L2 contracts + generator for CBOR, (3) managed types + `EncosyCbor` facade. JSON track after, as its own optional deliverable: (4) JSON writer + Ryu + transcoder + diagnostic notation, (5) JSON reader + fast_float + JSON→CBOR, (6) generator JSON opt-in. DOM/token index and validation follow per track |
| Ease-of-use API (RQ6) | `EncosyJson`/`EncosyCbor` facades | Unchanged shape; `EncosyJson` exists only in the JSON assembly. The Hybrid H vs Specialized S comparison is unaffected |

Recommendation for O9: opt in per assembly by reference (zero attributes, matches "less customization"), with an
optional per-type `Formats` narrowing for large assemblies that want JSON on only a few types.
