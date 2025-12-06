namespace Rndr;

/// <summary>
/// Storage and retrieval of reactive state signals with scoping support.
/// </summary>
public interface IStateStore
{
    /// <summary>
    /// Gets an existing signal or creates a new one with the initial value.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="scopeKey">The scope identifier (e.g., route path or "global").</param>
    /// <param name="key">The state key within the scope.</param>
    /// <param name="initialFactory">Factory function to create the initial value if the signal doesn't exist.</param>
    /// <param name="onChanged">Optional callback invoked when the signal value changes.</param>
    /// <returns>The signal for the specified scope and key.</returns>
    Signal<T> GetOrCreate<T>(string scopeKey, string key, Func<T> initialFactory, Action? onChanged = null);
}

