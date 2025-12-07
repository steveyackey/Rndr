using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rndr.Diagnostics;
using Rndr.Input;
using Rndr.Layout;
using Rndr.Navigation;
using Rndr.Rendering;

namespace Rndr;

/// <summary>
/// Async-aware event loop with timeout and cancellation support for component lifecycle.
/// </summary>
public sealed class AsyncEventLoop : IAsyncEventLoop
{
    private readonly ITuiRenderer _renderer;
    private readonly IInputSource _inputSource;
    private readonly IStateStore _stateStore;
    private readonly ILogger<AsyncEventLoop> _logger;
    private readonly RndrOptions _options;

    private bool _isDirty = true;
    private int _focusedElementIndex = -1;
    private List<ButtonNode> _currentButtons = [];
    private List<TextInputNode> _currentTextInputs = [];
    private List<FocusableElement> _focusableElements = [];
    private CancellationTokenSource _componentCts = new();
    
    private enum FocusType { Button, TextInput }
    
    private sealed class FocusableElement
    {
        public FocusType Type { get; set; }
        public int ButtonIndex { get; set; }
        public int TextInputIndex { get; set; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncEventLoop"/> class.
    /// </summary>
    public AsyncEventLoop(
        ITuiRenderer renderer,
        IInputSource inputSource,
        IStateStore stateStore,
        ILogger<AsyncEventLoop> logger,
        RndrOptions options)
    {
        _renderer = renderer;
        _inputSource = inputSource;
        _stateStore = stateStore;
        _logger = logger;
        _options = options;
        AsyncTimeout = options.AsyncTimeout;
    }

    /// <inheritdoc />
    public TimeSpan AsyncTimeout { get; set; }

    /// <inheritdoc />
    public CancellationToken ComponentCancellationToken => _componentCts.Token;

    /// <inheritdoc />
    public async Task RunAsync(TuiApp app, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Async event loop starting");

        try
        {
            while (!cancellationToken.IsCancellationRequested && !app.IsQuitting)
            {
                try
                {
                    // Render if needed
                    if (_isDirty)
                    {
                        await RenderAsync(app, cancellationToken);
                        _isDirty = false;
                    }

                    // Process input
                    if (_inputSource.KeyAvailable)
                    {
                        var key = _inputSource.ReadKey(true);
                        using var activity = RndrDiagnostics.StartKeyEventActivity(key.Key, key.KeyChar);
                        RndrDiagnostics.RecordKeyEvent(key.Key);

                        await ProcessKeyEventAsync(key, app);
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
                    _logger.LogError(ex, "Error in async event loop");
                }
            }
        }
        finally
        {
            // Cleanup: cancel component operations and wait briefly for graceful shutdown
            const int GracefulShutdownDelayMs = 100;
            _componentCts.Cancel();
            await Task.Delay(GracefulShutdownDelayMs, CancellationToken.None);
            _componentCts.Dispose();
        }

        _logger.LogInformation("Async event loop stopped");
    }

    private Task RenderAsync(TuiApp app, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        using var activity = RndrDiagnostics.StartRenderActivity();

        var currentRoute = app.CurrentRoute;
        var viewRegistration = app.GetViewRegistration(currentRoute);

        if (viewRegistration == null)
        {
            _logger.LogWarning("No view registered for route: {Route}", currentRoute);
            return Task.CompletedTask;
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
            // Instantiate the component using ActivatorUtilities for constructor injection
            var component = (TuiComponentBase)ActivatorUtilities.CreateInstance(
                app.Services, viewRegistration.ComponentType);
            
            // Populate @inject properties (property injection for generated components)
            PopulateInjectProperties(component, app.Services);
            
            component.AttachContext(viewContext);
            
            // For now, components don't have async lifecycle yet - that will come in later tasks
            // This AsyncEventLoop is ready to await them when they're added
            component.Build(layout);
        }
        else
        {
            viewRegistration.LayoutBuilder?.Invoke(viewContext, layout);
        }

        // Build node tree and find focusable elements in DOM order
        var nodes = layout.Build();
        _currentButtons = CollectButtons(nodes);
        _currentTextInputs = CollectTextInputs(nodes);
        _focusableElements = CollectFocusableElementsInOrder(nodes);

        // Ensure focus is valid
        if (_focusableElements.Count > 0)
        {
            if (_focusedElementIndex < 0 || _focusedElementIndex >= _focusableElements.Count)
            {
                _focusedElementIndex = 0;
            }
            else
            {
                _focusedElementIndex = Math.Clamp(_focusedElementIndex, 0, _focusableElements.Count - 1);
            }
        }
        else
        {
            _focusedElementIndex = -1;
        }

        // Determine which button/text input is focused for rendering
        var focusedButtonIndex = -1;
        var focusedTextInputIndex = -1;
        if (_focusedElementIndex >= 0 && _focusedElementIndex < _focusableElements.Count)
        {
            var focused = _focusableElements[_focusedElementIndex];
            if (focused.Type == FocusType.Button)
            {
                focusedButtonIndex = focused.ButtonIndex;
            }
            else
            {
                focusedTextInputIndex = focused.TextInputIndex;
            }
        }

        _renderer.Render(nodes, focusedButtonIndex, focusedTextInputIndex);

        sw.Stop();
        RndrDiagnostics.RecordFrameRendered(sw.Elapsed.TotalMilliseconds);
        
        return Task.CompletedTask;
    }

    private Task ProcessKeyEventAsync(KeyEvent key, TuiApp app)
    {
        // Handle text input if a TextInput is focused
        if (_focusedElementIndex >= 0 && _focusedElementIndex < _focusableElements.Count)
        {
            var focused = _focusableElements[_focusedElementIndex];
            if (focused.Type == FocusType.TextInput)
            {
                var textInput = _currentTextInputs[focused.TextInputIndex];
                if (HandleTextInputKey(key, textInput))
                {
                    _isDirty = true;
                    return Task.CompletedTask;
                }
            }
        }

        // Check global handlers
        if (app.HandleGlobalKey(key))
        {
            _isDirty = true;
            return Task.CompletedTask;
        }

        // Handle focus navigation and button activation
        switch (key.Key)
        {
            case ConsoleKey.Tab when key.Modifiers.HasFlag(ConsoleModifiers.Shift):
                if (_focusableElements.Count > 0)
                {
                    _focusedElementIndex = (_focusedElementIndex - 1 + _focusableElements.Count) % _focusableElements.Count;
                    _isDirty = true;
                }
                break;

            case ConsoleKey.Tab:
                if (_focusableElements.Count > 0)
                {
                    _focusedElementIndex = (_focusedElementIndex + 1) % _focusableElements.Count;
                    _isDirty = true;
                }
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                if (_focusedElementIndex >= 0 && _focusedElementIndex < _focusableElements.Count)
                {
                    var focused = _focusableElements[_focusedElementIndex];
                    if (focused.Type == FocusType.Button)
                    {
                        var button = _currentButtons[focused.ButtonIndex];
                        button.OnClick?.Invoke();
                        _isDirty = true;
                    }
                }
                break;
        }

        return Task.CompletedTask;
    }

    private bool HandleTextInputKey(KeyEvent key, TextInputNode textInput)
    {
        switch (key.Key)
        {
            case ConsoleKey.Backspace:
                if (textInput.Value.Length > 0)
                {
                    var backspaceValue = textInput.Value[..^1];
                    textInput.Value = backspaceValue;
                    textInput.OnChanged?.Invoke(backspaceValue);
                    return true;
                }
                return true;

            case ConsoleKey.Delete:
                return true;

            case ConsoleKey.Enter:
            case ConsoleKey.Tab:
            case ConsoleKey.Escape:
                return false;

            default:
                string? updatedValue = null;
                
                if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                {
                    updatedValue = textInput.Value + key.KeyChar;
                }
                else if (key.Key >= ConsoleKey.A && key.Key <= ConsoleKey.Z)
                {
                    var charToAdd = (char)(key.Key - ConsoleKey.A + (key.Modifiers.HasFlag(ConsoleModifiers.Shift) ? 'A' : 'a'));
                    updatedValue = textInput.Value + charToAdd;
                }
                else if (key.Key >= ConsoleKey.D0 && key.Key <= ConsoleKey.D9)
                {
                    var charToAdd = (char)(key.Key - ConsoleKey.D0 + '0');
                    updatedValue = textInput.Value + charToAdd;
                }
                else if (key.Key >= ConsoleKey.NumPad0 && key.Key <= ConsoleKey.NumPad9)
                {
                    var charToAdd = (char)(key.Key - ConsoleKey.NumPad0 + '0');
                    updatedValue = textInput.Value + charToAdd;
                }
                else if (key.Key == ConsoleKey.Spacebar)
                {
                    updatedValue = textInput.Value + ' ';
                }
                
                if (updatedValue != null)
                {
                    textInput.Value = updatedValue;
                    textInput.OnChanged?.Invoke(updatedValue);
                    return true;
                }
                return false;
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

    private static List<TextInputNode> CollectTextInputs(IReadOnlyList<Node> nodes)
    {
        var textInputs = new List<TextInputNode>();
        foreach (var node in nodes)
        {
            CollectTextInputsRecursive(node, textInputs);
        }
        return textInputs;
    }

    private static void CollectTextInputsRecursive(Node node, List<TextInputNode> textInputs)
    {
        if (node is TextInputNode textInput)
        {
            textInputs.Add(textInput);
        }

        foreach (var child in node.Children)
        {
            CollectTextInputsRecursive(child, textInputs);
        }
    }

    private List<FocusableElement> CollectFocusableElementsInOrder(IReadOnlyList<Node> nodes)
    {
        var elements = new List<FocusableElement>();
        var buttonIndex = 0;
        var textInputIndex = 0;
        
        foreach (var node in nodes)
        {
            CollectFocusableElementsRecursive(node, elements, ref buttonIndex, ref textInputIndex);
        }
        
        return elements;
    }

    private static void CollectFocusableElementsRecursive(
        Node node, 
        List<FocusableElement> elements, 
        ref int buttonIndex, 
        ref int textInputIndex)
    {
        if (node is ButtonNode)
        {
            elements.Add(new FocusableElement
            {
                Type = FocusType.Button,
                ButtonIndex = buttonIndex++,
                TextInputIndex = -1
            });
        }
        else if (node is TextInputNode)
        {
            elements.Add(new FocusableElement
            {
                Type = FocusType.TextInput,
                ButtonIndex = -1,
                TextInputIndex = textInputIndex++
            });
        }

        foreach (var child in node.Children)
        {
            CollectFocusableElementsRecursive(child, elements, ref buttonIndex, ref textInputIndex);
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL2072:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method",
        Justification = "Property injection for DI is inherently reflection-based. Component types are preserved via MapView annotations.")]
    [UnconditionalSuppressMessage("AOT", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method",
        Justification = "Component types registered via MapView have PublicProperties preserved.")]
    private static void PopulateInjectProperties(TuiComponentBase component, IServiceProvider services)
    {
        var componentType = component.GetType();
        
        foreach (var property in componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite || property.GetSetMethod() == null)
                continue;
            
            var currentValue = property.GetValue(component);
            if (currentValue != null && !IsDefaultValue(currentValue, property.PropertyType))
                continue;
            
            var service = services.GetService(property.PropertyType);
            if (service != null)
            {
                property.SetValue(component, service);
            }
        }
    }

    private static bool IsDefaultValue(
        object value, 
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    {
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return Equals(value, defaultValue);
        }
        return value == null;
    }
}
