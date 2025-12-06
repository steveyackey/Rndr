# Feature Specification: Rndr TUI Framework

**Feature Branch**: `001-rndr-tui-framework`  
**Created**: 2025-12-06  
**Status**: Draft  
**Input**: User description: "Rndr TUI Framework - A Vue-like TUI framework for .NET with Minimal API patterns, enabling developers to build beautiful terminal user interfaces with reactive state, navigation, and theming."

## Overview

Rndr is a Text User Interface (TUI) framework for .NET that combines the familiar patterns of ASP.NET Minimal APIs with Vue-like component authoring. The framework enables developers to build visually polished terminal applications with minimal boilerplate while supporting advanced scenarios like navigation, shared state, and theming.

### Core Value Proposition

1. **Familiar API patterns**: Developers who know ASP.NET Minimal APIs can immediately understand the application structure
2. **Component-based authoring**: Single-file components (.tui files) with markup, logic, and state in one place
3. **Beautiful defaults**: Professional-looking TUIs out of the box without manual ANSI escape sequences
4. **Production-ready**: AOT-compatible, testable, and observable from day one

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Build a Simple Counter App (Priority: P1)

A developer wants to create their first TUI application with Rndr. They create a minimal project with a single view that displays a counter and buttons to increment/decrement it, demonstrating the core application structure and reactive state.

**Why this priority**: This is the foundational "Hello World" experience that validates the framework works and introduces developers to the core concepts. Without a working basic app, nothing else matters.

**Independent Test**: Can be fully tested by creating a project, writing a counter view, and running it in a terminal. Delivers immediate visual feedback and validates the build/run cycle.

**Acceptance Scenarios**:

1. **Given** a new .NET project with Rndr referenced, **When** the developer creates a TuiApplication with a single mapped view containing text and buttons, **Then** the application compiles and displays the view in the terminal
2. **Given** a running counter view with reactive state, **When** the user presses a button to increment, **Then** the displayed count updates immediately without flickering
3. **Given** a running application, **When** the user presses a quit key, **Then** the application exits cleanly and returns the terminal to its original state

---

### User Story 2 - Navigate Between Multiple Views (Priority: P1)

A developer wants to build an application with multiple screens. They create a home view and a details view, with buttons and keyboard shortcuts to navigate between them, demonstrating the routing and navigation system.

**Why this priority**: Multi-view navigation is essential for any real application. Without navigation, developers can only build single-screen toys.

**Independent Test**: Can be fully tested by creating two views with navigation buttons, running the app, and navigating back and forth. Delivers a complete user flow.

**Acceptance Scenarios**:

1. **Given** an application with two mapped views ("/" and "/details"), **When** the user clicks a "Go to Details" button on the home view, **Then** the details view is displayed
2. **Given** the user is on the details view, **When** they press the "Back" button, **Then** they return to the home view
3. **Given** global key handlers are configured for "H" and "D", **When** the user presses "H" from any view, **Then** they navigate to the home view
4. **Given** navigation history exists, **When** the user calls Back() multiple times, **Then** each call returns to the previous view in the stack until reaching the initial view

---

### User Story 3 - Share State Across Views (Priority: P2)

A developer wants to maintain application state (like a logged-in user or a current task) that persists across navigation. They create global state that multiple views can read and modify.

**Why this priority**: Shared state is required for real-world applications where data must persist across navigation. This enables todo apps, dashboards, and any stateful application.

**Independent Test**: Can be fully tested by creating global state in one view, navigating to another view, and verifying the state persists. Delivers data continuity.

**Acceptance Scenarios**:

1. **Given** a global state object initialized with default values, **When** one view modifies the state and navigates to another view, **Then** the second view sees the updated state
2. **Given** route-scoped state created in a view, **When** the user navigates away and returns, **Then** the route-scoped state is preserved
3. **Given** both global and route-scoped state exist, **When** the developer accesses them in a view, **Then** each operates independently with its appropriate scope

---

### User Story 4 - Create Vue-like Single-File Components (Priority: P2)

A developer wants to author views using familiar markup syntax rather than pure code. They create .tui files with markup tags, code blocks, and reactive bindings that compile to standard components.

**Why this priority**: The .tui file format is the flagship developer experience differentiator. It makes Rndr feel like a modern frontend framework rather than imperative console code.

**Independent Test**: Can be fully tested by creating a .tui file with markup and @code block, building the project, and running the app. Delivers the Vue-like authoring experience.

**Acceptance Scenarios**:

1. **Given** a .tui file with @view directive and markup, **When** the project is built, **Then** the file compiles to a valid component class
2. **Given** a .tui file with @code block containing State declarations, **When** the component renders, **Then** the state values are displayed and reactive
3. **Given** a .tui file with Button elements and OnClick handlers, **When** the user clicks a button, **Then** the handler executes and updates state
4. **Given** a .tui file with @inject directive, **When** the component is instantiated, **Then** the injected service is available in the code block

---

### User Story 5 - Apply Consistent Visual Theming (Priority: P3)

A developer wants their application to have a consistent, professional look. They configure a theme with accent colors, border styles, and spacing that applies across all components without manual styling.

**Why this priority**: Theming differentiates Rndr from "ugly" TUI frameworks. However, good defaults mean theming is optional for basic apps.

**Independent Test**: Can be fully tested by configuring a custom theme, rendering components, and verifying colors and borders match the theme. Delivers visual polish.

**Acceptance Scenarios**:

1. **Given** an application with default theme, **When** panels and buttons render, **Then** they use the default accent color and border style
2. **Given** a custom theme with modified accent color, **When** accent-styled text renders, **Then** it uses the custom accent color
3. **Given** theme configuration for rounded borders, **When** panels render, **Then** they display rounded Unicode box-drawing characters

---

### User Story 6 - Test Components Without Terminal (Priority: P3)

A developer wants to write automated tests for their views. They use test helpers to build components in isolation, inspect the generated layout tree, and verify state behavior without requiring a real terminal.

**Why this priority**: Testability is essential for production applications but not for initial exploration. It's a "scale to large apps" feature.

**Independent Test**: Can be fully tested by writing a unit test that instantiates a component via test helper, calling methods, and asserting on the node tree. Delivers confidence in component behavior.

**Acceptance Scenarios**:

1. **Given** a component class, **When** the developer uses RndrTestHost.BuildComponent<T>(), **Then** they receive the node tree and context for inspection
2. **Given** a component with state, **When** the test modifies state and rebuilds, **Then** the node tree reflects the new state
3. **Given** a fake input source, **When** the test simulates key events, **Then** handlers execute and state updates predictably

---

### Edge Cases

- What happens when the user resizes the terminal window during rendering? (The framework should detect size changes and re-layout)
- How does the system handle terminals that don't support Unicode box-drawing characters? (Fallback to ASCII borders)
- What happens when navigation is called to a non-existent route? (Return false or throw clear exception)
- How does the system handle rapid state changes? (Batch updates to avoid excessive re-renders)
- What happens when a component attempts Console I/O directly? (Design should discourage; analyzers can warn)

---

## Requirements *(mandatory)*

### Functional Requirements

#### Application Lifecycle

- **FR-001**: System MUST provide a builder pattern for application setup that mirrors ASP.NET Minimal API conventions
- **FR-002**: System MUST support route-to-view mapping where each route displays a specific view
- **FR-003**: System MUST provide async application lifecycle with clean startup and shutdown
- **FR-004**: System MUST integrate with .NET generic host for configuration, logging, and dependency injection

#### State Management

- **FR-005**: System MUST provide reactive state primitives that automatically trigger re-renders when modified
- **FR-006**: System MUST support route-scoped state that persists for the lifetime of a route's presence in navigation history
- **FR-007**: System MUST support global state that persists for the application lifetime
- **FR-008**: System MUST provide change detection that only re-renders when state values actually change

#### Navigation

- **FR-009**: System MUST maintain a navigation stack enabling forward and back navigation
- **FR-010**: System MUST support programmatic navigation (Navigate, Back, Replace operations)
- **FR-011**: System MUST support global keyboard shortcuts that work regardless of current view
- **FR-012**: System MUST allow views to handle keyboard events for view-specific shortcuts

#### Layout & Rendering

- **FR-013**: System MUST provide layout primitives for vertical stacking (Column), horizontal arrangement (Row), bordered containers (Panel), text display (Text), interactive buttons (Button), text input (TextInput), flexible spacing (Spacer), and centered content (Centered)
- **FR-014**: System MUST render to terminal using ANSI/VT escape sequences for modern terminals
- **FR-015**: System MUST hide cursor during rendering and restore terminal state on exit
- **FR-016**: System MUST support box-drawing characters for panel borders with configurable styles

#### Component Authoring (.tui Files)

- **FR-017**: System MUST support single-file components with markup, code, and directives in one file
- **FR-018**: System MUST compile .tui files to standard component classes at build time
- **FR-019**: System MUST support reactive data binding in markup using @ syntax
- **FR-020**: System MUST support dependency injection into components via @inject directive

#### Theming

- **FR-021**: System MUST provide a default theme with professional appearance
- **FR-022**: System MUST support custom themes with configurable colors, borders, and spacing
- **FR-023**: System MUST apply themes consistently across all built-in components

#### Testability & Observability

- **FR-024**: System MUST abstract terminal I/O through interfaces for test substitution
- **FR-025**: System MUST provide test helpers to build and inspect components without terminal
- **FR-026**: System MUST expose diagnostic hooks for tracing and metrics collection
- **FR-027**: System MUST NOT require reflection for core operations (AOT compatibility)

---

### Key Entities

- **TuiApplication/TuiApp**: The application instance that manages views, routes, and lifecycle
- **View/Component**: A screen or reusable UI unit composed of layout nodes with associated state and handlers
- **Signal**: A reactive state container that tracks changes and triggers re-renders
- **Node**: An element in the layout tree (Column, Row, Panel, Text, Button, etc.)
- **Theme**: A collection of visual settings (colors, borders, spacing) applied across the application
- **Route**: A string path that maps to a specific view (e.g., "/", "/log", "/settings")
- **NavigationContext**: The current navigation state providing methods to navigate, go back, or replace routes

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can create and run a working "Hello World" TUI app in under 5 minutes using documentation
- **SC-002**: Developers familiar with ASP.NET Minimal APIs rate the API as "familiar" or "very familiar" in usability testing
- **SC-003**: Applications start and display first frame in under 500ms on standard hardware
- **SC-004**: Re-renders after state changes complete in under 16ms (60fps capable) for typical views
- **SC-005**: All sample applications compile and run successfully with .NET AOT publication
- **SC-006**: Component unit tests execute without requiring terminal access or mocking system console
- **SC-007**: Default themed applications receive "professional" or "polished" ratings from 80%+ of testers compared to raw ANSI output
- **SC-008**: The sample "Most Important Todo + Log" application demonstrates all core features (navigation, state, theming, .tui files) in a single cohesive example

---

## Assumptions

- Target developers are familiar with .NET and have used ASP.NET or similar frameworks
- Target terminals support UTF-8 encoding and ANSI/VT escape sequences (modern terminals)
- Applications will run in interactive terminal sessions, not piped/redirected scenarios
- Developers have access to .NET 8 SDK or later
- Initial release focuses on keyboard input; mouse support may be added later
- Initial release targets single-window applications; split-pane layouts may be added later

---

## Out of Scope

- Mouse input handling (designed for future but not in initial release)
- Advanced layout features like flex-wrap, grid systems, or complex alignment
- Plugin/extension system for third-party components
- Dynamic view discovery at runtime (to maintain AOT compatibility)
- IDE extensions and tooling (outlined but not fully implemented)
- Cross-terminal compatibility layer (assumes modern terminal)
