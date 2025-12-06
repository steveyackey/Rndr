namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents a parsing or validation error/warning.
/// </summary>
public sealed class TuiDiagnostic
{
    /// <summary>
    /// Diagnostic code (e.g., "TUI001").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Severity of the diagnostic.
    /// </summary>
    public TuiDiagnosticSeverity Severity { get; }

    /// <summary>
    /// Human-readable message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Position in source file.
    /// </summary>
    public SourceLocation Location { get; }

    /// <summary>
    /// Creates a new diagnostic.
    /// </summary>
    public TuiDiagnostic(string code, TuiDiagnosticSeverity severity, string message, SourceLocation location)
    {
        Code = code;
        Severity = severity;
        Message = message;
        Location = location;
    }

    /// <summary>
    /// Creates an error diagnostic.
    /// </summary>
    public static TuiDiagnostic Error(string code, string message, SourceLocation location)
        => new(code, TuiDiagnosticSeverity.Error, message, location);

    /// <summary>
    /// Creates a warning diagnostic.
    /// </summary>
    public static TuiDiagnostic Warning(string code, string message, SourceLocation location)
        => new(code, TuiDiagnosticSeverity.Warning, message, location);

    /// <summary>
    /// Creates an info diagnostic.
    /// </summary>
    public static TuiDiagnostic Info(string code, string message, SourceLocation location)
        => new(code, TuiDiagnosticSeverity.Info, message, location);

    /// <inheritdoc />
    public override string ToString() => $"{Location}: {Severity.ToString().ToLowerInvariant()} {Code}: {Message}";
}

/// <summary>
/// Severity levels for diagnostics.
/// </summary>
public enum TuiDiagnosticSeverity
{
    /// <summary>Information only.</summary>
    Info,
    /// <summary>Warning - compilation continues.</summary>
    Warning,
    /// <summary>Error - compilation fails.</summary>
    Error
}

