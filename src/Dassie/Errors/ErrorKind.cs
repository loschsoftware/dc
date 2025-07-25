using Dassie.Core;
using Dassie.Meta;
using System;
using System.Reflection.Emit;

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
    /// An unknown and not further specified error or an unhandled exception.
    /// </summary>
    DS0000_UnknownError,
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
    [Obsolete("This error code is not emitted by the compiler anymore.")]
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
    /// Emitted when a resource specified in dsconfig.xml could not be located.
    /// </summary>
    DS0067_ResourceFileNotFound,
    /// <summary>
    /// Emitted when multiple unmanaged resources are specified in dsconfig.xml.
    /// </summary>
    DS0068_MultipleUnmanagedResources,
    /// <summary>
    /// Emitted when a required Windows SDK tool could not be located.
    /// </summary>
    DS0069_WinSdkToolNotFound,
    /// <summary>
    /// Emitted when the <code>&lt;VersionInfo&gt;</code> tag is used in dsconfig.xml.
    /// </summary>
    DS0070_AvoidVersionInfoTag,
    /// <summary>
    /// Emitted when an ingored message is specified in dsconfig.xml that cannot be ignored.
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
    /// Emitted when more than 65535 locals are defined in a function.
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
    DS0081_InvalidProjectReference,
    /// <summary>
    /// Emitted when an invalid macro is used in dsconfig.xml.
    /// </summary>
    DS0082_InvalidDSConfigMacro,
    /// <summary>
    /// Emitted when the 'var' modifier is used on a method.
    /// </summary>
    DS0083_InvalidVarModifier,
    /// <summary>
    /// Emitted when the 'this' keyword is used in a static function.
    /// </summary>
    DS0084_ThisInStaticFunction,
    /// <summary>
    /// Emitted when a <see cref="TypeBuilder"/> could not be associated with any <see cref="TypeContext"/> object. Critical error that should never be emitted under normal circumstances.
    /// </summary>
    DS0085_TypeInfoCouldNotBeRead,
    /// <summary>
    /// Emitted when the command of a build profile or -event is invalid.
    /// </summary>
    DS0086_InvalidCommand,
    /// <summary>
    /// Emitted when a build or debug profile does not exist.
    /// </summary>
    DS0087_InvalidProfile,
    /// <summary>
    /// Emitted when a critical build event failed to run correctly.
    /// </summary>
    DS0088_BuildEventFailedToRun,
    /// <summary>
    /// Emitted when a dsconfig.xml document contains an invalid property.
    /// </summary>
    DS0089_InvalidDSConfigProperty,
    /// <summary>
    /// Emitted when some part of dsconfig.xml is malformed, either syntactically or semantically.
    /// </summary>
    DS0090_MalformedConfigurationFile,
    /// <summary>
    /// Emitted when the format of a dsconfig.xml file is newer than supported.
    /// </summary>
    DS0091_ConfigurationFormatVersionTooNew,
    /// <summary>
    /// Emitted when dsconfig.xml uses an outdated format.
    /// </summary>
    DS0092_ConfigurationFormatVersionTooOld,
    /// <summary>
    /// Emitted when a constructor returns a value, even though it is only allowed to return void.
    /// </summary>
    DS0093_ConstructorReturnsValue,
    /// <summary>
    /// Emitted when an init-only field is assigned to outside of a constructor.
    /// </summary>
    DS0094_InitOnlyFieldAssignedOutsideOfConstructor,
    /// <summary>
    /// Emitted when an immutable symbol is passed by reference.
    /// </summary>
    DS0095_ImmutableSymbolPassedByReference,
    /// <summary>
    /// Emitted when a symbol is passed by reference without the reference operator (&amp;).
    /// </summary>
    DS0096_PassByReferenceWithoutOperator,
    /// <summary>
    /// Emitted when an expression to be passed by reference is not valid.
    /// </summary>
    DS0097_InvalidExpressionPassedByReference,
    /// <summary>
    /// Emitted when a specified scratch could not be found.
    /// </summary>
    DS0098_ScratchNotFound,
    /// <summary>
    /// Emitted when multiple installed compiler extensions define a command with the same name.
    /// </summary>
    DS0099_DuplicateCompilerCommand,
    /// <summary>
    /// Emitted when dc is called with an invalid command name.
    /// </summary>
    DS0100_InvalidCommand,
    /// <summary>
    /// Diagnostic information message.
    /// </summary>
    DS0101_DiagnosticInfo,
    /// <summary>
    /// Message displayed when compiling ahead of time informing of performance penalty.
    /// </summary>
    DS0102_AotPerformanceWarning,
    /// <summary>
    /// Emitted when an operation that requires an internet connection cannot be completed.
    /// </summary>
    DS0103_NetworkError,
    /// <summary>
    /// Emitted when a package reference contains an invalid name or version number.
    /// </summary>
    DS0104_InvalidPackageReference,
    /// <summary>
    /// Emitted when the 'dc run' command is invoked with insufficient information.
    /// </summary>
    DS0105_DCRunInsufficientInfo,
    /// <summary>
    /// Emitted when no input source files are specified.
    /// </summary>
    DS0106_NoInputFiles,
    /// <summary>
    /// Emitted when a generic constraint is violated.
    /// </summary>
    DS0107_GenericTypeConstraintViolation,
    /// <summary>
    /// Emitted when a generic method is called without arguments.
    /// </summary>
    DS0108_GenericMethodCalledWithoutGenericArguments,
    /// <summary>
    /// Emitted when a local is defined with a name that already exists in a different scope.
    /// </summary>
    DS0109_LocalDefinedInDifferentScope,
    /// <summary>
    /// Emitted when a type parameter has multiple of the same attribute.
    /// </summary>
    DS0110_DuplicateTypeParameterAttributes,
    /// <summary>
    /// Emitted when a type parameter has multiple of the same type constraints.
    /// </summary>
    DS0111_DuplicateTypeParameterConstraint,
    /// <summary>
    /// Emitted when a type defines multiple type parameters with the same name.
    /// </summary>
    DS0112_DuplicateTypeParameter,
    /// <summary>
    /// Emitted when a type parameter defines an invalid set of attributes.
    /// </summary>
    DS0113_InvalidTypeParameterAttributes,
    /// <summary>
    /// Emitted when a generic method defines a type parameter that is already defined by the containing type.
    /// </summary>
    DS0114_TypeParameterIsDefinedInContainingScope,
    /// <summary>
    /// Emitted when a non-abstract method does not have a body.
    /// </summary>
    DS0115_NonAbstractMethodHasNoBody,
    /// <summary>
    /// Emitted when an abstract method has a body.
    /// </summary>
    DS0116_AbstractMethodHasBody,
    /// <summary>
    /// Emitted when a variance modifier is applied to a type parameter of a type other than a <c>template</c>.
    /// </summary>
    DS0117_VarianceModifierOnConcreteType,
    /// <summary>
    /// Emitted when variant type parameters are used in invalid ways.
    /// </summary>
    DS0118_InvalidVariance,
    /// <summary>
    /// Emitted when two types of the same name are defined.
    /// </summary>
    DS0119_DuplicateTypeName,
    /// <summary>
    /// Emitted when two types of the same name with different type parameters are defined.
    /// </summary>
    DS0120_DuplicateGenericTypeName,
    /// <summary>
    /// Emitted when a function pointer is created for a function with more than 16 arguments.
    /// </summary>
    DS0121_FunctionPointerTooManyArguments,
    /// <summary>
    /// Emitted when a function pointer is created for an invalid expression.
    /// </summary>
    DS0122_InvalidFunctionPointerTargetExpression,
    /// <summary>
    /// Emitted when an installed compiler extension is invalid.
    /// </summary>
    DS0123_InvalidExtensionPackage,
    /// <summary>
    /// Emitted when the 'dc run' command is called on a project that is not executable.
    /// </summary>
    DS0124_DCRunInvalidProjectType,
    /// <summary>
    /// Emitted when a value is ignored implicitly.
    /// </summary>
    DS0125_UnusedValue,
    /// <summary>
    /// Emitted when a non-nested type is declared as 'local'.
    /// </summary>
    DS0126_TopLevelTypeLocal,
    /// <summary>
    /// Emitted when a member cannot be accessed because of too restrictive access modifiers.
    /// </summary>
    DS0127_AccessModifiersTooRestrictive,
    /// <summary>
    /// Emitted when the 'dc deploy' command is run on a config file that does not represent a project group.
    /// </summary>
    DS0128_DeployCommandInvalidProjectGroupFile,
    /// <summary>
    /// Emitted when a project group contains no projects.
    /// </summary>
    DS0129_ProjectGroupNoComponents,
    /// <summary>
    /// Emitted when a project group defines no targets.
    /// </summary>
    DS0130_ProjectGroupNoTargets,
    /// <summary>
    /// Emitted when 'dc build' is used on a project group.
    /// </summary>
    DS0131_DCBuildCalledOnProjectGroup,
    /// <summary>
    /// Emitted when 'dc clean' is called but the working directory contains no compiler configuration file.
    /// </summary>
    DS0132_DCCleanNoProjectFile,
    /// <summary>
    /// Emitted when 'dc analyze' is called but the working directory contains no compiler configuration file.
    /// </summary>
    DS0133_DCAnalyzeNoProjectFile,
    /// <summary>
    /// Emitted when 'dc analyze' is called using an analyzer that could not be found.
    /// </summary>
    DS0134_DCAnalyzeInvalidAnalyzer,
    /// <summary>
    /// Emitted when attempting to divide by the constant value 0.
    /// </summary>
    DS0135_DivisionByConstantZero,
    /// <summary>
    /// Emitted when a conversion between two types is not possible.
    /// </summary>
    DS0136_InvalidConversion,
    /// <summary>
    /// Emitted when the 'literal' modifier is applied to a method.
    /// </summary>
    DS0137_LiteralModifierOnMethod,
    /// <summary>
    /// Emitted when a compile-time constant value is expected but was not provided.
    /// </summary>
    DS0138_CompileTimeConstantRequired,
    /// <summary>
    /// Emitted when trying to instantiate a module.
    /// </summary>
    DS0139_ModuleInstantiation,
    /// <summary>
    /// Emitted when an enumeration is not an integer type.
    /// </summary>
    DS0140_InvalidEnumerationType,
    /// <summary>
    /// Emitted when an enumeration type contains a method.
    /// </summary>
    DS0141_MethodInEnumeration,
    /// <summary>
    /// Emitted when an enumeration type is defined using the 'ref' modifier.
    /// </summary>
    DS0142_EnumTypeExplicitlyRef,
    /// <summary>
    /// Emitted when an enumeration type defines a base type other than System.Enum.
    /// </summary>
    DS0143_EnumTypeBaseType,
    /// <summary>
    /// Emitted when an enumeration type implements a template.
    /// </summary>
    DS0144_EnumTypeImplementsTemplate,
    /// <summary>
    /// Emitted when a type explicitly inherits from 'System.ValueType'.
    /// </summary>
    DS0145_ValueTypeInherited,
    /// <summary>
    /// Emitted when the inheritance list of a value type contains a reference type.
    /// </summary>
    DS0146_ValueTypeInheritsFromClass,
    /// <summary>
    /// Emitted when inheriting from a value type.
    /// </summary>
    DS0147_ValueTypeAsBaseType,
    /// <summary>
    /// Emitted when an immutable value has a byref type.
    /// </summary>
    DS0148_ImmutableValueOfByRefType,
    /// <summary>
    /// Emitted when a nested byref type (e.g. int&amp;&amp;) is used.
    /// </summary>
    DS0149_NestedByRefType,
    /// <summary>
    /// Emitted when a field of a byref or byref-like type is declared outside of a byref-like type.
    /// </summary>
    DS0150_ByRefFieldInNonByRefLikeType,
    /// <summary>
    /// Emitted when an immutable value type (val! type) contains a field explicitly marked as 'var'.
    /// </summary>
    DS0151_VarFieldInImmutableType,
    /// <summary>
    /// Emitted when a function pointer is created for an instance method.
    /// </summary>
    DS0152_FunctionPointerForInstanceMethod,
    /// <summary>
    /// Emitted when a list literal contains incompatible types.
    /// </summary>
    DS0153_ListLiteralDifferentTypes,
    /// <summary>
    /// Emitted when an incompatible expression is appended to a list.
    /// </summary>
    DS0154_ListAppendIncompatibleElement,
    /// <summary>
    /// Emitted when the operands of a range expression are not of type 'System.Index'.
    /// </summary>
    DS0155_RangeInvalidOperands,
    /// <summary>
    /// Emitted when a type implementing a template does not implement a required, abstract template member.
    /// </summary>
    DS0156_RequiredInterfaceMembersNotImplemented,
    /// <summary>
    /// Emitted when inheriting from a sealed type.
    /// </summary>
    DS0157_InheritingFromSealedType,
    /// <summary>
    /// Emitted when a template type contains an instance field.
    /// </summary>
    DS0158_InstanceFieldInTemplate,
    /// <summary>
    /// Displayed when a feature is not available due to a framework limitation.
    /// </summary>
    DS0159_FrameworkLimitation,
    /// <summary>
    /// Emitted when a custom operator is defined outside of a module.
    /// </summary>
    DS0160_CustomOperatorDefinedOutsideModule,
    /// <summary>
    /// Emitted when a custom operator has an access modifier other than 'global'.
    /// </summary>
    DS0161_CustomOperatorNotGlobal,
    /// <summary>
    /// Emitted when a custom operator has more than two operands.
    /// </summary>
    DS0162_CustomOperatorTooManyParameters,
    /// <summary>
    /// Emitted when a custom operator has no body.
    /// </summary>
    DS0163_CustomOperatorNoMethodBody,
    /// <summary>
    /// Emitted when a custom operator could not be resolved.
    /// </summary>
    DS0164_CustomOperatorNotFound,
    /// <summary>
    /// Emitted when a custom operator returns no value.
    /// </summary>
    DS0165_CustomOperatorNoReturnValue,
    /// <summary>
    /// Emitted when a module initializer does not fill all requirements.
    /// </summary>
    DS0166_ModuleInitializerInvalid,
    /// <summary>
    /// Emitted when a property is declared as a compile-time constant.
    /// </summary>
    DS0167_PropertyLiteral,
    /// <summary>
    /// Emitted when the ':?' operator is used on a value type.
    /// </summary>
    DS0168_InstanceCheckOperatorOnValueType,
    /// <summary>
    /// Warning emitted when a project file contains no build log devices, effectively disabling any error reporting.
    /// </summary>
    DS0169_NoBuildLogDevices,
    /// <summary>
    /// Emitted when an invalid build device is selected inside of a project file.
    /// </summary>
    DS0170_InvalidBuildDeviceName,
    /// <summary>
    /// Emitted when the <c>&lt;File&gt;</c> build log device is used without specifying a file path.
    /// </summary>
    DS0171_FileBuildLogDeviceNoPathSpecified,
    /// <summary>
    /// Emitted when a field has the <c>&lt;Event&gt;</c> and <c>&lt;Auto&gt;</c> attributes applied at the same time.
    /// </summary>
    DS0172_EventAndProperty,
    /// <summary>
    /// Emitted when an event has multiple 'add' or 'remove' handlers.
    /// </summary>
    DS0173_EventHasMultipleHandlers,
    /// <summary>
    /// Emitted when the field type of an event is not a delegate.
    /// </summary>
    DS0174_EventFieldTypeNotDelegate,
    /// <summary>
    /// Emitted when an event defines one of the required 'add' or 'remove' handlers, but not both.
    /// </summary>
    DS0175_EventMissingHandlers,
    /// <summary>
    /// Emitted when the '$lock' statement is used on a value type.
    /// </summary>
    DS0176_LockOnValueType,
    /// <summary>
    /// Emitted when a type is used as an attribute that does not inherit from <see cref="Attribute"/>.
    /// </summary>
    DS0177_InvalidAttributeType,
    /// <summary>
    /// Emitted when an expression used as an attribute argument is not a compile-time constant.
    /// </summary>
    DS0178_InvalidAttributeArgument,
    /// <summary>
    /// Emitted when an alias type has an attribute list.
    /// </summary>
    DS0179_AttributesOnAliasType,
    /// <summary>
    /// Emitted when an alias type has modifiers that are not permitted.
    /// </summary>
    DS0180_AliasTypeInvalidModifiers,
    /// <summary>
    /// Emitted when the target of an attribute is invalid.
    /// </summary>
    DS0181_InvalidAttributeTarget,
    /// <summary>
    /// Emitted when an inline union type has multiple cases with the same name.
    /// </summary>
    DS0182_UnionTypeDuplicateTagName,
    /// <summary>
    /// Emitted when an inline union type has multiple cases with the same type.
    /// </summary>
    DS0183_UnionTypeDuplicateTagType,
    /// <summary>
    /// Emitted when an inline union type has some named and some unnamed tags.
    /// </summary>
    DS0184_UnionTypeMixedTags,
    /// <summary>
    /// Emitted when a type alias implements one or more interfaces.
    /// </summary>
    DS0185_AliasTypeImplementsInterface,
    /// <summary>
    /// Emitted when a type alias explicitly specifies a base type.
    /// </summary>
    DS0186_AliasTypeExtendsType,
    /// <summary>
    /// Emitted when a type alias has a generic type parameter list. This restriction might get lifted in the future.
    /// </summary>
    DS0187_GenericAliasType,
    /// <summary>
    /// Emitted when a string processor is used that cannot be resolved.
    /// </summary>
    DS0188_InvalidStringProcessor,
    /// <summary>
    /// Emitted when a processed string contains interpolations.
    /// </summary>
    DS0189_ProcessedStringContainsInterpolations,
    /// <summary>
    /// Emitted when a string processor throws an exception at compile-time.
    /// </summary>
    DS0190_StringProcessorThrewException,
    /// <summary>
    /// Emitted when a program contains multiple explicit or implicit entry points.
    /// </summary>
    DS0191_AmbiguousEntryPoint,
    /// <summary>
    /// Emitted when an inheritance chain contains a circular reference.
    /// </summary>
    DS0192_CircularReference,
    /// <summary>
    /// Emitted when a CLI command is invoked with an option or flag that requires a value but no value is set.
    /// </summary>
    DS0193_ExpectedCliOptionValue,
    /// <summary>
    /// Emitted when the 'dc watch' command is invoked with an invalid combination of arguments.
    /// </summary>
    DS0194_DCWatchInvalidCombination,
    /// <summary>
    /// Emitted when the 'EntryPoint' property is set in <c>dsconfig.xml</c> while the application also manually marks a method using the <see cref="EntryPointAttribute"/> attribute.
    /// </summary>
    DS0195_EntryPointManuallySetWhenUsingDSConfigEntryPointProperty,
    /// <summary>
    /// Emitted when a string representation of a method is not a valid method identifier.
    /// </summary>
    DS0196_InvalidMethodIdentifier,
    /// <summary>
    /// Emitted when an imported project configuration file does not exist.
    /// </summary>
    DS0197_ImportedConfigFileNotFound,
    /// <summary>
    /// Emitted when a project configuration import results in a circular dependency.
    /// </summary>
    DS0198_ImportedConfigFileCircularDependency,
    /// <summary>
    /// Emitted when a file exports multiple namespaces.
    /// </summary>
    DS0199_MultipleExports,
    /// <summary>
    /// Emitted when a list literal constructed from an integer range does not use compile-time constant indices.
    /// </summary>
    DS0200_ListFromRangeNotCompileTimeConstant,
    /// <summary>
    /// Emitted when the designated application entry point has an invalid signature.
    /// </summary>
    DS0201_EntryPointInvalidSignature,
    /// <summary>
    /// Emitted when the condition of a conditional expression is a compile-time constant.
    /// </summary>
    DS0202_ConditionConstant,
    /// <summary>
    /// Emitted when an invalid type is used as a generic argument.
    /// </summary>
    DS0203_InvalidGenericArgument,
    /// <summary>
    /// Emitted when a project structure contains a circular dependency.
    /// </summary>
    DS0204_CircularProjectDependency,
    /// <summary>
    /// Emitted when the 'dc new' command is invoked with invalid arguments.
    /// </summary>
    DS0205_DCNewInvalidArguments,
    /// <summary>
    /// Emitted when the 'dc new' command specifies a project directory that is not empty.
    /// </summary>
    DS0206_DCNewNonEmptyDirectory,
    /// <summary>
    /// Emitted when a resource file passed to the compiler is invalid.
    /// </summary>
    DS0207_InvalidResourceFile,
    /// <summary>
    /// Emitted when the compiler produces code that is unverifiable.
    /// </summary>
    DS0208_UnverifiableCode,
    /// <summary>
    /// Emitted when the executable project defined in a project group file could not be found.
    /// </summary>
    DS0209_ProjectGroupExecutableInvalid,
    /// <summary>
    /// Emitted when AOT compilation is attempted for a system other than the current one.
    /// </summary>
    DS0210_CrossSystemAotCompilation,
    /// <summary>
    /// Emitted when unexpected or invalid arguments are passed to a command.
    /// </summary>
    DS0211_UnexpectedArgument,
    /// <summary>
    /// Emitted when a program that contains varargs function declarations is compiled ahead-of-time.
    /// </summary>
    DS0212_AotVarArgsFunction,
    /// <summary>
    /// Emitted when a program that contains a varargs function declaration is compiled for a platform other than Windows.
    /// </summary>
    DS0213_VarArgsNonWindows,
    /// <summary>
    /// Emitted when an 'extern' block contains another.
    /// </summary>
    DS0214_NestedExternalBlock,
    /// <summary>
    /// Emitted when an external extension caused an unhandled exception.
    /// </summary>
    DS0215_ExtensionThrewException,
    /// <summary>
    /// Emitted when a compiler directive is used as an expression.
    /// </summary>
    DS0216_CompilerDirectiveAsExpression,
    /// <summary>
    /// Emitted when a compiler directive is used that could not be found.
    /// </summary>
    DS0217_InvalidCompilerDirective,
    /// <summary>
    /// Emitted when a compiler directive is called with invalid arguments.
    /// </summary>
    DS0218_CompilerDirectiveInvalidArguments
}