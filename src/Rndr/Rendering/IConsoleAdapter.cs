namespace Rndr.Rendering;

/// <summary>
/// Abstraction for console output operations to enable testing without a real terminal.
/// </summary>
public interface IConsoleAdapter
{
    /// <summary>
    /// Gets the width of the console window in characters.
    /// </summary>
    int WindowWidth { get; }

    /// <summary>
    /// Gets the height of the console window in characters.
    /// </summary>
    int WindowHeight { get; }

    /// <summary>
    /// Clears the console screen.
    /// </summary>
    void Clear();

    /// <summary>
    /// Writes text at the specified position with optional colors.
    /// </summary>
    /// <param name="left">The column position (0-based).</param>
    /// <param name="top">The row position (0-based).</param>
    /// <param name="text">The text to write.</param>
    /// <param name="foreground">Optional foreground color.</param>
    /// <param name="background">Optional background color.</param>
    void WriteAt(int left, int top, string text, ConsoleColor? foreground = null, ConsoleColor? background = null);

    /// <summary>
    /// Hides the cursor.
    /// </summary>
    void HideCursor();

    /// <summary>
    /// Shows the cursor.
    /// </summary>
    void ShowCursor();

    /// <summary>
    /// Flushes any buffered output to the console.
    /// </summary>
    void Flush();
}

