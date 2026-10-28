# CBOR and JSON serialization — review record

- Family: `cbor` (research). Date: 2026-10-28. Branch `private/wip-cbor`.
- Current design: [cbor-serialization-design.md](cbor-serialization-design.md). Owner decisions D1–D16:
  [handoff.md](handoff.md).
- This file records the independent review passes: corrections adopted, positions per question and the decision each
  led to, disagreements still open, and name candidates with known conflicts. The pass reports themselves are not
  kept in the repo.

## Passes

Each pair (3/4, 5/6) answered one brief independently; neither pass read the other's report. Pass 2 did not see the
primary research's answers.

| # | Pass | Questions | Model and effort | Verification |
|---|---|---|---|---|
| 1 | Primary research | RQ1–RQ12 (layering, CBOR API, JSON core, transcoder, unified vs specialized, defaults, generator, performance, GenJson, dispatch, phasing) | Claude Opus 5.5 | Unverified |
| 2 | Earlier Astra pass | RQ1–RQ12, blind | Codex `gpt-6-astra`, `xhigh`, read-only sandbox | Verified from the Codex session log |
| 3 | O6 and naming, Opus | O6 with D1–D10 applied; names under D3 | `opus-high` subagent | Unverified |
| 4 | O6 and naming, Astra | Same brief as 3 | Codex `gpt-6-astra`, `xhigh` | Verified from the rollout; the launcher warned that codex-cli 0.165.0 is newer than its checked version |
| 5 | JSON path and opt-in, Opus | C (typed JSON path), D (frontend opt-in), D11–D13 applied | `opus-high` subagent | Unverified |
| 6 | JSON path and opt-in, Astra | Same brief as 5 | Codex `gpt-6-astra`, `xhigh` | Verified from the rollout (the contrast records no launcher warning) |

Passes 4 and 6 state that their supporting reads used pinned `opus-high` / `opus-medium` routes, model and effort
Unverified. Claims marked "checked" below were re-read against local sources, by the orchestrator before adoption or
during the 2026-10-28 rewrite of these documents.

## Corrections adopted

| # | Finding | Evidence | Pass |
|---|---|---|---|
| 1 | Collections `Unicode.Utf8ToUcs` is not a strict validator: the surrogate check in `IsValidCodePoint` is commented out. Own UTF-8 validator required | `com.unity.collections@88d5bf65ce66/Unity.Collections/Unicode.cs:286-287` (checked) | 2 |
| 2 | `UnmanagedSizeUpperBoundCalculator` computes memory layout, not encoded size; a wire-size bound is generated separately, for fields and auto-properties only | `UnmanagedSizeUpperBoundCalculator.cs:268-276` (checked) | 2 |
| 3 | Overloads cannot differ by generic constraints alone; facade overloads differ by parameter and return types | C# spec | 2 |
| 4 | Upstream `Canonical` is RFC 7049 length-first order; add `CoreDeterministic` (RFC 8949 §4.2.1) | `CborConformanceLevel.cs:214-226` (checked) | 2 |
| 5 | Major type 1 reaches −2⁶⁴; keep the upstream negative-integer representation APIs | RFC 8949 §3.1; `CborReader.Integer.cs:95`, `CborWriter.Integer.cs:79` | 2 |
| 6 | Typed `float` uses `f2s`; projection widens binary16/32 exactly, then `d2s` | design rule | 2 |
| 7 | Generated closed overloads on a library facade cannot be built: partial declarations cannot span assemblies. The facade stays generic | C# partial-type restrictions | 3, 4 |
| 8 | `FormatError`, `ParseError`, `ConversionError` are public in `Unity.Collections`; `Error` clashes with `EncosyTower.Common.Error` | `Unicode.cs:9` (checked); `EncosyTower.Core/Common/Error.cs:8` (checked) | 3, 4 |
| 9 | `JsonWriter` / `JsonReader` clash with Newtonsoft (referenced by Core) and `Unity.Serialization.Json` | `EncosyTower.Core.asmdef:22`; `Serialization.NewtonsoftJson/JsonHelperExtensions.cs:9`, `:47`; `com.unity.serialization@7725fb70c685/.../Json/JsonWriter.cs:69` (checked) | 3, 4 |
| 10 | Burst folding of `typeof(TWriter) == typeof(...)` is not documented (grep of Burst `Documentation~` finds `typeof` only in `MonoPInvokeCallback` examples). Unverified; Step 0 item | Burst 1.8.31 docs | 3 |
| 11 | `ICodec<T>.Write<TWriter>` is a generic method on an interface; Burst documents interface-constrained generics, not generic interface methods; IL2CPP needs every instantiation ahead of time. Codec methods take concrete writer types | `csharp-type-support.md:104-111` (checked) | 3 |
| 12 | One semantic writer cannot serve both the D8 transcoder and typed JSON: NaN is `null` vs `"NaN"`; bytes are base64url vs standard base64 | design | 3 |
| 13 | A member excluded from a format must not have its getter called; a suppressed write alone still runs it and keeps its types in the Burst call graph | design | 4, 6 |
| 14 | Burst eligibility is per format's reachable operations; a fieldless witness does not make managed data Burst-safe | design | 4 |
| 15 | Typed JSON through ordinary CBOR plus D8 projection is wrong in nine places: enum names, integer keys, `float` text (`0.1f` → `0.10000000149011612`), `Guid`, `decimal`, byte alphabet, NaN, Unity shapes, JSON-excluded members | design | 5, 6 |
| 16 | JSON → CBOR → typed reads lose precision: decimal → `double` → `float` double rounding; `9007199254740993.0` is exact in `decimal`, not in binary64. Parse number tokens directly into the target type | design | 5, 6 |
| 17 | "Never through CBOR" was too broad: a streaming C2 (JSON body → CBOR-model events → JSON sink) is viable and may compile to the same operations as C1 | design | 6 |
| 18 | Borrowed slices must be consumed before input changes; buffer growth invalidates views | `EncosyTower.Core/Buffers/BufferNative.cs:166-178`, `:291-313` | 6 |
| 19 | `EncosyTower.Core.asmdef` is the only asmdef under `EncosyTower.Core` (checked). Set aside by D13: assembly placement does not drive design or opt-in | `EncosyTower.Core.asmdef:27-93` | 3 |

Additions from pass 2, all adopted: A1 fieldless codec witness with `T` unconstrained; A2 context roots for types the
user does not own; A3 transcoder loss flags, strict mode, optional lossless envelope; A4 duplicate known members
rejected with a bitset, full tracking in a strict profile; A5 no measure-then-write over arbitrary getters; A6
schema-free document access (DOM, Burst token index); A7 GenJson pitfalls (`GenJsonSourceGenerator.cs:289`, `:3136`,
`:3328`, `:2929`; `GenJsonParser.cs:2211`); A8 cycle detection by active-path identity; A9 signed zero preserved.

## Questions, positions and decisions

| Question | Positions | Decision |
|---|---|---|
| O1 Ease-of-use API: unified, specialized, hybrid | Pass 1: Hybrid H. Pass 2: a hybrid | D1 Hybrid H; names open |
| O2 JSON member names | As declared vs camelCase; strip Unity prefixes? | D6 as declared, strip `m_`/`_`, user override |
| O3 Enum JSON style | Pass 1: names. Pass 2: integers (STJ and GenJson default) | D7 names; numbers on request |
| O4 First-release scope | CBOR→JSON only vs everything | D4 everything; CBOR track first |
| O5 Attribute and API naming | Prefixed single attribute vs format-specific | D3 naming rules; no `Encosy` prefix |
| Module layout | Pass 1: `Serialization.*` assemblies | D2 `Formatting` / `Serializing` namespaces and folders |
| O6 Shared format interface | Pass 1: shared `IFormatWriter`/`IFormatReader`, one body. Pass 2: separate codecs. Passes 3 and 4: shared member model, separate emitted bodies | D5 reopened it; D11 split, Step 0 measures both |
| O7 Transcoder default | Pass 1: projection. Pass 2: reject unless asked | D8 projection with loss flags; strict opt-in |
| O8 Unity/Mathematics JSON shape | Pass 1: objects (`JsonUtility` parity). Pass 2: arrays | D9 objects |
| O9 JSON opt-in granularity | Pass 1: by assembly reference, optional per-type narrowing | D10 assembly/type/member; D12 by attributes or configuration (assembly = assembly attribute); D13 placement irrelevant |
| C Typed JSON path | Both: C1 direct. Pass 5 drops C2; pass 6 keeps streaming C2 for Step 0 | D14 C1; Step 0 measures streaming C2 |
| D Frontend opt-in | Both: D-a independent, JSON-only types allowed. Bare marker: pass 5 CBOR, pass 6 none unless a setting chooses | D15 D-a; bare marker = CBOR |
| Nested types without a codec | Pass 5: diagnostic + code fix. Pass 6: on-demand generation | D16 on demand; diagnostics at exclusions and unsupported types |

## Disagreements still open

| Topic | Position A | Position B |
|---|---|---|
| Typed NaN/Infinity | Pass 1: `"NaN"`/`"Infinity"` strings, read back | Pass 2: reject in ordinary JSON |
| Typed `byte[]` alphabet | Pass 1: standard padded base64 (STJ/Newtonsoft interop); reader accepts both | Pass 2: base64url, no padding |
| `[Flags]` and unnamed enum values | Pass 1: `"A, B"`; unnamed → number | Pass 2: integers keep both cases |
| Member order | Pass 1: declaration order (design default) | Pass 2: ordinal wire-name order |
| `DateTime` with unspecified kind | Pass 1: write without offset | Pass 2: reject |
| One wire name or two | Pass 3: one rename for CBOR text keys and JSON | Pass 4: CBOR text key via the key attribute, JSON name separate |
| Facade style | Pass 3: per-format `*API` classes | Pass 4: scoped `Serializer` per format namespace, `using` aliases when mixed |
| Managed failure signalling | Pass 3: throwing managed overloads | Pass 4: `Try*` with error code; `Result<T>` like `JsonHelper` |
| Hiding the codec argument | Pass 3: per-format own-type interface; facade infers `T` | Pass 4: explicit codec argument; optional consumer-owned helpers |
| Generated codec type | Pass 3: one nested `Codec` for both formats | Pass 4: nested `CborCodec` / `JsonCodec` |
| Transcoder namespace | Pass 3: `EncosyTower.Formatting` | Pass 4: `EncosyTower.Formatting.Json` |
| Name casing | Pass 3: reuse `NameCasing` (but `Pascal = 0` is not "as declared") | Pass 4: do not reuse a casing policy for D6 |
| CBOR-only custom codec on a JSON type | Pass 5: optional write-only projection fallback with loss flags | Others: JSON half required |
| D10 details | Pass 4: assembly/type opt-in enables defaults; member annotation enables that member; `ExplicitMembers` mode; hard exclusions | Pass 5: same precedence; exact defaults unsettled |

## Name candidates

Filtered under D3 and D11–D13. Dropped: `EncosyCbor`/`EncosyJson` and `EncosyJsonWriter`/`EncosyJsonReader`
(`Encosy` prefix without a conflict, D3); `IFormatWriter`/`IFormatReader` and `IWriter`/`IReader` (no shared format
interfaces, D11); arguments that rested on a JSON assembly boundary (D13). Pass 3 assumed CBOR is always on; under D15
its JSON attribute combines with a `Formats` setting.

| Role | Pass 3 (Opus) | Pass 4 (Astra) | Conflicts and notes |
|---|---|---|---|
| Type attribute | `[Serialize]`; alts `[Codec]`, `[Serializing]` | `[Serialize]`; alt `[GenerateSerializer]` | Mirrors `[Persist]` (`EncosyTower.Persistence/Annotations/PersistAttribute.cs:10`). `[Serializable]` is BCL |
| Format flags | `Formats` property + flags enum | Same; alts `AllowedFormats`, `SerializationFormats` | Localization has `Format` (singular) |
| JSON options, assembly/type/member | `[Json]` (`Name`, `EnumAsNumber`, `Ignore`); alts `[JsonMember]`, `[Serialize(WithJson = true)]` + `[assembly: JsonForAssembly]` | `[SerializeJson]` (`Name`, `EnumAsNumber`, `ExplicitMembers`); alts `[IncludeJson]`, `[GenerateJson]` | `JsonAttribute` exists in System.Text.Json; `JsonIgnore`/`JsonProperty` in Newtonsoft |
| Ignore in all formats | `[SerializeIgnore]`; alts `[NotSerialized]`, honour `NonSerializedAttribute` | `[SerializeIgnore]` with optional `Formats`; alt `[IgnoreMember]` | `Ignore` NUnit; `IgnoreMember` MessagePack; `DontSerialize` Unity.Serialization |
| Rename | `[SerializeName]` both formats; `[Json(Name)]` JSON-only | `[SerializeJson(Name)]` JSON-only; alt `[JsonName]` | Depends on one wire name or two |
| Order | `[SerializeOrder(n)]` | `[SerializeOrder(order: n)]`; alt `[MemberOrder]` | `Order` NUnit; pattern of `[VisualOrder]` |
| CBOR key | `[CborKey(n)]`; alts `[SerializeKey]`, `[Cbor(Key = n)]` | `[SerializeKey(0)]` or `[SerializeKey("hp")]`; alt `[CborKey]` | `Key` MessagePack |
| Enum as number | `EnumAsNumber` on the JSON attribute; alts `[JsonEnumNumber]`, `JsonEnumStyle` | `EnumAsNumber` on the JSON attribute; alts `[EnumAsNumber]`, `[JsonEnumAsNumber]` | — |
| Context root | `[SerializeFor(typeof(X))]`; alts `[CodecFor]`, `[SerializeRoot]` | Consumer-owned context type (no name) | Pattern of `[EnumExtensionsFor]` |
| Custom codec | `[SerializeWith(typeof(C))]`; alts `[SerializeCodec]`, `[CborCodec]`/`[JsonCodec]` | — | — |
| Facades | `CborAPI` / `JsonAPI`; alt `Serializer.ToCbor`/`FromCbor`/`ToJson`/`FromJson` | `Serializing.Cbor.Serializer` / `.Json.Serializer`; alts `.Serialization`, `CborSerializer`, `JsonSerializer` | `ProtoBuf.Serializer`; `JsonSerializer` Newtonsoft and STJ; `JsonConvert`; `JsonSerialization` Unity.Serialization. Types `Cbor`/`Json` impossible beside namespaces of that name (CS0101) |
| CBOR writer/reader | `CborWriter` / `CborReader` | `Formatting.Cbor.Writer` / `Reader`; alt `ValueWriter`/`ValueReader` | System.Formats.Cbor (not in Unity's BCL). Scoped names collide when both formats are imported (CS0104) |
| JSON writer/reader | `Utf8JsonWriter` / `Utf8JsonReader`; alt `JsonTokenWriter`/`JsonTokenReader` | `Formatting.Json.Writer` / `Reader` | `JsonWriter`/`JsonReader` Newtonsoft and Unity.Serialization; `Utf8Json*` System.Text.Json |
| Error enum | `FormattingError`, shared; alts `CborError` + `JsonError`, `CodecError` | `Formatting.ErrorCode`, `None = 0`; alt `EncodingError` | `FormatError`/`ParseError`/`ConversionError` Collections; `Error` Common; `FormattingError` repeats the namespace word (D3.3) |
| Managed exception | `FormattingException` | none (`Try*` pattern) | Localization `FormattingException` (low risk); `JsonException` Newtonsoft |
| Codec witness | `ICborCodec<T>` / `IJsonCodec<T>`; alt `ICodec<T, TWriter, TReader>` | `Serializing.Cbor.ICodec<T>` / `.Json.ICodec<T>`; alt `ISerializer<T>` | Scoped `ICodec<T>` is ambiguous when both are imported |
| Own-type interface | `ICborSerializable` / `IJsonSerializable`; alts `ICborValue`, generated extension method | — | `ISerializable` BCL |
| Generated codec | nested `<Type>.Codec`; roots `<Root>.<X>Codec` | nested `<Type>.CborCodec` / `.JsonCodec`; roots `SaveContext.PlayerStateCborCodec` | Pattern of generated `Id.Serializable` / `Id.TypeConverter`; Latios `Codec` enum (no clash as a nested name) |
| Transcoder | static `Transcoder` with `CborToJson`, `JsonToCbor`, `CborToDiagnostic`; `TranscodeMode`; `[Flags] TranscodeLosses`; alts `CborJson`, `FormatConvert` | `Transcoder` with `TryToJson` / `TryToCbor`; alts `CborJson`, `Transcoding` | — |
| Low-level JSON namespace | `EncosyTower.Formatting.Json` | `EncosyTower.Formatting.Json` | Not named by D2 |

Repository conventions relevant to names:

- `*API` static entry classes: `PathAPI` (`EncosyTower.Core/IO/PathAPI.cs:6`), `SpanAPI`
  (`SystemExtensions/SpanAPI.cs:15`), `GuidAPI` (`SystemExtensions/GuidAPI.cs:6`) (checked). Pass 4 argues against
  adding `API` without a need.
- `Encosy` prefix only where a name would conflict, as on extension classes (`IO/EncosyMemoryStreamExtensions.cs:7-11`);
  owned extensions omit it (`Buffers/BufferNativeExtensions.cs:8`).
- `<Feature>Error` enums (`AddressableKeyError.cs:16`), `None = 0` (`Pooling.Native/NativeRentingError.cs:3-12`).
- Expected failures: `bool Try*` plus an error code (`docs/conventions/CODING-CONVENTIONS.md:1128-1133`).
- Existing conversion terms: `TypeConverter`, `IVariantConverter<T>`; hence `Codec`, not `Converter`.

## Known name conflicts

| Name | Owner | Evidence |
|---|---|---|
| `FormatError`, `ParseError`, `ConversionError` | Unity.Collections (namespace imported by most Burst code) | `Unity.Collections/Unicode.cs:9` |
| `Error` | `EncosyTower.Common.Error` | `EncosyTower.Core/Common/Error.cs:8` |
| `JsonWriter`, `JsonReader` | Newtonsoft (referenced by Core); `Unity.Serialization.Json.JsonWriter` (dependency of Entities) | `JsonWriter.cs:69`; `com.unity.entities@d619bde68c47/package.json:13` |
| `JsonSerializer`, `JsonConvert`, `JsonException`, `JsonIgnore`, `JsonProperty` | Newtonsoft (`JsonSerializer` also System.Text.Json) | Newtonsoft metadata |
| `JsonSerialization`, `DontSerialize` | Unity.Serialization | `Json/JsonSerialization.cs:202`, `Utility/Attributes.cs:86` |
| `Serializer` | `ProtoBuf.Serializer` (public); nested Addressables `Serializer` is not an import clash | protobuf-net source; `ContentCatalogData.cs:1042` |
| `CborReader`, `CborWriter`, `CborConformanceMode` | System.Formats.Cbor (not in Unity's scripting BCL) | .NET API docs |
| `Utf8JsonReader`, `Utf8JsonWriter`, `JsonAttribute` | System.Text.Json (Editor tooling runtimes only, not `MonoBleedingEdge` or `NetStandard`) | .NET API docs; install file search |
| `Ignore`, `Order` | NUnit | NUnit metadata |
| `Key`, `IgnoreMember` | MessagePack (`Key` also DataAnnotations in .NET Framework API profiles) | MessagePack README |
| `Serializable`, `NonSerialized`, `ISerializable` | BCL | `System`, `System.Runtime.Serialization` |
| `FormattingException` | `UnityEngine.Localization.SmartFormat.Core.Formatting` (rarely imported) | `FormattingException.cs:9` |
| `BinaryReader`, `BinaryWriter` | Entities interfaces | `Unity.Entities/Serialization/BinarySerialization.cs:8-15`, `:139` |

Method: pass 3 grepped C# declarations under `Library/PackageCache`, `Packages`, `Assets` and `Plugins` and scanned
DLL metadata strings (`grep -a`); pass 4 checked declared names in `Packages` and `Library/PackageCache` plus official
documentation. DLL-only inventories and compilation with mixed imports are unverified.
