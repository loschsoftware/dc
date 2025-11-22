using Dassie.Configuration;
using Dassie.Core;
using Dassie.Meta;
using System;
using System.Reflection.Emit;

namespace Dassie.Messages;

/// <summary>
/// Specifies the code of a diagnostic message.
/// </summary>
public enum MessageCode
{
    /// <summary>
    /// Represents a custom message emitted by tools such as code analyzers.
    /// </summary>
    Custom = -1,
    /// <summary>
    /// Specifies that an operation was successful.
    /// </summary>
    DS0000_Success,
    /// <summary>
    /// An unknown and not further specified error or an unhandled exception.
    /// </summary>
    DS0001_UnknownError,
    /// <summary>
    /// An error that occured during the parsing stage of the compilation process that was emitted by ANTLR.
    /// </summary>
    DS0002_SyntaxError,
    /// <summary>
    /// Emitted when the specified argument list does not match any available overload of this function.
    /// </summary>
    DS0003_MethodNotFound,
    /// <summary>
    /// Emitted when the specified argument list does not match any available constructor for this type.
    /// </summary>
    [Obsolete("This error code is not emitted by the compiler anymore.")]
    DS0004_ConstructorNotFound,
    /// <summary>
    /// Emitted when an import directive is used on a non-existent namespace.
    /// </summary>
    DS0005_NamespaceNotFound,
    /// <summary>
    /// Emitted when an identifier contains invalid characters or is a language keyword.
    /// </summary>
    DS0006_InvalidIdentifier,
    /// <summary>
    /// Emitted when the type of the new value of a variable does not match the type of the old value.
    /// </summary>
    DS0007_VariableTypeChanged,
    /// <summary>
    /// Emitted when an access modifier is invalid for the specified object.
    /// </summary>
    DS0008_InvalidModifier,
    /// <summary>
    /// Emitted when a function contains no expressions.
    /// </summary>
    DS0009_NoExpressionInFunction,
    /// <summary>
    /// Emitted when a specified type was not found.
    /// </summary>
    DS0010_TypeNotFound,
    /// <summary>
    /// Emitted when the condition of a while loop is not a boolean.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0011_LoopConditionNotBoolean,
    /// <summary>
    /// 
    /// </summary>
    DS0012_UndefinedValue,
    /// <summary>
    /// General error emitted if a command is invoked with an invalid set of arguments.
    /// </summary>
    DS0013_InvalidArgument,
    /// <summary>
    /// Emitted when a conversion between two types is not implemented.
    /// </summary>
    DS0014_InvalidConversion,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0015_UnknownType,
    /// <summary>
    /// Emitted when a constant is being reassigned.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0016_ConstantReassignment,
    /// <summary>
    /// Emitted when a constant is declared without a value.
    /// </summary>
    DS0017_UnassignedConstant,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0018_DoubleDefinition,
    /// <summary>
    /// Emitted when a constant is being reassigned.
    /// </summary>
    DS0019_ImmutableValueReassignment,
    /// <summary>
    /// 
    /// </summary>
    DS0020_GenericValueTypeInvalid,
    /// <summary>
    /// Emitted when a feature of a newer Dassie version is being used.
    /// </summary>
    DS0021_FeatureNotAvailable,
    /// <summary>
    /// 
    /// </summary>
    DS0022_PropertyNotFound,
    /// <summary>
    /// Emitted when an assembly reference could not be resolved.
    /// </summary>
    DS0023_InvalidAssemblyReference,
    /// <summary>
    /// Emitted when a file reference could not be resolved.
    /// </summary>
    DS0024_InvalidFileReference,
    /// <summary>
    /// 
    /// </summary>
    DS0025_ReadOnlyProperty,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0026_VariableAlreadyDefined,
    /// <summary>
    /// Emitted when an unknown escape sequence is being used.
    /// </summary>
    DS0027_InvalidEscapeSequence,
    /// <summary>
    /// Emitted when the program contains no executable code.
    /// </summary>
    DS0028_EmptyProgram,
    /// <summary>
    /// Emitted when a checked expression results in an integer overflow.
    /// </summary>
    DS0029_IntegerOverflow,
    /// <summary>
    /// Emitted when an IO error occurs, denying the Dassie compiler access to the source files or output file.
    /// </summary>
    DS0030_FileAccessDenied,
    /// <summary>
    /// Emitted when a Dassie code contains no suitable entry point.
    /// </summary>
    DS0031_NoEntryPoint,
    /// <summary>
    /// Obsolete.
    /// </summary>
    [Obsolete("Obsolete: This error is not emitted anymore by the Dassie compiler.")]
    DS0032_InvalidClassModifiers,
    /// <summary>
    /// Emitted when an argument violates a parameter constraint.
    /// </summary>
    DS0033_ConstraintViolation,
    /// <summary>
    /// Emitted when implicitly converting between two types is not possible.
    /// </summary>
    DS0034_ImplicitConversionNotPossible,
    /// <summary>
    /// Emitted when explicitly converting between two types is not possible.
    /// </summary>
    DS0035_ExplicitConversionNotPossible,
    /// <summary>
    /// Emitted when the entry point of a Dassie program is not a static function.
    /// </summary>
    DS0036_EntryPointNotStatic,
    /// <summary>
    /// General error message emitted for various kinds of arithmetic errors.
    /// </summary>
    DS0037_ArithmeticError,
    /// <summary>
    /// Emitted when the return types of branches do not match.
    /// </summary>
    DS0038_BranchExpressionTypesUnequal,
    /// <summary>
    /// Emitted when the condition of a conditional expression is not a boolean.
    /// </summary>
    DS0039_ConditionalExpressionClauseNotBoolean,
    /// <summary>
    /// Emitted when a type does not include a specified field.
    /// </summary>
    DS0040_FieldNotFound,
    /// <summary>
    /// Emitted when the specified name is not assigned to a value.
    /// </summary>
    DS0041_VariableNotFound,
    /// <summary>
    /// Emitted when the types of list or array items are not the same.
    /// </summary>
    DS0042_ListItemsHaveDifferentTypes,
    /// <summary>
    /// Emitted when the index expression of an array element assignment expression is not an integer.
    /// </summary>
    DS0043_ArrayElementAssignmentIndexExpressionNotInteger,
    /// <summary>
    /// Emitted when the compiler thinks an infinite loop could be unintentional.
    /// </summary>
    DS0044_PossiblyUnintentionalInfiniteLoop,
    /// <summary>
    /// Emitted when an update to dc is available. This is an information message, not an error.
    /// </summary>
    DS0045_DCUpdateAvailable,
    /// <summary>
    /// Emitted when an inline IL instruction has an invalid opcode.
    /// </summary>
    DS0046_InlineILInvalidOpCode,
    /// <summary>
    /// Emitted when an inline IL instruction has an invalid operand.
    /// </summary>
    DS0047_InlineILInvalidOperand,
    /// <summary>
    /// Emitted when there are duplicate types in a union type.
    /// </summary>
    DS0048_UnionTypeDuplicate,
    /// <summary>
    /// Emitted when a specified source file could not be found.
    /// </summary>
    DS0049_SourceFileNotFound,
    /// <summary>
    /// Emitted when a for loop contains more than 3 expressions.
    /// </summary>
    DS0050_InvalidForLoopSyntax,
    /// <summary>
    /// Emitted when the return value of a top-level program is not an integer.
    /// </summary>
    DS0051_ExpectedIntegerReturnValue,
    /// <summary>
    /// Emitted when the inheritance list of a type contains more than one CLR class.
    /// </summary>
    DS0052_MoreThanOneClassInInheritanceList,
    /// <summary>
    /// Emitted when a type member has an invalid access modifier.
    /// </summary>
    DS0053_InvalidAccessModifier,
    /// <summary>
    /// Emitted when the return value of a function does not match the function signature.
    /// </summary>
    DS0054_WrongReturnType,
    /// <summary>
    /// Emitted when a field is set to a value of the wrong type.
    /// </summary>
    DS0055_WrongFieldType,
    /// <summary>
    /// Emitted when multiple functions are declared as application entry points.
    /// </summary>
    DS0056_MultipleEntryPoints,
    /// <summary>
    /// Generic error emitted when a symbol of unspecific type could not be resolved.
    /// </summary>
    DS0057_SymbolResolveError,
    /// <summary>
    /// Emitted when a variable is assigned an expression of an incompatible type.
    /// </summary>
    DS0058_IncompatibleType,
    /// <summary>
    /// Message emitted when a type or member is declared with a redundant access modifier.
    /// </summary>
    DS0059_RedundantModifier,
    /// <summary>
    /// Emitted when a rethrow expression is used outside of a catch block.
    /// </summary>
    DS0060_RethrowOutsideCatchBlock,
    /// <summary>
    /// Emitted when the expression to be thrown is not a reference type.
    /// </summary>
    DS0061_InvalidThrowExpression,
    /// <summary>
    /// Emitted when a try block is not followed by a catch block.
    /// </summary>
    DS0062_MissingCatchBranch,
    /// <summary>
    /// Emitted when a local is not in scope.
    /// </summary>
    DS0063_LocalOutsideScope,
    /// <summary>
    /// Emitted when a feature that is not yet implemented is used.
    /// </summary>
    DS0064_UnsupportedFeature,
    /// <summary>
    /// Emitted when something that shouldn't is used as an expression.
    /// </summary>
    DS0065_InvalidExpression,
    /// <summary>
    /// Emitted when the left side of an assignment is invalid.
    /// </summary>
    DS0066_AssignmentInvalidLeftSide,
    /// <summary>
    /// Emitted when a property cannot be assigned to.
    /// </summary>
    DS0067_PropertyNoSuitableSetter,
    /// <summary>
    /// Emitted when a resource specified in dsconfig.xml could not be located.
    /// </summary>
    DS0068_ResourceFileNotFound,
    /// <summary>
    /// Emitted when multiple unmanaged resources are specified in dsconfig.xml.
    /// </summary>
    DS0069_MultipleUnmanagedResources,
    /// <summary>
    /// Emitted when a required Windows SDK tool could not be located.
    /// </summary>
    DS0070_WinSdkToolNotFound,
    /// <summary>
    /// Emitted when the <code>&lt;VersionInfo&gt;</code> tag is used in dsconfig.xml.
    /// </summary>
    DS0071_AvoidVersionInfoTag,
    /// <summary>
    /// Emitted when an ingored message is specified in dsconfig.xml that cannot be ignored.
    /// </summary>
    DS0072_IllegalIgnoredMessage,
    /// <summary>
    /// Emitted when 'dc build' is called but there are no source files.
    /// </summary>
    DS0073_NoSourceFilesFound,
    /// <summary>
    /// Unused.
    /// </summary>
    DS0074_Unused,
    /// <summary>
    /// Emitted when one of the conditions in the following list is met. These restrictions are dictated either by the ECMA-335 specification, implementation-specific restrictions of the Microsoft implementation of ECMA-335, or practical restrictions of the Dassie compiler.
    /// <list type="bullet">
    /// <item>A fully qualified type name, member name, type parameter or formal parameter name is longer than 1024 characters.</item>
    /// <item>A function contains more than 65,534 local variables, type parameters or formal parameters.</item>
    /// <item>The IL body of a function exceeds 2,147,483,647 bytes.</item>
    /// <item>A type contains more than 65,535 generic type parameters or members.</item>
    /// <item>An assembly contains more than 16,777,215 metadata entries.</item>
    /// <item>The value of one or more fields of the version number of the assembly exceeds 4,294,967,295.</item>
    /// <item>An assembly contains more than 65,536 assembly references.</item>
    /// </list>
    /// </summary>
    DS0075_MetadataLimitExceeded,
    /// <summary>
    /// Emitted when an integer literal is too big for its type.
    /// </summary>
    DS0076_Overflow,
    /// <summary>
    /// Emitted when a character literal contains no character.
    /// </summary>
    DS0077_EmptyCharacterLiteral,
    /// <summary>
    /// Emitted when an import directive is invalid.
    /// </summary>
    DS0078_InvalidImport,
    /// <summary>
    /// Emitted when the modifier of an access modifier group is the default member modifier of the containing type.
    /// </summary>
    DS0079_RedundantAccessModifierGroup,
    /// <summary>
    /// Emitted when an array has more than 32 dimensions.
    /// </summary>
    DS0080_ArrayTooManyDimensions,
    /// <summary>
    /// Emitted when an illegal identifier is used.
    /// </summary>
    DS0081_ReservedIdentifier,
    /// <summary>
    /// Emitted when a project reference is invalid.
    /// </summary>
    DS0082_InvalidProjectReference,
    /// <summary>
    /// Emitted when an invalid macro is used in dsconfig.xml.
    /// </summary>
    DS0083_InvalidDSConfigMacro,
    /// <summary>
    /// Emitted when the 'var' modifier is used on a method.
    /// </summary>
    DS0084_InvalidVarModifier,
    /// <summary>
    /// Emitted when the 'this' keyword is used in a static function.
    /// </summary>
    DS0085_ThisInStaticFunction,
    /// <summary>
    /// Emitted when a <see cref="TypeBuilder"/> could not be associated with any <see cref="TypeContext"/> object. Critical error that should never be emitted under normal circumstances.
    /// </summary>
    DS0086_TypeInfoCouldNotBeRead,
    /// <summary>
    /// Emitted when the command of a build profile or -event is invalid.
    /// </summary>
    DS0087_InvalidCommand,
    /// <summary>
    /// Emitted when a build or debug profile does not exist.
    /// </summary>
    DS0088_InvalidProfile,
    /// <summary>
    /// Emitted when a critical build event failed to run correctly.
    /// </summary>
    DS0089_BuildEventFailedToRun,
    /// <summary>
    /// Emitted when a dsconfig.xml document contains an invalid property.
    /// </summary>
    DS0090_InvalidDSConfigProperty,
    /// <summary>
    /// Emitted when some part of dsconfig.xml is malformed, either syntactically or semantically.
    /// </summary>
    DS0091_MalformedConfigurationFile,
    /// <summary>
    /// Emitted when the format of a dsconfig.xml file is newer than supported.
    /// </summary>
    DS0092_ConfigurationFormatVersionTooNew,
    /// <summary>
    /// Emitted when dsconfig.xml uses an outdated format.
    /// </summary>
    DS0093_ConfigurationFormatVersionTooOld,
    /// <summary>
    /// Emitted when a constructor returns a value, even though it is only allowed to return void.
    /// </summary>
    DS0094_ConstructorReturnsValue,
    /// <summary>
    /// Emitted when an init-only field is assigned to outside of a constructor.
    /// </summary>
    DS0095_InitOnlyFieldAssignedOutsideOfConstructor,
    /// <summary>
    /// Emitted when an immutable symbol is passed by reference.
    /// </summary>
    DS0096_ImmutableSymbolPassedByReference,
    /// <summary>
    /// Emitted when a symbol is passed by reference without the reference operator (&amp;).
    /// </summary>
    DS0097_PassByReferenceWithoutOperator,
    /// <summary>
    /// Emitted when an expression to be passed by reference is not valid.
    /// </summary>
    DS0098_InvalidExpressionPassedByReference,
    /// <summary>
    /// Emitted when a specified scratch could not be found.
    /// </summary>
    DS0099_ScratchNotFound,
    /// <summary>
    /// Emitted when multiple installed compiler extensions define a command with the same name.
    /// </summary>
    DS0100_DuplicateCompilerCommand,
    /// <summary>
    /// Emitted when dc is called with an invalid command name.
    /// </summary>
    DS0101_InvalidCommand,
    /// <summary>
    /// Diagnostic information message.
    /// </summary>
    DS0102_DiagnosticInfo,
    /// <summary>
    /// Message displayed when compiling ahead of time informing of performance penalty.
    /// </summary>
    DS0103_AotPerformanceWarning,
    /// <summary>
    /// Emitted when an operation that requires an internet connection cannot be completed.
    /// </summary>
    DS0104_NetworkError,
    /// <summary>
    /// Emitted when a package reference contains an invalid name or version number.
    /// </summary>
    DS0105_InvalidPackageReference,
    /// <summary>
    /// Emitted when the 'dc run' command is invoked with insufficient information.
    /// </summary>
    DS0106_DCRunInsufficientInfo,
    /// <summary>
    /// Emitted when no input source files are specified.
    /// </summary>
    DS0107_NoInputFiles,
    /// <summary>
    /// Emitted when a generic constraint is violated.
    /// </summary>
    DS0108_GenericTypeConstraintViolation,
    /// <summary>
    /// Emitted when a generic method is called without arguments.
    /// </summary>
    DS0109_GenericMethodCalledWithoutGenericArguments,
    /// <summary>
    /// Emitted when a local is defined with a name that already exists in a different scope.
    /// </summary>
    DS0110_LocalDefinedInDifferentScope,
    /// <summary>
    /// Emitted when a type parameter has multiple of the same attribute.
    /// </summary>
    DS0111_DuplicateTypeParameterAttributes,
    /// <summary>
    /// Emitted when a type parameter has multiple of the same type constraints.
    /// </summary>
    DS0112_DuplicateTypeParameterConstraint,
    /// <summary>
    /// Emitted when a type defines multiple type parameters with the same name.
    /// </summary>
    DS0113_DuplicateTypeParameter,
    /// <summary>
    /// Emitted when a type parameter defines an invalid set of attributes.
    /// </summary>
    DS0114_InvalidTypeParameterAttributes,
    /// <summary>
    /// Emitted when a generic method defines a type parameter that is already defined by the containing type.
    /// </summary>
    DS0115_TypeParameterIsDefinedInContainingScope,
    /// <summary>
    /// Emitted when a non-abstract method does not have a body.
    /// </summary>
    DS0116_NonAbstractMethodHasNoBody,
    /// <summary>
    /// Emitted when an abstract method has a body.
    /// </summary>
    DS0117_AbstractMethodHasBody,
    /// <summary>
    /// Emitted when a variance modifier is applied to a type parameter of a type other than a <c>template</c>.
    /// </summary>
    DS0118_VarianceModifierOnConcreteType,
    /// <summary>
    /// Emitted when variant type parameters are used in invalid ways.
    /// </summary>
    DS0119_InvalidVariance,
    /// <summary>
    /// Emitted when two types of the same name are defined.
    /// </summary>
    DS0120_DuplicateTypeName,
    /// <summary>
    /// Emitted when two types of the same name with different type parameters are defined.
    /// </summary>
    DS0121_DuplicateGenericTypeName,
    /// <summary>
    /// Emitted when a function pointer is created for a function with more than 16 arguments.
    /// </summary>
    DS0122_FunctionPointerTooManyArguments,
    /// <summary>
    /// Emitted when a function pointer is created for an invalid expression.
    /// </summary>
    DS0123_InvalidFunctionPointerTargetExpression,
    /// <summary>
    /// Emitted when an installed compiler extension is invalid.
    /// </summary>
    DS0124_InvalidExtensionPackage,
    /// <summary>
    /// Emitted when the 'dc run' command is called on a project that is not executable.
    /// </summary>
    DS0125_DCRunInvalidProjectType,
    /// <summary>
    /// Emitted when a value is ignored implicitly.
    /// </summary>
    DS0126_UnusedValue,
    /// <summary>
    /// Emitted when a non-nested type is declared as 'local'.
    /// </summary>
    DS0127_TopLevelTypeLocal,
    /// <summary>
    /// Emitted when a member cannot be accessed because of too restrictive access modifiers.
    /// </summary>
    DS0128_AccessModifiersTooRestrictive,
    /// <summary>
    /// Emitted when the 'dc deploy' command is run on a config file that does not represent a project group.
    /// </summary>
    DS0129_DeployCommandInvalidProjectGroupFile,
    /// <summary>
    /// Emitted when a project group contains no projects.
    /// </summary>
    DS0130_ProjectGroupNoComponents,
    /// <summary>
    /// Emitted when a project group defines no targets.
    /// </summary>
    DS0131_ProjectGroupNoTargets,
    /// <summary>
    /// Emitted when 'dc build' is used on a project group.
    /// </summary>
    DS0132_DCBuildCalledOnProjectGroup,
    /// <summary>
    /// Emitted when 'dc clean' is called but the working directory contains no compiler configuration file.
    /// </summary>
    DS0133_DCCleanNoProjectFile,
    /// <summary>
    /// Emitted when 'dc analyze' is called but the working directory contains no compiler configuration file.
    /// </summary>
    DS0134_DCAnalyzeNoProjectFile,
    /// <summary>
    /// Emitted when 'dc analyze' is called using an analyzer that could not be found.
    /// </summary>
    DS0135_DCAnalyzeInvalidAnalyzer,
    /// <summary>
    /// Emitted when attempting to divide by the constant value 0.
    /// </summary>
    DS0136_DivisionByConstantZero,
    /// <summary>
    /// Emitted when a conversion between two types is not possible.
    /// </summary>
    DS0137_InvalidConversion,
    /// <summary>
    /// Emitted when the 'literal' modifier is applied to a method.
    /// </summary>
    DS0138_LiteralModifierOnMethod,
    /// <summary>
    /// Emitted when a compile-time constant value is expected but was not provided.
    /// </summary>
    DS0139_CompileTimeConstantRequired,
    /// <summary>
    /// Emitted when trying to instantiate a module.
    /// </summary>
    DS0140_ModuleInstantiation,
    /// <summary>
    /// Emitted when an enumeration is not an integer type.
    /// </summary>
    DS0141_InvalidEnumerationType,
    /// <summary>
    /// Emitted when an enumeration type contains a method.
    /// </summary>
    DS0142_MethodInEnumeration,
    /// <summary>
    /// Emitted when an enumeration type is defined using the 'ref' modifier.
    /// </summary>
    DS0143_EnumTypeExplicitlyRef,
    /// <summary>
    /// Emitted when an enumeration type defines a base type other than System.Enum.
    /// </summary>
    DS0144_EnumTypeBaseType,
    /// <summary>
    /// Emitted when an enumeration type implements a template.
    /// </summary>
    DS0145_EnumTypeImplementsTemplate,
    /// <summary>
    /// Emitted when a type explicitly inherits from 'System.ValueType'.
    /// </summary>
    DS0146_ValueTypeInherited,
    /// <summary>
    /// Emitted when the inheritance list of a value type contains a reference type.
    /// </summary>
    DS0147_ValueTypeInheritsFromClass,
    /// <summary>
    /// Emitted when inheriting from a value type.
    /// </summary>
    DS0148_ValueTypeAsBaseType,
    /// <summary>
    /// Emitted when an immutable value has a byref type.
    /// </summary>
    DS0149_ImmutableValueOfByRefType,
    /// <summary>
    /// Emitted when a nested byref type (e.g. int&amp;&amp;) is used.
    /// </summary>
    DS0150_NestedByRefType,
    /// <summary>
    /// Emitted when a field of a byref or byref-like type is declared outside of a byref-like type.
    /// </summary>
    DS0151_ByRefFieldInNonByRefLikeType,
    /// <summary>
    /// Emitted when an immutable value type (val! type) contains a field explicitly marked as 'var'.
    /// </summary>
    DS0152_VarFieldInImmutableType,
    /// <summary>
    /// Emitted when a function pointer is created for an instance method.
    /// </summary>
    DS0153_FunctionPointerForInstanceMethod,
    /// <summary>
    /// Emitted when a list literal contains incompatible types.
    /// </summary>
    DS0154_ListLiteralDifferentTypes,
    /// <summary>
    /// Emitted when an incompatible expression is appended to a list.
    /// </summary>
    DS0155_ListAppendIncompatibleElement,
    /// <summary>
    /// Emitted when the operands of a range expression are not of type 'System.Index'.
    /// </summary>
    DS0156_RangeInvalidOperands,
    /// <summary>
    /// Emitted when a type implementing a template does not implement a required, abstract template member.
    /// </summary>
    DS0157_RequiredInterfaceMembersNotImplemented,
    /// <summary>
    /// Emitted when inheriting from a sealed type.
    /// </summary>
    DS0158_InheritingFromSealedType,
    /// <summary>
    /// Emitted when a template type contains an instance field.
    /// </summary>
    DS0159_InstanceFieldInTemplate,
    /// <summary>
    /// Displayed when a feature is not available due to a framework limitation.
    /// </summary>
    DS0160_FrameworkLimitation,
    /// <summary>
    /// Emitted when a custom operator is defined outside of a module.
    /// </summary>
    DS0161_CustomOperatorDefinedOutsideModule,
    /// <summary>
    /// Emitted when a custom operator has an access modifier other than 'global'.
    /// </summary>
    DS0162_CustomOperatorNotGlobal,
    /// <summary>
    /// Emitted when a custom operator has more than two operands.
    /// </summary>
    DS0163_CustomOperatorTooManyParameters,
    /// <summary>
    /// Emitted when a custom operator has no body.
    /// </summary>
    DS0164_CustomOperatorNoMethodBody,
    /// <summary>
    /// Emitted when a custom operator could not be resolved.
    /// </summary>
    DS0165_CustomOperatorNotFound,
    /// <summary>
    /// Emitted when a custom operator returns no value.
    /// </summary>
    DS0166_CustomOperatorNoReturnValue,
    /// <summary>
    /// Emitted when a module initializer does not fill all requirements.
    /// </summary>
    DS0167_ModuleInitializerInvalid,
    /// <summary>
    /// Emitted when a property is declared as a compile-time constant.
    /// </summary>
    DS0168_PropertyLiteral,
    /// <summary>
    /// Emitted when the ':?' operator is used on a value type.
    /// </summary>
    DS0169_InstanceCheckOperatorOnValueType,
    /// <summary>
    /// Warning emitted when a project file contains no build log devices, effectively disabling any error reporting.
    /// </summary>
    DS0170_NoBuildLogDevices,
    /// <summary>
    /// Emitted when an invalid build device is selected inside of a project file.
    /// </summary>
    DS0171_InvalidBuildDeviceName,
    /// <summary>
    /// Emitted when the <c>&lt;File&gt;</c> build log device is used without specifying a file path.
    /// </summary>
    DS0172_FileBuildLogDeviceNoPathSpecified,
    /// <summary>
    /// Emitted when a field has the <c>&lt;Event&gt;</c> and <c>&lt;Auto&gt;</c> attributes applied at the same time.
    /// </summary>
    DS0173_EventAndProperty,
    /// <summary>
    /// Emitted when an event has multiple 'add' or 'remove' handlers.
    /// </summary>
    DS0174_EventHasMultipleHandlers,
    /// <summary>
    /// Emitted when the field type of an event is not a delegate.
    /// </summary>
    DS0175_EventFieldTypeNotDelegate,
    /// <summary>
    /// Emitted when an event defines one of the required 'add' or 'remove' handlers, but not both.
    /// </summary>
    DS0176_EventMissingHandlers,
    /// <summary>
    /// Emitted when the '$lock' statement is used on a value type.
    /// </summary>
    DS0177_LockOnValueType,
    /// <summary>
    /// Emitted when a type is used as an attribute that does not inherit from <see cref="Attribute"/>.
    /// </summary>
    DS0178_InvalidAttributeType,
    /// <summary>
    /// Emitted when an expression used as an attribute argument is not a compile-time constant.
    /// </summary>
    DS0179_InvalidAttributeArgument,
    /// <summary>
    /// Emitted when an alias type has an attribute list.
    /// </summary>
    DS0180_AttributesOnAliasType,
    /// <summary>
    /// Emitted when an alias type has modifiers that are not permitted.
    /// </summary>
    DS0181_AliasTypeInvalidModifiers,
    /// <summary>
    /// Emitted when the target of an attribute is invalid.
    /// </summary>
    DS0182_InvalidAttributeTarget,
    /// <summary>
    /// Emitted when an inline union type has multiple cases with the same name.
    /// </summary>
    DS0183_UnionTypeDuplicateTagName,
    /// <summary>
    /// Emitted when an inline union type has multiple cases with the same type.
    /// </summary>
    DS0184_UnionTypeDuplicateTagType,
    /// <summary>
    /// Emitted when an inline union type has some named and some unnamed tags.
    /// </summary>
    DS0185_UnionTypeMixedTags,
    /// <summary>
    /// Emitted when a type alias implements one or more interfaces.
    /// </summary>
    DS0186_AliasTypeImplementsInterface,
    /// <summary>
    /// Emitted when a type alias explicitly specifies a base type.
    /// </summary>
    DS0187_AliasTypeExtendsType,
    /// <summary>
    /// Emitted when a type alias has a generic type parameter list. This restriction might get lifted in the future.
    /// </summary>
    DS0188_GenericAliasType,
    /// <summary>
    /// Emitted when a string processor is used that cannot be resolved.
    /// </summary>
    DS0189_InvalidStringProcessor,
    /// <summary>
    /// Emitted when a processed string contains interpolations.
    /// </summary>
    DS0190_ProcessedStringContainsInterpolations,
    /// <summary>
    /// Emitted when a string processor throws an exception at compile-time.
    /// </summary>
    DS0191_StringProcessorThrewException,
    /// <summary>
    /// Emitted when a program contains multiple explicit or implicit entry points.
    /// </summary>
    DS0192_AmbiguousEntryPoint,
    /// <summary>
    /// Emitted when an inheritance chain contains a circular reference.
    /// </summary>
    DS0193_CircularReference,
    /// <summary>
    /// Emitted when a CLI command is invoked with an option or flag that requires a value but no value is set.
    /// </summary>
    DS0194_ExpectedCliOptionValue,
    /// <summary>
    /// Emitted when the 'dc watch' command is invoked with an invalid combination of arguments.
    /// </summary>
    DS0195_DCWatchInvalidCombination,
    /// <summary>
    /// Emitted when the 'EntryPoint' property is set in <c>dsconfig.xml</c> while the application also manually marks a method using the <see cref="EntryPointAttribute"/> attribute.
    /// </summary>
    DS0196_EntryPointManuallySetWhenUsingDSConfigEntryPointProperty,
    /// <summary>
    /// Emitted when a string representation of a method is not a valid method identifier.
    /// </summary>
    DS0197_InvalidMethodIdentifier,
    /// <summary>
    /// Emitted when an imported project or global configuration file does not exist.
    /// </summary>
    DS0198_ImportedConfigFileNotFound,
    /// <summary>
    /// Emitted when a project configuration import results in a circular dependency.
    /// </summary>
    DS0199_ImportedConfigFileCircularDependency,
    /// <summary>
    /// Emitted when a file exports multiple namespaces.
    /// </summary>
    DS0200_MultipleExports,
    /// <summary>
    /// Emitted when a list literal constructed from an integer range does not use compile-time constant indices.
    /// </summary>
    DS0201_ListFromRangeNotCompileTimeConstant,
    /// <summary>
    /// Emitted when the designated application entry point has an invalid signature.
    /// </summary>
    DS0202_EntryPointInvalidSignature,
    /// <summary>
    /// Emitted when the condition of a conditional expression is a compile-time constant.
    /// </summary>
    DS0203_ConditionConstant,
    /// <summary>
    /// Emitted when an invalid type is used as a generic argument.
    /// </summary>
    DS0204_InvalidGenericArgument,
    /// <summary>
    /// Emitted when a project structure contains a circular dependency.
    /// </summary>
    DS0205_CircularProjectDependency,
    /// <summary>
    /// Emitted when the 'dc new' command is invoked with invalid arguments.
    /// </summary>
    DS0206_DCNewInvalidArguments,
    /// <summary>
    /// Emitted when the 'dc new' command specifies a project directory that is not empty.
    /// </summary>
    DS0207_DCNewNonEmptyDirectory,
    /// <summary>
    /// Emitted when a resource file passed to the compiler is invalid.
    /// </summary>
    DS0208_InvalidResourceFile,
    /// <summary>
    /// Emitted when the compiler produces code that is unverifiable.
    /// </summary>
    DS0209_UnverifiableCode,
    /// <summary>
    /// Emitted when the executable project defined in a project group file could not be found.
    /// </summary>
    DS0210_ProjectGroupExecutableInvalid,
    /// <summary>
    /// Emitted when AOT compilation is attempted for a system other than the current one.
    /// </summary>
    DS0211_CrossSystemAotCompilation,
    /// <summary>
    /// Emitted when unexpected or invalid arguments are passed to a command.
    /// </summary>
    DS0212_UnexpectedArgument,
    /// <summary>
    /// Emitted when a program that contains varargs function declarations is compiled ahead-of-time.
    /// </summary>
    DS0213_AotVarArgsFunction,
    /// <summary>
    /// Emitted when a program that contains a varargs function declaration is compiled for a platform other than Windows.
    /// </summary>
    DS0214_VarArgsNonWindows,
    /// <summary>
    /// Emitted when an 'extern' block contains another.
    /// </summary>
    DS0215_NestedExternalBlock,
    /// <summary>
    /// Emitted when an external extension caused an unhandled exception.
    /// </summary>
    DS0216_ExtensionThrewException,
    /// <summary>
    /// Emitted when a compiler directive is used as an expression.
    /// </summary>
    DS0217_CompilerDirectiveAsExpression,
    /// <summary>
    /// Emitted when a compiler directive is used that could not be found.
    /// </summary>
    DS0218_InvalidCompilerDirective,
    /// <summary>
    /// Emitted when a compiler directive is called with invalid arguments.
    /// </summary>
    DS0219_CompilerDirectiveInvalidArguments,
    /// <summary>
    /// Emitted when a compiler directive is called in an invalid scope.
    /// </summary>
    DS0220_CompilerDirectiveInvalidScope,
    /// <summary>
    /// Emitted when the target of an import compiler directive is invalid.
    /// </summary>
    DS0221_ImportDirectiveInvalidTarget,
    /// <summary>
    /// Emitted when a specified extension file (.dll) does not exist.
    /// </summary>
    DS0222_ExtensionFileNotFound,
    /// <summary>
    /// Emitted when a compiler extension is attempted to be initialized in an unsupported mode.
    /// </summary>
    DS0223_ExtensionUnsupportedMode,
    /// <summary>
    /// Emitted when the initializer of an extension package returns a nonzero status code.
    /// </summary>
    DS0224_ExtensionInitializerFailed,
    /// <summary>
    /// Emitted when an extension is loaded multiple times in different modes.
    /// </summary>
    DS0225_ExtensionDuplicateMode,
    /// <summary>
    /// Emitted when an exception occured while loading extensions from a remote source.
    /// </summary>
    DS0226_RemoteExtensionException,
    /// <summary>
    /// Emitted when the 'dc package install' command is used but the specified extension couldn't be found.
    /// </summary>
    DS0227_PackageInstallNotFound,
    /// <summary>
    /// Emitted when the 'dc package source' command is invoked with an invalid set of arguments.
    /// </summary>
    DS0228_PackageSourceInvalidArguments,
    /// <summary>
    /// Emitted when the 'dc package install' command is used but the specified extension is already installed.
    /// </summary>
    DS0229_PackageInstallAlreadyInstalled,
    /// <summary>
    /// Unused.
    /// </summary>
    DS0230_Unused,
    /// <summary>
    /// Emitted when a function declared as a <c>&lt;Predicate&gt;</c> does not return a value of type <see cref="bool"/>.
    /// </summary>
    DS0231_PredicateFunctionNotBoolean,
    /// <summary>
    /// Emitted when a specified document source cannot be found.
    /// </summary>
    DS0232_DocumentSourceNotFound,
    /// <summary>
    /// Emitted when multiple active document sources generate the same document name.
    /// </summary>
    DS0233_DocumentSourcesDuplicateDocumentName,
    /// <summary>
    /// Emitted when the compilation was terminated due to the maximum number of error messages being reached.
    /// </summary>
    DS0234_CompilationTerminated,
    /// <summary>
    /// Error code for informational messages caused by enabling the <see cref="DassieConfig.MeasureElapsedTime"/> option.
    /// </summary>
    DS0235_ElapsedTime,
    /// <summary>
    /// Emitted when the 'dc test' command is used in a directory that does not contain a project file.
    /// </summary>
    DS0236_DCTestNoProjectFile,
    /// <summary>
    /// Emitted when a target specified in a project group could not be found.
    /// </summary>
    DS0237_DeploymentTargetNotFound,
    /// <summary>
    /// Emitted when a deployment target exits with a nonzero exit code.
    /// </summary>
    DS0238_DeploymentTargetFailed,
    /// <summary>
    /// Emitted when the <c>&lt;Directory&gt;</c> deployment target is used but no path is specified.
    /// </summary>
    DS0239_DirectoryTargetPathRequired,
    /// <summary>
    /// Emitted when a source file is larger than the maximum allowed size of a <see cref="string"/>.
    /// </summary>
    DS0240_SourceFileTooLarge,
    /// <summary>
    /// Emitted when a project file declares a duplicate reference.
    /// </summary>
    DS0241_DuplicateReference,
    /// <summary>
    /// Emitted when a value type has a cyclic dependency on itself through one of its fields.
    /// </summary>
    DS0242_ValueTypeFieldCycle,
    /// <summary>
    /// Emitted when a module inherits from a type or implements an interface.
    /// </summary>
    DS0243_ModuleInheritance,
    /// <summary>
    /// Emitted when a module has invalid modifiers like 'open'.
    /// </summary>
    DS0244_ModuleInvalidModifiers,
    /// <summary>
    /// Emitted when a module appears in the inheritance list of a type.
    /// </summary>
    DS0245_ModuleInherited,
    /// <summary>
    /// Emitted when the 'dc test' command is invoked but the project to be tested contains no test modules.
    /// </summary>
    DS0246_DCTestNoTestModules,
    /// <summary>
    /// Emitted when the 'dc test' command is invoked with an invalid assembly.
    /// </summary>
    DS0247_DCTestAssemblyNotFound,
    /// <summary>
    /// Emitted when the 'dc test' command is invoked with an invalid module name.
    /// </summary>
    DS0248_DCTestInvalidModule,
    /// <summary>
    /// Emitted when 'dc test' is invoked on a project group.
    /// </summary>
    DS0249_DCTestProjectGroup,
    /// <summary>
    /// Emitted when the hidden 'dc compile' command is used. This command only exists to provide help details for the 'dc &lt;Files&gt;' command.
    /// </summary>
    DS0250_DCCompileInvoked,
    /// <summary>
    /// Emitted when an invalid subsystem is specified.
    /// </summary>
    DS0251_InvalidSubsystem,
    /// <summary>
    /// Emitted when the &lt;EntryPoint&gt; attribute is applied to a function inside of a library.
    /// </summary>
    DS0252_EntryPointInNonExecutableProgram,
    /// <summary>
    /// Emitted when an invalid valid property (from a project file or the global configuration) is referenced in a call to the 'dc config' command.
    /// </summary>
    DS0253_DCConfigInvalidProperty,
    /// <summary>
    /// Emitted when 'dc config' was invoked to modify a property, but the data type of the property is not supported.
    /// </summary>
    DS0254_DCConfigUnsupportedDataType,
    /// <summary>
    /// Emitted when the value of a property modified through 'dc config' is invalid.
    /// </summary>
    DS0255_DCConfigInvalidValue,
    /// <summary>
    /// Emitted when the global configuration file contains a namespace not associated with any extension or a property not defined by an extension.
    /// </summary>
    DS0256_GlobalConfigInvalidElement,
    /// <summary>
    /// Emitted when the global configuration file is not a valid XML document.
    /// </summary>
    DS0257_GlobalConfigFileMalformed,
    /// <summary>
    /// Emitted when the value of a global property is invalid for the data type of the property.
    /// </summary>
    DS0258_GlobalConfigPropertyValueMalformed,
    /// <summary>
    /// Emitted when the 'dc scratchpad load' command is used and the editor is set to 'default'.
    /// </summary>
    DS0259_DCScratchpadLoadDefaultEditor,
    /// <summary>
    /// Emitted when the result of a compiler directive invocation is not a compile-time constant.
    /// </summary>
    DS0260_CompilerDirectiveResultNotConstant,
    /// <summary>
    /// Emitted when the value of the 'core.locations.extensions' global property is set to an invalid path.
    /// </summary>
    DS0261_ExtensionsLocationPropertyInvalidPath,
    /// <summary>
    /// Emitted when an action that is referenced in a build profile does not exist.
    /// </summary>
    DS0262_DCBuildInvalidActionName,
    /// <summary>
    /// Emitted when a build action is executed in an invalid mode.
    /// </summary>
    DS0263_BuildActionInvalidMode,
    /// <summary>
    /// Emitted when a build action ends with a nonzero exit code.
    /// </summary>
    DS0264_BuildActionFailed
}