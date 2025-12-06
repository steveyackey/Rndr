namespace Rndr.Razor;

/// <summary>
/// Razor configuration for .tui single-file components.
/// Maps .tui markup to TuiComponentBase-derived classes.
/// </summary>
public static class TuiRazorConfiguration
{
    /// <summary>
    /// The file extension for TUI components.
    /// </summary>
    public const string TuiFileExtension = ".tui";

    /// <summary>
    /// The namespace directive for .tui files.
    /// </summary>
    public const string ViewDirective = "@view";

    /// <summary>
    /// Maps .tui markup tags to layout builder method calls.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> TagMappings = new Dictionary<string, string>
    {
        ["Column"] = "layout.Column",
        ["Row"] = "layout.Row",
        ["Panel"] = "col.Panel",
        ["Text"] = "col.Text",
        ["Button"] = "col.Button",
        ["Spacer"] = "col.Spacer",
        ["Centered"] = "col.Centered",
        ["TextInput"] = "col.TextInput"
    };

    /// <summary>
    /// Maps .tui markup attributes to builder method parameters.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> AttributeMappings = new Dictionary<string, string>
    {
        ["Padding"] = "Padding",
        ["Gap"] = "Gap",
        ["Bold"] = "style.Bold",
        ["Accent"] = "style.Accent",
        ["Faint"] = "style.Faint",
        ["Align"] = "style.Align",
        ["Width"] = "Width",
        ["Primary"] = "IsPrimary",
        ["OnClick"] = "OnClick",
        ["Title"] = "Title",
        ["Placeholder"] = "Placeholder"
    };
}

