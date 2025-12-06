using System.Text;

namespace Rndr.Rendering;

/// <summary>
/// Default console adapter using ANSI escape sequences for terminal output.
/// </summary>
public sealed class SystemConsoleAdapter : IConsoleAdapter
{
    private readonly StringBuilder _buffer = new();
    private bool _useBuffer = true;

    /// <inheritdoc />
    public int WindowWidth => Console.WindowWidth;

    /// <inheritdoc />
    public int WindowHeight => Console.WindowHeight;

    /// <inheritdoc />
    public void Clear()
    {
        // ANSI escape: clear screen and move cursor to home position
        Write("\x1b[2J\x1b[H");
    }

    /// <inheritdoc />
    public void WriteAt(int left, int top, string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        // ANSI escape: move cursor to position (1-based)
        Write($"\x1b[{top + 1};{left + 1}H");

        if (foreground.HasValue || background.HasValue)
        {
            if (foreground.HasValue)
            {
                Write($"\x1b[{GetAnsiForegroundCode(foreground.Value)}m");
            }

            if (background.HasValue)
            {
                Write($"\x1b[{GetAnsiBackgroundCode(background.Value)}m");
            }
        }

        Write(text);

        if (foreground.HasValue || background.HasValue)
        {
            // Reset colors
            Write("\x1b[0m");
        }
    }

    /// <inheritdoc />
    public void HideCursor()
    {
        Write("\x1b[?25l");
    }

    /// <inheritdoc />
    public void ShowCursor()
    {
        Write("\x1b[?25h");
    }

    /// <inheritdoc />
    public void Flush()
    {
        if (_useBuffer && _buffer.Length > 0)
        {
            Console.Write(_buffer.ToString());
            _buffer.Clear();
        }
    }

    private void Write(string text)
    {
        if (_useBuffer)
        {
            _buffer.Append(text);
        }
        else
        {
            Console.Write(text);
        }
    }

    private static int GetAnsiForegroundCode(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => 30,
        ConsoleColor.DarkRed => 31,
        ConsoleColor.DarkGreen => 32,
        ConsoleColor.DarkYellow => 33,
        ConsoleColor.DarkBlue => 34,
        ConsoleColor.DarkMagenta => 35,
        ConsoleColor.DarkCyan => 36,
        ConsoleColor.Gray => 37,
        ConsoleColor.DarkGray => 90,
        ConsoleColor.Red => 91,
        ConsoleColor.Green => 92,
        ConsoleColor.Yellow => 93,
        ConsoleColor.Blue => 94,
        ConsoleColor.Magenta => 95,
        ConsoleColor.Cyan => 96,
        ConsoleColor.White => 97,
        _ => 37
    };

    private static int GetAnsiBackgroundCode(ConsoleColor color) => color switch
    {
        ConsoleColor.Black => 40,
        ConsoleColor.DarkRed => 41,
        ConsoleColor.DarkGreen => 42,
        ConsoleColor.DarkYellow => 43,
        ConsoleColor.DarkBlue => 44,
        ConsoleColor.DarkMagenta => 45,
        ConsoleColor.DarkCyan => 46,
        ConsoleColor.Gray => 47,
        ConsoleColor.DarkGray => 100,
        ConsoleColor.Red => 101,
        ConsoleColor.Green => 102,
        ConsoleColor.Yellow => 103,
        ConsoleColor.Blue => 104,
        ConsoleColor.Magenta => 105,
        ConsoleColor.Cyan => 106,
        ConsoleColor.White => 107,
        _ => 40
    };
}

