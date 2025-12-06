namespace Rndr.Input;

/// <summary>
/// Default input source implementation using Console.ReadKey.
/// </summary>
public sealed class SystemInputSource : IInputSource
{
    /// <inheritdoc />
    public KeyEvent ReadKey(bool intercept)
    {
        var info = Console.ReadKey(intercept);
        return new KeyEvent(info.Key, info.KeyChar, info.Modifiers);
    }

    /// <inheritdoc />
    public bool KeyAvailable => Console.KeyAvailable;
}

