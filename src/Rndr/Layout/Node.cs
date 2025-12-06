namespace Rndr.Layout;

/// <summary>
/// Base class for all layout nodes in the Rndr UI tree.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// Gets the kind of node.
    /// </summary>
    public abstract NodeKind Kind { get; }

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    public List<Node> Children { get; } = [];

    /// <summary>
    /// Gets or sets the visual style for this node.
    /// </summary>
    public NodeStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets an optional identifier for this node.
    /// </summary>
    public string? Id { get; set; }
}

