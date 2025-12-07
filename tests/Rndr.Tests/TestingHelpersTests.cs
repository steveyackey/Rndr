using Rndr.Testing;
using Rndr.Layout;
using Xunit;

namespace Rndr.Tests;

public class TestingHelpersTests
{
    [Fact]
    public void ClickButton_FindsAndClicksButton()
    {
        // Arrange
        bool clicked = false;
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Button("Test Button", () => clicked = true);
                });
            });
        });

        // Act
        var result = nodes.ClickButton("Test Button");

        // Assert
        Assert.True(result);
        Assert.True(clicked);
    }

    [Fact]
    public void ClickButton_ReturnsFalseWhenButtonNotFound()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Text("No buttons here");
                });
            });
        });

        // Act
        var result = nodes.ClickButton("Missing Button");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AssertButtonExists_ReturnsButtonWhenFound()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Button("Save", null);
                });
            });
        });

        // Act
        var button = nodes.AssertButtonExists("Save");

        // Assert
        Assert.NotNull(button);
        Assert.Equal("Save", button.Label);
    }

    [Fact]
    public void AssertButtonExists_ThrowsWhenNotFound()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Text("No buttons");
                });
            });
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            nodes.AssertButtonExists("Save"));
        Assert.Contains("Save", exception.Message);
    }

    [Fact]
    public void AssertTextExists_ReturnsTextWhenFound()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Text("Hello World");
                });
            });
        });

        // Act
        var text = nodes.AssertTextExists("Hello");

        // Assert
        Assert.NotNull(text);
        Assert.Contains("Hello", text.Content);
    }

    [Fact]
    public void AssertTextExists_ThrowsWhenNotFound()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Button("Just a button", null);
                });
            });
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            nodes.AssertTextExists("Hello"));
        Assert.Contains("Hello", exception.Message);
    }
}
