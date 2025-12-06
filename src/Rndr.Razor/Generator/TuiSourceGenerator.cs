using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Rndr.Razor.Parsing;

namespace Rndr.Razor.Generator;

/// <summary>
/// Roslyn incremental source generator for .tui files.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class TuiSourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter for .tui files from AdditionalFiles and select their content
        // This enables incremental caching - only regenerate when .tui content changes
        var tuiFilesWithContent = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".tui", StringComparison.OrdinalIgnoreCase))
            .Select((file, ct) => 
            {
                var text = file.GetText(ct);
                return new TuiFileInfo(file.Path, text?.ToString() ?? "");
            });

        // Get root namespace from compilation (cached separately)
        var rootNamespaceProvider = context.CompilationProvider
            .Select((compilation, ct) => GetRootNamespace(compilation));

        // Combine each file with root namespace - this allows per-file caching
        var filesWithNamespace = tuiFilesWithContent.Combine(rootNamespaceProvider);

        // Also get compilation for namespace determination based on file paths
        var filesWithCompilation = filesWithNamespace.Combine(context.CompilationProvider);

        // Register source output for each file
        context.RegisterSourceOutput(filesWithCompilation, (spc, source) =>
        {
            var ((fileInfo, rootNamespace), compilation) = source;
            ProcessTuiFileIncremental(spc, fileInfo, rootNamespace, compilation);
        });
    }
    
    /// <summary>
    /// Holds cached .tui file information for incremental processing.
    /// </summary>
    private sealed class TuiFileInfo : IEquatable<TuiFileInfo>
    {
        public string Path { get; }
        public string Content { get; }
        
        public TuiFileInfo(string path, string content)
        {
            Path = path;
            Content = content;
        }
        
        public bool Equals(TuiFileInfo? other)
        {
            if (other is null) return false;
            return Path == other.Path && Content == other.Content;
        }
        
        public override bool Equals(object? obj) => Equals(obj as TuiFileInfo);
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (Path.GetHashCode() * 397) ^ Content.GetHashCode();
            }
        }
    }

    private void ProcessTuiFileIncremental(
        SourceProductionContext context,
        TuiFileInfo fileInfo,
        string rootNamespace,
        Compilation compilation)
    {
        if (string.IsNullOrEmpty(fileInfo.Content))
        {
            return;
        }

        try
        {
            // Parse the .tui file
            var document = TuiSyntaxParser.ParseDocument(fileInfo.Path, fileInfo.Content);

            // Report any diagnostics from parsing
            foreach (var diagnostic in document.Diagnostics)
            {
                var location = CreateLocationFromContent(fileInfo.Path, fileInfo.Content, diagnostic.Location);
                var severity = diagnostic.Severity switch
                {
                    TuiDiagnosticSeverity.Error => DiagnosticSeverity.Error,
                    TuiDiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                    _ => DiagnosticSeverity.Info
                };

                var descriptor = GetDiagnosticDescriptor(diagnostic.Code);
                if (descriptor != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, location, diagnostic.Message));
                }
            }

            // Don't generate code if there are errors
            if (document.HasErrors)
            {
                return;
            }

            // Determine namespace from file path
            var ns = DetermineNamespace(fileInfo.Path, rootNamespace, compilation);

            // Generate C# source code
            var source = TuiCodeEmitter.EmitComponent(document, ns);

            // Add to compilation
            var hintName = $"{document.FileName}.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
        catch (Exception ex)
        {
            // Report any unexpected errors
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "TUI999",
                    "Generator Error",
                    "Error processing {0}: {1}",
                    "TuiGenerator",
                    DiagnosticSeverity.Error,
                    isEnabledByDefault: true),
                Location.None,
                fileInfo.Path,
                ex.Message));
        }
    }
    
    private Location CreateLocationFromContent(string filePath, string content, SourceLocation tuiLocation)
    {
        if (tuiLocation.Line <= 0 || string.IsNullOrEmpty(content))
        {
            return Location.None;
        }

        try
        {
            var text = SourceText.From(content);
            
            // Convert 1-based line/column to 0-based
            var line = Math.Max(0, tuiLocation.Line - 1);
            var column = Math.Max(0, tuiLocation.Column - 1);

            // Get line span
            if (line >= text.Lines.Count)
            {
                return Location.None;
            }

            var textLine = text.Lines[line];
            var start = textLine.Start + Math.Min(column, textLine.Span.Length);
            var length = Math.Min(tuiLocation.Length, textLine.End - start);
            if (length <= 0) length = 1;

            var span = new TextSpan(start, length);
            var lineSpan = text.Lines.GetLinePositionSpan(span);

            return Location.Create(filePath, span, lineSpan);
        }
        catch
        {
            return Location.None;
        }
    }

    private string GetRootNamespace(Compilation compilation)
    {
        // Strategy: Look at all namespaces and find the common root.
        // For namespaces like "A.B.C.D" and "A.B.C.E", the root is "A.B.C"
        // But we also need to account for file paths to identify folder-based namespace suffixes.
        
        var namespaces = new HashSet<string>();
        var filePathToNamespace = new Dictionary<string, string>();
        
        foreach (var tree in compilation.SyntaxTrees)
        {
            if (string.IsNullOrEmpty(tree.FilePath))
                continue;
                
            var root = tree.GetRoot();
            var nsDecl = root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax>()
                .Select(n => n.Name.ToString())
                .FirstOrDefault();
            
            if (!string.IsNullOrEmpty(nsDecl))
            {
                namespaces.Add(nsDecl);
                filePathToNamespace[tree.FilePath] = nsDecl;
            }
        }
        
        if (namespaces.Count == 0)
        {
            return compilation.AssemblyName ?? "GeneratedNamespace";
        }
        
        // If there's only one namespace, check if it ends with a folder name from its path
        if (namespaces.Count == 1)
        {
            var ns = namespaces.First();
            var parts = ns.Split('.');
            
            // Check if the last part is a folder name (like "Models" or "Pages")
            // If so, the root namespace is everything before it
            if (parts.Length > 1)
            {
                var lastPart = parts[parts.Length - 1];
                // Common folder suffixes that should not be part of root namespace
                var folderSuffixes = new[] { "Models", "Pages", "Services", "Controllers", "Components", "Views", "ViewModels" };
                if (folderSuffixes.Contains(lastPart, StringComparer.OrdinalIgnoreCase))
                {
                    return string.Join(".", parts.Take(parts.Length - 1));
                }
            }
            
            return ns;
        }
        
        // Multiple namespaces - find common prefix
        string? rootNamespace = null;
        foreach (var ns in namespaces)
        {
            if (rootNamespace == null)
            {
                rootNamespace = ns;
            }
            else
            {
                rootNamespace = GetCommonNamespacePrefix(rootNamespace, ns);
                if (string.IsNullOrEmpty(rootNamespace))
                {
                    // No common prefix found, fall back to assembly name
                    return compilation.AssemblyName ?? "GeneratedNamespace";
                }
            }
        }
        
        return rootNamespace ?? compilation.AssemblyName ?? "GeneratedNamespace";
    }
    
    private string GetCommonNamespacePrefix(string ns1, string ns2)
    {
        var parts1 = ns1.Split('.');
        var parts2 = ns2.Split('.');
        var common = new List<string>();
        var minLength = Math.Min(parts1.Length, parts2.Length);
        
        for (var i = 0; i < minLength; i++)
        {
            if (parts1[i] == parts2[i])
            {
                common.Add(parts1[i]);
            }
            else
            {
                break;
            }
        }
        
        return string.Join(".", common);
    }

    private string DetermineNamespace(string filePath, string rootNamespace, Compilation compilation)
    {
        // Get directory containing the .tui file
        var directory = Path.GetDirectoryName(filePath) ?? "";
        
        // Find the project root by looking at source files in compilation
        // The project root is the common ancestor of all source files
        string? projectRoot = null;
        
        foreach (var tree in compilation.SyntaxTrees)
        {
            var treePath = tree.FilePath;
            if (string.IsNullOrEmpty(treePath))
                continue;
                
            var treeDir = Path.GetDirectoryName(treePath);
            if (string.IsNullOrEmpty(treeDir))
                continue;
            
            if (projectRoot == null)
            {
                projectRoot = treeDir;
            }
            else
            {
                // Find shortest common ancestor
                var ancestor = FindCommonAncestor(projectRoot, treeDir);
                if (!string.IsNullOrEmpty(ancestor))
                {
                    projectRoot = ancestor;
                }
            }
        }

        if (string.IsNullOrEmpty(projectRoot))
        {
            // Fall back to just appending directory name to root namespace
            var dirName = Path.GetFileName(directory);
            if (!string.IsNullOrEmpty(dirName) && !string.Equals(dirName, rootNamespace, StringComparison.OrdinalIgnoreCase))
            {
                return $"{rootNamespace}.{dirName}";
            }
            return rootNamespace;
        }

        // Calculate relative path from project root to the .tui file's directory
        string relativePath;
        if (projectRoot != null && directory.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            relativePath = directory.Substring(projectRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        else
        {
            // If .tui file is outside project root, use its immediate directory
            relativePath = Path.GetFileName(directory) ?? "";
        }

        if (string.IsNullOrEmpty(relativePath))
        {
            return rootNamespace;
        }

        // Convert path separators to namespace separators
        var folders = relativePath
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        return $"{rootNamespace}.{folders}";
    }

    private string FindCommonAncestor(string path1, string path2)
    {
        var parts1 = path1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parts2 = path2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        var common = new List<string>();
        var minLength = Math.Min(parts1.Length, parts2.Length);

        for (var i = 0; i < minLength; i++)
        {
            if (string.Equals(parts1[i], parts2[i], StringComparison.OrdinalIgnoreCase))
            {
                common.Add(parts1[i]);
            }
            else
            {
                break;
            }
        }

        return string.Join(Path.DirectorySeparatorChar.ToString(), common);
    }

    private DiagnosticDescriptor? GetDiagnosticDescriptor(string code)
    {
        return code switch
        {
            "TUI001" => DiagnosticDescriptors.MissingViewDirective,
            "TUI002" => DiagnosticDescriptors.ViewDirectiveNotFirst,
            "TUI003" => DiagnosticDescriptors.UnknownTag,
            "TUI004" => DiagnosticDescriptors.UnknownAttribute,
            "TUI005" => DiagnosticDescriptors.InvalidAttributeValue,
            "TUI006" => DiagnosticDescriptors.UnclosedTag,
            "TUI007" => DiagnosticDescriptors.UnexpectedClosingTag,
            "TUI008" => DiagnosticDescriptors.CodeBlockSyntaxError,
            "TUI009" => DiagnosticDescriptors.EmptyCodeBlock,
            "TUI010" => DiagnosticDescriptors.GeneratedFileInfo,
            _ => null
        };
    }
}

