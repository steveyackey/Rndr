# Tasks: Rndr TUI Framework

**Input**: Design documents from `/specs/001-rndr-tui-framework/`  
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/layout-api.md ✓, quickstart.md ✓

**Tests**: Included - spec.md references testing requirements (FR-024, FR-025, SC-006) and plan.md specifies xUnit testing.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Based on plan.md project structure:

- **Core library**: `src/Rndr/`
- **Razor integration**: `src/Rndr.Razor/`
- **Test helpers**: `src/Rndr.Testing/`
- **Sample app**: `src/Rndr.Samples.MyFocusTui/`
- **Unit tests**: `tests/Rndr.Tests/`
- **Razor tests**: `tests/Rndr.Razor.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and solution structure

- [x] T001 Create .NET solution file `Rndr.sln` at repository root
- [x] T002 Create `src/Rndr/Rndr.csproj` with .NET 8, C# 12, nullable enabled, and package references for Microsoft.Extensions.Hosting, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging, Microsoft.Extensions.Options
- [x] T003 [P] Create `src/Rndr.Razor/Rndr.Razor.csproj` with Razor SDK and project reference to Rndr
- [x] T004 [P] Create `src/Rndr.Testing/Rndr.Testing.csproj` with project reference to Rndr
- [x] T005 [P] Create `src/Rndr.Samples.MyFocusTui/Rndr.Samples.MyFocusTui.csproj` with project references to Rndr and Rndr.Razor
- [x] T006 [P] Create `tests/Rndr.Tests/Rndr.Tests.csproj` with xUnit and project reference to Rndr and Rndr.Testing
- [x] T007 [P] Create `tests/Rndr.Razor.Tests/Rndr.Razor.Tests.csproj` with xUnit and project reference to Rndr.Razor
- [x] T008 Create Directory.Build.props at repository root with shared properties (LangVersion, ImplicitUsings, TreatWarningsAsErrors)
- [x] T009 [P] Create `.editorconfig` with C# coding conventions
- [x] T010 [P] Create `global.json` pinning .NET SDK version

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Core Abstractions

- [x] T011 Create `src/Rndr/IClock.cs` with `DateTime Now { get; }` interface
- [x] T012 [P] Create `src/Rndr/SystemClock.cs` implementing IClock with `DateTime.Now`
- [x] T013 [P] Create `src/Rndr/Input/KeyEvent.cs` with record `KeyEvent(ConsoleKey Key, char KeyChar, ConsoleModifiers Modifiers)`
- [x] T014 [P] Create `src/Rndr/Input/IInputSource.cs` with `KeyEvent ReadKey(bool intercept)` interface
- [x] T015 [P] Create `src/Rndr/Input/SystemInputSource.cs` implementing IInputSource wrapping Console.ReadKey
- [x] T016 [P] Create `src/Rndr/Rendering/IConsoleAdapter.cs` with WindowWidth, WindowHeight, Clear, WriteAt, HideCursor, ShowCursor
- [x] T017 [P] Create `src/Rndr/Rendering/SystemConsoleAdapter.cs` implementing IConsoleAdapter using ANSI escape sequences

### Layout Node Types

- [x] T018 Create `src/Rndr/Layout/NodeKind.cs` with enum (Column, Row, Panel, Text, Button, TextInput, Spacer, Centered)
- [x] T019 [P] Create `src/Rndr/Layout/TextAlign.cs` with enum (Left, Center, Right)
- [x] T020 [P] Create `src/Rndr/Layout/NodeStyle.cs` with Padding, Gap, Align, Bold, Accent, Faint properties
- [x] T021 Create `src/Rndr/Layout/Node.cs` with abstract base class (Kind, Children, Style properties)
- [x] T022 [P] Create `src/Rndr/Layout/ColumnNode.cs` extending Node
- [x] T023 [P] Create `src/Rndr/Layout/RowNode.cs` extending Node
- [x] T024 [P] Create `src/Rndr/Layout/PanelNode.cs` extending Node with Title property
- [x] T025 [P] Create `src/Rndr/Layout/TextNode.cs` extending Node with Content property
- [x] T026 [P] Create `src/Rndr/Layout/ButtonNode.cs` extending Node with Label, OnClick, IsPrimary, Width properties
- [x] T027 [P] Create `src/Rndr/Layout/TextInputNode.cs` extending Node with Value, OnChanged, Placeholder properties
- [x] T028 [P] Create `src/Rndr/Layout/SpacerNode.cs` extending Node with Weight property
- [x] T029 [P] Create `src/Rndr/Layout/CenteredNode.cs` extending Node

### State Primitives

- [x] T030 Create `src/Rndr/ISignal.cs` with `object? UntypedValue { get; set; }` interface
- [x] T031 Create `src/Rndr/Signal.cs` with generic Signal<T> implementing ISignal with Value property and change detection
- [x] T032 Create `src/Rndr/IStateStore.cs` with `Signal<T> GetOrCreate<T>(string scopeKey, string key, Func<T> initialFactory)` interface
- [x] T033 Create `src/Rndr/InMemoryStateStore.cs` implementing IStateStore with Dictionary<(string Scope, string Key), ISignal>

### Diagnostics

- [x] T034 Create `src/Rndr/Diagnostics/RndrDiagnostics.cs` with static ActivitySource "Rndr.Core" and Meter "Rndr.Core"

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Build a Simple Counter App (Priority: P1) 🎯 MVP

**Goal**: Developers can create and run a working counter TUI with text, buttons, and reactive state

**Independent Test**: Create a project, write a counter view with increment/decrement, run in terminal. Counter updates on button press, Q quits cleanly.

### Tests for User Story 1

- [x] T035 [P] [US1] Create `tests/Rndr.Tests/SignalTests.cs` with tests for Signal<T> value changes and equality comparison
- [x] T036 [P] [US1] Create `tests/Rndr.Tests/StateStoreTests.cs` with tests for InMemoryStateStore GetOrCreate with scopes
- [x] T037 [P] [US1] Create `tests/Rndr.Tests/LayoutBuilderTests.cs` with tests for building Column, Row, Panel, Text, Button nodes

### Implementation for User Story 1

- [x] T038 [US1] Create `src/Rndr/Layout/LayoutBuilder.cs` with Column, Row, Padding, Gap methods returning node tree via Build()
- [x] T039 [US1] Create `src/Rndr/Layout/ColumnBuilder.cs` with Text, Panel, Button, Spacer, Centered, Row, Gap, Padding, AlignCenter methods
- [x] T040 [US1] Create `src/Rndr/Layout/RowBuilder.cs` with Text, Button, Spacer, Column, Gap, AlignCenter methods
- [x] T041 [US1] Create `src/Rndr/Rendering/ITuiRenderer.cs` with `void Render(IReadOnlyList<Node> rootNodes)` interface
- [x] T042 [US1] Create `src/Rndr/Rendering/ConsoleRenderer.cs` implementing ITuiRenderer with basic ANSI layout and panel rendering
- [x] T043 [US1] Create `src/Rndr/ViewDefinition.cs` with Title, Use, OnKey fluent methods
- [x] T044 [US1] Create `src/Rndr/ViewContext.cs` with Services, Navigation, Logger, Route, State<T>, StateGlobal<T>
- [x] T045 [US1] Create `src/Rndr/GlobalContext.cs` with Navigation and Application properties
- [x] T046 [US1] Create `src/Rndr/Navigation/NavigationContext.cs` with CurrentRoute, Navigate, Back, Replace methods (stub implementation)
- [x] T047 [US1] Create `src/Rndr/IEventLoop.cs` with `Task RunAsync(TuiApp app, CancellationToken cancellationToken)` interface
- [x] T048 [US1] Create `src/Rndr/DefaultEventLoop.cs` implementing IEventLoop with render-input-dispatch cycle
- [x] T049 [US1] Create `src/Rndr/TuiApplication.cs` with static `CreateBuilder(string[] args)` returning TuiAppBuilder
- [x] T050 [US1] Create `src/Rndr/TuiAppBuilder.cs` wrapping HostApplicationBuilder with Services, Build() returning TuiApp
- [x] T051 [US1] Create `src/Rndr/TuiApp.cs` with Services, MapView(route, configure), OnGlobalKey, RunAsync, Quit methods
- [x] T052 [US1] Create `src/Rndr/Extensions/RndrServiceCollectionExtensions.cs` with AddRndr() registering core services (renderer, input, state store, event loop)
- [x] T053 [US1] Add focus tracking to DefaultEventLoop for Tab navigation between buttons in `src/Rndr/DefaultEventLoop.cs`
- [x] T054 [US1] Add Enter/Space handling to activate focused button in `src/Rndr/DefaultEventLoop.cs`

### Smoke Test for User Story 1

- [x] T055 [US1] Create `tests/Rndr.Tests/ConsoleRendererTests.cs` with smoke test using FakeConsoleAdapter to verify panel rendering output

**Checkpoint**: Counter app compiles, runs, displays UI, responds to input, quits cleanly

---

## Phase 4: User Story 2 - Navigate Between Multiple Views (Priority: P1)

**Goal**: Developers can create multi-screen apps with button and keyboard navigation between views

**Independent Test**: Create home and details views with navigation buttons, run app, navigate back and forth. Global keys (H, Q) work from any view.

### Tests for User Story 2

- [x] T056 [P] [US2] Create `tests/Rndr.Tests/NavigationStateTests.cs` with tests for stack push/pop/replace operations

### Implementation for User Story 2

- [x] T057 [US2] Create `src/Rndr/Navigation/INavigationState.cs` with CurrentRoute and Stack properties
- [x] T058 [US2] Create `src/Rndr/Navigation/NavigationState.cs` implementing INavigationState with Stack<string>
- [x] T059 [US2] Update `src/Rndr/Navigation/NavigationContext.cs` to delegate to NavigationState for Navigate, Back, Replace
- [x] T060 [US2] Update `src/Rndr/TuiApp.cs` to store Dictionary<string, ViewRegistration> for route mappings
- [x] T061 [US2] Update `src/Rndr/TuiApp.cs` to implement OnGlobalKey handler registration and dispatch
- [x] T062 [US2] Update `src/Rndr/DefaultEventLoop.cs` to call global handlers before view handlers on key events
- [x] T063 [US2] Update `src/Rndr/DefaultEventLoop.cs` to re-render on navigation changes
- [x] T064 [US2] Add Activity tracing for navigation in `src/Rndr/Navigation/NavigationState.cs` using RndrDiagnostics.ActivitySource

**Checkpoint**: Navigate between views, global keys work, back button returns to previous view

---

## Phase 5: User Story 3 - Share State Across Views (Priority: P2)

**Goal**: Application state persists across navigation using global state, while route-scoped state is isolated per route

**Independent Test**: Set global state in home view, navigate to details, verify state persisted. Create route-scoped state, navigate away and back, verify preserved.

### Implementation for User Story 3

- [x] T065 [US3] Update `src/Rndr/InMemoryStateStore.cs` to support "global" scope key for StateGlobal
- [x] T066 [US3] Update `src/Rndr/ViewContext.cs` to use route as scopeKey for State() and "global" for StateGlobal()
- [x] T067 [US3] Verify state isolation in `tests/Rndr.Tests/StateStoreTests.cs` with route-scoped vs global scoped state tests

**Checkpoint**: Global state persists across navigation, route state is isolated

---

## Phase 6: User Story 4 - Create Vue-like Single-File Components (Priority: P2)

**Goal**: Developers can author views using .tui files with markup and @code blocks that compile to C# classes

**Independent Test**: Create a .tui file with Counter markup and @code block, build project, run app. Component renders and state is reactive.

### Implementation for User Story 4

- [x] T068 [US4] Create `src/Rndr/TuiComponentBase.cs` with Context, AttachContext, OnInit, State<T>, StateGlobal<T>, abstract Build(LayoutBuilder)
- [x] T069 [US4] Update `src/Rndr/TuiApp.cs` to add MapView(string route, Type viewComponentType) overload
- [x] T070 [US4] Update `src/Rndr/DefaultEventLoop.cs` to instantiate TuiComponentBase types via DI and call AttachContext/Build
- [x] T071 [US4] Create `src/Rndr.Razor/build/Rndr.Razor.targets` with MSBuild targets for <TuiComponent Include="**/*.tui" />
- [x] T072 [US4] Create `src/Rndr.Razor/TuiRazorConfiguration.cs` with Razor configuration for @view directive
- [x] T073 [US4] Implement tag-to-builder mapping in Razor generator: Column→LayoutBuilder.Column, Row, Panel, Text, Button, Spacer, Centered
- [x] T074 [US4] Implement attribute mapping in Razor generator: Padding, Gap, Bold, Accent, Faint, Align, Width, Primary, OnClick

### Tests for User Story 4

- [x] T075 [US4] Create `tests/Rndr.Razor.Tests/TuiComponentGenerationTests.cs` verifying .tui compiles to TuiComponentBase with correct Build() output

**Checkpoint**: .tui files compile to components, sample component runs identically to C# version

---

## Phase 7: User Story 5 - Apply Consistent Visual Theming (Priority: P3)

**Goal**: Applications have professional appearance with configurable themes for colors, borders, and spacing

**Independent Test**: Configure custom theme with magenta accent, render panel with accent text, verify colors match theme.

### Implementation for User Story 5

- [x] T076 [US5] Create `src/Rndr/BorderStyle.cs` with enum (Square, Rounded)
- [x] T077 [P] [US5] Create `src/Rndr/PanelTheme.cs` with BorderStyle property defaulting to Rounded
- [x] T078 [US5] Create `src/Rndr/RndrTheme.cs` with AccentColor, TextColor, MutedTextColor, ErrorColor, Panel, SpacingUnit
- [x] T079 [US5] Update `src/Rndr/Extensions/RndrServiceCollectionExtensions.cs` with AddRndrTheme(Action<RndrTheme>? configure) method
- [x] T080 [US5] Update `src/Rndr/Rendering/ConsoleRenderer.cs` to use RndrTheme for colors and border characters
- [x] T081 [US5] Implement rounded border chars (╭╮╰╯─│) and square border chars (┌┐└┘─│) in ConsoleRenderer

**Checkpoint**: Default theme applies, custom themes work, borders render with correct style

---

## Phase 8: User Story 6 - Test Components Without Terminal (Priority: P3)

**Goal**: Developers can write automated tests for components without requiring a real terminal

**Independent Test**: Write xUnit test that builds component via RndrTestHost, inspects node tree, modifies state, verifies tree updates.

### Implementation for User Story 6

- [x] T082 [US6] Create `src/Rndr.Testing/FakeConsoleAdapter.cs` implementing IConsoleAdapter with configurable size and recorded writes
- [x] T083 [P] [US6] Create `src/Rndr.Testing/FakeInputSource.cs` implementing IInputSource with Queue<KeyEvent> for scripted input
- [x] T084 [P] [US6] Create `src/Rndr.Testing/FakeClock.cs` implementing IClock with settable Now property
- [x] T085 [US6] Create `src/Rndr.Testing/RndrTestHost.cs` with BuildComponent<T>(IServiceProvider?, IStateStore?) returning (nodes, context)
- [x] T086 [US6] Create `src/Rndr.Testing/NodeExtensions.cs` with FindButton(label), FindText(contains) helpers for test assertions

### Tests for User Story 6

- [x] T087 [US6] Create integration test in `tests/Rndr.Tests/TestHostIntegrationTests.cs` demonstrating RndrTestHost usage

**Checkpoint**: Components testable in isolation, fake adapters enable deterministic testing

---

## Phase 9: Sample Application

**Purpose**: Reference implementation demonstrating all features

### MyFocusTui Sample App

- [x] T088 Create `src/Rndr.Samples.MyFocusTui/Models/ActionEntry.cs` with Timestamp and Message properties
- [x] T089 [P] Create `src/Rndr.Samples.MyFocusTui/Models/FocusState.cs` with CurrentTodo string and Log list
- [x] T090 Create `src/Rndr.Samples.MyFocusTui/Pages/Home.tui` with todo display, Start/Finish/Edit/Clear buttons, recent log preview
- [x] T091 [P] Create `src/Rndr.Samples.MyFocusTui/Pages/Log.tui` with full log display and Back button
- [x] T092 Create `src/Rndr.Samples.MyFocusTui/Program.cs` with TuiApplication setup, MapView for Home and Log, global key handlers (Q, H, L)

**Checkpoint**: Sample app runs demonstrating navigation, global state, .tui components, theming

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Runtime options, documentation, and final polish

- [x] T093 Create `src/Rndr/RndrOptions.cs` with EnableDoubleBuffering, UseAnsiColors, IdleFrameDelay properties
- [x] T094 Update `src/Rndr/Extensions/RndrServiceCollectionExtensions.cs` to register RndrOptions via IOptions pattern
- [x] T095 Update `src/Rndr/DefaultEventLoop.cs` to respect RndrOptions settings
- [x] T096 Add Activity tracing for render cycle in `src/Rndr/DefaultEventLoop.cs` using RndrDiagnostics.ActivitySource
- [x] T097 Add metrics (frames_rendered counter, render_duration_ms histogram) in `src/Rndr/DefaultEventLoop.cs` using RndrDiagnostics.Meter
- [x] T098 Create `README.md` at repository root with goals, quickstart, code samples, and sample app description
- [x] T099 Validate quickstart.md examples compile and run correctly
- [x] T100 Verify AOT compilation of sample app with `dotnet publish -c Release` without trim warnings

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1 and US2 are both P1 but US2 builds on US1's runtime
  - US3 and US4 are P2 and can run after US2
  - US5 and US6 are P3 and can run after foundational
- **Sample App (Phase 9)**: Depends on US1-US4 completion
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

```
Phase 2 (Foundational)
         │
         ▼
    US1 (P1) ◄── MVP
         │
         ▼
    US2 (P1)
         │
    ┌────┴────┐
    ▼         ▼
  US3 (P2)  US4 (P2)
    │         │
    └────┬────┘
         ▼
    ┌────┴────┐
    ▼         ▼
  US5 (P3)  US6 (P3)
         │
         ▼
    Sample App
         │
         ▼
      Polish
```

### Within Each User Story

- Tests SHOULD be written first (TDD approach)
- Models/primitives before services
- Services before event loop integration
- Core implementation before polish

### Parallel Opportunities

**Setup Phase**:

```bash
# Parallel project creation (T003-T007, T009-T010)
T003: Rndr.Razor.csproj
T004: Rndr.Testing.csproj
T005: MyFocusTui.csproj
T006: Rndr.Tests.csproj
T007: Rndr.Razor.Tests.csproj
T009: .editorconfig
T010: global.json
```

**Foundational Phase**:

```bash
# Parallel abstractions (T012-T017)
T012: SystemClock.cs
T013: KeyEvent.cs
T014: IInputSource.cs
T015: SystemInputSource.cs
T016: IConsoleAdapter.cs
T017: SystemConsoleAdapter.cs

# Parallel node types (T019-T029)
T019-T029: All node type files
```

**User Story 1**:

```bash
# Parallel tests (T035-T037)
T035: SignalTests.cs
T036: StateStoreTests.cs
T037: LayoutBuilderTests.cs
```

**User Story 6**:

```bash
# Parallel fake implementations (T082-T084)
T082: FakeConsoleAdapter.cs
T083: FakeInputSource.cs
T084: FakeClock.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Run counter app in terminal
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Counter works → **MVP!**
3. Add User Story 2 → Multi-view navigation works
4. Add User Story 3 → Global state works
5. Add User Story 4 → .tui files work
6. Add User Story 5 → Theming works
7. Add User Story 6 → Testing helpers work
8. Complete Sample App → Full demo ready
9. Polish → Production ready

### Validation Points

| Checkpoint | Validation |
|------------|------------|
| After Phase 2 | Solution builds, all interfaces defined |
| After US1 | Counter app runs in terminal |
| After US2 | Navigate between two views with global keys |
| After US3 | State persists across navigation |
| After US4 | .tui file compiles and renders |
| After US5 | Custom theme colors appear |
| After US6 | Unit test passes without terminal |
| After Sample | MyFocusTui demonstrates all features |
| After Polish | AOT publish succeeds, docs complete |

---

## Notes

- [P] tasks = different files, no dependencies within the phase
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- ANSI escape sequences: `\x1b[2J` (clear), `\x1b[H` (home), `\x1b[?25l/h` (cursor hide/show)
- Box drawing: Rounded `╭╮╰╯─│`, Square `┌┐└┘─│`

