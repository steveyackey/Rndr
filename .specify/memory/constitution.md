<!--
  SYNC IMPACT REPORT
  ==================
  Version change: 1.0.0 → 2.0.0 (major amendment - Principle II redefined)
  
  Modified principles: 
    - Principle II: Vue-like Component Model → Razor Component Model
  
  Rationale for change:
    - Native IDE tooling support (IntelliSense, syntax highlighting, error detection) without custom LSP development
    - Blazor ComponentBase designed for async-first patterns, essential for real-world TUI applications
    - Leverages existing .NET developer knowledge from Blazor web development
    - Eliminates maintenance burden of custom .tui parser and tooling
  
  Breaking changes:
    - All .tui files must be migrated to .razor files
    - Component lifecycle changes from OnInit() to OnInitializedAsync()
    - Component base class changes from TuiComponentBase to RazorComponentBase (inherits ComponentBase)
    - Build() method replaced with BuildRenderTree(RenderTreeBuilder)
  
  Templates requiring updates:
    ✅ plan-template.md - Constitution Check section updated to reference Principle II v2.0
    ✅ spec-template.md - Component model examples updated to Razor patterns
    ⚠️ Existing specs 001-003 - Reference outdated Principle II, marked as legacy
  
  Follow-up TODOs: 
    - Update all code samples in documentation from .tui to .razor
    - Update README.md quick start guide
    - Archive .tui examples as legacy reference
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

### II. Razor Component Model

View authoring MUST use Blazor Razor components with first-class IDE support:

- `.razor` files with declarative markup and `@code` blocks
- Native IDE support (IntelliSense, syntax highlighting, error detection) in VS Code, Visual Studio, Rider
- Async-first lifecycle methods: `OnInitializedAsync()`, `OnParametersSetAsync()`, `OnAfterRenderAsync()`
- Component base inherits from `Microsoft.AspNetCore.Components.ComponentBase` with Rndr-specific extensions
- Reactive state via `Signal<T>` pattern integrated with component lifecycle
- Razor syntax: `@if`, `@foreach`, `@inject`, parameter binding with `[Parameter]`
- Semantic terminal layout primitives: `<Column>`, `<Row>`, `<Panel>`, `<Text>`, `<Button>`, `<Spacer>`, `<Centered>`

**Rationale**: Blazor Razor provides production-grade IDE tooling, async-first design, and leverages 
existing .NET ecosystem knowledge. Native IDE support eliminates need for custom LSP development and 
ongoing tooling maintenance. Async component lifecycle enables real-world scenarios (API calls, file I/O, 
database queries) with proper error handling and cancellation support.

**Evolution from v1.0**: Previously defined "Vue-like Component Model" with `.tui` files. Changed to Razor 
to gain native IDE tooling and async patterns. Semantic layout primitives (Column, Row, Panel) preserved 
from v1.0 design.

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
- **Phase 3**: `.razor` Blazor integration (Razor component model with async support)
- **Phase 4 (stretch)**: Hot reload, advanced tooling features

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
- `Rndr.Razor` - Razor component integration and RenderTree adapter

### API Design Rules

1. Prefer extension methods for optional functionality
2. Use `Action<T>` callbacks for builder configuration (not return-based fluent)
3. Expose `IServiceCollection` and `IConfiguration` for user customization
4. Mark internal implementation details with `internal` visibility
5. Use `sealed` on classes not designed for inheritance

### Dependencies

- **Allowed**: Microsoft.Extensions.* packages, System.Diagnostics.*, Microsoft.AspNetCore.Components (for Razor component runtime)
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
| .razor Components | Snapshot tests via RndrTestHost |
| Async Lifecycle | Unit tests for async initialization, cancellation, error propagation |

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

1. Feels most natural to a .NET developer familiar with Minimal APIs + Blazor
2. Keeps the implementation AOT-friendly and testable
3. Keeps the public API small, consistent, and discoverable

**Version**: 2.0.0 | **Ratified**: 2025-12-06 | **Last Amended**: 2025-12-07
