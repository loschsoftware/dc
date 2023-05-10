using System;

namespace LoschScript.Errors;

/// <summary>
/// Specifies the type of a LoschScript error.
/// </summary>
public enum ErrorKind
{
    /// <summary>
    /// An unexpected and not further specified error.
    /// </summary>
    LS0000_UnexpectedError,
    /// <summary>
    /// An error that occured during the parsing stage of the compilation process that was emitted by ANTLR.
    /// </summary>
    LS0001_SyntaxError,
    /// <summary>
    /// Emitted when the specified argument list does not match any available overload of this function.
    /// </summary>
    LS0002_MethodNotFound,
    /// <summary>
    /// Emitted when the specified argument list does not match any available constructor for this type.
    /// </summary>
    LS0003_ConstructorNotFound,
    /// <summary>
    /// Emitted when an import directive is used on a non-existent namespace.
    /// </summary>
    LS0004_NamespaceNotFound,
    /// <summary>
    /// Emitted when an identifier contains invalid characters or is a language keyword.
    /// </summary>
    LS0005_InvalidIdentifier,
    /// <summary>
    /// Emitted when the type of the new value of a variable does not match the type of the old value.
    /// </summary>
    LS0006_VariableTypeChanged,
    /// <summary>
    /// Emitted when an access modifier is invalid for the specified object.
    /// </summary>
    LS0007_InvalidModifier,
    /// <summary>
    /// Emitted when a function contains no expressions.
    /// </summary>
    LS0008_NoExpressionInFunction,
    /// <summary>
    /// Emitted when a specified type was not found.
    /// </summary>
    LS0009_TypeNotFound,
    /// <summary>
    /// Emitted when the condition of a while loop is not a boolean.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the LoschScript compiler.")]
    LS0010_LoopConditionNotBoolean,
    /// <summary>
    /// 
    /// </summary>
    LS0011_UndefinedValue,
    /// <summary>
    /// 
    /// </summary>
    LS0012_InvalidArgument,
    /// <summary>
    /// Emitted when a conversion between two types is not implemented.
    /// </summary>
    LS0013_InvalidConversion,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the LoschScript compiler.")]
    LS0014_UnknownType,
    /// <summary>
    /// Emitted when a constant is being reassigned.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the LoschScript compiler.")]
    LS0015_ConstantReassignment,
    /// <summary>
    /// Emitted when a constant is declared without a value.
    /// </summary>
    LS0016_UnassignedConstant,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the LoschScript compiler.")]
    LS0017_DoubleDefinition,
    /// <summary>
    /// Emitted when a constant is being reassigned.
    /// </summary>
    LS0018_ImmutableValueReassignment,
    /// <summary>
    /// 
    /// </summary>
    LS0019_GenericValueTypeInvalid,
    /// <summary>
    /// Emitted when a feature of a newer LoschScript version is being used.
    /// </summary>
    LS0020_FeatureNotAvailable,
    /// <summary>
    /// 
    /// </summary>
    LS0021_PropertyNotFound,
    /// <summary>
    /// Emitted when an assembly reference could not be resolved.
    /// </summary>
    LS0022_InvalidAssemblyReference,
    /// <summary>
    /// Emitted when a file reference could not be resolved.
    /// </summary>
    LS0023_InvalidFileReference,
    /// <summary>
    /// 
    /// </summary>
    LS0024_ReadOnlyProperty,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the LoschScript compiler.")]
    LS0025_VariableAlreadyDefined,
    /// <summary>
    /// Emitted when an unknown escape sequence is being used.
    /// </summary>
    LS0026_InvalidEscapeSequence,
    /// <summary>
    /// Emitted when the program contains no executable code.
    /// </summary>
    LS0027_EmptyProgram,
    /// <summary>
    /// Emitted when a checked expression results in an integer overflow.
    /// </summary>
    LS0028_IntegerOverflow,
    /// <summary>
    /// Emitted when an IO error occurs, denying the LoschScript compiler access to the source files or output file.
    /// </summary>
    LS0029_FileAccessDenied,
    /// <summary>
    /// Emitted when a LoschScript code contains no suitable entry point.
    /// </summary>
    LS0030_NoEntryPoint,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the LoschScript compiler.")]
    LS0031_InvalidClassModifiers,
    /// <summary>
    /// Emitted when an argument violates a parameter constraint.
    /// </summary>
    LS0032_ConstraintViolation,
    /// <summary>
    /// Emitted when implicitly converting between two types is not possible.
    /// </summary>
    LS0033_ImplicitConversionNotPossible,
    /// <summary>
    /// Emitted when explicitly converting between two types is not possible.
    /// </summary>
    LS0034_ExplicitConversionNotPossible,
    /// <summary>
    /// Emitted when the entry point of a LoschScript program is not a static function.
    /// </summary>
    LS0035_EntryPointNotStatic,
    /// <summary>
    /// General error message emitted for various kinds of arithmetic errors.
    /// </summary>
    LS0036_ArithmeticError,
    /// <summary>
    /// Emitted when the return types of branches do not match.
    /// </summary>
    LS0037_BranchExpressionTypesUnequal,
    /// <summary>
    /// Emitted when the condition of a conditional expression is not a boolean.
    /// </summary>
    LS0038_ConditionalExpressionClauseNotBoolean,
    /// <summary>
    /// Emitted when a type does not include a specified field.
    /// </summary>
    LS0039_FieldNotFound
}