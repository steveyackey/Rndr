namespace Rndr.Samples.MyFocusTui.Models;

/// <summary>
/// Global application state for the focus timer.
/// </summary>
public sealed class FocusState
{
    /// <summary>
    /// Gets or sets the current todo/focus item.
    /// </summary>
    public string? CurrentTodo { get; set; }

    /// <summary>
    /// Gets the log of actions.
    /// </summary>
    public List<ActionEntry> Log { get; } = [];

    /// <summary>
    /// Gets whether there is an active focus item.
    /// </summary>
    public bool HasActiveFocus => !string.IsNullOrWhiteSpace(CurrentTodo);
}

