using Dassie.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dassie.Model;

internal record DassieType
{
    internal DassieType() { }

    protected DassieType(DassieType source)
    {
        Name = source.Name;
        Namespace = source.Namespace;
        Kind = source.Kind;
        BaseType = source.BaseType;
        Interfaces = source.Interfaces;
        Methods = source.Methods;
        Fields = source.Fields;
        TypeAttributes = source.TypeAttributes;
        Parameters = source.Parameters;
    }

    // TODO: Cache commonly used types in a table (Type->DassieType)

    public static DassieType FromType(Type type)
    {
        if (type == null)
            return null;

        if (type.IsGenericParameter)
        {
            return new GenericTypeParameter()
            {
                Name = type.Name
            };
        }

        TypeKind kind = TypeKind.ReferenceType;

        if (type.IsValueType)
            kind = TypeKind.ValueType;
        else if (type.IsInterface)
            kind = TypeKind.Template;
        else if (type.IsSealed && type.IsAbstract)
            kind = TypeKind.Module;

        List<TypeParameter> parameters = null;

        if (type.IsGenericType)
        {
            parameters = [];

            foreach (Type genericParam in type.GetGenericTypeDefinition().GetGenericArguments())
            {
                parameters.Add(new GenericTypeParameter()
                {
                    Name = genericParam.Name
                });
            }
        }

        DassieType dassieType = new()
        {
            Name = type.Name,
            Namespace = type.Namespace,
            Kind = kind,
            BaseType = FromType(type.BaseType),
            Interfaces = type.GetInterfaces().Select(FromType),
            Methods = type.GetMethods(BindingFlags.NonPublic).Select(DassieMethod.FromMethodInfo),
            Fields = type.GetFields(BindingFlags.NonPublic).Select(DassieField.FromFieldInfo),
            TypeAttributes = type.GetCustomAttributesData(),
            Parameters = parameters
        };

        if (dassieType.TypeAttributes.Any(t => t.AttributeType == typeof(AliasAttribute)))
        {
            CustomAttributeData cad = dassieType.TypeAttributes.First(t => t.AttributeType == typeof(AliasAttribute));
            DassieType aliasedType = FromType((Type)cad.ConstructorArguments[0].Value);

            if (dassieType.TypeAttributes.Any(t => t.AttributeType == typeof(NewTypeAttribute)))
            {
                dassieType = new NewType(dassieType)
                {
                    AliasedType = aliasedType
                };
            }
            else
            {
                dassieType = new AliasType(dassieType)
                {
                    AliasedType = aliasedType
                };
            }
        }

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            return new ParameterizedDassieType(dassieType)
            {
                GenericTypeArguments = type.GetGenericArguments().Select(FromType).ToArray()
            };
        }

        return dassieType;
    }

    public string Name { get; set; }
    public string Namespace { get; set; }
    public string FullName => $"{Namespace}.{Name}";

    public TypeKind Kind { get; set; }
    public DassieType BaseType { get; set; }
    public IEnumerable<DassieType> Interfaces { get; set; }

    public IEnumerable<DassieMethod> Methods { get; set; }
    public IEnumerable<DassieField> Fields { get; set; }

    public IEnumerable<CustomAttributeData> TypeAttributes { get; set; }

    public IEnumerable<TypeParameter> Parameters { get; set; }

    public override string ToString()
    {
        StringBuilder paramList = new();

        if (Parameters?.Any() == true)
        {
            paramList.Append('[');

            foreach (TypeParameter param in Parameters)
            {
                paramList.Append(param.ToString());

                if (param != Parameters.Last())
                    paramList.Append(", ");
            }

            paramList.Append(']');
        }

        return $"{FullName}{paramList}";
    }
}