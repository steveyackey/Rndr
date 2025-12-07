using Microsoft.Extensions.Logging.Abstractions;
using Rndr;

namespace Rndr.Testing;

/// <summary>
/// Fake implementation of IAsyncEventLoop for testing async component lifecycle.
/// </summary>
public sealed class FakeAsyncEventLoop : IAsyncEventLoop
{
    private readonly List<string> _executionLog = [];
    
    /// <summary>
    /// Gets or sets the timeout for async lifecycle methods.
    /// </summary>
    public TimeSpan AsyncTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the cancellation token for the currently rendering component.
    /// </summary>
    public CancellationToken ComponentCancellationToken { get; private set; } = CancellationToken.None;

    /// <summary>
    /// Gets the log of execution steps for test verification.
    /// </summary>
    public IReadOnlyList<string> ExecutionLog => _executionLog;

    /// <summary>
    /// Gets or sets whether RunAsync should throw an exception for testing error handling.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    /// <summary>
    /// Simulates running the async event loop.
    /// </summary>
    public Task RunAsync(TuiApp app, CancellationToken cancellationToken)
    {
        _executionLog.Add("RunAsync started");
        ComponentCancellationToken = cancellationToken;

        if (ExceptionToThrow != null)
        {
            _executionLog.Add($"RunAsync threw {ExceptionToThrow.GetType().Name}");
            return Task.FromException(ExceptionToThrow);
        }

        _executionLog.Add("RunAsync completed");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears the execution log.
    /// </summary>
    public void ClearLog()
    {
        _executionLog.Clear();
    }
}
