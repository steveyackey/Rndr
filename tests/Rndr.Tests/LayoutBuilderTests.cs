using Rndr.Layout;
using Xunit;

namespace Rndr.Tests;

public class LayoutBuilderTests
{
    [Fact]
    public void Build_EmptyBuilder_ReturnsEmptyList()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        var nodes = builder.Build();

        // Assert
        Assert.Empty(nodes);
    }

    [Fact]
    public void Column_CreatesColumnNode()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(_ => { });
        var nodes = builder.Build();

        // Assert
        Assert.Single(nodes);
        Assert.IsType<ColumnNode>(nodes[0]);
    }

    [Fact]
    public void Row_CreatesRowNode()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Row(_ => { });
        var nodes = builder.Build();

        // Assert
        Assert.Single(nodes);
        Assert.IsType<RowNode>(nodes[0]);
    }

    [Fact]
    public void Column_WithText_AddsTextChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Text("Hello World");
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        Assert.Single(column.Children);
        var text = Assert.IsType<TextNode>(column.Children[0]);
        Assert.Equal("Hello World", text.Content);
    }

    [Fact]
    public void Column_WithButton_AddsButtonChild()
    {
        // Arrange
        var builder = new LayoutBuilder();
        var clicked = false;

        // Act
        builder.Column(col =>
        {
            col.Button("Click Me", () => clicked = true);
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        var button = Assert.IsType<ButtonNode>(column.Children[0]);
        Assert.Equal("Click Me", button.Label);
        button.OnClick?.Invoke();
        Assert.True(clicked);
    }

    [Fact]
    public void Column_WithPanel_AddsPanelChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Panel("My Panel", _ => { });
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        var panel = Assert.IsType<PanelNode>(column.Children[0]);
        Assert.Equal("My Panel", panel.Title);
    }

    [Fact]
    public void Column_WithSpacer_AddsSpacerChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Spacer(2);
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        var spacer = Assert.IsType<SpacerNode>(column.Children[0]);
        Assert.Equal(2, spacer.Weight);
    }

    [Fact]
    public void Column_WithCentered_AddsCenteredChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Centered(inner =>
            {
                inner.Column(c => c.Text("Centered Text"));
            });
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        Assert.IsType<CenteredNode>(column.Children[0]);
    }

    [Fact]
    public void Column_WithNestedRow_AddsRowChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Row(row =>
            {
                row.Text("Item 1");
                row.Text("Item 2");
            });
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        var row = Assert.IsType<RowNode>(column.Children[0]);
        Assert.Equal(2, row.Children.Count);
    }

    [Fact]
    public void Column_Gap_SetsStyleGap()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Gap(2);
            col.Text("Test");
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        Assert.Equal(2, column.Style.Gap);
    }

    [Fact]
    public void Column_Padding_SetsStylePadding()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Padding(3);
            col.Text("Test");
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        Assert.Equal(3, column.Style.Padding);
    }

    [Fact]
    public void Column_AlignCenter_SetsStyleAlign()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.AlignCenter();
            col.Text("Test");
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        Assert.Equal(TextAlign.Center, column.Style.Align);
    }

    [Fact]
    public void Text_WithStyleAction_AppliesStyle()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col =>
        {
            col.Text("Styled", style =>
            {
                style.Bold = true;
                style.Accent = true;
            });
        });
        var nodes = builder.Build();

        // Assert
        var column = Assert.IsType<ColumnNode>(nodes[0]);
        var text = Assert.IsType<TextNode>(column.Children[0]);
        Assert.True(text.Style.Bold);
        Assert.True(text.Style.Accent);
    }

    [Fact]
    public void Row_WithButton_AddsButtonChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Row(row =>
        {
            row.Button("-", null);
            row.Button("+", null);
        });
        var nodes = builder.Build();

        // Assert
        var row = Assert.IsType<RowNode>(nodes[0]);
        Assert.Equal(2, row.Children.Count);
        Assert.All(row.Children, child => Assert.IsType<ButtonNode>(child));
    }

    [Fact]
    public void Row_WithSpacer_AddsSpacerChild()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Row(row =>
        {
            row.Button("Left", null);
            row.Spacer();
            row.Button("Right", null);
        });
        var nodes = builder.Build();

        // Assert
        var row = Assert.IsType<RowNode>(nodes[0]);
        Assert.Equal(3, row.Children.Count);
        Assert.IsType<SpacerNode>(row.Children[1]);
    }

    [Fact]
    public void MultipleRootNodes_AllAdded()
    {
        // Arrange
        var builder = new LayoutBuilder();

        // Act
        builder.Column(col => col.Text("First"));
        builder.Column(col => col.Text("Second"));
        builder.Row(row => row.Text("Third"));
        var nodes = builder.Build();

        // Assert
        Assert.Equal(3, nodes.Count);
    }
}

