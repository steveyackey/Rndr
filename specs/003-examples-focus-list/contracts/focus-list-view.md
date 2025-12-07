# API Contract: Focus List View

**Feature**: 003-examples-focus-list  
**Date**: 2025-01-27  
**Scope**: View API contract for the new FocusList view component

## Overview

This document defines the API contract for the FocusList view, which displays the currently active focus item. The view uses existing Rndr framework APIs for state management, navigation, and UI rendering.

---

## View Registration

### Route Mapping

The FocusList view is registered in `Program.cs` using the standard `MapView` API:

```csharp
app.MapView("/focus-list", typeof(FocusList));
```

**Route**: `/focus-list`  
**Component Type**: `MyFocusTui.Pages.FocusList` (generated from `FocusList.tui`)

---

## Component Structure

### File Location

**Source**: `examples/MyFocusTui/Pages/FocusList.tui`  
**Generated**: `obj/Generated/Rndr.Razor/.../FocusList.g.cs`  
**Namespace**: `MyFocusTui.Pages`

### Component Class

```csharp
namespace MyFocusTui.Pages;

public partial class FocusList : TuiComponentBase
{
    private Signal<FocusState> state = default!;
    
    protected override void OnInit()
    {
        state = StateGlobal("focus", new FocusState());
    }
}
```

---

## State Management Contract

### Global State Access

The FocusList view accesses the same global state as Home and Log views:

```csharp
state = StateGlobal("focus", new FocusState());
```

**State Key**: `"focus"`  
**State Type**: `FocusState`  
**Scope**: Global (shared across all views)  
**Reactivity**: Changes to `state.Value` automatically trigger view re-renders

### State Properties Used

| Property | Type | Usage |
|----------|------|-------|
| `state.Value.CurrentTodo` | `string?` | Displayed when active focus exists |
| `state.Value.HasActiveFocus` | `bool` | Determines which UI to render (active vs empty state) |

---

## UI Rendering Contract

### Active Focus State

When `state.Value.HasActiveFocus` is `true`:

```razor
<Panel Title="Active Focus">
    <Column Padding="1" Gap="1">
        <Text Bold="true" Accent="true">@state.Value.CurrentTodo</Text>
        <Text Faint="true">Currently working on this focus item.</Text>
    </Column>
</Panel>
```

**Display Elements**:
- Panel with title "Active Focus"
- Bold, accented text showing `CurrentTodo`
- Helper text indicating active status

### Empty State

When `state.Value.HasActiveFocus` is `false`:

```razor
<Panel Title="Active Focus">
    <Column Padding="1">
        <Text Faint="true">No active focus</Text>
        <Text Faint="true">Start a focus session from the home screen.</Text>
    </Column>
</Panel>
```

**Display Elements**:
- Panel with title "Active Focus"
- Faint text indicating no active focus
- Helper text directing user to home screen

---

## Navigation Contract

### Navigation Actions

The FocusList view provides navigation back to home:

```razor
<Button OnClick="@GoBack">← Back to Home</Button>
```

**Implementation**:
```csharp
void GoBack()
{
    Context.Navigation.Back();
}
```

**Behavior**: Returns to previous route in navigation stack (typically Home view)

### Global Keyboard Shortcuts

Navigation to FocusList is handled via global key handler in `Program.cs`:

```csharp
app.OnGlobalKey((key, ctx) =>
{
    switch (key.KeyChar)
    {
        case 'f' or 'F':
            ctx.Navigation.Navigate("/focus-list");
            return true;
        // ... other handlers
    }
    return false;
});
```

**Shortcut**: `f` or `F` key  
**Action**: Navigate to `/focus-list` route

---

## Layout Structure

### Complete View Layout

```razor
<Column Padding="1" Gap="1">
    <Text Bold="true" Accent="true">📋 Focus List</Text>
    
    <Panel Title="Active Focus">
        <!-- Active or empty state content -->
    </Panel>
    
    <Spacer />
    
    <Row Gap="2">
        <Button OnClick="@GoBack">← Back to Home</Button>
    </Row>
    
    <Text Faint="true">[Q] Quit  [H] Home  [F] Focus List  [Tab] Navigate</Text>
</Column>
```

**Layout Elements**:
1. Header text with emoji and accent styling
2. Main content panel (conditional rendering based on state)
3. Flexible spacer
4. Navigation button row
5. Help text footer

---

## Reactive Updates Contract

### Automatic Re-rendering

The FocusList view automatically updates when the global focus state changes:

**Trigger Scenarios**:
1. User starts a focus from Home view → FocusList shows active focus
2. User finishes/clears focus from Home view → FocusList shows empty state
3. User navigates to FocusList while focus is active → Shows current state immediately

**Mechanism**: Framework's `Signal<T>` reactivity system detects value changes and triggers re-render of all views using that signal.

---

## Integration with Existing Views

### State Sharing

All three views (Home, Log, FocusList) share the same global state:

```csharp
// In each view's OnInit():
state = StateGlobal("focus", new FocusState());
```

**Consistency**: All views always see the same `FocusState` instance, ensuring UI consistency across the application.

### Navigation Flow

```
Home View
  ├─ [F key] → FocusList View
  ├─ [L key] → Log View
  └─ [Q key] → Quit

FocusList View
  ├─ [Back button] → Previous view (typically Home)
  ├─ [H key] → Home View
  └─ [Q key] → Quit

Log View
  ├─ [Back button] → Previous view (typically Home)
  ├─ [H key] → Home View
  └─ [Q key] → Quit
```

---

## Error Handling

### Missing State

If `StateGlobal("focus", ...)` is called with a new instance but state already exists, the framework returns the existing instance. No error handling needed.

### Null Safety

The view handles null `CurrentTodo` via `HasActiveFocus` property, which checks for null/whitespace. No additional null checks required in the view.

---

## Testing Considerations

### Unit Testing

The FocusList component can be tested using `RndrTestHost`:

```csharp
[Fact]
public void FocusList_ShowsActiveFocus_WhenFocusExists()
{
    var host = new RndrTestHost();
    var state = host.GetGlobalState<FocusState>("focus");
    state.CurrentTodo = "Test focus";
    
    var component = new FocusList();
    // ... test rendering
}
```

### Integration Testing

Manual testing scenarios:
1. Navigate to FocusList with no active focus → Verify empty state
2. Start focus from Home → Navigate to FocusList → Verify active focus displayed
3. Finish focus from Home → Navigate to FocusList → Verify empty state
4. Verify reactive updates when focus changes while FocusList is visible

---

## Version Compatibility

This view uses existing Rndr framework APIs from Phase 2 and Phase 3:
- **Navigation**: Phase 2 (NavigationContext, route mapping)
- **State Management**: Phase 2 (StateGlobal, Signal<T>)
- **Component Model**: Phase 3 (.tui files, Razor source generation)
- **Layout Primitives**: Phase 1 (Column, Row, Panel, Text, Button, Spacer)

**No new framework features required.**

