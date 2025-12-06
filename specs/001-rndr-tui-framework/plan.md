# Implementation Plan: Rndr TUI Framework

**Branch**: `001-rndr-tui-framework` | **Date**: 2025-12-06 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/001-rndr-tui-framework/spec.md`

## Summary

Rndr is a TUI framework for .NET that combines ASP.NET Minimal API patterns with Vue-like single-file components. The implementation follows a phased approach: Phase 1 delivers core runtime with C# views, Phase 2 adds navigation/state/theming, Phase 3 introduces .tui Razor integration, and Phase 4 (stretch) adds tooling.

## Technical Context

**Language/Version**: C# 12+ / .NET 8 (designed for .NET 10 future-proofing)  
**Primary Dependencies**: Microsoft.Extensions.Hosting, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging, Microsoft.Extensions.Options  
**Storage**: N/A (in-memory state via `InMemoryStateStore`)  
**Testing**: xUnit with `Rndr.Testing` helpers, fake `IConsoleAdapter` and `IInputSource`  
**Target Platform**: Cross-platform terminals with ANSI/VT support and UTF-8 encoding  
**Project Type**: Multi-project .NET solution (4 projects)  
**Performance Goals**: <16ms re-render (60fps capable), <500ms startup time  
**Constraints**: AOT-compatible (no reflection), abstracted I/O for testability, no external TUI framework dependencies  
**Scale/Scope**: Framework library supporting single-view MVPs to multi-view production applications

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Minimal API Ergonomics | ✅ PASS | `TuiApplication.CreateBuilder()`, `MapView()`, `RunAsync()` pattern defined |
| II. Vue-like Component Model | ✅ PASS | `.tui` files with `@code` blocks and `Signal<T>` state planned for Phase 3 |
| III. Beautiful by Default | ✅ PASS | Design tokens, Unicode borders, semantic layout primitives specified |
| IV. AOT-Friendly by Design | ✅ PASS | No reflection, Razor source generation for .tui, explicit registrations |
| V. Testability First | ✅ PASS | `IConsoleAdapter`, `IInputSource`, `IClock`, `IEventLoop` abstractions defined |
| VI. Generic Host Integration | ✅ PASS | `TuiAppBuilder` wraps `HostApplicationBuilder`, standard DI/logging/config |
| VII. Observability Ready | ✅ PASS | `ActivitySource` and `Meter` via `System.Diagnostics` (no OTel dependency) |
| VIII. Phased Development | ✅ PASS | 4 phases defined, each produces runnable functionality |

**Gate Result**: ✅ PASS - All constitution principles satisfied

## Project Structure

### Documentation (this feature)

```text
specs/001-rndr-tui-framework/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (internal API contracts)
│   └── layout-api.md
└── checklists/
    └── requirements.md  # Validation checklist
```

### Source Code (repository root)

```text
src/
├── Rndr/                              # Core runtime library
│   ├── Rndr.csproj
│   ├── TuiApplication.cs              # Entry point factory
│   ├── TuiAppBuilder.cs               # Builder wrapping HostApplicationBuilder
│   ├── TuiApp.cs                      # Application instance
│   ├── ViewDefinition.cs              # View configuration
│   ├── ViewContext.cs                 # Per-view context with state/navigation
│   ├── GlobalContext.cs               # Global key handler context
│   ├── Signal.cs                      # Reactive state primitive
│   ├── IStateStore.cs                 # State store interface
│   ├── InMemoryStateStore.cs          # Default state implementation
│   ├── TuiComponentBase.cs            # Base class for .tui components
│   ├── RndrTheme.cs                   # Theme configuration
│   ├── RndrOptions.cs                 # Runtime options
│   ├── IClock.cs                      # Time abstraction
│   ├── Layout/
│   │   ├── Node.cs                    # Base layout node
│   │   ├── NodeKind.cs                # Node type enumeration
│   │   ├── NodeStyle.cs               # Styling properties
│   │   ├── ColumnNode.cs
│   │   ├── RowNode.cs
│   │   ├── PanelNode.cs
│   │   ├── TextNode.cs
│   │   ├── ButtonNode.cs
│   │   ├── TextInputNode.cs
│   │   ├── SpacerNode.cs
│   │   ├── CenteredNode.cs
│   │   ├── LayoutBuilder.cs           # Root layout builder
│   │   ├── ColumnBuilder.cs           # Column child builder
│   │   └── RowBuilder.cs              # Row child builder
│   ├── Rendering/
│   │   ├── ITuiRenderer.cs            # Renderer interface
│   │   ├── IConsoleAdapter.cs         # Console abstraction
│   │   ├── SystemConsoleAdapter.cs    # Real console implementation
│   │   └── ConsoleRenderer.cs         # ANSI/VT renderer
│   ├── Input/
│   │   ├── KeyEvent.cs                # Key event record
│   │   ├── IInputSource.cs            # Input abstraction
│   │   └── SystemInputSource.cs       # Real input implementation
│   ├── Navigation/
│   │   ├── NavigationContext.cs       # Navigation operations
│   │   ├── INavigationState.cs        # Observable navigation state
│   │   └── NavigationState.cs         # Default implementation
│   ├── Diagnostics/
│   │   └── RndrDiagnostics.cs         # ActivitySource and Meter
│   └── Extensions/
│       └── RndrServiceCollectionExtensions.cs
│
├── Rndr.Razor/                        # Razor/.tui integration
│   ├── Rndr.Razor.csproj
│   ├── build/
│   │   └── Rndr.Razor.targets         # MSBuild targets for .tui files
│   └── TuiRazorConfiguration.cs       # Razor engine configuration
│
├── Rndr.Testing/                      # Test helpers
│   ├── Rndr.Testing.csproj
│   ├── RndrTestHost.cs                # Component test helper
│   ├── FakeConsoleAdapter.cs          # Test console
│   ├── FakeInputSource.cs             # Scripted key input
│   └── FakeClock.cs                   # Controllable time
│
└── Rndr.Samples.MyFocusTui/           # Reference sample
    ├── Rndr.Samples.MyFocusTui.csproj
    ├── Program.cs
    ├── Models/
    │   ├── ActionEntry.cs
    │   └── FocusState.cs
    └── Pages/
        ├── Home.tui
        └── Log.tui

tests/
├── Rndr.Tests/
│   ├── Rndr.Tests.csproj
│   ├── SignalTests.cs
│   ├── StateStoreTests.cs
│   ├── NavigationStateTests.cs
│   ├── LayoutBuilderTests.cs
│   └── ConsoleRendererTests.cs
└── Rndr.Razor.Tests/
    ├── Rndr.Razor.Tests.csproj
    └── TuiComponentGenerationTests.cs
```

**Structure Decision**: Multi-project solution following constitution's mandated structure. Core library (`Rndr`) contains all runtime functionality. Razor integration (`Rndr.Razor`) is separate for clean dependency boundaries. Testing helpers (`Rndr.Testing`) enable component testing without terminal. Sample app demonstrates all features.

## Implementation Phases

### Phase 1: MVP - Core Runtime & C# Views

**Goal**: Create working TUI apps with C#-only view definitions

**Deliverables**:
1. `TuiApplication`, `TuiAppBuilder`, `TuiApp` with generic host integration
2. `MapView(string route, Action<ViewDefinition> configure)` for C# views
3. `ViewContext` with `Signal<T>` state (route-scoped only in Phase 1)
4. Layout primitives: Column, Row, Panel, Text, Button, Spacer, Centered
5. `ConsoleRenderer` with basic ANSI output
6. Input loop reading keys and dispatching to buttons/handlers
7. Counter sample view demonstrating core concepts

**Exit Criteria**: Counter app compiles, runs, displays UI, responds to input, quits cleanly

### Phase 2: Navigation, State & Theming

**Goal**: Multi-view apps with shared state and visual polish

**Deliverables**:
1. Navigation stack with `Navigate()`, `Back()`, `Replace()`
2. Global state via `StateGlobal("key", initial)`
3. Global key handlers via `OnGlobalKey()`
4. `RndrTheme` with colors, borders, spacing
5. `INavigationState` for observability
6. Two-view sample (Home + Log) with navigation

**Exit Criteria**: Navigate between views, global state persists, theme applies consistently

### Phase 3: .tui Razor Integration

**Goal**: Vue-like single-file components

**Deliverables**:
1. `.tui` file format with `@view`, `@code`, `@inject`
2. MSBuild targets generating C# from .tui files
3. Tag-to-builder mapping (Column → LayoutBuilder.Column, etc.)
4. `TuiComponentBase` with `Build(LayoutBuilder)` method
5. `MapView(string route, Type componentType)` overload
6. MyFocusTui sample rewritten with .tui files

**Exit Criteria**: .tui files compile to components, sample app runs identically to Phase 2 version

### Phase 4 (Stretch): Tooling & Polish

**Goal**: Developer experience improvements

**Deliverables**:
1. VS Code extension for .tui syntax highlighting
2. Basic Rider plugin scaffolding
3. Hot reload exploration

**Exit Criteria**: .tui files have syntax highlighting in VS Code

## Complexity Tracking

> No violations requiring justification. Design follows constitution principles.

| Aspect | Chosen Approach | Rationale |
|--------|----------------|-----------|
| Renderer | Custom ANSI (no Spectre.Console) | Constitution forbids external TUI deps in core |
| State | Simple Signal<T> with scoped stores | Avoids complexity of full reactivity system |
| .tui Generation | Razor SDK | Proven technology, familiar to .NET devs |
