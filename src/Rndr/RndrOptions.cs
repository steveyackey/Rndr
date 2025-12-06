namespace Rndr;

/// <summary>
/// Runtime configuration options for the Rndr framework.
/// </summary>
public sealed class RndrOptions
{
    /// <summary>
    /// Gets or sets whether to use double buffering for rendering.
    /// Default: true
    /// </summary>
    public bool EnableDoubleBuffering { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use ANSI color codes.
    /// Default: true
    /// </summary>
    public bool UseAnsiColors { get; set; } = true;

    /// <summary>
    /// Gets or sets the delay between frames when idle (in milliseconds).
    /// Default: 16 (~60fps)
    /// </summary>
    public int IdleFrameDelay { get; set; } = 16;
}

