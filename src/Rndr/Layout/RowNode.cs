namespace Rndr.Layout;

/// <summary>
/// A layout node that arranges its children horizontally.
/// </summary>
public sealed class RowNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Row;
}

