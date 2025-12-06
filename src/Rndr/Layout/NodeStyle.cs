namespace Rndr.Layout;

/// <summary>
/// Visual styling properties for layout nodes.
/// </summary>
public sealed class NodeStyle
{
    /// <summary>
    /// Gets or sets the internal padding (in character cells).
    /// </summary>
    public int? Padding { get; set; }

    /// <summary>
    /// Gets or sets the gap between children (in character cells).
    /// </summary>
    public int? Gap { get; set; }

    /// <summary>
    /// Gets or sets the text alignment.
    /// </summary>
    public TextAlign? Align { get; set; }

    /// <summary>
    /// Gets or sets whether text should be bold.
    /// </summary>
    public bool Bold { get; set; }

    /// <summary>
    /// Gets or sets whether to use the accent color.
    /// </summary>
    public bool Accent { get; set; }

    /// <summary>
    /// Gets or sets whether text should be faint/dimmed.
    /// </summary>
    public bool Faint { get; set; }

    /// <summary>
    /// Creates a clone of this style.
    /// </summary>
    public NodeStyle Clone() => new()
    {
        Padding = Padding,
        Gap = Gap,
        Align = Align,
        Bold = Bold,
        Accent = Accent,
        Faint = Faint
    };
}

