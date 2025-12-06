<!--
  SYNC IMPACT REPORT
  ==================
  Version change: 0.0.0 → 1.0.0 (initial ratification)
  
  Modified principles: N/A (initial version)
  
  Added sections:
    - Core Principles (8 principles)
    - Technical Constraints
    - Development Workflow
    - Governance
  
  Removed sections: N/A (initial version)
  
  Templates requiring updates:
    ✅ plan-template.md - Constitution Check section compatible
    ✅ spec-template.md - User stories align with phased approach
    ✅ tasks-template.md - Phase structure matches constitution phases
  
  Follow-up TODOs: None
-->

# Rndr Constitution

## Core Principles

### I. Minimal API Ergonomics

The public API MUST feel like ASP.NET Minimal APIs at the application level:

- Entry point pattern: `TuiApplication.CreateBuilder(args)`, `Build()`, `MapView()`, `RunAsync()`
- Route-based view registration via `MapView(string route, ...)`
- Fluent builder patterns for configuration
- Extension method conventions for service registration

**Rationale**: .NET developers familiar with ASP.NET should feel immediately at home. This reduces
learning curve and leverages existing ecosystem knowledge.

### II. Vue-like Component Model

View authoring MUST feel like Vue single-file components:

- `.tui` files with declarative XML-like markup (`<Column>`, `<Row>`, `<Panel>`, `<Text>`, `<Button>`)
- `@code { ... }` blocks for component logic
- Reactive state via `State("key", initialValue)` returning `Signal<T>`
- Props passed as XML attributes with Razor expression support (`@variable`)

**Rationale**: Vue's SFC pattern is proven for component-based UI. Declarative markup with
co-located logic enables rapid development and clear separation of concerns.

### III. Beautiful by Default

Visual output MUST be polished and consistent without custom styling:

- Semantic layout primitives (Column, Row, Panel, Spacer, Centered) over raw positioning
- Design tokens for theming (AccentColor, TextColor, BorderStyle, SpacingUnit)
- Unicode box-drawing characters for panels and borders
- Consistent spacing and alignment across all components

**Rationale**: Most TUI frameworks produce ugly output by default. Rndr MUST differentiate by
making beautiful the path of least resistance.

### IV. AOT-Friendly by Design

The framework MUST be fully compatible with Native AOT and trimming:

- NO reflection-based discovery in core runtime paths
- `.tui` files compile to plain C# classes via Razor source generation
- All service registrations MUST be explicit (no assembly scanning)
- Type information preserved through generic constraints, not runtime reflection

**Rationale**: Modern .NET emphasizes AOT compilation for performance and deployment simplicity.
Rndr MUST not be the component that breaks AOT compatibility.

### V. Testability First

All I/O and external dependencies MUST be abstracted for testability:

- `IConsoleAdapter` for terminal output (not direct `Console` calls)
- `IInputSource` for key input
- `IClock` for time-dependent operations
- `IEventLoop` for the main run loop
- `TuiComponentBase.Build()` MUST be pure (no side effects, no I/O)
- `RndrTestHost` helper for component testing without a real terminal

**Rationale**: Untestable UI frameworks lead to untested applications. First-class test support
enables TDD and ensures component correctness.

### VI. Generic Host Integration

The framework MUST integrate with the .NET Generic Host:

- `TuiAppBuilder` wraps `HostApplicationBuilder`
- Standard `IServiceCollection` for dependency injection
- `IConfiguration`, `ILogger<T>`, `IOptions<T>` available throughout
- `IHostApplicationLifetime` for graceful shutdown
- Compatible with side-by-side hosting (ASP.NET, workers, etc.)

**Rationale**: The Generic Host is the foundation of modern .NET applications. Integration
enables use of the entire .NET ecosystem (logging, configuration, health checks, etc.).

### VII. Observability Ready

The framework MUST support observability without hard dependencies:

- `ActivitySource` for distributed tracing (navigation, render cycles, key events)
- `Meter` for metrics (frames rendered, render duration)
- Use `System.Diagnostics` APIs only (no OpenTelemetry package dependency)
- Document how users wire up OTel exporters via standard host configuration

**Rationale**: Production applications need observability. Using System.Diagnostics allows
opt-in OTel integration without forcing the dependency on all users.

### VIII. Phased Development

Implementation MUST follow incremental phases with working software at each milestone:

- **Phase 1 (MVP)**: Core runtime, C# views, basic layout, simple renderer, input loop
- **Phase 2**: Navigation, shared state, global keys, theming
- **Phase 3**: `.tui` Razor integration (Vue-like SFCs)
- **Phase 4 (stretch)**: Tooling (VS Code extension, hot reload)

Each phase MUST produce runnable, demonstrable functionality. Phase N MUST NOT block
Phase N-1 from being useful.

**Rationale**: Waterfall delivery delays feedback. Phased delivery enables early validation
and course correction.

## Technical Constraints

### Target Runtime & Language

- **Runtime**: .NET 8+ (design for .NET 10 future-proofing)
- **Language**: C# 12+ (records, primary constructors, collection expressions)
- **Terminal**: ANSI/VT-compatible terminals with UTF-8 support

### Project Structure

The solution MUST maintain this structure:

```
Rndr/                          # Core runtime library
Rndr.Razor/                    # Razor/.tui integration and code generation
Rndr.Testing/                  # Test helpers (RndrTestHost)
Rndr.Samples.MyFocusTui/       # Reference sample application
```

### Namespace Conventions

- `Rndr` - Core types (TuiApplication, TuiApp, Signal, ViewContext)
- `Rndr.Layout` - Layout primitives and node tree
- `Rndr.Rendering` - Renderer interfaces and implementations
- `Rndr.Input` - Input events and sources
- `Rndr.Navigation` - Navigation context and state
- `Rndr.Diagnostics` - ActivitySource, Meter definitions

### API Design Rules

1. Prefer extension methods for optional functionality
2. Use `Action<T>` callbacks for builder configuration (not return-based fluent)
3. Expose `IServiceCollection` and `IConfiguration` for user customization
4. Mark internal implementation details with `internal` visibility
5. Use `sealed` on classes not designed for inheritance

### Dependencies

- **Allowed**: Microsoft.Extensions.* packages, System.Diagnostics.*
- **Forbidden in Core**: OpenTelemetry packages, Spectre.Console, Terminal.Gui
- **Samples Only**: May reference additional packages for demonstration

## Development Workflow

### Code Quality Gates

All code MUST pass these gates before merge:

1. **Build**: `dotnet build` succeeds with no warnings (TreatWarningsAsErrors)
2. **Tests**: All unit and integration tests pass
3. **Format**: Code formatted per `.editorconfig` rules
4. **AOT Check**: `dotnet publish -c Release` with AOT enabled produces no trim warnings

### Test Requirements

| Component Type | Required Tests |
|---------------|----------------|
| Signal/State | Unit tests for reactivity, equality, scope isolation |
| Navigation | Unit tests for stack operations, route matching |
| Layout Builders | Unit tests for node tree construction |
| Renderer | Integration tests with fake IConsoleAdapter |
| .tui Components | Snapshot tests via RndrTestHost |

### Commit Message Format

```
<type>(<scope>): <description>

Types: feat, fix, refactor, test, docs, chore
Scopes: core, layout, render, input, nav, razor, samples
```

### Branch Strategy

- `main` - Stable, releasable code
- `feature/<name>` - Feature development
- `fix/<issue>` - Bug fixes

## Governance

### Amendment Process

1. Propose changes via pull request to this constitution
2. Document rationale for principle additions/modifications/removals
3. Update version according to semantic versioning:
   - **MAJOR**: Principle removal or incompatible redefinition
   - **MINOR**: New principle or significant expansion
   - **PATCH**: Clarification or wording refinement
4. Update `LAST_AMENDED_DATE` on merge

### Compliance Verification

- All pull requests MUST include a Constitution Check (see plan-template.md)
- Reviewers MUST verify changes align with stated principles
- Violations require explicit justification in Complexity Tracking section

### Precedence

This constitution supersedes all other documentation when conflicts arise.
Implementation decisions not covered here should choose the option that:

1. Feels most natural to a .NET developer familiar with Minimal APIs + Vue
2. Keeps the implementation AOT-friendly and testable
3. Keeps the public API small, consistent, and discoverable

**Version**: 1.0.0 | **Ratified**: 2025-12-06 | **Last Amended**: 2025-12-06
