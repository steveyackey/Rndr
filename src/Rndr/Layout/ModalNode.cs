namespace Rndr.Layout;

/// <summary>
/// Represents a modal overlay dialog that appears on top of other content.
/// </summary>
public sealed class ModalNode : Node
{
    /// <summary>
    /// Gets the node kind.
    /// </summary>
    public override NodeKind Kind => NodeKind.Modal;

    /// <summary>
    /// Gets or sets the modal title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred width of the modal (in characters).
    /// If null, the modal will auto-size based on content.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the preferred height of the modal (in lines).
    /// If null, the modal will auto-size based on content.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the modal is closed.
    /// </summary>
    public Action? OnClose { get; set; }

    /// <summary>
    /// Gets or sets whether the modal can be closed by pressing Escape or clicking outside.
    /// Default is true.
    /// </summary>
    public bool AllowDismiss { get; set; } = true;
}
