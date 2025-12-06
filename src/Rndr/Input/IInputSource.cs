namespace Rndr.Input;

/// <summary>
/// Abstraction for keyboard input to enable testing with scripted input.
/// </summary>
public interface IInputSource
{
    /// <summary>
    /// Reads the next key event from the input source.
    /// </summary>
    /// <param name="intercept">If true, the key is not echoed to the console.</param>
    /// <returns>The key event that was read.</returns>
    KeyEvent ReadKey(bool intercept);

    /// <summary>
    /// Gets a value indicating whether a key is available to read.
    /// </summary>
    bool KeyAvailable { get; }
}

