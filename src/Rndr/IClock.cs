namespace Rndr;

/// <summary>
/// Abstraction for time operations to enable testing with controlled time.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTime Now { get; }
}

