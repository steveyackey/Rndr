namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents the required @view directive that identifies a .tui file as a TUI component.
/// </summary>
public sealed class TuiViewDirective
{
    /// <summary>
    /// Position in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// Creates a new @view directive.
    /// </summary>
    public TuiViewDirective(SourceLocation location)
    {
        Location = location;
    }
}

