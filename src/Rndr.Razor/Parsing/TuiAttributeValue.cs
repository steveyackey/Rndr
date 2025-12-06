namespace Rndr.Razor.Parsing;

/// <summary>
/// Represents the value of an attribute on a markup element.
/// </summary>
public sealed class TuiAttributeValue
{
    /// <summary>
    /// The type of value (literal, expression, lambda, method reference).
    /// </summary>
    public AttributeValueType ValueType { get; }

    /// <summary>
    /// The raw string value as written in the .tui file.
    /// </summary>
    public string RawValue { get; }

    /// <summary>
    /// Parsed value for literals (int, bool, string, enum).
    /// </summary>
    public object? ParsedValue { get; }

    /// <summary>
    /// Creates a new attribute value.
    /// </summary>
    public TuiAttributeValue(AttributeValueType valueType, string rawValue, object? parsedValue = null)
    {
        ValueType = valueType;
        RawValue = rawValue;
        ParsedValue = parsedValue;
    }

    /// <summary>
    /// Creates a literal attribute value.
    /// </summary>
    public static TuiAttributeValue Literal(string rawValue, object? parsedValue = null)
        => new(AttributeValueType.Literal, rawValue, parsedValue);

    /// <summary>
    /// Creates an expression attribute value (@variable).
    /// </summary>
    public static TuiAttributeValue Expression(string expression)
        => new(AttributeValueType.Expression, expression);

    /// <summary>
    /// Creates a lambda attribute value (@(() => expr) or @(x => expr)).
    /// </summary>
    public static TuiAttributeValue Lambda(string lambda)
        => new(AttributeValueType.Lambda, lambda);

    /// <summary>
    /// Creates a method reference attribute value (@MethodName).
    /// </summary>
    public static TuiAttributeValue MethodReference(string methodName)
        => new(AttributeValueType.MethodReference, methodName);
}

/// <summary>
/// Types of attribute values.
/// </summary>
public enum AttributeValueType
{
    /// <summary>Plain value like "10" or "Center".</summary>
    Literal,
    /// <summary>Razor expression like @count.Value.</summary>
    Expression,
    /// <summary>Lambda expression like @(() => count.Value++).</summary>
    Lambda,
    /// <summary>Method name like @Increment.</summary>
    MethodReference
}

