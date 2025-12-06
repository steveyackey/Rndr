using System.Collections.Concurrent;

namespace Rndr;

/// <summary>
/// Default in-memory implementation of <see cref="IStateStore"/>.
/// </summary>
public sealed class InMemoryStateStore : IStateStore
{
    private readonly ConcurrentDictionary<(string Scope, string Key), ISignal> _store = new();

    /// <inheritdoc />
    public Signal<T> GetOrCreate<T>(string scopeKey, string key, Func<T> initialFactory, Action? onChanged = null)
    {
        var storeKey = (scopeKey, key);

        if (_store.TryGetValue(storeKey, out var existing))
        {
            return (Signal<T>)existing;
        }

        var signal = new Signal<T>(initialFactory(), onChanged);
        var added = _store.TryAdd(storeKey, signal);

        if (!added)
        {
            // Another thread beat us to it, return the existing one
            return (Signal<T>)_store[storeKey];
        }

        return signal;
    }

    /// <summary>
    /// Gets all state keys for a specific scope.
    /// </summary>
    /// <param name="scopeKey">The scope to query.</param>
    /// <returns>All keys stored in the specified scope.</returns>
    public IEnumerable<string> GetKeysForScope(string scopeKey)
    {
        return _store.Keys
            .Where(k => k.Scope == scopeKey)
            .Select(k => k.Key);
    }

    /// <summary>
    /// Clears all state for a specific scope.
    /// </summary>
    /// <param name="scopeKey">The scope to clear.</param>
    public void ClearScope(string scopeKey)
    {
        var keysToRemove = _store.Keys.Where(k => k.Scope == scopeKey).ToList();
        foreach (var key in keysToRemove)
        {
            _store.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Clears all stored state.
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }
}

