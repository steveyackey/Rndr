# Feature Specification: .tui Razor Source Generator

**Feature Branch**: `002-tui-razor-generator`  
**Created**: 2025-12-06  
**Status**: Draft  
**Input**: Complete the .tui file integration from Phase 3 of Plan.md - the Razor source generator that transforms Vue-like single-file components into C# classes.

## Overview

The Rndr framework currently has a complete C#-based component system (TuiComponentBase, LayoutBuilder, etc.) but lacks the promised Vue-like single-file component authoring experience. This feature implements the Razor source generator that transforms `.tui` files with markup and `@code` blocks into standard `TuiComponentBase`-derived classes at build time.

### Core Value Proposition

1. **Vue-like authoring**: Developers write markup with `<Column>`, `<Panel>`, `<Button>` tags instead of C# builder chains
2. **Single-file components**: Markup, code, and state live together in one `.tui` file
3. **Build-time compilation**: No runtime overhead—`.tui` files become regular C# classes
4. **Seamless integration**: Generated components work identically to hand-written C# components

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a Simple .tui Component (Priority: P1)

A developer wants to author a TUI component using familiar HTML-like markup instead of C# builder code. They create a `.tui` file with the `@view` directive, add markup tags, and the build process generates a working component class.

**Why this priority**: This is the foundational capability that enables all .tui file functionality. Without file compilation, nothing else works.

**Independent Test**: Create a `.tui` file with `@view` and basic markup, build the project, verify a C# class is generated that inherits from `TuiComponentBase`.

**Acceptance Scenarios**:

1. **Given** a project referencing Rndr.Razor, **When** the developer creates a file `Counter.tui` with `@view` directive and `<Column>` markup, **Then** building the project generates a `Counter` class that inherits from `TuiComponentBase`
2. **Given** a `.tui` file with nested markup like `<Panel><Text>Hello</Text></Panel>`, **When** the project builds, **Then** the generated `Build(LayoutBuilder)` method produces the correct nested node structure
3. **Given** a generated component, **When** it is mapped via `MapView("/", typeof(Counter))` and the app runs, **Then** it renders identically to an equivalent hand-written C# component

---

### User Story 2 - Use Reactive State in .tui Components (Priority: P1)

A developer wants to use Rndr's reactive state system within their `.tui` component. They declare state in a `@code` block and bind values to markup using `@` syntax.

**Why this priority**: State binding is essential for any interactive component. Without it, .tui files can only render static content.

**Independent Test**: Create a `.tui` file with `@code { var count = State("count", 0); }` and `<Text>Count: @count.Value</Text>`, run the app, verify the value displays and updates when state changes.

**Acceptance Scenarios**:

1. **Given** a `.tui` file with `@code { var count = State("count", 0); }`, **When** built and run, **Then** calling `count.Value++` triggers a re-render
2. **Given** markup containing `@count.Value`, **When** the state value changes, **Then** the displayed text updates to reflect the new value
3. **Given** a `@code` block with `StateGlobal("key", value)`, **When** the component renders, **Then** the global state is accessible and reactive

---

### User Story 3 - Handle Events in .tui Components (Priority: P1)

A developer wants buttons and other interactive elements to call methods defined in their `@code` block.

**Why this priority**: Event handling enables user interaction. Without it, components are display-only.

**Independent Test**: Create a `.tui` file with `<Button OnClick="@Increment">+</Button>` and `void Increment() => count.Value++`, click the button, verify state updates.

**Acceptance Scenarios**:

1. **Given** a `.tui` file with `<Button OnClick="@HandleClick">Click</Button>` and a `void HandleClick()` method in `@code`, **When** the button is activated, **Then** the method executes
2. **Given** markup with inline lambda `OnClick="@(() => count.Value++)"`, **When** built, **Then** the lambda is correctly bound to the button
3. **Given** a `<TextInput OnChanged="@(v => text.Value = v)">`, **When** the user types, **Then** the state updates with each change

---

### User Story 4 - Inject Dependencies into .tui Components (Priority: P2)

A developer wants to use services registered in DI within their `.tui` component. They use `@inject` to declare dependencies that are resolved when the component is instantiated.

**Why this priority**: Dependency injection enables components to access services like loggers, repositories, or custom services. Important for real applications but not blocking for basic functionality.

**Independent Test**: Create a `.tui` file with `@inject ILogger<MyComponent> Logger`, build and run, verify the logger is available and works.

**Acceptance Scenarios**:

1. **Given** a `.tui` file with `@inject ILogger<Counter> Logger`, **When** the component is instantiated, **Then** `Logger` is a valid logger instance
2. **Given** multiple `@inject` directives, **When** built, **Then** each service is injected as a property on the generated class
3. **Given** an injected service used in a button handler, **When** the button is clicked, **Then** the service method executes correctly

---

### User Story 5 - Convert Sample App to .tui Files (Priority: P2)

A developer reviewing the sample application wants to see .tui files in action. The MyFocusTui sample app's Home and Log pages should use `.tui` files instead of C# classes.

**Why this priority**: The sample app is the primary teaching tool. It should demonstrate the flagship authoring experience.

**Independent Test**: Replace HomeComponent.cs and LogComponent.cs with Home.tui and Log.tui, build and run the sample, verify identical functionality.

**Acceptance Scenarios**:

1. **Given** the sample app with `Pages/Home.tui` replacing `Pages/HomeComponent.cs`, **When** the app runs, **Then** it functions identically to the C# version
2. **Given** both .tui pages using `StateGlobal("focus", new FocusState())`, **When** state changes in Home, **Then** Log shows the updated state
3. **Given** the sample app with all .tui files, **When** published with AOT, **Then** no trim warnings occur

---

### Edge Cases

- What happens when a `.tui` file has syntax errors in markup? (Build should fail with clear error message pointing to line/column)
- What happens when a `.tui` file references a non-existent method in `OnClick`? (Compile error in generated code with traceable line reference)
- How are special characters in text content handled? (Escape sequences preserved, `@` escaped as `@@`)
- What happens with self-closing tags like `<Spacer />`? (Treated as elements with no children)
- How are `@if`/`@else` blocks handled? (Standard Razor syntax; generates C# if/else statements wrapping builder calls)
- How are `@foreach` loops handled? (Standard Razor syntax; generates C# foreach iterating builder calls)
- How are `@switch` blocks handled? (Standard Razor syntax; generates C# switch expression or statement with builder calls per case)

---

## Requirements *(mandatory)*

### Functional Requirements

#### File Format & Parsing

- **FR-001**: System MUST recognize `.tui` files as Rndr single-file components
- **FR-002**: System MUST require the `@view` directive as the first non-whitespace content
- **FR-003**: System MUST support `@using` directives for namespace imports
- **FR-004**: System MUST support `@inject` directives for dependency injection
- **FR-005**: System MUST support `@code { }` blocks containing C# component logic
- **FR-006**: System MUST support Razor expression syntax (`@expression`) for data binding in markup

#### Markup Processing

- **FR-007**: System MUST map `<Column>` tags to `LayoutBuilder.Column()` calls
- **FR-008**: System MUST map `<Row>` tags to builder `.Row()` calls
- **FR-009**: System MUST map `<Panel Title="X">` to builder `.Panel("X", ...)` calls
- **FR-010**: System MUST map `<Text>content</Text>` to builder `.Text("content", ...)` calls
- **FR-011**: System MUST map `<Button OnClick="@handler">label</Button>` to builder `.Button("label", handler)` calls
- **FR-012**: System MUST map `<Spacer />` to builder `.Spacer()` calls
- **FR-013**: System MUST map `<Centered>` to builder `.Centered(...)` calls
- **FR-014**: System MUST map `<TextInput>` to builder `.TextInput(...)` calls

#### Attribute Mapping

- **FR-015**: System MUST map `Padding="n"` attribute to `.Padding(n)` builder call
- **FR-016**: System MUST map `Gap="n"` attribute to `.Gap(n)` builder call
- **FR-017**: System MUST map `Bold="true"` to `style.Bold = true` in style configuration
- **FR-018**: System MUST map `Accent="true"` to `style.Accent = true`
- **FR-019**: System MUST map `Faint="true"` to `style.Faint = true`
- **FR-020**: System MUST map `Align="Center|Left|Right"` to `style.Align = TextAlign.X`
- **FR-021**: System MUST map `Primary="true"` on Button to `.IsPrimary = true`
- **FR-022**: System MUST map `Width="n"` on Button to width configuration

#### Code Generation

- **FR-023**: System MUST generate a partial class inheriting from `TuiComponentBase`
- **FR-024**: System MUST place `@code` block contents as class members
- **FR-025**: System MUST generate `Build(LayoutBuilder layout)` method from markup
- **FR-026**: System MUST generate properties for `@inject` directives
- **FR-027**: System MUST support Razor control flow (`@if`, `@foreach`, `@switch`) in markup

#### Build Integration

- **FR-028**: System MUST integrate with MSBuild to process `.tui` files at build time
- **FR-029**: System MUST generate source files before compilation (source generator or MSBuild task)
- **FR-030**: System MUST report build errors with file path, line, and column for syntax errors
- **FR-031**: System MUST NOT require runtime reflection for generated components (AOT compatible)

---

### Key Entities

- **.tui File**: A single-file component containing markup and optional `@code` block
- **TuiRazorGenerator**: The source generator/build task that transforms `.tui` to C#
- **Generated Component**: The output C# class inheriting from `TuiComponentBase`
- **Tag Mapping**: The translation rules from markup tags to builder method calls
- **Attribute Mapping**: The translation rules from markup attributes to builder/style configurations

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All markup tags (`Column`, `Row`, `Panel`, `Text`, `Button`, `Spacer`, `Centered`, `TextInput`) compile to correct builder code
- **SC-002**: `.tui` files with `@code` blocks compile successfully with state and methods accessible in markup
- **SC-003**: Generated components produce identical node trees as equivalent hand-written C# components
- **SC-004**: Build errors in `.tui` files report accurate file, line, and column information
- **SC-005**: Sample app .tui files (Home.tui, Log.tui) pass all existing integration tests
- **SC-006**: Sample app with .tui files publishes successfully with AOT (no trim warnings)
- **SC-007**: Developers can create a new .tui component and see it render in under 30 seconds (build + run cycle)

---

## Assumptions

- Developers are familiar with Razor syntax from ASP.NET (Blazor, MVC)
- The existing `TuiRazorConfiguration.cs` tag/attribute mappings are the correct translations
- Source generators are preferred over MSBuild tasks for faster incremental builds
- IntelliSense/IDE support is a stretch goal, not required for initial implementation
- The `.tui` file extension is unique and won't conflict with other tooling

---

## Out of Scope

- VS Code extension for .tui syntax highlighting (Phase 4 in Plan.md)
- Rider plugin for .tui file support (Phase 4 in Plan.md)
- Hot reload / dotnet watch integration (Phase 4 in Plan.md)
- Two-way data binding (one-way from state to UI is sufficient)
- Component nesting/composition (calling one .tui component from another)
- CSS-like styling within .tui files (theming handles styling)

