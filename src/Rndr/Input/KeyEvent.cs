namespace Rndr.Input;

/// <summary>
/// Represents a keyboard input event.
/// </summary>
/// <param name="Key">The console key that was pressed.</param>
/// <param name="KeyChar">The character representation of the key.</param>
/// <param name="Modifiers">Any modifier keys (Shift, Alt, Control) that were held.</param>
public sealed record KeyEvent(
    ConsoleKey Key,
    char KeyChar,
    ConsoleModifiers Modifiers);

