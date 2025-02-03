using Dassie.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Dassie.Helpers;

internal static class InheritanceHelpers
{
    public static List<MetaFieldInfo> GetInheritedFields(Type baseType)
    {
        List<MetaFieldInfo> fields = [];

        Type currentType = baseType;
        while (currentType != null)
        {
            if (currentType == typeof(TypeBuilder) || currentType.FullName.StartsWith("System.Reflection.Emit.TypeBuilderInstantiation"))
            {
                TypeContext tc = Context.Types.First(t => t.FullName == currentType.FullName);
                foreach (MetaFieldInfo mfi in tc.Fields)
                    fields.Add(mfi);
            }
            else
            {
                try
                {
                    foreach (FieldInfo field in currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        object constant = null;

                        try
                        {
                            constant = field.GetRawConstantValue();
                        }
                        catch (Exception) { }

                        fields.Add(new MetaFieldInfo
                        {
                            Builder = field,
                            ConstantValue = constant,
                            Name = field.Name
                        });
                    }
                }
                catch { }
            }

            currentType = currentType.BaseType;
        }

        return fields;
    }
}