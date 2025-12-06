namespace Rndr.Layout;

/// <summary>
/// A layout node that centers its content horizontally and vertically.
/// </summary>
public sealed class CenteredNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Centered;
}

