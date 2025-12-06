using Rndr.Razor.Parsing;
using Xunit;

namespace Rndr.Razor.Tests;

/// <summary>
/// Tests for TuiSyntaxParser.
/// </summary>
public class ParsingTests
{
    [Fact]
    public void ParseDocument_WithViewDirective_Succeeds()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Hello</Text>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.NotNull(document.ViewDirective);
        Assert.False(document.HasErrors);
        Assert.Single(document.RootMarkup);
    }

    [Fact]
    public void ParseDocument_WithoutViewDirective_ReportsError()
    {
        // Arrange
        var content = @"<Column>
    <Text>Hello</Text>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.Null(document.ViewDirective);
        Assert.True(document.HasErrors);
        Assert.Contains(document.Diagnostics, d => d.Code == "TUI001");
    }

    [Fact]
    public void ParseDocument_WithNestedMarkup_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view

<Column Padding=""1"">
    <Panel Title=""Test"">
        <Text Bold=""true"">Hello</Text>
        <Button OnClick=""@DoSomething"">Click</Button>
    </Panel>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        Assert.Single(document.RootMarkup);
        
        var column = document.RootMarkup[0];
        Assert.Equal("Column", column.TagName);
        Assert.Single(column.Attributes);
        Assert.Equal("Padding", column.Attributes[0].Name);
        
        // Column should have one child (Panel)
        Assert.Single(column.Children);
        var panel = column.Children[0];
        Assert.Equal("Panel", panel.TagName);
    }

    [Fact]
    public void ParseDocument_WithUsingDirective_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view
@using System.Linq
@using MyApp.Models

<Column>
    <Text>Hello</Text>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        Assert.Equal(2, document.UsingDirectives.Count);
        Assert.Equal("System.Linq", document.UsingDirectives[0].Namespace);
        Assert.Equal("MyApp.Models", document.UsingDirectives[1].Namespace);
    }

    [Fact]
    public void ParseDocument_WithInjectDirective_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view
@inject ILogger<Test> Logger
@inject ITimeService TimeService

<Column>
    <Text>Hello</Text>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        Assert.Equal(2, document.InjectDirectives.Count);
        
        Assert.Equal("ILogger<Test>", document.InjectDirectives[0].TypeName);
        Assert.Equal("Logger", document.InjectDirectives[0].PropertyName);
        
        Assert.Equal("ITimeService", document.InjectDirectives[1].TypeName);
        Assert.Equal("TimeService", document.InjectDirectives[1].PropertyName);
    }

    [Fact]
    public void ParseDocument_WithCodeBlock_ParsesCorrectly()
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

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        Assert.NotNull(document.CodeBlock);
        Assert.Contains("private Signal<int> count", document.CodeBlock.Content);
        Assert.Contains("void Increment()", document.CodeBlock.Content);
    }

    [Fact]
    public void ParseDocument_WithUnknownTag_ReportsError()
    {
        // Arrange
        var content = @"@view

<Column>
    <UnknownTag>Hello</UnknownTag>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.True(document.HasErrors);
        Assert.Contains(document.Diagnostics, d => d.Code == "TUI003" && d.Message.Contains("UnknownTag"));
    }

    [Fact]
    public void ParseDocument_WithAllTags_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view

<Column Padding=""1"" Gap=""1"">
    <Row Gap=""2"">
        <Text Bold=""true"" Accent=""true"">Hello</Text>
        <Button OnClick=""@Click"">Click</Button>
    </Row>
    <Panel Title=""Test"">
        <Centered>
            <Text Faint=""true"">Centered</Text>
        </Centered>
    </Panel>
    <Spacer />
    <TextInput Value=""@value"" OnChanged=""@OnChange"" Placeholder=""Enter..."" />
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
    }

    [Fact]
    public void ParseDocument_WithExpressionInText_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view

<Column>
    <Text>Count: @count.Value</Text>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        
        var column = document.RootMarkup[0];
        var text = column.Children[0];
        
        // Should have text and expression children
        Assert.True(text.Children.Count >= 1);
    }

    [Fact]
    public void ParseDocument_WithMethodReferenceAttribute_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view

<Column>
    <Button OnClick=""@HandleClick"">Click</Button>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        
        var column = document.RootMarkup[0];
        var button = column.Children[0];
        
        Assert.Single(button.Attributes);
        var onClick = button.Attributes[0];
        Assert.Equal("OnClick", onClick.Name);
        Assert.Equal(AttributeValueType.MethodReference, onClick.Value.ValueType);
        Assert.Equal("HandleClick", onClick.Value.RawValue);
    }

    [Fact]
    public void ParseDocument_WithLambdaAttribute_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view

<Column>
    <Button OnClick=""@(() => count.Value++)"">+</Button>
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        
        var column = document.RootMarkup[0];
        var button = column.Children[0];
        
        var onClick = button.Attributes[0];
        Assert.Equal("OnClick", onClick.Name);
        Assert.Equal(AttributeValueType.Lambda, onClick.Value.ValueType);
        Assert.Contains("count.Value++", onClick.Value.RawValue);
    }

    [Fact]
    public void ParseDocument_WithSelfClosingTag_ParsesCorrectly()
    {
        // Arrange
        var content = @"@view

<Column>
    <Spacer />
</Column>";

        // Act
        var document = TuiSyntaxParser.ParseDocument("Test.tui", content);

        // Assert
        Assert.False(document.HasErrors);
        
        var column = document.RootMarkup[0];
        var spacer = column.Children[0];
        
        Assert.Equal("Spacer", spacer.TagName);
        Assert.True(spacer.IsSelfClosing);
    }
}

