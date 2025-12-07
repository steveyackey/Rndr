namespace Rndr.Layout;

/// <summary>
/// Builder for configuring column (vertical stack) contents.
/// </summary>
public sealed class ColumnBuilder
{
    private readonly ColumnNode _node;

    internal ColumnBuilder(ColumnNode node)
    {
        _node = node;
    }

    /// <summary>
    /// Adds text content to the column.
    /// </summary>
    /// <param name="content">The text to display.</param>
    /// <param name="configure">Optional action to configure the text style.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Text(string content, Action<NodeStyle>? configure = null)
    {
        var node = new TextNode { Content = content };
        configure?.Invoke(node.Style);
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a bordered panel with a title.
    /// </summary>
    /// <param name="title">The panel title.</param>
    /// <param name="body">Action to configure the panel's contents.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Panel(string title, Action<LayoutBuilder> body)
    {
        var node = new PanelNode { Title = title };
        var innerBuilder = new LayoutBuilder();
        body(innerBuilder);
        foreach (var child in innerBuilder.Build())
        {
            node.Children.Add(child);
        }
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a clickable button.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <param name="onClick">Action to invoke when clicked.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Button(string label, Action? onClick)
    {
        var node = new ButtonNode { Label = label, OnClick = onClick };
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a text input field.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="onChanged">Callback when value changes.</param>
    /// <param name="placeholder">Optional placeholder text.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder TextInput(string value, Action<string>? onChanged, string? placeholder = null)
    {
        var node = new TextInputNode
        {
            Value = value,
            OnChanged = onChanged,
            Placeholder = placeholder
        };
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds flexible space.
    /// </summary>
    /// <param name="weight">The flex weight (default: 1).</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Spacer(int weight = 1)
    {
        var node = new SpacerNode { Weight = weight };
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds centered content.
    /// </summary>
    /// <param name="child">Action to configure the centered content.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Centered(Action<LayoutBuilder> child)
    {
        var node = new CenteredNode();
        var innerBuilder = new LayoutBuilder();
        child(innerBuilder);
        foreach (var c in innerBuilder.Build())
        {
            node.Children.Add(c);
        }
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a nested row.
    /// </summary>
    /// <param name="configure">Action to configure the row's contents.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Row(Action<RowBuilder> configure)
    {
        var node = new RowNode();
        var builder = new RowBuilder(node);
        configure(builder);
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Adds a modal overlay dialog.
    /// </summary>
    /// <param name="title">The modal title.</param>
    /// <param name="configure">Action to configure the modal's contents.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Modal(string title, Action<LayoutBuilder> configure)
    {
        var node = new ModalNode { Title = title };
        var innerBuilder = new LayoutBuilder();
        configure(innerBuilder);
        foreach (var child in innerBuilder.Build())
        {
            node.Children.Add(child);
        }
        _node.Children.Add(node);
        return this;
    }

    /// <summary>
    /// Sets the gap between children.
    /// </summary>
    /// <param name="value">The gap value in character cells.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Gap(int value)
    {
        _node.Style.Gap = value;
        return this;
    }

    /// <summary>
    /// Sets the internal padding.
    /// </summary>
    /// <param name="value">The padding value in character cells.</param>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder Padding(int value)
    {
        _node.Style.Padding = value;
        return this;
    }

    /// <summary>
    /// Sets text alignment to center.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder AlignCenter()
    {
        _node.Style.Align = TextAlign.Center;
        return this;
    }

    /// <summary>
    /// Sets text alignment to right.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public ColumnBuilder AlignRight()
    {
        _node.Style.Align = TextAlign.Right;
        return this;
    }
}

