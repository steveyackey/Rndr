namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents a @using directive for namespace imports.
/// </summary>
public sealed class TuiUsingDirective
{
    /// <summary>
    /// The namespace to import.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Position in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// Creates a new @using directive.
    /// </summary>
    public TuiUsingDirective(string ns, SourceLocation location)
    {
        Namespace = ns;
        Location = location;
    }
}

