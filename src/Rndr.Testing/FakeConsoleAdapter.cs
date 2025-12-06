using System.Text;
using Rndr.Rendering;

namespace Rndr.Testing;

/// <summary>
/// Fake console adapter for testing that records all output.
/// </summary>
public sealed class FakeConsoleAdapter : IConsoleAdapter
{
    private readonly char[,] _buffer;
    private readonly StringBuilder _rawOutput = new();
    private readonly List<WriteRecord> _writes = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeConsoleAdapter"/> class.
    /// </summary>
    /// <param name="width">The simulated console width.</param>
    /// <param name="height">The simulated console height.</param>
    public FakeConsoleAdapter(int width = 80, int height = 24)
    {
        WindowWidth = width;
        WindowHeight = height;
        _buffer = new char[height, width];
        Clear();
    }

    /// <inheritdoc />
    public int WindowWidth { get; }

    /// <inheritdoc />
    public int WindowHeight { get; }

    /// <summary>
    /// Gets whether the cursor is currently hidden.
    /// </summary>
    public bool IsCursorHidden { get; private set; }

    /// <inheritdoc />
    public void Clear()
    {
        for (var row = 0; row < WindowHeight; row++)
        {
            for (var col = 0; col < WindowWidth; col++)
            {
                _buffer[row, col] = ' ';
            }
        }
        _rawOutput.Clear();
    }

    /// <inheritdoc />
    public void WriteAt(int left, int top, string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        _writes.Add(new WriteRecord(left, top, text, foreground, background));
        _rawOutput.Append(text);

        // Write to buffer
        for (var i = 0; i < text.Length && left + i < WindowWidth; i++)
        {
            if (top >= 0 && top < WindowHeight && left + i >= 0)
            {
                _buffer[top, left + i] = text[i];
            }
        }
    }

    /// <inheritdoc />
    public void HideCursor()
    {
        IsCursorHidden = true;
    }

    /// <inheritdoc />
    public void ShowCursor()
    {
        IsCursorHidden = false;
    }

    /// <inheritdoc />
    public void Flush()
    {
        // No-op for fake adapter
    }

    /// <summary>
    /// Gets the complete output as a string.
    /// </summary>
    public string GetOutput() => _rawOutput.ToString();

    /// <summary>
    /// Gets all write records.
    /// </summary>
    public IReadOnlyList<WriteRecord> GetWrites() => _writes.AsReadOnly();

    /// <summary>
    /// Gets a specific row from the buffer.
    /// </summary>
    /// <param name="row">The row index.</param>
    public string GetRow(int row)
    {
        if (row < 0 || row >= WindowHeight)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(WindowWidth);
        for (var col = 0; col < WindowWidth; col++)
        {
            sb.Append(_buffer[row, col]);
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Gets the entire buffer as a string with newlines.
    /// </summary>
    public string GetBuffer()
    {
        var sb = new StringBuilder();
        for (var row = 0; row < WindowHeight; row++)
        {
            for (var col = 0; col < WindowWidth; col++)
            {
                sb.Append(_buffer[row, col]);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}

/// <summary>
/// Record of a write operation.
/// </summary>
/// <param name="Left">The column position.</param>
/// <param name="Top">The row position.</param>
/// <param name="Text">The text written.</param>
/// <param name="Foreground">The foreground color.</param>
/// <param name="Background">The background color.</param>
public sealed record WriteRecord(
    int Left,
    int Top,
    string Text,
    ConsoleColor? Foreground,
    ConsoleColor? Background);

