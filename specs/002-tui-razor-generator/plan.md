# Implementation Plan: .tui Razor Source Generator

**Branch**: `002-tui-razor-generator` | **Date**: 2025-12-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-tui-razor-generator/spec.md`

## Summary

Implement a Razor-based source generator that transforms `.tui` single-file components into C# classes inheriting from `TuiComponentBase`. The generator processes `@view`, `@using`, `@inject`, and `@code` directives, maps XML-like markup tags (`<Column>`, `<Panel>`, `<Button>`, etc.) to `LayoutBuilder` method calls, and produces AOT-compatible code at build time.

## Technical Context

**Language/Version**: C# 12+ / .NET 8  
**Primary Dependencies**: Microsoft.NET.Sdk.Razor, Roslyn Source Generators  
**Storage**: N/A (build-time code generation, no runtime storage)  
**Testing**: xUnit with snapshot tests via RndrTestHost  
**Target Platform**: .NET 8+ (cross-platform terminal apps)  
**Project Type**: Library with MSBuild integration  
**Performance Goals**: Incremental compilation support, sub-second rebuild for single .tui file changes  
**Constraints**: No reflection in generated code (AOT-compatible), accurate error reporting with file/line/column  
**Scale/Scope**: Support 8 tag types, 10+ attributes, unlimited nesting depth

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Minimal API Ergonomics | ✅ Pass | Generated components register via existing `MapView(route, Type)` pattern |
| II. Vue-like Component Model | ✅ Pass | This feature directly enables `.tui` files with `@code` blocks and XML markup |
| III. Beautiful by Default | ✅ Pass | Generated code uses existing themed layout builders |
| IV. AOT-Friendly by Design | ✅ Pass | Source generation at build time produces plain C# - no reflection |
| V. Testability First | ✅ Pass | Generated components testable via `RndrTestHost.BuildComponent<T>()` |
| VI. Generic Host Integration | ✅ Pass | `@inject` directive generates properties resolved via DI |
| VII. Observability Ready | ✅ Pass | Generated code is plain C#; can use `ActivitySource` in `@code` blocks |
| VIII. Phased Development | ✅ Pass | This is Phase 3 as defined in constitution |

**Gate Result**: ✅ PASS - No violations

## Project Structure

### Documentation (this feature)

```text
specs/002-tui-razor-generator/
├── plan.md              # This file
├── research.md          # Phase 0 output - source generator patterns
├── data-model.md        # Phase 1 output - generator entities
├── quickstart.md        # Phase 1 output - developer guide
├── contracts/           # Phase 1 output - API contracts
│   └── generator-api.md
├── checklists/
│   └── requirements.md  # Already complete
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Rndr/                            # Core framework (exists)
│   ├── TuiComponentBase.cs          # Base class for generated components
│   └── Layout/                      # LayoutBuilder APIs
├── Rndr.Razor/                      # Razor integration (to be implemented)
│   ├── Rndr.Razor.csproj            # Source generator project
│   ├── TuiRazorConfiguration.cs     # Tag/attribute mappings (exists)
│   ├── build/
│   │   └── Rndr.Razor.targets       # MSBuild integration (exists, needs update)
│   ├── Generator/                   # NEW: Source generator code
│   │   ├── TuiSourceGenerator.cs    # IIncrementalGenerator implementation
│   │   ├── TuiSyntaxParser.cs       # .tui file parsing
│   │   ├── TuiCodeEmitter.cs        # C# code generation
│   │   └── DiagnosticDescriptors.cs # Error codes and messages
│   └── Parsing/                     # NEW: Markup parsing
│       ├── TuiDocument.cs           # Parsed .tui representation
│       ├── TuiDirective.cs          # @view, @using, @inject, @code
│       └── TuiMarkupNode.cs         # Markup tag tree
└── Rndr.Samples.MyFocusTui/         # Sample app (exists)
    └── Pages/
        ├── Home.tui                 # NEW: Replace HomeComponent.cs
        └── Log.tui                  # NEW: Replace LogComponent.cs

tests/
└── Rndr.Razor.Tests/                # Generator tests (exists, needs expansion)
    ├── TuiComponentGenerationTests.cs  # Existing tests
    ├── ParsingTests.cs              # NEW: Directive/markup parsing tests
    ├── CodeEmitterTests.cs          # NEW: Generated code verification
    └── Snapshots/                   # NEW: Expected output snapshots
```

**Structure Decision**: Extends existing `src/Rndr.Razor/` project with source generator implementation. Creates new `Generator/` and `Parsing/` directories to organize generator code. Sample app gets new `.tui` files alongside (then replacing) existing C# components.

## Complexity Tracking

> No constitution violations requiring justification.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| - | - | - |
