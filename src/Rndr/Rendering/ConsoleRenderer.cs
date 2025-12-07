using Rndr.Layout;

namespace Rndr.Rendering;

/// <summary>
/// Renders layout trees to the console using ANSI escape sequences.
/// </summary>
public sealed class ConsoleRenderer : ITuiRenderer
{
    private readonly IConsoleAdapter _console;
    private readonly RndrTheme _theme;
    private int _currentRow;
    private int _buttonIndex;
    private int _focusedButtonIndex;
    private int _textInputIndex;
    private int _focusedTextInputIndex;
    private int _cursorLeft = -1;
    private int _cursorTop = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleRenderer"/> class.
    /// </summary>
    /// <param name="console">The console adapter to use for output.</param>
    /// <param name="theme">The theme to use for rendering.</param>
    public ConsoleRenderer(IConsoleAdapter console, RndrTheme theme)
    {
        _console = console;
        _theme = theme;
    }

    /// <inheritdoc />
    public void Render(IReadOnlyList<Node> rootNodes, int focusedButtonIndex = -1, int focusedTextInputIndex = -1)
    {
        _currentRow = 0;
        _buttonIndex = 0;
        _focusedButtonIndex = focusedButtonIndex;
        _textInputIndex = 0;
        _focusedTextInputIndex = focusedTextInputIndex;
        _cursorLeft = -1;
        _cursorTop = -1;

        // Hide cursor by default; we'll show it when a text input is focused
        _console.HideCursor();
        _console.Clear();

        foreach (var node in rootNodes)
        {
            RenderNode(node, 0, _console.WindowWidth);
        }

        // After rendering everything, place the cursor at the focused text input (if any)
        if (_cursorLeft >= 0 && _cursorTop >= 0)
        {
            _console.ShowCursor();
            _console.WriteAt(_cursorLeft, _cursorTop, string.Empty);
        }

        _console.Flush();
    }

    private void RenderNode(Node node, int leftOffset, int availableWidth)
    {
        switch (node.Kind)
        {
            case NodeKind.Column:
                RenderColumn(node, leftOffset, availableWidth);
                break;
            case NodeKind.Row:
                RenderRow(node, leftOffset, availableWidth);
                break;
            case NodeKind.Panel:
                RenderPanel((PanelNode)node, leftOffset, availableWidth);
                break;
            case NodeKind.Modal:
                RenderModal((ModalNode)node, leftOffset, availableWidth);
                break;
            case NodeKind.Text:
                RenderText((TextNode)node, leftOffset, availableWidth);
                break;
            case NodeKind.Button:
                RenderButton((ButtonNode)node, leftOffset);
                break;
            case NodeKind.Spacer:
                RenderSpacer((SpacerNode)node);
                break;
            case NodeKind.Centered:
                RenderCentered(node, leftOffset, availableWidth);
                break;
            case NodeKind.TextInput:
                RenderTextInput((TextInputNode)node, leftOffset, availableWidth);
                break;
        }
    }

    private void RenderColumn(Node node, int leftOffset, int availableWidth)
    {
        var padding = node.Style.Padding ?? 0;
        var gap = node.Style.Gap ?? 0;
        var innerOffset = leftOffset + padding;
        var innerWidth = availableWidth - (padding * 2);

        _currentRow += padding;

        for (var i = 0; i < node.Children.Count; i++)
        {
            if (i > 0)
            {
                _currentRow += gap;
            }
            RenderNode(node.Children[i], innerOffset, innerWidth);
        }

        _currentRow += padding;
    }

    private void RenderRow(Node node, int leftOffset, int availableWidth)
    {
        var gap = node.Style.Gap ?? 1;
        var childCount = node.Children.Count;

        if (childCount == 0)
        {
            return;
        }

        // Calculate space for each child (simple equal distribution)
        var spacerCount = node.Children.Count(c => c.Kind == NodeKind.Spacer);
        var nonSpacerCount = childCount - spacerCount;
        var totalGap = (childCount - 1) * gap;

        // Estimate widths for non-spacer items
        var buttonWidths = node.Children
            .Where(c => c.Kind == NodeKind.Button)
            .Cast<ButtonNode>()
            .Sum(b => (b.Label.Length + 4)); // [ label ]

        var textWidths = node.Children
            .Where(c => c.Kind == NodeKind.Text)
            .Cast<TextNode>()
            .Sum(t => t.Content.Length);

        var fixedWidth = buttonWidths + textWidths + totalGap;
        var remainingWidth = availableWidth - fixedWidth;
        var spacerWidth = spacerCount > 0 ? remainingWidth / spacerCount : 0;

        var currentLeft = leftOffset;
        var startRow = _currentRow;

        foreach (var child in node.Children)
        {
            _currentRow = startRow; // Reset row for each horizontal element

            int childWidth;
            if (child.Kind == NodeKind.Spacer)
            {
                childWidth = spacerWidth;
                currentLeft += childWidth;
            }
            else if (child.Kind == NodeKind.Button)
            {
                childWidth = ((ButtonNode)child).Label.Length + 4;
                RenderNode(child, currentLeft, childWidth);
                currentLeft += childWidth + gap;
            }
            else if (child.Kind == NodeKind.Text)
            {
                childWidth = ((TextNode)child).Content.Length;
                RenderNode(child, currentLeft, childWidth);
                currentLeft += childWidth + gap;
            }
            else
            {
                childWidth = availableWidth / nonSpacerCount;
                RenderNode(child, currentLeft, childWidth);
                currentLeft += childWidth + gap;
            }
        }

        _currentRow = startRow + 1;
    }

    private void RenderPanel(PanelNode node, int leftOffset, int availableWidth)
    {
        var borderChars = _theme.Panel.BorderStyle == BorderStyle.Rounded
            ? ("╭", "╮", "╰", "╯", "─", "│")
            : ("┌", "┐", "└", "┘", "─", "│");

        var (topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical) = borderChars;

        // Top border with title
        var title = node.Title ?? "";
        var titleDisplay = string.IsNullOrEmpty(title) ? "" : $" {title} ";
        var topBorderLength = availableWidth - 2 - titleDisplay.Length;
        var topBorder = topLeft + titleDisplay + new string(horizontal[0], Math.Max(0, topBorderLength)) + topRight;

        _console.WriteAt(leftOffset, _currentRow, topBorder, _theme.TextColor);
        _currentRow++;

        // Store starting row for content
        var contentStartRow = _currentRow;

        // Render children
        foreach (var child in node.Children)
        {
            RenderNode(child, leftOffset + 2, availableWidth - 4);
        }

        // Add vertical borders for each content row
        var contentEndRow = _currentRow;
        for (var row = contentStartRow; row < contentEndRow; row++)
        {
            _console.WriteAt(leftOffset, row, vertical, _theme.TextColor);
            _console.WriteAt(leftOffset + availableWidth - 1, row, vertical, _theme.TextColor);
        }

        // Bottom border
        var bottomBorder = bottomLeft + new string(horizontal[0], availableWidth - 2) + bottomRight;
        _console.WriteAt(leftOffset, _currentRow, bottomBorder, _theme.TextColor);
        _currentRow++;
    }

    private void RenderModal(ModalNode node, int leftOffset, int availableWidth)
    {
        // Modals are rendered like panels with emphasis and centered positioning
        // Use double-line borders to make them more prominent
        var borderChars = ("╔", "╗", "╚", "╝", "═", "║");
        var (topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical) = borderChars;

        // Determine modal width - use provided width or default to 60% of available width
        var modalWidth = node.Width ?? Math.Min(60, (int)(availableWidth * 0.6));
        var centerOffset = (availableWidth - modalWidth) / 2;
        var actualLeft = leftOffset + centerOffset;

        // Top border with title
        var title = node.Title ?? "";
        var titleDisplay = string.IsNullOrEmpty(title) ? "" : $" {title} ";
        var topBorderLength = modalWidth - 2 - titleDisplay.Length;
        var topBorder = topLeft + titleDisplay + new string(horizontal[0], Math.Max(0, topBorderLength)) + topRight;

        _console.WriteAt(actualLeft, _currentRow, topBorder, _theme.AccentColor);
        _currentRow++;

        // Store starting row for content
        var contentStartRow = _currentRow;

        // Render children
        foreach (var child in node.Children)
        {
            RenderNode(child, actualLeft + 2, modalWidth - 4);
        }

        // Add vertical borders for each content row
        var contentEndRow = _currentRow;
        for (var row = contentStartRow; row < contentEndRow; row++)
        {
            _console.WriteAt(actualLeft, row, vertical, _theme.AccentColor);
            _console.WriteAt(actualLeft + modalWidth - 1, row, vertical, _theme.AccentColor);
        }

        // Bottom border
        var bottomBorder = bottomLeft + new string(horizontal[0], modalWidth - 2) + bottomRight;
        _console.WriteAt(actualLeft, _currentRow, bottomBorder, _theme.AccentColor);
        _currentRow++;
    }

    private void RenderText(TextNode node, int leftOffset, int availableWidth)
    {
        var color = node.Style.Accent ? _theme.AccentColor :
                    node.Style.Faint ? _theme.MutedTextColor :
                    _theme.TextColor;

        var content = node.Content;

        // Handle alignment
        if (node.Style.Align == TextAlign.Center)
        {
            var padding = Math.Max(0, (availableWidth - content.Length) / 2);
            leftOffset += padding;
        }
        else if (node.Style.Align == TextAlign.Right)
        {
            var padding = Math.Max(0, availableWidth - content.Length);
            leftOffset += padding;
        }

        // Apply bold via ANSI if supported
        if (node.Style.Bold)
        {
            content = $"\x1b[1m{content}\x1b[22m";
        }

        _console.WriteAt(leftOffset, _currentRow, content, color);
        _currentRow++;
    }

    private void RenderButton(ButtonNode node, int leftOffset)
    {
        var isFocused = _buttonIndex == _focusedButtonIndex;
        var label = node.Label;

        var displayText = isFocused ? $"[ {label} ]" : $"  {label}  ";
        var color = node.IsPrimary || isFocused ? _theme.AccentColor : _theme.TextColor;

        _console.WriteAt(leftOffset, _currentRow, displayText, color);
        _currentRow++;
        _buttonIndex++;
    }

    private void RenderSpacer(SpacerNode node)
    {
        _currentRow += node.Weight;
    }

    private void RenderCentered(Node node, int leftOffset, int availableWidth)
    {
        // For centering, we need to calculate the content width first
        // For simplicity, just add some left offset
        var centerOffset = availableWidth / 4;
        foreach (var child in node.Children)
        {
            RenderNode(child, leftOffset + centerOffset, availableWidth - (centerOffset * 2));
        }
    }

    private void RenderTextInput(TextInputNode node, int leftOffset, int availableWidth)
    {
        var isFocused = _textInputIndex == _focusedTextInputIndex;
        var displayValue = string.IsNullOrEmpty(node.Value) ? node.Placeholder ?? "" : node.Value;
        var color = isFocused ? _theme.AccentColor :
                    string.IsNullOrEmpty(node.Value) ? _theme.MutedTextColor :
                    _theme.TextColor;

        var content = displayValue.PadRight(Math.Max(10, availableWidth - 4));
        var inputDisplay = $"[{content}]";
        var row = _currentRow;

        _console.WriteAt(leftOffset, row, inputDisplay, color);

        if (isFocused)
        {
            // Position cursor just after the current text inside the brackets
            _cursorLeft = leftOffset + 1 + displayValue.Length;
            _cursorTop = row;
        }

        _currentRow++;
        _textInputIndex++;
    }
}

