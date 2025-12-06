namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents a position in a source file for error reporting.
/// </summary>
public readonly struct SourceLocation
{
    /// <summary>
    /// Path to the source file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 1-based line number.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// 1-based column number.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Length of the span in characters.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Creates a new source location.
    /// </summary>
    public SourceLocation(string filePath, int line, int column, int length = 0)
    {
        FilePath = filePath;
        Line = line;
        Column = column;
        Length = length;
    }

    /// <summary>
    /// Returns a string representation of this location.
    /// </summary>
    public override string ToString() => $"{FilePath}({Line},{Column})";

    /// <summary>
    /// Creates an empty/unknown location.
    /// </summary>
    public static SourceLocation None => new(string.Empty, 0, 0, 0);
}

