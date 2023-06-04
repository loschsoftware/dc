using System;
using System.Collections.Generic;
using System.Linq;

namespace LoschScript.Runtime;

/// <summary>
/// Represents a union value, which can be of multiple types.
/// </summary>
public struct UnionValue : IEquatable<UnionValue>, IDisposable
{
    /// <summary>
    /// Creates a new instance of the <see cref="UnionValue"/> structure.
    /// </summary>
    /// <param name="initialValue">The initial value of the union.</param>
    /// <param name="allowedTypes">The allowed types of the union.</param>
    public UnionValue(object initialValue, params Type[] allowedTypes)
    {
        AllowedTypes = allowedTypes;
        Value = initialValue;
    }

    /// <summary>
    /// The allowed types of the union value.
    /// </summary>
    public Type[] AllowedTypes { get; }

    private object _value;

    /// <summary>
    /// The value of the union.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public object Value
    {
        get => _value;

        set
        {
            if (!AllowedTypes.Contains(value.GetType()))
                throw new InvalidOperationException($"A value of type '{value.GetType()}' is not supported by the union type.");

            _value = value;
        }
    }

    /// <summary>
    /// Tries to set the value of the variable to the specified object.
    /// </summary>
    /// <param name="value">The new value.</param>
    /// <returns>Returns <see langword="true"/> if the operation was successful.</returns>
    public bool TrySetValue(object value)
    {
        if (!AllowedTypes.Contains(value.GetType()))
            return false;

        Value = value;
        return true;
    }

    /// <summary>
    /// Formats the union value as a string.
    /// </summary>
    /// <returns>The string representation of the union value.</returns>
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    /// Compares the current instance of <see cref="UnionValue"/> with another.
    /// </summary>
    /// <param name="other">The union value to compare.</param>
    /// <returns>true, if the specified union value is equal to the current one.</returns>
    public bool Equals(UnionValue other) => Value == other.Value && AllowedTypes == other.AllowedTypes;

    /// <summary>
    /// Generates a hash code for the current instance of <see cref="UnionValue"/>.
    /// </summary>
    /// <returns>The generated hash code.</returns>
    public override int GetHashCode()
    {
        int hashCode = 236200514;
        hashCode = hashCode * -1521134295 + EqualityComparer<Type[]>.Default.GetHashCode(AllowedTypes);
        hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
        return hashCode;
    }

    /// <summary>
    /// Compares the current instance of <see cref="UnionValue"/> to any object.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    /// <returns>true, if the specified object is equal to the current instance.</returns>
    public override bool Equals(object obj) => base.Equals(obj);

    /// <summary>
    /// If the current value is disposable, disposes it.
    /// </summary>
    public void Dispose()
    {
        if (Value is IDisposable d)
            d.Dispose();
    }

    /// <summary>
    /// Compares two instances of <see cref="UnionValue"/>.
    /// </summary>
    /// <param name="a">The first instance to compare.</param>
    /// <param name="b">The second instance to compare.</param>
    /// <returns>true, if the first instance is equal to the second instance.</returns>
    public static bool operator ==(UnionValue a, UnionValue b) => a.Equals(b);

    /// <summary>
    /// Compares two instances of <see cref="UnionValue"/>.
    /// </summary>
    /// <param name="a">The first instance to compare.</param>
    /// <param name="b">The second instance to compare.</param>
    /// <returns>true, if the first instance is not equal to the second instance.</returns>
    public static bool operator !=(UnionValue a, UnionValue b) => !a.Equals(b);
}