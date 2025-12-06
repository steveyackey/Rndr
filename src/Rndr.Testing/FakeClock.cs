namespace Rndr.Testing;

/// <summary>
/// Fake clock for testing with controllable time.
/// </summary>
public sealed class FakeClock : IClock
{
    /// <summary>
    /// Gets or sets the current time.
    /// </summary>
    public DateTime Now { get; set; } = DateTime.Now;

    /// <summary>
    /// Advances the clock by the specified amount.
    /// </summary>
    /// <param name="time">The time span to advance.</param>
    public void Advance(TimeSpan time)
    {
        Now = Now.Add(time);
    }

    /// <summary>
    /// Sets the clock to a specific time.
    /// </summary>
    /// <param name="time">The time to set.</param>
    public void SetTime(DateTime time)
    {
        Now = time;
    }
}

