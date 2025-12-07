# Quickstart: Examples Reorganization and Focus List

**Feature**: 003-examples-focus-list  
**Date**: 2025-01-27  
**Scope change (2025-12-07)**: Focus list view was cut; current sample includes Home and Log only.

## Overview

This quickstart guide covers the reorganized MyFocusTui sample application structure and the new focus list functionality. The application has been moved from `src/Rndr.Samples.MyFocusTui` to `examples/MyFocusTui` and enhanced with a focus list view.

---

## Project Structure

### New Location

The MyFocusTui sample application is now located at:

```
examples/MyFocusTui/
├── MyFocusTui.csproj          # Renamed from Rndr.Samples.MyFocusTui.csproj
├── Program.cs                 # Updated namespace: MyFocusTui
├── Models/
│   ├── ActionEntry.cs         # Updated namespace: MyFocusTui.Models
│   └── FocusState.cs          # Updated namespace: MyFocusTui.Models
└── Pages/
    ├── Home.tui               # Updated @using: MyFocusTui.Models
    └── Log.tui                # Updated @using: MyFocusTui.Models
```

### Building the Application

From the repository root:

```bash
# Build the entire solution
dotnet build

# Build just the MyFocusTui application
dotnet build examples/MyFocusTui/MyFocusTui.csproj

# Run the application
dotnet run --project examples/MyFocusTui/MyFocusTui.csproj
```

---

## Running MyFocusTui

### Starting the Application

```bash
cd examples/MyFocusTui
dotnet run
```

Or from the repository root:

```bash
dotnet run --project examples/MyFocusTui/MyFocusTui.csproj
```

### Navigation

The application has two views:

| View | Route | Keyboard Shortcut | Description |
|------|-------|-------------------|-------------|
| Home | `/` | `H` | Main view with current focus and recent activity |
| Log | `/log` | `L` | Full activity log |

### Global Keyboard Shortcuts

- `Q` or `q` - Quit the application
- `H` or `h` - Navigate to Home
- `L` or `l` - Navigate to Log
- `Tab` - Navigate between interactive elements

---

## Focus List

The planned Focus List view was cut from this feature on 2025-12-07. Navigation and shortcuts cover only Home and Log.

---

## Understanding the Code Structure

### Global State

All views share the same global state:

```csharp
// In each view's @code block:
private Signal<FocusState> state = default!;

protected override void OnInit()
{
    state = StateGlobal("focus", new FocusState());
}
```

The `FocusState` contains:
- `CurrentTodo` - The active focus item (null if none)
- `Log` - List of all action entries
- `HasActiveFocus` - Computed property indicating if focus is active

### FocusList.tui Component

The new FocusList view follows the same pattern as Home and Log:

```razor
@view
@using MyFocusTui.Models

<Column Padding="1" Gap="1">
    <Text Bold="true" Accent="true">📋 Focus List</Text>
    
    <Panel Title="Active Focus">
        <Column Padding="1" Gap="1">
            @if (state.Value.HasActiveFocus)
            {
                <Text Bold="true" Accent="true">@state.Value.CurrentTodo</Text>
                <Text Faint="true">Currently working on this focus item.</Text>
            }
            else
            {
                <Text Faint="true">No active focus</Text>
                <Text Faint="true">Start a focus session from the home screen.</Text>
            }
        </Column>
    </Panel>
    
    <Spacer />
    <Row Gap="2">
        <Button OnClick="@GoBack">← Back to Home</Button>
    </Row>
    
    <Text Faint="true">[Q] Quit  [H] Home  [F] Focus List  [Tab] Navigate</Text>
</Column>

@code {
    private Signal<FocusState> state = default!;
    
    protected override void OnInit()
    {
        state = StateGlobal("focus", new FocusState());
    }
    
    void GoBack()
    {
        Context.Navigation.Back();
    }
}
```

### Program.cs Registration

The FocusList view is registered in `Program.cs`:

```csharp
var app = builder.Build();

app.MapView("/", typeof(Home));
app.MapView("/log", typeof(Log));
app.MapView("/focus-list", typeof(FocusList));  // NEW

app.OnGlobalKey((key, ctx) =>
{
    switch (key.KeyChar)
    {
        case 'q' or 'Q':
            ctx.Application.Quit();
            return true;
        case 'h' or 'H':
            ctx.Navigation.Navigate("/");
            return true;
        case 'l' or 'L':
            ctx.Navigation.Navigate("/log");
            return true;
        case 'f' or 'F':  // NEW
            ctx.Navigation.Navigate("/focus-list");
            return true;
    }
    return false;
});

await app.RunAsync();
```

---

## Development Workflow

### Making Changes

1. **Edit .tui files**: Modify `Pages/*.tui` files as needed
2. **Rebuild**: The Razor source generator will regenerate component classes
3. **Run**: Test your changes with `dotnet run`

### Adding New Views

To add a new view:

1. Create a new `.tui` file in `Pages/`:
   ```razor
   @view
   @using MyFocusTui.Models
   
   <Column Padding="1">
       <Text>My New View</Text>
   </Column>
   ```

2. Register it in `Program.cs`:
   ```csharp
   app.MapView("/my-view", typeof(MyNewView));
   ```

3. Add navigation (optional):
   ```csharp
   app.OnGlobalKey((key, ctx) =>
   {
       if (key.KeyChar is 'm' or 'M')
       {
           ctx.Navigation.Navigate("/my-view");
           return true;
       }
       return false;
   });
   ```

### Modifying State

To add new state properties:

1. Update `Models/FocusState.cs`:
   ```csharp
   public class FocusState
   {
       public string? CurrentTodo { get; set; }
       public List<ActionEntry> Log { get; } = [];
       public string? NewProperty { get; set; }  // NEW
   }
   ```

2. Access in views:
   ```csharp
   state.Value.NewProperty = "value";
   ```

3. The reactive system will automatically update all views using this state.

---

## Troubleshooting

### Build Errors After Move

If you encounter build errors after the reorganization:

1. **Clean and rebuild**:
   ```bash
   dotnet clean
   dotnet build
   ```

2. **Check solution file**: Ensure `Rndr.sln` references the new path:
   ```
   examples\MyFocusTui\MyFocusTui.csproj
   ```

3. **Check namespaces**: Verify all files use `MyFocusTui` namespace instead of `Rndr.Samples.MyFocusTui`

### Focus List Not Updating

If the Focus List doesn't update when focus changes:

1. **Verify state key**: Ensure all views use the same key: `StateGlobal("focus", ...)`
2. **Check signal usage**: Make sure you're modifying `state.Value.Property`, not creating a new instance
3. **Rebuild**: Sometimes a clean rebuild fixes state management issues

### Navigation Not Working

If keyboard shortcuts don't work:

1. **Check global handler**: Verify `OnGlobalKey` is registered in `Program.cs`
2. **Check route**: Ensure the route matches exactly: `"/focus-list"`
3. **Check component type**: Verify `typeof(FocusList)` matches the generated class name

---

## Next Steps

- Explore the other views (Home, Log) to understand the full application
- Modify the FocusList view to add additional features
- Add new views or functionality to the application
- Review the Rndr framework documentation for advanced features

---

## Related Documentation

- [Feature Specification](./spec.md) - Complete feature requirements
- [Implementation Plan](./plan.md) - Technical implementation details
- [Data Model](./data-model.md) - Entity and state structure
- [API Contract](./contracts/focus-list-view.md) - FocusList view API details
- [Research](./research.md) - Design decisions and rationale

