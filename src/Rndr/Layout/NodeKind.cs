namespace Rndr.Layout;

/// <summary>
/// Identifies the type of a layout node.
/// </summary>
public enum NodeKind
{
    /// <summary>
    /// Vertical stack container.
    /// </summary>
    Column,

    /// <summary>
    /// Horizontal arrangement container.
    /// </summary>
    Row,

    /// <summary>
    /// Bordered container with optional title.
    /// </summary>
    Panel,

    /// <summary>
    /// Text display element.
    /// </summary>
    Text,

    /// <summary>
    /// Clickable button element.
    /// </summary>
    Button,

    /// <summary>
    /// Text input field element.
    /// </summary>
    TextInput,

    /// <summary>
    /// Flexible space element.
    /// </summary>
    Spacer,

    /// <summary>
    /// Centers its content horizontally and vertically.
    /// </summary>
    Centered,

    /// <summary>
    /// Modal overlay dialog element.
    /// </summary>
    Modal
}

