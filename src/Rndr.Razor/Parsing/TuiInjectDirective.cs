namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents an @inject directive for dependency injection.
/// </summary>
public sealed class TuiInjectDirective
{
    /// <summary>
    /// The service type (e.g., "ILogger&lt;Counter&gt;").
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The property name to generate.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Position in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// Creates a new @inject directive.
    /// </summary>
    public TuiInjectDirective(string typeName, string propertyName, SourceLocation location)
    {
        TypeName = typeName;
        PropertyName = propertyName;
        Location = location;
    }
}

