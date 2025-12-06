namespace Rndr.Layout;

/// <summary>
/// A layout node that represents a text input field.
/// </summary>
public sealed class TextInputNode : Node
{
    /// <inheritdoc />
    public override NodeKind Kind => NodeKind.TextInput;

    /// <summary>
    /// Gets or sets the current value of the input.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback invoked when the value changes.
    /// </summary>
    public Action<string>? OnChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text shown when the input is empty.
    /// </summary>
    public string? Placeholder { get; set; }
}

