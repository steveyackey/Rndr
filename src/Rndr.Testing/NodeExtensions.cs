using Rndr.Layout;

namespace Rndr.Testing;

/// <summary>
/// Extension methods for finding and inspecting nodes in test scenarios.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Finds the first button with the specified label.
    /// </summary>
    /// <param name="nodes">The nodes to search.</param>
    /// <param name="label">The button label to find.</param>
    /// <returns>The found button node, or null if not found.</returns>
    public static ButtonNode? FindButton(this IReadOnlyList<Node> nodes, string label)
    {
        foreach (var node in nodes)
        {
            var found = FindButtonRecursive(node, label);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the first text node containing the specified text.
    /// </summary>
    /// <param name="nodes">The nodes to search.</param>
    /// <param name="contains">The text to search for.</param>
    /// <returns>The found text node, or null if not found.</returns>
    public static TextNode? FindText(this IReadOnlyList<Node> nodes, string contains)
    {
        foreach (var node in nodes)
        {
            var found = FindTextRecursive(node, contains);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the first panel with the specified title.
    /// </summary>
    /// <param name="nodes">The nodes to search.</param>
    /// <param name="title">The panel title to find.</param>
    /// <returns>The found panel node, or null if not found.</returns>
    public static PanelNode? FindPanel(this IReadOnlyList<Node> nodes, string title)
    {
        foreach (var node in nodes)
        {
            var found = FindPanelRecursive(node, title);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets all buttons from the node tree.
    /// </summary>
    /// <param name="nodes">The nodes to search.</param>
    /// <returns>All button nodes found.</returns>
    public static IReadOnlyList<ButtonNode> GetAllButtons(this IReadOnlyList<Node> nodes)
    {
        var buttons = new List<ButtonNode>();
        foreach (var node in nodes)
        {
            CollectButtonsRecursive(node, buttons);
        }
        return buttons;
    }

    /// <summary>
    /// Gets all text nodes from the node tree.
    /// </summary>
    /// <param name="nodes">The nodes to search.</param>
    /// <returns>All text nodes found.</returns>
    public static IReadOnlyList<TextNode> GetAllTexts(this IReadOnlyList<Node> nodes)
    {
        var texts = new List<TextNode>();
        foreach (var node in nodes)
        {
            CollectTextsRecursive(node, texts);
        }
        return texts;
    }

    private static ButtonNode? FindButtonRecursive(Node node, string label)
    {
        if (node is ButtonNode button && button.Label == label)
        {
            return button;
        }

        foreach (var child in node.Children)
        {
            var found = FindButtonRecursive(child, label);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static TextNode? FindTextRecursive(Node node, string contains)
    {
        if (node is TextNode text && text.Content.Contains(contains, StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        foreach (var child in node.Children)
        {
            var found = FindTextRecursive(child, contains);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static PanelNode? FindPanelRecursive(Node node, string title)
    {
        if (node is PanelNode panel && panel.Title == title)
        {
            return panel;
        }

        foreach (var child in node.Children)
        {
            var found = FindPanelRecursive(child, title);
            if (found != null)
            {
                return found;
            }
        }

        return null;
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

    private static void CollectTextsRecursive(Node node, List<TextNode> texts)
    {
        if (node is TextNode text)
        {
            texts.Add(text);
        }

        foreach (var child in node.Children)
        {
            CollectTextsRecursive(child, texts);
        }
    }
}

