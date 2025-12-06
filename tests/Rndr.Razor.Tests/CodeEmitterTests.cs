using Rndr.Razor.Generator;
using Rndr.Razor.Parsing;
using Xunit;

namespace Rndr.Razor.Tests;

/// <summary>
/// Tests for TuiCodeEmitter.
/// </summary>
public class CodeEmitterTests
{
    [Fact]
    public void EmitComponent_WithMinimalComponent_GeneratesValidClass()
    {
        // Arrange
        var content = @"@view

<Column Padding=""1"">
    <Text Bold=""true"">Hello, World!</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Hello.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("namespace TestNamespace;", source);
        Assert.Contains("public partial class Hello : TuiComponentBase", source);
        Assert.Contains("public override void Build(LayoutBuilder layout)", source);
        Assert.Contains("layout.Column(", source);
        Assert.Contains(".Padding(1)", source);
        Assert.Contains(".Text(\"Hello, World!\"", source);
        Assert.Contains("s.Bold = true", source);
    }

    [Fact]
    public void EmitComponent_WithUsingDirectives_IncludesUsings()
    {
        // Arrange
        var content = @"@view
@using System.Linq
@using MyApp.Models

<Column>
    <Text>Hello</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("using System.Linq;", source);
        Assert.Contains("using MyApp.Models;", source);
    }

    [Fact]
    public void EmitComponent_WithInjectDirective_GeneratesProperty()
    {
        // Arrange
        var content = @"@view
@inject ILogger<Test> Logger

<Column>
    <Text>Hello</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("public ILogger<Test> Logger { get; set; } = default!", source);
    }

    [Fact]
    public void EmitComponent_WithCodeBlock_IncludesCodeBlockContent()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Hello</Text>
</Column>

@code {
    private Signal<int> count = default!;
    
    void Increment() => count.Value++;
}";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("private Signal<int> count = default!", source);
        Assert.Contains("void Increment() => count.Value++;", source);
        // Should also generate state initialization
        Assert.Contains("count = State(\"count\"", source);
    }

    [Fact]
    public void EmitComponent_WithButton_GeneratesButtonCall()
    {
        // Arrange
        var content = @"@view

<Column>
    <Button OnClick=""@HandleClick"">Click Me</Button>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Button(\"Click Me\", HandleClick)", source);
    }

    [Fact]
    public void EmitComponent_WithLambdaOnClick_GeneratesLambda()
    {
        // Arrange
        var content = @"@view

<Column>
    <Button OnClick=""@(() => count.Value++)"">+</Button>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Button(\"+\"", source);
        Assert.Contains("count.Value++", source);
    }

    [Fact]
    public void EmitComponent_WithPanel_GeneratesNestedStructure()
    {
        // Arrange
        var content = @"@view

<Column>
    <Panel Title=""My Panel"">
        <Text>Inside Panel</Text>
    </Panel>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Panel(\"My Panel\"", source);
        Assert.Contains(".Text(\"Inside Panel\")", source);
    }

    [Fact]
    public void EmitComponent_WithRow_GeneratesRowCall()
    {
        // Arrange
        var content = @"@view

<Column>
    <Row Gap=""2"">
        <Button OnClick=""@Dec"">-</Button>
        <Button OnClick=""@Inc"">+</Button>
    </Row>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Row(", source);
        Assert.Contains(".Gap(2)", source);
    }

    [Fact]
    public void EmitComponent_WithSpacer_GeneratesSpacerCall()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Top</Text>
    <Spacer />
    <Text>Bottom</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Spacer()", source);
    }

    [Fact]
    public void EmitComponent_WithTextInput_GeneratesTextInputCall()
    {
        // Arrange
        var content = @"@view

<Column>
    <TextInput Value=""@name.Value"" OnChanged=""@(v => name.Value = v)"" Placeholder=""Enter name"" />
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".TextInput(", source);
        Assert.Contains("name.Value", source);
        Assert.Contains("\"Enter name\"", source);
    }

    [Fact]
    public void EmitComponent_WithExpressionInText_GeneratesInterpolatedString()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Count: @count.Value</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("$\"Count: {count.Value}\"", source);
    }

    [Fact]
    public void EmitComponent_WithAutoGeneratedComment_IncludesHeader()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Hello</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("// <auto-generated />", source);
        Assert.Contains("#nullable enable", source);
    }

    [Fact]
    public void EmitComponent_WithCentered_GeneratesCenteredCall()
    {
        // Arrange
        var content = @"@view

<Centered>
    <Panel Title=""Welcome"">
        <Text>Hello</Text>
    </Panel>
</Centered>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains(".Centered(", source);
        Assert.Contains(".Panel(\"Welcome\"", source);
    }

    [Fact]
    public void EmitComponent_WithMultipleStyleAttributes_GeneratesAllStyles()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text Bold=""true"" Accent=""true"" Faint=""true"">Styled</Text>
</Column>";

        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Act
        var source = TuiCodeEmitter.EmitComponent(document, "TestNamespace");

        // Assert
        Assert.Contains("s.Bold = true", source);
        Assert.Contains("s.Accent = true", source);
        Assert.Contains("s.Faint = true", source);
    }
}

