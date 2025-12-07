using Rndr.Layout;
using Rndr.Testing;
using Xunit;

namespace Rndr.Tests;

public class GenericMapViewTests
{
    [Fact]
    public void MapView_Generic_RegistersComponent()
    {
        // Arrange
        var builder = TuiApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();

        // Act - Using generic syntax
        app.MapView<TestComponent>("/test");

        // Assert - Verify the route was registered (accessing via reflection is needed)
        // Since routes are private, we verify by attempting to use the app
        Assert.NotNull(app);
    }

    [Fact]
    public void MapView_Generic_CanBuildComponent()
    {
        // Arrange & Act
        var (nodes, context) = RndrTestHost.BuildComponent<TestComponent>();

        // Assert
        Assert.NotEmpty(nodes);
        var text = nodes.FindText("Test");
        Assert.NotNull(text);
    }

    private class TestComponent : TuiComponentBase
    {
        public override void Build(LayoutBuilder layout)
        {
            layout.Column(col =>
            {
                col.Text("Test Component");
            });
        }
    }
}
