using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Rndr.Diagnostics;

/// <summary>
/// Provides tracing and metrics instrumentation for the Rndr framework.
/// </summary>
public static class RndrDiagnostics
{
    /// <summary>
    /// The name used for all Rndr diagnostics.
    /// </summary>
    public const string SourceName = "Rndr.Core";

    /// <summary>
    /// Activity source for distributed tracing.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, "1.0.0");

    /// <summary>
    /// Meter for metrics collection.
    /// </summary>
    public static readonly Meter Meter = new(SourceName, "1.0.0");

    // Pre-defined counters and histograms
    private static readonly Counter<long> FramesRendered = Meter.CreateCounter<long>(
        "rndr.frames_rendered",
        "frames",
        "Total number of frames rendered");

    private static readonly Histogram<double> RenderDuration = Meter.CreateHistogram<double>(
        "rndr.render_duration_ms",
        "ms",
        "Time taken to render a frame");

    private static readonly Counter<long> NavigationCount = Meter.CreateCounter<long>(
        "rndr.navigations",
        "navigations",
        "Total number of navigation events");

    private static readonly Counter<long> KeyEvents = Meter.CreateCounter<long>(
        "rndr.key_events",
        "events",
        "Total number of key events processed");

    /// <summary>
    /// Records that a frame was rendered.
    /// </summary>
    /// <param name="durationMs">The time taken to render in milliseconds.</param>
    public static void RecordFrameRendered(double durationMs)
    {
        FramesRendered.Add(1);
        RenderDuration.Record(durationMs);
    }

    /// <summary>
    /// Records a navigation event.
    /// </summary>
    /// <param name="from">The route navigated from.</param>
    /// <param name="to">The route navigated to.</param>
    public static void RecordNavigation(string from, string to)
    {
        NavigationCount.Add(1, new KeyValuePair<string, object?>("route.from", from),
                                new KeyValuePair<string, object?>("route.to", to));
    }

    /// <summary>
    /// Records a key event.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    public static void RecordKeyEvent(ConsoleKey key)
    {
        KeyEvents.Add(1, new KeyValuePair<string, object?>("key", key.ToString()));
    }

    /// <summary>
    /// Starts an activity for a render frame.
    /// </summary>
    public static Activity? StartRenderActivity()
    {
        return ActivitySource.StartActivity("RenderFrame");
    }

    /// <summary>
    /// Starts an activity for navigation.
    /// </summary>
    /// <param name="from">The route navigating from.</param>
    /// <param name="to">The route navigating to.</param>
    public static Activity? StartNavigationActivity(string from, string to)
    {
        var activity = ActivitySource.StartActivity("Navigate");
        activity?.SetTag("route.from", from);
        activity?.SetTag("route.to", to);
        return activity;
    }

    /// <summary>
    /// Starts an activity for processing a key event.
    /// </summary>
    /// <param name="key">The key being processed.</param>
    /// <param name="keyChar">The character of the key.</param>
    public static Activity? StartKeyEventActivity(ConsoleKey key, char keyChar)
    {
        var activity = ActivitySource.StartActivity("KeyEvent");
        activity?.SetTag("key", key.ToString());
        activity?.SetTag("char", keyChar.ToString());
        return activity;
    }
}

