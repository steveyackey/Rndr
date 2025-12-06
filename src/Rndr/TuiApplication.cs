namespace Rndr;

/// <summary>
/// Static factory for creating Rndr TUI applications.
/// </summary>
public static class TuiApplication
{
    /// <summary>
    /// Creates a new application builder.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A new TuiAppBuilder instance.</returns>
    public static TuiAppBuilder CreateBuilder(string[] args)
    {
        return new TuiAppBuilder(args);
    }
}

