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

    internal string Key => $"{ExtensionIdentifier}.{Name}";
    internal bool IsRegistered => ExtensionIdentifier != null && GlobalConfigManager.Properties.ContainsKey(Key);

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
        if (!IsRegistered)
            throw new InvalidOperationException($"The property '{Name}' has not been registered.");

        return GlobalConfigManager.Properties[Key].Value;
    }

    /// <summary>
    /// Sets the value of the global property.
    /// </summary>
    /// <param name="value">The new value of the property.</param>
    /// <exception cref="InvalidOperationException"/>
    public void SetValue(object value)
    {
        if (!IsRegistered)
            throw new InvalidOperationException($"The property '{Name}' has not been registered.");

        foreach (var validator in Validators ?? [])
        {
            if (!validator(value))
                return;
        }

        GlobalConfigManager.Set(Key, Type, value);
    }

    /// <summary>
    /// Gets the validation delegates that are invoked whenever the property is set.
    /// </summary>
    /// <remarks>
    /// Each validator receives the proposed new property value and returns whether the value is valid.
    /// Validators are invoked in order. If a validator returns <see langword="false"/>, the set operation
    /// is aborted and no remaining validators are invoked.
    /// <para/>
    /// Validators are responsible for emitting their own validation error messages.
    /// </remarks>
    public virtual Func<object, bool>[] Validators => [];
}