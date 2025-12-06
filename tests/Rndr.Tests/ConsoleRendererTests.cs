using Rndr.Layout;
using Rndr.Rendering;
using Rndr.Testing;
using Xunit;

namespace Rndr.Tests;

public class ConsoleRendererTests
{
    [Fact]
    public void Render_EmptyNodes_DoesNotThrow()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        var renderer = new ConsoleRenderer(console, theme);
        var nodes = new List<Node>();

        // Act & Assert (should not throw)
        renderer.Render(nodes);
    }

    [Fact]
    public void Render_Panel_OutputsBoxCharacters()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        var renderer = new ConsoleRenderer(console, theme);

        var builder = new LayoutBuilder();
        builder.Column(col =>
        {
            col.Panel("Test Panel", panel =>
            {
                panel.Column(inner => inner.Text("Content"));
            });
        });

        // Act
        renderer.Render(builder.Build());

        // Assert
        var output = console.GetOutput();
        Assert.Contains("╭", output); // Top-left corner (rounded)
        Assert.Contains("╯", output); // Bottom-right corner
        Assert.Contains("Test Panel", output);
        Assert.Contains("Content", output);
    }

    [Fact]
    public void Render_SquareBorder_UsesSquareCharacters()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        theme.Panel.BorderStyle = BorderStyle.Square;
        var renderer = new ConsoleRenderer(console, theme);

        var builder = new LayoutBuilder();
        builder.Column(col =>
        {
            col.Panel("Square", _ => { });
        });

        // Act
        renderer.Render(builder.Build());

        // Assert
        var output = console.GetOutput();
        Assert.Contains("┌", output); // Square top-left
        Assert.Contains("┘", output); // Square bottom-right
    }

    [Fact]
    public void Render_Button_ShowsFocusIndicator()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        var renderer = new ConsoleRenderer(console, theme);

        var builder = new LayoutBuilder();
        builder.Column(col =>
        {
            col.Button("Click Me", null);
        });

        // Act - Render with button 0 focused
        renderer.Render(builder.Build(), focusedButtonIndex: 0);

        // Assert
        var output = console.GetOutput();
        Assert.Contains("[ Click Me ]", output);
    }

    [Fact]
    public void Render_Button_UnfocusedHasNoIndicator()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        var renderer = new ConsoleRenderer(console, theme);

        var builder = new LayoutBuilder();
        builder.Column(col =>
        {
            col.Button("Click Me", null);
        });

        // Act - Render with no focus
        renderer.Render(builder.Build(), focusedButtonIndex: -1);

        // Assert
        var output = console.GetOutput();
        Assert.Contains("Click Me", output);
        Assert.DoesNotContain("[ Click Me ]", output);
    }

    [Fact]
    public void Render_Text_OutputsContent()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        var renderer = new ConsoleRenderer(console, theme);

        var builder = new LayoutBuilder();
        builder.Column(col =>
        {
            col.Text("Hello World");
        });

        // Act
        renderer.Render(builder.Build());

        // Assert
        var output = console.GetOutput();
        Assert.Contains("Hello World", output);
    }

    [Fact]
    public void Render_Row_ArrangesHorizontally()
    {
        // Arrange
        var console = new FakeConsoleAdapter(80, 24);
        var theme = new RndrTheme();
        var renderer = new ConsoleRenderer(console, theme);

        var builder = new LayoutBuilder();
        builder.Row(row =>
        {
            row.Button("A", null);
            row.Button("B", null);
        });

        // Act
        renderer.Render(builder.Build());

        // Assert
        var output = console.GetOutput();
        Assert.Contains("A", output);
        Assert.Contains("B", output);
    }
}

