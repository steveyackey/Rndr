using Rndr.Input;

namespace Rndr.Testing;

/// <summary>
/// Fake input source for testing with scripted key sequences.
/// </summary>
public sealed class FakeInputSource : IInputSource
{
    private readonly Queue<KeyEvent> _keyQueue = new();

    /// <summary>
    /// Enqueues a key event to be returned by the next ReadKey call.
    /// </summary>
    /// <param name="key">The console key.</param>
    /// <param name="keyChar">The character representation.</param>
    /// <param name="modifiers">Any modifier keys.</param>
    public void EnqueueKey(ConsoleKey key, char keyChar = '\0', ConsoleModifiers modifiers = 0)
    {
        _keyQueue.Enqueue(new KeyEvent(key, keyChar, modifiers));
    }

    /// <summary>
    /// Enqueues multiple key events.
    /// </summary>
    /// <param name="keys">The keys to enqueue.</param>
    public void EnqueueKeys(params KeyEvent[] keys)
    {
        foreach (var key in keys)
        {
            _keyQueue.Enqueue(key);
        }
    }

    /// <inheritdoc />
    public KeyEvent ReadKey(bool intercept)
    {
        if (_keyQueue.Count == 0)
        {
            throw new InvalidOperationException("No keys available in fake input source");
        }
        return _keyQueue.Dequeue();
    }

    /// <inheritdoc />
    public bool KeyAvailable => _keyQueue.Count > 0;

    /// <summary>
    /// Clears all queued keys.
    /// </summary>
    public void Clear() => _keyQueue.Clear();

    /// <summary>
    /// Gets the number of keys remaining in the queue.
    /// </summary>
    public int PendingKeyCount => _keyQueue.Count;
}

