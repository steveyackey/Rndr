using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rndr.Diagnostics;
using Rndr.Input;
using Rndr.Layout;
using Rndr.Navigation;
using Rndr.Rendering;

namespace Rndr;

/// <summary>
/// Default implementation of the event loop with render-input-dispatch cycle.
/// </summary>
public sealed class DefaultEventLoop : IEventLoop
{
    private readonly ITuiRenderer _renderer;
    private readonly IInputSource _inputSource;
    private readonly IStateStore _stateStore;
    private readonly ILogger<DefaultEventLoop> _logger;
    private readonly RndrOptions _options;

    private bool _isDirty = true;
    private int _focusedButtonIndex;
    private List<ButtonNode> _currentButtons = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEventLoop"/> class.
    /// </summary>
    public DefaultEventLoop(
        ITuiRenderer renderer,
        IInputSource inputSource,
        IStateStore stateStore,
        ILogger<DefaultEventLoop> logger,
        RndrOptions options)
    {
        _renderer = renderer;
        _inputSource = inputSource;
        _stateStore = stateStore;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc />
    public async Task RunAsync(TuiApp app, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event loop starting");

        while (!cancellationToken.IsCancellationRequested && !app.IsQuitting)
        {
            try
            {
                // Render if needed
                if (_isDirty)
                {
                    RenderCurrentView(app);
                    _isDirty = false;
                }

                // Process input
                if (_inputSource.KeyAvailable)
                {
                    var key = _inputSource.ReadKey(true);
                    using var activity = RndrDiagnostics.StartKeyEventActivity(key.Key, key.KeyChar);
                    RndrDiagnostics.RecordKeyEvent(key.Key);

                    ProcessKeyEvent(key, app);
                }
                else
                {
                    // Small delay to prevent CPU spinning
                    await Task.Delay(_options.IdleFrameDelay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event loop");
            }
        }

        _logger.LogInformation("Event loop stopped");
    }

    private void RenderCurrentView(TuiApp app)
    {
        var sw = Stopwatch.StartNew();
        using var activity = RndrDiagnostics.StartRenderActivity();

        var currentRoute = app.CurrentRoute;
        var viewRegistration = app.GetViewRegistration(currentRoute);

        if (viewRegistration == null)
        {
            _logger.LogWarning("No view registered for route: {Route}", currentRoute);
            return;
        }

        // Build the layout
        var layout = new LayoutBuilder();
        var navigationState = app.GetNavigationState();
        var navigationContext = new NavigationContext(
            navigationState,
            route => { app.Navigate(route); MarkDirty(); },
            () => { var result = app.Back(); MarkDirty(); return result; },
            route => { app.Replace(route); MarkDirty(); }
        );

        var viewContext = new ViewContext(
            app.Services,
            navigationContext,
            _logger,
            currentRoute,
            _stateStore,
            MarkDirty
        );

        // Handle component types vs inline view definitions
        if (viewRegistration.ComponentType != null)
        {
            // Instantiate the component
            var component = (TuiComponentBase)Activator.CreateInstance(viewRegistration.ComponentType)!;
            component.AttachContext(viewContext);
            component.Build(layout);
        }
        else
        {
            viewRegistration.LayoutBuilder?.Invoke(viewContext, layout);
        }

        // Build node tree and find buttons
        var nodes = layout.Build();
        _currentButtons = CollectButtons(nodes);

        // Ensure focus is valid
        if (_currentButtons.Count > 0)
        {
            _focusedButtonIndex = Math.Clamp(_focusedButtonIndex, 0, _currentButtons.Count - 1);
        }
        else
        {
            _focusedButtonIndex = -1;
        }

        // Render
        _renderer.Render(nodes, _focusedButtonIndex);

        sw.Stop();
        RndrDiagnostics.RecordFrameRendered(sw.Elapsed.TotalMilliseconds);
    }

    private void ProcessKeyEvent(KeyEvent key, TuiApp app)
    {
        // First, check global handlers
        if (app.HandleGlobalKey(key))
        {
            _isDirty = true;
            return;
        }

        // Then handle focus navigation
        switch (key.Key)
        {
            case ConsoleKey.Tab when key.Modifiers.HasFlag(ConsoleModifiers.Shift):
                // Shift+Tab: Move focus backward
                if (_currentButtons.Count > 0)
                {
                    _focusedButtonIndex = (_focusedButtonIndex - 1 + _currentButtons.Count) % _currentButtons.Count;
                    _isDirty = true;
                }
                break;

            case ConsoleKey.Tab:
                // Tab: Move focus forward
                if (_currentButtons.Count > 0)
                {
                    _focusedButtonIndex = (_focusedButtonIndex + 1) % _currentButtons.Count;
                    _isDirty = true;
                }
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                // Activate focused button
                if (_focusedButtonIndex >= 0 && _focusedButtonIndex < _currentButtons.Count)
                {
                    var button = _currentButtons[_focusedButtonIndex];
                    button.OnClick?.Invoke();
                    _isDirty = true;
                }
                break;
        }
    }

    private void MarkDirty()
    {
        _isDirty = true;
    }

    private static List<ButtonNode> CollectButtons(IReadOnlyList<Node> nodes)
    {
        var buttons = new List<ButtonNode>();
        foreach (var node in nodes)
        {
            CollectButtonsRecursive(node, buttons);
        }
        return buttons;
    }

    private static void CollectButtonsRecursive(Node node, List<ButtonNode> buttons)
    {
        if (node is ButtonNode button)
        {
            buttons.Add(button);
        }

        foreach (var child in node.Children)
        {
            CollectButtonsRecursive(child, buttons);
        }
    }
}

