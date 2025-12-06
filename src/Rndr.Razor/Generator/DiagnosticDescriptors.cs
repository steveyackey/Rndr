using Microsoft.CodeAnalysis;

namespace Rndr.Razor.Generator;

/// <summary>
/// Diagnostic descriptors for .tui source generator errors and warnings.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string Category = "TuiGenerator";

    /// <summary>
    /// TUI001: Missing @view directive.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingViewDirective = new(
        id: "TUI001",
        title: "Missing @view directive",
        messageFormat: ".tui file must start with @view directive",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI002: @view not first directive.
    /// </summary>
    public static readonly DiagnosticDescriptor ViewDirectiveNotFirst = new(
        id: "TUI002",
        title: "@view directive must be first",
        messageFormat: "@view directive must be first non-whitespace content",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI003: Unknown tag name.
    /// </summary>
    public static readonly DiagnosticDescriptor UnknownTag = new(
        id: "TUI003",
        title: "Unknown tag",
        messageFormat: "Unknown tag '{0}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI004: Unknown attribute name.
    /// </summary>
    public static readonly DiagnosticDescriptor UnknownAttribute = new(
        id: "TUI004",
        title: "Unknown attribute",
        messageFormat: "Unknown attribute '{0}' on <{1}>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI005: Invalid attribute value.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidAttributeValue = new(
        id: "TUI005",
        title: "Invalid attribute value",
        messageFormat: "Invalid value '{0}' for attribute '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI006: Unclosed tag.
    /// </summary>
    public static readonly DiagnosticDescriptor UnclosedTag = new(
        id: "TUI006",
        title: "Unclosed tag",
        messageFormat: "Unclosed tag <{0}> at line {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI007: Unexpected closing tag.
    /// </summary>
    public static readonly DiagnosticDescriptor UnexpectedClosingTag = new(
        id: "TUI007",
        title: "Unexpected closing tag",
        messageFormat: "Unexpected closing tag '{0}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI008: @code block syntax error.
    /// </summary>
    public static readonly DiagnosticDescriptor CodeBlockSyntaxError = new(
        id: "TUI008",
        title: "@code block syntax error",
        messageFormat: "Syntax error in @code block: {0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI009: Empty @code block warning.
    /// </summary>
    public static readonly DiagnosticDescriptor EmptyCodeBlock = new(
        id: "TUI009",
        title: "Empty @code block",
        messageFormat: "Empty @code block can be removed",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// TUI010: Generated file info.
    /// </summary>
    public static readonly DiagnosticDescriptor GeneratedFileInfo = new(
        id: "TUI010",
        title: "Generated component",
        messageFormat: "Generated {0} from {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}

