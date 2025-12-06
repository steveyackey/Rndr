using Rndr.Input;
using Rndr.Layout;

namespace Rndr;

/// <summary>
/// Fluent configuration for defining a view's behavior and layout.
/// </summary>
public sealed class ViewDefinition
{
    internal string? ViewTitle { get; private set; }
    internal Action<ViewContext, LayoutBuilder>? ViewLayoutBuilder { get; private set; }
    internal Func<KeyEvent, ViewContext, bool>? KeyHandler { get; private set; }

    /// <summary>
    /// Sets the view title.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <returns>This definition for chaining.</returns>
    public ViewDefinition Title(string title)
    {
        ViewTitle = title;
        return this;
    }

    /// <summary>
    /// Configures the view's layout using a builder callback.
    /// </summary>
    /// <param name="build">The callback that builds the view layout.</param>
    /// <returns>This definition for chaining.</returns>
    public ViewDefinition Use(Action<ViewContext> build)
    {
        ViewLayoutBuilder = (ctx, layout) => build(ctx);
        return this;
    }

    /// <summary>
    /// Configures the view's layout using a builder callback with explicit layout parameter.
    /// </summary>
    /// <param name="build">The callback that builds the view layout.</param>
    /// <returns>This definition for chaining.</returns>
    public ViewDefinition Use(Action<ViewContext, LayoutBuilder> build)
    {
        ViewLayoutBuilder = build;
        return this;
    }

    /// <summary>
    /// Registers a key handler for this view.
    /// </summary>
    /// <param name="handler">Handler that returns true if the key was consumed.</param>
    /// <returns>This definition for chaining.</returns>
    public ViewDefinition OnKey(Func<KeyEvent, ViewContext, bool> handler)
    {
        KeyHandler = handler;
        return this;
    }
}

