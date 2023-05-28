namespace LoschScript.Text;

/// <summary>
/// A list of color types commonly used by an editor. In LSEdit, these correspond to the colors of the current editor theme.
/// </summary>
public enum Color : byte
{
    /// <summary>
    /// The default color.
    /// </summary>
    Default,
    /// <summary>
    /// The color of a string literal.
    /// </summary>
    StringLiteral,
    /// <summary>
    /// The color of an integer or real literal.
    /// </summary>
    NumericLiteral,
    /// <summary>
    /// The color of a keyword.
    /// </summary>
    Word,
    /// <summary>
    /// The color of a control flow keyword or operator.
    /// </summary>
    ControlFlow,
    /// <summary>
    /// The color of a reference type.
    /// </summary>
    ReferenceType,
    /// <summary>
    /// The color of a value type.
    /// </summary>
    ValueType,
    /// <summary>
    /// The color of a template type.
    /// </summary>
    TemplateType,
    /// <summary>
    /// The color of a module.
    /// </summary>
    Module,
    /// <summary>
    /// The color of a namespace.
    /// </summary>
    Namespace,
    /// <summary>
    /// The color of a function.
    /// </summary>
    Function,
    /// <summary>
    /// The color of a local immutable value.
    /// </summary>
    LocalValue,
    /// <summary>
    /// The color of a local variable.
    /// </summary>
    LocalVariable,
    /// <summary>
    /// The color of a field.
    /// </summary>
    Field,
    /// <summary>
    /// The color of a property.
    /// </summary>
    Property,
    /// <summary>
    /// The color of an overloaded operator.
    /// </summary>
    OverloadedOperator,
    /// <summary>
    /// The color of a built-in operator.
    /// </summary>
    BuiltinOperator,
    /// <summary>
    /// The color of a type parameter.
    /// </summary>
    TypeParameter,
    /// <summary>
    /// The color of a primitive type.
    /// </summary>
    PrimitiveType,
    /// <summary>
    /// The color of parentheses at nesting depth 1.
    /// </summary>
    ParenLevel1,
    /// <summary>
    /// The color of parentheses at nesting depth 2.
    /// </summary>
    ParenLevel2,
    /// <summary>
    /// The color of parentheses at nesting depth 3.
    /// </summary>
    ParenLevel3,
    /// <summary>
    /// The color of parentheses at nesting depth 4.
    /// </summary>
    ParenLevel4,
    /// <summary>
    /// The color of parentheses at nesting depth 5. At depth 6, the color will restart from <see cref="ParenLevel1"/>.
    /// </summary>
    ParenLevel5
}