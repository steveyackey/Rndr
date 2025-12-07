# Implementation Plan: Examples Reorganization and Focus List

**Branch**: `003-examples-focus-list` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-examples-focus-list/spec.md`
**Scope change (2025-12-07)**: Focus list enhancements cut; scope now limited to moving and renaming MyFocusTui.

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Reorganize the project structure by moving `Rndr.Samples.MyFocusTui` from `src/` to a new top-level `examples/` directory and renaming it to `MyFocusTui`. Focus list enhancements were cut from scope; current work focuses solely on relocation/rename and ensuring the sample still builds and runs.

## Technical Context

**Language/Version**: C# 12+ / .NET 8+  
**Primary Dependencies**: Rndr (core framework), Rndr.Razor (Razor/.tui integration), Microsoft.Extensions.Hosting, Microsoft.Extensions.DependencyInjection  
**Storage**: In-memory state via `StateGlobal()` - no persistent storage required  
**Testing**: xUnit (existing test infrastructure), manual testing for UI functionality  
**Target Platform**: ANSI/VT-compatible terminals with UTF-8 support (cross-platform: Windows, macOS, Linux)  
**Project Type**: Single executable application (sample/demo project)  
**Performance Goals**: Responsive UI updates (<100ms render time), smooth navigation transitions  
**Constraints**: Must maintain existing functionality after move/rename, must compile with AOT-friendly patterns, solution file must be updated correctly  
**Scale/Scope**: Single sample application with 2 views (Home, Log); focus list view descoped; ~3 model classes, minimal state management

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Minimal API Ergonomics
✅ **PASS**: No changes to public API. Sample app uses existing `TuiApplication.CreateBuilder()`, `MapView()`, `RunAsync()` patterns.

### II. Vue-like Component Model
✅ **PASS**: Uses existing `.tui` files with declarative markup. New FocusList view will follow same pattern.

### III. Beautiful by Default
✅ **PASS**: Uses existing layout primitives (Column, Row, Panel, Text, Button). No custom styling required.

### IV. AOT-Friendly by Design
✅ **PASS**: No reflection-based discovery. All code uses existing Razor source generation. Project move/rename doesn't introduce new AOT concerns.

### V. Testability First
✅ **PASS**: No new I/O abstractions needed. Existing `RndrTestHost` can test new view if needed. Focus list functionality uses existing state management.

### VI. Generic Host Integration
✅ **PASS**: No changes to host integration. Sample app continues using existing `TuiAppBuilder` wrapping `HostApplicationBuilder`.

### VII. Observability Ready
✅ **PASS**: No changes to observability. Uses existing `ActivitySource` and `Meter` from framework.

### VIII. Phased Development
✅ **PASS**: This is a reorganization and enhancement task, not a new phase. Changes are incremental and don't block other work.

**Gate Status (Pre-Phase 0)**: ✅ **ALL GATES PASS** - No violations detected. This is a straightforward reorganization and feature enhancement that aligns with all constitution principles.

### Post-Phase 1 Re-evaluation

After completing Phase 1 design (data model, contracts, quickstart):

**Gate Status (Post-Phase 1)**: ✅ **ALL GATES PASS** - Design artifacts confirm no violations:

- **FocusList view design** uses existing `.tui` component pattern, no new framework APIs required
- **State management** leverages existing `StateGlobal()` mechanism, no new abstractions
- **Navigation** uses standard `MapView()` and `NavigationContext`, no new routing features
- **Layout** uses existing primitives (Column, Row, Panel, Text, Button), no custom components
- **All design decisions** align with constitution principles and existing framework capabilities

**Conclusion**: Design phase confirms initial assessment. Implementation can proceed without constitution concerns.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Rndr/                          # Core runtime library (unchanged)
├── Rndr.Razor/                    # Razor/.tui integration (unchanged)
└── Rndr.Testing/                  # Test helpers (unchanged)

examples/                          # NEW: Top-level examples directory
└── MyFocusTui/                    # MOVED & RENAMED from src/Rndr.Samples.MyFocusTui
    ├── MyFocusTui.csproj          # RENAMED from Rndr.Samples.MyFocusTui.csproj
    ├── Program.cs                 # Updated namespace references
    ├── Models/
    │   ├── ActionEntry.cs         # Updated namespace
    │   └── FocusState.cs          # Updated namespace
    └── Pages/
        ├── Home.tui               # Updated namespace in @using
        └── Log.tui                # Updated namespace in @using

tests/
├── Rndr.Tests/                    # (unchanged)
└── Rndr.Razor.Tests/              # (unchanged)

Rndr.sln                           # UPDATED: Project reference path changed
```

**Structure Decision**: Reorganization of existing sample application. Moving from `src/Rndr.Samples.MyFocusTui` to `examples/MyFocusTui` to clearly separate example code from core library code. This aligns with common .NET project conventions where examples are kept separate from source libraries. The structure maintains all existing functionality while improving discoverability and project organization.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
