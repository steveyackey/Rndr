namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents a node in the markup tree (tag element, text content, or control flow).
/// </summary>
public sealed class TuiMarkupNode
{
    /// <summary>
    /// The type of this node.
    /// </summary>
    public MarkupNodeType NodeType { get; }

    /// <summary>
    /// Tag name for Element nodes (e.g., "Column", "Button").
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// Attributes on element nodes.
    /// </summary>
    public List<TuiAttribute> Attributes { get; } = new();

    /// <summary>
    /// Child nodes.
    /// </summary>
    public List<TuiMarkupNode> Children { get; } = new();

    /// <summary>
    /// Text content for Text/Expression nodes.
    /// </summary>
    public string? TextContent { get; set; }

    /// <summary>
    /// Position in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// True for self-closing tags (&lt;Tag /&gt;).
    /// </summary>
    public bool IsSelfClosing { get; set; }

    /// <summary>
    /// For control flow nodes (@if, @foreach, @switch): the condition/expression.
    /// </summary>
    public string? ControlFlowExpression { get; set; }

    /// <summary>
    /// For @if/@switch: the else/else-if/default blocks.
    /// </summary>
    public List<TuiMarkupNode>? AlternateChildren { get; set; }

    /// <summary>
    /// For @switch: the case value (e.g., "case \"a\":").
    /// </summary>
    public string? CaseValue { get; set; }

    /// <summary>
    /// Creates a new element node.
    /// </summary>
    public static TuiMarkupNode Element(string tagName, SourceLocation location)
        => new(MarkupNodeType.Element, location) { TagName = tagName };

    /// <summary>
    /// Creates a new text node.
    /// </summary>
    public static TuiMarkupNode Text(string content, SourceLocation location)
        => new(MarkupNodeType.Text, location) { TextContent = content };

    /// <summary>
    /// Creates a new expression node (@variable).
    /// </summary>
    public static TuiMarkupNode Expression(string expression, SourceLocation location)
        => new(MarkupNodeType.Expression, location) { TextContent = expression };

    /// <summary>
    /// Creates a new control flow node (@if, @foreach, @switch).
    /// </summary>
    public static TuiMarkupNode ControlFlow(string controlFlowType, string expression, SourceLocation location)
        => new(MarkupNodeType.ControlFlow, location) 
        { 
            TagName = controlFlowType, // "if", "foreach", "switch", "else", "else if", "case", "default"
            ControlFlowExpression = expression 
        };

    private TuiMarkupNode(MarkupNodeType nodeType, SourceLocation location)
    {
        NodeType = nodeType;
        Location = location;
    }
}

/// <summary>
/// Types of markup nodes.
/// </summary>
public enum MarkupNodeType
{
    /// <summary>A tag like &lt;Column&gt; or &lt;Button&gt;.</summary>
    Element,
    /// <summary>Raw text content.</summary>
    Text,
    /// <summary>Razor expression like @count.Value.</summary>
    Expression,
    /// <summary>@if, @foreach, @switch blocks.</summary>
    ControlFlow
}

