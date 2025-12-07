# Data Model: Examples Reorganization and Focus List

**Feature**: 003-examples-focus-list  
**Date**: 2025-01-27

## Overview

This document describes the data model for the MyFocusTui sample application. The model is simple and uses in-memory state management via the Rndr framework's `StateGlobal()` mechanism. No persistent storage is required.

---

## Entities

### FocusState

**Purpose**: Global application state that tracks the current focus item and activity log.

**Location**: `examples/MyFocusTui/Models/FocusState.cs` (moved from `src/Rndr.Samples.MyFocusTui/Models/FocusState.cs`)

**Namespace**: `MyFocusTui.Models` (changed from `Rndr.Samples.MyFocusTui.Models`)

**Properties**:

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `CurrentTodo` | `string?` | The currently active focus item. Null when no focus is active. | None (nullable string) |
| `Log` | `List<ActionEntry>` | Collection of all logged actions (start, finish, clear events). | Read-only collection, initialized as empty list |
| `HasActiveFocus` | `bool` | Computed property indicating whether there is an active focus. | Derived from `CurrentTodo` (not null/whitespace) |

**State Management**:
- Stored globally via `StateGlobal("focus", new FocusState())`
- Accessed across all views (Home, Log, FocusList) using the same key
- Reactive updates via `Signal<FocusState>` trigger UI re-renders automatically

**State Transitions**:

1. **No Focus → Active Focus**:
   - Trigger: User clicks "Start Focus" button
   - Action: Set `CurrentTodo` to focus text, add "Started" entry to `Log`
   - Result: `HasActiveFocus` becomes `true`

2. **Active Focus → No Focus (Finish)**:
   - Trigger: User clicks "Finish" button
   - Action: Add "Completed" entry to `Log`, set `CurrentTodo` to `null`
   - Result: `HasActiveFocus` becomes `false`

3. **Active Focus → No Focus (Clear)**:
   - Trigger: User clicks "Clear" button
   - Action: Add "Cleared" entry to `Log`, set `CurrentTodo` to `null`
   - Result: `HasActiveFocus` becomes `false`

---

### ActionEntry

**Purpose**: Represents a single logged action in the focus timer application.

**Location**: `examples/MyFocusTui/Models/ActionEntry.cs` (moved from `src/Rndr.Samples.MyFocusTui/Models/ActionEntry.cs`)

**Namespace**: `MyFocusTui.Models` (changed from `Rndr.Samples.MyFocusTui.Models`)

**Type**: C# record (immutable)

**Properties**:

| Property | Type | Description | Validation |
|----------|------|-------------|------------|
| `Timestamp` | `DateTime` | When the action occurred. | Required (init-only) |
| `Message` | `string` | Human-readable description of the action. | Required (init-only), non-null |

**Usage**:
- Created when focus is started, finished, or cleared
- Stored in `FocusState.Log` collection
- Displayed in Home view (recent 3 entries) and Log view (all entries)
- Sorted by timestamp (newest first) for display

**Example Messages**:
- `"Started: Working on important task"`
- `"Completed: Working on important task"`
- `"Cleared: Working on important task"`

---

## Relationships

### FocusState → ActionEntry (One-to-Many)

- One `FocusState` contains many `ActionEntry` items
- Relationship: `FocusState.Log` is a `List<ActionEntry>`
- Lifetime: ActionEntry instances persist for the application session (in-memory only)
- Ordering: Entries are typically sorted by `Timestamp` descending for display

---

## State Scope

### Global State

The `FocusState` is stored as **global application state** using `StateGlobal("focus", ...)`. This means:

- **Shared across all views**: Home, Log, and FocusList all access the same state instance
- **Reactive updates**: Changes in one view automatically reflect in other views
- **Session lifetime**: State persists for the duration of the application session
- **No persistence**: State is lost when application exits (by design for this sample)

### View-Scoped State

No view-scoped state is required for this feature. All functionality uses the global `FocusState`.

---

## Validation Rules

### FocusState

1. **CurrentTodo**:
   - Can be `null` (no active focus)
   - Can be non-empty string (active focus)
   - Empty or whitespace-only strings should be treated as "no focus" (handled by `HasActiveFocus` property)

2. **Log**:
   - Always initialized as empty list (never null)
   - Can grow unbounded during session (no size limit for this sample)
   - Entries are append-only (no deletion except "Clear All" in Log view)

### ActionEntry

1. **Timestamp**:
   - Must be set when entry is created
   - Typically uses `DateTime.Now` for current time
   - No validation on value (can be past or future, though typically current time)

2. **Message**:
   - Must be non-null
   - No length restrictions
   - Format is free-form, but typically follows pattern: `"{Action}: {FocusText}"`

---

## Data Flow

### Starting a Focus

```
User clicks "Start Focus" button
  → Home.tui: StartFocus() method
  → Sets state.Value.CurrentTodo = "Working on important task"
  → Creates new ActionEntry with Timestamp and Message
  → Adds ActionEntry to state.Value.Log
  → Signal<FocusState> triggers reactive update
  → All views re-render (Home, Log, FocusList if visible)
```

### Viewing Focus List

```
User navigates to FocusList view
  → FocusList.tui: OnInit() accesses StateGlobal("focus", ...)
  → Reads state.Value.CurrentTodo and state.Value.HasActiveFocus
  → Renders appropriate UI (active focus or empty state)
  → Updates automatically when focus changes (reactive)
```

### Finishing/Clearing Focus

```
User clicks "Finish" or "Clear" button
  → Home.tui: FinishFocus() or ClearFocus() method
  → Creates ActionEntry for the action
  → Adds to state.Value.Log
  → Sets state.Value.CurrentTodo = null
  → Signal<FocusState> triggers reactive update
  → All views update to show "no active focus" state
```

---

## Future Considerations

While out of scope for this feature, potential enhancements could include:

1. **Persistence**: Save state to file/database on exit, restore on startup
2. **Multiple Focuses**: Support multiple simultaneous focus items (would require model changes)
3. **Focus History**: Track focus sessions with start/end times, duration
4. **Categories/Tags**: Organize focuses by category or tag
5. **Statistics**: Track completion rates, time spent, etc.

These would require significant model changes and are explicitly out of scope per the feature specification.

