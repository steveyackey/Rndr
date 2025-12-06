# Data Model: .tui Razor Source Generator

**Feature**: 002-tui-razor-generator  
**Date**: 2025-12-06

## Overview

This document defines the entities used by the .tui source generator. Since this is a build-time tool, the "data model" focuses on the parsed representation of .tui files and the intermediate structures used during code generation.

---

## Core Entities

### TuiDocument

The parsed representation of a complete `.tui` file.

| Field | Type | Description |
|-------|------|-------------|
| FilePath | string | Absolute path to the .tui file |
| FileName | string | File name without extension (becomes class name) |
| ViewDirective | TuiViewDirective | Required @view directive |
| UsingDirectives | List\<TuiUsingDirective\> | @using namespace imports |
| InjectDirectives | List\<TuiInjectDirective\> | @inject service declarations |
| CodeBlock | TuiCodeBlock? | Optional @code { ... } content |
| RootMarkup | List\<TuiMarkupNode\> | Top-level markup nodes |
| Diagnostics | List\<TuiDiagnostic\> | Parsing errors/warnings |

**Relationships**:
- Contains one TuiViewDirective (required)
- Contains zero or more TuiUsingDirective
- Contains zero or more TuiInjectDirective
- Contains zero or one TuiCodeBlock
- Contains one or more TuiMarkupNode (root elements)
- May contain TuiDiagnostics from parsing

**State Transitions**:
```
Raw Text → Parsed → Validated → Generated
              ↓         ↓
          HasErrors  HasErrors
```

---

### TuiViewDirective

The required `@view` directive that identifies the file as a TUI component.

| Field | Type | Description |
|-------|------|-------------|
| Location | SourceLocation | Position in source file |

**Validation Rules**:
- Must be present
- Must be first non-whitespace content in file
- Only one allowed per file

---

### TuiUsingDirective

A `@using` directive for namespace imports.

| Field | Type | Description |
|-------|------|-------------|
| Namespace | string | The namespace to import |
| Location | SourceLocation | Position in source file |

**Examples**:
- `@using System.Linq`
- `@using Rndr.Samples.MyFocusTui.Models`

---

### TuiInjectDirective

An `@inject` directive for dependency injection.

| Field | Type | Description |
|-------|------|-------------|
| TypeName | string | The service type (e.g., "ILogger\<Counter\>") |
| PropertyName | string | The property name to generate |
| Location | SourceLocation | Position in source file |

**Examples**:
- `@inject ILogger<Counter> Logger` → Type: `ILogger<Counter>`, Property: `Logger`
- `@inject ITimeService Time` → Type: `ITimeService`, Property: `Time`

**Generated Code**:
```csharp
public ILogger<Counter> Logger { get; set; } = default!;
```

---

### TuiCodeBlock

The `@code { ... }` block containing C# class members.

| Field | Type | Description |
|-------|------|-------------|
| Content | string | Raw C# code inside the braces |
| Location | SourceLocation | Position in source file |
| OpenBraceLocation | SourceLocation | Position of `{` |
| CloseBraceLocation | SourceLocation | Position of `}` |

**Notes**:
- Content is pasted verbatim as class members
- Includes fields, methods, properties, nested types
- Variables declared here can be referenced in markup

---

### TuiMarkupNode

A node in the markup tree (tag element or text content).

| Field | Type | Description |
|-------|------|-------------|
| NodeType | MarkupNodeType | Element, Text, Expression, ControlFlow |
| TagName | string? | Tag name for Element nodes |
| Attributes | List\<TuiAttribute\> | Attributes on element |
| Children | List\<TuiMarkupNode\> | Child nodes |
| TextContent | string? | Text for Text/Expression nodes |
| Location | SourceLocation | Position in source file |
| IsSelfClosing | bool | True for `<Tag />` syntax |

**MarkupNodeType Enum**:
- `Element`: A tag like `<Column>` or `<Button>`
- `Text`: Raw text content
- `Expression`: Razor expression like `@count.Value`
- `ControlFlow`: `@if`, `@foreach`, `@switch` blocks

---

### TuiAttribute

An attribute on a markup element.

| Field | Type | Description |
|-------|------|-------------|
| Name | string | Attribute name (e.g., "Padding", "OnClick") |
| Value | TuiAttributeValue | The attribute value |
| Location | SourceLocation | Position in source file |

---

### TuiAttributeValue

The value of an attribute.

| Field | Type | Description |
|-------|------|-------------|
| ValueType | AttributeValueType | Literal, Expression, Lambda |
| RawValue | string | The raw string value |
| ParsedValue | object? | Parsed value (int, bool, etc.) |

**AttributeValueType Enum**:
- `Literal`: Plain value like `"10"` or `"Center"`
- `Expression`: Razor expression like `@count.Value`
- `Lambda`: Lambda expression like `@(() => count.Value++)`
- `MethodReference`: Method name like `@Increment`

---

### SourceLocation

Source file position for error reporting.

| Field | Type | Description |
|-------|------|-------------|
| FilePath | string | Path to the .tui file |
| Line | int | 1-based line number |
| Column | int | 1-based column number |
| Length | int | Length of the span |

---

### TuiDiagnostic

A parsing or validation error/warning.

| Field | Type | Description |
|-------|------|-------------|
| Code | string | Diagnostic code (e.g., "TUI001") |
| Severity | DiagnosticSeverity | Error, Warning, Info |
| Message | string | Human-readable message |
| Location | SourceLocation | Position in source file |

---

## Tag Mapping Entities

### TagMapping

Defines how a markup tag maps to C# code.

| Field | Type | Description |
|-------|------|-------------|
| TagName | string | e.g., "Column", "Button" |
| IsContainer | bool | Whether tag has child content |
| BuilderMethod | string | Method name on parent builder |
| RequiredParams | List\<string\> | Required positional parameters |
| OptionalParams | List\<string\> | Optional named parameters |

**Existing Mappings** (from TuiRazorConfiguration):

| Tag | Container | Builder Method |
|-----|-----------|---------------|
| Column | Yes | Column(Action\<ColumnBuilder\>) |
| Row | Yes | Row(Action\<RowBuilder\>) |
| Panel | Yes | Panel(string title, Action\<LayoutBuilder\>) |
| Centered | Yes | Centered(Action\<LayoutBuilder\>) |
| Text | No | Text(string, Action\<NodeStyle\>?) |
| Button | No | Button(string, Action?) |
| Spacer | No | Spacer(int weight = 1) |
| TextInput | No | TextInput(string, Action\<string\>?, string?) |

---

### AttributeMapping

Defines how a markup attribute maps to C# code.

| Field | Type | Description |
|-------|------|-------------|
| AttributeName | string | e.g., "Padding", "Bold" |
| TargetType | AttributeTargetType | Style, FluentMethod, Parameter |
| PropertyName | string | Target property or method |
| ValueType | Type | Expected value type |

**Existing Mappings** (from TuiRazorConfiguration):

| Attribute | Target | Type |
|-----------|--------|------|
| Padding | FluentMethod `Padding(int)` | int |
| Gap | FluentMethod `Gap(int)` | int |
| Bold | StyleProperty `style.Bold` | bool |
| Accent | StyleProperty `style.Accent` | bool |
| Faint | StyleProperty `style.Faint` | bool |
| Align | StyleProperty `style.Align` | TextAlign |
| Width | ButtonProperty `Width` | int |
| Primary | ButtonProperty `IsPrimary` | bool |
| OnClick | Parameter | Action |
| Title | Parameter | string |
| Placeholder | Parameter | string |

---

## Code Generation Entities

### GeneratedComponent

The output of code generation for a .tui file.

| Field | Type | Description |
|-------|------|-------------|
| ClassName | string | Generated class name |
| Namespace | string | Generated namespace |
| SourceCode | string | Complete C# source code |
| HintName | string | File name hint for generator |

---

### CodeGenerationContext

Context passed through code generation.

| Field | Type | Description |
|-------|------|-------------|
| Document | TuiDocument | The parsed .tui document |
| Indentation | int | Current indentation level |
| CurrentBuilder | string | Current builder variable name |
| UsedVariables | HashSet\<string\> | Track used variable names |

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        TuiDocument                               │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐    │
│  │TuiViewDirective│  │TuiUsingDirective│ │TuiInjectDirective│   │
│  │    (one)       │  │    (many)       │ │    (many)        │   │
│  └────────────────┘  └────────────────┘  └────────────────┘    │
│                                                                  │
│  ┌────────────────┐  ┌────────────────────────────────────┐    │
│  │  TuiCodeBlock  │  │         TuiMarkupNode              │    │
│  │   (optional)   │  │            (tree)                  │    │
│  └────────────────┘  └────────────────────────────────────┘    │
│                               │                                  │
│                               ▼                                  │
│                      ┌────────────────┐                         │
│                      │  TuiAttribute  │                         │
│                      │    (many)      │                         │
│                      └───────┬────────┘                         │
│                              │                                   │
│                              ▼                                   │
│                      ┌──────────────────┐                       │
│                      │TuiAttributeValue │                       │
│                      └──────────────────┘                       │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Code Generation
                              ▼
                    ┌──────────────────┐
                    │GeneratedComponent│
                    │                  │
                    │  - ClassName     │
                    │  - Namespace     │
                    │  - SourceCode    │
                    └──────────────────┘
```

---

## Processing Pipeline

### Parse Phase

1. **Tokenize**: Split .tui content into tokens (directives, tags, text)
2. **Parse Directives**: Extract @view, @using, @inject, @code
3. **Parse Markup**: Build tree of TuiMarkupNode from remaining content
4. **Collect Diagnostics**: Record any parsing errors with locations

### Validate Phase

1. **Check @view**: Ensure present and first
2. **Validate Tags**: All tags exist in TagMappings
3. **Validate Attributes**: All attributes exist in AttributeMappings
4. **Type Check Values**: Attribute values parse to expected types
5. **Balance Tags**: All opened tags are properly closed

### Generate Phase

1. **Build Namespace**: Determine from file path and project
2. **Emit Usings**: Standard usings + @using directives
3. **Emit Class Declaration**: Partial class extending TuiComponentBase
4. **Emit Inject Properties**: One property per @inject directive
5. **Emit Code Block**: Paste @code content as members
6. **Emit Build Method**: Transform markup tree to builder calls

---

## Error Handling

### Diagnostic Codes

| Code | Severity | Condition |
|------|----------|-----------|
| TUI001 | Error | Missing @view directive |
| TUI002 | Error | @view not first directive |
| TUI003 | Error | Unknown tag: {tagName} |
| TUI004 | Error | Unknown attribute: {attrName} on {tagName} |
| TUI005 | Error | Invalid attribute value: {value} for {attrName} |
| TUI006 | Error | Unclosed tag: {tagName} |
| TUI007 | Error | Unexpected closing tag: {tagName} |
| TUI008 | Error | @code block syntax error: {details} |
| TUI009 | Warning | Empty @code block |
| TUI010 | Warning | Unused @inject: {propertyName} |

### Recovery Strategy

- **Missing @view**: Generate placeholder comment, report error
- **Unknown tags**: Skip tag, report error, continue with children
- **Unknown attributes**: Ignore attribute, report warning
- **Unclosed tags**: Close implicitly at parent end, report error

