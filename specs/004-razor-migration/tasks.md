# Tasks: Razor Component Migration

**Input**: Design documents from `/specs/004-razor-migration/`
**Prerequisites**: plan.md, spec.md

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 [P] Add Microsoft.AspNetCore.Components package reference to src/Rndr/Rndr.csproj
- [X] T002 [P] Update Directory.Build.props to enable Razor SDK support globally
- [X] T003 [P] Configure .editorconfig for Razor file formatting rules

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create IAsyncEventLoop interface in src/Rndr/IAsyncEventLoop.cs
- [X] T005 Refactor IEventLoop.RunAsync to support async Task return type in src/Rndr/IEventLoop.cs
- [X] T006 Implement AsyncEventLoop class with timeout and cancellation support in src/Rndr/AsyncEventLoop.cs
- [X] T007 Add AsyncTimeout property to RndrOptions in src/Rndr/RndrOptions.cs (default 30 seconds)
- [X] T008 Update DefaultEventLoop to implement async await patterns in src/Rndr/DefaultEventLoop.cs
- [X] T009 [P] Add FakeAsyncEventLoop test helper in src/Rndr.Testing/FakeAsyncEventLoop.cs
- [X] T010 [P] Create AsyncTestHelpers utility class in src/Rndr.Testing/AsyncTestHelpers.cs
- [X] T011 Update RndrTestHost to support async component testing in src/Rndr.Testing/RndrTestHost.cs
- [ ] T012 Add unit tests for AsyncEventLoop in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [X] T013 Verify existing .tui components still work with async event loop (regression test)
- [X] T014 Run AOT publish test - verify zero new trim warnings: dotnet publish -p:PublishAot=true

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 4 - RenderTree to LayoutBuilder Adapter (Priority: P1) 🎯 BLOCKING

**Goal**: Convert Blazor RenderTreeBuilder instructions into Rndr LayoutBuilder node tree

**Independent Test**: Create simple Razor component with `<Column><Text>Hello</Text></Column>`, render through adapter, verify resulting node tree

### Implementation for User Story 4

- [ ] T015 Create IRenderTreeAdapter interface in specs/004-razor-migration/contracts/IRenderTreeAdapter.cs
- [ ] T016 Implement RenderTreeToLayoutAdapter class skeleton in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T017 [US4] Implement BeginCapture method to capture RenderTreeBuilder frames in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T018 [US4] Implement ProcessFrame method to handle individual RenderTreeFrame in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T019 [US4] Implement OpenElement method with element stack management in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T020 [US4] Implement CloseElement method with stack pop validation in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T021 [P] [US4] Implement MapElement for Column primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T022 [P] [US4] Implement MapElement for Row primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T023 [P] [US4] Implement MapElement for Panel primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T024 [P] [US4] Implement MapElement for Text primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T025 [P] [US4] Implement MapElement for Button primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T026 [P] [US4] Implement MapElement for Spacer primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T027 [P] [US4] Implement MapElement for Centered primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T028 [P] [US4] Implement MapElement for TextInput primitive in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T029 [US4] Implement AddAttribute method with type conversion logic in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T030 [US4] Implement AddContent method for text content handling in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T031 [US4] Implement BuildLayoutTree method to produce final node tree in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T032 [US4] Add error handling for unsupported element names with clear messages in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T033 [US4] Add error handling for invalid attribute types with expected type hints in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T034 [US4] Add validation for unclosed elements (stack depth check) in src/Rndr/Rendering/RenderTreeToLayoutAdapter.cs
- [ ] T035 [P] [US4] Create unit tests for RenderTreeToLayoutAdapter element mapping in tests/Rndr.Tests/RenderTreeAdapterTests.cs
- [ ] T036 [P] [US4] Create unit tests for nested element hierarchy in tests/Rndr.Tests/RenderTreeAdapterTests.cs
- [ ] T037 [P] [US4] Create unit tests for attribute type conversion in tests/Rndr.Tests/RenderTreeAdapterTests.cs
- [ ] T038 [P] [US4] Create unit tests for error handling (unsupported elements) in tests/Rndr.Tests/RenderTreeAdapterTests.cs
- [ ] T039 [US4] Create performance benchmark test for 50-node tree (target < 5ms) in tests/Rndr.Tests/RenderTreeAdapterTests.cs
- [ ] T040 [US4] Test conditional rendering (@if) with adapter in tests/Rndr.Tests/RenderTreeAdapterTests.cs
- [ ] T041 [US4] Test loop rendering (@foreach) with adapter in tests/Rndr.Tests/RenderTreeAdapterTests.cs

**Checkpoint**: RenderTree adapter complete and tested - Razor components can now be rendered

---

## Phase 4: User Story 2 - Async Component Lifecycle (Priority: P1)

**Goal**: Enable async initialization, data loading, and event handling in component lifecycle

**Independent Test**: Create component with OnInitializedAsync that loads data, verify it completes before rendering

### Implementation for User Story 2

- [ ] T042 Create IAsyncLifecycle internal interface for async lifecycle methods in src/Rndr/IAsyncLifecycle.cs
- [ ] T043 [US2] Add CancellationTokenSource for per-component cancellation in AsyncEventLoop in src/Rndr/AsyncEventLoop.cs
- [ ] T044 [US2] Implement InvokeComponentLifecycleAsync with timeout wrapper in src/Rndr/AsyncEventLoop.cs
- [ ] T045 [US2] Update RenderAsync to await OnInitializedAsync on first render in src/Rndr/AsyncEventLoop.cs
- [ ] T046 [US2] Update RenderAsync to await OnParametersSetAsync on parameter changes in src/Rndr/AsyncEventLoop.cs
- [ ] T047 [US2] Update RenderAsync to await OnAfterRenderAsync after BuildRenderTree in src/Rndr/AsyncEventLoop.cs
- [ ] T048 [US2] Implement async button click handler support (Task-returning methods) in src/Rndr/AsyncEventLoop.cs
- [ ] T049 [US2] Add cancellation on navigation: call _componentCts.Cancel() in src/Rndr/AsyncEventLoop.cs
- [ ] T050 [US2] Add cancellation on dispose: await grace period after cancel in src/Rndr/AsyncEventLoop.cs
- [ ] T051 [US2] Implement timeout exception with component name and method details in src/Rndr/AsyncEventLoop.cs
- [ ] T052 [US2] Integrate Signal<T> changes to trigger StateHasChanged and queue re-render in src/Rndr/Signal.cs
- [ ] T053 [P] [US2] Create unit tests for OnInitializedAsync invocation in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [ ] T054 [P] [US2] Create unit tests for OnParametersSetAsync invocation in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [ ] T055 [P] [US2] Create unit tests for OnAfterRenderAsync invocation in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [ ] T056 [P] [US2] Create unit tests for async timeout handling in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [ ] T057 [P] [US2] Create unit tests for cancellation token propagation in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [ ] T058 [P] [US2] Create unit tests for async exception propagation in tests/Rndr.Tests/AsyncEventLoopTests.cs
- [ ] T059 [US2] Create integration test for async data loading scenario in tests/Rndr.Tests/TestHostIntegrationTests.cs
- [ ] T060 [US2] Create integration test for async button click handler in tests/Rndr.Tests/TestHostIntegrationTests.cs
- [ ] T061 [US2] Test Signal<T> changes during async operation trigger re-render in tests/Rndr.Tests/SignalTests.cs
- [ ] T062 [US2] Performance benchmark: verify async overhead < 5% vs sync baseline in tests/Rndr.Tests/AsyncEventLoopTests.cs

**Checkpoint**: Async lifecycle complete - components can perform async operations

---

## Phase 5: User Story 1 - Component Authoring with IDE Support (Priority: P1) 🎯 MVP

**Goal**: Enable developers to write components in .razor files with full IDE tooling

**Independent Test**: Create new .razor file in VS Code, verify IntelliSense, syntax highlighting, and error squiggles work

### Implementation for User Story 1

- [ ] T063 Create IRazorComponent interface contract in specs/004-razor-migration/contracts/IRazorComponent.cs
- [ ] T064 [US1] Implement RazorComponentBase class inheriting from ComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T065 [US1] Add AttachContext method to RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T066 [US1] Expose Context property in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T067 [US1] Expose Navigation property shortcut in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T068 [US1] Expose Services property shortcut in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T069 [US1] Implement State<T> helper method in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T070 [US1] Implement StateGlobal<T> helper method in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T071 [US1] Override OnInitializedAsync with empty default implementation in src/Rndr/RazorComponentBase.cs
- [ ] T072 [US1] Override OnParametersSetAsync with empty default implementation in src/Rndr/RazorComponentBase.cs
- [ ] T073 [US1] Override OnAfterRenderAsync with empty default implementation in src/Rndr/RazorComponentBase.cs
- [ ] T074 [US1] Override BuildRenderTree with empty default (generated by Razor) in src/Rndr/RazorComponentBase.cs
- [ ] T075 [US1] Add [DynamicallyAccessedMembers] annotations for AOT on RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T076 [US1] Update TuiApp.MapView to accept RazorComponentBase types in src/Rndr/TuiApp.cs
- [ ] T077 [US1] Update TuiApp to call AttachContext before rendering Razor components in src/Rndr/TuiApp.cs
- [ ] T078 [US1] Integrate RenderTreeToLayoutAdapter into render pipeline in src/Rndr/TuiApp.cs
- [ ] T079 [US1] Update ViewContext to expose to Razor components (make public if needed) in src/Rndr/ViewContext.cs
- [ ] T080 [US1] Update Rndr.Razor.csproj to add Razor SDK: <Project Sdk="Microsoft.NET.Sdk.Razor"> in src/Rndr.Razor/Rndr.Razor.csproj
- [ ] T081 [US1] Update TuiRazorConfiguration to recognize .razor file extension in src/Rndr.Razor/TuiRazorConfiguration.cs
- [ ] T082 [US1] Update build targets to process .razor files instead of .tui in src/Rndr.Razor/build/
- [ ] T083 [US1] Update TuiSourceGenerator to process .razor files in src/Rndr.Razor/Generator/TuiSourceGenerator.cs
- [ ] T084 [US1] Create RazorCodeEmitter to emit ComponentBase-based code in src/Rndr.Razor/Generator/RazorCodeEmitter.cs
- [ ] T085 [US1] Evaluate TuiSyntaxParser - replace with Razor compiler or keep for now in src/Rndr.Razor/Parsing/TuiSyntaxParser.cs
- [ ] T086 [P] [US1] Create unit tests for RazorComponentBase lifecycle in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T087 [P] [US1] Create unit tests for State<T> helper methods in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T088 [P] [US1] Create unit tests for AttachContext integration in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T089 [US1] Create RazorCodeEmitter unit tests in tests/Rndr.Razor.Tests/RazorCodeEmitterTests.cs
- [ ] T090 [US1] Update existing generator tests for .razor syntax in tests/Rndr.Razor.Tests/
- [ ] T091 [US1] Create Hello.razor example component in examples/MyFocusTui/Pages/Hello.razor
- [ ] T092 [US1] Test Hello.razor renders correctly with AsyncEventLoop
- [ ] T093 [US1] Verify IntelliSense works in Hello.razor (manual IDE test - VS Code)
- [ ] T094 [US1] Verify syntax highlighting works in Hello.razor (manual IDE test - VS Code)
- [ ] T095 [US1] Verify error squiggles work for syntax errors (manual IDE test - VS Code)
- [ ] T096 [US1] Verify go-to-definition works from markup to @code (manual IDE test - VS Code)
- [ ] T097 [US1] Run AOT publish test with RazorComponentBase - verify zero trim warnings
- [ ] T098 [US1] Document any required AOT annotations in docs or inline comments

**Checkpoint**: .razor components work with full IDE support - MVP complete!

---

## Phase 6: User Story 3 - Razor Component Base Integration (Priority: P2)

**Goal**: Support standard Blazor component features: [Parameter], [CascadingParameter], @inject, StateHasChanged

**Independent Test**: Create component with [Parameter] properties, pass from parent, verify OnParametersSetAsync triggered

### Implementation for User Story 3

- [ ] T099 [P] [US3] Add support for [Parameter] attribute binding in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T100 [P] [US3] Add support for [CascadingParameter] attribute in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T101 [US3] Implement StateHasChanged to queue re-render via event loop in src/Rndr/RazorComponentBase.cs
- [ ] T102 [US3] Add support for @inject directive (DI integration) in RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T103 [US3] Implement IDisposable.Dispose for component cleanup in src/Rndr/RazorComponentBase.cs
- [ ] T104 [US3] Update AsyncEventLoop to call Dispose on component navigation in src/Rndr/AsyncEventLoop.cs
- [ ] T105 [P] [US3] Create unit tests for [Parameter] binding in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T106 [P] [US3] Create unit tests for [CascadingParameter] in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T107 [P] [US3] Create unit tests for StateHasChanged behavior in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T108 [P] [US3] Create unit tests for @inject DI integration in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T109 [US3] Create unit tests for IDisposable.Dispose invocation in tests/Rndr.Tests/RazorComponentBaseTests.cs
- [ ] T110 [US3] Create integration test for parent-child parameter passing in tests/Rndr.Tests/TestHostIntegrationTests.cs
- [ ] T111 [US3] Create integration test for cascading values in tests/Rndr.Tests/TestHostIntegrationTests.cs

**Checkpoint**: Standard Blazor component features work in Rndr

---

## Phase 7: User Story 6 - AOT Compatibility Preservation (Priority: P1)

**Goal**: Ensure Native AOT publishing works with zero trim warnings

**Independent Test**: Run `dotnet publish -p:PublishAot=true` on MyFocusTui, verify zero warnings and successful execution

### Implementation for User Story 6

- [ ] T112 [P] [US6] Test AOT publish on macOS (osx-arm64) for MyFocusTui example
- [ ] T113 [P] [US6] Test AOT publish on Windows (win-x64) for MyFocusTui example
- [ ] T114 [P] [US6] Test AOT publish on Linux (linux-x64) for MyFocusTui example
- [ ] T115 [US6] Analyze all trim warnings and categorize by severity
- [ ] T116 [US6] Add [DynamicallyAccessedMembers] annotations where needed in src/Rndr/
- [ ] T117 [US6] Add [RequiresUnreferencedCode] warnings if reflection unavoidable in src/Rndr/
- [ ] T118 [US6] Test AOT-published binary runtime behavior on all platforms
- [ ] T119 [US6] Verify published binary size < 15MB for basic TUI apps
- [ ] T120 [US6] Document reflection usage and verify not in hot rendering paths in docs/AOT-COMPATIBILITY.md
- [ ] T121 [US6] Create AOT troubleshooting guide with common issues and solutions in docs/AOT-COMPATIBILITY.md
- [ ] T122 [US6] Update CI/CD to include AOT publish verification step

**Checkpoint**: AOT compatibility verified - SC-004 met (zero trim warnings)

---

## Phase 8: User Story 5 - Migration Path (Priority: P2)

**Goal**: Provide migration guide and tools for converting .tui components to .razor

**Independent Test**: Follow migration guide to convert Home.tui and Log.tui, verify identical functionality

### Implementation for User Story 5

- [ ] T123 [US5] Manually migrate Home.tui to Home.razor in examples/MyFocusTui/Pages/Home.razor
- [ ] T124 [US5] Manually migrate Log.tui to Log.razor in examples/MyFocusTui/Pages/Log.razor
- [ ] T125 [US5] Update Program.cs to register .razor components in examples/MyFocusTui/Program.cs
- [ ] T126 [US5] Test migrated MyFocusTui app - verify identical functionality to .tui version
- [ ] T127 [US5] Test migrated MyFocusTui app - verify identical visual output to .tui version
- [ ] T128 [US5] Measure migration time for Home.razor (target < 10 minutes)
- [ ] T129 [US5] Measure migration time for Log.razor (target < 10 minutes)
- [ ] T130 [US5] Create migration guide with step-by-step instructions in specs/004-razor-migration/MIGRATION-GUIDE.md
- [ ] T131 [P] [US5] Document .tui to .razor file rename pattern in migration guide
- [ ] T132 [P] [US5] Document OnInit to OnInitializedAsync conversion in migration guide
- [ ] T133 [P] [US5] Document Build method removal (use Razor markup) in migration guide
- [ ] T134 [P] [US5] Document State<T> initialization pattern in migration guide
- [ ] T135 [P] [US5] Document event handler conversion pattern in migration guide
- [ ] T136 [US5] Create before/after code samples for all .tui patterns in migration guide
- [ ] T137 [US5] Document common pitfalls (async/await, Signal initialization) in migration guide
- [ ] T138 [US5] Create migration checklist template in migration guide
- [ ] T139 [US5] Update README.md with Razor quick start section in README.md
- [ ] T140 [US5] Update README.md to reference .razor as primary component format in README.md
- [ ] T141 [US5] Archive .tui examples as legacy reference in examples/Legacy-TUI/
- [ ] T142 [US5] Create video walkthrough of migration process (optional)

**Checkpoint**: Migration path documented and tested - developers can migrate existing apps

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements, documentation, and verification

- [ ] T143 [P] Update all XML documentation comments for new public APIs in src/Rndr/
- [ ] T144 [P] Update all XML documentation comments for RazorComponentBase in src/Rndr/RazorComponentBase.cs
- [ ] T145 [P] Create quickstart.md with Hello World example in specs/004-razor-migration/quickstart.md
- [ ] T146 [P] Create data-model.md with entity definitions and ERD in specs/004-razor-migration/data-model.md
- [ ] T147 [P] Create research.md documenting Blazor integration findings in specs/004-razor-migration/research.md
- [ ] T148 Update CHANGELOG.md with breaking changes and migration notes in CHANGELOG.md
- [ ] T149 Create upgrade guide for existing Rndr applications in docs/UPGRADE-TO-RAZOR.md
- [ ] T150 Performance optimization: profile RenderTreeToLayoutAdapter hot paths
- [ ] T151 Performance optimization: reduce allocations in async event loop
- [ ] T152 Performance benchmark: final async overhead measurement (verify < 5%)
- [ ] T153 Performance benchmark: final adapter performance (verify < 5ms for 50 nodes)
- [ ] T154 Code cleanup: remove deprecated TuiComponentBase references
- [ ] T155 Code cleanup: remove legacy .tui generator code paths
- [ ] T156 Security review: async timeout configurations and cancellation
- [ ] T157 [P] Run full test suite and verify 90%+ coverage on async paths
- [ ] T158 [P] Run code coverage report and identify gaps
- [ ] T159 Update all NuGet package metadata for 2.0 release
- [ ] T160 Create release notes for Razor migration feature

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 4 (Phase 3)**: Depends on Foundational (Phase 2) - BLOCKING for all other stories
- **User Story 2 (Phase 4)**: Depends on Foundational (Phase 2) - Can run parallel with US4 but needs US4 for integration
- **User Story 1 (Phase 5)**: Depends on US4 (Phase 3) and US2 (Phase 4) - Primary MVP deliverable
- **User Story 3 (Phase 6)**: Depends on US1 (Phase 5) - Enhancement of MVP
- **User Story 6 (Phase 7)**: Can run parallel with any phase, but comprehensive test needs US1 complete
- **User Story 5 (Phase 8)**: Depends on US1 (Phase 5) - Migration requires working .razor support
- **Polish (Phase 9)**: Depends on all desired user stories being complete

### Critical Path

1. **Setup** (T001-T003) → 
2. **Foundational** (T004-T014) → 
3. **US4: RenderTree Adapter** (T015-T041) → 
4. **US2: Async Lifecycle** (T042-T062) + **US1: IDE Support** (T063-T098) → 
5. **US3: Blazor Integration** (T099-T111) → 
6. **US5: Migration** (T123-T142) + **US6: AOT** (T112-T122) → 
7. **Polish** (T143-T160)

### Parallel Opportunities

**Within Phase 1 (Setup)**:
- T001, T002, T003 can all run in parallel

**Within Phase 2 (Foundational)**:
- T009, T010 can run in parallel with T004-T008
- T012 must wait for T006

**Within Phase 3 (US4)**:
- T021-T028 (MapElement implementations) can all run in parallel
- T035-T038 (unit tests) can run in parallel after T034 complete

**Within Phase 4 (US2)**:
- T053-T058 (lifecycle tests) can run in parallel
- T059-T061 (integration tests) can run in parallel

**Within Phase 5 (US1)**:
- T086-T088 (RazorComponentBase tests) can run in parallel

**Within Phase 6 (US3)**:
- T099, T100 (parameter support) can run in parallel
- T105-T108 (tests) can run in parallel

**Within Phase 7 (US6)**:
- T112, T113, T114 (platform-specific AOT tests) can run in parallel

**Within Phase 8 (US5)**:
- T131-T135 (migration guide sections) can run in parallel

**Within Phase 9 (Polish)**:
- T143, T144, T145, T146, T147 (documentation) can all run in parallel
- T157, T158 (test coverage) can run in parallel

### Parallelization Example

After Foundational phase completes, these can run concurrently:
```
├─ US4 Developer A: RenderTree adapter (T015-T041)
├─ US2 Developer B: Async lifecycle (T042-T062)
└─ Documentation Team: Research/design docs (T145-T147)
```

After US4 completes:
```
├─ US1 Developer A: RazorComponentBase (T063-T098)
├─ US6 Developer B: AOT testing (T112-T122)
└─ Documentation Team: Migration guide drafting
```

---

## Implementation Strategy

### MVP First (Minimum Viable Product)

1. Complete **Phase 1: Setup** (T001-T003)
2. Complete **Phase 2: Foundational** (T004-T014) - BLOCKING
3. Complete **Phase 3: US4 - RenderTree Adapter** (T015-T041) - BLOCKING
4. Complete **Phase 4: US2 - Async Lifecycle** (T042-T062)
5. Complete **Phase 5: US1 - IDE Support** (T063-T098)
6. **STOP and VALIDATE**: Test .razor components end-to-end
7. Deploy/demo MVP with working .razor components

**MVP Definition**: Developers can write .razor components with full IDE support, async lifecycle methods work, components render correctly to terminal.

### Full Feature Delivery

After MVP validation:
1. **Phase 6: US3 - Blazor Integration** (T099-T111) - Standard component features
2. **Phase 7: US6 - AOT Verification** (T112-T122) - Constitutional compliance
3. **Phase 8: US5 - Migration Path** (T123-T142) - User migration support
4. **Phase 9: Polish** (T143-T160) - Production readiness

### Incremental Delivery Strategy

- **Milestone 1**: Foundation + Adapter (Phases 1-3) → Internal demo
- **Milestone 2**: Async + IDE Support (Phases 4-5) → MVP demo
- **Milestone 3**: Blazor Integration (Phase 6) → Enhanced demo
- **Milestone 4**: Migration + AOT (Phases 7-8) → Production candidate
- **Milestone 5**: Polish (Phase 9) → Production release

---

## Task Summary

### Total Tasks: 160

### Tasks by User Story:
- **Setup**: 3 tasks (T001-T003)
- **Foundational**: 11 tasks (T004-T014)
- **US4 (RenderTree Adapter)**: 27 tasks (T015-T041)
- **US2 (Async Lifecycle)**: 21 tasks (T042-T062)
- **US1 (IDE Support)**: 36 tasks (T063-T098)
- **US3 (Blazor Integration)**: 13 tasks (T099-T111)
- **US6 (AOT Compatibility)**: 11 tasks (T112-T122)
- **US5 (Migration Path)**: 20 tasks (T123-T142)
- **Polish**: 18 tasks (T143-T160)

### Tasks by Priority:
- **P1 (Critical Path)**: US4 (27) + US2 (21) + US1 (36) + US6 (11) = 95 tasks
- **P2 (Enhancement)**: US3 (13) + US5 (20) = 33 tasks
- **Infrastructure**: Setup (3) + Foundational (11) + Polish (18) = 32 tasks

### Parallelizable Tasks: 47
- Element mapping: 8 tasks (T021-T028)
- Unit tests: 15+ tasks across all phases
- Documentation: 12 tasks (T131-T135, T143-T147)
- Platform-specific: 3 tasks (T112-T114)
- Miscellaneous: 9 tasks

### Estimated Timeline

| Phase | Tasks | Estimated Duration | Dependencies |
|-------|-------|-------------------|--------------|
| Phase 1: Setup | 3 | 1 day | None |
| Phase 2: Foundational | 11 | 1 week | Phase 1 |
| Phase 3: US4 (Adapter) | 27 | 1.5 weeks | Phase 2 |
| Phase 4: US2 (Async) | 21 | 1 week | Phase 2 |
| Phase 5: US1 (IDE) | 36 | 2 weeks | Phases 3-4 |
| Phase 6: US3 (Blazor) | 13 | 1 week | Phase 5 |
| Phase 7: US6 (AOT) | 11 | 1 week | Phase 5 (parallel) |
| Phase 8: US5 (Migration) | 20 | 1 week | Phase 5 |
| Phase 9: Polish | 18 | 1 week | Phases 6-8 |
| **Total** | **160** | **7-9 weeks** | Sequential with parallelization |

---

## Notes

- Tasks marked [P] can run in parallel with other [P] tasks in the same phase
- Tasks marked [US#] belong to specific user story for traceability
- Foundational phase (T004-T014) BLOCKS all user story work
- US4 (RenderTree Adapter) BLOCKS US1 (IDE Support) - critical path
- AOT verification (US6) can run parallel with other phases but needs comprehensive test at end
- Stop at any checkpoint to validate independently
- Commit after completing logical task groups (related functionality)
- All async paths must have unit tests for 90%+ coverage requirement

---

## Success Criteria Verification

Map tasks to spec success criteria:

- **SC-001** (IntelliSense < 500ms): T093 - Manual IDE test
- **SC-002** (Error squiggles < 1s): T095 - Manual IDE test
- **SC-003** (Async timeout < 30s): T007 - RndrOptions configuration
- **SC-004** (AOT warnings = 0): T097, T112-T114, T121 - AOT publish tests
- **SC-005** (Migration < 10min): T128-T129 - Timed migration
- **SC-007** (Adapter < 5ms): T039 - Performance benchmark
- **SC-008** (Test coverage 90%+): T157-T158 - Coverage report
- **NFR-001** (Async overhead < 5%): T062, T152 - Performance benchmark

All success criteria mapped to verifiable tasks!
