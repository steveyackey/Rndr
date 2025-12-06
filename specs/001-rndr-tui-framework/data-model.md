# Data Model: Rndr TUI Framework

**Feature**: 001-rndr-tui-framework  
**Date**: 2025-12-06

## Overview

This document defines the core entities, their relationships, and state transitions for the Rndr framework. Rndr is a framework (not a data-driven app), so the "data model" focuses on runtime object structures rather than persistent storage.

---

## Core Entities

### TuiApp

The running application instance that manages the lifecycle.

| Field | Type | Description |
|-------|------|-------------|
| Host | IHost | The underlying generic host |
| Routes | Dictionary<string, ViewRegistration> | Registered route-to-view mappings |
| GlobalKeyHandlers | List<GlobalKeyHandler> | Global keyboard handlers |
| NavigationStack | Stack<string> | Current navigation history |
| CurrentRoute | string | Active route path |
| IsRunning | bool | Whether the event loop is active |

**Relationships**:
- Contains many ViewRegistrations
- Contains one NavigationState
- Uses IEventLoop for execution

**State Transitions**:
```
Created → Running → Stopped
           ↓
        Quitting
```

---

### ViewRegistration

Internal representation of a mapped view.

| Field | Type | Description |
|-------|------|-------------|
| Route | string | The URL-like path (e.g., "/", "/log") |
| ViewBuilder | Action<ViewDefinition>? | C# view configuration (Phase 1-2) |
| ComponentType | Type? | .tui component type (Phase 3) |
| Title | string? | Optional view title |
| KeyHandler | ViewKeyHandler? | View-specific key handler |

**Relationships**:
- Belongs to TuiApp
- Produces ViewContext at runtime

---

### ViewContext

Per-render context provided to views.

| Field | Type | Description |
|-------|------|-------------|
| Services | IServiceProvider | DI container |
| Navigation | NavigationContext | Navigation operations |
| Logger | ILogger | Scoped logger |
| Route | string | Current route path |

**Methods**:
- `State<T>(key, initial)` → Signal<T> (route-scoped)
- `StateGlobal<T>(key, initial)` → Signal<T> (global-scoped)

**Relationships**:
- Uses IStateStore for state management
- Provides NavigationContext for navigation

---

### Signal<T>

Reactive state container.

| Field | Type | Description |
|-------|------|-------------|
| Value | T | Current state value |
| OnChanged | Action | Callback invoked on value change |

**Behavior**:
- Setting `Value` to a different value triggers `OnChanged`
- Equality check uses `EqualityComparer<T>.Default`
- Implements `ISignal` for untyped access

**Relationships**:
- Stored in IStateStore by (scope, key) tuple

---

### IStateStore

State storage and retrieval.

| Method | Description |
|--------|-------------|
| `GetOrCreate<T>(scopeKey, key, initialFactory)` | Get existing or create new Signal |

**Scopes**:
- Route-scoped: `scopeKey = route` (e.g., "/", "/log")
- Global: `scopeKey = "global"`

---

### NavigationContext

Navigation operations for a view.

| Field | Type | Description |
|-------|------|-------------|
| CurrentRoute | string | Active route |

| Method | Return | Description |
|--------|--------|-------------|
| `Navigate(route)` | bool | Push route onto stack |
| `Back()` | bool | Pop and go to previous |
| `Replace(route)` | bool | Replace current route |

**Relationships**:
- Delegates to TuiApp's navigation stack

---

### INavigationState

Observable navigation state (for testing/diagnostics).

| Property | Type | Description |
|----------|------|-------------|
| CurrentRoute | string | Active route |
| Stack | IReadOnlyList<string> | Full navigation history |

---

## Layout Entities

### Node (abstract)

Base class for all layout nodes.

| Field | Type | Description |
|-------|------|-------------|
| Kind | NodeKind | Discriminator (Column, Row, etc.) |
| Children | List<Node> | Child nodes |
| Style | NodeStyle | Visual styling |

**Node Kinds**:
- Column, Row, Panel, Text, Button, TextInput, Spacer, Centered

---

### NodeStyle

Visual styling for nodes.

| Field | Type | Description |
|-------|------|-------------|
| Padding | int? | Internal padding |
| Gap | int? | Space between children |
| Align | TextAlign? | Text alignment (Left, Center, Right) |
| Bold | bool | Bold text |
| Accent | bool | Use accent color |
| Faint | bool | Dimmed text |

---

### Concrete Node Types

**ColumnNode**: Stacks children vertically
- Inherits: Node
- Additional: none

**RowNode**: Arranges children horizontally
- Inherits: Node
- Additional: none

**PanelNode**: Bordered container
- Inherits: Node
- Additional: `Title: string`

**TextNode**: Text display
- Inherits: Node
- Additional: `Content: string`

**ButtonNode**: Clickable button
- Inherits: Node
- Additional: `Label: string`, `OnClick: Action?`, `IsPrimary: bool`, `Width: int?`

**TextInputNode**: Text entry field
- Inherits: Node
- Additional: `Value: string`, `OnChanged: Action<string>?`, `Placeholder: string?`

**SpacerNode**: Flexible space
- Inherits: Node
- Additional: `Weight: int` (for flex distribution)

**CenteredNode**: Centers content
- Inherits: Node
- Additional: none

---

## Theming Entities

### RndrTheme

Application-wide visual theme.

| Field | Type | Default |
|-------|------|---------|
| AccentColor | ConsoleColor | Cyan |
| TextColor | ConsoleColor | Gray |
| MutedTextColor | ConsoleColor | DarkGray |
| ErrorColor | ConsoleColor | Red |
| Panel | PanelTheme | (nested) |
| SpacingUnit | int | 1 |

### PanelTheme

Panel-specific theming.

| Field | Type | Default |
|-------|------|---------|
| BorderStyle | BorderStyle | Rounded |

### BorderStyle (enum)

- Square: `┌─┐│└─┘`
- Rounded: `╭─╮│╰─╯`

---

## Input Entities

### KeyEvent

Keyboard input event.

| Field | Type | Description |
|-------|------|-------------|
| Key | ConsoleKey | The key pressed |
| KeyChar | char | Character representation |
| Modifiers | ConsoleModifiers | Shift, Alt, Control |

---

## Component Entities

### TuiComponentBase

Base class for .tui-generated components.

| Field | Type | Description |
|-------|------|-------------|
| Context | ViewContext | Attached view context |

| Method | Description |
|--------|-------------|
| `AttachContext(context)` | Called before Build |
| `OnInit()` | Override for initialization |
| `Build(LayoutBuilder)` | Abstract - generates layout |
| `State<T>(key, initial)` | Convenience for Context.State |
| `StateGlobal<T>(key, initial)` | Convenience for Context.StateGlobal |

---

## Diagnostics Entities

### RndrDiagnostics (static)

| Field | Type | Description |
|-------|------|-------------|
| ActivitySource | ActivitySource | "Rndr.Core" for tracing |
| Meter | Meter | "Rndr.Core" for metrics |

**Activities**:
- `Navigate`: route.from, route.to tags
- `RenderFrame`: duration
- `KeyEvent`: key, char tags

**Metrics**:
- `rndr.frames_rendered`: Counter
- `rndr.render_duration_ms`: Histogram

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         TuiApp                              │
│  ┌─────────────────┐   ┌─────────────────┐                 │
│  │ ViewRegistration│   │ NavigationState │                 │
│  │    (many)       │   │    (one)        │                 │
│  └────────┬────────┘   └────────┬────────┘                 │
│           │                     │                           │
└───────────┼─────────────────────┼───────────────────────────┘
            │                     │
            ▼                     ▼
     ┌──────────────┐      ┌──────────────┐
     │  ViewContext │──────│NavigationCtx │
     └──────┬───────┘      └──────────────┘
            │
            ▼
     ┌──────────────┐      ┌──────────────┐
     │  IStateStore │◄─────│  Signal<T>   │
     │  (scoped)    │      │   (many)     │
     └──────────────┘      └──────────────┘
            │
            ▼
     ┌──────────────┐
     │LayoutBuilder │
     │              │
     │  ┌────────┐  │
     │  │  Node  │  │
     │  │ (tree) │  │
     │  └────────┘  │
     └──────────────┘
            │
            ▼
     ┌──────────────┐      ┌──────────────┐
     │ ITuiRenderer │──────│  RndrTheme   │
     └──────────────┘      └──────────────┘
```

---

## State Lifecycle

### Application Startup
1. `TuiApplication.CreateBuilder(args)` creates builder
2. Services registered via `builder.Services`
3. `Build()` creates `TuiApp` with `IHost`
4. `MapView()` calls register routes
5. `OnGlobalKey()` registers global handlers
6. `RunAsync()` starts event loop

### View Rendering
1. Event loop determines current route
2. ViewRegistration retrieved for route
3. ViewContext created with services, navigation, state store
4. For C# views: `Action<ViewDefinition>` invoked
5. For .tui components: instance created, `AttachContext()`, `Build()`
6. Layout tree passed to renderer
7. Renderer computes positions and writes to console

### State Change
1. View code sets `signal.Value = newValue`
2. Signal compares with current value
3. If different: `OnChanged` callback invoked
4. Event loop marks view as dirty
5. Next tick: re-render triggered

### Navigation
1. View calls `Navigation.Navigate("/route")`
2. TuiApp pushes route onto stack
3. Current route updated
4. Event loop re-renders new view
5. Activity emitted for diagnostics

