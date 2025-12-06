using Rndr.Razor.Generator;
using Rndr.Razor.Parsing;
using Xunit;

namespace Rndr.Razor.Tests;

/// <summary>
/// Snapshot tests for verifying generated code output.
/// </summary>
public class SnapshotTests
{
    [Fact]
    public void MinimalComponent_GeneratesExpectedCode()
    {
        // Arrange
        var content = @"@view

<Column Padding=""1"">
    <Text Bold=""true"">Hello, World!</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Minimal.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert - verify key components of the generated code
        Assert.Contains("namespace TestNamespace;", source);
        Assert.Contains("public partial class Minimal : TuiComponentBase", source);
        Assert.Contains("public override void Build(LayoutBuilder layout)", source);
        Assert.Contains("layout.Column(col =>", source);
        Assert.Contains("col.Padding(1)", source);
        Assert.Contains("col.Text(\"Hello, World!\"", source);
        Assert.Contains("s.Bold = true", source);
    }

    [Fact]
    public void CounterComponent_GeneratesStateInitialization()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Count: @count.Value</Text>
    <Button OnClick=""@Increment"">+</Button>
</Column>

@code {
    private Signal<int> count = default!;
    
    void Increment() => count.Value++;
}";

        var document = TuiSyntaxParser.ParseDocument("Counter.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("private Signal<int> count = default!", source);
        Assert.Contains("void Increment() => count.Value++;", source);
        Assert.Contains("count = State(\"count\", 0);", source);
        Assert.Contains("$\"Count: {count.Value}\"", source);
        Assert.Contains(".Button(\"+\", Increment);", source);
    }

    [Fact]
    public void ComponentWithOnInit_SkipsStateInitializationForInitializedSignals()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>@state.Value.Name</Text>
</Column>

@code {
    private Signal<MyState> state = default!;
    
    protected override void OnInit()
    {
        state = StateGlobal(""appState"", new MyState());
    }
}";

        var document = TuiSyntaxParser.ParseDocument("OnInitComponent.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert - state should NOT be initialized in Build() since it's done in OnInit()
        Assert.Contains("protected override void OnInit()", source);
        Assert.Contains("state = StateGlobal(\"appState\", new MyState());", source);
        
        // The Build() method should NOT have State() call for 'state'
        var buildMethodStart = source.IndexOf("public override void Build(LayoutBuilder layout)");
        var buildMethodContent = source.Substring(buildMethodStart);
        Assert.DoesNotContain("state = State(", buildMethodContent);
    }

    [Fact]
    public void ComponentWithInject_GeneratesProperty()
    {
        // Arrange
        var content = @"@view
@inject ILogger<MyComponent> Logger

<Column>
    <Text>Hello</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("InjectComponent.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("public ILogger<MyComponent> Logger { get; set; } = default!", source);
    }

    [Fact]
    public void ComponentWithUsing_GeneratesUsingDirectives()
    {
        // Arrange
        var content = @"@view
@using System.Linq
@using MyApp.Models

<Column>
    <Text>Hello</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("UsingComponent.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("using System.Linq;", source);
        Assert.Contains("using MyApp.Models;", source);
    }

    [Fact]
    public void ComponentWithControlFlow_GeneratesIfForeachStatements()
    {
        // Arrange
        var content = @"@view

<Column>
    @if (showDetails)
    {
        <Text>Details shown</Text>
    }
    else
    {
        <Text Faint=""true"">Hidden</Text>
    }
    
    @foreach (var item in items)
    {
        <Text>@item</Text>
    }
</Column>

@code {
    private bool showDetails = true;
    private List<string> items = new();
}";

        var document = TuiSyntaxParser.ParseDocument("ControlFlowComponent.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("if (showDetails)", source);
        Assert.Contains("else", source);
        Assert.Contains("foreach (var item in items)", source);
    }

    [Fact]
    public void ComponentWithPanel_GeneratesNestedStructure()
    {
        // Arrange
        var content = @"@view

<Panel Title=""My Panel"">
    <Column>
        <Text>Inside Panel</Text>
    </Column>
</Panel>";

        var document = TuiSyntaxParser.ParseDocument("PanelComponent.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Panel(\"My Panel\", panel =>", source);
    }

    [Fact]
    public void ComponentWithAllTags_GeneratesCorrectBuilderCalls()
    {
        // Arrange
        var content = @"@view

<Column Padding=""1"" Gap=""1"">
    <Row Gap=""2"">
        <Text Bold=""true"">Hello</Text>
        <Button OnClick=""@Click"">Press</Button>
    </Row>
    <Centered>
        <Text>Centered text</Text>
    </Centered>
    <Spacer />
    <TextInput Value=""@text.Value"" OnChanged=""@(v => text.Value = v)"" Placeholder=""Enter..."" />
</Column>";

        var document = TuiSyntaxParser.ParseDocument("AllTags.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Column(col =>", source);
        Assert.Contains(".Row(row =>", source);
        Assert.Contains(".Text(\"Hello\"", source);
        Assert.Contains(".Button(\"Press\", Click);", source);
        Assert.Contains(".Centered(centered =>", source);
        Assert.Contains(".Spacer();", source);
        Assert.Contains(".TextInput(", source);
    }
}

