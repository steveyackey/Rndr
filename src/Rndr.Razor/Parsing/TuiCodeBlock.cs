namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents the @code { ... } block containing C# class members.
/// </summary>
public sealed class TuiCodeBlock
{
    /// <summary>
    /// Raw C# code inside the braces.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Position of the @code keyword in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// Position of the opening brace.
    /// </summary>
    public SourceLocation OpenBraceLocation { get; }

    /// <summary>
    /// Position of the closing brace.
    /// </summary>
    public SourceLocation CloseBraceLocation { get; }

    /// <summary>
    /// Creates a new @code block.
    /// </summary>
    public TuiCodeBlock(
        string content,
        SourceLocation location,
        SourceLocation openBraceLocation,
        SourceLocation closeBraceLocation)
    {
        Content = content;
        Location = location;
        OpenBraceLocation = openBraceLocation;
        CloseBraceLocation = closeBraceLocation;
    }

    /// <summary>
    /// Returns true if the code block is empty or contains only whitespace.
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Content);
}

