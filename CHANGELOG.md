# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- `Samples.Data` to `0.1.6-preview.10`

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
- `Samples.Data` to `0.1.6-preview.9`

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
- `Samples.Data` to `0.1.6-preview.8`
 
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
- `Samples.Data` to `0.1.6-preview.7`

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
- `Samples.Data` to `0.1.6-preview.6`

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
- `Samples.Data` to `0.1.6-preview.5`

## 0.1.6-preview.4

### Entities.Stats

- Added method `GetStatComponentTypeSet` to `StatAPI`

### SourceGen

- Emitted additional markers to help navigating generated code

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.4`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.4`
- `Samples.Data` to `0.1.6-preview.4`

## 0.1.6-preview.3

### Contracts

- Added interfaces `IIsValid` and `IIsInitialized`
- Added the interfaces on types that has properties `bool IsValid` or `bool IsInitialized`
- Modified source generators to include the interfaces

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.3`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.3`
- `Samples.Data` to `0.1.6-preview.3`

## 0.1.6-preview.2

### Databases.Authoring
- Fixed: `DatabaseRawSheetImporter` now ignores column path that contains `$` character
- Fixed: `SheetUtility` now correctly validates and sanitizes file names

### Versioning

- `EncosyTower.Formatters` to `0.1.6-preview.2`
- `EncosyTower.SourceGen.*` to `0.1.6-preview.2`
- `Samples.Data` to `0.1.6-preview.2`

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
- `Samples.Data` to `0.1.6-preview.1`
