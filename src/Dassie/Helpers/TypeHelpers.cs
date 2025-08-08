using Dassie.CodeGeneration.Helpers;
using Dassie.Core;
using Dassie.Meta;
using Dassie.Parser;
using Dassie.Symbols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Dassie.Helpers;

#pragma warning disable IDE0305

/// <summary>
/// Provides helper methods regarding data types.
/// </summary>
internal static class TypeHelpers
{
    /// <summary>
    /// Removes the 'ByRef' modifier from a type if the specified type is a ByRef type.
    /// </summary>
    /// <param name="t">The type to remove the modifier from.</param>
    /// <returns>A type representing <paramref name="t"/> without a ByRef modifier.</returns>
    public static Type RemoveByRef(this Type t)
    {
        if (t == null)
            return t;

        if (t.IsByRef /*|| t.IsByRefLike*/)
            t = t.GetElementType();

        return t;
    }

    /// <summary>
    /// Gets the IL instruction to load a value of the specified type indirectly.
    /// </summary>
    /// <param name="t">The type to load indirectly.</param>
    /// <returns>The <c>ldind.X</c> opcode corresponding to the specified type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static OpCode GetLoadIndirectOpCode(this Type t)
    {
        if (t == typeof(byte) || t == typeof(sbyte))
            return OpCodes.Ldind_I1;

        if (t == typeof(short) || t == typeof(ushort))
            return OpCodes.Ldind_I2;

        if (t == typeof(int) || t == typeof(uint))
            return OpCodes.Ldind_I4;

        if (t == typeof(long) || t == typeof(ulong))
            return OpCodes.Ldind_I8;

        if (t == typeof(float))
            return OpCodes.Ldind_R4;

        if (t == typeof(double))
            return OpCodes.Ldind_R8;

        if (t == typeof(nint) || t == typeof(nuint))
            return OpCodes.Ldind_I;

        return OpCodes.Ldind_Ref;
    }

    /// <summary>
    /// Gets the IL instruction to set a value of the specified type indirectly.
    /// </summary>
    /// <param name="t">The type to set indirectly.</param>
    /// <returns>The <c>stind.X</c> opcode corresponding to the specified type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static OpCode GetSetIndirectOpCode(this Type t)
    {
        if (t == typeof(byte) || t == typeof(sbyte))
            return OpCodes.Stind_I1;

        if (t == typeof(short) || t == typeof(ushort))
            return OpCodes.Stind_I2;

        if (t == typeof(int) || t == typeof(uint))
            return OpCodes.Stind_I4;

        if (t == typeof(long) || t == typeof(ulong))
            return OpCodes.Stind_I8;

        if (t == typeof(float))
            return OpCodes.Stind_R4;

        if (t == typeof(double))
            return OpCodes.Stind_R8;

        if (t == typeof(nint) || t == typeof(nuint))
            return OpCodes.Stind_I;

        return OpCodes.Stind_Ref;
    }

    /// <summary>
    /// Gets the IL instruction to set an array element.
    /// </summary>
    /// <param name="t">The element type of the array.</param>
    /// <returns>The <c>stelem.X</c> opcode corresponding to the specified type.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static OpCode GetStelemOpCode(this Type t)
    {
        if (t == typeof(byte) || t == typeof(sbyte))
            return OpCodes.Stelem_I1;

        if (t == typeof(short) || t == typeof(ushort))
            return OpCodes.Stelem_I2;

        if (t == typeof(int) || t == typeof(uint))
            return OpCodes.Stelem_I4;

        if (t == typeof(long) || t == typeof(ulong))
            return OpCodes.Stelem_I8;

        if (t == typeof(float))
            return OpCodes.Stelem_R4;

        if (t == typeof(double))
            return OpCodes.Stelem_R8;

        if (t == typeof(nint) || t == typeof(nuint))
            return OpCodes.Stelem_I;

        return OpCodes.Stelem_Ref;
    }

    /// <summary>
    /// Emits an <c>ldind</c> instruction if the specified type is a ByRef type.
    /// </summary>
    /// <param name="t">The type to load indirectly.</param>
    public static void LoadIndirectlyIfPossible(this Type t)
    {
        if (!t.IsByRef /*&& !t.IsByRefLike*/)
            return;

        if (!IsNumericType(t.RemoveByRef()))
            return;

        CurrentMethod.IL.Emit(t.RemoveByRef().GetLoadIndirectOpCode());
    }

    /// <summary>
    /// Checks wheter a specified type can be used in a place that requires a boolean value.
    /// </summary>
    /// <param name="t">The type to check</param>
    /// <returns>Returns <see langword="true"/> if at least one of the following is <see langword="true"/>:
    /// <list type="bullet">
    ///     <item><paramref name="t"/> is of type <see cref="bool"/>.</item>
    ///     <item><paramref name="t"/> defines an implicit conversion into <see cref="bool"/></item>
    ///     <item><paramref name="t"/> overloads the <c>op_True</c> and <c>op_False</c> operators.</item>
    /// </list>
    /// </returns>
    public static bool IsBoolean(Type t)
    {
        if (t == typeof(bool))
            return true;

        // Should we allow implicit conversions into other types that are IsBoolean() instead of just bool?
        if (t.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == t)
            .Any())
            return true;

        return t.GetMethods()
            .Where(m => m.Name == "op_True")
            .Where(m => m.ReturnType == typeof(bool))
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == t)
            .Any();
    }

    /// <summary>
    /// Checks if <paramref name="t"/> is a <see cref="bool"/> or can be converted into one and then performs the conversion if necessary. If <paramref name="t"/> is incompatible with <see cref="bool"/>, an error is thrown.
    /// </summary>
    /// <param name="t">The type of the conversion.</param>
    /// <param name="line">The line in the code used for error messages.</param>
    /// <param name="col">The column in the code used for error messages.</param>
    /// <param name="length">The length of the symbol causing a code error..</param>
    /// <param name="throwError">Wheter to throw an error if the type could not be converted.</param>
    public static void EnsureBoolean(Type t, int line = 0, int col = 0, int length = 0, bool throwError = true)
    {
        if (!IsBoolean(t))
        {
            if (!throwError)
                return;

            EmitErrorMessage(
                line, col, length,
                DS0038_ConditionalExpressionClauseNotBoolean,
                $"The type '{t.FullName}' cannot be converted to type '{typeof(bool).FullName}'.");
        }

        if (t != typeof(bool))
            EmitBoolConversion(t);
    }

    /// <summary>
    /// Checks wheter type <paramref name="from"/> can be converted into type <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The type to convert from.</param>
    /// <param name="to">The type to convert into.</param>
    /// <returns>Wheter or not a conversion from <paramref name="from"/> to <paramref name="to"/> exists.</returns>
    public static bool CanBeConverted(Type from, Type to)
    {
        if (from.IsByRef)
            from = from.GetElementType();

        if (to.IsByRef)
            to = to.GetElementType();

        if (from == to)
            return true;

        if (from == typeof(nint) && to.IsFunctionPointer)
            return true;

        if (from.IsFunctionPointer && to == typeof(nint))
            return true;

        if (to == typeof(object))
            return true;

        if (IsNumericType(from) && IsNumericType(to))
            return true;

        if (from.IsAssignableTo(to))
            return true;

        if (from.IsAssignableFrom(to))
            return true;

        if (from.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from)
            .Any())
            return true;

        if (from.GetMethods()
            .Where(m => m.Name == "op_Explicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from)
            .Any())
            return true;

        if (to.GetMethods()
            .Where(m => m.Name == "op_Implicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from)
            .Any())
            return true;

        if (to.GetMethods()
            .Where(m => m.Name == "op_Explicit")
            .Where(m => m.ReturnType == to)
            .Where(m => m.GetParameters().Length == 1)
            .Where(m => m.GetParameters()[0].ParameterType == from)
            .Any())
            return true;

        return false;
    }

    public static bool IsNumericType(Type type)
    {
        Type[] numerics =
        [
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(nint),
            typeof(nuint),
            typeof(char)
        ];

        return numerics.Contains(type.RemoveByRef());
    }

    public static bool IsIntegerType(Type type)
    {
        Type[] numerics =
        [
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(nint),
            typeof(nuint),
            typeof(char)
        ];

        return numerics.Contains(type.RemoveByRef());
    }

    public static bool IsUnsignedIntegerType(Type type)
    {
        Type[] numerics =
        [
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(nuint)
        ];

        return numerics.Contains(type.RemoveByRef());
    }

    public static bool IsFloatingPointType(Type type)
    {
        Type[] floats =
        [
            typeof(float),
            typeof(double)
        ];

        return floats.Contains(type.RemoveByRef());
    }

    public static bool IsDelegateType(Type type) => type.BaseType == typeof(Delegate) || IsMulticastDelegate(type);
    public static bool IsMulticastDelegate(Type type) => type.BaseType == typeof(MulticastDelegate);

    public static Type GetEnumeratedType(this Type type) =>
        (type?.GetElementType() ?? (typeof(IEnumerable).IsAssignableFrom(type)
            ? type.GenericTypeArguments.FirstOrDefault()
            : null))!;

    private static readonly Dictionary<string, string> IntrinsicAliases = new()
    {
        ["System.IntPtr"] = "native",
        ["System.UIntPtr"] = "unative",
        ["System.SByte"] = "int8",
        ["System.Byte"] = "uint8",
        ["System.Int16"] = "int16",
        ["System.UInt16"] = "uint16",
        ["System.Int32"] = "int",
        ["System.UInt32"] = "uint",
        ["System.Int64"] = "int64",
        ["System.UInt64"] = "uint64",
        ["System.Int128"] = "int128",
        ["System.UInt128"] = "uint128",
        ["System.Half"] = "float16",
        ["System.Single"] = "float32",
        ["System.Double"] = "float64",
        ["System.Decimal"] = "decimal",
        ["System.Boolean"] = "bool",
        ["System.String"] = "string",
        ["System.Char"] = "char",
        ["System.Void"] = "null",
        ["System.Object"] = "object"
    };

    private static string GetTypeNameOrAlias(Type type)
    {
        string name = type.FullName ?? type.Name;

        if (type.IsArray || type.IsByRef || type.IsPointer)
        {
            type = type.GetElementType();
            name = type.FullName ?? type.Name;
        }

        if (type.IsGenericType)
            name = name.Split('`')[0];

        if (IntrinsicAliases.TryGetValue(name, out string alias))
            return alias;

        return name.Split('.').Last();
    }

    /// <summary>
    /// Formats the specified type name to be used in error messages.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>The formatted type name.</returns>
    public static string Format(Type type)
    {
        StringBuilder name = new(GetTypeNameOrAlias(type));
        Type elem = type;

        if (type.IsArray || type.IsByRef || type.IsPointer)
            elem = type.GetElementType();

        if (elem.IsGenericType)
        {
            name.Clear();
            name.Append(GetTypeNameOrAlias(elem));
            name.Append('[');
            
            foreach (Type typeArg in elem.GetGenericArguments()[..^1])
            {
                name.Append(TypeName(typeArg));
                name.Append(", ");
            }

            name.Append(TypeName(type.GetGenericArguments().Last()));
            name.Append(']');
        }

        if (type.IsByRef)
            name = new($"Ref[{name}]");

        if (type.IsPointer)
        {
            StringBuilder inner = name;
            name = new("Ptr");
            if (type.GetElementType() != typeof(void))
                name.Append($"[{inner}]");
        }

        if (type.IsArray)
        {
            if (type.GetArrayRank() == 1)
                name = new($"Vector[{name}]");
            else
                name = new($"Array[{name}, {type.GetArrayRank()}]");
        }

        return name.ToString();
    }

    public static string GetOpenGenericTypeString(string closedGenericTypeString)
    {
        if (!closedGenericTypeString.Contains('['))
            return closedGenericTypeString;

        if (!closedGenericTypeString.Contains('`'))
        {
            int _brIndex = closedGenericTypeString.IndexOf('[');
            return $"{closedGenericTypeString[0.._brIndex]}";
        }

        string[] parts = closedGenericTypeString.Split('`');
        int braceIndex = parts[1].IndexOf('[');
        int typeParamCount = int.Parse(parts[1][0..braceIndex]);
        return $"{parts[0]}`{typeParamCount}{closedGenericTypeString.Split(']').Last()}";
    }

    // List<int> -> List<>
    public static Type DeconstructGenericType(Type type)
    {
        if (!type.IsGenericType)
            return type;

        string typeName = GetOpenGenericTypeString(type.AssemblyQualifiedName);
        return SymbolResolver.ResolveTypeName(typeName, doNotFillGenericTypeDefinition: true);
    }

    /// <summary>
    /// Checks if the specified generic type can be initialized with the specified type arguments.
    /// </summary>
    /// <param name="genericType">The uninitialized generic type.</param>
    /// <param name="genericArgs">The generic arguments to initialize the specified generic type with.</param>
    /// <param name="row"/>
    /// <param name="col"/>
    /// <param name="len"/>
    /// <param name="emitErrors"/>
    /// <returns>Wheter or not the specified generic type can be initialized with the specified type arguments.</returns>
    public static bool CheckGenericTypeCompatibility(Type genericType, Generics.GenericArgumentContext[] genericArgs, int row = 0, int col = 0, int len = 0, bool emitErrors = true)
    {
        if (genericType == null)
            return true;

        if (!genericType.IsGenericType)
            return true;

        if (!genericType.IsGenericTypeDefinition)
            return true;

        Type[] typeParams = genericType.GetGenericArguments();
        return CheckGenericCompatibility(genericType.FullName, true, typeParams, genericArgs, row, col, len, emitErrors);
    }

    /// <summary>
    /// Checks if the specified generic method can be initialized with the specified type arguments.
    /// </summary>
    /// <param name="method">The uninitialized generic method.</param>
    /// <param name="genericArgs">The generic arguments to initialize the specified generic type with.</param>
    /// <param name="row"/>
    /// <param name="col"/>
    /// <param name="len"/>
    /// <param name="emitErrors"/>
    /// <returns>Wheter or not the specified generic type can be initialized with the specified type arguments.</returns>
    public static bool CheckGenericMethodCompatibility(MethodBase method, Generics.GenericArgumentContext[] genericArgs, int row = 0, int col = 0, int len = 0, bool emitErrors = true)
    {
        if (!method.IsGenericMethod)
            return true;

        if (!method.IsGenericMethodDefinition)
            return true;

        Type[] typeParams = method.GetGenericArguments();
        return CheckGenericCompatibility(method.Name, false, typeParams, genericArgs, row, col, len, emitErrors);
    }

    public static void ThrowErrorForInvalidTypeArgumentCount(int row, int col, int len, bool isType, string name, int paramCount, int argCount)
    {
        EmitErrorMessage(
            row, col, len,
            DS0107_GenericTypeConstraintViolation,
            $"The generic {(isType ? "type" : "function")} '{name}' requires {paramCount} type argument{(paramCount > 1 ? "s" : "")}, but {(argCount == 0 ? "none" : argCount.ToString())} {(argCount == 1 ? "was" : "were")} specified.");
    }

    private static bool CheckGenericCompatibility(string name, bool isType, Type[] parameters, Generics.GenericArgumentContext[] arguments, int row, int col, int len, bool emitErrors)
    {
        if (arguments == null || parameters.Length != arguments.Length)
        {
            arguments ??= [];
            ThrowErrorForInvalidTypeArgumentCount(row, col, len, isType, name, parameters.Length, arguments.Length);
            return false;
        }

        bool result = true;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (arguments[i].Type == TypeContext.Current.Builder)
                continue;

            if (!MeetsGenericConstraints(parameters[i], arguments[i], row, col, len, emitErrors))
                result = false;
        }

        return result;
    }

    /// <summary>
    /// Checks wheter or not <paramref name="arg"/> can be used as an argument for the generic parameter <paramref name="param"/>.
    /// </summary>
    /// <param name="param"></param>
    /// <param name="arg"></param>
    /// <param name="row"/>
    /// <param name="col"/>
    /// <param name="len"/>
    /// <param name="throwErrors"/>
    /// <returns></returns>
    private static bool MeetsGenericConstraints(Type param, Generics.GenericArgumentContext arg, int row, int col, int len, bool throwErrors)
    {
        if (!param.IsGenericParameter)
            return true;

        if (arg.Type == typeof(void))
        {
            if (throwErrors)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0203_InvalidGenericArgument,
                    $"Invalid generic argument '{TypeName(arg.Type)}': 'null' cannot be used as a type argument.");
            }

            return true;
        }

        if (arg.Type.IsByRef)
        {
            if (throwErrors)
            {
                EmitErrorMessage(
                    row, col, len,
                    DS0203_InvalidGenericArgument,
                    $"Invalid generic argument '{TypeName(arg.Type)}': References cannot be used as type arguments.");
            }

            return true;
        }

        string errMsgStart = $"Generic argument '{Format(arg.Type)}' is incompatible with generic parameter '{Format(param)}': ";
        GenericParameterAttributes attributes = param.GenericParameterAttributes;

        if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
        {
            if (!arg.Type.IsClass)
            {
                if (throwErrors)
                {
                    EmitErrorMessage(
                        row, col, len,
                        DS0107_GenericTypeConstraintViolation,
                        $"{errMsgStart}'{Format(param)}' only allows reference types.");
                }

                return false;
            }
        }

        if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
        {
            if (!arg.Type.IsValueType)
            {
                if (throwErrors)
                {
                    EmitErrorMessage(
                        row, col, len,
                        DS0107_GenericTypeConstraintViolation,
                        $"{errMsgStart}'{Format(param)}' only allows value types.");
                }

                return false;
            }
        }

        if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
        {
            if (arg.Type.GetConstructor([]) == null && !arg.Type.IsValueType)
            {
                if (throwErrors)
                {
                    EmitErrorMessage(
                        row, col, len,
                        DS0107_GenericTypeConstraintViolation,
                        $"{errMsgStart}The generic argument needs to define a parameterless constructor.");
                }

                return false;
            }
        }

        bool result = true;

        foreach (Type constraint in param.GetGenericParameterConstraints())
        {
            try
            {
                if (!constraint.IsAssignableFrom(arg.Type) && !CanBeConverted(arg.Type, constraint))
                {
                    EmitErrorMessage(
                        row, col, len,
                        DS0107_GenericTypeConstraintViolation,
                        $"{errMsgStart}'{Format(arg.Type)}' violates constraint '{Format(constraint)}'.");

                    result = false;
                }
            }
            catch (NotSupportedException) { }
        }

        return result;
    }

    public static GenericParameterContext BuildTypeParameter(DassieParser.Generic_parameterContext context)
    {
        string name = context.Identifier().GetIdentifier();
        GenericParameterAttributes attribs = GenericParameterAttributes.None;
        List<Type> interfaceConstraints = [];
        Type baseTypeConstraint = null;

        if (context.generic_parameter_variance() != null)
        {
            if (context.generic_parameter_variance().Plus() != null)
                attribs |= GenericParameterAttributes.Covariant;

            if (context.generic_parameter_variance().Minus() != null)
                attribs |= GenericParameterAttributes.Contravariant;

            if (context.generic_parameter_variance().Equals() == null && !TypeContext.Current.Builder.IsInterface)
            {
                EmitErrorMessage(
                    context.generic_parameter_variance().Start.Line,
                    context.generic_parameter_variance().Start.Column,
                    context.generic_parameter_variance().GetText().Length,
                    DS0117_VarianceModifierOnConcreteType,
                    $"The variance modifier '{context.generic_parameter_variance().GetText()}' is invalid on type '{Format(TypeContext.Current.Builder)}'. Only type parameters of template types can have variance modifiers.");
            }
        }

        if (context.generic_parameter_attribute() != null)
        {
            foreach (var attrib in context.generic_parameter_attribute())
            {
                bool duplicate = false;

                if (attrib.Ref() != null)
                {
                    if (attribs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                        duplicate = true;

                    attribs |= GenericParameterAttributes.ReferenceTypeConstraint;
                }

                if (attrib.Val() != null)
                {
                    if (attribs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                        duplicate = true;

                    attribs |= GenericParameterAttributes.NotNullableValueTypeConstraint;
                }

                if (attrib.Default() != null)
                {
                    if (attribs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                    {
                        duplicate = true;
                        continue;
                    }

                    attribs |= GenericParameterAttributes.DefaultConstructorConstraint;
                }

                if (duplicate)
                {
                    EmitErrorMessage(
                        attrib.Start.Line,
                        attrib.Start.Column,
                        attrib.GetText().Length,
                        DS0110_DuplicateTypeParameterAttributes,
                        $"Duplicate attribute '{attrib.GetText()}'.");
                }
            }

            if (attribs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attribs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                EmitErrorMessage(
                    context.Start.Line,
                    context.Start.Column,
                    context.GetText().Length,
                    DS0113_InvalidTypeParameterAttributes,
                    $"The type parameter attributes 'ref' and 'val' are mutually exclusive.");
            }
        }

        bool removeNone = context.generic_parameter_variance() != null
            || context.generic_parameter_attribute() != null;

        if (removeNone)
            attribs &= ~GenericParameterAttributes.None;

        Type valueType = null;

        if (context.type_name() != null)
        {
            foreach (var type in context.type_name())
            {
                Type constraint = SymbolResolver.ResolveTypeName(type);

                if (context.Single_Quote() != null || context.Double_Quote() != null)
                {
                    valueType = constraint;
                    continue;
                }
                
                if (constraint.IsClass)
                {
                    if (baseTypeConstraint != null)
                    {
                        EmitErrorMessage(
                            type.Start.Line,
                            type.Start.Column,
                            type.GetText().Length,
                            DS0111_DuplicateTypeParameterConstraint,
                            $"Duplicate base type constraint '{Format(constraint)}': A generic type parameter can only define one base type.");

                        continue;
                    }

                    baseTypeConstraint = constraint;
                    continue;
                }

                if (interfaceConstraints.Contains(constraint))
                {
                    EmitErrorMessage(
                        type.Start.Line,
                        type.Start.Column,
                        type.GetText().Length,
                        DS0111_DuplicateTypeParameterConstraint,
                        $"Duplicate type constraint '{Format(constraint)}'");

                    continue;
                }

                interfaceConstraints.Add(constraint);
            }
        }

        return new()
        {
            Name = name,
            Attributes = attribs,
            InterfaceConstraints = interfaceConstraints,
            BaseTypeConstraint = baseTypeConstraint,
            IsCompileTimeConstant = context.Single_Quote() != null,
            IsRuntimeValue = context.Double_Quote() != null,
            ValueType = valueType
        };
    }

    public static List<Type> GetInheritedTypes(DassieParser.Inheritance_listContext context, bool noErrors = false)
    {
        List<Type> types = [];
        int classCount = 0;

        foreach (DassieParser.Type_nameContext typeName in context.type_name())
        {
            Type t = SymbolResolver.ResolveTypeName(typeName, noErrors: noErrors);

            if (t != null)
            {
                types.Add(t);

                if (t.IsClass)
                    classCount++;
            }

            if (classCount > 1)
            {
                EmitErrorMessage(
                    typeName.Start.Line,
                    typeName.Start.Column,
                    typeName.GetText().Length,
                    DS0051_MoreThanOneClassInInheritanceList,
                    "A type can only extend one base type."
                    );
            }
        }

        return types;
    }

    public static bool IsValueTuple(Type t) => t.IsGenericType && t.FullName.StartsWith("System.ValueTuple`");

    public static string TypeName(Type t)
    {
        bool isUnion = false;

        if (t is not TypeBuilder)
        {
            try
            {
                isUnion = t.GetCustomAttribute<Union>() != null;
            }
            catch (Exception)
            {
                isUnion = false;
            }
        }
        else
            isUnion = t.Name.StartsWith(SymbolNameGenerator.GetInlineUnionTypeName(0)[..^1]);

        if (isUnion)
        {
            IEnumerable<string> flagNames = t.GetMethods().Where(m => m.Name.StartsWith("set_"))
                .Select(m => TypeName(m.GetParameters()[0].ParameterType));

            return $"({string.Join(" | ", flagNames)})";
        }

        return Format(t);
    }

    public static List<(Type Type, string Name)> GetTupleItems(DassieParser.Type_nameContext name, bool noEmitFragments, bool noEmitDS0149)
        => GetTupleOrUnionItems(name, "Tuple", noEmitFragments, noEmitDS0149);

    public static List<(Type Type, string Name)> GetUnionItems(DassieParser.Type_nameContext name, bool noEmitFragments, bool noEmitDS0149)
    => GetTupleOrUnionItems(name, "Union", noEmitFragments, noEmitDS0149);

    private static List<(Type Type, string Name)> GetTupleOrUnionItems(DassieParser.Type_nameContext name, string kind, bool noEmitFragments, bool noEmitDS0149)
    {
        List<(Type Type, string Name)> partTypes = [];
        foreach (DassieParser.Union_or_tuple_type_memberContext unionMember in name.union_or_tuple_type_member())
        {
            Type type = SymbolResolver.ResolveTypeName(unionMember.type_name(), noEmitFragments, noEmitDS0149);
            string unionMemberName = null;

            if (partTypes.Any(p => p.Name != null && p.Name == unionMemberName))
            {
                EmitErrorMessage(
                    unionMember.Identifier().Symbol.Line,
                    unionMember.Identifier().Symbol.Column,
                    unionMemberName.Length,
                    DS0182_UnionTypeDuplicateTagName,
                    $"{kind} type contains duplicate tag name '{unionMemberName}'.");
            }

            if (partTypes.Any(p => p.Type == type))
            {
                EmitErrorMessage(
                    unionMember.type_name().Start.Line,
                    unionMember.type_name().Start.Column,
                    unionMember.type_name().GetText().Length,
                    DS0183_UnionTypeDuplicateTagType,
                    $"{kind} type contains multiple tags of type '{unionMemberName}'.");
            }

            if (unionMember.Identifier() != null)
                unionMemberName = unionMember.Identifier().GetIdentifier();

            partTypes.Add((type, unionMemberName));
        }

        int namedTags = partTypes.Count(p => p.Name != null);

        if (!(namedTags == 0 || partTypes.Count == namedTags))
        {
            EmitErrorMessage(
                name.Start.Line,
                name.Start.Column,
                name.GetText().Length,
                DS0184_UnionTypeMixedTags,
                $"{kind} type cannot contain mixed named and unnamed tags. All tags need to be either named or unnamed.");
        }

        return partTypes;
    }

    public static Type GetValueTupleType(Type[] elementTypes)
    {
        if (elementTypes.Length <= 8)
            return CreateValueTupleType(elementTypes);

        Type[] mainTypes = elementTypes.Take(7).ToArray();
        Type[] remainingTypes = elementTypes.Skip(7).ToArray();

        Type restType = GetValueTupleType(remainingTypes);

        Type[] allTypes = mainTypes.Concat([restType]).ToArray();
        return CreateValueTupleType(allTypes);
    }

    private static Type CreateValueTupleType(Type[] elementTypes)
    {
        return elementTypes.Length switch
        {
            1 => typeof(ValueTuple<>).MakeGenericType(elementTypes),
            2 => typeof(ValueTuple<,>).MakeGenericType(elementTypes),
            3 => typeof(ValueTuple<,,>).MakeGenericType(elementTypes),
            4 => typeof(ValueTuple<,,,>).MakeGenericType(elementTypes),
            5 => typeof(ValueTuple<,,,,>).MakeGenericType(elementTypes),
            6 => typeof(ValueTuple<,,,,,>).MakeGenericType(elementTypes),
            7 => typeof(ValueTuple<,,,,,,>).MakeGenericType(elementTypes),
            8 => typeof(ValueTuple<,,,,,,,>).MakeGenericType(elementTypes),
            _ => throw new ArgumentException("Invalid number of element types."),
        };
    }

    public static string GetTypeName(DassieParser.TypeContext context)
        => $"{(string.IsNullOrEmpty(CurrentFile.ExportedNamespace) ? "" : $"{CurrentFile.ExportedNamespace}.")}{context.Identifier().GetIdentifier()}";
}