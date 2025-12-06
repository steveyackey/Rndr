namespace Rndr.Layout;

/// <summary>
/// A layout node that displays text content.
/// </summary>
public sealed class TextNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Text;

    /// <summary>
    /// Gets or sets the text content to display.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

