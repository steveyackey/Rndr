using System.Text;
using System.Text.RegularExpressions;

namespace Rndr.Razor.Parsing;

/// <summary>
/// Parses .tui files into TuiDocument representations.
/// </summary>
public sealed class TuiSyntaxParser
{
    private readonly string _filePath;
    private readonly string _content;
    private readonly TuiDocument _document;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    /// <summary>
    /// Valid tag names for .tui markup.
    /// </summary>
    public static readonly HashSet<string> ValidTags = new(StringComparer.Ordinal)
    {
        "Column", "Row", "Panel", "Modal", "Centered", "Text", "Button", "Spacer", "TextInput"
    };

    /// <summary>
    /// Container tags that can have children.
    /// </summary>
    public static readonly HashSet<string> ContainerTags = new(StringComparer.Ordinal)
    {
        "Column", "Row", "Panel", "Modal", "Centered"
    };

    /// <summary>
    /// Valid attribute names per tag.
    /// </summary>
    public static readonly Dictionary<string, HashSet<string>> ValidAttributes = new()
    {
        ["Column"] = new() { "Padding", "Gap" },
        ["Row"] = new() { "Gap" },
        ["Panel"] = new() { "Title", "Padding" },
        ["Modal"] = new() { "Title", "Width", "Height", "OnClose", "AllowDismiss" },
        ["Centered"] = new(),
        ["Text"] = new() { "Bold", "Accent", "Faint", "Align" },
        ["Button"] = new() { "OnClick", "Width", "Primary" },
        ["Spacer"] = new() { "Weight" },
        ["TextInput"] = new() { "Value", "OnChanged", "Placeholder" }
    };

    private TuiSyntaxParser(string filePath, string content)
    {
        _filePath = filePath;
        _content = content;
        _document = new TuiDocument(filePath);
    }

    /// <summary>
    /// Parses a .tui file and returns the document representation.
    /// </summary>
    public static TuiDocument ParseDocument(string filePath, string content)
    {
        var parser = new TuiSyntaxParser(filePath, content);
        return parser.Parse();
    }

    private TuiDocument Parse()
    {
        // Skip leading whitespace
        SkipWhitespace();

        // Parse directives first
        ParseDirectives();

        // Parse markup
        ParseMarkup();

        // Validate document
        ValidateDocument();

        return _document;
    }

    private void ParseDirectives()
    {
        while (!IsAtEnd())
        {
            SkipWhitespace();
            if (IsAtEnd()) break;

            // Check if we're at markup start (not a directive)
            if (Peek() == '<')
            {
                break;
            }

            // Check for directives
            if (Peek() == '@')
            {
                var directiveStart = CurrentLocation();
                Advance(); // consume @

                if (Match("view"))
                {
                    if (_document.ViewDirective != null)
                    {
                        _document.AddError("TUI002", "@view directive can only appear once", directiveStart);
                    }
                    else if (_document.UsingDirectives.Count > 0 || _document.InjectDirectives.Count > 0)
                    {
                        _document.AddError("TUI002", "@view directive must be first non-whitespace content", directiveStart);
                        _document.ViewDirective = new TuiViewDirective(directiveStart);
                    }
                    else
                    {
                        _document.ViewDirective = new TuiViewDirective(directiveStart);
                    }
                    SkipToEndOfLine();
                }
                else if (Match("using"))
                {
                    SkipWhitespace();
                    var ns = ReadToEndOfLine().Trim();
                    if (!string.IsNullOrEmpty(ns))
                    {
                        _document.UsingDirectives.Add(new TuiUsingDirective(ns, directiveStart));
                    }
                }
                else if (Match("inject"))
                {
                    SkipWhitespace();
                    var injectContent = ReadToEndOfLine().Trim();
                    ParseInjectDirective(injectContent, directiveStart);
                }
                else if (Match("code"))
                {
                    ParseCodeBlock(directiveStart);
                    break; // @code block should be at the end, markup follows before it
                }
                else
                {
                    // Unknown directive - could be an expression in markup, backtrack
                    _position--;
                    _column--;
                    break;
                }
            }
            else
            {
                // Not a directive, must be start of markup
                break;
            }
        }
    }

    private void ParseInjectDirective(string content, SourceLocation location)
    {
        // Parse "TypeName PropertyName"
        // Type can be generic like ILogger<Counter>
        var match = Regex.Match(content, @"^(.+?)\s+(\w+)$");
        if (match.Success)
        {
            var typeName = match.Groups[1].Value.Trim();
            var propertyName = match.Groups[2].Value.Trim();
            _document.InjectDirectives.Add(new TuiInjectDirective(typeName, propertyName, location));
        }
        else
        {
            _document.AddError("TUI004", $"Invalid @inject syntax: {content}", location);
        }
    }

    private void ParseCodeBlock(SourceLocation startLocation)
    {
        SkipWhitespace();

        // Expect opening brace
        if (Peek() != '{')
        {
            _document.AddError("TUI008", "Expected '{' after @code", CurrentLocation());
            return;
        }

        var openBraceLocation = CurrentLocation();
        Advance(); // consume {

        // Find matching closing brace
        var content = new StringBuilder();
        var braceDepth = 1;
        var closeBraceLocation = openBraceLocation;

        while (!IsAtEnd() && braceDepth > 0)
        {
            var c = Peek();
            if (c == '{')
            {
                braceDepth++;
                content.Append(c);
                Advance();
            }
            else if (c == '}')
            {
                braceDepth--;
                if (braceDepth > 0)
                {
                    content.Append(c);
                }
                else
                {
                    closeBraceLocation = CurrentLocation();
                }
                Advance();
            }
            else if (c == '"')
            {
                // Handle string literals
                content.Append(c);
                Advance();
                while (!IsAtEnd() && Peek() != '"')
                {
                    if (Peek() == '\\' && _position + 1 < _content.Length)
                    {
                        content.Append(Peek());
                        Advance();
                    }
                    content.Append(Peek());
                    Advance();
                }
                if (Peek() == '"')
                {
                    content.Append(Peek());
                    Advance();
                }
            }
            else if (c == '\'')
            {
                // Handle char literals
                content.Append(c);
                Advance();
                while (!IsAtEnd() && Peek() != '\'')
                {
                    if (Peek() == '\\' && _position + 1 < _content.Length)
                    {
                        content.Append(Peek());
                        Advance();
                    }
                    content.Append(Peek());
                    Advance();
                }
                if (Peek() == '\'')
                {
                    content.Append(Peek());
                    Advance();
                }
            }
            else
            {
                content.Append(c);
                Advance();
            }
        }

        if (braceDepth > 0)
        {
            _document.AddError("TUI008", "Unclosed @code block - missing '}'", openBraceLocation);
        }

        var codeContent = content.ToString().Trim();
        _document.CodeBlock = new TuiCodeBlock(codeContent, startLocation, openBraceLocation, closeBraceLocation);

        if (_document.CodeBlock.IsEmpty)
        {
            _document.AddWarning("TUI009", "Empty @code block can be removed", startLocation);
        }
    }

    private void ParseMarkup()
    {
        while (!IsAtEnd())
        {
            SkipWhitespace();
            if (IsAtEnd()) break;

            // Check for @code block at end
            if (Peek() == '@')
            {
                var pos = _position;
                Advance();
                if (Match("code"))
                {
                    _position = pos;
                    _column -= 5;
                    ParseDirectives(); // Will parse @code
                    continue;
                }
                else if (Match("if") || Match("foreach") || Match("switch"))
                {
                    // Control flow - rewind and parse as control flow
                    _position = pos;
                    _column -= (_position - pos);
                    var node = ParseControlFlow();
                    if (node != null)
                    {
                        _document.RootMarkup.Add(node);
                    }
                    continue;
                }
                // Back up
                _position = pos;
                _column--;
            }

            if (Peek() == '<')
            {
                var node = ParseElement();
                if (node != null)
                {
                    _document.RootMarkup.Add(node);
                }
            }
            else if (!IsAtEnd())
            {
                // Unexpected content
                var location = CurrentLocation();
                var content = ReadToNextTag();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    // Could be text content at root level - allowed for simple cases
                    _document.RootMarkup.Add(TuiMarkupNode.Text(content.Trim(), location));
                }
            }
        }
    }

    private TuiMarkupNode? ParseElement()
    {
        var startLocation = CurrentLocation();
        
        if (Peek() != '<')
        {
            return null;
        }
        Advance(); // consume <

        // Check for closing tag
        if (Peek() == '/')
        {
            // Unexpected closing tag at root
            Advance();
            var tagName = ReadIdentifier();
            _document.AddError("TUI007", $"Unexpected closing tag </{tagName}>", startLocation);
            SkipUntil('>');
            Advance();
            return null;
        }

        // Read tag name
        var elementName = ReadIdentifier();
        if (string.IsNullOrEmpty(elementName))
        {
            _document.AddError("TUI003", "Expected tag name after '<'", startLocation);
            SkipUntil('>');
            return null;
        }

        // Validate tag name
        if (!ValidTags.Contains(elementName))
        {
            _document.AddError("TUI003", $"Unknown tag '{elementName}'. Valid tags: Column, Row, Panel, Text, Button, Spacer, Centered, TextInput", startLocation);
        }

        var node = TuiMarkupNode.Element(elementName, startLocation);

        // Parse attributes
        ParseAttributes(node, elementName);

        SkipWhitespace();

        // Check for self-closing tag
        if (Peek() == '/')
        {
            Advance();
            if (Peek() == '>')
            {
                Advance();
                node.IsSelfClosing = true;
                return node;
            }
        }

        // Expect >
        if (Peek() != '>')
        {
            _document.AddError("TUI006", $"Expected '>' to close tag <{elementName}>", CurrentLocation());
            return node;
        }
        Advance(); // consume >

        // Parse children if container tag
        if (ContainerTags.Contains(elementName))
        {
            ParseChildren(node, elementName);
        }
        else
        {
            // Leaf tags can have text content
            ParseTextContent(node, elementName);
        }

        return node;
    }

    private void ParseAttributes(TuiMarkupNode node, string tagName)
    {
        while (!IsAtEnd())
        {
            SkipWhitespace();
            
            var c = Peek();
            if (c == '>' || c == '/')
            {
                break;
            }

            var attrLocation = CurrentLocation();
            var attrName = ReadIdentifier();
            if (string.IsNullOrEmpty(attrName))
            {
                break;
            }

            // Validate attribute name
            if (ValidAttributes.TryGetValue(tagName, out var validAttrs) && !validAttrs.Contains(attrName))
            {
                _document.AddError("TUI004", $"Unknown attribute '{attrName}' on <{tagName}>", attrLocation);
            }

            SkipWhitespace();
            if (Peek() != '=')
            {
                // Boolean attribute (e.g., Bold without value means Bold="true")
                node.Attributes.Add(new TuiAttribute(attrName, TuiAttributeValue.Literal("true", true), attrLocation));
                continue;
            }
            Advance(); // consume =

            SkipWhitespace();
            var value = ParseAttributeValue();
            node.Attributes.Add(new TuiAttribute(attrName, value, attrLocation));
        }
    }

    private TuiAttributeValue ParseAttributeValue()
    {
        if (Peek() != '"')
        {
            // Unquoted value - read until whitespace or end
            var value = ReadUntilAny(new[] { ' ', '\t', '>', '/', '\r', '\n' });
            return TuiAttributeValue.Literal(value);
        }

        Advance(); // consume opening "

        var sb = new StringBuilder();
        var isExpression = false;
        var isLambda = false;
        var isMethodRef = false;
        var parenDepth = 0;

        while (!IsAtEnd() && (Peek() != '"' || parenDepth > 0))
        {
            var c = Peek();
            
            if (c == '@' && sb.Length == 0)
            {
                Advance();
                if (Peek() == '(')
                {
                    // Lambda: @(() => ...)
                    isLambda = true;
                    Advance();
                    parenDepth = 1;
                    while (!IsAtEnd() && parenDepth > 0)
                    {
                        c = Peek();
                        if (c == '(') parenDepth++;
                        else if (c == ')') parenDepth--;
                        
                        if (parenDepth > 0 || c != ')')
                        {
                            sb.Append(c);
                        }
                        Advance();
                    }
                }
                else
                {
                    // Could be method reference or simple expression
                    var expr = ReadUntilAny(new[] { '"', ' ', '\t', '>', '/' });
                    if (expr.Contains('.') || expr.Contains('['))
                    {
                        isExpression = true;
                        sb.Append(expr);
                    }
                    else
                    {
                        isMethodRef = true;
                        sb.Append(expr);
                    }
                }
            }
            else
            {
                sb.Append(c);
                Advance();
            }
        }

        if (Peek() == '"')
        {
            Advance(); // consume closing "
        }

        var rawValue = sb.ToString();

        if (isLambda)
        {
            return TuiAttributeValue.Lambda(rawValue);
        }
        if (isMethodRef)
        {
            return TuiAttributeValue.MethodReference(rawValue);
        }
        if (isExpression)
        {
            return TuiAttributeValue.Expression(rawValue);
        }

        // Try to parse as literal
        return ParseLiteralValue(rawValue);
    }

    private TuiAttributeValue ParseLiteralValue(string rawValue)
    {
        // Try bool
        if (bool.TryParse(rawValue, out var boolVal))
        {
            return TuiAttributeValue.Literal(rawValue, boolVal);
        }

        // Try int
        if (int.TryParse(rawValue, out var intVal))
        {
            return TuiAttributeValue.Literal(rawValue, intVal);
        }

        // String value
        return TuiAttributeValue.Literal(rawValue, rawValue);
    }

    private void ParseChildren(TuiMarkupNode parent, string parentTagName)
    {
        while (!IsAtEnd())
        {
            SkipWhitespace();
            if (IsAtEnd()) break;

            // Check for closing tag
            if (Peek() == '<')
            {
                var pos = _position;
                Advance();
                if (Peek() == '/')
                {
                    Advance();
                    var closingTag = ReadIdentifier();
                    SkipWhitespace();
                    if (Peek() == '>')
                    {
                        Advance();
                        if (closingTag != parentTagName)
                        {
                            _document.AddError("TUI007", $"Unexpected closing tag </{closingTag}>. Expected </{parentTagName}>", CurrentLocation());
                        }
                        return;
                    }
                }
                // Not a closing tag, back up
                _position = pos;
                _column -= (_position - pos);
            }

            // Check for control flow
            if (Peek() == '@')
            {
                var pos = _position;
                var loc = CurrentLocation();
                Advance();
                
                if (MatchPeek("if") || MatchPeek("foreach") || MatchPeek("switch"))
                {
                    _position = pos;
                    _column--;
                    var controlNode = ParseControlFlow();
                    if (controlNode != null)
                    {
                        parent.Children.Add(controlNode);
                    }
                    continue;
                }
                
                // Expression in content
                var expr = ReadUntilAny(new[] { '<', '@', '\r', '\n' });
                if (!string.IsNullOrEmpty(expr))
                {
                    parent.Children.Add(TuiMarkupNode.Expression(expr.Trim(), loc));
                }
                continue;
            }

            // Check for element
            if (Peek() == '<')
            {
                var child = ParseElement();
                if (child != null)
                {
                    parent.Children.Add(child);
                }
            }
            else
            {
                // Text content
                var textLocation = CurrentLocation();
                var text = ReadTextContent();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Check for embedded expressions
                    var parts = ParseTextWithExpressions(text, textLocation);
                    parent.Children.AddRange(parts);
                }
            }
        }

        // Unclosed tag
        _document.AddError("TUI006", $"Unclosed tag <{parentTagName}>", parent.Location);
    }

    private List<TuiMarkupNode> ParseTextWithExpressions(string text, SourceLocation baseLocation)
    {
        var result = new List<TuiMarkupNode>();
        var remaining = text;
        var currentCol = baseLocation.Column;

        while (!string.IsNullOrEmpty(remaining))
        {
            var atIndex = remaining.IndexOf('@');
            if (atIndex < 0)
            {
                // No more expressions
                if (!string.IsNullOrWhiteSpace(remaining))
                {
                    result.Add(TuiMarkupNode.Text(remaining, new SourceLocation(baseLocation.FilePath, baseLocation.Line, currentCol)));
                }
                break;
            }

            // Check for @@
            if (atIndex + 1 < remaining.Length && remaining[atIndex + 1] == '@')
            {
                // Escaped @
                var beforeAt = remaining.Substring(0, atIndex + 1);
                result.Add(TuiMarkupNode.Text(beforeAt, new SourceLocation(baseLocation.FilePath, baseLocation.Line, currentCol)));
                currentCol += atIndex + 2;
                remaining = remaining.Substring(atIndex + 2);
                continue;
            }

            // Text before @
            if (atIndex > 0)
            {
                var beforeExpr = remaining.Substring(0, atIndex);
                if (!string.IsNullOrWhiteSpace(beforeExpr))
                {
                    result.Add(TuiMarkupNode.Text(beforeExpr, new SourceLocation(baseLocation.FilePath, baseLocation.Line, currentCol)));
                }
            }

            currentCol += atIndex + 1;
            remaining = remaining.Substring(atIndex + 1);

            // Parse expression
            var exprEnd = FindExpressionEnd(remaining);
            var expr = remaining.Substring(0, exprEnd);
            result.Add(TuiMarkupNode.Expression(expr, new SourceLocation(baseLocation.FilePath, baseLocation.Line, currentCol)));
            
            currentCol += exprEnd;
            remaining = remaining.Substring(exprEnd);
        }

        return result;
    }

    private int FindExpressionEnd(string text)
    {
        var i = 0;
        var parenDepth = 0;
        var bracketDepth = 0;

        while (i < text.Length)
        {
            var c = text[i];
            
            if (c == '(') parenDepth++;
            else if (c == ')') { parenDepth--; if (parenDepth < 0) break; }
            else if (c == '[') bracketDepth++;
            else if (c == ']') { bracketDepth--; if (bracketDepth < 0) break; }
            else if (parenDepth == 0 && bracketDepth == 0)
            {
                // Check for expression terminators
                if (char.IsWhiteSpace(c) || c == '<' || c == '@')
                {
                    break;
                }
            }
            i++;
        }

        return i;
    }

    private void ParseTextContent(TuiMarkupNode parent, string parentTagName)
    {
        var textLocation = CurrentLocation();
        var sb = new StringBuilder();

        while (!IsAtEnd())
        {
            if (Peek() == '<')
            {
                var pos = _position;
                Advance();
                if (Peek() == '/')
                {
                    Advance();
                    var closingTag = ReadIdentifier();
                    SkipWhitespace();
                    if (Peek() == '>')
                    {
                        Advance();
                        if (closingTag != parentTagName)
                        {
                            _document.AddError("TUI007", $"Unexpected closing tag </{closingTag}>. Expected </{parentTagName}>", CurrentLocation());
                        }
                        break;
                    }
                }
                // Not a closing tag, treat as content error
                _position = pos;
                _column--;
                sb.Append('<');
                Advance();
                continue;
            }

            sb.Append(Peek());
            Advance();
        }

        var text = sb.ToString().Trim();
        if (!string.IsNullOrEmpty(text))
        {
            var parts = ParseTextWithExpressions(text, textLocation);
            parent.Children.AddRange(parts);
        }
    }

    private TuiMarkupNode? ParseControlFlow()
    {
        var location = CurrentLocation();
        
        if (Peek() != '@')
        {
            return null;
        }
        Advance();

        if (Match("if"))
        {
            return ParseIfStatement(location);
        }
        if (Match("foreach"))
        {
            return ParseForeachStatement(location);
        }
        if (Match("switch"))
        {
            return ParseSwitchStatement(location);
        }

        return null;
    }

    private TuiMarkupNode ParseIfStatement(SourceLocation location)
    {
        SkipWhitespace();
        
        // Parse condition in parentheses
        if (Peek() != '(')
        {
            _document.AddError("TUI008", "Expected '(' after @if", CurrentLocation());
            return TuiMarkupNode.ControlFlow("if", "", location);
        }

        var condition = ReadBalancedParentheses();
        var node = TuiMarkupNode.ControlFlow("if", condition, location);

        SkipWhitespace();

        // Parse body in braces
        if (Peek() != '{')
        {
            _document.AddError("TUI008", "Expected '{' after @if condition", CurrentLocation());
            return node;
        }

        ParseControlFlowBody(node);

        // Check for else/else if
        SkipWhitespace();
        while (!IsAtEnd())
        {
            var pos = _position;
            if (Match("else"))
            {
                SkipWhitespace();
                if (Match("if"))
                {
                    // else if
                    SkipWhitespace();
                    if (Peek() != '(')
                    {
                        _document.AddError("TUI008", "Expected '(' after else if", CurrentLocation());
                        break;
                    }
                    var elseIfCondition = ReadBalancedParentheses();
                    var elseIfNode = TuiMarkupNode.ControlFlow("else if", elseIfCondition, CurrentLocation());
                    SkipWhitespace();
                    if (Peek() == '{')
                    {
                        ParseControlFlowBody(elseIfNode);
                    }
                    node.AlternateChildren ??= new();
                    node.AlternateChildren.Add(elseIfNode);
                }
                else
                {
                    // else
                    var elseNode = TuiMarkupNode.ControlFlow("else", "", CurrentLocation());
                    SkipWhitespace();
                    if (Peek() == '{')
                    {
                        ParseControlFlowBody(elseNode);
                    }
                    node.AlternateChildren ??= new();
                    node.AlternateChildren.Add(elseNode);
                    break;
                }
            }
            else
            {
                _position = pos;
                break;
            }
            SkipWhitespace();
        }

        return node;
    }

    private TuiMarkupNode ParseForeachStatement(SourceLocation location)
    {
        SkipWhitespace();
        
        if (Peek() != '(')
        {
            _document.AddError("TUI008", "Expected '(' after @foreach", CurrentLocation());
            return TuiMarkupNode.ControlFlow("foreach", "", location);
        }

        var expression = ReadBalancedParentheses();
        var node = TuiMarkupNode.ControlFlow("foreach", expression, location);

        SkipWhitespace();

        if (Peek() != '{')
        {
            _document.AddError("TUI008", "Expected '{' after @foreach expression", CurrentLocation());
            return node;
        }

        ParseControlFlowBody(node);
        return node;
    }

    private TuiMarkupNode ParseSwitchStatement(SourceLocation location)
    {
        SkipWhitespace();
        
        if (Peek() != '(')
        {
            _document.AddError("TUI008", "Expected '(' after @switch", CurrentLocation());
            return TuiMarkupNode.ControlFlow("switch", "", location);
        }

        var expression = ReadBalancedParentheses();
        var node = TuiMarkupNode.ControlFlow("switch", expression, location);

        SkipWhitespace();

        if (Peek() != '{')
        {
            _document.AddError("TUI008", "Expected '{' after @switch expression", CurrentLocation());
            return node;
        }

        // Parse switch body
        Advance(); // consume {
        var braceDepth = 1;

        while (!IsAtEnd() && braceDepth > 0)
        {
            SkipWhitespace();
            
            if (Match("case"))
            {
                SkipWhitespace();
                var caseValue = ReadUntil(':');
                Advance(); // consume :
                
                var caseNode = TuiMarkupNode.ControlFlow("case", caseValue.Trim(), CurrentLocation());
                caseNode.CaseValue = caseValue.Trim();
                
                // Parse case content until break or next case/default
                ParseCaseContent(caseNode);
                node.Children.Add(caseNode);
            }
            else if (Match("default"))
            {
                SkipWhitespace();
                if (Peek() == ':') Advance();
                
                var defaultNode = TuiMarkupNode.ControlFlow("default", "", CurrentLocation());
                ParseCaseContent(defaultNode);
                node.Children.Add(defaultNode);
            }
            else if (Peek() == '}')
            {
                braceDepth--;
                Advance();
            }
            else
            {
                Advance();
            }
        }

        return node;
    }

    private void ParseCaseContent(TuiMarkupNode caseNode)
    {
        while (!IsAtEnd())
        {
            SkipWhitespace();
            
            if (MatchPeek("case") || MatchPeek("default") || Peek() == '}')
            {
                break;
            }
            
            if (Match("break"))
            {
                SkipWhitespace();
                if (Peek() == ';') Advance();
                break;
            }

            if (Peek() == '<')
            {
                var child = ParseElement();
                if (child != null)
                {
                    caseNode.Children.Add(child);
                }
            }
            else
            {
                Advance();
            }
        }
    }

    private void ParseControlFlowBody(TuiMarkupNode node)
    {
        if (Peek() != '{')
        {
            return;
        }

        Advance(); // consume {
        var braceDepth = 1;

        while (!IsAtEnd() && braceDepth > 0)
        {
            SkipWhitespace();

            if (Peek() == '{')
            {
                braceDepth++;
                Advance();
            }
            else if (Peek() == '}')
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    Advance();
                    break;
                }
                Advance();
            }
            else if (Peek() == '<')
            {
                var child = ParseElement();
                if (child != null)
                {
                    node.Children.Add(child);
                }
            }
            else if (Peek() == '@')
            {
                var pos = _position;
                Advance();
                if (MatchPeek("if") || MatchPeek("foreach") || MatchPeek("switch"))
                {
                    _position = pos;
                    _column--;
                    var controlChild = ParseControlFlow();
                    if (controlChild != null)
                    {
                        node.Children.Add(controlChild);
                    }
                }
                else
                {
                    // Expression
                    var expr = ReadUntilAny(new[] { '<', '@', '\r', '\n', '}' });
                    if (!string.IsNullOrEmpty(expr))
                    {
                        node.Children.Add(TuiMarkupNode.Expression(expr.Trim(), CurrentLocation()));
                    }
                }
            }
            else
            {
                // Text content
                var textLoc = CurrentLocation();
                var text = ReadTextUntilControlOrBrace();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var parts = ParseTextWithExpressions(text.Trim(), textLoc);
                    node.Children.AddRange(parts);
                }
            }
        }
    }

    private void ValidateDocument()
    {
        if (_document.ViewDirective == null)
        {
            _document.AddError("TUI001", ".tui file must start with @view directive", 
                new SourceLocation(_filePath, 1, 1));
        }

        if (_document.RootMarkup.Count == 0 && !_document.HasErrors)
        {
            _document.AddError("TUI001", ".tui file must contain at least one root element", 
                new SourceLocation(_filePath, 1, 1));
        }
    }

    #region Helper Methods

    private bool IsAtEnd() => _position >= _content.Length;

    private char Peek() => IsAtEnd() ? '\0' : _content[_position];

    private char PeekNext() => _position + 1 >= _content.Length ? '\0' : _content[_position + 1];

    private void Advance()
    {
        if (IsAtEnd()) return;
        
        if (_content[_position] == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        _position++;
    }

    private bool Match(string expected)
    {
        if (_position + expected.Length > _content.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (_content[_position + i] != expected[i])
            {
                return false;
            }
        }

        for (var i = 0; i < expected.Length; i++)
        {
            Advance();
        }
        return true;
    }

    private bool MatchPeek(string expected)
    {
        if (_position + expected.Length > _content.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (_content[_position + i] != expected[i])
            {
                return false;
            }
        }
        return true;
    }

    private SourceLocation CurrentLocation() => new(_filePath, _line, _column);

    private void SkipWhitespace()
    {
        while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
        {
            Advance();
        }
    }

    private void SkipToEndOfLine()
    {
        while (!IsAtEnd() && Peek() != '\n' && Peek() != '\r')
        {
            Advance();
        }
        // Skip newline
        if (Peek() == '\r') Advance();
        if (Peek() == '\n') Advance();
    }

    private string ReadToEndOfLine()
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && Peek() != '\n' && Peek() != '\r')
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private string ReadIdentifier()
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private void SkipUntil(char c)
    {
        while (!IsAtEnd() && Peek() != c)
        {
            Advance();
        }
    }

    private string ReadUntil(char c)
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && Peek() != c)
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private string ReadUntilAny(char[] chars)
    {
        var sb = new StringBuilder();
        var set = new HashSet<char>(chars);
        while (!IsAtEnd() && !set.Contains(Peek()))
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private string ReadToNextTag()
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && Peek() != '<' && Peek() != '@')
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private string ReadTextContent()
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && Peek() != '<')
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private string ReadTextUntilControlOrBrace()
    {
        var sb = new StringBuilder();
        while (!IsAtEnd() && Peek() != '<' && Peek() != '@' && Peek() != '{' && Peek() != '}')
        {
            sb.Append(Peek());
            Advance();
        }
        return sb.ToString();
    }

    private string ReadBalancedParentheses()
    {
        if (Peek() != '(')
        {
            return string.Empty;
        }

        Advance(); // consume (
        var sb = new StringBuilder();
        var depth = 1;

        while (!IsAtEnd() && depth > 0)
        {
            var c = Peek();
            if (c == '(') depth++;
            else if (c == ')') depth--;

            if (depth > 0)
            {
                sb.Append(c);
            }
            Advance();
        }

        return sb.ToString();
    }

    #endregion
}

