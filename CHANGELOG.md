# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.1.8-preview.1

### General

- Added `docs/conventions/PROJECT-CONVENTIONS.md` and `docs/conventions/COMMIT-CONVENTIONS.md`
- Added Project Auditor and Unity Pipeline packages to the development project
- Added `com.unity.project-auditor` `3.1.1` as a package dependency
- Changed `com.unity.burst` dependency to `1.8.30`
- Changed `.editorconfig` to disallow single-line statements and consecutive blank lines
- Moved `CODING-CONVENTIONS.md` to `docs/conventions/CODING-CONVENTIONS.md`
- Updated the bundled `Bcl.Runtime`, `Bcl.RuntimeUnsafe`, and `Raffinert.FuzzySharp` assemblies
- Removed the `com.annulusgames.unity-codegen` dependency and the `ANNULUS_CODEGEN` symbol

### Core

- Added `UnityTask` and `UnityTask<T>`, a single awaitable type over UniTask or `Awaitable`
- Added `WhenAny`, `WhenEach`, `Delay`, `Yield`, `RunOnThreadPool`, `ContinueWith`, `FromResult`, `FromException`, and `FromCanceled` helpers for `Awaitable`
- Added `SharedArrayNative<T>`, `SharedArraySet<T>`, `SharedArraySetNative<T>`, and `SharedArray<T>.ReadOnly`
- Added `Contains`, `IndexOf`, `BinarySearch`, and `Sort` extensions for shared arrays
- Added Unity serialization support to `ArrayMap`, `ArraySet`, `SharedArray`, `SharedArrayMap`, `SharedArraySet`, `SharedList`, `SharedQueue`, `SharedStack`, and `SharedReference`
- Added `BufferShared<T>` and `BufferShared<T, TNative>`
- Added `ListFast<T>` constructors from capacity, array, `ArraySegment<T>`, `ReadOnlySpan<T>`, and `IEnumerable<T>`
- Added `ListFast<T>.GetBufferUnsafe`, an `IsReadOnly` property on lists, and `EnsureNotNull` for arrays
- Added `ICodeGenerator`, `CodeGeneratorAttribute`, `GeneratedCode`, `ApiMode`, and `StateMode` for writing code generators
- Added a `Container` option to `[PolyEnumStruct]` to support generic enum structs
- Added `TypeFinder` and editor asset loading helpers: `EditorAPI.LoadAsset`, `GetOrLoadAsset`, `LoadStyleSheet`, and `GetOrLoadStyleSheet`
- Added `ValidOrDefault` and `ValidOrInitialize` for Unity objects
- Added `VariantEventHandler`
- Added `ThrowIfNull`, `ThrowIfNullOrEmpty`, and `ThrowIfUnityObjectInvalid` argument checks to public APIs
- Added `DISABLE_ENCOSY_RUNTIME_CHECKS` and `DISABLE_ENCOSY_EDITOR_CHECKS` symbols
- Changed async APIs for addressable keys, resource keys, atlased sprites, scenes, localization, vaults, loaders, and pooling to return `UnityTask` or `UnityTask<T>` (breaking)
- Changed `IStringVault` to extend `IReadOnlyStringVault`, `IDisposable`, `IClearable`, and `IIncreaseCapacity`
- Changed string vault read-only views to implement `IReadOnlyList<UnmanagedString>`
- Changed `CodeGenAPI` to create results with `GetGeneratedCode` (breaking)
- Renamed `HashHelpers.HashPrime` to `HASH_PRIME` (breaking)
- Moved `Awaitables` to the `UnityEngine.Tasks` namespace (breaking)
- Moved Data, Databases, Entities, Jobs, MVVM, Page Flows, Persistence, Processing, and PubSub out of `EncosyTower.Core` into their own assemblies (breaking: add the new assembly references)
- Removed `UnityTasks` and the per-file `UnityTask` aliases, use `UnityTask` instead (breaking)
- Removed `FasterList<T>`, `FasterListPool<T>`, and `ToFasterList`, use `ListFast<T>` instead (breaking)
- Removed `Push`, `Pop`, and `Peek` from `ListFast`, `ListNative`, `ListUnsafe`, and `ListProxy` (breaking)
- Removed `SharedArray.Resize` (breaking)
- Removed the `ListFast(List<T>)` constructor, use `AsListFast()` instead (breaking)
- Removed `CodeGenAPI.GetOutputFolderPathFromCaller` and `CodeGenAPI.TryGetOutputFolderPath` (breaking)

### Data

- Added the `EncosyTower.Data` assembly for `EncosyTower.Data` and `EncosyTower.Databases`, namespaces are unchanged (breaking: add the assembly reference)
- Added `TransposeAttribute` for database authoring

### Databases.Authoring

- Added `NamingMap`
- Changed converters to build on BakingSheet raw sheet importers and converters, with clearer sheet errors
- Renamed the `emptyRowStreakThreshold` parameter to `emptyRowAllowance` (breaking)
- Removed the CSV `splitHeader` option (breaking)

### Databases.Settings

- Renamed the `emptyRowStreakThreshold` setting to `emptyRowAllowance`, saved values reset to the default (breaking)
- Fixed cancelled conversion tasks to skip the asset refresh and clear their progress
- Removed the `splitHeader` setting

### Entities

- Added the `EncosyTower.Entities` assembly for `EncosyTower.Entities` and `EncosyTower.Jobs`, compiled only with `UNITY_ENTITIES` (breaking: add the assembly reference)

### Entities.Stats

- Changed stat generators to use the built-in code generator, `ENCOSY_STAT_VALUE_TYPES_GENERATOR` and UnityCodeGen are no longer needed

### PubSub

- Added the `EncosyTower.PubSub` assembly (breaking: add the assembly reference)
- Added `[PubSub]` to opt message types into source-generated subscribe and publish APIs
- Added `WithScope`, `WithGlobalScope`, and `WithUnityScope` to publishers and subscribers, and `Clear` to subscribers
- Changed PubSub to no longer require `UNITASK` or `UNITY_6000_0_OR_NEWER`
- Fixed stateful subscriptions whose state object was collected, they are now removed and an error is logged

### Processing

- Added the `EncosyTower.Processing` assembly (breaking: add the assembly reference)
- Added `ProcessingContext`, `ProcessingStrategy`, and `[Processing]`
- Added `Processor.UnityScope` and `UnityHub`
- Added `WithScope`, `WithGlobalScope`, `Clear`, and contextual handlers
- Changed `Process`, `TryProcess`, `ProcessAsync`, and `TryProcessAsync` to take a `ProcessingContext` instead of `silent` or `waitForHandler` (breaking)
- Renamed `ProcessHub<TScope>` and `ProcessHub<TScope, TState>` to `Processor.Hub<...>` (breaking)
- Fixed stateful handlers whose state object was collected, they are now removed and an error is logged
- Removed delegate-typed `Unregister` overloads, `ContainsHandler`, and `ContainsAsyncHandler` (breaking)

### PageFlows

- Added the `EncosyTower.PageFlows` assembly (breaking: add the assembly reference)
- Added `IHasCurrentPage<T>`, `IHasPageCollection<T>`, `IHasPages<T>`, and `PageCollection`
- Changed page flow messages to use `[PubSub]`
- Changed list flows so `Pages` returns `ListFast<T>.ReadOnly` (breaking)
- Changed page interfaces to no longer inherit `IPageListStrategy` or `IPageStackStrategy`

### PageFlows.MonoPages

- Added the `EncosyTower.PageFlows.MonoPages` assembly (breaking: add the assembly reference)
- Added `MovedFrom` attributes so serialized MonoPages types survive the assembly move
- Changed requests to use `[Processing]` and `GetPageListRequest` to return `ListFast<IMonoPage>.ReadOnly` (breaking)
- Renamed `*AsyncMessage` types to `*Message`, for example `ShowPageAsyncMessage` to `ShowPageMessage` (breaking)

### Persistence

- Added the `EncosyTower.Persistence` assembly, the namespace is still `EncosyTower.Persistences` (breaking: add the assembly reference)
- Added checks for required `SerializeFunc`, `DeserializeFunc`, `CreateDataFunc`, and `SourceArgs` arguments
- Changed `SetData` to take an `allowNull` parameter (breaking for implementers)
- Renamed `PersistSourceDevice` to `PersistSourceLocal` and `SaveDestination.Device` to `SaveDestination.Local` (breaking)
- Removed `TryCloneDataFromRemote` (breaking)

### Mvvm

- Added `MovedFrom` attributes to view binding types and adapters so serialized data survives the move
- Changed adapter generators to use the built-in code generator
- Moved the core MVVM types from `EncosyTower.Core` into the `EncosyTower.Mvvm` assembly (breaking: add the assembly reference)

### VisualToolkit

- Renamed `EncosyTower.Core.Extended` to `EncosyTower.VisualToolkit` (breaking)
- Renamed the `EncosyTower.VisualDebugging.Commands` namespaces to `EncosyTower.VisualToolkit.Commands` (breaking)

### Editor

- Changed `EncosyMenu.AddMenuItem` to throw `ArgumentNullException` for a null item
- Moved `EditorIcons` to `EncosyTower.Editor.Icons` and `EncosyDebugLogLinkRouter` to `EncosyTower.Editor.Logging` (breaking)
- Renamed `EncosyTower.Editor.VisualDebugging.Commands` to `EncosyTower.Editor.VisualToolkit.Commands` (breaking)
- Fixed generated XML documentation file names to use the assembly name

### Editor.CodeGen

- Added the `EncosyTower.Editor.CodeGen` module to run `[CodeGenerator]` types on demand
- Added `Encosy Tower/CodeGen/Generate in Unity` and `Encosy Tower/CodeGen/Generate in .NET` menu commands
- Added a .NET backend that runs generators in a temporary solution even when unrelated Unity code fails to compile
- Added batch writing that only writes changed files and refreshes Unity once
- Added `Preferences/Encosy Tower/CodeGen` for automatic generation and retained .NET solutions

### Editor.SourceGen

- Added the `EncosyTower.Editor.SourceGen` module to capture Roslyn source generator output
- Added `Encosy Tower/SourceGen/Output Generated Files` and `Reveal In Finder` menu commands
- Added `Preferences/Encosy Tower/SourceGen` with a `Retain Output` option
- Added recovery of the original compiler arguments after an interrupted capture

### Bcl.Extensions

- Changed exposed `List`, `Dictionary`, and `HashSet` wrappers to throw `ArgumentNullException` for null collections

### SourceGen

- Added `EncosyTower.Processing.Generators` with a `[Processing]` request generator and analyzer
- Added `EncosyTower.PubSub.Generators` with a `[PubSub]` message generator and analyzer
- Added support for generic `[PolyEnumStruct]` types through a non-generic container
- Added support for open generic `[PolyEnumFactoryFor]` targets, with arity and constraint diagnostics
- Added validation for horizontal collections in database authoring (`SG_AUTHOR_DATABASE_0100`)
- Added support for user-defined equality on generated database key types (`SG_AUTHOR_DATABASE_0101`)
- Added cancellation support to `Printer` and generated-source helpers
- Changed `EncosyTower.SourceGen.Generators`, `EncosyTower.SourceGen.Analyzers`, and `EncosyTower.SourceGen.CodeRefactors` into per-module `*.Generators` and `*.CodeRefactors` assemblies, each shipped with its own module (breaking)
- Changed generated file names to stable `<Type>.<Role>.<hash>.g.cs` names (breaking)
- Changed generated code to use `g__`-prefixed namespace aliases
- Changed diagnostic IDs to stay contiguous in the `SG_ENUM_TEMPLATE`, `SG_NEWTONSOFT_AOT_HELPER`, `SG_PERSISTENCE`, `SG_POLY_ENUM_FACTORY`, `SG_POLY_ENUM_STRUCT`, `SG_DATABASE_TABLE`, and `SG_DATABASE_HORIZONTAL` families (breaking: update `#pragma` and `.editorconfig` suppressions)
- Moved data helpers into `EncosyTower.SourceGen.Data.Helpers` and variant helpers into `EncosyTower.SourceGen.Helpers.Variants`
- Fixed `SG_ISYSTEM_0001` being reported on `ISystem` types that already have the full partial contract
- Fixed `[NewtonsoftJsonAotHelper]` and `[PersistAccessor]` not rejecting declared generic types
- Fixed forwarded attributes dropping the names of named arguments
- Removed the `sourcegen-output-path` option and the `#line` directives pointing to `Temp/GeneratedCode` (breaking)
- Removed the `*_UNKNOWN_0001` diagnostics, generator exceptions now surface as compiler warnings
- Removed unused enum template and database table diagnostics
- Removed diagnostics that rejected generic `[PolyEnumStruct]` types and generic `[PolyEnumFactoryFor]` targets
- Rebuilt all source generators for `0.1.8-preview.1`

### Tests

- Added EditorMode coverage for `Editor.CodeGen`, `Editor.SourceGen`, PubSub, Processing, `PersistStoreDefault`, database authoring, database conversion tasks, and the MonoPageFlow settings editor
- Added coverage for shared arrays and sets, collection serialization, `BufferShared`, `TypeFinder`, `UnityTask`, `Awaitables`, and string values in `Variant`
- Added SourceGen tests for the Processing and PubSub generators, verified snapshots, and diagnostic contracts
- Changed test assemblies to compile only when `ENCOSY_TESTS_ENABLED` is defined
- Changed `EncosyTower.Tests.EditorMode` to no longer be auto-referenced
- Changed SourceGen tests to follow the per-module layout
- Removed `FasterList` tests

### Samples

- Added a BakingSheet features table to the data sample
- Changed samples to use the new module assemblies, `UnityTask`, `[PubSub]`, and `[Processing]`
- Renamed the `EncosyTower.Samples.VisualDebugging` sample to `EncosyTower.Samples.VisualToolkit`
- Updated sample paths for `0.1.8-preview.1`

### Versioning

- `EncosyTower.SourceGen.*` to `0.1.8-preview.1`
- Package and sample references to `0.1.8-preview.1`

## 0.1.7-preview.3

### Core

- Added custom allocator support to `ArrayUnsafe<T>`
- Changed custom allocation handling to use allocator strategies for allocation and disposal
- Fixed custom allocator resizing to preserve existing data
- Fixed zero-length `ArrayUnsafe<T>` creation and disposal

### Tests

- Added EditorMode coverage for Entities world-update allocators across buffers and native and unsafe collections
- Added `ArrayUnsafe<T>` coverage for custom allocator handles, zero-length arrays, and invalid allocator strategies
- Changed Core EditorMode test namespaces to follow the test directory layout

### SourceGen

- Rebuilt all source generators for `0.1.7-preview.3`

### Samples

- Added an interactive Processing sample
- Updated data sample paths for `0.1.7-preview.3`

### Versioning

- `EncosyTower.Formatters` to `0.1.7-preview.3`
- `EncosyTower.SourceGen.*` to `0.1.7-preview.3`
- Package and sample references to `0.1.7-preview.3`

## 0.1.7-preview.2

### General

- Updated coding conventions for centralized validation symbols and collection exception helpers

### Core

- Added centralized validation symbols for global, collections, PubSub, processing, and stats runtime checks
- Added `EncosyTower.Collections.ThrowHelper` for collection and buffer validation
- Replaced file-local validation guard attributes with centralized global and namespace-specific symbols
- Moved collection and buffer exception helpers from `EncosyTower.Debugging` to `EncosyTower.Collections`
- Updated managed collection cleanup checks to use Encosy unmanaged type detection
- Limited `EncosyTower.Debugging.ThrowHelper` to generic exception factories
- Removed redundant file-local validation define blocks from conditional guard call sites
- Removed redundant unmanaged type validation from `BufferNative<T>`

### SourceGen

- Updated database, stats, persistence, poly-enum, and union ID output to use centralized validation symbols
- Rebuilt all source generators for `0.1.7-preview.2`
- Fixed generated persistence code to reference collection exception helpers from `EncosyTower.Collections`
- Removed generated file-local validation define blocks

### Entities.Stats

- Updated runtime and generated validation to use global and stats-specific symbols

### Mvvm

- Updated view binding validation to use centralized runtime symbols

### Tests

- Updated validation tests for centralized symbols and collection exception helpers

### Samples

- Updated the data preset and persistence sample for `0.1.7-preview.2`

### Versioning

- `EncosyTower.Formatters` to `0.1.7-preview.2`
- `EncosyTower.SourceGen.*` to `0.1.7-preview.2`
- Package and sample references to `0.1.7-preview.2`

## 0.1.7-preview.1

### General

- Added `docs/conventions/CODING-CONVENTIONS.md` to document the project coding conventions

### Core

- Added `BufferManaged`, `BufferNative`, and `BufferUnsafe` along with `AllocatorStrategy`
- Added native and unsafe variants of list, queue, stack, and reference collections (`ListNative`, `QueueNative`, `StackNative`, `ReferenceNative`, and their unsafe counterparts)
- Added shared collection types: `SharedQueue`, `SharedStack`, `SharedQueueNative`, and `SharedStackNative`
- Added read-only and unsafe extension methods for the new and existing collection types
- Added indexer contracts for collection types: `IIndexer<T>`, `IReadOnlyIndexer<T>`, `IRefIndexer<T>`, and `IRefReadOnlyIndexer<T>`
- Added `ArrayUnsafe`, `ReferenceUnsafe`, and `EncosyMemoryExtensions`
- Added extension methods for `ArraySetNative` types
- Added `StringVaultUnsafe` and the `IStringVault` and `IReadOnlyStringVault` interfaces covering both managed and native string vaults
- Added the `ENCOSY_CLEAR_GLOBAL_STRING_VAULT_ON_ENTER_PLAY_MODE` compilation symbol to clear `GlobalStringVault` on entering Play Mode
- Added an internal constructor for `NativeSliceReadOnly`
- Renamed `NativeStringVault` to `StringVaultNative` and its `TryGetString` API to `TryGetUnmanagedString`
- Renamed `StatelessList` to `ListProxy`, reimplemented it with missing functions, and replaced `ProxiedList` and `IListProxy` with it
- Unified buffer implementations: merged the strategy types into the buffers, then renamed `ManagedBuffer` and `NativeBuffer` to `BufferManaged` and `BufferNative`
- Updated validation checks across runtime code to use shared settings via `ThrowHelper`
- Marked overlapping fields and pointer operations with safe and unsafe guidance for future updates
- Moved built-in `JsonArrayMap` support to the persistence sample
- Fixed incorrect extension methods for `SharedList` types
- Fixed the implementation of unsafe exposed types in the Bcl.Extensions plugins
- Removed `FixedArray` and `FixedHashMap`
- Removed the buffer strategy types: `IBufferStrategy`, `ManagedStrategy`, `NativeStrategy`, and `BufferBase`

### SourceGen

- Updated generated code to conform to the "Unsafe Evolution" direction of future C#
- Rebuilt all source generators for `0.1.7-preview.1`
- Fixed compilation symbol defines at the top of generated files
- Fixed a wrong type reference: `HashSetAPI` should be `EncosyHashSetExtenions`

### Entities.Stats

- Updated validation checks to use shared settings and generated overlapping-field guidance for `StatVariant`

### Tests

- Added EditorMode coverage for buffers, collections, extensions, and validation helpers
- Allowed the test assembly to inspect `EncosyTower.Core` internals

### Samples

- Updated samples
- Moved `JsonArrayMap` support to the persistence sample

### Versioning

- `EncosyTower.Formatters` to `0.1.7-preview.1`
- `EncosyTower.SourceGen.*` to `0.1.7-preview.1`
- Package and sample references to `0.1.7-preview.1`

## 0.1.6-preview.10

### Core

- Added parameter for customizing AES iterations
- Added overloads with `count` param to WhenAll tasks
- Fixed incorrect ByteBool conversion
- Fixed incorrect Dispose method for StringVaults
- Fixed incorrect string collisions handling in StringVaults
- Fixed incorrect implementation of Awaitables.Completed.Awaitable
- Fixed incorrect empty separator handling for SpanSplitExtensions.Split overloads
- Fixed missing invocation of _actionOnReturn in SimpleConcurrentPool
- Fixed missing character W in RandomStringGenerator.CHARSET
- Fixed incorrect range checks for Insert and FindIndex
- Fixed incorrect hash calculation for managed string in StringVault
- Fixed incorrect method call syntax in SharedArray & SharedReference
- Fixed leak in NativeStrategy<T>.Dispose
- Fixed incorrect range check for CopyFromSpan & CopyToSpan
- Fixed incorrect implementation of DateTimeId.IsValid
- Fixed incorrect implementation of MessageBroker<T>.PublishAsync
- Fixed incorrect return value for CachedPublisher`2.Validate
- Fixed incorrect implementation for IndexOf and Remove in StatelessList
- Fixed incorrect implementation of EncosyNativeArrayExtensionsUnsafe.MemoryCopyUnsafe
- Fixed incorrect implementation of NB<T>.Clear
- Fixed wrong implementation of equality for Result type
- Prevented possible null exception for Option.Equals<T> when T is reference type
- Removed implicit conversion of decimal to Variant

### Entities.Stats

- Fixed an incorrection that makes Assert always fails for TryUpdateStatAssumeSingleEntity

### Samples

- Updated samples

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.10`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.10`
- Package and sample references to `0.1.6-preview.10`

## 0.1.6-preview.9

### Core

- ByteBool types are now serializable
- ByteBool types now implement IFixedString and IFixedString<T>

### SourceGen

- Entities.Stats: StatDataStore now stores ByteBool types

### Samples

- Updated samples

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.9`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.9`
- Package and sample references to `0.1.6-preview.9`

## 0.1.6-preview.8

### Core

- Added ByteBool variants for bool2, bool2x2,... bool4x4 in Unity Mathematics

### SourceGen

- Entities.Stats: repaced bools with ByteBool variants to fix Burst error BC1063

### Samples

- Updated samples

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.8`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.8`
- Package and sample references to `0.1.6-preview.8`

## 0.1.6-preview.7

### Core

- Variants: Added CanStore<T> method to VariantConverter

### SourceGen

- Variants: Replaced compiled time condition with VariantConverter.CanStore<T>
- PolyEnumStructs: Improved algorithm to calculate struct size

### Samples

- Updated samples

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.7`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.7`
- Package and sample references to `0.1.6-preview.7`

## 0.1.6-preview.6

### Entities.Stats

- Fixed multiple bugs

### SourceGen

- Fixed missing generated API for Entities.Stats
- Fixed wrong calculation of type size. Now corrected with field alignments.

### Samples

- Updated samples

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.6`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.6`
- Package and sample references to `0.1.6-preview.6`

## 0.1.6-preview.5

### Entities.Stats

- Fixed multiple bugs

### SourceGen

- Updated code emission for `StatSystemSpec+WriteCode`

### Samples

- Updated sample for Stats

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.5`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.5`
- Package and sample references to `0.1.6-preview.5`

## 0.1.6-preview.4

### Entities.Stats

- Added method `GetStatComponentTypeSet` to `StatAPI`

### SourceGen

- Emitted additional markers to help navigating generated code

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.4`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.4`
- Package and sample references to `0.1.6-preview.4`

## 0.1.6-preview.3

### Contracts

- Added interfaces `IIsValid` and `IIsInitialized`
- Added the interfaces on types that has properties `bool IsValid` or `bool IsInitialized`
- Modified source generators to include the interfaces

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.3`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.3`
- Package and sample references to `0.1.6-preview.3`

## 0.1.6-preview.2

### Databases.Authoring
- Fixed: `DatabaseRawSheetImporter` now ignores column path that contains `$` character
- Fixed: `SheetUtility` now correctly validates and sanitizes file names

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.2`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.2`
- Package and sample references to `0.1.6-preview.2`

## 0.1.6-preview.1

### General

- Moved the development of this package over here from [Tower of Encosy](https://github.com/laicasaane/tower_of_encosy/)
- Added a "Sign and release" CI to support [UPM Signing](https://docs.unity3d.com/6000.3/Documentation/Manual/cus-export.html)

### Breaking changes

- Rebranded `UserDataVault` to `Persistence`
  - Performed multiple renaming on related APIs

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.1`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.1`
- Package and sample references to `0.1.6-preview.1`
