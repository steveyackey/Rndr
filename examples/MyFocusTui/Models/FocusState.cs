namespace MyFocusTui.Models;

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
    /// Gets or sets the pending focus text being entered in the input field.
    /// </summary>
    public string PendingFocusText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the edit modal is open.
    /// </summary>
    public bool IsEditing { get; set; }

    /// <summary>
    /// Gets or sets the text being edited in the edit modal.
    /// </summary>
    public string EditingText { get; set; } = string.Empty;

    /// <summary>
    /// Gets the log of actions.
    /// </summary>
    public List<ActionEntry> Log { get; } = [];

    /// <summary>
    /// Gets whether there is an active focus item.
    /// </summary>
    public bool HasActiveFocus => !string.IsNullOrWhiteSpace(CurrentTodo);
}

