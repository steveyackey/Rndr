# Quickstart: .tui Components

**Feature**: 002-tui-razor-generator  
**Date**: 2025-12-06

## Overview

This guide shows how to create TUI components using `.tui` single-file components—the Vue-like authoring experience for Rndr.

---

## Prerequisites

- .NET 8 SDK or later
- A project referencing `Rndr` and `Rndr.Razor` packages

---

## Step 1: Add Package References

```xml
<ItemGroup>
    <PackageReference Include="Rndr" Version="1.0.0" />
    <PackageReference Include="Rndr.Razor" Version="1.0.0" />
</ItemGroup>
```

The `Rndr.Razor` package automatically configures MSBuild to process `.tui` files.

---

## Step 2: Create Your First .tui Component

Create `Pages/Hello.tui`:

```razor
@view

<Column Padding="1">
    <Text Bold="true" Accent="true">Hello, Rndr!</Text>
    <Text Faint="true">Welcome to TUI development</Text>
</Column>
```

**Key points:**
- `@view` directive is required and must be first
- Markup uses XML-like tags from Rndr's layout system
- Attributes map directly to builder properties

---

## Step 3: Register the Component

In `Program.cs`:

```csharp
using Rndr;
using MyApp.Pages; // Namespace matches folder structure

var app = TuiApplication.CreateBuilder(args).Build();

app.MapView("/", typeof(Hello));

await app.RunAsync();
```

---

## Step 4: Build and Run

```bash
dotnet run
```

You should see your styled "Hello, Rndr!" message in the terminal.

---

## Adding Interactivity

### State Management

Add state using the `@code` block:

```razor
@view

@code {
    private Signal<int> count = default!;
}

<Column Padding="1" Gap="1">
    <Text Bold="true">Count: @count.Value</Text>
    <Row Gap="2">
        <Button OnClick="@(() => count.Value--)">-</Button>
        <Button OnClick="@(() => count.Value++)" Primary="true">+</Button>
    </Row>
</Column>
```

**Note**: Signal fields are declared with `= default!` and initialized automatically in the generated `Build()` method.

### Extracted Methods

For cleaner code, extract handlers:

```razor
@view

@code {
    private Signal<int> count = default!;
    
    void Increment() => count.Value++;
    void Decrement() => count.Value--;
}

<Column Padding="1" Gap="1">
    <Text Bold="true">Count: @count.Value</Text>
    <Row Gap="2">
        <Button OnClick="@Decrement">-</Button>
        <Button OnClick="@Increment" Primary="true">+</Button>
    </Row>
</Column>
```

---

## Using Global State

Share state across pages with `StateGlobal`:

```razor
@view
@using MyApp.Models

@code {
    private Signal<AppState> state = default!;
    
    protected override void OnInit()
    {
        state = StateGlobal("app", new AppState());
    }
}

<Column Padding="1">
    <Text>User: @state.Value.UserName</Text>
</Column>
```

---

## Dependency Injection

Inject services using the `@inject` directive:

```razor
@view
@using Microsoft.Extensions.Logging

@inject ILogger<MyComponent> Logger

@code {
    void HandleClick()
    {
        Logger.LogInformation("Button was clicked");
    }
}

<Column>
    <Button OnClick="@HandleClick">Click Me</Button>
</Column>
```

Register services in `Program.cs`:

```csharp
var builder = TuiApplication.CreateBuilder(args);
builder.Services.AddLogging();

var app = builder.Build();
// ...
```

---

## Conditional Rendering

Use `@if` for conditional content:

```razor
@view

@code {
    private Signal<bool> isLoggedIn = default!;
}

<Column Padding="1">
    @if (isLoggedIn.Value)
    {
        <Text Accent="true">Welcome back!</Text>
        <Button OnClick="@(() => isLoggedIn.Value = false)">Log Out</Button>
    }
    else
    {
        <Text>Please log in</Text>
        <Button OnClick="@(() => isLoggedIn.Value = true)" Primary="true">Log In</Button>
    }
</Column>
```

---

## Lists and Iteration

Use `@foreach` for collections:

```razor
@view

@code {
    private List<string> items = ["Apple", "Banana", "Cherry"];
}

<Column Padding="1" Gap="1">
    <Text Bold="true">Fruits:</Text>
    @foreach (var item in items)
    {
        <Text>• @item</Text>
    }
</Column>
```

---

## Layout Patterns

### Panel with Content

```razor
<Panel Title="Settings">
    <Column Padding="1" Gap="1">
        <Text>Option 1</Text>
        <Text>Option 2</Text>
    </Column>
</Panel>
```

### Horizontal Button Row

```razor
<Row Gap="2">
    <Button OnClick="@Cancel">Cancel</Button>
    <Button OnClick="@Save" Primary="true">Save</Button>
</Row>
```

### Centered Content

```razor
<Centered>
    <Panel Title="Welcome">
        <Text>Centered panel content</Text>
    </Panel>
</Centered>
```

### Spacers for Layout

```razor
<Column>
    <Text>Top content</Text>
    <Spacer />
    <Text Faint="true">Footer at bottom</Text>
</Column>
```

---

## Navigation

Access navigation via the inherited `Context`:

```razor
@view

@code {
    void GoToSettings()
    {
        Context.Navigation.Navigate("/settings");
    }
    
    void GoBack()
    {
        Context.Navigation.Back();
    }
}

<Column Padding="1" Gap="1">
    <Button OnClick="@GoToSettings">Settings</Button>
    <Button OnClick="@GoBack">Back</Button>
</Column>
```

---

## Text Input

Capture user input with `TextInput`:

```razor
@view

@code {
    private Signal<string> name = default!;
}

<Column Padding="1" Gap="1">
    <TextInput 
        Value="@name.Value" 
        OnChanged="@(v => name.Value = v)"
        Placeholder="Enter your name" />
    
    <Text>Hello, @name.Value!</Text>
</Column>
```

---

## Complete Example: Todo App

`Pages/Home.tui`:

```razor
@view
@using MyTodoApp.Models

@code {
    private Signal<TodoState> state = default!;
    private Signal<string> newTodo = default!;
    
    protected override void OnInit()
    {
        state = StateGlobal("todos", new TodoState());
        newTodo = State("newTodo", "");
    }
    
    void AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(newTodo.Value))
        {
            state.Value.Items.Add(newTodo.Value);
            newTodo.Value = "";
        }
    }
    
    void RemoveTodo(string item)
    {
        state.Value.Items.Remove(item);
    }
}

<Column Padding="1" Gap="1">
    <Text Bold="true" Accent="true">📝 My Todos</Text>
    
    <Row Gap="1">
        <TextInput 
            Value="@newTodo.Value" 
            OnChanged="@(v => newTodo.Value = v)"
            Placeholder="New todo..." />
        <Button OnClick="@AddTodo" Primary="true">Add</Button>
    </Row>
    
    <Panel Title="Tasks">
        <Column Padding="1">
            @if (state.Value.Items.Count == 0)
            {
                <Text Faint="true">No todos yet</Text>
            }
            else
            {
                @foreach (var item in state.Value.Items)
                {
                    <Row Gap="2">
                        <Text>• @item</Text>
                        <Button OnClick="@(() => RemoveTodo(item))">✕</Button>
                    </Row>
                }
            }
        </Column>
    </Panel>
    
    <Spacer />
    <Text Faint="true">[Q] Quit</Text>
</Column>
```

---

## Troubleshooting

### Build Errors

**Error: TUI001 - Missing @view directive**
- Ensure `@view` is the first non-whitespace content in the file

**Error: TUI003 - Unknown tag**
- Check spelling: valid tags are `Column`, `Row`, `Panel`, `Text`, `Button`, `Spacer`, `Centered`, `TextInput`

**Error: TUI004 - Unknown attribute**
- Verify the attribute is supported for that tag (see [contracts/generator-api.md](contracts/generator-api.md))

### Runtime Issues

**Component not found**
- Verify the namespace in `typeof()` matches the generated namespace (project root + folder path)
- Check that `Rndr.Razor` package is referenced

**State not updating**
- Ensure you're using `signal.Value = newValue` (not reassigning the signal itself)
- State must be declared in `@code` block

### IDE Support

While full IntelliSense for `.tui` files is a stretch goal, you can:
- Use the C# `@code` block normally (IntelliSense works there)
- Reference the generated `.g.cs` files in `obj/` for debugging

---

## Next Steps

- See the [sample app](../../../src/Rndr.Samples.MyFocusTui/) for a complete working example
- Read the [API contracts](contracts/generator-api.md) for full tag/attribute reference
- Check the [specification](spec.md) for all supported features

