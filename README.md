# Rndr

A modern TUI (Terminal User Interface) framework for .NET that combines ASP.NET Minimal API patterns with Vue-like single-file components.

## Features

- **Minimal API Ergonomics** - Familiar patterns from ASP.NET: `TuiApplication.CreateBuilder()`, `MapView()`, `RunAsync()`
- **Vue-like Components** - Single-file `.tui` components with markup and `@code` blocks
- **Reactive State** - Simple `Signal<T>` state management with automatic re-rendering
- **Navigation Stack** - Built-in navigation with `Navigate()`, `Back()`, and `Replace()`
- **Beautiful by Default** - Unicode box-drawing, semantic layout primitives, customizable themes
- **AOT-Compatible** - No reflection, designed for Native AOT compilation
- **Testable** - Abstract I/O interfaces and `RndrTestHost` for testing without a terminal

## Quick Start

### Create a Console App

```bash
dotnet new console -n MyTuiApp
cd MyTuiApp
dotnet add package Rndr
```

### Write Your First TUI

```csharp
using Rndr;
using Rndr.Layout;

var app = TuiApplication.CreateBuilder(args).Build();

app.MapView("/", view =>
{
    view.Title("My First TUI")
        .Use((ctx, layout) =>
        {
            var count = ctx.State("count", 0);

            layout.Column(col =>
            {
                col.Padding(2).Gap(1).AlignCenter();

                col.Panel("Counter", panel =>
                {
                    panel.Column(inner =>
                    {
                        inner.Text($"Count: {count.Value}", s =>
                        {
                            s.Bold = true;
                            s.Accent = true;
                        });

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
})
.OnGlobalKey((key, ctx) =>
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

### Run Your App

```bash
dotnet run
```

Use **Tab** to navigate between buttons, **Enter** to click, and **Q** to quit.

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
// Register multiple views
app.MapView("/", view => { /* home */ });
app.MapView("/settings", view => { /* settings */ });

// Navigate from within a view
ctx.Navigation.Navigate("/settings");  // Push
ctx.Navigation.Back();                  // Pop
ctx.Navigation.Replace("/other");       // Replace current

// Global navigation keys
app.OnGlobalKey((key, ctx) =>
{
    if (key.KeyChar == 'h') { ctx.Navigation.Navigate("/"); return true; }
    return false;
});
```

## State Management

```csharp
// Route-scoped state (reset when leaving the route)
var count = ctx.State("count", 0);

// Global state (persists across navigation)
var user = ctx.StateGlobal("user", new User { Name = "Guest" });

// State changes automatically trigger re-renders
count.Value++;  // UI updates
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

Use `RndrTestHost` to test components without a terminal:

```csharp
using Rndr.Testing;
using Xunit;

public class CounterTests
{
    [Fact]
    public void Counter_Increments_OnClick()
    {
        // Build the view
        var (nodes, context) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                var count = ctx.State("count", 0);
                layout.Column(col =>
                {
                    col.Text($"Count: {count.Value}");
                    col.Button("+", () => count.Value++);
                });
            });
        });

        // Find and click the button
        var button = nodes.FindButton("+");
        button?.OnClick?.Invoke();

        // Verify state changed
        var text = nodes.FindText("Count:");
        Assert.Contains("0", text!.Content);  // Original render
    }
}
```

## Project Structure

```
src/
├── Rndr/                    # Core framework library
├── Rndr.Razor/              # Razor/.tui file integration
├── Rndr.Testing/            # Test helpers
└── Rndr.Samples.MyFocusTui/ # Sample application

tests/
├── Rndr.Tests/              # Unit tests
└── Rndr.Razor.Tests/        # Razor integration tests
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

