using Rndr.Razor.Parsing;

namespace Rndr.Razor.Generator;

/// <summary>
/// Context passed through code generation.
/// </summary>
public sealed class CodeGenerationContext
{
    /// <summary>
    /// The parsed .tui document.
    /// </summary>
    public TuiDocument Document { get; }

    /// <summary>
    /// Current indentation level.
    /// </summary>
    public int Indentation { get; set; }

    /// <summary>
    /// Current builder variable name in scope.
    /// </summary>
    public string CurrentBuilder { get; set; } = "layout";

    /// <summary>
    /// Track used variable names to avoid collisions.
    /// </summary>
    public HashSet<string> UsedVariables { get; } = new();

    /// <summary>
    /// The namespace for the generated class.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The class name for the generated component.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Creates a new code generation context.
    /// </summary>
    public CodeGenerationContext(TuiDocument document, string ns)
    {
        Document = document;
        Namespace = ns;
        ClassName = document.FileName;
    }

    /// <summary>
    /// Gets the next available variable name with the given prefix.
    /// </summary>
    public string GetNextVariable(string prefix)
    {
        if (!UsedVariables.Contains(prefix))
        {
            UsedVariables.Add(prefix);
            return prefix;
        }

        var i = 1;
        string name;
        do
        {
            name = $"{prefix}{i++}";
        } while (UsedVariables.Contains(name));

        UsedVariables.Add(name);
        return name;
    }

    /// <summary>
    /// Gets the indentation string for the current level.
    /// </summary>
    public string GetIndent() => new(' ', Indentation * 4);

    /// <summary>
    /// Increments indentation.
    /// </summary>
    public void Indent() => Indentation++;

    /// <summary>
    /// Decrements indentation.
    /// </summary>
    public void Dedent() => Indentation = Math.Max(0, Indentation - 1);
}

