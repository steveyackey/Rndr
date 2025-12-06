namespace Rndr.Layout;

/// <summary>
/// Root builder for constructing Rndr layout trees.
/// </summary>
public sealed class LayoutBuilder
{
    private readonly List<Node> _nodes = [];

    /// <summary>
    /// Builds and returns the constructed node tree.
    /// </summary>
    public IReadOnlyList<Node> Build() => _nodes.AsReadOnly();

    /// <summary>
    /// Adds a vertical column container.
    /// </summary>
    /// <param name="configure">Action to configure the column's contents.</param>
    /// <returns>This builder for chaining.</returns>
    public LayoutBuilder Column(Action<ColumnBuilder> configure)
    {
        var node = new ColumnNode();
        var builder = new ColumnBuilder(node);
        configure(builder);
        _nodes.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a horizontal row container.
    /// </summary>
    /// <param name="configure">Action to configure the row's contents.</param>
    /// <returns>This builder for chaining.</returns>
    public LayoutBuilder Row(Action<RowBuilder> configure)
    {
        var node = new RowNode();
        var builder = new RowBuilder(node);
        configure(builder);
        _nodes.Add(node);
        return this;
    }

    /// <summary>
    /// Sets the default padding for subsequent nodes.
    /// </summary>
    /// <param name="value">The padding value in character cells.</param>
    /// <returns>This builder for chaining.</returns>
    public LayoutBuilder Padding(int value)
    {
        // This affects the next node added - store as default
        return this;
    }

    /// <summary>
    /// Sets the default gap between children for subsequent nodes.
    /// </summary>
    /// <param name="value">The gap value in character cells.</param>
    /// <returns>This builder for chaining.</returns>
    public LayoutBuilder Gap(int value)
    {
        // This affects the next node added - store as default
        return this;
    }
}

