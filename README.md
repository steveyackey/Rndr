# Rndr

A modern TUI (Terminal User Interface) framework for .NET with minimal-API ergonomics, a source generator for `.tui` single-file components, and built-in reactive state, navigation, theming, and testing tools.

## Features

- **.tui single-file components** compiled by `Rndr.Razor` with Razor-style markup and `@code` blocks
- **Minimal API patterns**: `TuiApplication.CreateBuilder()`, `MapView()`, `RunAsync()`
- **Reactive signals**: `State()` / `StateGlobal()` for automatic re-rendering
- **Navigation stack**: `Navigate()`, `Back()`, `Replace()` with global key hooks
- **Theming + layout primitives**: Unicode box drawing, panels, rows/columns, text input, buttons
- **Testing-first**: `RndrTestHost` and fake console/input adapters for terminal-less tests
- **AOT-ready**: no reflection, designed for Native AOT

## Quick Start (.tui components)

1) Create a console app and add packages
```bash
dotnet new console -n MyTuiApp
cd MyTuiApp
dotnet add package Rndr
dotnet add package Rndr.Razor
```

2) Add `Pages/Home.tui`
```razor
@view
@using Rndr.Layout

@code {
    private Signal<int> count = default!;
    void Increment() => count.Value++;
    void Decrement() => count.Value--;
}

<Column Padding="2" Gap="1">
    <Centered>
        <Panel Title="Counter">
            <Column Gap="1">
                <Text Align="Center" Bold="true" Accent="true">Count: @count.Value</Text>
                <Row Gap="2" Align="Center">
                    <Button OnClick="@Decrement">-</Button>
                    <Button OnClick="@Increment" Primary="true">+</Button>
                </Row>
            </Column>
        </Panel>
    </Centered>
</Column>
```

3) Map the component in `Program.cs`
```csharp
using Rndr;
using MyTuiApp.Pages;

var app = TuiApplication.CreateBuilder(args).Build();

// Type-safe generic MapView - no typeof() needed!
app.MapView<Home>("/");

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

4) Run it
```bash
dotnet run
```
Use Tab to move focus, Enter/Space to click, and Q to quit.

### Prefer code-only layouts?
`.tui` files are optional—you can keep everything in C#:
```csharp
app.MapView("/", view =>
{
    view.Title("Counter").Use((ctx, layout) =>
    {
        var count = ctx.State("count", 0);
        layout.Column(col =>
        {
            col.Padding(2).Gap(1).AlignCenter();
            col.Panel("Counter", panel =>
            {
                panel.Column(inner =>
                {
                    inner.Text($"Count: {count.Value}", s => { s.Bold = true; s.Accent = true; });
                    inner.Row(row =>
                    {
                        row.Gap(2);
                        row.Button("-", () => count.Value--);
                        row.Button("+", () => count.Value++);
                    });
                });
            });
        });
    });
});
```

## Layout Primitives

| Primitive | Description |
|-----------|-------------|
| `Column` | Vertical stack of children |
| `Row` | Horizontal arrangement of children |
| `Panel` | Bordered container with title |
| `Text` | Text display with optional styling |
| `Button` | Clickable button with label |
| `Spacer` | Flexible space between elements |
| `Centered` | Centers its content |
| `TextInput` | Text entry field |

## Navigation

```csharp
// Register multiple views with type-safe generics
app.MapView<Home>("/");
app.MapView<Settings>("/settings");

// Navigate from within a view
Context.Navigation.Navigate("/settings");  // Push
Context.Navigation.Back();                 // Pop
Context.Navigation.Replace("/other");      // Replace current

// Ergonomic navigation extensions
Context.Navigation.NavigateHome();         // Navigate to "/"
Context.Navigation.BackOrHome();           // Back if possible, else home
if (Context.Navigation.CanGoBack()) { ... } // Check if can go back

// Global navigation keys
app.OnGlobalKey((key, ctx) =>
{
    // Use the new NavigateHome() extension
    if (key.KeyChar == 'h') { ctx.Navigation.NavigateHome(); return true; }
    return false;
});
```

## State Management

```csharp
// Route-scoped state (reset when leaving the route)
var count = ctx.State("count", 0);

// Global state (persists across navigation)
var user = ctx.StateGlobal("user", new User { Name = "Guest" });

// Access global state from global key handlers (no DI required!)
app.OnGlobalKey((key, ctx) =>
{
    var settings = ctx.StateGlobal("settings", new Settings());
    // ...
});

// State changes automatically trigger re-renders
count.Value++;  // UI updates

// State keys are validated - helpful error messages
// ctx.State("", 0);  // throws ArgumentException
```

## Component Lifecycle

`.tui` components have lifecycle hooks for initialization, rendering, and cleanup:

```csharp
@code {
    protected override void OnInit()
    {
        // Called once when component is initialized
        // Perfect for setting up state
    }

    protected override void OnAfterRender()
    {
        // Called after each render
        // Use for side effects or logging
    }

    protected override void OnDestroy()
    {
        // Called when navigating away
        // Clean up resources, cancel timers, etc.
    }
}
```

## Theming

```csharp
builder.Services.AddRndrTheme(theme =>
{
    theme.AccentColor = ConsoleColor.Magenta;
    theme.TextColor = ConsoleColor.White;
    theme.Panel.BorderStyle = BorderStyle.Square;  // or Rounded
    theme.SpacingUnit = 2;
});
```

## Testing

Test components without a terminal using `RndrTestHost`:
```csharp
using Rndr.Testing;
using Xunit;

public class CounterTests
{
    [Fact]
    public void Counter_Increments_OnClick()
    {
        // Build and test components easily
        var (nodes, context) = RndrTestHost.BuildComponent<Home>();
        
        // Fluent testing helpers
        nodes.ClickButton("+");  // Find and click in one line!
        
        var (updated, _) = RndrTestHost.BuildComponent<Home>(stateStore: context.StateStore);
        
        // Assert with helpful error messages
        var text = updated.AssertTextExists("Count:");
        Assert.Contains("1", text.Content);
    }
}
```

## Project Structure

```
src/
├── Rndr/             # Core framework library
├── Rndr.Razor/       # .tui source generator and build targets
├── Rndr.Testing/     # Test host and fakes

examples/
└── MyFocusTui/       # Sample app built with .tui components

tests/
├── Rndr.Tests/       # Framework unit tests
└── Rndr.Razor.Tests/ # Generator/parsing tests

specs/                # Design docs and plans
```

## Keyboard Shortcuts

| Key | Behavior |
|-----|----------|
| Tab | Move focus forward |
| Shift+Tab | Move focus backward |
| Enter/Space | Activate focused button |
| Custom | Via `OnGlobalKey` handler |

## Requirements

- .NET 8.0 or later
- Terminal with ANSI/VT support (most modern terminals)
- UTF-8 capable font for box-drawing characters

## License

MIT

## Contributing

Contributions are welcome! Please read the specification docs in `specs/` before submitting PRs.

