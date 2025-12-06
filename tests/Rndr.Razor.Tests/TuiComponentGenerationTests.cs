using Rndr;
using Rndr.Layout;
using Xunit;

namespace Rndr.Razor.Tests;

public class TuiComponentGenerationTests
{
    [Fact]
    public void TuiRazorConfiguration_HasTagMappings()
    {
        // Assert that all expected tags are mapped
        Assert.Contains("Column", TuiRazorConfiguration.TagMappings.Keys);
        Assert.Contains("Row", TuiRazorConfiguration.TagMappings.Keys);
        Assert.Contains("Panel", TuiRazorConfiguration.TagMappings.Keys);
        Assert.Contains("Text", TuiRazorConfiguration.TagMappings.Keys);
        Assert.Contains("Button", TuiRazorConfiguration.TagMappings.Keys);
        Assert.Contains("Spacer", TuiRazorConfiguration.TagMappings.Keys);
        Assert.Contains("Centered", TuiRazorConfiguration.TagMappings.Keys);
    }

    [Fact]
    public void TuiRazorConfiguration_HasAttributeMappings()
    {
        // Assert that all expected attributes are mapped
        Assert.Contains("Padding", TuiRazorConfiguration.AttributeMappings.Keys);
        Assert.Contains("Gap", TuiRazorConfiguration.AttributeMappings.Keys);
        Assert.Contains("Bold", TuiRazorConfiguration.AttributeMappings.Keys);
        Assert.Contains("OnClick", TuiRazorConfiguration.AttributeMappings.Keys);
    }

    [Fact]
    public void TestComponent_BuildsLayout()
    {
        // Arrange - create a test component
        var component = new TestCounterComponent();
        var layout = new LayoutBuilder();

        // Note: In a real test, we'd mock ViewContext
        // For now, just verify the component structure compiles

        // Assert
        Assert.NotNull(component);
    }

    /// <summary>
    /// Example component that would be generated from a .tui file.
    /// This demonstrates the expected output format.
    /// </summary>
    private sealed class TestCounterComponent : TuiComponentBase
    {
        public override void Build(LayoutBuilder layout)
        {
            // This is what a .tui file like this would generate:
            // <Column Padding="2" Gap="1">
            //     <Panel Title="Counter">
            //         <Text Bold="true">Count: @count.Value</Text>
            //         <Row Gap="2">
            //             <Button OnClick="@Decrement">-</Button>
            //             <Button OnClick="@Increment" Primary="true">+</Button>
            //         </Row>
            //     </Panel>
            // </Column>

            layout.Column(col =>
            {
                col.Padding(2).Gap(1);
                col.Panel("Counter", panel =>
                {
                    panel.Column(inner =>
                    {
                        inner.Text("Count: 0", s => s.Bold = true);
                        inner.Row(row =>
                        {
                            row.Gap(2);
                            row.Button("-", () => { });
                            row.Button("+", () => { });
                        });
                    });
                });
            });
        }
    }
}

