# CBOR and JSON serialization — design

- Family: `cbor` (research). Date: 2026-10-28. Branch `private/wip-cbor`. Research only: nothing was compiled or run.
- Binding decisions: D1–D16 in [handoff.md](handoff.md). CBOR port feasibility:
  [cbor-burst-feasibility.md](cbor-burst-feasibility.md). Review passes, corrections and name candidates:
  [cbor-review-record.md](cbor-review-record.md).
- Toolchain (verified in repo): Unity `6000.3.27f1`, C# `-langversion:10`
  (`Packages/com.laicasaane.encosy-tower/EncosyTower.Core/csc.rsp`), so no static abstract interface members and no
  `required`. Burst 1.8.31, Collections 2.7.1, Mathematics 1.3.3.
- References: upstream `System.Formats.Cbor` at `dotnet/dotnet@cd03739`;
  GenJson (`github.com/PereViader/GenJson`) @ `b4a181c` (v1.11.0, MIT).
- **Names.** API names are open (D1, D3). Low-level CBOR types keep the upstream names (`CborWriter`, `CborReader`,
  `CborTag`, `CborConformanceMode`) as working names. Every other public name is a placeholder in angle brackets:
  `<Marker>` (type attribute), `<Formats>` (format flags), `<JsonOptions>` (JSON member options), `<CborFacade>`,
  `<JsonFacade>`, `<CborCodec<T>>`, `<JsonCodec<T>>`, `<ErrorCode>`, `<JsonWriter>`, `<JsonReader>`, `<Transcoder>`,
  `<Losses>`. Candidates are in the review record.

## 1. Requirements

- R1, R2: CBOR → JSON conversion works in managed and in Burst context.
- R3: offer both a unified API (one API for all contexts) and specialized APIs (managed / Burst).
- R4, R5: the ported CBOR core stays one unmanaged, Burst-compatible API and exposes a low-level public API for
  specialization.
- R6, R10: an ease-of-use API for JSON and CBOR, ready to use with "less customization"; research unified vs
  specialized for it.
- R7: high-performance ready. R8: Roslyn source generators wherever possible. R9: study GenJson as reference.

**"Less customization" (Owner definition):** users never configure anything to get output and input that conform to
the format's standards. Correctness and performance are the defaults. Configuration exists only for *preference*
(naming style, enum style), never for *correctness*. Contrast: Newtonsoft.Json needs converters, settings, contract
resolvers and AOT hints for correctness and speed.

First release (D4): everything — CBOR and JSON, both directions, typed serialization, and the D8 transcoder. Work
order inside the release: CBOR track first.

## 2. Layering

```
Serialization frontends    EncosyTower.Serializing   (folder EncosyTower.Core/Serializing)
  .Cbor   <CborFacade>, <CborCodec<T>>, generated CBOR bodies
  .Json   <JsonFacade>, <JsonCodec<T>>, generated JSON bodies, typed JSON policy helpers
JSON text routines         <JsonWriter> / <JsonReader>, escaping, Ryu, fast_float     (namespace open)
D8 transcoder              <Transcoder>, diagnostic-notation writer                   (namespace open)
Formatting backend (CBOR)  EncosyTower.Formatting    (folder EncosyTower.Core/Formatting)
  .Cbor   CborWriter / CborReader (port of System.Formats.Cbor)
  primitives: byte buffers, strict UTF-8 validator, base64/base64url/base16, big-endian and half helpers, <ErrorCode>
Managed adapters           string, byte[], Stream, IBufferWriter<byte>; BigInteger, decimal, DateTimeOffset
```

- **Formatting backend = CBOR (D12).** The low-level foundation is built for CBOR. The JSON text routines use its
  primitives (buffers, UTF-8 validation, error codes, depth limits, rollback) and carry no CBOR state.
- **Serialization frontends = CBOR and/or JSON (D12, D15).** Both depend on the backend, never on each other; they are
  optional to each other. Users opt in by attributes or configuration at assembly, type and member level.
- **Typed JSON never passes through CBOR (D14).** The JSON frontend writes and reads JSON text directly. The D8
  transcoder is a separate, untyped tool.
- **Namespaces (D2).** `EncosyTower.Formatting` holds higher-level formatting API if any is needed. The existing
  `EncosyTower.Core/Serialization` folder (namespace `EncosyTower.Serialization`) is renamed to `Serializing`.
- **Assembly placement is not a design concern (D13).** CBOR and JSON code may share an assembly or not; no opt-in
  rule, dependency or cost here depends on it. Opt-in never comes from an assembly reference or a namespace import.
- **Burst.** Everything except the managed adapters is Burst-compatible and also runs as ordinary IL under Mono and
  IL2CPP; the Burst boundary follows reachable code. `Span<T>` cannot cross a `[BurstCompile]` entry-point signature
  (`com.unity.burst@bfa84123c21a/Documentation~/csharp-type-support.md:146-157`): entry points take `byte*` + `int`
  and `UnsafeList<byte>*`; spans stay inside Burst code.
- **Unified core plus thin adapters (R3).** The unmanaged API is public and *is* the unified API. Managed adapters
  (`string`, `byte[]`, `Stream`, `IBufferWriter<byte>`) and Burst adapters (`FixedString` output, job helpers) are
  5–20-line wrappers over it, not a second implementation.

## 3. Low-level CBOR API

Keep the upstream API almost verbatim, as unmanaged structs. Port details, blockers and effort are in
[cbor-burst-feasibility.md](cbor-burst-feasibility.md).

```csharp
public struct CborWriter : IDisposable                   // Burst + managed
{
    public CborWriter(Allocator allocator, CborConformanceMode mode = CborConformanceMode.Strict,
                      bool convertIndefiniteLengthEncodings = false, int initialCapacity = 256);
    public CborWriter(byte* buffer, int capacity, ...);  // fixed external buffer, no growth
    public <ErrorCode> Error { get; }                    // sticky: first error wins, later calls no-op
    public void WriteInt32(int v); WriteInt64; WriteUInt64; WriteSingle; WriteDouble; WriteHalf(half v);
    public void WriteTextString(ReadOnlySpan<byte> utf8); WriteTextString<T>(in T fs) where T : unmanaged, IUTF8Bytes;
    public void WriteByteString(ReadOnlySpan<byte> bytes);
    public void WriteStartArray(int? length); WriteEndArray(); WriteStartMap(int? length); WriteEndMap();
    public void WriteTag(CborTag tag); WriteSimpleValue(CborSimpleValue v); WriteBoolean; WriteNull;
    public void WriteEncodedValue(ReadOnlySpan<byte> encoded);
    public ReadOnlySpan<byte> AsSpan(); public NativeArray<byte> AsNativeArray();
}

public struct CborReader                                  // Burst + managed; always passed by ref
{
    public CborReader(byte* data, int length, CborConformanceMode mode = Strict, bool allowMultipleRoots = false);
    public CborReaderState PeekState();
    public <ErrorCode> TryReadInt32(out int v); ... TryReadTextString(out ReadOnlySpan<byte> utf8) ...
    public <ErrorCode> TryReadStartMap(out int? length); ... SkipValue(); SlideData(...);
}
```

- **Errors.** Status codes plus a sticky error, never exceptions in Burst code. A `[Conditional]` check layer reports
  programmer errors in the Editor only. Managed extensions map the code to an exception.
- **Negative integers.** Major type 1 reaches −2⁶⁴, beyond `long`. Keep upstream
  `ReadCborNegativeIntegerRepresentation` / `WriteCborNegativeIntegerRepresentation` (`CborReader.Integer.cs:95`,
  `CborWriter.Integer.cs:79`) as `TryRead…` / `Write…`.
- **Conformance modes.** Upstream `Canonical` is the legacy RFC 7049 §3.9 length-first order, and key equality compares
  encoded bytes (`CborConformanceLevel.cs:214-226`). Add `CoreDeterministic` (RFC 8949 §4.2.1, bytewise lexicographic)
  and document `Canonical` as legacy.
- **Escape hatches for specialization (R5).** `WriteEncodedValue`, `PeekInitialByte`, `ReadEncodedValue` (raw slice of
  the next item), checkpoint/restore, and public big-endian and half helpers. Users write custom codecs without
  library changes.
- **Managed-only extensions** (separate static class): `WriteTextString(string)`, `ReadTextString() → string`,
  `BigInteger`, `decimal`, `DateTimeOffset`, `ToArray() → byte[]`.

## 4. JSON text routines on the CBOR foundation

New code. Nothing existing fits: `System.Text.Json` is not in Unity's scripting BCL; `Utf8Formatter` and
`double.TryParse` are not Burst; GenJson's runtime is managed (strings, `Span<char>`, exceptions).

| Piece | Design | Notes |
|---|---|---|
| `<JsonWriter>` | UTF-8 only, over the backend buffer. `WriteStartObject/EndObject/StartArray/EndArray`, `WritePropertyName(utf8)`, `WriteRawPropertyName(preEncoded)`, `WriteString(utf8)`, numbers, bool, null, `WriteRawValue`; optional indentation | Small bit stack for comma/colon insertion and structural checks |
| Escaping | Scan for `"`, `\`, `< 0x20` (optionally `0x7F` and `<`/`>`/`&` in an HTML-safe mode); fast copy when clean | Burst auto-vectorizes the scan loop |
| Strict UTF-8 | Own validator: overlongs, surrogates U+D800–DFFF, > U+10FFFF, truncation; escaped surrogate pairs on read | Collections `Unicode.IsValidCodePoint` has its surrogate check commented out (`com.unity.collections@88d5bf65ce66/Unity.Collections/Unicode.cs:286-287`), so `Utf8ToUcs` is not strict |
| Float/double → text | Port Ryu: `f2s` for `float` (shortest binary32), `d2s` for `double` | Needs a hand-written 64×64→128 multiply (`Math.BigMul(ulong…)` is absent in Unity's BCL) and ~10 KB tables as `static readonly ulong[]`. Alternatives: Schubfach, Dragonbox. Collections' `FixedString.Append(float)` is not round-trip and has no `double` overload (`FixedStringAppendMethods.cs:195`) |
| Integer → text | Digit count + two-digits-at-a-time table | — |
| Text → float/double | Port fast_float (Eisel–Lemire) with binary32 and binary64 paths, plus the slow-path fallback | The fallback is the expensive part to port correctly |
| `<JsonReader>` | Pull tokenizer over UTF-8 (`TokenType`, `ValueSpan`, `HasEscapes`), RFC 8259 validating, whitespace-tolerant, depth limit, `TrySkip()` | A parser that rejects whitespace and does not validate (GenJson README.md:503-505) is not acceptable |
| Base64 / base64url / base16 | Encode/decode helpers | Shared by typed JSON and the transcoder |

- The token writer and reader take no serializer options. Typed JSON policy (NaN text, base64 alphabet, enum names,
  dates, `Guid`, D9 shapes) lives in the JSON frontend helpers that generated JSON bodies call.
- The reader exposes the raw number token. A typed body parses it directly into the target type (`float` through the
  binary32 path, `decimal` from the digits), so no value goes through `double` first.
- The same routines serve the generated JSON bodies, the D8 transcoder and the diagnostic-notation writer.

## 5. D8 transcoder (separate, untyped)

An explicit call only, never used by typed serialization. Single pass, no intermediate tree, O(depth) memory,
Burst-compilable. Concrete CBOR→JSON and JSON→CBOR routines over the raw reader and writer APIs.

Default mapping (RFC 8949 §6.1 and I-JSON RFC 7493):

| CBOR | JSON default | Loss / option |
|---|---|---|
| int (major 0/1) within ±(2⁵³−1) | number | — |
| int beyond ±(2⁵³−1), down to −2⁶⁴ | number, exact digits | Option: string for I-JSON interop |
| float16/32/64 | widened exactly to binary64, then `d2s` | width lost |
| NaN / ±Infinity | `null` | Option: error |
| byte string | base64url, no padding | Tag 22 → base64, tag 23 → base16 |
| text string | string (escaped) | Invalid UTF-8 → error |
| array / map, definite or indefinite | array / object | indefiniteness lost |
| map key not a text string | int/float/bool keys stringified; others → diagnostic-notation string | Option: error |
| duplicate keys | pass through in Lax; error in Strict and deterministic modes | I-JSON forbids duplicates |
| false / true / null; undefined and other simple values | same; `null` | — |
| tag 2/3 bignum | base64url string | Option: decimal digits string |
| tag 0 / tag 1 | string / number | — |
| other tags | inner value, tag dropped | — |

- **Default (D8): projection with loss flags.** The call returns `<Losses>` flags (NaN replaced, bytes encoded, tag
  dropped, key stringified, …), so callers see what was lost without configuring anything. Signed zero is kept.
- **Strict mode is opt-in (D8):** fail on any loss.
- **Optional extras (proposals):** a lossless JSON envelope mode with an unambiguous grammar for every node (no
  `$type` sniffing); a CBOR diagnostic-notation writer (RFC 8949 §8) on the same skeleton, for an Editor inspector.
- **JSON → CBOR:** integers → CBOR integers; non-integers → smallest lossless float width in deterministic modes,
  else double. This direction loses decimal exactness by design; typed JSON never uses it.

## 6. Typed defaults per format

### 6.1 JSON frontend

Target: every output is valid RFC 8259 and I-JSON unless the data cannot be represented; every valid RFC 8259 input
is accepted. No settings are needed for that.

| Concern | Default (automatic) |
|---|---|
| Member names (D6) | As declared; strip a Unity `m_` or `_` prefix; the user can override per member |
| Enums (D7) | Name strings; numbers when the user asks (member, type or assembly). Reader accepts both |
| Unity / Mathematics types (D9) | Objects with component names (`{"x":…,"y":…}`); built-in codecs shipped |
| Encoding, escaping, culture | UTF-8, no BOM (reader skips one); minimal escaping (`"`, `\`, controls), non-ASCII raw, HTML-safe as an option; always invariant culture |
| Invalid UTF-8 / UTF-16 in strings | Error (managed: exception or error code; Burst: error code) |
| `float` / `double` | Shortest round trip (`f2s` / `d2s`); signed zero preserved |
| NaN / ±Infinity | Proposal: `"NaN"`, `"Infinity"`, `"-Infinity"` strings, read back (open edge) |
| `long` / `ulong` / `decimal` | Numbers with exact digits; reader parses exactly within the target type (limits open) |
| `byte[]` / `NativeArray<byte>` | Proposal: standard padded base64; reader also accepts base64url (open edge) |
| `[Flags]` and unnamed enum values | Proposal: flags as `"A, B"`; unnamed values fall back to a number (open edge) |
| Member order | Declaration order |
| Missing / unknown / required members | Keep default / skip / opt-in attribute (C# 10 has no `required`) |
| Duplicate keys on read | Duplicate known member → error (bitset, near free); full I-JSON tracking in a strict profile |
| Null references, `Nullable<T>` | `null` |
| `DateTime` / `DateTimeOffset`, `Guid`, `TimeSpan` | RFC 3339 string; `D` string; `TimeSpan` format open |
| Dictionaries | `Dictionary<string,T>` → object; primitive/enum keys stringified; other keys → array of pairs |
| Polymorphism | Off unless enabled per type; discriminator `"$type"` first |
| Depth, cycles | Limit 64; cycles → error via active-path identity tracking (repeated acyclic references serialize again) |

### 6.2 CBOR frontend

| Concern | Default (automatic) |
|---|---|
| Writer conformance | `Strict`; deterministic modes (`CoreDeterministic`, legacy `Canonical`, `Ctap2Canonical`) opt-in |
| Member keys | Text keys from the member name (renaming and stripping: open points 4, 5); integer keys on request per member |
| Enums | Integers |
| Floats | Shortest width that keeps the exact value (half/float/double) |
| Dates, `Guid`, big integers, `decimal` | Tag 0 (RFC 3339 string; tag 1 epoch as an option); tag 37; tags 2/3; tag 4 |
| Unmanaged arrays | Typed arrays (RFC 8746) |
| Maps / arrays | Definite length when the count is known at generation time (struct members), so no patching |
| Unity / Mathematics types | Shape open (array or map) |

### 6.3 Preference configuration

Naming policy, enum style, indentation, HTML-safe escaping, long-as-string, NaN policy, strictness. Set by attribute
per member, type or assembly (generation time, zero runtime cost), plus a small runtime options struct for runtime
choices (indentation, depth). Runtime options can vary policy inside generated code; they cannot create a missing
codec.

## 7. Generator

### 7.1 What users write

```csharp
[<Marker>]                                   // bare marker: CBOR (D15)
public partial struct NetState { public int Tick; public float3 Pos; public FixedString32Bytes Tag; }

[<Marker>(<Formats> = Cbor | Json)]           // both frontends
public partial class SaveData { public string Name; [<JsonOptions>(EnumAsNumber = true)] public Faction Faction; }

[<Marker>(<Formats> = Json)]                 // JSON-only type (D15)
public partial class ConfigDto { public string Url; public int Retries; }
```

### 7.2 One shared member model, two emitters (D11)

- The generator analyzes each type once into an equatable member model: symbols, access, construction binding,
  declaration order, raw annotations, eligibility per format, CBOR key, effective JSON name, JSON inclusion, enum style,
  and nested codec dependencies.
- From the model it derives a CBOR projection and a JSON projection (included members, keys, representations).
  Map counts, seen-member bits and required-member checks use the same projection as the body.
- A CBOR emitter writes `Write(ref CborWriter, in T)` / `Read(ref CborReader, out T)`; a JSON emitter writes the
  `<JsonWriter>` / `<JsonReader>` pair. Each runs only for the formats selected for that type. Bodies are concrete and
  non-generic; there are no public shared format reader/writer interfaces.
- A member excluded from a format is absent from that body: its getter is never called and its types do not enter
  that format's call graph. A suppressed write alone would still run the getter.
- Pre-encoded keys per selected format as RVA `ReadOnlySpan<byte>` blobs (CBOR: header + UTF-8 or integer; JSON:
  `"name":`), each written with one copy.
- Read matching: in-declaration-order fall-through chain (fast for our own output), falling back to a length-bucketed
  `switch` with `ulong` compares. CBOR also switches on integer keys. JSON compares decoded names: raw bytes when the
  name has no escapes, unescaped otherwise.
- Wire-size upper bound, generated for fields and auto-properties only (no double evaluation of arbitrary getters).
  `UnmanagedSizeUpperBoundCalculator` in SourceGen.Common computes memory layout, not encoded size, so it is not
  reusable (`UnmanagedSizeUpperBoundCalculator.cs:268-276`).
- Root collections (`List<T>`, `T[]`, `Dictionary<string,T>` at the top level) use generic runtime codecs that call the
  element's codec.

### 7.3 Opt-in resolution and precedence

Decided: a bare marker means CBOR (D15); assembly, type and member settings change it (D10, D12); opt-in is resolved at
generation time. An assembly-level setting is an assembly attribute, read from `compilation.Assembly` the same way
`SkipSourceGeneratorsForAssembly` is read (`EncosyTower.SourceGen.Helpers/Helpers/GeneratorHelpers.cs:9-39`).

Proposed precedence (both review passes; details open):

1. The assembly setting provides the default format set and format defaults (enum style, name casing). A type setting
   replaces or narrows it.
2. Member selection is separate per format. A member-level inclusion enables that member and its containing body when
   no explicit type exclusion forbids it.
3. Explicit exclusion (format removed from the set, ignore attribute) is hard. A contradictory inclusion is a
   diagnostic, never a silent re-enable. Equal-precedence contradictions are errors, never decided by build order.
4. A marked root that resolves to no format gets a diagnostic. An assembly default never turns every type into a root.

### 7.4 Nested types: codecs on demand (D16)

- If a type has format F, every member type it serializes in F needs an F codec: built-in, generated, custom, or from a
  context root.
- When the root policy allows, the generator emits codecs on demand for accessible nested types that lack one, keyed by
  (type, format, effective policy). It never enables the other format because both share the backend.
- Diagnostics only at explicit exclusions (a nested type that opted out of F) and unsupported types, with the format
  and member path.
- Generated code is never nested inside a type from another assembly; on-demand codecs for such types go under a
  consumer-owned generated type.

### 7.5 Third-party types, custom codecs, Burst eligibility

- **Context roots.** An attribute on a `static partial` class names a third-party type or a closed generic root; the
  generator emits its codecs under that class (with a member-name collision diagnostic). Own types use the marker.
- **Custom codecs** are per format. Shared conversion logic can feed both, but no format-generic custom codec is
  offered. The analyzer checks the codec shape and a code fix stubs it (GenJson GENJSON006 pattern).
- **Burst eligibility** is computed per format from the reachable operations of that format's body, transitively over
  nested codecs. The input must still be Burst-valid: excluding a managed member does not make a managed struct or
  class a valid job value, and a fieldless witness does not make the data Burst-safe.
- **Conventions.** Split emitters over one equatable model, `Printer`, `EquatableArray`, `NamingPolicy`, snapshot and
  tracked-step tests, following `build-generators-analyzers` and the projects under `Plugins/SourceGenerator`.

### 7.6 Diagnostics (categories; ids and severities are planning work)

The analyzer owns the diagnostics. Each names the format and the member path.

- Declarations: type not partial or static; generated name collides with a user member; readonly member without a
  matching constructor parameter; failed access or construction rules.
- Opt-in: contradictory inclusion and exclusion; empty format set after resolution; a format-specific option on a
  member or type without that format (no effect).
- Members: unsupported member type per format; duplicate effective JSON names after prefix stripping (within the JSON
  member set only); duplicate CBOR keys; invalid enum-number option.
- Nested types: dependency excluded from the format, or unsupported.
- Burst: type used from Burst with a managed member on the reachable path.
- Custom codecs: invalid shape (with code fix); missing half for a format the type uses.
- Polymorphism: derived type not registered, discriminator clash.
- Call sites: facade call for a format the type does not have (clearer than the generic-constraint error).

## 8. Dispatch under C# 10

A generated fieldless codec witness per format, called through constrained generics.

```csharp
public interface <CborCodec<T>>                          // one interface per format; no generic methods
{
    <ErrorCode> Write(ref CborWriter writer, in T value);
    <ErrorCode> Read(ref CborReader reader, out T value);
}
// <JsonCodec<T>>: the same with <JsonWriter> / <JsonReader>.

public static <ErrorCode> Serialize<T, TCodec>(in T value, ref NativeList<byte> output, TCodec codec)
    where TCodec : unmanaged, <CborCodec<T>>;
```

- `T` is unconstrained, so classes, structs and third-party types work. A fieldless codec is unmanaged even when `T` is
  a class; no boxing; Burst devirtualizes the constrained call
  (`com.unity.burst@bfa84123c21a/Documentation~/csharp-type-support.md:104-111`; generic-job discovery limits in
  `compilation-generic-jobs.md:45-77`).
- No generic virtual methods: the codec methods take concrete writer and reader types, so IL2CPP and Burst need no
  per-writer instantiation of an interface method.
- Nested members call concrete codecs directly (`default(Item.<Codec>).Write(ref writer, in value.Item)`). Constrained
  generics remain only at the facade and in container codecs.
- **Generated overloads cannot be added to a library static class.** Partial declarations must be in one assembly, so
  the facade cannot gain closed per-type overloads from user code. The facade stays generic. Ways to avoid naming the
  codec at the call site: infer `TCodec` from a `default` codec argument; the generator implements a per-format
  own-type interface on the user's partial type (explicit implementation) so the facade infers `T`; or generated
  consumer-owned helpers. Reads of reference types need construction: a `new()` constraint or a generated static
  generic cache (GenJson `Info<T>` pattern, `GenJsonGenericRegistry.cs:13`) — no reflection, no all-types registry.
- Burst entry points from managed code: optional generated `[BurstCompile]` static methods per eligible type taking
  `byte*` / `UnsafeList<byte>*`.

## 9. Ease-of-use facade: Hybrid H (D1)

One facade per format. Overloads by context, distinguished by parameter and return types only (C# signatures exclude
constraints):

- **Native overloads** (Burst and managed): caller-owned `NativeList<byte>` / spans, `<ErrorCode>` results, no strings,
  arrays or exceptions. The only ones legal in Burst.
- **Managed overloads**: `string`, `byte[]`, `Stream`, `IBufferWriter<byte>`, class types. 1–10-line wrappers that
  rent a native buffer, call the native path and copy out once.

```csharp
<ErrorCode> e = <CborFacade>.Serialize(in state, ref output);  // native: Burst job or managed code
e = <CborFacade>.Deserialize(input, out NetState copy);
e = <JsonFacade>.Serialize(in save, ref output);               // SaveData selects JSON
byte[] cbor = <CborFacade>.Serialize(save);                    // managed only
string json = <JsonFacade>.Serialize(save);
SaveData loaded = <JsonFacade>.Deserialize<SaveData>(json);
string text = <Transcoder>.CborToJson(cbor, out <Losses> losses);
```

A Burst-ineligible type used with a native overload is reported by the analyzer; Burst rejects it anyway. Open: facade
style and names, codec inference (section 8), and managed failure signalling (exceptions, or `Try*` with an error
code and `Result<T>` like `Serialization.NewtonsoftJson/JsonHelper.cs`).

## 10. Performance notes

- No reflection, boxing or delegates on hot paths; constrained generics and concrete calls only.
- One growable native buffer per operation (`Temp` in jobs, pooled `Persistent` in managed adapters); optional external
  fixed buffer when the size bound is known. Allocate only on map dedup, canonical sort and indefinite-string paths,
  never for scalars.
- Pre-encoded keys (one copy), in-order key matching, ASCII fast paths, vectorized escape scan.
- Ryu and fast_float, one formatting pass: size floats by an upper bound (24/16 bytes), never format twice.
- Typed JSON is one pass (D14): no intermediate CBOR bytes, no `Temp` buffer, no adapter state.
- Managed `string` output: UTF-8 into the native buffer, then one final `string` allocation.
- Borrowed name/string/number slices are consumed before the input changes; no output pointer is kept across buffer
  growth (`BufferNative.cs` invalidates views on resize, `:166-178`, `:291-313`).
- Baselines: Newtonsoft (existing `Serialization.NewtonsoftJson`), GenJson, Unity `JsonUtility`, Burst vs Mono.

GenJson lessons. Borrow: size precompute (as upper bounds), RVA key bytes, in-order key chain with unknown-member
skip, cursor + `bool Try…` parsing, duck-typed static codecs with analyzer and code fix, `Info<T>` cache, collection
presizing. Avoid: non-validating parser that rejects whitespace; formatting floats twice; separate char and byte code;
a single-file `StringBuilder` generator with no diagnostics; `null` as the only error signal; throwing writes; the
most-parameterized-constructor heuristic (`GenJsonSourceGenerator.cs:289`); required-ness tied to nullability
(`:3136`, `:3328`); successful `null` treated as failure (`:2929`); skip-value without literal and number checks
(`GenJsonParser.cs:2211`).

## 11. Phasing (one release, CBOR track first — D4)

0. Step 0 spike (section 12), before any API freeze. Names are settled before step 2 (Owner rule: names first).
1. Primitives and the CBOR core port, with RFC 8949 Appendix A vectors.
2. Generator member model, CBOR emitter, analyzers, CBOR facade: structs, primitives, Unity types.
3. CBOR managed types: classes, `string`, collections, polymorphism; managed facade overloads.
4. JSON text writer, Ryu, escaping, base64; D8 transcoder CBOR → JSON; diagnostic notation.
5. JSON reader, fast_float, JSON → CBOR.
6. JSON emitter and JSON facade, including JSON-only types.
7. Benchmarks, IL2CPP and platform checks, fuzzing (JSONTestSuite, RFC 8949, Ryu and fast_float vectors), docs.

## 12. Step 0 spike

One schema corpus: three structs with `int`, `ulong`, `float`, an enum, a `[Flags]` enum, `float3`,
`FixedString64Bytes`, a nested struct and `Nullable<int>`; one CBOR-only member; one JSON-only member; a JSON-only type;
a managed DTO. Record toolchain, safety checks, stripping level and IL2CPP code-generation mode for every run.

| # | Measurement | Acceptance question |
|---|---|---|
| 1 | Ryu/fast_float kernels, RVA `ReadOnlySpan<byte>` keys, `Nullable<T>` fields, constrained codec call, under Burst and IL2CPP | Do they compile and run in both? |
| 2 | Split bodies (D11) vs a shared-body candidate: Burst Inspector indirect calls, native size, Burst compile time | Does the split have no indirect calls? Does the shared body fold `w.Format == Json` branches? Does `typeof(TWriter) == typeof(CborWriter)` fold at all (not documented in the Burst docs)? |
| 3 | IL2CPP "Faster runtime" vs "Faster (smaller) builds": C++ for codec calls, binary size, 10k write/read loops | Direct calls or shared-generic lookups? |
| 4 | CBOR read: inline compare chain and integer `switch` vs reader-side member-index matching; in-order, shuffled, integer keys | Does index matching cost CBOR anything? |
| 5 | JSON-off and CBOR-off: Cecil scan of user-assembly IL for CBOR-only, JSON-only, both, neither, partial-member JSON | No JSON writer/reader/codec reference in a CBOR-only type, no CBOR reference in a JSON-only type; shared runtime IL counted separately; stripped share per managed stripping level |
| 6 | Generated size: lines, IL and table bytes per type, with and without JSON, split vs shared body | Replace estimates with numbers |
| 7 | Typed JSON: C1 direct body vs streaming C2 (JSON body → CBOR-model events → JSON sink) vs CBOR-bytes C2: throughput, first-output latency, allocations, peak scratch, under Burst and Mono | Switch from C1 only on a material measured gain (D14) |
| 8 | Float text: `0.1f` → `"0.1"` through `f2s`; projection gives `"0.10000000149011612"` | Typed and projected output stay distinct |
| 9 | Float parse: decimal → `float` direct vs decimal → `double` → `float` over fast_float binary32 vectors; exact decimal `9007199254740993.0`; 64-bit bounds; signed zero | No intermediate precision loss; errors at unsupported limits |
| 10 | JSON token writer on the backend buffer and error type | No CBOR state, no indirect calls; native size |
| 11 | Member semantics: excluded getter with a counter, D6 names and stripping, D7 enums, D9 objects, escaped/duplicate/unknown/shuffled keys, integer and text CBOR keys | Excluded getters never run; included getters run once; counts and bindings match |
| 12 | Opt-in combinations and nested on-demand codecs (D16), explicit nested exclusion, per-member custom codec | Correct closure per format; useful diagnostics; the other format never enabled implicitly |
| 13 | Incremental generator: tracked-step tests for the assembly default and on-demand nested codecs | Unrelated edits do not invalidate per-type steps |
| 14 | Failure paths: truncated/malformed input, output exhaustion, buffer growth, early sink failure | Error position, rollback of logical length, borrowed-slice lifetime |
| 15 | Facade shape: generic facade with inferred codec, own-type interface, consumer helpers; class reads | Assembly-valid and Burst/IL2CPP-valid without reflection |
| 16 | Cold and warm builds | Generator, C#, Burst and IL2CPP build times |

Correctness and structural JSON-off/CBOR-off absence come first. The Owner sets quantitative limits after the
baseline. If the shared body or streaming C2 wins materially, report the numbers and the reuse that produced the gain;
do not switch silently. Do not credit a candidate with speed it gets from weaker validation, failure behaviour or
precision.

## 13. Risks

- Ryu and fast_float port correctness (mitigate with upstream vectors); strict UTF-8 parity with the .NET decoder.
- Burst acceptance of RVA spans, `Nullable<T>` and constrained witnesses; IL2CPP generic sharing modes; unsafe
  lifetimes of borrowed slices across buffer growth.
- Generated code size: two bodies for dual-format types; on-demand nested codecs per (type, format, policy) can repeat
  across user assemblies. Incremental cost of on-demand closures and assembly defaults.
- Two emitters drifting: keep order, eligibility and null handling in the model; shared fixtures for common behaviour,
  separate expectations for intentional differences; no cross-format byte equality as an oracle. Custom codecs per
  format add user work for dual-format types.
- Name conflicts with Unity.Collections, Newtonsoft, Unity.Serialization, NUnit and BCL names (review record).
- Migration of `EncosyTower.Serialization`: 10 package files declare the namespace (6 in `Serialization.NewtonsoftJson`)
  and about 62 C# files reference it, generator test stubs included (grep, 2026-10-28).

## 14. Open points for the Owner

1. **Facade style and names**: per-format `*API` classes, scoped `Serializer` types in the format namespaces, or one
   class with `ToCbor`/`ToJson`; managed failure signalling; how the codec argument is hidden (section 8); all other
   names (candidates in the review record).
2. **Namespace of the JSON text routines and the transcoder**: D2 names only `EncosyTower.Formatting.Cbor`; both
   passes proposed `EncosyTower.Formatting.Json`; the transcoder in `EncosyTower.Formatting` or `.Formatting.Json`.
3. **CBOR shape of Unity/Mathematics types**: array (compact) or map (same shape as JSON).
4. **One wire name or two**: one rename for CBOR text keys and JSON, or a separate CBOR key and JSON name.
5. **Default member selection and prefix stripping**: public fields and properties only, or also `[SerializeField]`
   private fields; strip one `m_`, else one `_`; repeated prefixes; how "as declared" is told apart from
   `NameCasing.Pascal = 0`.
6. **D10 precedence details** (section 7.3), including an explicit-members mode on a type and on-demand discovery
   defaults for unmarked nested types.
7. **Typed JSON policy edges**: NaN/Infinity text, base64 flavour, `[Flags]` and unnamed enum values, exact-number
   limits, `TimeSpan` format, `DateTime` with unspecified kind.
8. **Write-only projection fallback** for CBOR-only custom codecs on JSON-enabled types (opt-in per codec, loss flags),
   or require a JSON half for every such codec.
9. **Migration** of `EncosyTower.Serialization` and `Serialization.NewtonsoftJson` (including the
   `NewtonsoftJsonAotHelper` generator's namespace constants) under the D2 rename.
10. **Low-level details** (proposals stand unless changed): writer buffer ownership (allocator-owned and fixed
    external buffer, both offered); conformance modes (all upstream modes plus `CoreDeterministic`); schema-free
    document access (managed DOM, Burst read-only token index) — in the first release or later.
