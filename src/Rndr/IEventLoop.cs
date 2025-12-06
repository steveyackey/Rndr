namespace Rndr;

/// <summary>
/// Interface for the main event loop that drives the TUI application.
/// </summary>
public interface IEventLoop
{
    /// <summary>
    /// Runs the event loop until cancellation is requested or the app quits.
    /// </summary>
    /// <param name="app">The application instance.</param>
    /// <param name="cancellationToken">Token to cancel the event loop.</param>
    Task RunAsync(TuiApp app, CancellationToken cancellationToken);
}

