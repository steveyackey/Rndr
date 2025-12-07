namespace MyFocusTui.Models;

/// <summary>
/// Represents an action logged in the focus timer.
/// </summary>
public sealed record ActionEntry
{
    /// <summary>
    /// Gets the timestamp when the action occurred.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the message describing the action.
    /// </summary>
    public required string Message { get; init; }
}

