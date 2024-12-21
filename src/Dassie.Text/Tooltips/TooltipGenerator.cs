using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Dassie.Text.Tooltips;

/// <summary>
/// Provides functionality for generating tooltips for objects to be used in editors.
/// </summary>
public static class TooltipGenerator
{
    /// <summary>
    /// Generates a tooltip for a local value.
    /// </summary>
    /// <param name="name">The name of the local.</param>
    /// <param name="mutable">Wheter the local is mutable.</param>
    /// <param name="local">The local to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Local(string name, bool mutable, LocalVariableInfo local)
    {
        ObservableCollection<Word> words =
        [
            BuildWord(mutable ? "var " : "val ", Color.Word),
            BuildWord(name, Color.LocalValue),
            BuildWord(": "),
            .. Type(local.LocalType.GetTypeInfo(), false, true, true).Words,
        ];

        return new()
        {
            Words = words,
            IconResourceName = "LocalVariable"
        };
    }

    /// <summary>
    /// Generates a tooltip for a parameter.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="type">The type of the parameter.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Parameter(string name, Type type)
    {
        ObservableCollection<Word> words =
        [
            BuildWord(name, Color.LocalValue),
            BuildWord(": "),
            .. Type(type.GetTypeInfo(), false, true, true).Words,
        ];

        return new()
        {
            Words = words,
            IconResourceName = "LocalVariable"
        };
    }

    /// <summary>
    /// Generates a tooltip for a property.
    /// </summary>
    /// <param name="property">The property to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Property(PropertyInfo property)
    {
        ObservableCollection<Word> words =
        [
            BuildWord(property.Name, Color.Property),
            BuildWord(": "),
            .. Type(property.PropertyType.GetTypeInfo(), false, true, true).Words,
        ];

        return new()
        {
            Words = words,
            IconResourceName = "Property"
        };
    }

    /// <summary>
    /// Generates a tooltip for a field.
    /// </summary>
    /// <param name="field">The field to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Field(FieldInfo field)
    {
        ObservableCollection<Word> words = [];

        if (field.IsStatic)
            words.Add(BuildWord("static ", Color.Word));

        words.Add(BuildWord(field.Name, Color.Field));
        words.Add(BuildWord(": "));

        foreach (Word word in Type(field.FieldType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        return new()
        {
            Words = words,
            IconResourceName = "FieldPublic"
        };
    }

    /// <summary>
    /// Generates a tooltip for an enumeration member.
    /// </summary>
    /// <param name="field">The enum member to generate a tooltip for.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip EnumField(FieldInfo field)
    {
        ObservableCollection<Word> words =
        [
            BuildWord(field.Name, Color.EnumField),
            BuildWord(": "),
            BuildWord(field.FieldType.Name, ColorForType(field.FieldType.GetTypeInfo()))
        ];

        return new()
        {
            Words = words,
            IconResourceName = "EnumerationItemPublic"
        };
    }

    /// <summary>
    /// Generates a tooltip for a constructor.
    /// </summary>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Constructor(Type type, List<(Type Type, string Name)> parameters)
    {
        ObservableCollection<Word> words =
        [
            BuildWord(type.Name, ColorForType(type.GetTypeInfo()))
        ];

        if (parameters.Count > 0)
        {
            words.Add(BuildWord(" ("));

            foreach ((Type _type, string _name) in parameters.ToArray()[..^1])
            {
                words.Add(BuildWord(_name, Color.LocalValue));
                words.Add(BuildWord(": "));

                foreach (Word word in Type(_type.GetTypeInfo(), false, true, true).Words)
                    words.Add(word);

                words.Add(BuildWord(", "));
            }

            words.Add(BuildWord(parameters.Last().Name, Color.LocalValue));
            words.Add(BuildWord(": "));

            foreach (Word word in Type(parameters.Last().Type.GetTypeInfo(), false, true, true).Words)
                words.Add(word);

            words.Add(BuildWord(")"));
        }

        return new()
        {
            Words = words,
            IconResourceName = "Method"
        };
    }

    /// <summary>
    /// Generates a tooltip for a constructor.
    /// </summary>
    /// <param name="ctor">The ConstructorInfo representing the constructor.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Constructor(ConstructorInfo ctor)
    {
        ObservableCollection<Word> words =
        [
            BuildWord(ctor.DeclaringType.Name, ColorForType(ctor.DeclaringType.GetTypeInfo()))
        ];

        if (ctor.GetParameters().Length > 0)
        {
            words.Add(BuildWord(" ("));

            foreach (ParameterInfo param in ctor.GetParameters()[..^1])
            {
                words.Add(BuildWord(param.Name, Color.LocalValue));
                words.Add(BuildWord(": "));

                foreach (Word word in Type(param.ParameterType.GetTypeInfo(), false, true, true).Words)
                    words.Add(word);

                words.Add(BuildWord(", "));
            }

            words.Add(BuildWord(ctor.GetParameters()[0].Name, Color.LocalValue));
            words.Add(BuildWord(": "));

            foreach (Word word in Type(ctor.GetParameters()[0].ParameterType.GetTypeInfo(), false, true, true).Words)
                words.Add(word);

            words.Add(BuildWord(")"));
        }

        return new()
        {
            Words = words,
            IconResourceName = "Method"
        };
    }

    /// <summary>
    /// Generates a tooltip for a function.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="returnType"></param>
    /// <param name="parameters"></param>
    /// <param name="intrinsic"></param>
    /// <param name="translatedWords"></param>
    /// <returns></returns>
    public static Tooltip Function(string name, Type returnType, Parameter[] parameters, bool intrinsic = false, Dictionary<string, string> translatedWords = null)
    {
        ObservableCollection<Word> words = [];

        if (intrinsic)
            words.Add(BuildWord("[intrinsic] "));

        words.Add(BuildWord(name, intrinsic ? Color.IntrinsicFunction : Color.Function));

        if (parameters.Length > 0)
        {
            words.Add(BuildWord(" ("));

            foreach (Parameter param in parameters[..^1])
            {
                words.Add(BuildWord(param.Name, Color.LocalValue));
                words.Add(BuildWord(": "));

                foreach (Word word in Type(param.Type.GetTypeInfo(), false, true, true).Words)
                    words.Add(word);

                words.Add(BuildWord(", "));
            }

            words.Add(BuildWord(parameters[0].Name, Color.LocalValue));
            words.Add(BuildWord(": "));

            foreach (Word word in Type(parameters[0].Type.GetTypeInfo(), false, true, true).Words)
                words.Add(word);

            words.Add(BuildWord(")"));
        }

        words.Add(BuildWord(": "));
        foreach (Word word in Type((returnType ?? typeof(Unknown)).GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        if (parameters.Any(p => !string.IsNullOrEmpty(p.Constraint)))
        {
            string constraintsWord = "Constraints";
            if (translatedWords != null && translatedWords.TryGetValue("Constraints", out string value))
                constraintsWord = value;

            words.Add(Word.LineBreak);
            words.Add(new()
            {
                Text = $"{constraintsWord}\r\n",
                Fragment = new()
            });

            foreach (Parameter p in parameters)
            {
                if (string.IsNullOrEmpty(p.Constraint))
                    continue;

                words.Add(new()
                {
                    Text = $"\t• {p.Name}: {p.Constraint}\r\n",
                    Fragment = new()
                });
            }
        }

        return new()
        {
            Words = words,
            IconResourceName = "Method"
        };
    }

    /// <summary>
    /// Generates a tooltip for a function.
    /// </summary>
    /// <param name="method">The MethodInfo representing the function.</param>
    /// <param name="intrinsic"></param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Function(MethodInfo method, bool intrinsic = false)
    {
        ObservableCollection<Word> words = [];

        if (intrinsic)
            words.Add(BuildWord("[intrinsic] "));

        words.Add(BuildWord(method.Name, intrinsic ? Color.IntrinsicFunction : Color.Function));


        if (method.GetParameters().Length > 0)
        {
            words.Add(BuildWord(" ("));

            foreach (ParameterInfo param in method.GetParameters()[..^1])
            {
                words.Add(BuildWord(param.Name, Color.LocalValue));
                words.Add(BuildWord(": "));

                foreach (Word word in Type(param.ParameterType.GetTypeInfo(), false, true, true).Words)
                    words.Add(word);

                words.Add(BuildWord(", "));
            }

            words.Add(BuildWord(method.GetParameters()[0].Name, Color.LocalValue));
            words.Add(BuildWord(": "));

            foreach (Word word in Type(method.GetParameters()[0].ParameterType.GetTypeInfo(), false, true, true).Words)
                words.Add(word);

            words.Add(BuildWord(")"));
        }

        words.Add(BuildWord(": "));
        foreach (Word word in Type(method.ReturnType.GetTypeInfo(), false, true, true).Words)
            words.Add(word);

        return new()
        {
            Words = words,
            IconResourceName = "Method"
        };
    }

    /// <summary>
    /// Generates a tooltip for a namespace.
    /// </summary>
    /// <param name="name">The name of the namespace.</param>
    /// <returns>The generated tooltip.</returns>
    public static Tooltip Namespace(string name)
    {
        return new()
        {
            IconResourceName = "Namespace",
            Words =
            [
                BuildWord(name, Color.Namespace),
            ]
        };
    }

    static readonly Dictionary<Type, string> builtinTypeAliases = new()
    {
        [typeof(sbyte)] = "int8",
        [typeof(byte)] = "uint8",
        [typeof(short)] = "int16",
        [typeof(ushort)] = "uint16",
        [typeof(int)] = "int32",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint32",
        [typeof(uint)] = "uint",
        [typeof(long)] = "int64",
        [typeof(ulong)] = "uint64",
        [typeof(float)] = "float32",
        [typeof(double)] = "float64",
        [typeof(decimal)] = "decimal",
        [typeof(nint)] = "native",
        [typeof(nuint)] = "unative",
        [typeof(bool)] = "bool",
        [typeof(string)] = "string",
        [typeof(char)] = "char",
        [typeof(void)] = "null",
        [typeof(object)] = "object"
    };

    /// <summary>
    /// Generates a tooltip for a type.
    /// </summary>
    /// <param name="type">The type to generate a tooltip for.</param>
    /// <param name="showBaseType">Wheter to include the base type in the tooltip.</param>
    /// <param name="omitNamespace">If <see langword="true"/>, omits the namespace of the type from the tooltip.</param>
    /// <param name="noModifiers">If <see langword="true"/>, hides all type modifiers from the tooltip.</param>
    /// <param name="doc">Optional XML documentation of the type.</param>
    /// <param name="ignoreBuiltinAliases">Wheter to ignore built-in type aliases.</param>
    /// <returns>Returns the generated tooltip.</returns>
    public static Tooltip Type(TypeInfo type, bool showBaseType, bool omitNamespace = false, bool noModifiers = false, Tooltip doc = null, bool ignoreBuiltinAliases = false)
    {
        try
        {
            ObservableCollection<Word> words = [];

            if (!ignoreBuiltinAliases && builtinTypeAliases.ContainsKey(type.AsType()))
            {
                words.Add(new()
                {
                    Fragment = new() { Color = Color.Word },
                    Text = builtinTypeAliases[type.AsType()]
                });

                return new()
                {
                    Words = words,
                    IconResourceName = ResourceNameForType(type)
                };
            }

            if (!noModifiers)
            {
                if (type.IsEnum)
                    words.Add(BuildWord("enum ", Color.Word));
                else if (type.IsInterface)
                    words.Add(BuildWord("template ", Color.Word));
                else if (type.IsSealed && type.IsAbstract)
                    words.Add(BuildWord("module ", Color.Word));
                else
                {
                    if (!type.IsSealed)
                        words.Add(BuildWord("open ", Color.Word));

                    if (type.IsAbstract)
                        words.Add(BuildWord("abstract ", Color.Word));

                    words.Add(BuildWord(type.IsValueType ? "val type " : "ref type ", Color.Word));
                }
            }

            words.Add(BuildWord(omitNamespace ? "" : (type.Namespace + ".")));
            words.Add(BuildWord(type.Name.Split('`')[0], ColorForType(type)));

            if (type.GenericTypeArguments.Length > 0)
            {
                words.Add(BuildWord("["));

                if (type.GenericTypeArguments.Length > 1)
                {
                    foreach (Type param in type.GenericTypeArguments[..^1])
                    {
                        foreach (Word word in Type(param.GetTypeInfo(), false, omitNamespace, true).Words)
                            words.Add(word);

                        words.Add(BuildWord(", "));
                    }
                }

                foreach (Word word in Type(type.GenericTypeArguments.Last().GetTypeInfo(), false, omitNamespace, true).Words)
                    words.Add(word);

                words.Add(BuildWord("]"));
            }

            if (type.IsByRef)
                words.Add(BuildWord("&"));

            else if (showBaseType && type.BaseType != null)
            {
                if (type.BaseType != typeof(ValueType) && type.BaseType != typeof(object) || type.ImplementedInterfaces.Count() > 0)
                    words.Add(BuildWord(": ", Color.Default));

                if (type.BaseType != typeof(ValueType) && type.BaseType != typeof(object))
                {
                    foreach (Word word in Type(type.BaseType.GetTypeInfo(), false, omitNamespace, true).Words)
                        words.Add(word);

                    if (type.ImplementedInterfaces.Count() >= 1)
                        words.Add(BuildWord(", "));
                }

                if (type.ImplementedInterfaces.Count() > 1)
                {
                    foreach (Type t in type.ImplementedInterfaces.ToArray()[..^1])
                    {
                        foreach (Word word in Type(t.GetTypeInfo(), false, omitNamespace, true).Words)
                            words.Add(word);

                        words.Add(BuildWord(", "));
                    }
                }

                if (type.ImplementedInterfaces.Count() > 0)
                {
                    foreach (Word word in Type(type.ImplementedInterfaces.Last().GetTypeInfo(), false, omitNamespace, true).Words)
                        words.Add(word);
                }
            }

            if (doc != null)
            {
                words.Add(BuildWord(Environment.NewLine));

                foreach (Word word in doc.Words)
                    words.Add(word);
            }

            return new()
            {
                Words = words,
                IconResourceName = ResourceNameForType(type)
            };
        }
        catch (Exception)
        {
            return new();
        }
    }

    private static Word BuildWord(string text, Color color = Color.Default) => new()
    {
        Fragment = new() { Color = color },
        Text = $"{text}"
    };

    /// <summary>
    /// Returns the appropriate <see cref="Color"/> for the specified type.
    /// </summary>
    /// <param name="type">The type to get a color for.</param>
    /// <returns>The appropriate color.</returns>
    public static Color ColorForType(TypeInfo type)
    {
        if (type.IsInterface)
            return Color.TemplateType;

        if (type.IsEnum)
            return Color.EnumType;

        if (type.IsAbstract && type.IsSealed) // That's how static types are represented in the CLR...
            return Color.Module;

        if (type.IsValueType)
            return Color.ValueType;

        return Color.ReferenceType;
    }

    private static string ResourceNameForType(TypeInfo type)
    {
        string part1 = ColorForType(type) switch
        {
            Color.TemplateType => "Interface",
            Color.Module => "Module",
            Color.ValueType => "ValueType",
            Color.EnumType => "Enumeration",
            _ => "Class"
        };

        string part2 = "Public";

        if (type.IsNestedFamORAssem)
            part2 = "Internal";

        if (type.IsNestedPrivate)
            part2 = "Private";

        return $"{part1}{part2}";
    }
}