namespace Rndr.Layout;

/// <summary>
/// A layout node that stacks its children vertically.
/// </summary>
public sealed class ColumnNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.Column;
}

