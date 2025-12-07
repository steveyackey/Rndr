using Rndr.Layout;

namespace Rndr.Rendering;

/// <summary>
/// Interface for rendering layout node trees to the console.
/// </summary>
public interface ITuiRenderer
{
    /// <summary>
    /// Renders the given node tree to the console.
    /// </summary>
    /// <param name="rootNodes">The root nodes of the layout tree.</param>
    /// <param name="focusedButtonIndex">The index of the currently focused button, or -1 for none.</param>
    /// <param name="focusedTextInputIndex">The index of the currently focused text input, or -1 for none.</param>
    void Render(IReadOnlyList<Node> rootNodes, int focusedButtonIndex = -1, int focusedTextInputIndex = -1);
}

