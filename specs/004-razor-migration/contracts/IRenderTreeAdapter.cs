using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Rndr.Layout;

namespace Rndr.Rendering;

/// <summary>
/// Contract for adapters that convert Blazor RenderTree to Rndr LayoutBuilder.
/// Transforms Blazor's frame-based RenderTreeBuilder instructions into Rndr's semantic layout tree.
/// </summary>
public interface IRenderTreeAdapter
{
    /// <summary>
    /// Captures a RenderTreeBuilder's frames and converts to Rndr layout nodes.
    /// </summary>
    /// <param name="renderTree">The Blazor render tree builder containing component markup.</param>
    /// <returns>Rndr layout nodes ready for rendering to terminal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when render tree structure is invalid (unclosed elements, etc.).</exception>
    /// <exception cref="NotSupportedException">Thrown when unsupported HTML elements are encountered.</exception>
    IReadOnlyList<Node> AdaptRenderTree(RenderTreeBuilder renderTree);

    /// <summary>
    /// Maps a Razor element name to an Rndr primitive configuration.
    /// Supported elements: Column, Row, Panel, Text, Button, Spacer, Centered, TextInput.
    /// </summary>
    /// <param name="elementName">Razor element name (e.g., "Column", "Button").</param>
    /// <param name="attributes">Element attributes as key-value pairs.</param>
    /// <returns>Configuration action for LayoutBuilder to create the element.</returns>
    /// <exception cref="NotSupportedException">Element name not recognized as an Rndr primitive.</exception>
    Action<LayoutBuilder> MapElement(string elementName, IReadOnlyDictionary<string, object?> attributes);
}
