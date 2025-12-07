# Feature Specification: Razor Component Migration

**Feature Branch**: `004-razor-migration`  
**Created**: 2025-12-07  
**Status**: Draft  
**Input**: User description: "Migrate from .tui files to .razor files with full IDE syntax support and async support throughout the component lifecycle and rendering pipeline"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Component Authoring with Full IDE Support (Priority: P1)

Developers write TUI components in `.razor` files instead of `.tui` files and receive full IDE tooling support including IntelliSense, syntax highlighting, error squiggles, go-to-definition, and real-time validation.

**Why this priority**: IDE support dramatically improves developer productivity by catching errors as they type, providing autocomplete, and enabling code navigation. This is the primary user-facing value of the migration.

**Independent Test**: Can be fully tested by creating a new `.razor` component file in VS Code/Visual Studio/Rider and verifying IntelliSense works for Razor syntax, C# code completion appears in `@code` blocks, and error squiggles show for syntax errors.

**Acceptance Scenarios**:

1. **Given** a developer opens a `.razor` file in VS Code with C# extension, **When** they type `@code {`, **Then** the editor provides C# IntelliSense for methods and properties
2. **Given** a developer creates a new component, **When** they type `<` in the markup section, **Then** IDE suggests available Rndr components (Column, Row, Panel, Text, Button)
3. **Given** a developer has a syntax error in their `.razor` file, **When** they save the file, **Then** error squiggles appear immediately in the editor without requiring a build
4. **Given** a developer right-clicks a method name in `@code` block, **When** they select "Go to Definition", **Then** IDE navigates to the method definition
5. **Given** a developer hovers over a component parameter, **When** IntelliSense activates, **Then** IDE shows parameter type and documentation

---

### User Story 2 - Async Component Lifecycle (Priority: P1)

Developers implement async initialization, data loading, and event handling within component lifecycle methods, with proper async/await support throughout the rendering pipeline.

**Why this priority**: Async operations are essential for real-world applications (API calls, file I/O, database queries). Without async support, developers must resort to blocking calls or fire-and-forget patterns that break error handling.

**Independent Test**: Can be fully tested by creating a component with `OnInitializedAsync()` that loads data from an async source, verifying it completes before rendering, and ensuring exceptions propagate correctly.

**Acceptance Scenarios**:

1. **Given** a component needs to load data on initialization, **When** developer implements `OnInitializedAsync()` with an await call, **Then** rendering waits for initialization to complete
2. **Given** a button click requires async processing, **When** developer assigns an async method to button's OnClick, **Then** the method executes asynchronously and UI updates afterward
3. **Given** an async operation throws an exception, **When** the exception occurs during lifecycle method, **Then** it propagates to the application error handler
4. **Given** a component updates state during async operation, **When** state changes via Signal, **Then** UI re-renders automatically after state update
5. **Given** multiple components initialize asynchronously, **When** app starts, **Then** all component initializations run concurrently where possible

---

### User Story 3 - Razor Component Base Integration (Priority: P2)

Components inherit from a base class that implements Blazor's `ComponentBase` patterns while maintaining Rndr's layout-based rendering model, enabling use of standard Razor component features.

**Why this priority**: Leveraging Blazor's component infrastructure provides access to parameter binding, cascading values, and lifecycle management patterns familiar to .NET developers. This enables code reuse and knowledge transfer from Blazor.

**Independent Test**: Can be fully tested by creating a component with `[Parameter]` properties, passing parameters from parent to child, and verifying parameter changes trigger `OnParametersSetAsync()`.

**Acceptance Scenarios**:

1. **Given** a component defines a `[Parameter]` property, **When** parent component sets the parameter value, **Then** child component receives the value before rendering
2. **Given** a component implements `OnParametersSetAsync()`, **When** parent changes parameter values, **Then** `OnParametersSetAsync()` executes with new values
3. **Given** a component calls `StateHasChanged()`, **When** called from async context, **Then** component queues re-render on UI thread
4. **Given** a parent provides a cascading value, **When** child component declares `[CascadingParameter]`, **Then** child receives the cascaded value automatically
5. **Given** a component needs cleanup, **When** component is disposed, **Then** `IDisposable.Dispose()` is called

---

### User Story 4 - RenderTree to LayoutBuilder Adapter (Priority: P1)

The framework converts Blazor's RenderTreeBuilder instructions into Rndr's LayoutBuilder node tree, translating HTML-like markup to terminal layout primitives.

**Why this priority**: This is the core technical bridge enabling Razor components to work with Rndr's rendering system. Without this adapter, Razor components cannot render to the terminal.

**Independent Test**: Can be fully tested by creating a simple Razor component with markup (`<Column><Text>Hello</Text></Column>`), rendering it through the adapter, and verifying the resulting LayoutBuilder node tree matches expected structure.

**Acceptance Scenarios**:

1. **Given** a Razor component renders `<Column>` element, **When** RenderTree is processed, **Then** adapter creates Column node in LayoutBuilder tree
2. **Given** a component renders nested elements, **When** RenderTree is processed, **Then** adapter preserves parent-child relationships in node tree
3. **Given** a component uses `@if` conditional rendering, **When** condition is true, **Then** adapter includes conditional nodes in tree
4. **Given** a component uses `@foreach` loop, **When** RenderTree is processed, **Then** adapter creates nodes for each iteration
5. **Given** a component renders text with expressions, **When** RenderTree is processed, **Then** adapter evaluates expressions and creates Text nodes with values

---

### User Story 5 - Migration Path for Existing .tui Components (Priority: P2)

Developers convert existing `.tui` components to `.razor` components with clear migration guidance, automated tooling where possible, and compatibility layer for gradual migration.

**Why this priority**: Existing Rndr applications (like MyFocusTui example) must have a path forward. Without migration support, this becomes a breaking change that prevents adoption.

**Independent Test**: Can be fully tested by taking an existing `.tui` component (Home.tui, Log.tui), following migration guide, converting to `.razor`, and verifying functionality matches original.

**Acceptance Scenarios**:

1. **Given** a developer has a `.tui` component, **When** they rename to `.razor` and update syntax, **Then** component builds without errors
2. **Given** a `.tui` component uses `State("key", value)`, **When** migrated to `.razor`, **Then** equivalent `Signal<T>` pattern works identically
3. **Given** a `.tui` component has `OnInit()`, **When** migrated to `.razor`, **Then** replacing with `OnInitialized()` provides same behavior
4. **Given** a `.tui` component uses `@inject`, **When** migrated to `.razor`, **Then** dependency injection works identically
5. **Given** all components in an app are migrated from `.tui` to `.razor`, **When** running, **Then** app functions identically to pre-migration state

---

### User Story 6 - AOT Compatibility Preservation (Priority: P1)

The framework maintains Native AOT compatibility despite adding Blazor dependencies, ensuring applications can still compile and publish with AOT enabled without trim warnings.

**Why this priority**: AOT compatibility is a constitutional requirement (Principle IV). Breaking AOT support violates core project values and blocks deployment scenarios.

**Independent Test**: Can be fully tested by running `dotnet publish -c Release --self-contained -r osx-arm64 -p:PublishAot=true` on example application and verifying zero trim warnings and successful native binary generation.

**Acceptance Scenarios**:

1. **Given** an Rndr application using Razor components, **When** published with AOT enabled, **Then** build succeeds with zero trim warnings
2. **Given** a Razor component uses dependency injection, **When** AOT analysis runs, **Then** no reflection warnings appear for service resolution
3. **Given** a component uses parameter binding, **When** AOT analysis runs, **Then** parameter properties are preserved by trimmer
4. **Given** the RenderTree adapter uses generic types, **When** AOT analysis runs, **Then** all generic instantiations are statically analyzable
5. **Given** an AOT-published application, **When** executed, **Then** all Razor components render correctly without runtime reflection errors

---

### Edge Cases

- What happens when a Razor component's `OnInitializedAsync()` never completes (infinite await)? System should timeout or provide cancellation token.
- How does system handle exceptions thrown during async lifecycle methods? Must propagate to error boundary or application-level handler.
- What happens when RenderTree contains unsupported HTML elements (e.g., `<div>`, `<span>`)? Adapter should map to nearest Rndr equivalent or throw clear error.
- How does system handle rapid state changes during async operations? Should batch updates to avoid excessive re-renders.
- What happens if developer mixes old `Build()` pattern with new `BuildRenderTree()` pattern? Should be compile-time error, not runtime.
- How does navigation work with async component initialization? Should wait for `OnInitializedAsync()` before considering navigation complete.
- What happens when async operation outlives component lifetime? Should cancel automatically on dispose.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support `.razor` file extension for component files with full Razor syntax parsing
- **FR-002**: IDE MUST provide IntelliSense, syntax highlighting, and error detection for `.razor` components in VS Code, Visual Studio, and Rider
- **FR-003**: Component base class MUST implement async lifecycle methods: `OnInitializedAsync()`, `OnParametersSetAsync()`, `OnAfterRenderAsync(bool firstRender)`
- **FR-004**: Component base class MUST support `BuildRenderTree(RenderTreeBuilder builder)` method for declarative rendering
- **FR-005**: System MUST provide adapter that converts Blazor RenderTree instructions to Rndr LayoutBuilder node tree
- **FR-006**: Event handlers (button clicks, key presses) MUST support async Task-returning methods
- **FR-007**: Signal<T> state management MUST support async change notifications
- **FR-008**: Event loop MUST await async component lifecycle methods before proceeding
- **FR-009**: Rendering pipeline MUST handle async operations in sequence: initialization → parameter binding → rendering → after-render
- **FR-010**: System MUST preserve Native AOT compatibility with zero trim warnings when using Razor components
- **FR-011**: System MUST support Razor component features: `[Parameter]` properties, `[CascadingParameter]`, `@inject` directives, `@code` blocks
- **FR-012**: System MUST support Razor control flow: `@if`, `@foreach`, `@for`, `@while`, `@switch`
- **FR-013**: System MUST support Razor expressions: `@variable`, `@(expression)`, string interpolation
- **FR-014**: Migration documentation MUST provide step-by-step guide for converting `.tui` components to `.razor`
- **FR-015**: System MUST handle component disposal, calling `IDisposable.Dispose()` and cancelling async operations
- **FR-016**: System MUST provide clear error messages when RenderTree contains unsupported elements or patterns
- **FR-017**: System MUST map Razor markup to Rndr primitives: maintain semantic layout (Column, Row, Panel, Text, Button, Spacer, Centered)
- **FR-018**: System MUST integrate with existing Rndr.Testing infrastructure, enabling `RndrTestHost` to work with Razor components
- **FR-019**: System MUST maintain Generic Host integration with `IServiceCollection`, `IConfiguration`, `ILogger<T>` working in Razor components
- **FR-020**: System MUST update `.csproj` configuration to reference Razor SDK and Microsoft.AspNetCore.Components packages

### Key Entities

- **RazorComponentBase**: Adapter base class bridging Blazor ComponentBase with Rndr's rendering model
  - Inherits from `Microsoft.AspNetCore.Components.ComponentBase`
  - Implements async lifecycle methods
  - Provides `BuildRenderTree()` override
  - Manages ViewContext integration
  - Exposes `State<T>()`, `Navigate()` helpers

- **RenderTreeToLayoutAdapter**: Converts Blazor RenderTree to Rndr LayoutBuilder
  - Processes RenderTreeBuilder frames
  - Maps HTML-like elements to Rndr primitives
  - Handles nested element hierarchies
  - Evaluates Razor expressions
  - Manages component parameters
  - Tracks sequence numbers for diffing

- **AsyncEventLoop**: Enhanced event loop supporting async operations
  - Awaits component lifecycle methods
  - Queues async re-renders
  - Manages cancellation tokens
  - Handles async exception propagation
  - Coordinates concurrent async operations

- **AsyncSignal<T>**: Enhanced signal with async notifications
  - Async value change callbacks
  - Task-returning event handlers
  - Batched update support
  - Thread-safe async operations

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can author components in `.razor` files and receive IntelliSense suggestions within 500ms of typing
- **SC-002**: Syntax errors in `.razor` files appear as editor squiggles within 1 second without requiring build
- **SC-003**: Components with async `OnInitializedAsync()` methods complete initialization and render within timeout period (default 30 seconds)
- **SC-004**: AOT publishing of Razor component applications produces zero trim warnings and binaries under 15MB for basic TUI apps
- **SC-005**: Migration guide enables developers to convert existing `.tui` components to `.razor` in under 10 minutes per component
- **SC-006**: Async event handlers (button clicks) complete within configurable timeout (default 30 seconds) before marking operation failed
- **SC-007**: RenderTree to LayoutBuilder adapter processes typical component trees (50 nodes) in under 5ms
- **SC-008**: All existing Rndr.Tests pass after async refactoring, with additional async-specific tests achieving 90%+ coverage of async paths
- **SC-009**: Example application (MyFocusTui) successfully migrates from `.tui` to `.razor` with identical functionality and visual output
- **SC-010**: Framework maintains compatibility with .NET 8+ Generic Host, enabling side-by-side hosting with ASP.NET Core applications
- **SC-011**: IDE go-to-definition works from Razor markup to C# method definitions in 100% of tested scenarios
- **SC-012**: Async exception handling propagates errors to application error handlers with full stack traces preserved

### Non-Functional Requirements

- **NFR-001**: Performance - Async overhead adds no more than 5% latency to render cycles compared to synchronous baseline
- **NFR-002**: Compatibility - Framework maintains backward compatibility with existing Rndr abstractions (IConsoleAdapter, IInputSource, etc.)
- **NFR-003**: Documentation - Migration guide includes complete before/after code samples for every `.tui` pattern
- **NFR-004**: Testing - All async paths have corresponding unit tests using `RndrTestHost` with fake async sources
- **NFR-005**: Error Handling - Async timeouts provide clear error messages indicating which component and lifecycle method timed out
- **NFR-006**: Dependencies - Framework adds Microsoft.AspNetCore.Components package (only required addition)
- **NFR-007**: AOT - All types used in Razor components must have `[DynamicallyAccessedMembers]` annotations where needed

## Assumptions

1. Developers have VS Code with C# extension, Visual Studio 2022+, or Rider 2024+ for IDE support
2. Existing `.tui` components are in working state before migration begins
3. Applications target .NET 8 or higher
4. Developers are familiar with basic async/await patterns in C#
5. Terminal environments support ANSI/VT sequences (no compatibility change from current)
6. Async operations in components have reasonable timeouts (not infinite loops)
8. Project has no external users yet, allowing breaking changes without version constraints
9. Migration is clean break - no coexistence of `.tui` and `.razor` files, all components migrated at once
10. OpenTelemetry packages remain optional despite adding Blazor components
11. Constitution will be amended to update Principle II before implementation begins

## Dependencies

- **External**: Migration requires adding `Microsoft.AspNetCore.Components` NuGet package (~1.5MB)
- **Internal**: Changes affect all core libraries: Rndr, Rndr.Razor, Rndr.Testing
- **Tooling**: Requires Razor Language Service (included in standard .NET SDK installations)
- **Documentation**: Depends on creating comprehensive migration guide with code samples

## Out of Scope

- Hybrid rendering (mixing Blazor web components with Rndr terminal components)
- Blazor Server or Blazor WebAssembly hosting models
- Hot reload support during development (potential future Phase 4 feature)
- Custom Razor directives specific to Rndr (use standard Razor syntax only)
- Blazor component libraries (PrimeBlazor, MudBlazor) - terminal rendering incompatible
- Automatic `.tui` to `.razor` conversion tool (manual migration only)
- Support for older .NET versions (remains .NET 8+ minimum)

## Constitutional Compliance

This feature **implements** constitutional principles as amended:

### ✅ Principle II: Razor Component Model - **IMPLEMENTED**

**Amended Principle (v2.0)**: "View authoring MUST use Blazor Razor components with first-class IDE support"

**Implementation**: This spec defines the migration from legacy `.tui` format to constitutional `.razor` format:
- `.razor` files with native IDE support
- Async-first lifecycle methods
- ComponentBase inheritance with Rndr extensions
- Semantic layout primitives preserved (Column, Row, Panel, Text, Button)

**Constitutional Alignment**: Full compliance with amended Principle II.

### ⚠️ Principle IV: AOT-Friendly by Design - **REQUIRES VERIFICATION**

**Constitutional Principle**: "Framework MUST be fully compatible with Native AOT with no reflection in core runtime paths."

**Implementation Plan**:
- Add `Microsoft.AspNetCore.Components` package (may use reflection for parameter binding)
- Run AOT analysis: `dotnet publish -p:PublishAot=true`
- Add `[DynamicallyAccessedMembers]` annotations where needed
- Test with trim warnings as errors
- Document acceptable warnings with workarounds if needed

**Constitutional Alignment**: Requires verification during implementation per User Story 6.

### ✅ Other Principles - PRESERVED

- **Principle III (Beautiful by Default)**: ✅ Preserved - layout primitives unchanged
- **Principle V (Testability First)**: ✅ Preserved - RndrTestHost enhanced for Razor components
- **Principle VI (Generic Host Integration)**: ✅ Preserved - DI and configuration unchanged  
- **Principle VII (Observability Ready)**: ✅ Preserved - System.Diagnostics APIs unchanged
- **Principle VIII (Phased Development)**: ⚠️ Requires Phase 3 rewrite but maintains working software

## Risks

1. **AOT Compatibility Risk** (MEDIUM): Blazor components may introduce trim warnings requiring workarounds - verification needed during implementation
2. **Migration Effort Risk** (HIGH): 7-11 weeks estimated, potential for scope creep
3. **Performance Risk** (LOW): Async overhead should remain under 5% per NFR-001, requires monitoring
4. **Dependency Risk** (LOW): Adding Microsoft.AspNetCore.Components increases package footprint by ~1.5MB

## Open Questions

**RESOLVED**: All clarifications have been resolved:

1. **Coexistence Strategy** - Clean break approach: No support for mixing `.tui` and `.razor`. All components must be migrated at once. This simplifies implementation and avoids dual code paths.

2. **Version Strategy** - No version constraints: Project has no external users yet, so breaking changes are acceptable without versioning concerns.

3. **Approval Process** - Constitution amendment required: Principle II (Vue-like Component Model) will be updated before implementation to replace Vue-like patterns with Blazor Razor component patterns as the standard authoring model.
