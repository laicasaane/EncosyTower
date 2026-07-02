# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.1.6-preview.4

### Entities.Stats

- Add method `GetStatComponentTypeSet` to `StatAPI`

### SourceGen

- Emit additional markers to help navigating generated code

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
