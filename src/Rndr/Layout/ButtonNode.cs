namespace Rndr.Layout;

/// <summary>
/// A layout node that represents a clickable button.
/// </summary>
public sealed class ButtonNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Button;

    /// <summary>
    /// Gets or sets the button label text.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action to invoke when the button is clicked.
    /// </summary>
    public Action? OnClick { get; set; }

    /// <summary>
    /// Gets or sets whether this is a primary (accent-styled) button.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Gets or sets a fixed width for the button (in characters).
    /// </summary>
    public int? Width { get; set; }
}

