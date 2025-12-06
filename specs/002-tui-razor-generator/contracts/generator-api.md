# API Contract: .tui Source Generator

**Feature**: 002-tui-razor-generator  
**Date**: 2025-12-06  
**Scope**: Public APIs for .tui file authoring and build integration

## Overview

This document defines the contracts for the .tui source generator: the file format developers write, the MSBuild integration for consumers, and the generated code patterns.

---

## .tui File Format Contract

### File Structure

```text
@view                           # Required: identifies file as TUI component
@using Namespace                # Optional: namespace imports (0+)
@inject Type PropertyName       # Optional: DI declarations (0+)

<!-- Markup content -->
<RootTag>                       # Required: at least one root element
  ...children...
</RootTag>

@code {                         # Optional: C# class members
    // Fields, methods, properties
}
```

### Directive Syntax

| Directive | Syntax | Required | Multiple |
|-----------|--------|----------|----------|
| @view | `@view` | Yes | No |
| @using | `@using Namespace.Path` | No | Yes |
| @inject | `@inject TypeName PropertyName` | No | Yes |
| @code | `@code { ... }` | No | No |

### Supported Tags

| Tag | Attributes | Children | Description |
|-----|------------|----------|-------------|
| `<Column>` | Padding, Gap | Yes | Vertical stack container |
| `<Row>` | Gap | Yes | Horizontal arrangement |
| `<Panel>` | Title, Padding | Yes | Bordered container with title |
| `<Centered>` | - | Yes | Centers child content |
| `<Text>` | Bold, Accent, Faint, Align | Text only | Text display |
| `<Button>` | OnClick, Width, Primary | Text only | Clickable button |
| `<Spacer>` | - | No | Flexible space |
| `<TextInput>` | Value, OnChanged, Placeholder | No | Text entry field |

### Supported Attributes

#### Layout Attributes

| Attribute | Type | Applies To | Description |
|-----------|------|------------|-------------|
| Padding | int | Column, Panel | Internal padding |
| Gap | int | Column, Row | Space between children |
| Title | string | Panel | Panel title text |

#### Style Attributes

| Attribute | Type | Applies To | Description |
|-----------|------|------------|-------------|
| Bold | bool | Text | Bold text style |
| Accent | bool | Text | Accent color |
| Faint | bool | Text | Dimmed text |
| Align | "Left"\|"Center"\|"Right" | Text | Text alignment |

#### Interactive Attributes

| Attribute | Type | Applies To | Description |
|-----------|------|------------|-------------|
| OnClick | Action\|lambda | Button | Click handler |
| Width | int | Button | Fixed button width |
| Primary | bool | Button | Primary style |
| Value | string\|expression | TextInput | Current value |
| OnChanged | Action\<string\> | TextInput | Change handler |
| Placeholder | string | TextInput | Placeholder text |

### Expression Syntax

| Context | Syntax | Output |
|---------|--------|--------|
| Text content | `@expression` | String interpolation |
| Attribute value | `"@expression"` | Direct expression |
| Method reference | `"@MethodName"` | Method delegate |
| Lambda | `"@(() => expr)"` | Inline lambda |
| Lambda with param | `"@(x => expr)"` | Lambda with parameter |
| Escaped @ | `@@` | Literal @ character |

### Control Flow Syntax

```razor
@if (condition)
{
    <Tag>...</Tag>
}
else if (condition2)
{
    <Tag>...</Tag>
}
else
{
    <Tag>...</Tag>
}

@foreach (var item in collection)
{
    <Tag>@item</Tag>
}

@switch (value)
{
    case "a":
        <Tag>A</Tag>
        break;
    default:
        <Tag>Default</Tag>
        break;
}
```

---

## Example .tui Files

### Minimal Component

```razor
@view

<Column Padding="1">
    <Text Bold="true">Hello, World!</Text>
</Column>
```

### Counter with State

```razor
@view
@using Rndr

@code {
    private Signal<int> _count = default!;
    
    void Increment() => _count.Value++;
    void Decrement() => _count.Value--;
}

<Column Padding="1" Gap="1">
    <Panel Title="Counter">
        <Column Gap="1">
            <Text Bold="true" Accent="true">Count: @_count.Value</Text>
            <Row Gap="2">
                <Button OnClick="@Decrement">-</Button>
                <Button OnClick="@Increment" Primary="true">+</Button>
            </Row>
        </Column>
    </Panel>
</Column>
```

### With Dependency Injection

```razor
@view
@using Microsoft.Extensions.Logging

@inject ILogger<MyComponent> Logger

@code {
    void HandleClick()
    {
        Logger.LogInformation("Button clicked");
    }
}

<Column>
    <Button OnClick="@HandleClick">Click Me</Button>
</Column>
```

### Conditional Rendering

```razor
@view

@code {
    private Signal<bool> _showDetails = default!;
}

<Column Padding="1">
    @if (_showDetails.Value)
    {
        <Panel Title="Details">
            <Text>Here are the details...</Text>
        </Panel>
    }
    else
    {
        <Text Faint="true">Click to show details</Text>
    }
    
    <Button OnClick="@(() => _showDetails.Value = !_showDetails.Value)">
        Toggle Details
    </Button>
</Column>
```

---

## Generated Code Contract

### Generated Class Structure

For a file `Pages/Counter.tui` in project `MyApp`:

```csharp
// <auto-generated />
// Source: Pages/Counter.tui
#nullable enable

using Rndr;
using Rndr.Layout;
// ... @using directives ...

namespace MyApp.Pages;

/// <summary>
/// Generated from Counter.tui
/// </summary>
public partial class Counter : TuiComponentBase
{
    // @inject properties
    public ILogger<Counter> Logger { get; set; } = default!;
    
    // @code block members
    private Signal<int> _count = default!;
    
    void Increment() => _count.Value++;
    
    // Generated Build method
    public override void Build(LayoutBuilder layout)
    {
        // State initialization
        _count = State("_count", 0);
        
        // Markup translation
        layout.Column(col =>
        {
            col.Padding(1).Gap(1);
            col.Text($"Count: {_count.Value}", s => s.Bold = true);
            col.Button("+", Increment);
        });
    }
}
```

### State Initialization Pattern

Signal fields declared in `@code` are initialized in `Build()`:

```csharp
// @code { var count = State("count", 0); }
// Becomes:
private Signal<int> count = default!;

public override void Build(LayoutBuilder layout)
{
    count = State("count", 0);
    // ... rest of build
}
```

### Inject Property Pattern

```csharp
// @inject ILogger<Counter> Logger
// Becomes:
public ILogger<Counter> Logger { get; set; } = default!;
```

---

## MSBuild Integration Contract

### Package Reference

```xml
<ItemGroup>
    <PackageReference Include="Rndr" Version="1.0.0" />
    <PackageReference Include="Rndr.Razor" Version="1.0.0" />
</ItemGroup>
```

### Automatic .tui Discovery

The `Rndr.Razor.targets` file automatically configures:

```xml
<Project>
    <ItemGroup>
        <!-- Include all .tui files as additional files -->
        <AdditionalFiles Include="**/*.tui" />
    </ItemGroup>
</Project>
```

### Manual Configuration (if needed)

```xml
<ItemGroup>
    <!-- Include specific .tui files -->
    <AdditionalFiles Include="Pages\*.tui" />
    
    <!-- Exclude specific files -->
    <AdditionalFiles Remove="Pages\Draft.tui" />
</ItemGroup>
```

### Generated File Location

Generated files appear in:
```
obj/Debug/net8.0/generated/Rndr.Razor/
├── Counter.g.cs
├── Home.g.cs
└── Log.g.cs
```

---

## Diagnostic Contract

### Error Codes

| Code | Severity | Message Template |
|------|----------|-----------------|
| TUI001 | Error | `.tui file must start with @view directive` |
| TUI002 | Error | `@view directive must be first non-whitespace content` |
| TUI003 | Error | `Unknown tag '{0}'. Valid tags: Column, Row, Panel, Text, Button, Spacer, Centered, TextInput` |
| TUI004 | Error | `Unknown attribute '{0}' on <{1}>` |
| TUI005 | Error | `Invalid value '{0}' for attribute '{1}'. Expected {2}` |
| TUI006 | Error | `Unclosed tag <{0}> at line {1}` |
| TUI007 | Error | `Unexpected closing tag </{0}>. Expected </{1}>` |
| TUI008 | Error | `Syntax error in @code block: {0}` |
| TUI009 | Warning | `Empty @code block can be removed` |
| TUI010 | Info | `Generated {0} from {1}` |

### Error Reporting Format

```
Pages/Counter.tui(5,3): error TUI003: Unknown tag 'Colum'. Valid tags: Column, Row, Panel, Text, Button, Spacer, Centered, TextInput
```

---

## Framework Integration Contract

### Component Registration

```csharp
var app = TuiApplication.CreateBuilder(args).Build();

// Register generated component
app.MapView("/", typeof(Counter));
app.MapView("/log", typeof(Log));

await app.RunAsync();
```

### Inject Property Population

The framework must populate `@inject` properties before `AttachContext()`:

```csharp
// In TuiApp.MapView(route, Type) implementation:
internal void RenderView(Type componentType, ViewContext context)
{
    var component = (TuiComponentBase)ActivatorUtilities.CreateInstance(
        Services, componentType);
    
    // Populate @inject properties
    foreach (var property in componentType.GetProperties())
    {
        if (property.CanWrite && property.GetSetMethod() != null)
        {
            var service = Services.GetService(property.PropertyType);
            if (service != null)
            {
                property.SetValue(component, service);
            }
        }
    }
    
    component.AttachContext(context);
    
    var layout = new LayoutBuilder();
    component.Build(layout);
    // ... render layout
}
```

**Note**: This reflection-based approach is for the runtime. Generated components are still AOT-friendly because the generated code itself has no reflection.

---

## Version Compatibility

| Rndr.Razor Version | Rndr Version | .NET Version |
|-------------------|--------------|--------------|
| 1.0.x | 1.0.x | net8.0+ |

### Breaking Change Policy

- **Major version**: Changes to .tui file format, generated code structure
- **Minor version**: New tags, attributes, or directives
- **Patch version**: Bug fixes, improved error messages

