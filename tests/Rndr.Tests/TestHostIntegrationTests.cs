using Rndr.Layout;
using Rndr.Testing;
using Xunit;

namespace Rndr.Tests;

public class TestHostIntegrationTests
{
    [Fact]
    public void BuildView_CreatesNodesAndContext()
    {
        // Arrange & Act
        var (nodes, context) = RndrTestHost.BuildView(view =>
        {
            view.Title("Test View")
                .Use((ctx, layout) =>
                {
                    layout.Column(col =>
                    {
                        col.Text("Hello World");
                        col.Button("Click Me", () => { });
                    });
                });
        });

        // Assert
        Assert.NotEmpty(nodes);
        Assert.NotNull(context);
        Assert.Equal("/", context.Route);
    }

    [Fact]
    public void BuildView_WithState_PreservesStateBetweenBuilds()
    {
        // Arrange
        var stateStore = new InMemoryStateStore();
        int buildCount = 0;

        // Act - First build
        var (nodes1, context1) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                var count = ctx.State("count", 0);
                count.Value = 42;
                buildCount++;
                layout.Column(col => col.Text($"Count: {count.Value}"));
            });
        }, stateStore: stateStore);

        // Act - Second build with same store
        var (nodes2, context2) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                var count = ctx.State("count", 0);
                buildCount++;
                layout.Column(col => col.Text($"Count: {count.Value}"));
            });
        }, stateStore: stateStore);

        // Assert
        Assert.Equal(2, buildCount);
        var text = nodes2.FindText("Count:");
        Assert.NotNull(text);
        Assert.Contains("42", text!.Content);
    }

    [Fact]
    public void FindButton_ReturnsCorrectButton()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Button("Save", () => { });
                    col.Button("Cancel", () => { });
                });
            });
        });

        // Act
        var saveButton = nodes.FindButton("Save");
        var cancelButton = nodes.FindButton("Cancel");
        var missingButton = nodes.FindButton("Delete");

        // Assert
        Assert.NotNull(saveButton);
        Assert.Equal("Save", saveButton!.Label);
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton!.Label);
        Assert.Null(missingButton);
    }

    [Fact]
    public void FindText_ReturnsMatchingText()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Text("Welcome to Rndr!");
                    col.Text("Version 1.0");
                });
            });
        });

        // Act
        var welcomeText = nodes.FindText("Welcome");
        var versionText = nodes.FindText("Version");

        // Assert
        Assert.NotNull(welcomeText);
        Assert.Contains("Welcome", welcomeText!.Content);
        Assert.NotNull(versionText);
    }

    [Fact]
    public void FindPanel_ReturnsCorrectPanel()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Panel("Settings", _ => { });
                    col.Panel("About", _ => { });
                });
            });
        });

        // Act
        var settingsPanel = nodes.FindPanel("Settings");

        // Assert
        Assert.NotNull(settingsPanel);
        Assert.Equal("Settings", settingsPanel!.Title);
    }

    [Fact]
    public void GetAllButtons_ReturnsAllButtons()
    {
        // Arrange
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Button("One", null);
                    col.Panel("Panel", panel =>
                    {
                        panel.Row(row =>
                        {
                            row.Button("Two", null);
                            row.Button("Three", null);
                        });
                    });
                });
            });
        });

        // Act
        var buttons = nodes.GetAllButtons();

        // Assert
        Assert.Equal(3, buttons.Count);
    }

    [Fact]
    public void Button_OnClick_CanBeInvoked()
    {
        // Arrange
        var clicked = false;
        var (nodes, _) = RndrTestHost.BuildView(view =>
        {
            view.Use((ctx, layout) =>
            {
                layout.Column(col =>
                {
                    col.Button("Click Me", () => clicked = true);
                });
            });
        });

        // Act
        var button = nodes.FindButton("Click Me");
        button?.OnClick?.Invoke();

        // Assert
        Assert.True(clicked);
    }
}

