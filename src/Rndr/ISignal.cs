namespace Rndr;

/// <summary>
/// Non-generic interface for reactive state signals, enabling untyped access.
/// </summary>
public interface ISignal
{
    /// <summary>
    /// Gets or sets the value as an untyped object.
    /// </summary>
    object? UntypedValue { get; set; }
}

