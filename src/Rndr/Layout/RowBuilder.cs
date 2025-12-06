namespace Rndr.Layout;

/// <summary>
/// Builder for configuring row (horizontal arrangement) contents.
/// </summary>
public sealed class RowBuilder
{
    private readonly RowNode _node;

    internal RowBuilder(RowNode node)
    {
        _node = node;
    }

    /// <summary>
    /// Adds text content to the row.
    /// </summary>
    /// <param name="content">The text to display.</param>
    /// <param name="configure">Optional action to configure the text style.</param>
    /// <returns>This builder for chaining.</returns>
    public RowBuilder Text(string content, Action<NodeStyle>? configure = null)
    {
        var node = new TextNode { Content = content };
        configure?.Invoke(node.Style);
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a clickable button.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <param name="onClick">Action to invoke when clicked.</param>
    /// <returns>This builder for chaining.</returns>
    public RowBuilder Button(string label, Action? onClick)
    {
        var node = new ButtonNode { Label = label, OnClick = onClick };
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds flexible space.
    /// </summary>
    /// <param name="weight">The flex weight (default: 1).</param>
    /// <returns>This builder for chaining.</returns>
    public RowBuilder Spacer(int weight = 1)
    {
        var node = new SpacerNode { Weight = weight };
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a nested column.
    /// </summary>
    /// <param name="configure">Action to configure the column's contents.</param>
    /// <returns>This builder for chaining.</returns>
    public RowBuilder Column(Action<ColumnBuilder> configure)
    {
        var columnNode = new ColumnNode();
        var builder = new ColumnBuilder(columnNode);
        configure(builder);
        _node.Children.Add(columnNode);
        return this;
    }

    /// <summary>
    /// Sets the gap between children.
    /// </summary>
    /// <param name="value">The gap value in character cells.</param>
    /// <returns>This builder for chaining.</returns>
    public RowBuilder Gap(int value)
    {
        _node.Style.Gap = value;
        return this;
    }

    /// <summary>
    /// Sets text alignment to center.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public RowBuilder AlignCenter()
    {
        _node.Style.Align = TextAlign.Center;
        return this;
    }
}

