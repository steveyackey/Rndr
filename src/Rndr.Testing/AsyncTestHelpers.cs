namespace Rndr.Testing;

/// <summary>
/// Utility methods for testing async component operations.
/// </summary>
public static class AsyncTestHelpers
{
    /// <summary>
    /// Waits for a task to complete with a timeout, throwing if the task doesn't complete in time.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="taskDescription">Description of the task for error messages.</param>
    /// <exception cref="TimeoutException">Thrown if the task doesn't complete within the timeout.</exception>
    public static async Task WaitForAsync(Task task, TimeSpan timeout, string taskDescription = "Task")
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask != task)
        {
            throw new TimeoutException($"{taskDescription} did not complete within {timeout.TotalSeconds}s");
        }

        // Propagate exceptions from the task
        await task;
    }

    /// <summary>
    /// Waits for a task to complete with a timeout, returning a result or throwing if the task doesn't complete in time.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="taskDescription">Description of the task for error messages.</param>
    /// <returns>The task result.</returns>
    /// <exception cref="TimeoutException">Thrown if the task doesn't complete within the timeout.</exception>
    public static async Task<T> WaitForAsync<T>(Task<T> task, TimeSpan timeout, string taskDescription = "Task")
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask != task)
        {
            throw new TimeoutException($"{taskDescription} did not complete within {timeout.TotalSeconds}s");
        }

        return await task;
    }

    /// <summary>
    /// Asserts that a task completes within the specified timeout.
    /// </summary>
    /// <param name="task">The task to wait for.</param>
    /// <param name="timeoutMs">The timeout in milliseconds.</param>
    /// <param name="message">Optional message for the assertion.</param>
    public static async Task AssertCompletesWithinAsync(Task task, int timeoutMs, string? message = null)
    {
        var timeout = TimeSpan.FromMilliseconds(timeoutMs);
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        
        if (completedTask != task)
        {
            var errorMessage = message ?? $"Task did not complete within {timeoutMs}ms";
            throw new AssertionException(errorMessage);
        }

        // Propagate exceptions from the task
        await task;
    }

    /// <summary>
    /// Asserts that a task throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="task">The task that should throw.</param>
    /// <returns>The thrown exception.</returns>
    public static async Task<TException> AssertThrowsAsync<TException>(Task task)
        where TException : Exception
    {
        try
        {
            await task;
            throw new AssertionException($"Expected {typeof(TException).Name} but no exception was thrown");
        }
        catch (TException ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            throw new AssertionException($"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a cancellation token that is canceled after the specified delay.
    /// </summary>
    /// <param name="delayMs">Delay in milliseconds before cancellation.</param>
    /// <returns>A cancellation token.</returns>
    public static CancellationToken CreateTimeoutToken(int delayMs)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(delayMs);
        return cts.Token;
    }
}

/// <summary>
/// Exception thrown by async test helper assertions.
/// This is a simple test exception for Rndr's internal testing utilities.
/// When using with xUnit, NUnit, or MSTest, consider using their native assertion methods instead.
/// </summary>
public sealed class AssertionException : Exception
{
    public AssertionException(string message) : base(message)
    {
    }
}
