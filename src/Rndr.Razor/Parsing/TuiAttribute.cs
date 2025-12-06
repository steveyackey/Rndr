namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents an attribute on a markup element.
/// </summary>
public sealed class TuiAttribute
{
    /// <summary>
    /// Attribute name (e.g., "Padding", "OnClick").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The attribute value.
    /// </summary>
    public TuiAttributeValue Value { get; }

    /// <summary>
    /// Position in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// Creates a new attribute.
    /// </summary>
    public TuiAttribute(string name, TuiAttributeValue value, SourceLocation location)
    {
        Name = name;
        Value = value;
        Location = location;
    }
}

