using Dassie.Meta;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dassie.Helpers;

internal static class InheritanceHelpers
{
    public static List<MetaFieldInfo> GetInheritedFields(Type baseType)
    {
        List<MetaFieldInfo> fields = [];

        Type currentType = baseType;
        while (currentType != null)
        {
            foreach (FieldInfo field in currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                object constant = null;

                try
                {
                    constant = field.GetRawConstantValue();
                }
                catch (NotSupportedException) { }

                fields.Add(new MetaFieldInfo
                {
                    Builder = field,
                    ConstantValue = constant,
                    Name = field.Name
                });
            }

            currentType = currentType.BaseType;
        }

        return fields;
    }
}