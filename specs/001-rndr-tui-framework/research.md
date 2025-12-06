# Research: Rndr TUI Framework

**Feature**: 001-rndr-tui-framework  
**Date**: 2025-12-06

## Overview

This document consolidates research findings for implementing the Rndr TUI framework. All "NEEDS CLARIFICATION" items from the technical context have been resolved through research and design decisions.

---

## Decision 1: Console Rendering Strategy

**Question**: How should Rndr render to the terminal without external TUI frameworks?

**Decision**: Custom ANSI/VT escape sequence renderer with double-buffered output

**Rationale**:
- Constitution forbids Spectre.Console, Terminal.Gui, and similar in core
- ANSI/VT sequences are universally supported in modern terminals
- Double-buffering minimizes flicker by computing full frame before writing
- Custom implementation gives full control over visual appearance

**Alternatives Considered**:
- **Spectre.Console**: Rejected - forbidden by constitution, adds external dependency
- **Terminal.Gui**: Rejected - too heavyweight, widget-focused rather than declarative
- **Raw Console.Write**: Rejected - no ANSI support, platform-specific behavior

**Implementation Notes**:
```csharp
// Key escape sequences needed:
"\x1b[2J"       // Clear screen
"\x1b[H"        // Move cursor to home
"\x1b[{row};{col}H"  // Move cursor to position
"\x1b[?25l"     // Hide cursor
"\x1b[?25h"     // Show cursor
"\x1b[{n}m"     // SGR (color/style)
```

---

## Decision 2: State Management Pattern

**Question**: How should reactive state work without a full reactivity framework?

**Decision**: Simple `Signal<T>` wrapper with change detection and re-render callback

**Rationale**:
- Vue-like `Signal<T>` with `.Value` property is familiar and minimal
- Change detection via `EqualityComparer<T>.Default.Equals` avoids unnecessary re-renders
- Single `Action onChanged` callback triggers re-render request
- Scoped storage (`IStateStore`) separates route-scoped from global state

**Alternatives Considered**:
- **Full Observable pattern (INotifyPropertyChanged)**: Rejected - overkill for TUI, verbose
- **Immutable state with reducers**: Rejected - too complex for target audience
- **No reactivity (manual refresh)**: Rejected - poor developer experience

**Implementation Notes**:
- State is stored in `Dictionary<(string Scope, string Key), ISignal>`
- Route-scoped: scopeKey = current route path
- Global: scopeKey = "global"
- On change: Signal invokes callback → EventLoop marks dirty → next tick re-renders

---

## Decision 3: Layout Algorithm

**Question**: How should layout computation work for Column/Row/Panel primitives?

**Decision**: Simple constraint-based layout with single-pass measurement

**Rationale**:
- TUI layouts are fundamentally simpler than GUI (character cells, not pixels)
- Column: children stack vertically, each takes full width
- Row: children share width equally (or proportionally with weights)
- Panel: adds border characters, insets content by 1
- Single-pass measurement is fast and predictable

**Alternatives Considered**:
- **Flexbox-like system**: Rejected for MVP - adds complexity, can be added later
- **CSS-like constraint solver**: Rejected - overkill for terminal grids
- **Absolute positioning only**: Rejected - defeats purpose of semantic layout

**Implementation Notes**:
```
LayoutContext {
    AvailableWidth: int
    AvailableHeight: int
    OffsetX: int
    OffsetY: int
}

Each node type implements:
- Measure(ctx) → (width, height)
- Arrange(ctx, width, height) → void (positions children)
```

---

## Decision 4: Razor Integration for .tui Files

**Question**: How should .tui files compile to C# components?

**Decision**: Use Razor SDK with custom directives and tag helpers

**Rationale**:
- Razor is proven technology for C# code generation from markup
- `@view` directive maps to class inheritance from `TuiComponentBase`
- `@code` block becomes the class body
- Tags (`<Column>`, `<Panel>`, etc.) map to builder method calls
- MSBuild integration via .targets file

**Alternatives Considered**:
- **Custom parser + Roslyn generator**: Rejected - significant implementation effort
- **T4 templates**: Rejected - poor IDE support, unfamiliar syntax
- **Blazor components directly**: Rejected - wrong rendering target

**Implementation Notes**:
- .tui files treated as Razor files with custom configuration
- Generated class inherits `TuiComponentBase`
- `Build(LayoutBuilder layout)` method generated from markup
- Tag attributes map to builder method parameters

---

## Decision 5: Input Handling Model

**Question**: How should keyboard input flow through the system?

**Decision**: Event bubbling with global handlers first, then view handlers, then focused element

**Rationale**:
- Global keys (Q to quit, H for home) should work everywhere
- View-specific keys handled if global doesn't consume
- Button focus allows Enter/Space activation
- Simple model avoids complex event propagation

**Alternatives Considered**:
- **Capture phase (top-down)**: Rejected - less intuitive for simple cases
- **Focus-only handling**: Rejected - global shortcuts would require view cooperation
- **Command pattern with bindings**: Rejected - overkill for keyboard TUI

**Event Flow**:
1. Read `ConsoleKeyInfo` via `IInputSource`
2. Create `KeyEvent` record
3. Call global handlers in order; if any returns `true`, stop
4. Call current view's `OnKey` handler; if returns `true`, stop
5. Pass to focused element (button click on Enter/Space)

---

## Decision 6: Generic Host Integration

**Question**: How should Rndr integrate with Microsoft.Extensions.Hosting?

**Decision**: `TuiAppBuilder` wraps `HostApplicationBuilder`, `TuiApp` holds `IHost`

**Rationale**:
- Generic Host is the standard .NET application model
- Provides `IConfiguration`, `ILogger<T>`, `IOptions<T>` for free
- Enables side-by-side hosting with ASP.NET, workers, etc.
- DI container available for components

**Implementation Notes**:
```csharp
public static class TuiApplication
{
    public static TuiAppBuilder CreateBuilder(string[] args)
        => new(new HostApplicationBuilder(args));
}

public sealed class TuiAppBuilder
{
    public HostApplicationBuilder HostBuilder { get; }
    public IServiceCollection Services => HostBuilder.Services;
    
    public TuiApp Build() => new(HostBuilder.Build());
}
```

---

## Decision 7: Theming System

**Question**: How should themes be structured and applied?

**Decision**: Single `RndrTheme` class with design tokens, registered as singleton

**Rationale**:
- Design tokens (colors, borders, spacing) provide consistency
- Single theme object avoids complex inheritance
- DI registration allows custom themes
- Renderer reads theme during rendering

**Theme Tokens**:
```csharp
public sealed class RndrTheme
{
    // Colors
    public ConsoleColor AccentColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor TextColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor MutedTextColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
    
    // Borders
    public PanelTheme Panel { get; } = new();
    
    // Spacing
    public int SpacingUnit { get; set; } = 1;
}

public enum BorderStyle { Square, Rounded }
```

---

## Decision 8: Testing Strategy

**Question**: How should components be tested without a real terminal?

**Decision**: Abstract I/O interfaces + `RndrTestHost` helper

**Rationale**:
- `IConsoleAdapter` and `IInputSource` enable fake implementations
- `RndrTestHost.BuildComponent<T>()` instantiates component in isolation
- Tests can inspect node tree structure
- Tests can script key inputs and verify state changes

**Test Infrastructure**:
- `FakeConsoleAdapter`: Records all write calls, configurable size
- `FakeInputSource`: Queue of `KeyEvent` to return
- `FakeClock`: Controllable `DateTime.Now`
- `TestEventLoop`: Runs N frames or until condition met

---

## Decision 9: AOT Compatibility

**Question**: How do we ensure Native AOT works?

**Decision**: No reflection in hot paths, explicit service registration, source-generated .tui

**Rationale**:
- Razor generates concrete C# classes at build time
- No `Type.GetType()` or assembly scanning
- DI uses explicit `AddSingleton<T>()` registrations
- `IOptions<T>` pattern is AOT-friendly

**Verification**:
- CI should run `dotnet publish -c Release` with AOT enabled
- Sample app must publish without trim warnings
- Add `[DynamicallyAccessedMembers]` attributes where needed

---

## Decision 10: Diagnostics Integration

**Question**: How should tracing and metrics work?

**Decision**: System.Diagnostics APIs only, no OpenTelemetry package dependency

**Rationale**:
- `ActivitySource` and `Meter` are built into .NET
- OTel can subscribe to these without framework dependency
- Users configure OTel in their host, Rndr just emits

**Instrumentation Points**:
```csharp
public static class RndrDiagnostics
{
    public static readonly ActivitySource ActivitySource = new("Rndr.Core");
    public static readonly Meter Meter = new("Rndr.Core");
    
    // Activities: Navigate, RenderFrame, KeyEvent
    // Metrics: rndr.frames_rendered (counter), rndr.render_duration_ms (histogram)
}
```

---

## Summary

All technical decisions have been made. The design:
- Uses custom ANSI renderer for full control
- Implements simple Signal-based reactivity
- Leverages Razor SDK for .tui compilation
- Integrates fully with Generic Host
- Abstracts all I/O for testability
- Supports AOT without reflection
- Provides observability via System.Diagnostics

Ready to proceed to data model and API contracts.

