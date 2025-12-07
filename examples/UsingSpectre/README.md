# UsingSpectre Example

This example demonstrates how to use [Spectre.Console](https://spectreconsole.net/) within Rndr `.tui` files.

## Overview

This example shows that even though Rndr provides its own TUI components, you can still leverage Spectre.Console's rich text rendering capabilities, markup language, and other features within your `.tui` files.

## What It Demonstrates

The `Home.tui` file showcases several integrations:

1. **Spectre.Console Markup**: Using `Markup.Remove()` to safely render colored text
2. **Text Formatting**: Creating ASCII art, tables, and bar charts
3. **Rndr Components**: Mixing Spectre.Console utilities with Rndr's native components
4. **Reactive State**: Using Rndr's `Signal<T>` for state management alongside Spectre features

## Key Features

- **Color Examples**: Demonstrates rendering colored text with Spectre.Console markup
- **Table Rendering**: Shows ASCII table creation
- **Bar Charts**: Simple ASCII bar chart visualization
- **Interactive Counter**: Combines Rndr's reactive state with Spectre.Console progress visualization

## Running the Example

```bash
cd examples/UsingSpectre
dotnet run
```

## Usage

- **Tab**: Navigate between interactive elements
- **Enter/Space**: Click buttons
- **+/-**: Increment/decrement counter
- **Q**: Quit the application

## Integration Pattern

The example demonstrates that Spectre.Console and Rndr work well together:

```csharp
@view
@using Spectre.Console

<Panel Title="Color Examples">
    <Column Padding="1" Gap="1">
        <Text>@GetColoredText()</Text>
    </Column>
</Panel>

@code {
    string GetColoredText()
    {
        // Use Spectre.Console's Markup class
        return Markup.Remove("[green]Green text[/] and [red]red text[/]");
    }
}
```

## Notes

- Spectre.Console is added as a NuGet package dependency in the project file
- The `@using Spectre.Console` directive makes Spectre.Console types available in the `.tui` file
- Methods in the `@code` block can use any Spectre.Console API
- The generated text is rendered through Rndr's `Text` component

## Constitution Compliance

Per the Rndr constitution:
- **Core libraries** forbid Spectre.Console as a dependency (to maintain AOT compatibility and minimal core)
- **Sample/Example projects** MAY reference additional packages for demonstration purposes
- This example falls under "Samples Only" and is allowed to use Spectre.Console

This demonstrates the flexibility of the Rndr framework while maintaining clean separation between core and examples.
