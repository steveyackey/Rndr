# Implementation Plan: Razor Component Migration

**Branch**: `004-razor-migration` | **Date**: 2025-12-07 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/004-razor-migration/spec.md`

## Summary

Migrate Rndr from custom `.tui` files to standard `.razor` files with full IDE support and async-first component lifecycle. This requires:

1. **Blazor ComponentBase Integration**: Replace `TuiComponentBase` with a new `RazorComponentBase` that inherits from `Microsoft.AspNetCore.Components.ComponentBase`
2. **RenderTree to LayoutBuilder Adapter**: Convert Blazor's `RenderTreeBuilder` instructions into Rndr's semantic layout primitives (Column, Row, Panel, Text, Button)
3. **Async Event Loop & Rendering Pipeline**: Refactor `IEventLoop`, `Signal<T>`, and component lifecycle to support async operations throughout
4. **Migration Path**: Provide documentation and examples showing how to convert existing `.tui` components to `.razor`
5. **AOT Compatibility Verification**: Ensure Microsoft.AspNetCore.Components package doesn't break Native AOT publishing

**Technical Approach**: This is a phased rewrite of the rendering pipeline. Phase 0 researches Blazor integration patterns. Phase 1 designs the async-first architecture with clear contracts. Phase 2+ implements incrementally with continuous AOT verification.

## Technical Context

**Language/Version**: C# 12+ / .NET 8+  
**Primary Dependencies**: 
- Existing: `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Options`
- New: `Microsoft.AspNetCore.Components` (v8.0.0+) - adds ~1.5MB
- Existing Dev: `Microsoft.CodeAnalysis.CSharp` (source generator - already in Rndr.Razor)

**Storage**: `InMemoryStateStore` for state management (existing)  
**Testing**: xUnit with `RndrTestHost` (existing framework)  
**Target Platform**: .NET terminals with ANSI/VT support (unchanged)  
**Project Type**: Multi-project solution (Rndr, Rndr.Razor, Rndr.Testing + tests + examples)  
**Performance Goals**: 
- Async overhead < 5% render latency (NFR-001)
- RenderTree adapter < 5ms for 50-node trees (SC-007)
- IDE IntelliSense < 500ms response time (SC-001)

**Constraints**: 
- Must preserve Native AOT compatibility (zero trim warnings)
- No breaking changes to public API surface where possible
- Maintain Generic Host integration patterns
- Preserve semantic layout primitives (Column, Row, Panel, Text, Button, Spacer, Centered, TextInput)
- Async operations must have configurable timeouts (default 30s)

**Scale/Scope**: 
- Example app: MyFocusTui (2 components: Home.tui, Log.tui) - migration target
- Core components affected: TuiComponentBase, DefaultEventLoop, Signal<T>, TuiApp, ViewContext
- New components: RazorComponentBase, RenderTreeToLayoutAdapter, AsyncEventLoop, AsyncSignal<T>

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ I. Minimal API Ergonomics
**Compliance**: FULL - API patterns unchanged
- `TuiApplication.CreateBuilder()`, `Build()`, `MapView()`, `RunAsync()` preserved
- Route-based view registration maintained
- Extension methods for configuration remain
- **No violations**: Minimal API entry point patterns untouched

### ✅ II. Razor Component Model
**Compliance**: FULL - This spec IMPLEMENTS Principle II v2.0
- Migrating from `.tui` (legacy) to `.razor` (constitutional standard)
- Blazor Razor components with native IDE support
- Async-first lifecycle: `OnInitializedAsync()`, `OnParametersSetAsync()`, `OnAfterRenderAsync()`
- ComponentBase inheritance with Rndr extensions
- Signal<T> reactive state integrated with component lifecycle
- Semantic layout primitives preserved: Column, Row, Panel, Text, Button, Spacer, Centered, TextInput
- **Constitutional Mandate**: This feature brings the codebase into compliance with amended constitution

### ✅ III. Beautiful by Default
**Compliance**: FULL - Visual output unchanged
- Semantic layout primitives preserved (Column, Row, Panel, etc.)
- Design tokens for theming maintained
- Unicode box-drawing characters retained
- Consistent spacing and alignment preserved
- **No violations**: Rendering layer unchanged, only authoring model changes

### ⚠️ IV. AOT-Friendly by Design
**Compliance**: REQUIRES VERIFICATION (User Story 6)
- Microsoft.AspNetCore.Components package may use reflection for parameter binding
- Must verify: `dotnet publish -c Release --self-contained -r osx-arm64 -p:PublishAot=true` produces zero trim warnings
- May require `[DynamicallyAccessedMembers]` annotations on RazorComponentBase
- RenderTreeToLayoutAdapter must use compile-time known types only
- **Mitigation Plan**: 
  - Test AOT publishing at each phase
  - Add necessary trim annotations immediately when warnings appear
  - Document any reflection paths and ensure they're not in hot paths
  - If ComponentBase uses reflection, verify it's only during initialization
- **Acceptance Criteria**: SC-004 requires zero trim warnings for basic TUI apps

### ✅ V. Testability First
**Compliance**: FULL - Test infrastructure enhanced
- RndrTestHost will be updated to support RazorComponentBase
- Async component testing via Task-based assertions
- All I/O abstractions maintained (IConsoleAdapter, IInputSource, IClock, IEventLoop)
- Build() → BuildRenderTree() remains side-effect free (no I/O)
- **Enhancement**: Async testing support added to RndrTestHost
- **No violations**: Testability enhanced, not reduced

### ✅ VI. Generic Host Integration
**Compliance**: FULL - Host integration preserved
- TuiAppBuilder continues wrapping HostApplicationBuilder
- IServiceCollection for DI unchanged
- IConfiguration, ILogger<T>, IOptions<T> available in Razor components via @inject
- IHostApplicationLifetime respected
- **No violations**: Standard .NET patterns maintained

### ✅ VII. Observability Ready
**Compliance**: FULL - Observability unchanged
- ActivitySource for tracing (navigation, render cycles, key events) maintained
- Meter for metrics preserved
- System.Diagnostics APIs only (no OpenTelemetry hard dependency)
- **No violations**: Diagnostic instrumentation unaffected by async changes

### ⚠️ VIII. Phased Development
**Compliance**: REQUIRES PHASE 3 REWRITE - Breaking change
- **Rationale**: Migration from .tui to .razor is inherently breaking
- **Mitigation**: 
  - Clear migration guide with step-by-step instructions (FR-014, SC-005)
  - Working examples demonstrating migration path
  - All existing tests updated and passing
  - Each sub-phase maintains working software
- **Project Status**: No external users yet (per assumptions), allows clean break
- **Phasing Strategy**: 
  - Phase 0: Research (non-breaking)
  - Phase 1: Design (non-breaking)
  - Phase 2: Core async infrastructure (internal breaking changes)
  - Phase 3: Razor integration (external breaking change - requires migration)
  - Phase 4: Documentation & migration (migration support)

**CONSTITUTION CHECK RESULT**: 2 principles require verification/mitigation:
- **Principle IV (AOT)**: Verify at each phase, add trim annotations as needed
- **Principle VIII (Phased)**: Accept breaking change, provide migration guide

All other principles fully compliant or enhanced. **Proceed to Phase 0**.

## Project Structure

### Documentation (this feature)

```text
specs/004-razor-migration/
├── plan.md              # This file (generated by plan command)
├── spec.md              # Feature specification (input)
├── research.md          # Phase 0 output (generated below)
├── data-model.md        # Phase 1 output (generated below)
├── quickstart.md        # Phase 1 output (generated below)
├── contracts/           # Phase 1 output (generated below)
│   ├── IRazorComponent.cs
│   ├── IRenderTreeAdapter.cs
│   └── IAsyncEventLoop.cs
└── tasks.md             # Phase 2 output (NOT created by this plan - requires /speckit.tasks)
```

### Source Code (repository root)

```text
src/
├── Rndr/                          # Core framework library
│   ├── TuiComponentBase.cs        # [DEPRECATED] Legacy .tui base class
│   ├── RazorComponentBase.cs      # [NEW] Blazor ComponentBase adapter
│   ├── IEventLoop.cs              # [MODIFIED] Add async Task RunAsync
│   ├── DefaultEventLoop.cs        # [MODIFIED] Await async lifecycle
│   ├── AsyncEventLoop.cs          # [NEW] Enhanced async event loop
│   ├── Signal.cs                  # [MODIFIED] Add async change callbacks
│   ├── AsyncSignal.cs             # [NEW] Task-based signal with async support
│   ├── TuiApp.cs                  # [MODIFIED] Support RazorComponentBase types
│   ├── ViewContext.cs             # [MODIFIED] Expose to Razor components
│   └── Rendering/
│       └── RenderTreeToLayoutAdapter.cs  # [NEW] Blazor → Rndr adapter
│
├── Rndr.Razor/                    # Razor integration and code generation
│   ├── Rndr.Razor.csproj          # [MODIFIED] Add Microsoft.AspNetCore.Components ref
│   ├── TuiRazorConfiguration.cs   # [MODIFIED] Support .razor extension
│   ├── build/                     # [MODIFIED] Update targets for .razor files
│   ├── Generator/                 # [MODIFIED] Generate from .razor instead of .tui
│   │   ├── TuiSourceGenerator.cs  # [MODIFIED] Process .razor files
│   │   └── RazorCodeEmitter.cs    # [NEW] Emit ComponentBase-based code
│   └── Parsing/                   # [POTENTIALLY DEPRECATED] May use Razor parser
│       └── TuiSyntaxParser.cs     # [EVALUATE] Replace with Razor compiler?
│
├── Rndr.Testing/                  # Test helpers
│   ├── RndrTestHost.cs            # [MODIFIED] Support RazorComponentBase
│   ├── FakeAsyncEventLoop.cs      # [NEW] Test async operations
│   └── AsyncTestHelpers.cs        # [NEW] Task-based test utilities
│
tests/
├── Rndr.Tests/                    # Framework unit tests
│   ├── RazorComponentBaseTests.cs # [NEW] Component lifecycle tests
│   ├── RenderTreeAdapterTests.cs  # [NEW] Adapter logic tests
│   ├── AsyncEventLoopTests.cs     # [NEW] Async event loop tests
│   ├── AsyncSignalTests.cs        # [NEW] Async signal tests
│   └── [EXISTING TESTS]           # [MODIFIED] Update for async patterns
│
├── Rndr.Razor.Tests/              # Generator tests
│   ├── RazorCodeEmitterTests.cs   # [NEW] Razor code generation tests
│   └── [EXISTING TESTS]           # [MODIFIED] Update for .razor syntax
│
examples/
└── MyFocusTui/                    # Reference sample application
    ├── Program.cs                 # [MODIFIED] MapView with RazorComponentBase
    └── Pages/
        ├── Home.razor             # [NEW] Migrated from Home.tui
        └── Log.razor              # [NEW] Migrated from Log.tui
```

**Structure Decision**: The existing multi-project structure is preserved. Core changes focus on Rndr (async infrastructure + adapter), Rndr.Razor (Razor support), and Rndr.Testing (async test helpers). The change is primarily additive with clear deprecation paths for legacy .tui components.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Principle VIII: Phase 3 Breaking Change | Migration from .tui to .razor is inherently incompatible at the file format level. No gradual migration possible while maintaining two parsers. | Supporting both .tui and .razor simultaneously would require maintaining dual code paths, duplicate testing, and create confusion about which format to use. Project has no external users yet, so clean break is acceptable. |
| Principle IV: Potential AOT Warnings | Microsoft.AspNetCore.Components may use reflection for parameter binding, which could introduce trim warnings. | Could build custom parameter binding system without reflection, but would lose compatibility with standard Blazor patterns and require reinventing Blazor's infrastructure. Mitigation: verify and annotate as needed. |

---

# Phase 0: Research

*Goal: Gather architectural knowledge before committing to design decisions*

## Research Topics

### 1. Blazor ComponentBase Integration Patterns

**Question**: How does `Microsoft.AspNetCore.Components.ComponentBase` manage lifecycle, parameter binding, and state changes?

**Research Approach**:
- Review Microsoft.AspNetCore.Components source code on GitHub
- Identify core lifecycle methods: `SetParametersAsync`, `OnInitializedAsync`, `OnParametersSetAsync`, `OnAfterRenderAsync`, `StateHasChanged`
- Understand parameter binding mechanism: `[Parameter]` attributes, how Blazor populates these properties
- Investigate cascading parameters: `[CascadingParameter]` pattern
- Examine how ComponentBase interacts with RenderTreeBuilder

**Key Questions**:
1. Can ComponentBase be used outside of Blazor Server/WebAssembly hosting models?
2. What reflection does ComponentBase use internally? Is it AOT-safe?
3. How does `StateHasChanged()` queue re-renders?
4. Can we override `BuildRenderTree(RenderTreeBuilder)` and intercept the render output?
5. How are exceptions in async lifecycle methods propagated?

**Expected Findings**:
- ComponentBase is designed to work with a "renderer" abstraction
- We need to provide a custom renderer that captures RenderTreeBuilder frames
- Lifecycle methods are template method pattern - we can override safely
- Parameter binding likely uses reflection (investigate trim safety)
- `StateHasChanged()` uses a synchronization context to queue re-renders

**Deliverable**: Document section titled "ComponentBase Integration Analysis" with:
- Lifecycle method call order diagram
- Parameter binding flow (including reflection usage)
- StateHasChanged mechanism
- Custom renderer requirements
- AOT compatibility assessment

---

### 2. RenderTreeBuilder to LayoutBuilder Mapping Strategy

**Question**: How do we convert Blazor's frame-based RenderTreeBuilder instructions into Rndr's semantic layout tree?

**Research Approach**:
- Study RenderTreeBuilder API: `OpenElement`, `CloseElement`, `AddAttribute`, `AddContent`, `AddMarkupContent`
- Analyze RenderTree frame structure: sequence numbers, element names, attributes
- Map Razor element names to Rndr primitives:
  - `<Column>` → `LayoutBuilder.Column()`
  - `<Row>` → `LayoutBuilder.Row()`
  - `<Panel>` → `LayoutBuilder.Panel()`
  - `<Text>` → `LayoutBuilder.Text()`
  - `<Button>` → `LayoutBuilder.Button()`
  - etc.
- Handle attributes: convert Razor attributes to method calls (e.g., `Padding="2"` → `.Padding(2)`)
- Handle event bindings: `@onclick` → button OnClick callbacks
- Handle text content and expressions: `@variable` → interpolated strings

**Key Questions**:
1. Can we intercept RenderTreeBuilder calls before frames are submitted?
2. How do we handle nested elements (parent-child relationships)?
3. What happens with conditional rendering (`@if`, `@foreach`) - do we see frames or just results?
4. How do we map arbitrary HTML elements user might write (e.g., `<div>`) to Rndr primitives?
5. Performance: can we avoid allocating temporary collections during mapping?

**Expected Findings**:
- RenderTreeBuilder uses a frame sequence with Open/Close operations - perfect for tree building
- Conditional rendering appears as variable frame sequences - handled naturally
- Need whitelist of allowed element names + error for unsupported HTML elements
- Attribute mapping requires type conversion (string → int for Padding, string → Action for OnClick)
- Can build LayoutBuilder tree incrementally as frames arrive

**Deliverable**: Document section titled "RenderTree Mapping Strategy" with:
- Frame sequence example with corresponding LayoutBuilder calls
- Element name mapping table
- Attribute mapping rules
- Error handling for unsupported elements
- Performance considerations

---

### 3. Async Lifecycle Implementation Approach

**Question**: How do we refactor the event loop and component lifecycle to support async/await throughout?

**Research Approach**:
- Review current `DefaultEventLoop.RunAsync()` synchronous render cycle
- Identify await points: component initialization, parameter updates, event handlers, after-render
- Design async signal propagation: when `Signal<T>.Value` changes during async operation, how to queue re-render?
- Handle concurrency: what if user clicks button while async initialization is still running?
- Implement cancellation: when component navigates away during async operation, cancel in-flight tasks
- Design timeout mechanism: async operations have configurable timeout (default 30s per spec)

**Key Questions**:
1. Should event loop await each lifecycle method sequentially or run some in parallel?
2. How do we prevent race conditions when multiple async operations update state?
3. What thread does async continuation run on? Do we need synchronization context?
4. How do we cancel async operations on navigation/disposal?
5. Should Signal<T> support async change callbacks or keep them sync and queue re-renders?

**Expected Findings**:
- Event loop must await lifecycle methods sequentially to maintain order
- Signal<T> should remain synchronous, but trigger async re-render queue
- Need `CancellationTokenSource` per component, cancel on dispose
- SynchronizationContext not needed in console app (already single-threaded input loop)
- Timeout wrapper: `await Task.WhenAny(userTask, Task.Delay(timeout))` pattern

**Deliverable**: Document section titled "Async Lifecycle Architecture" with:
- Async event loop flow diagram
- Lifecycle method execution order with await points
- Cancellation token propagation pattern
- Timeout implementation approach
- Thread safety analysis

---

### 4. AOT Compatibility Analysis

**Question**: Does Microsoft.AspNetCore.Components package break Native AOT, and if so, how do we mitigate?

**Research Approach**:
- Create minimal test project: Rndr + Microsoft.AspNetCore.Components + simple Razor component
- Publish with AOT: `dotnet publish -c Release --self-contained -r osx-arm64 -p:PublishAot=true`
- Analyze trim warnings (if any)
- Identify reflection usage in ComponentBase and parameter binding
- Research `[DynamicallyAccessedMembers]` annotations for AOT preservation
- Test runtime behavior of AOT-published app with Razor components

**Key Questions**:
1. Does ComponentBase itself use reflection in hot paths?
2. Does parameter binding via `[Parameter]` attributes survive trimming?
3. Are there any dynamic assembly loading issues?
4. Can we annotate RazorComponentBase to preserve necessary metadata?
5. Performance: does AOT compilation affect async/await overhead?

**Expected Findings**:
- ComponentBase parameter binding uses reflection (likely ILLink warnings)
- Need `[DynamicallyAccessedMembers(PublicProperties | PublicMethods)]` on RazorComponentBase
- May need `[RequiresUnreferencedCode]` warnings if reflection is unavoidable
- Runtime behavior likely unaffected if proper annotations applied
- Worst case: document reflection usage and verify it's not in hot rendering path

**Deliverable**: Document section titled "AOT Compatibility Report" with:
- List of trim warnings (if any) with explanations
- Required annotations for RazorComponentBase
- Performance impact assessment
- Mitigation strategies for any unavoidable reflection
- Pass/fail verdict on constitutional Principle IV

---

### 5. Migration Pattern Recommendations

**Question**: What is the best step-by-step process for developers to migrate existing .tui components to .razor?

**Research Approach**:
- Manually migrate MyFocusTui example (Home.tui → Home.razor, Log.tui → Log.razor)
- Document every syntax change required
- Identify common patterns: State() → Signal<T> in @code, OnInit() → OnInitializedAsync()
- Test migrated components for functional equivalence
- Measure migration time per component (target < 10 minutes per SC-005)
- Create before/after code samples for each pattern

**Key Questions**:
1. What's the minimum viable migration (get something rendering)?
2. Which patterns are mechanical renames vs. require rethinking?
3. Are there any .tui patterns that don't have .razor equivalents?
4. Should we provide code mod scripts or manual instructions?
5. How do we test that migration preserved behavior?

**Expected Findings**:
- File rename: `.tui` → `.razor`
- Directive change: `@view` → remove (implicit in Razor)
- Base class change: inherits from ComponentBase (implicit), methods available via RazorComponentBase
- Lifecycle change: `OnInit()` → `OnInitializedAsync()` (add async/await)
- State initialization: `Signal<T> x = default!;` + `OnInit() { x = State("x", 0); }` → keep same pattern
- Build method: remove (uses Razor markup instead)
- Markup: largely unchanged (Column, Row, Panel syntax compatible)

**Deliverable**: Document section titled "Migration Patterns" with:
- Step-by-step migration checklist
- Before/after code samples for each pattern
- Common pitfalls and solutions
- Testing approach for migration verification
- Time estimate per component type

---

## Research Deliverables

Create `specs/004-razor-migration/research.md` with the following structure:

```markdown
# Research: Razor Component Migration

**Date**: 2025-12-07  
**Researcher**: [AI/Human pair]  
**Status**: Complete

## Executive Summary

[2-3 paragraph summary of key findings and go/no-go recommendation]

## 1. ComponentBase Integration Analysis

[Findings from research topic 1]

## 2. RenderTree Mapping Strategy

[Findings from research topic 2]

## 3. Async Lifecycle Architecture

[Findings from research topic 3]

## 4. AOT Compatibility Report

[Findings from research topic 4]

## 5. Migration Patterns

[Findings from research topic 5]

## Recommendations for Phase 1 Design

[List of architectural decisions informed by research]

## Open Questions for Phase 1

[Any unresolved issues that design must address]
```

---

# Phase 1: Design

*Goal: Define data models, contracts, and component interfaces before implementation*

## Design Outputs

### 1. Data Model (`specs/004-razor-migration/data-model.md`)

Define the key entities and their relationships:

```markdown
# Data Model: Razor Component Migration

## Entity Relationship Diagram

```
┌─────────────────────┐
│ RazorComponentBase  │ (inherits ComponentBase, adapts to Rndr)
├─────────────────────┤
│ + ViewContext       │ ─────┐
│ + Navigation        │      │
│ + State<T>()        │      │
│ + StateGlobal<T>()  │      │
│                     │      │
│ # OnInitializedAsync│      │
│ # OnParametersSetAsync     │
│ # OnAfterRenderAsync│      │
│ # BuildRenderTree() │      │
└─────────────────────┘      │
         │                   │
         │ uses              │
         ▼                   │
┌─────────────────────┐      │
│ RenderTreeBuilder   │      │ (Blazor framework)
├─────────────────────┤      │
│ + OpenElement()     │      │
│ + CloseElement()    │      │
│ + AddAttribute()    │      │
│ + AddContent()      │      │
└─────────────────────┘      │
         │                   │
         │ frames            │
         ▼                   │
┌────────────────────────┐   │
│ RenderTreeToLayout     │   │
│ Adapter                │   │
├────────────────────────┤   │
│ + CaptureFrames()      │   │
│ + BuildLayoutTree()    │   │
│ + MapElement()         │   │
│ + MapAttribute()       │   │
│ + HandleEventBinding() │   │
└────────────────────────┘   │
         │                   │
         │ produces          │
         ▼                   │
┌─────────────────────┐      │
│ LayoutBuilder       │      │ (existing Rndr)
├─────────────────────┤      │
│ + Column()          │      │
│ + Row()             │      │
│ + Panel()           │      │
│ + Text()            │      │
│ + Button()          │      │
└─────────────────────┘      │
                             │
┌─────────────────────┐      │
│ AsyncEventLoop      │ ◄────┘ (uses ViewContext)
├─────────────────────┤
│ + RunAsync()        │ (awaits async lifecycle)
│ - RenderAsync()     │
│ - ProcessInputAsync()│
│ - HandleEventAsync()│
└─────────────────────┘
         │
         │ triggers
         ▼
┌─────────────────────┐
│ AsyncSignal<T>      │ (optional - keep Signal<T> sync)
├─────────────────────┤
│ + Value (get/set)   │
│ + OnChangedAsync    │ (Task-returning callback)
│ + NotifyAsync()     │
└─────────────────────┘
```

## Entity Definitions

### RazorComponentBase

**Purpose**: Adapter class that bridges Blazor's ComponentBase with Rndr's rendering model.

**Properties**:
- `ViewContext Context { get; }` - Access to services, navigation, state
- `NavigationContext Navigation { get; }` - Shortcut to Context.Navigation
- `IServiceProvider Services { get; }` - Shortcut to Context.Services

**Methods**:
- `protected Signal<T> State<T>(string key, T initial)` - Route-scoped state
- `protected Signal<T> StateGlobal<T>(string key, T initial)` - Global state
- `protected override Task OnInitializedAsync()` - Async initialization hook
- `protected override Task OnParametersSetAsync()` - Async parameter update hook
- `protected override Task OnAfterRenderAsync(bool firstRender)` - Async after-render hook
- `protected override void BuildRenderTree(RenderTreeBuilder builder)` - Razor markup compilation target

**Inheritance**:
- Inherits: `Microsoft.AspNetCore.Components.ComponentBase`
- Implements: `IDisposable` (via ComponentBase)

**Lifecycle**:
1. Construction (DI constructor injection)
2. Property injection (@inject directives)
3. AttachContext(ViewContext) - internal framework call
4. SetParametersAsync() - ComponentBase framework
5. OnInitializedAsync() - user override
6. OnParametersSetAsync() - user override (on subsequent renders)
7. BuildRenderTree() - generates Razor markup
8. OnAfterRenderAsync() - user override
9. Dispose() - cleanup

**State Management**:
- Internally calls `Context.State()` / `Context.StateGlobal()`
- Signal changes automatically trigger `StateHasChanged()` → re-render

---

### RenderTreeToLayoutAdapter

**Purpose**: Converts Blazor RenderTreeBuilder frame sequence into Rndr LayoutBuilder node tree.

**Properties**:
- `private Stack<LayoutContext> _builderStack` - Tracks nested element contexts
- `private RenderTreeBuilder _renderTree` - Captured Blazor render tree
- `private LayoutBuilder _rootLayout` - Rndr layout being built

**Methods**:
- `public void BeginCapture(RenderTreeBuilder renderTree)` - Start capturing frames
- `public IReadOnlyList<Node> BuildLayoutTree()` - Convert frames to Rndr nodes
- `private void ProcessFrame(RenderTreeFrame frame)` - Handle individual frame
- `private void OpenElement(string elementName, int sequence)` - Push element context
- `private void CloseElement()` - Pop element context
- `private void AddAttribute(string name, object? value)` - Store attribute for current element
- `private void AddContent(string text)` - Add text content to current element
- `private void MapToRndrPrimitive(string elementName, Dictionary<string, object?> attrs)` - Create Rndr element

**Element Mapping**:
| Razor Element | Rndr Primitive | Notes |
|---------------|----------------|-------|
| `<Column>` | `LayoutBuilder.Column()` | Padding, Gap attributes |
| `<Row>` | `LayoutBuilder.Row()` | Padding, Gap, Align |
| `<Panel>` | `LayoutBuilder.Panel()` | Title attribute |
| `<Text>` | `LayoutBuilder.Text()` | Bold, Accent, Faint, Align |
| `<Button>` | `LayoutBuilder.Button()` | OnClick event binding |
| `<Spacer>` | `LayoutBuilder.Spacer()` | Weight attribute |
| `<Centered>` | `LayoutBuilder.Centered()` | Container element |
| `<TextInput>` | `LayoutBuilder.TextInput()` | Value, OnChanged, Placeholder |

**Attribute Mapping**:
- String literals: `Padding="2"` → `.Padding(2)` (parse to int)
- Event handlers: `OnClick="@HandleClick"` → `.Button("label", HandleClick)`
- Expressions: `Value="@model.Text"` → `.TextInput(model.Text, ...)`
- Booleans: `Bold="true"` → `s.Bold = true`

**Error Handling**:
- Unsupported element name → throw clear exception with suggestion
- Invalid attribute type → throw with expected type
- Unclosed element → detected by stack depth check

---

### AsyncEventLoop

**Purpose**: Enhanced event loop supporting async component lifecycle and event handlers.

**Properties**:
- `private CancellationTokenSource _componentCts` - Per-component cancellation
- `private TimeSpan _asyncTimeout` - Default 30 seconds (configurable via RndrOptions)

**Methods**:
- `public async Task RunAsync(TuiApp app, CancellationToken ct)` - Main loop (replaces sync version)
- `private async Task RenderAsync(TuiApp app)` - Async render cycle
- `private async Task ProcessInputAsync(KeyEvent key, TuiApp app)` - Async input handling
- `private async Task InvokeComponentLifecycleAsync(RazorComponentBase component, string method)` - Lifecycle wrapper with timeout

**Async Flow**:
1. Render cycle: await component.OnInitializedAsync() (first render only)
2. Render cycle: await component.OnParametersSetAsync() (parameter changes)
3. Render cycle: component.BuildRenderTree() (synchronous)
4. Render cycle: await adapter.BuildLayoutTree() (synchronous but returns Task for future)
5. Render cycle: await component.OnAfterRenderAsync(firstRender)
6. Input cycle: await button.OnClickAsync() (if button handler is async)

**Timeout Handling**:
```csharp
var timeoutTask = Task.Delay(_asyncTimeout, ct);
var lifecycleTask = component.OnInitializedAsync();
var completed = await Task.WhenAny(lifecycleTask, timeoutTask);
if (completed == timeoutTask) 
{
    throw new TimeoutException($"Component {component.GetType().Name}.OnInitializedAsync timed out");
}
await lifecycleTask; // Propagate exceptions
```

**Cancellation**:
- Pass `_componentCts.Token` to all async lifecycle methods
- On navigation away: `_componentCts.Cancel()` + `await Task.Delay(100)` (grace period)
- On app quit: global cancellation token cancels all

---

### AsyncSignal<T>

**Purpose**: Optional async-aware signal for Task-returning change callbacks.

**Decision**: After research, likely NOT needed. Keep `Signal<T>` synchronous. Async operations triggered by signal changes should be queued via `StateHasChanged()` → event loop re-renders → lifecycle hooks run async.

**Rationale**: Introducing async callbacks in signals adds complexity. Blazor's `StateHasChanged()` mechanism already provides async support via lifecycle hooks. Signal changes can remain synchronous, triggering re-render, which then invokes async lifecycle methods.

**Alternative Design** (if needed):
```csharp
public class AsyncSignal<T>
{
    private T _value;
    private Func<T, Task>? _onChangedAsync;
    
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                // Fire and forget - caller awaits re-render separately
                _ = _onChangedAsync?.Invoke(value);
            }
        }
    }
}
```

---

## Relationships

- **RazorComponentBase** uses **ViewContext** (injected by framework via AttachContext)
- **RazorComponentBase** overrides **BuildRenderTree** which writes to **RenderTreeBuilder** (Blazor)
- **RenderTreeToLayoutAdapter** consumes **RenderTreeBuilder** frames
- **RenderTreeToLayoutAdapter** produces **LayoutBuilder** tree
- **AsyncEventLoop** invokes **RazorComponentBase** lifecycle methods (async)
- **AsyncEventLoop** uses **RenderTreeToLayoutAdapter** to convert markup to layout
- **AsyncEventLoop** triggers **Signal<T>** changes, which call **StateHasChanged()**, which queues re-render
```

---

### 2. Contracts (`specs/004-razor-migration/contracts/`)

Define interface contracts for key abstractions:

#### `IRazorComponent.cs`

```csharp
namespace Rndr;

/// <summary>
/// Contract for Razor-based TUI components.
/// Extends ComponentBase with Rndr-specific functionality.
/// </summary>
public interface IRazorComponent : IDisposable
{
    /// <summary>
    /// Attaches the view context to this component.
    /// Called by the framework before rendering.
    /// </summary>
    void AttachContext(ViewContext context);

    /// <summary>
    /// Gets the view context. Available after AttachContext is called.
    /// </summary>
    ViewContext Context { get; }

    /// <summary>
    /// Async initialization hook. Called once after first parameter set.
    /// </summary>
    Task OnInitializedAsync();

    /// <summary>
    /// Async parameter update hook. Called when parameters change.
    /// </summary>
    Task OnParametersSetAsync();

    /// <summary>
    /// Async after-render hook. Called after BuildRenderTree completes.
    /// </summary>
    /// <param name="firstRender">True if this is the first render.</param>
    Task OnAfterRenderAsync(bool firstRender);

    /// <summary>
    /// Builds the component's render tree using Blazor's RenderTreeBuilder.
    /// Generated from .razor markup by Razor compiler.
    /// </summary>
    /// <param name="builder">The render tree builder.</param>
    void BuildRenderTree(RenderTreeBuilder builder);
}
```

#### `IRenderTreeAdapter.cs`

```csharp
namespace Rndr.Rendering;

/// <summary>
/// Contract for adapters that convert Blazor RenderTree to Rndr LayoutBuilder.
/// </summary>
public interface IRenderTreeAdapter
{
    /// <summary>
    /// Captures a RenderTreeBuilder's frames and converts to Rndr layout nodes.
    /// </summary>
    /// <param name="renderTree">The Blazor render tree builder.</param>
    /// <returns>Rndr layout nodes ready for rendering.</returns>
    IReadOnlyList<Node> AdaptRenderTree(RenderTreeBuilder renderTree);

    /// <summary>
    /// Maps a Razor element name to an Rndr primitive, or throws if unsupported.
    /// </summary>
    /// <param name="elementName">Razor element name (e.g., "Column", "Button").</param>
    /// <param name="attributes">Element attributes as key-value pairs.</param>
    /// <returns>Configuration action for LayoutBuilder.</returns>
    /// <exception cref="NotSupportedException">Element name not recognized.</exception>
    Action<LayoutBuilder> MapElement(string elementName, IReadOnlyDictionary<string, object?> attributes);
}
```

#### `IAsyncEventLoop.cs`

```csharp
namespace Rndr;

/// <summary>
/// Contract for async-aware event loop that drives TUI application.
/// Extends IEventLoop with async lifecycle support.
/// </summary>
public interface IAsyncEventLoop : IEventLoop
{
    /// <summary>
    /// Runs the async event loop until cancellation or quit.
    /// Awaits component lifecycle methods before proceeding.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="cancellationToken">Token to cancel the event loop.</param>
    /// <returns>Task that completes when event loop exits.</returns>
    new Task RunAsync(TuiApp app, CancellationToken cancellationToken);

    /// <summary>
    /// Timeout for async lifecycle methods. Default 30 seconds.
    /// </summary>
    TimeSpan AsyncTimeout { get; set; }

    /// <summary>
    /// Cancellation token for the currently rendering component.
    /// Canceled when component is disposed or navigation occurs.
    /// </summary>
    CancellationToken ComponentCancellationToken { get; }
}
```

---

### 3. Quick Start Guide (`specs/004-razor-migration/quickstart.md`)

Provide hello-world example for new Razor components:

```markdown
# Quick Start: Razor Components in Rndr

## Prerequisites

- .NET 8 SDK or later
- Terminal with ANSI/VT support
- Code editor with C# and Razor support (VS Code, Visual Studio, Rider)

## Step 1: Create Project

```bash
dotnet new console -n MyRazorTui
cd MyRazorTui
dotnet add package Rndr
dotnet add package Rndr.Razor
```

## Step 2: Create Razor Component

Create `Pages/Hello.razor`:

```razor
@inherits RazorComponentBase
@using Rndr.Layout

<Column Padding="2" Gap="1">
    <Text Bold="true" Accent="true">Hello, Razor!</Text>
    <Text>This is a Razor component in Rndr.</Text>
    <Button OnClick="@Increment">Click Me (@count)</Button>
</Column>

@code {
    private Signal<int> count = default!;

    protected override Task OnInitializedAsync()
    {
        count = State("count", 0);
        return Task.CompletedTask;
    }

    void Increment()
    {
        count.Value++;
    }
}
```

## Step 3: Register Component

Update `Program.cs`:

```csharp
using Rndr;
using MyRazorTui.Pages;

var app = TuiApplication.CreateBuilder(args).Build();

app.MapView("/", typeof(Hello));

app.OnGlobalKey((key, ctx) =>
{
    if (key.KeyChar is 'q' or 'Q')
    {
        ctx.Application.Quit();
        return true;
    }
    return false;
});

await app.RunAsync();
```

## Step 4: Run

```bash
dotnet run
```

Press Tab to focus button, Enter/Space to click, Q to quit.

## Next Steps

### Async Data Loading

```razor
@code {
    private Signal<string> data = default!;

    protected override async Task OnInitializedAsync()
    {
        data = State("data", "Loading...");
        
        // Simulate async API call
        await Task.Delay(1000);
        data.Value = "Data loaded!";
    }
}
```

### Dependency Injection

```razor
@inject ILogger<Hello> Logger

@code {
    protected override Task OnInitializedAsync()
    {
        Logger.LogInformation("Hello component initialized");
        return Task.CompletedTask;
    }
}
```

### Navigation

```razor
<Button OnClick="@GoToSettings">Settings</Button>

@code {
    void GoToSettings()
    {
        Navigation.Navigate("/settings");
    }
}
```

## IDE Support

### VS Code

1. Install "C# Dev Kit" extension
2. Razor syntax highlighting and IntelliSense work automatically
3. Error squiggles appear in editor without building

### Visual Studio 2022

1. Open solution in VS 2022
2. Full Razor tooling support out of the box
3. IntelliSense for Rndr components in markup

### Rider 2024

1. Open solution in Rider
2. Razor support enabled by default
3. Code completion for Rndr primitives
```

---

## Design Deliverables Checklist

- [ ] `specs/004-razor-migration/data-model.md` - Entity definitions with ERD
- [ ] `specs/004-razor-migration/contracts/IRazorComponent.cs` - Component interface
- [ ] `specs/004-razor-migration/contracts/IRenderTreeAdapter.cs` - Adapter interface
- [ ] `specs/004-razor-migration/contracts/IAsyncEventLoop.cs` - Async loop interface
- [ ] `specs/004-razor-migration/quickstart.md` - Hello world example

---

# Implementation Phases (Phase 2+)

*Note: Detailed task breakdown will be generated by `/speckit.tasks` command. This section provides high-level phasing only.*

## Phase 2: Core Async Infrastructure (2 weeks)

**Goal**: Refactor event loop and signal to support async operations without Blazor yet.

**Deliverables**:
- [ ] `AsyncEventLoop` implementing `IAsyncEventLoop`
- [ ] Refactor `DefaultEventLoop` to await async operations
- [ ] Add async timeout and cancellation support
- [ ] Update tests to handle async patterns
- [ ] Verify AOT compatibility (no new warnings)

**Acceptance**: Existing .tui components still work, event loop supports async (even if components don't use it yet).

---

## Phase 3: Blazor Integration (3 weeks)

**Goal**: Add ComponentBase, RenderTreeAdapter, and RazorComponentBase.

**Deliverables**:
- [ ] Add `Microsoft.AspNetCore.Components` package reference
- [ ] Implement `RazorComponentBase` with ViewContext integration
- [ ] Implement `RenderTreeToLayoutAdapter` with element mapping
- [ ] Update `TuiApp.MapView()` to support RazorComponentBase types
- [ ] Update Rndr.Razor generator to process .razor files
- [ ] Verify AOT compatibility with trim analysis
- [ ] Add AOT annotations where needed

**Acceptance**: Can create and render simple .razor component with sync lifecycle.

---

## Phase 4: Async Component Lifecycle (2 weeks)

**Goal**: Enable async lifecycle methods in Razor components.

**Deliverables**:
- [ ] Implement async lifecycle invocation in AsyncEventLoop
- [ ] Support async event handlers (button clicks)
- [ ] Integrate Signal<T> changes with StateHasChanged()
- [ ] Add timeout handling for async operations
- [ ] Add cancellation on navigation/disposal
- [ ] Update tests for async lifecycle

**Acceptance**: Components can use `OnInitializedAsync()`, load data asynchronously, handle async button clicks.

---

## Phase 5: Migration & Documentation (2 weeks)

**Goal**: Migrate example app and create migration guide.

**Deliverables**:
- [ ] Migrate MyFocusTui from .tui to .razor (Home.tui → Home.razor, Log.tui → Log.razor)
- [ ] Create migration guide with before/after samples
- [ ] Update README.md with Razor quick start
- [ ] Create video walkthrough of migration process
- [ ] Archive .tui examples as legacy reference
- [ ] Update all docs to reference .razor as primary

**Acceptance**: MyFocusTui fully functional in .razor, migration guide tested on at least 3 component types, documentation updated.

---

## Phase 6: AOT Verification & Polish (1 week)

**Goal**: Final AOT testing and performance validation.

**Deliverables**:
- [ ] Publish MyFocusTui with AOT on macOS (osx-arm64)
- [ ] Publish MyFocusTui with AOT on Windows (win-x64)
- [ ] Publish MyFocusTui with AOT on Linux (linux-x64)
- [ ] Verify zero trim warnings on all platforms
- [ ] Performance benchmark: async overhead < 5% (NFR-001)
- [ ] Performance benchmark: RenderTree adapter < 5ms (SC-007)
- [ ] Address any remaining AOT issues

**Acceptance**: SC-004 met (zero trim warnings), NFR-001 met (async overhead < 5%).

---

## Estimated Timeline

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 0: Research | 3-5 days | None |
| Phase 1: Design | 3-5 days | Phase 0 complete |
| Phase 2: Async Infrastructure | 2 weeks | Phase 1 complete |
| Phase 3: Blazor Integration | 3 weeks | Phase 2 complete |
| Phase 4: Async Lifecycle | 2 weeks | Phase 3 complete |
| Phase 5: Migration & Docs | 2 weeks | Phase 4 complete |
| Phase 6: AOT Verification | 1 week | Phase 5 complete |
| **Total** | **7-11 weeks** | Sequential |

---

## Success Metrics Tracking

| Metric | Target | Measurement | Status |
|--------|--------|-------------|--------|
| SC-001: IntelliSense response | < 500ms | Manual IDE testing | Pending |
| SC-002: Error squiggle latency | < 1s | Manual IDE testing | Pending |
| SC-003: Async init timeout | < 30s default | Configurable via RndrOptions | Pending |
| SC-004: AOT trim warnings | 0 warnings | dotnet publish output | Pending |
| SC-005: Migration time | < 10 min/component | Timed migration of MyFocusTui | Pending |
| SC-007: Adapter performance | < 5ms for 50 nodes | Benchmark test | Pending |
| SC-008: Test coverage | 90%+ async paths | Code coverage report | Pending |
| NFR-001: Async overhead | < 5% latency | Benchmark sync vs async render | Pending |

---

## Risk Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| AOT breaks with ComponentBase | HIGH | MEDIUM | Verify early in Phase 3, add annotations, document reflection usage |
| Async overhead too high | MEDIUM | LOW | Benchmark continuously, optimize hot paths, keep Signal<T> sync |
| Razor compiler incompatible | HIGH | LOW | Research in Phase 0, use standard Razor tooling, test incrementally |
| Migration too complex | MEDIUM | MEDIUM | Create automated migration script, provide detailed guide, prioritize common patterns |
| Breaking external users | LOW | N/A | No external users yet (per assumptions), but document breaking changes clearly |

---

## Next Steps

1. **Approve this plan** - Review with stakeholders, confirm architecture aligns with goals
2. **Execute Phase 0 Research** - Spend 3-5 days investigating Blazor integration patterns
3. **Review research findings** - Go/no-go decision point after research
4. **Execute Phase 1 Design** - Create detailed data models and contracts
5. **Generate tasks** - Run `/speckit.tasks` to break Phase 2+ into atomic implementation tasks
6. **Begin implementation** - Start with Phase 2 async infrastructure

---

**Plan Status**: READY FOR REVIEW  
**Constitution Compliance**: 2 principles require verification (IV: AOT, VIII: Breaking change) - both have mitigation strategies  
**Estimated Effort**: 7-11 weeks sequential development  
**Next Command**: Review plan, then execute Phase 0 research
