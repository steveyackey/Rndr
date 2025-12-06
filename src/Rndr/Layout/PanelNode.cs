namespace Rndr.Layout;

/// <summary>
/// A layout node that renders a bordered container with an optional title.
/// </summary>
public sealed class PanelNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Panel;

    /// <summary>
    /// Gets or sets the panel title displayed in the border.
    /// </summary>
    public string? Title { get; set; }
}

