namespace Rndr;

/// <summary>
/// Default clock implementation using system time.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime Now => DateTime.Now;
}

