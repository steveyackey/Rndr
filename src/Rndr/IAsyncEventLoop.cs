namespace Rndr;

/// <summary>
/// Interface for an async-aware event loop that supports async component lifecycle.
/// Extends IEventLoop with async-specific capabilities like timeout and cancellation.
/// </summary>
public interface IAsyncEventLoop : IEventLoop
{
    /// <summary>
    /// Gets or sets the timeout for async lifecycle methods (OnInitializedAsync, OnParametersSetAsync, etc.).
    /// Default: 30 seconds
    /// </summary>
    TimeSpan AsyncTimeout { get; set; }

    /// <summary>
    /// Gets the cancellation token for the currently rendering component.
    /// Canceled when the component is disposed or navigation occurs.
    /// </summary>
    CancellationToken ComponentCancellationToken { get; }
}
