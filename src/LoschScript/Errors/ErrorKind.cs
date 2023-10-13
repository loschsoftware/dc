using System;

namespace LoschScript.Errors;

/// <summary>
/// Specifies the type of a LoschScript error.
/// </summary>
public enum ErrorKind
{
    /// <summary>
    /// Represents a custom error message emitted by tools such as code analyzers.
    /// </summary>
    CustomError = -1,
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
    LS0039_FieldNotFound,
    /// <summary>
    /// Emitted when the specified name is not assigned to a value.
    /// </summary>
    LS0040_VariableNotFound,
    /// <summary>
    /// Emitted when the types of list or array items are not the same.
    /// </summary>
    LS0041_ListItemsHaveDifferentTypes,
    /// <summary>
    /// Emitted when the index expression of an array element assignment expression is not an integer.
    /// </summary>
    LS0042_ArrayElementAssignmentIndexExpressionNotInteger,
    /// <summary>
    /// Emitted when the compiler thinks an infinite loop could be unintentional.
    /// </summary>
    LS0043_PossiblyUnintentionalInfiniteLoop,
    /// <summary>
    /// Emitted when an update to LSC is available. This is an information message, not an error.
    /// </summary>
    LS0044_LscUpdateAvailable,
    /// <summary>
    /// Emitted when an inline IL instruction has an invalid opcode.
    /// </summary>
    LS0045_InlineILInvalidOpCode,
    /// <summary>
    /// Emitted when an inline IL instruction has an invalid operand.
    /// </summary>
    LS0046_InlineILInvalidOperand,
    /// <summary>
    /// Emitted when there are duplicate types in a union type.
    /// </summary>
    LS0047_UnionTypeDuplicate,
    /// <summary>
    /// Emitted when a specified source file could not be found.
    /// </summary>
    LS0048_SourceFileNotFound,
    /// <summary>
    /// Emitted when a for loop contains more than 3 expressions.
    /// </summary>
    LS0049_InvalidForLoopSyntax,
    /// <summary>
    /// Emitted when the return value of a top-level program is not an integer.
    /// </summary>
    LS0050_ExpectedIntegerReturnValue,
    /// <summary>
    /// Emitted when the inheritance list of a type contains more than one CLR class.
    /// </summary>
    LS0051_MoreThanOneClassInInheritanceList,
    /// <summary>
    /// Emitted when a type member has an invalid access modifier.
    /// </summary>
    LS0052_InvalidAccessModifier,
    /// <summary>
    /// Emitted when the return value of a function does not match the function signature.
    /// </summary>
    LS0053_WrongReturnType,
    /// <summary>
    /// Emitted when a field is set to a value of the wrong type.
    /// </summary>
    LS0054_WrongFieldType,
    /// <summary>
    /// Emitted when multiple functions are declared as application entry points.
    /// </summary>
    LS0055_MultipleEntryPoints,
    /// <summary>
    /// Generic error emitted when a symbol of various kinds could not be resolved.
    /// </summary>
    LS0056_SymbolResolveError,
    /// <summary>
    /// Emitted when a variable is assigned an expression of an incompatible type.
    /// </summary>
    LS0057_IncompatibleType,
    /// <summary>
    /// Message emitted when a type or member is declared with a redundant access modifier.
    /// </summary>
    LS0058_RedundantModifier,
    /// <summary>
    /// Emitted when a rethrow expression is used outside of a catch block.
    /// </summary>
    LS0059_RethrowOutsideCatchBlock,
    /// <summary>
    /// Emitted when the expression to be thrown is not a reference type.
    /// </summary>
    LS0060_InvalidThrowExpression,
    /// <summary>
    /// Emitted when a try block is not followed by a catch block.
    /// </summary>
    LS0061_MissingCatchBranch,
    /// <summary>
    /// Emitted when a local is not in scope.
    /// </summary>
    LS0062_LocalOutsideScope,
    /// <summary>
    /// Emitted when a feature that is not yet implemented is used.
    /// </summary>
    LS0063_UnsupportedFeature,
    /// <summary>
    /// Emitted when something that shouldn't is used as an expression.
    /// </summary>
    LS0064_InvalidExpression,
    /// <summary>
    /// Emitted when the left side of an assignment is invalid.
    /// </summary>
    LS0065_AssignmentInvalidLeftSide,
    /// <summary>
    /// Emitted when a property cannot be assigned to.
    /// </summary>
    LS0066_PropertyNoSuitableSetter,
    /// <summary>
    /// Emitted when a resource specified in lsconfig.xml could not be located.
    /// </summary>
    LS0067_ResourceFileNotFound,
    /// <summary>
    /// Emitted when multiple unmanaged resources are specified in lsconfig.xml.
    /// </summary>
    LS0068_MultipleUnmanagedResources,
    /// <summary>
    /// Emitted when a required Windows SDK tool could not be located.
    /// </summary>
    LS0069_WinSdkToolNotFound,
    /// <summary>
    /// Emitted when the <code>&lt;VersionInfo&gt;</code> tag is used in lsconfig.xml.
    /// </summary>
    LS0070_AvoidVersionInfoTag,
    /// <summary>
    /// Emitted when an ingored message is specified in lsconfig.xml that cannot be ignored.
    /// </summary>
    LS0071_IllegalIgnoredMessage,
    /// <summary>
    /// Emitted when 'lsc build' is called but there are no source files.
    /// </summary>
    LS0072_NoSourceFilesFound,
    /// <summary>
    /// Emitted when the name of a type exceeds the .NET limit of 1024 characters.
    /// </summary>
    LS0073_TypeNameTooLong,
    /// <summary>
    /// Emitted when more than 65534 locals are defined in a function.
    /// </summary>
    LS0074_TooManyLocals,
    /// <summary>
    /// Emitted when an integer literal is too big for its type.
    /// </summary>
    LS0075_Overflow,
    /// <summary>
    /// Emitted when a character literal contains no character.
    /// </summary>
    LS0076_EmptyCharacterLiteral,
    /// <summary>
    /// Emitted when an import directive is invalid.
    /// </summary>
    LS0077_InvalidImport
}