namespace Dassie.CodeAnalysis.Structure;

/// <summary>
/// Represents a type inside a Dassie code structure.
/// </summary>
public class Type
{
    /// <summary>
    /// Represents a kind of type.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        /// Represents a reference type.
        /// </summary>
        RefType,
        /// <summary>
        /// Represents a value type.
        /// </summary>
        ValType,
        /// <summary>
        /// Represents a module.
        /// </summary>
        Module,
        /// <summary>
        /// Represents a template type.
        /// </summary>
        Template,
        /// <summary>
        /// Represents an enumeration type.
        /// </summary>
        Enum
    }

    /// <summary>
    /// The name of the type, without the namespace.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The files containing the type definition.
    /// </summary>
    public string[] Files { get; set; }

    /// <summary>
    /// Specifies the kind of type.
    /// </summary>
    public Kind TypeKind { get; set; }
}