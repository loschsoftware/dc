namespace Dassie.Configuration;

/// <summary>
/// Represents an XML-backed configuration object.
/// </summary>
public abstract class ConfigObject
{
    /// <summary>
    /// The <see cref="PropertyStore"/> associated with the configuration object.
    /// </summary>
    protected PropertyStore Store { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigObject"/> type based on the specified <see cref="PropertyStore"/>.
    /// </summary>
    /// <param name="store"></param>
    protected ConfigObject(PropertyStore store)
    {
        Store = store ?? PropertyStore.Empty;
    }

    /// <summary>
    /// Evaluates a property of the configuration object.
    /// </summary>
    /// <typeparam name="T">The type of the property to evaluate.</typeparam>
    /// <param name="name">The name of the property to evaluate.</param>
    /// <returns>The value of the specified property.</returns>
    protected T Get<T>(string name)
        => (T)Store.Get(name);

    /// <summary>
    /// Sets the specified property.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property represented by <paramref name="name"/> to.</param>
    protected void Set(string name, object value)
        => Store.Set(name, value);
}