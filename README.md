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

app.MapView("/", typeof(Home));
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
// Register multiple views
app.MapView("/", typeof(Home));
app.MapView("/settings", typeof(Settings));

// Navigate from within a view
Context.Navigation.Navigate("/settings");  // Push
Context.Navigation.Back();                 // Pop
Context.Navigation.Replace("/other");      // Replace current

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

Test components without a terminal using `RndrTestHost`:
```csharp
using Rndr.Testing;
using Xunit;

public class CounterTests
{
    [Fact]
    public void Counter_Increments_OnClick()
    {
        var (nodes, context) = RndrTestHost.BuildComponent<Home>(); // generated from Home.tui
        var button = nodes.FindButton("+");

        button?.OnClick?.Invoke();

        var (updated, _) = RndrTestHost.BuildComponent<Home>(stateStore: context.StateStore);
        var text = updated.FindText("Count:");
        Assert.Contains("1", text!.Content);
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

Contributions are welcome! Before submitting PRs:

1. **Read the Constitution**: Review `.specify/memory/constitution.md` to understand the project's core principles and technical constraints.
2. **Review Copilot Instructions**: If using GitHub Copilot, see `.github/copilot-instructions.md` for development guidelines that ensure changes align with the constitution.
3. **Follow Spec Kit Workflow**: When possible, use the spec kit commands (`/speckit.specify`, `/speckit.plan`, `/speckit.implement`) which have built-in constitutional compliance checks.
4. **Read Specification Docs**: Review relevant docs in `specs/` for feature context.

All changes must align with the constitutional principles, especially:
- Minimal API Ergonomics (ASP.NET Minimal API patterns)
- Vue-like Component Model (.tui single-file components)
- AOT-Friendly by Design (no reflection in core paths)
- Testability First (abstracted I/O, pure Build() methods)

