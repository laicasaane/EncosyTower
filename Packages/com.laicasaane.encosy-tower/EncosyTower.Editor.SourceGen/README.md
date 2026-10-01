# EncosyTower Editor SourceGen

`EncosyTower.Editor.SourceGen` is closed internal infrastructure that captures
the C# files produced by Roslyn source generators during Unity compilation.
Users do not reference this assembly; its assembly definition is
non-auto-referenced and Unity loads it for Editor initialization.

The module asks Unity's compiler host to persist generated files for one clean
compilation. It does not run generators itself or change how they emit source.

Generated-source capture runs only when requested. It is not part of normal C#
compilation.

## Running generated-source capture

Use `Encosy Tower/SourceGen/Output Generated Files`.

The command is available only when Unity is not compiling, updating, entering
Play Mode, or already running or recovering a capture.

Output retention is configured at `Preferences/Encosy Tower/SourceGen`.
`Retain Output` is off by default.

When retention is off, each successful run renews:

```text
<project-root>/Library/EncosyTower/SourceGen/Current/
```

When retention is on, each run writes to a new directory under:

```text
<project-root>/Library/EncosyTower/SourceGen/<unix-milliseconds>/
```

Use `Encosy Tower/SourceGen/Reveal In Finder` to open the SourceGen output root.
The command becomes available after that directory exists.

## What happens during a capture

One capture uses this process:

1. Read the selected build target's complete compiler-argument array.
2. Renew `Current` or create a unique retained output directory, then save recovery state under `Library`.
3. Append `-generatedfilesout:"<capture-directory>"` to the target's compiler
   arguments.
4. Request a clean Unity script compilation.
5. Let Unity run the installed source generators and write their generated C#.
6. Count compiler errors reported for that exact compilation.
7. Restore and verify the original compiler arguments.
8. Request another clean compilation without the capture option.
9. Retain the captured files and report the result.

The capture option is applied through `PlayerSettings` for the selected named
build target. This lets Unity propagate it to normal script assemblies without
creating or changing an `Assets/csc.rsp` file.

Only one capture can run at a time. Unity's opaque compilation context is used
to ensure that unrelated compilation callbacks do not finish the active
capture.

## Captured files

Unity's compiler decides the directories and file names below the capture
directory. SourceGen counts generated `.cs` files recursively after a
successful compilation and reports the total in the Console.

If compilation reports errors, any partial generated files are retained and a
dialog points to their location. Review the Unity Console for the compiler
errors.

Captured files are inspection artifacts. SourceGen does not copy them into
`Assets` or `Packages` or add them to compilation. Renewing `Current` does not
delete retained timestamp directories.

## Restoring compiler settings safely

Compiler arguments are persistent project settings, so SourceGen records the
exact original array before changing them. The recovery state stores:

- its format version;
- the selected build-target group and stable named-target identity;
- the capture directory name;
- every original compiler argument in its original order.

SourceGen verifies the complete argument array after both installation and
restoration. It refuses to start if the target already has a
`generatedfilesout` option or recovery evidence exists.

If a domain reload or Editor restart interrupts a capture, Editor initialization
restores the saved arguments before accepting another run. The generated files
remain available, and Unity requests a clean cleanup compilation.

Malformed, unsupported, or cross-target recovery state is not applied. SourceGen
reports the problem and keeps new captures blocked so the saved settings are not
silently lost or restored to the wrong target.

## Code layout

| File | Purpose |
|---|---|
| [`Settings`](Settings) | Stores and displays per-user SourceGen preferences |
| [`SourceGenCapture.cs`](SourceGenCapture.cs) | Owns menu commands and Unity's compilation lifecycle |
| [`SourceGenCaptureOperation.cs`](SourceGenCaptureOperation.cs) | Prepares output, persists recovery state, and restores compiler arguments |

The module's implementation types are internal. Source generators continue to
use Roslyn's normal `AddSource` API; the Unity compiler host owns physical file
output.
