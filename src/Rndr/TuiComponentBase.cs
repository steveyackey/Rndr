using Rndr.Layout;

namespace Rndr;

/// <summary>
/// Base class for .tui single-file components.
/// Provides access to context and state management.
/// </summary>
public abstract class TuiComponentBase
{
    private ViewContext? _context;

    /// <summary>
    /// Gets the view context. Only available after AttachContext has been called.
    /// </summary>
    protected ViewContext Context => _context ?? throw new InvalidOperationException("Context not attached. Call AttachContext first.");

    /// <summary>
    /// Attaches the view context to this component.
    /// Called by the framework before Build is invoked.
    /// </summary>
    /// <param name="context">The view context.</param>
    public void AttachContext(ViewContext context)
    {
        _context = context;
        OnInit();
    }

    /// <summary>
    /// Called after the context is attached and before Build.
    /// Override to perform initialization.
    /// </summary>
    protected virtual void OnInit()
    {
    }

    /// <summary>
    /// Builds the component's layout.
    /// Override this method to define the component's UI.
    /// </summary>
    /// <param name="layout">The layout builder to use.</param>
    public abstract void Build(LayoutBuilder layout);

    /// <summary>
    /// Gets or creates route-scoped state.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="key">The state key.</param>
    /// <param name="initialValue">The initial value if the state doesn't exist.</param>
    /// <returns>A signal containing the state value.</returns>
    protected Signal<T> State<T>(string key, T initialValue)
    {
        return Context.State(key, initialValue);
    }

    /// <summary>
    /// Gets or creates global state that persists across navigation.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    /// <param name="key">The state key.</param>
    /// <param name="initialValue">The initial value if the state doesn't exist.</param>
    /// <returns>A signal containing the state value.</returns>
    protected Signal<T> StateGlobal<T>(string key, T initialValue)
    {
        return Context.StateGlobal(key, initialValue);
    }
}

