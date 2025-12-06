namespace Rndr;

/// <summary>
/// Application-wide visual theme configuration.
/// </summary>
public sealed class RndrTheme
{
    /// <summary>
    /// Gets or sets the accent color used for primary buttons and highlighted text.
    /// </summary>
    public ConsoleColor AccentColor { get; set; } = ConsoleColor.Cyan;

    /// <summary>
    /// Gets or sets the default text color.
    /// </summary>
    public ConsoleColor TextColor { get; set; } = ConsoleColor.Gray;

    /// <summary>
    /// Gets or sets the color for muted/dimmed text.
    /// </summary>
    public ConsoleColor MutedTextColor { get; set; } = ConsoleColor.DarkGray;

    /// <summary>
    /// Gets or sets the color for error messages.
    /// </summary>
    public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

    /// <summary>
    /// Gets the panel-specific theme settings.
    /// </summary>
    public PanelTheme Panel { get; } = new();

    /// <summary>
    /// Gets or sets the base spacing unit (in character cells).
    /// </summary>
    public int SpacingUnit { get; set; } = 1;
}

/// <summary>
/// Panel-specific theme settings.
/// </summary>
public sealed class PanelTheme
{
    /// <summary>
    /// Gets or sets the border style for panels.
    /// </summary>
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Rounded;
}

/// <summary>
/// Border style options for panels.
/// </summary>
public enum BorderStyle
{
    /// <summary>
    /// Square corners: ┌┐└┘
    /// </summary>
    Square,

    /// <summary>
    /// Rounded corners: ╭╮╰╯
    /// </summary>
    Rounded
}

