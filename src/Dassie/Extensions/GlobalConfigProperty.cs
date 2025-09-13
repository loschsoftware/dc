using Dassie.Configuration.Global;
using System;
using System.Collections.Generic;

namespace Dassie.Extensions;

/// <summary>
/// Represents a global configuration property defined by an extension.
/// </summary>
public abstract class GlobalConfigProperty
{
    internal string ExtensionIdentifier { get; set; }

    /// <summary>
    /// The name of the property. Properties are automatically namespaced using the identifier of the containing extension.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The data type of the property.
    /// </summary>
    public abstract GlobalConfigDataType Type { get; }

    /// <summary>
    /// The default value of the property.
    /// </summary>
    public virtual object DefaultValue => GetDefaultValue();

    private object GetDefaultValue()
    {
        Type propType = Type.BaseType switch
        {
            GlobalConfigBaseType.Boolean => typeof(bool),
            GlobalConfigBaseType.Integer => typeof(int),
            GlobalConfigBaseType.Real => typeof(double),
            _ => typeof(string)
        };

        if (Type.IsList)
            propType = typeof(List<>).MakeGenericType(propType);

        if (!propType.IsValueType)
            return null;

        return Activator.CreateInstance(propType);
    }

    /// <summary>
    /// Gets the current value of the global property.
    /// </summary>
    /// <returns>The current value of the property.</returns>
    /// <exception cref="InvalidOperationException"/>
    public object GetValue()
    {
        string key = $"{ExtensionIdentifier}.{Name}";
        if (ExtensionIdentifier == null || !GlobalConfigManager.Properties.TryGetValue(key, out (GlobalConfigDataType Type, object Value) prop))
            throw new InvalidOperationException($"The property '{Name}' has not been registered.");

        return prop.Value;
    }

    /// <summary>
    /// Sets the value of the global property.
    /// </summary>
    /// <param name="value">The new value of the property.</param>
    /// <exception cref="InvalidOperationException"/>
    public void SetValue(object value)
    {
        string key = $"{ExtensionIdentifier}.{Name}";
        if (ExtensionIdentifier == null || !GlobalConfigManager.Properties.ContainsKey(key))
            throw new InvalidOperationException($"The property '{Name}' has not been registered.");

        GlobalConfigManager.Set(key, Type, value);
    }
}