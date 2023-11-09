using System;

namespace Dassie.Errors;

/// <summary>
/// Specifies the type of a Dassie error.
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
    DS0000_UnexpectedError,
    /// <summary>
    /// An error that occured during the parsing stage of the compilation process that was emitted by ANTLR.
    /// </summary>
    DS0001_SyntaxError,
    /// <summary>
    /// Emitted when the specified argument list does not match any available overload of this function.
    /// </summary>
    DS0002_MethodNotFound,
    /// <summary>
    /// Emitted when the specified argument list does not match any available constructor for this type.
    /// </summary>
    DS0003_ConstructorNotFound,
    /// <summary>
    /// Emitted when an import directive is used on a non-existent namespace.
    /// </summary>
    DS0004_NamespaceNotFound,
    /// <summary>
    /// Emitted when an identifier contains invalid characters or is a language keyword.
    /// </summary>
    DS0005_InvalidIdentifier,
    /// <summary>
    /// Emitted when the type of the new value of a variable does not match the type of the old value.
    /// </summary>
    DS0006_VariableTypeChanged,
    /// <summary>
    /// Emitted when an access modifier is invalid for the specified object.
    /// </summary>
    DS0007_InvalidModifier,
    /// <summary>
    /// Emitted when a function contains no expressions.
    /// </summary>
    DS0008_NoExpressionInFunction,
    /// <summary>
    /// Emitted when a specified type was not found.
    /// </summary>
    DS0009_TypeNotFound,
    /// <summary>
    /// Emitted when the condition of a while loop is not a boolean.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0010_LoopConditionNotBoolean,
    /// <summary>
    /// 
    /// </summary>
    DS0011_UndefinedValue,
    /// <summary>
    /// 
    /// </summary>
    DS0012_InvalidArgument,
    /// <summary>
    /// Emitted when a conversion between two types is not implemented.
    /// </summary>
    DS0013_InvalidConversion,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0014_UnknownType,
    /// <summary>
    /// Emitted when a constant is being reassigned.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0015_ConstantReassignment,
    /// <summary>
    /// Emitted when a constant is declared without a value.
    /// </summary>
    DS0016_UnassignedConstant,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0017_DoubleDefinition,
    /// <summary>
    /// Emitted when a constant is being reassigned.
    /// </summary>
    DS0018_ImmutableValueReassignment,
    /// <summary>
    /// 
    /// </summary>
    DS0019_GenericValueTypeInvalid,
    /// <summary>
    /// Emitted when a feature of a newer Dassie version is being used.
    /// </summary>
    DS0020_FeatureNotAvailable,
    /// <summary>
    /// 
    /// </summary>
    DS0021_PropertyNotFound,
    /// <summary>
    /// Emitted when an assembly reference could not be resolved.
    /// </summary>
    DS0022_InvalidAssemblyReference,
    /// <summary>
    /// Emitted when a file reference could not be resolved.
    /// </summary>
    DS0023_InvalidFileReference,
    /// <summary>
    /// 
    /// </summary>
    DS0024_ReadOnlyProperty,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0025_VariableAlreadyDefined,
    /// <summary>
    /// Emitted when an unknown escape sequence is being used.
    /// </summary>
    DS0026_InvalidEscapeSequence,
    /// <summary>
    /// Emitted when the program contains no executable code.
    /// </summary>
    DS0027_EmptyProgram,
    /// <summary>
    /// Emitted when a checked expression results in an integer overflow.
    /// </summary>
    DS0028_IntegerOverflow,
    /// <summary>
    /// Emitted when an IO error occurs, denying the Dassie compiler access to the source files or output file.
    /// </summary>
    DS0029_FileAccessDenied,
    /// <summary>
    /// Emitted when a Dassie code contains no suitable entry point.
    /// </summary>
    DS0030_NoEntryPoint,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0031_InvalidClassModifiers,
    /// <summary>
    /// Emitted when an argument violates a parameter constraint.
    /// </summary>
    DS0032_ConstraintViolation,
    /// <summary>
    /// Emitted when implicitly converting between two types is not possible.
    /// </summary>
    DS0033_ImplicitConversionNotPossible,
    /// <summary>
    /// Emitted when explicitly converting between two types is not possible.
    /// </summary>
    DS0034_ExplicitConversionNotPossible,
    /// <summary>
    /// Emitted when the entry point of a Dassie program is not a static function.
    /// </summary>
    DS0035_EntryPointNotStatic,
    /// <summary>
    /// General error message emitted for various kinds of arithmetic errors.
    /// </summary>
    DS0036_ArithmeticError,
    /// <summary>
    /// Emitted when the return types of branches do not match.
    /// </summary>
    DS0037_BranchExpressionTypesUnequal,
    /// <summary>
    /// Emitted when the condition of a conditional expression is not a boolean.
    /// </summary>
    DS0038_ConditionalExpressionClauseNotBoolean,
    /// <summary>
    /// Emitted when a type does not include a specified field.
    /// </summary>
    DS0039_FieldNotFound,
    /// <summary>
    /// Emitted when the specified name is not assigned to a value.
    /// </summary>
    DS0040_VariableNotFound,
    /// <summary>
    /// Emitted when the types of list or array items are not the same.
    /// </summary>
    DS0041_ListItemsHaveDifferentTypes,
    /// <summary>
    /// Emitted when the index expression of an array element assignment expression is not an integer.
    /// </summary>
    DS0042_ArrayElementAssignmentIndexExpressionNotInteger,
    /// <summary>
    /// Emitted when the compiler thinks an infinite loop could be unintentional.
    /// </summary>
    DS0043_PossiblyUnintentionalInfiniteLoop,
    /// <summary>
    /// Emitted when an update to dc is available. This is an information message, not an error.
    /// </summary>
    DS0044_dcUpdateAvailable,
    /// <summary>
    /// Emitted when an inline IL instruction has an invalid opcode.
    /// </summary>
    DS0045_InlineILInvalidOpCode,
    /// <summary>
    /// Emitted when an inline IL instruction has an invalid operand.
    /// </summary>
    DS0046_InlineILInvalidOperand,
    /// <summary>
    /// Emitted when there are duplicate types in a union type.
    /// </summary>
    DS0047_UnionTypeDuplicate,
    /// <summary>
    /// Emitted when a specified source file could not be found.
    /// </summary>
    DS0048_SourceFileNotFound,
    /// <summary>
    /// Emitted when a for loop contains more than 3 expressions.
    /// </summary>
    DS0049_InvalidForLoopSyntax,
    /// <summary>
    /// Emitted when the return value of a top-level program is not an integer.
    /// </summary>
    DS0050_ExpectedIntegerReturnValue,
    /// <summary>
    /// Emitted when the inheritance list of a type contains more than one CLR class.
    /// </summary>
    DS0051_MoreThanOneClassInInheritanceList,
    /// <summary>
    /// Emitted when a type member has an invalid access modifier.
    /// </summary>
    DS0052_InvalidAccessModifier,
    /// <summary>
    /// Emitted when the return value of a function does not match the function signature.
    /// </summary>
    DS0053_WrongReturnType,
    /// <summary>
    /// Emitted when a field is set to a value of the wrong type.
    /// </summary>
    DS0054_WrongFieldType,
    /// <summary>
    /// Emitted when multiple functions are declared as application entry points.
    /// </summary>
    DS0055_MultipleEntryPoints,
    /// <summary>
    /// Generic error emitted when a symbol of various kinds could not be resolved.
    /// </summary>
    DS0056_SymbolResolveError,
    /// <summary>
    /// Emitted when a variable is assigned an expression of an incompatible type.
    /// </summary>
    DS0057_IncompatibleType,
    /// <summary>
    /// Message emitted when a type or member is declared with a redundant access modifier.
    /// </summary>
    DS0058_RedundantModifier,
    /// <summary>
    /// Emitted when a rethrow expression is used outside of a catch block.
    /// </summary>
    DS0059_RethrowOutsideCatchBlock,
    /// <summary>
    /// Emitted when the expression to be thrown is not a reference type.
    /// </summary>
    DS0060_InvalidThrowExpression,
    /// <summary>
    /// Emitted when a try block is not followed by a catch block.
    /// </summary>
    DS0061_MissingCatchBranch,
    /// <summary>
    /// Emitted when a local is not in scope.
    /// </summary>
    DS0062_LocalOutsideScope,
    /// <summary>
    /// Emitted when a feature that is not yet implemented is used.
    /// </summary>
    DS0063_UnsupportedFeature,
    /// <summary>
    /// Emitted when something that shouldn't is used as an expression.
    /// </summary>
    DS0064_InvalidExpression,
    /// <summary>
    /// Emitted when the left side of an assignment is invalid.
    /// </summary>
    DS0065_AssignmentInvalidLeftSide,
    /// <summary>
    /// Emitted when a property cannot be assigned to.
    /// </summary>
    DS0066_PropertyNoSuitableSetter,
    /// <summary>
    /// Emitted when a resource specified in DassieConfig.xml could not be located.
    /// </summary>
    DS0067_ResourceFileNotFound,
    /// <summary>
    /// Emitted when multiple unmanaged resources are specified in DassieConfig.xml.
    /// </summary>
    DS0068_MultipleUnmanagedResources,
    /// <summary>
    /// Emitted when a required Windows SDK tool could not be located.
    /// </summary>
    DS0069_WinSdkToolNotFound,
    /// <summary>
    /// Emitted when the <code>&lt;VersionInfo&gt;</code> tag is used in DassieConfig.xml.
    /// </summary>
    DS0070_AvoidVersionInfoTag,
    /// <summary>
    /// Emitted when an ingored message is specified in DassieConfig.xml that cannot be ignored.
    /// </summary>
    DS0071_IllegalIgnoredMessage,
    /// <summary>
    /// Emitted when 'dc build' is called but there are no source files.
    /// </summary>
    DS0072_NoSourceFilesFound,
    /// <summary>
    /// Emitted when the name of a type exceeds the .NET limit of 1024 characters.
    /// </summary>
    DS0073_TypeNameTooLong,
    /// <summary>
    /// Emitted when more than 65534 locals are defined in a function.
    /// </summary>
    DS0074_TooManyLocals,
    /// <summary>
    /// Emitted when an integer literal is too big for its type.
    /// </summary>
    DS0075_Overflow,
    /// <summary>
    /// Emitted when a character literal contains no character.
    /// </summary>
    DS0076_EmptyCharacterLiteral,
    /// <summary>
    /// Emitted when an import directive is invalid.
    /// </summary>
    DS0077_InvalidImport,
    /// <summary>
    /// Emitted when the modifier of an access modifier group is the default member modifier of the containing type.
    /// </summary>
    DS0078_RedundantAccessModifierGroup,
    /// <summary>
    /// Emitted when an array has more than 32 dimensions.
    /// </summary>
    DS0079_ArrayTooManyDimensions,
    /// <summary>
    /// Emitted when an illegal identifier is used.
    /// </summary>
    DS0080_ReservedIdentifier,
    /// <summary>
    /// Emitted when a project reference is invalid.
    /// </summary>
    DS0081_InvalidProjectReference
}