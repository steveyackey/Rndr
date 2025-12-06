namespace Rndr.Razor.Parsing;

/// <summary>
/// The parsed representation of a complete .tui file.
/// </summary>
public sealed class TuiDocument
{
    /// <summary>
    /// Absolute path to the .tui file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// File name without extension (becomes class name).
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Required @view directive.
    /// </summary>
    public TuiViewDirective? ViewDirective { get; set; }

    /// <summary>
    /// @using namespace imports.
    /// </summary>
    public List<TuiUsingDirective> UsingDirectives { get; } = new();

    /// <summary>
    /// @inject service declarations.
    /// </summary>
    public List<TuiInjectDirective> InjectDirectives { get; } = new();

    /// <summary>
    /// Optional @code { ... } content.
    /// </summary>
    public TuiCodeBlock? CodeBlock { get; set; }

    /// <summary>
    /// Top-level markup nodes.
    /// </summary>
    public List<TuiMarkupNode> RootMarkup { get; } = new();

    /// <summary>
    /// Parsing errors/warnings.
    /// </summary>
    public List<TuiDiagnostic> Diagnostics { get; } = new();

    /// <summary>
    /// Creates a new document for the given file.
    /// </summary>
    public TuiDocument(string filePath)
    {
        FilePath = filePath;
        FileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Returns true if the document has any errors.
    /// </summary>
    public bool HasErrors => Diagnostics.Any(d => d.Severity == TuiDiagnosticSeverity.Error);

    /// <summary>
    /// Returns true if the document is valid (has @view directive and no errors).
    /// </summary>
    public bool IsValid => ViewDirective != null && !HasErrors;

    /// <summary>
    /// Adds an error diagnostic.
    /// </summary>
    public void AddError(string code, string message, SourceLocation location)
        => Diagnostics.Add(TuiDiagnostic.Error(code, message, location));

    /// <summary>
    /// Adds a warning diagnostic.
    /// </summary>
    public void AddWarning(string code, string message, SourceLocation location)
        => Diagnostics.Add(TuiDiagnostic.Warning(code, message, location));
}

