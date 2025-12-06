# Tasks: .tui Razor Source Generator

**Input**: Design documents from `/specs/002-tui-razor-generator/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/generator-api.md

**Tests**: Test tasks are included to validate each user story independently.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Core framework**: `src/Rndr/`
- **Razor generator**: `src/Rndr.Razor/`
- **Sample app**: `src/Rndr.Samples.MyFocusTui/`
- **Tests**: `tests/Rndr.Razor.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and generator structure

- [X] T001 Create Generator directory structure in src/Rndr.Razor/Generator/
- [X] T002 Create Parsing directory structure in src/Rndr.Razor/Parsing/
- [X] T003 Update src/Rndr.Razor/Rndr.Razor.csproj to configure as source generator with required analyzer packages
- [X] T004 [P] Create src/Rndr.Razor/Generator/DiagnosticDescriptors.cs with TUI001-TUI010 error codes from data-model.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core parsing entities and infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T005 Create src/Rndr.Razor/Parsing/SourceLocation.cs with FilePath, Line, Column, Length for error reporting
- [X] T006 [P] Create src/Rndr.Razor/Parsing/TuiDiagnostic.cs with Code, Severity, Message, Location fields
- [X] T007 [P] Create src/Rndr.Razor/Parsing/TuiViewDirective.cs with Location field
- [X] T008 [P] Create src/Rndr.Razor/Parsing/TuiUsingDirective.cs with Namespace, Location fields
- [X] T009 [P] Create src/Rndr.Razor/Parsing/TuiInjectDirective.cs with TypeName, PropertyName, Location fields
- [X] T010 [P] Create src/Rndr.Razor/Parsing/TuiCodeBlock.cs with Content, Location, brace location fields
- [X] T011 Create src/Rndr.Razor/Parsing/TuiAttributeValue.cs with ValueType enum (Literal, Expression, Lambda, MethodReference), RawValue, ParsedValue
- [X] T012 Create src/Rndr.Razor/Parsing/TuiAttribute.cs with Name, Value (TuiAttributeValue), Location
- [X] T013 Create src/Rndr.Razor/Parsing/TuiMarkupNode.cs with NodeType enum, TagName, Attributes, Children, TextContent, Location, IsSelfClosing
- [X] T014 Create src/Rndr.Razor/Parsing/TuiDocument.cs aggregating all directives, code block, root markup, diagnostics
- [X] T015 Update src/Rndr.Razor/build/Rndr.Razor.targets to include **/*.tui as AdditionalFiles per research.md

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Create a Simple .tui Component (Priority: P1) 🎯 MVP

**Goal**: Developers can create a .tui file with @view directive and basic markup that compiles to a TuiComponentBase class

**Independent Test**: Create `Test.tui` with `@view` and `<Column><Text>Hello</Text></Column>`, build, verify generated class inherits TuiComponentBase and Build() produces correct nodes

### Tests for User Story 1

- [X] T016 [P] [US1] Create tests/Rndr.Razor.Tests/ParsingTests.cs with test for parsing @view directive
- [X] T017 [P] [US1] Add ParsingTests.cs test for parsing nested markup tags (Column, Text, Panel)
- [X] T018 [P] [US1] Create tests/Rndr.Razor.Tests/CodeEmitterTests.cs with test for generating minimal component class

### Implementation for User Story 1

- [X] T019 [US1] Implement src/Rndr.Razor/Parsing/TuiSyntaxParser.cs with ParseDocument method that extracts @view directive
- [X] T020 [US1] Add TuiSyntaxParser markup parsing for all 8 tags (Column, Row, Panel, Centered, Text, Button, Spacer, TextInput)
- [X] T021 [US1] Add TuiSyntaxParser attribute parsing for Padding, Gap, Title, Bold, Accent, Faint, Align attributes
- [X] T022 [US1] Create src/Rndr.Razor/Generator/CodeGenerationContext.cs with Document, Indentation, CurrentBuilder fields
- [X] T023 [US1] Implement src/Rndr.Razor/Generator/TuiCodeEmitter.cs with EmitComponent method generating class declaration
- [X] T024 [US1] Add TuiCodeEmitter.EmitBuildMethod generating Build(LayoutBuilder) from markup tree
- [X] T025 [US1] Add TuiCodeEmitter container tag generation (Column, Row, Panel, Centered) with nested callbacks
- [X] T026 [US1] Add TuiCodeEmitter leaf tag generation (Text, Button, Spacer, TextInput) with direct calls
- [X] T027 [US1] Implement src/Rndr.Razor/Generator/TuiSourceGenerator.cs as IIncrementalGenerator with Initialize method
- [X] T028 [US1] Add TuiSourceGenerator.RegisterSourceOutput wiring parser and emitter together
- [X] T029 [US1] Add error reporting with TUI001-TUI003 diagnostics for missing @view and unknown tags

**Checkpoint**: Basic .tui files compile to working TuiComponentBase classes

---

## Phase 4: User Story 2 - Use Reactive State in .tui Components (Priority: P1)

**Goal**: Developers can declare state in @code blocks and bind values to markup using @ syntax

**Independent Test**: Create .tui with `@code { var count = State("count", 0); }` and `<Text>Count: @count.Value</Text>`, run app, verify value displays

### Tests for User Story 2

- [X] T030 [P] [US2] Add ParsingTests.cs test for parsing @code block content extraction
- [X] T031 [P] [US2] Add ParsingTests.cs test for parsing @expression syntax in text content
- [X] T032 [P] [US2] Add CodeEmitterTests.cs test for generating @code block as class members

### Implementation for User Story 2

- [X] T033 [US2] Add TuiSyntaxParser @code block parsing with brace matching and content extraction
- [X] T034 [US2] Add TuiSyntaxParser Razor expression parsing in markup text content (@variable)
- [X] T035 [US2] Add TuiCodeEmitter code block emission as class members
- [X] T036 [US2] Add TuiCodeEmitter expression interpolation for text content ($"Count: {count.Value}")
- [X] T037 [US2] Add TUI008-TUI009 diagnostics for @code block syntax errors and warnings

**Checkpoint**: .tui components with state and data binding work correctly

---

## Phase 5: User Story 3 - Handle Events in .tui Components (Priority: P1)

**Goal**: Buttons and interactive elements call methods defined in @code blocks

**Independent Test**: Create .tui with `<Button OnClick="@Increment">+</Button>` and `void Increment()` method, click button, verify state updates

### Tests for User Story 3

- [X] T038 [P] [US3] Add ParsingTests.cs test for parsing OnClick="@MethodName" attribute
- [X] T039 [P] [US3] Add ParsingTests.cs test for parsing OnClick="@(() => expr)" lambda attribute
- [X] T040 [P] [US3] Add CodeEmitterTests.cs test for generating Button with method reference handler

### Implementation for User Story 3

- [X] T041 [US3] Add TuiSyntaxParser method reference parsing (OnClick="@Handler")
- [X] T042 [US3] Add TuiSyntaxParser lambda expression parsing (OnClick="@(() => count.Value++)")
- [X] T043 [US3] Add TuiSyntaxParser OnChanged callback parsing for TextInput
- [X] T044 [US3] Add TuiCodeEmitter event handler emission for Button OnClick
- [X] T045 [US3] Add TuiCodeEmitter event handler emission for TextInput OnChanged
- [X] T046 [US3] Add Width and Primary attribute support for Button tag

**Checkpoint**: Interactive .tui components with event handlers work correctly

---

## Phase 6: User Story 4 - Inject Dependencies into .tui Components (Priority: P2)

**Goal**: Developers can use @inject to declare dependencies resolved from DI

**Independent Test**: Create .tui with `@inject ILogger<MyComponent> Logger`, build and run, verify Logger is available and functional

### Tests for User Story 4

- [X] T047 [P] [US4] Add ParsingTests.cs test for parsing @inject TypeName PropertyName directive
- [X] T048 [P] [US4] Add CodeEmitterTests.cs test for generating inject property with correct type

### Implementation for User Story 4

- [X] T049 [US4] Add TuiSyntaxParser @inject directive parsing with type and property name extraction
- [X] T050 [US4] Add TuiCodeEmitter inject property generation (public T PropName { get; set; } = default!)
- [X] T051 [US4] Update src/Rndr/DefaultEventLoop.cs RenderCurrentView to populate @inject properties via ActivatorUtilities.CreateInstance before AttachContext
- [X] T052 [US4] Add integration test for DI injection in generated component

**Checkpoint**: @inject directive enables DI in .tui components

---

## Phase 7: User Story 5 - Convert Sample App to .tui Files (Priority: P2)

**Goal**: Sample app demonstrates .tui authoring with Home.tui and Log.tui replacing C# components

**Independent Test**: Replace HomeComponent.cs and LogComponent.cs with .tui versions, run sample, verify identical functionality

### Tests for User Story 5

- [X] T053 [P] [US5] Add integration test comparing Home.tui output to original HomeComponent.cs output

### Implementation for User Story 5

- [X] T054 [US5] Add TuiSyntaxParser @using directive parsing
- [X] T055 [US5] Add TuiCodeEmitter @using emission in generated file
- [X] T056 [US5] Create src/Rndr.Samples.MyFocusTui/Pages/Home.tui from HomeComponent.cs per quickstart.md patterns
- [X] T057 [US5] Create src/Rndr.Samples.MyFocusTui/Pages/Log.tui from LogComponent.cs per quickstart.md patterns
- [X] T058 [US5] Update src/Rndr.Samples.MyFocusTui/Program.cs MapView calls to use generated .tui types (typeof(Home), typeof(Log))
- [X] T059 [US5] Delete or archive original Pages/HomeComponent.cs and Pages/LogComponent.cs
- [X] T060 [US5] Verify AOT publish with no trim warnings (dotnet publish -c Release)

**Checkpoint**: Sample app fully uses .tui files with identical functionality

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories and final validation

- [X] T061 [P] Add TuiSyntaxParser @if/@else control flow parsing in markup
- [X] T062 [P] Add TuiSyntaxParser @foreach iteration parsing in markup
- [X] T062b [P] Add TuiSyntaxParser @switch control flow parsing in markup (case/default blocks)
- [X] T063 Add TuiCodeEmitter control flow emission as C# if/foreach/switch blocks
- [X] T064 [P] Add TUI004-TUI007 diagnostics for unknown attributes and tag balance errors
- [X] T065 Implement incremental generator caching for unchanged .tui files
- [X] T066 [P] Add snapshot tests in tests/Rndr.Razor.Tests/Snapshots/ for generated code verification
- [X] T067 Run quickstart.md validation scenarios end-to-end
- [X] T068 Performance validation: verify rebuild completes in <1000ms for single .tui file change

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - US1 → US2 → US3 (P1 stories have some dependencies on parsing)
  - US4 and US5 can start after US3 (P2 stories)
- **Polish (Phase 8)**: Depends on US1-US5 completion

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - Establishes core generator pipeline
- **User Story 2 (P1)**: Depends on US1 parser foundation - Adds @code and expression handling
- **User Story 3 (P1)**: Depends on US2 expression parsing - Adds event handler attributes
- **User Story 4 (P2)**: Can start after US1 - Independent @inject functionality
- **User Story 5 (P2)**: Depends on US1-US4 completion - Full feature usage in sample

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- Parser changes before emitter changes
- Core functionality before error handling
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Test tasks within a story marked [P] can run in parallel
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Create tests/Rndr.Razor.Tests/ParsingTests.cs with test for parsing @view directive"
Task: "Add ParsingTests.cs test for parsing nested markup tags"
Task: "Create tests/Rndr.Razor.Tests/CodeEmitterTests.cs with test for generating minimal component class"

# Launch foundational parallel tasks:
Task: "Create src/Rndr.Razor/Parsing/TuiDiagnostic.cs"
Task: "Create src/Rndr.Razor/Parsing/TuiViewDirective.cs"
Task: "Create src/Rndr.Razor/Parsing/TuiUsingDirective.cs"
Task: "Create src/Rndr.Razor/Parsing/TuiInjectDirective.cs"
Task: "Create src/Rndr.Razor/Parsing/TuiCodeBlock.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test basic .tui compilation independently
5. Demo: Static .tui files compile to working components

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Basic .tui works (MVP!)
3. Add User Story 2 → Test independently → State binding works
4. Add User Story 3 → Test independently → Events work → **Feature Complete for P1**
5. Add User Story 4 → Test independently → DI works
6. Add User Story 5 → Test independently → Sample app converted → **Feature Complete**
7. Polish phase → Control flow, diagnostics, performance

### Critical Path

```
Setup → Foundational → US1 (Parser+Emitter+Generator) → US2 (@code) → US3 (Events)
                                    ↓
                           US4 (@inject) → US5 (Sample App)
```

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- The generator produces AOT-compatible code (no reflection in generated output)
- Existing TuiRazorConfiguration.cs contains tag/attribute mappings to reference

