namespace Rndr.Layout;

/// <summary>
/// A layout node that represents flexible space between elements.
/// </summary>
public sealed class SpacerNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Spacer;

    /// <summary>
    /// Gets or sets the weight for flex distribution (default: 1).
    /// </summary>
    public int Weight { get; set; } = 1;
}

