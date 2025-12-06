# Quickstart: Rndr TUI Framework

**Feature**: 001-rndr-tui-framework  
**Date**: 2025-12-06

## Prerequisites

- .NET 8 SDK or later
- A terminal with ANSI/VT support (most modern terminals)

---

## Create Your First Rndr App

### 1. Create a new console project

```bash
dotnet new console -n MyTuiApp
cd MyTuiApp
```

### 2. Add the Rndr package

```bash
dotnet add package Rndr
```

### 3. Write your first TUI

Replace `Program.cs` with:

```csharp
using Rndr;
using Rndr.Layout;

var app = TuiApplication.CreateBuilder(args).Build();

app.MapView("/", view =>
{
    view.Title("My First TUI")
        .Use(ctx =>
        {
            var count = ctx.State("count", 0);
            
            var layout = new LayoutBuilder();
            layout.Column(col =>
            {
                col.Padding(2)
                   .Gap(1)
                   .AlignCenter();
                
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
                            row.Button("-", () => count.Value--)
                               .Spacer()
                               .Button("+", () => count.Value++);
                        }).Gap(2);
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

### 4. Run your app

```bash
dotnet run
```

You should see a centered panel with a counter. Use Tab to move between buttons, Enter to click, and Q to quit.

---

## Add Navigation

Add a second view and navigate between them:

```csharp
using Rndr;
using Rndr.Layout;

var app = TuiApplication.CreateBuilder(args).Build();

// Home view
app.MapView("/", view =>
{
    view.Title("Home")
        .Use(ctx =>
        {
            var layout = new LayoutBuilder();
            layout.Column(col =>
            {
                col.Padding(1);
                col.Text("Welcome to Rndr!", s => s.Bold = true);
                col.Spacer();
                col.Button("Go to About", () => ctx.Navigation.Navigate("/about"));
            });
        });
});

// About view
app.MapView("/about", view =>
{
    view.Title("About")
        .Use(ctx =>
        {
            var layout = new LayoutBuilder();
            layout.Column(col =>
            {
                col.Padding(1);
                col.Text("Rndr is a TUI framework for .NET");
                col.Spacer();
                col.Button("Back", () => ctx.Navigation.Back());
            });
        });
});

// Global shortcuts
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
    }
    return false;
});

await app.RunAsync();
```

---

## Share State Across Views

Use `StateGlobal` for state that persists across navigation:

```csharp
// Define a shared state model
public class AppState
{
    public string Username { get; set; } = "Guest";
    public int VisitCount { get; set; } = 0;
}

// In your views:
app.MapView("/", view =>
{
    view.Use(ctx =>
    {
        var state = ctx.StateGlobal("app", new AppState());
        state.Value.VisitCount++;
        
        var layout = new LayoutBuilder();
        layout.Column(col =>
        {
            col.Text($"Hello, {state.Value.Username}!");
            col.Text($"Visits: {state.Value.VisitCount}", s => s.Faint = true);
        });
    });
});
```

---

## Apply a Custom Theme

Configure the theme during app building:

```csharp
var builder = TuiApplication.CreateBuilder(args);

builder.Services.AddRndrTheme(theme =>
{
    theme.AccentColor = ConsoleColor.Magenta;
    theme.Panel.BorderStyle = BorderStyle.Square;
    theme.SpacingUnit = 2;
});

var app = builder.Build();
```

---

## Use .tui Single-File Components (Phase 3)

### 1. Add Rndr.Razor package

```bash
dotnet add package Rndr.Razor
```

### 2. Create a .tui file

Create `Pages/Home.tui`:

```razor
@view
@using Rndr

@code {
    var count = State("count", 0);
    
    void Increment() => count.Value++;
    void Decrement() => count.Value--;
}

<Column Padding="2" Gap="1">
    <Centered>
        <Panel Title="Counter">
            <Column Gap="1">
                <Text Align="Center" Bold="true" Accent="true">
                    Count: @count.Value
                </Text>
                
                <Row Gap="2" Align="Center">
                    <Button OnClick="@Decrement">-</Button>
                    <Button OnClick="@Increment" Primary="true">+</Button>
                </Row>
            </Column>
        </Panel>
    </Centered>
</Column>
```

### 3. Map the component

```csharp
using MyTuiApp.Pages;

var app = TuiApplication.CreateBuilder(args).Build();

app.MapView("/", typeof(Home));

await app.RunAsync();
```

---

## Testing Components

Use `RndrTestHost` to test components without a terminal:

```csharp
using Rndr.Testing;
using Xunit;

public class CounterTests
{
    [Fact]
    public void Counter_Increments_OnButtonClick()
    {
        // Arrange
        var (nodes, context) = RndrTestHost.BuildComponent<Counter>();
        
        // Find the button node
        var button = nodes.FindButton("+");
        
        // Act
        button.OnClick?.Invoke();
        
        // Assert
        var (updatedNodes, _) = RndrTestHost.BuildComponent<Counter>(
            stateStore: context.StateStore);
        var text = updatedNodes.FindText("Count:");
        Assert.Contains("1", text.Content);
    }
}
```

---

## Keyboard Shortcuts Reference

| Key | Default Behavior |
|-----|------------------|
| Tab | Move focus to next button |
| Shift+Tab | Move focus to previous button |
| Enter/Space | Click focused button |
| Q (configurable) | Quit application |

---

## Next Steps

- Read the full [API Reference](./contracts/layout-api.md)
- Explore the [Sample App](../../src/Rndr.Samples.MyFocusTui/)
- Learn about [Theming](./research.md#decision-7-theming-system)
- Set up [Observability](./research.md#decision-10-diagnostics-integration)

---

## Troubleshooting

### Terminal doesn't show colors
- Ensure your terminal supports ANSI escape codes
- On Windows, use Windows Terminal or enable VT processing

### Characters look wrong
- Ensure your terminal uses a font with Unicode box-drawing support
- Try Cascadia Code, JetBrains Mono, or Fira Code

### App doesn't respond to keys
- Check that stdin is a terminal (not redirected)
- Verify no other process is capturing input

