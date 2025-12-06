# API Contract: Layout Builder APIs

**Feature**: 001-rndr-tui-framework  
**Date**: 2025-12-06  
**Scope**: Internal API contracts for layout primitives and builders

## Overview

This document defines the public API contracts for Rndr's layout system. These are the APIs developers use to construct view layouts in C# code and that .tui files compile to.

---

## Namespace: Rndr.Layout

### LayoutBuilder

Root builder for constructing layouts.

```csharp
namespace Rndr.Layout;

public sealed class LayoutBuilder
{
    /// <summary>
    /// Get the constructed node tree.
    /// </summary>
    public IReadOnlyList<Node> Build();
    
    /// <summary>
    /// Add a vertical column container.
    /// </summary>
    public LayoutBuilder Column(Action<ColumnBuilder> configure);
    
    /// <summary>
    /// Add a horizontal row container.
    /// </summary>
    public LayoutBuilder Row(Action<RowBuilder> configure);
    
    /// <summary>
    /// Set padding for subsequent nodes.
    /// </summary>
    public LayoutBuilder Padding(int value);
    
    /// <summary>
    /// Set gap between children for subsequent nodes.
    /// </summary>
    public LayoutBuilder Gap(int value);
}
```

---

### ColumnBuilder

Builder for column (vertical stack) contents.

```csharp
namespace Rndr.Layout;

public sealed class ColumnBuilder
{
    /// <summary>
    /// Add text content.
    /// </summary>
    public ColumnBuilder Text(string content, Action<NodeStyle>? configure = null);
    
    /// <summary>
    /// Add a bordered panel with title.
    /// </summary>
    public ColumnBuilder Panel(string title, Action<LayoutBuilder> body);
    
    /// <summary>
    /// Add a clickable button.
    /// </summary>
    public ColumnBuilder Button(string label, Action? onClick);
    
    /// <summary>
    /// Add a text input field.
    /// </summary>
    public ColumnBuilder TextInput(
        string value,
        Action<string>? onChanged,
        string? placeholder = null);
    
    /// <summary>
    /// Add flexible spacer.
    /// </summary>
    public ColumnBuilder Spacer(int weight = 1);
    
    /// <summary>
    /// Add centered content.
    /// </summary>
    public ColumnBuilder Centered(Action<LayoutBuilder> child);
    
    /// <summary>
    /// Add a nested row.
    /// </summary>
    public ColumnBuilder Row(Action<RowBuilder> configure);
    
    // Style methods (fluent)
    public ColumnBuilder Gap(int value);
    public ColumnBuilder Padding(int value);
    public ColumnBuilder AlignCenter();
    public ColumnBuilder AlignRight();
}
```

---

### RowBuilder

Builder for row (horizontal arrangement) contents.

```csharp
namespace Rndr.Layout;

public sealed class RowBuilder
{
    /// <summary>
    /// Add text content.
    /// </summary>
    public RowBuilder Text(string content, Action<NodeStyle>? configure = null);
    
    /// <summary>
    /// Add a clickable button.
    /// </summary>
    public RowBuilder Button(string label, Action? onClick);
    
    /// <summary>
    /// Add flexible spacer.
    /// </summary>
    public RowBuilder Spacer(int weight = 1);
    
    /// <summary>
    /// Add a nested column.
    /// </summary>
    public RowBuilder Column(Action<ColumnBuilder> configure);
    
    // Style methods (fluent)
    public RowBuilder Gap(int value);
    public RowBuilder AlignCenter();
}
```

---

### ButtonBuilder (fluent extensions)

```csharp
namespace Rndr.Layout;

public static class ButtonExtensions
{
    /// <summary>
    /// Set fixed button width.
    /// </summary>
    public static ColumnBuilder Width(this ColumnBuilder builder, int width);
    
    /// <summary>
    /// Mark button as primary (accent styled).
    /// </summary>
    public static ColumnBuilder Primary(this ColumnBuilder builder);
}
```

---

## Namespace: Rndr

### TuiApplication

Static factory for creating applications.

```csharp
namespace Rndr;

public static class TuiApplication
{
    /// <summary>
    /// Create a new application builder.
    /// </summary>
    public static TuiAppBuilder CreateBuilder(string[] args);
}
```

---

### TuiAppBuilder

Application configuration builder.

```csharp
namespace Rndr;

public sealed class TuiAppBuilder
{
    /// <summary>
    /// Access the underlying host builder.
    /// </summary>
    public HostApplicationBuilder HostBuilder { get; }
    
    /// <summary>
    /// Access the service collection.
    /// </summary>
    public IServiceCollection Services { get; }
    
    /// <summary>
    /// Build the configured application.
    /// </summary>
    public TuiApp Build();
}
```

---

### TuiApp

Running application instance.

```csharp
namespace Rndr;

public sealed class TuiApp
{
    /// <summary>
    /// Access the service provider.
    /// </summary>
    public IServiceProvider Services { get; }
    
    /// <summary>
    /// Map a route to a C# view builder.
    /// </summary>
    public TuiApp MapView(string route, Action<ViewDefinition> configure);
    
    /// <summary>
    /// Map a route to a .tui component type.
    /// </summary>
    public TuiApp MapView(string route, Type viewComponentType);
    
    /// <summary>
    /// Register a global key handler.
    /// </summary>
    public TuiApp OnGlobalKey(Func<KeyEvent, GlobalContext, bool> handler);
    
    /// <summary>
    /// Run the application event loop.
    /// </summary>
    public Task RunAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Signal application shutdown.
    /// </summary>
    public void Quit();
}
```

---

### ViewDefinition

View configuration DSL.

```csharp
namespace Rndr;

public sealed class ViewDefinition
{
    /// <summary>
    /// Set the view title.
    /// </summary>
    public ViewDefinition Title(string title);
    
    /// <summary>
    /// Configure the view's layout.
    /// </summary>
    public ViewDefinition Use(Action<ViewContext> build);
    
    /// <summary>
    /// Handle view-specific key events.
    /// </summary>
    public ViewDefinition OnKey(Func<KeyEvent, ViewContext, bool> handler);
}
```

---

### ViewContext

Per-render context provided to views.

```csharp
namespace Rndr;

public sealed class ViewContext
{
    /// <summary>
    /// Service provider for DI.
    /// </summary>
    public IServiceProvider Services { get; }
    
    /// <summary>
    /// Navigation operations.
    /// </summary>
    public NavigationContext Navigation { get; }
    
    /// <summary>
    /// Logger for this view.
    /// </summary>
    public ILogger Logger { get; }
    
    /// <summary>
    /// Current route path.
    /// </summary>
    public string Route { get; }
    
    /// <summary>
    /// Get or create route-scoped state.
    /// </summary>
    public Signal<T> State<T>(string key, T initialValue);
    
    /// <summary>
    /// Get or create global state.
    /// </summary>
    public Signal<T> StateGlobal<T>(string key, T initialValue);
}
```

---

### Signal<T>

Reactive state primitive.

```csharp
namespace Rndr;

public sealed class Signal<T>
{
    /// <summary>
    /// Get or set the current value.
    /// Setting triggers re-render if value changed.
    /// </summary>
    public T Value { get; set; }
}
```

---

## Namespace: Rndr.Navigation

### NavigationContext

Navigation operations.

```csharp
namespace Rndr.Navigation;

public sealed class NavigationContext
{
    /// <summary>
    /// Current route path.
    /// </summary>
    public string CurrentRoute { get; }
    
    /// <summary>
    /// Navigate to a new route (push).
    /// </summary>
    /// <returns>True if navigation succeeded.</returns>
    public bool Navigate(string route);
    
    /// <summary>
    /// Go back to previous route (pop).
    /// </summary>
    /// <returns>True if there was a route to go back to.</returns>
    public bool Back();
    
    /// <summary>
    /// Replace current route (no stack change).
    /// </summary>
    /// <returns>True if replacement succeeded.</returns>
    public bool Replace(string route);
}
```

---

### INavigationState

Observable navigation state.

```csharp
namespace Rndr.Navigation;

public interface INavigationState
{
    /// <summary>
    /// Current route path.
    /// </summary>
    string CurrentRoute { get; }
    
    /// <summary>
    /// Full navigation stack.
    /// </summary>
    IReadOnlyList<string> Stack { get; }
}
```

---

## Namespace: Rndr.Input

### KeyEvent

Keyboard input event.

```csharp
namespace Rndr.Input;

public sealed record KeyEvent(
    ConsoleKey Key,
    char KeyChar,
    ConsoleModifiers Modifiers);
```

---

## Namespace: Rndr.Rendering

### IConsoleAdapter

Console abstraction for testability.

```csharp
namespace Rndr.Rendering;

public interface IConsoleAdapter
{
    int WindowWidth { get; }
    int WindowHeight { get; }
    
    void Clear();
    void WriteAt(int left, int top, string text,
                 ConsoleColor? foreground = null,
                 ConsoleColor? background = null);
    void HideCursor();
    void ShowCursor();
}
```

---

### IInputSource

Input abstraction for testability.

```csharp
namespace Rndr.Input;

public interface IInputSource
{
    /// <summary>
    /// Read a key event.
    /// </summary>
    /// <param name="intercept">If true, don't echo the key.</param>
    KeyEvent ReadKey(bool intercept);
}
```

---

## Usage Examples

### Minimal Counter (Phase 1)

```csharp
var app = TuiApplication.CreateBuilder(args).Build();

app.MapView("/", view =>
{
    view.Title("Counter")
        .Use(ctx =>
        {
            var count = ctx.State("count", 0);
            
            var layout = new LayoutBuilder();
            layout.Column(col =>
            {
                col.Text($"Count: {count.Value}")
                   .Button("+", () => count.Value++)
                   .Button("-", () => count.Value--);
            });
        });
});

await app.RunAsync();
```

### Multi-View with Navigation (Phase 2)

```csharp
app.MapView("/", view =>
{
    view.Use(ctx =>
    {
        var layout = new LayoutBuilder();
        layout.Column(col =>
        {
            col.Text("Home")
               .Button("Go to Log", () => ctx.Navigation.Navigate("/log"));
        });
    });
});

app.MapView("/log", view =>
{
    view.Use(ctx =>
    {
        var layout = new LayoutBuilder();
        layout.Column(col =>
        {
            col.Text("Log Page")
               .Button("Back", () => ctx.Navigation.Back());
        });
    });
});

app.OnGlobalKey((key, ctx) =>
{
    if (key.KeyChar is 'q') { ctx.Application.Quit(); return true; }
    return false;
});
```

---

## .tui to C# Mapping (Phase 3)

| .tui Markup | Generated C# |
|-------------|--------------|
| `<Column Padding="1">` | `layout.Column(col => { }).Padding(1)` |
| `<Row Gap="1">` | `col.Row(row => { }).Gap(1)` |
| `<Panel Title="X">` | `col.Panel("X", body => { })` |
| `<Text Bold="true">` | `col.Text("...", s => s.Bold = true)` |
| `<Button OnClick="@Handler">` | `col.Button("...", Handler)` |
| `<Spacer />` | `col.Spacer()` |
| `<Centered>` | `col.Centered(inner => { })` |

