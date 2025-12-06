namespace Rndr;

/// <summary>
/// A reactive state container that triggers callbacks when the value changes.
/// </summary>
/// <typeparam name="T">The type of the state value.</typeparam>
public sealed class Signal<T> : ISignal
{
    private T _value;
    private readonly Action? _onChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Signal{T}"/> class.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="onChanged">Optional callback invoked when the value changes.</param>
    public Signal(T initialValue, Action? onChanged = null)
    {
        _value = initialValue;
        _onChanged = onChanged;
    }

    /// <summary>
    /// Gets or sets the current value. Setting to a different value triggers the change callback.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                _onChanged?.Invoke();
            }
        }
    }

    /// <inheritdoc />
    object? ISignal.UntypedValue
    {
        get => _value;
        set => Value = (T)value!;
    }

    /// <summary>
    /// Implicitly converts the signal to its value.
    /// </summary>
    public static implicit operator T(Signal<T> signal) => signal.Value;
}

