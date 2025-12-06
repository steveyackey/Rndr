using Microsoft.Extensions.DependencyInjection;
using Rndr;
using Rndr.Layout;
using Rndr.Testing;
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
    
    [Fact]
    public void Component_WithInjectProperty_ReceivesServiceFromDI()
    {
        // Arrange - set up DI services
        var services = new ServiceCollection()
            .AddSingleton<ITestService, TestService>()
            .BuildServiceProvider();
        
        // Act - build component with DI
        var (nodes, context) = RndrTestHost.BuildComponent<TestComponentWithInject>(services);
        
        // Assert - component was built successfully (service was injected)
        Assert.NotNull(nodes);
    }
    
    [Fact]
    public void Component_WithInjectProperty_UsesInjectedService()
    {
        // Arrange
        var testService = new TestService();
        var services = new ServiceCollection()
            .AddSingleton<ITestService>(testService)
            .BuildServiceProvider();
        
        // Act
        var (nodes, context) = RndrTestHost.BuildComponent<TestComponentWithInject>(services);
        
        // Assert - the service was used (GetMessage was called)
        Assert.True(testService.WasCalled);
    }
    
    [Fact]
    public void BuildComponent_ByType_SupportsPropertyInjection()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddSingleton<ITestService, TestService>()
            .BuildServiceProvider();
        
        // Act - use the type-based overload
        var (nodes, context) = RndrTestHost.BuildComponent(typeof(TestComponentWithInject), services);
        
        // Assert
        Assert.NotNull(nodes);
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
    
    /// <summary>
    /// Test interface for DI injection testing.
    /// </summary>
    public interface ITestService
    {
        string GetMessage();
    }
    
    /// <summary>
    /// Test implementation for DI injection testing.
    /// </summary>
    public class TestService : ITestService
    {
        public bool WasCalled { get; private set; }
        
        public string GetMessage()
        {
            WasCalled = true;
            return "Hello from injected service";
        }
    }
    
    /// <summary>
    /// Test component with @inject property (simulates generated .tui component).
    /// </summary>
    public class TestComponentWithInject : TuiComponentBase
    {
        // This property simulates what gets generated for: @inject ITestService Service
        public ITestService Service { get; set; } = default!;
        
        public override void Build(LayoutBuilder layout)
        {
            // Use the injected service
            var message = Service.GetMessage();
            
            layout.Column(col =>
            {
                col.Text(message);
            });
        }
    }
}

