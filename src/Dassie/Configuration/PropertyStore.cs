using Dassie.Configuration.Macros;
using Dassie.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Dassie.Configuration;

/// <summary>
/// Provides functionality for evaluating and storing configuration properties.
/// </summary>
public class PropertyStore
{
    /// <summary>
    /// Creates a new property store without any registered properties.
    /// </summary>
    public static PropertyStore Empty => new();

    /// <summary>
    /// Creates an instance of <see cref="PropertyStore"/> with a default set of registered properties.
    /// </summary>
    public static PropertyStore Default
    {
        get
        {
            MacroParser mp = new();
            PropertyStore ps = new(ExtensionLoader.Properties, mp);
            mp.BindPropertyResolver(ps.Get);
            return ps;
        }
    }

    private IEnumerable<Property> _propertyDefs;
    internal IEnumerable<Property> Properties => _propertyDefs;

    private readonly MacroParser _parser;
    private readonly Dictionary<string, object> _uninstantiatedProperties;
    private readonly Dictionary<string, object> _instantiatedProperties = [];
    private readonly Dictionary<string, (int Line, int Column)> _propertyLocationMapping;

    private PropertyStore()
    {
        _propertyDefs = [];
        _uninstantiatedProperties = [];
        _propertyLocationMapping = [];
    }

    internal PropertyStore(IEnumerable<Property> defs, MacroParser parser, Dictionary<string, object> uninstantiatedValues = null)
    {
        _propertyDefs = defs;
        _parser = parser;
        _uninstantiatedProperties = uninstantiatedValues ?? [];
    }

    internal bool IsPropertySet(string name) =>
        _instantiatedProperties.ContainsKey(name) || _uninstantiatedProperties.ContainsKey(name);

    // Brittle, but works for all relevant cases;
    // can be updated if necessary in the future
    internal static Type GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        if (collectionType.IsGenericType)
            return collectionType.GetGenericArguments()[0];

        return collectionType;
    }

    private Property GetPropertyDef(string key)
    {
        return _propertyDefs.FirstOrDefault(p => p.Name == key);
    }

    private void AddPropertyDef(Property prop)
    {
        _propertyDefs = _propertyDefs.Concat([prop]);
    }

    private static bool IsCollectionType(Type type)
    {
        return type != null &&
            (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)));
    }
    
    private static object ConvertCollectionType(object items, Type target, Type elemType)
    {
        MethodInfo castMethod = typeof(Enumerable)
            .GetMethod(nameof(Enumerable.Cast))
            .MakeGenericMethod(elemType);

        object casted = castMethod.Invoke(null, [items]);

        if (target.IsArray)
        {
            MethodInfo toArrayMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.ToArray))
                .MakeGenericMethod(elemType);

            return toArrayMethod.Invoke(null, [casted]);
        }

        if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(List<>))
        {
            MethodInfo toListMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.ToList))
                .MakeGenericMethod(elemType);

            return toListMethod.Invoke(null, [casted]);
        }

        throw new InvalidOperationException("Unsupported collection type.");
    }

    private (object Result, bool CanBeCached) DeserializeObject(XElement element, Type type)
    {
        Type targetType = ResolveConcreteType(type, element);

        if (targetType == typeof(object))
            return (element, false);

        IEnumerable<XAttribute> attributes = element.Attributes();
        IEnumerable<XElement> childElements = element.Elements();
        PropertyInfo[] props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        HashSet<XAttribute> consumedAttributes = [];
        HashSet<XElement> consumedElements = [];

        List<Property> defs = [];
        Dictionary<string, object> rawValues = [];

        foreach (PropertyInfo prop in props)
        {
            if (prop.GetCustomAttribute<XmlIgnoreAttribute>() != null)
                continue;

            string propName = prop.Name;
            Type propType = prop.PropertyType;

            object defaultValue = prop.GetCustomAttribute<DefaultValueAttribute>()?.Value;
            defs.Add(new Property(propName, propType, defaultValue));

            if (prop.GetCustomAttribute<XmlAnyAttributeAttribute>() != null)
            {
                List<XAttribute> remaining = attributes.Where(a => !consumedAttributes.Contains(a)).ToList();
                remaining.ForEach(a => consumedAttributes.Add(a));
                rawValues[propName] = ConvertAnyAttributes(remaining, propType);
                continue;
            }

            if (prop.GetCustomAttribute<XmlAnyElementAttribute>() != null)
            {
                List<XElement> remaining = childElements.Where(e => !consumedElements.Contains(e)).ToList();
                remaining.ForEach(e => consumedElements.Add(e));
                rawValues[propName] = ConvertAnyElements(remaining, propType);
                continue;
            }

            if (prop.GetCustomAttribute<XmlTextAttribute>() != null)
            {
                rawValues[propName] = string.Concat(element.Nodes().OfType<XText>().Select(t => t.Value));
                continue;
            }

            if (prop.GetCustomAttribute<XmlArrayAttribute>() is XmlArrayAttribute xmlArray)
            {
                string containerName = string.IsNullOrWhiteSpace(xmlArray.ElementName) ? propName : xmlArray.ElementName;
                XElement container = childElements.FirstOrDefault(e => e.Name.LocalName == containerName);

                if (container != null)
                {
                    consumedElements.Add(container);

                    Type elemType = GetElementType(propType);
                    List<object> values = [];

                    foreach (XElement item in container.Elements())
                    {
                        consumedElements.Add(item);
                        values.Add(ReadRawXmlValue(item, elemType));
                    }

                    rawValues[propName] = values.ToArray();
                }

                continue;
            }

            XmlElementAttribute[] elementAttributes = prop.GetCustomAttributes<XmlElementAttribute>().ToArray();
            if (elementAttributes.Length > 0)
            {
                List<string> names = elementAttributes
                    .Select(a => string.IsNullOrWhiteSpace(a.ElementName) ? propName : a.ElementName)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (propType.IsArray || (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    Type elemType = GetElementType(propType);
                    List<XElement> matches = childElements.Where(e => names.Contains(e.Name.LocalName)).ToList();

                    foreach (XElement match in matches)
                        consumedElements.Add(match);

                    if (matches.Count > 0)
                        rawValues[propName] = matches.Select(m => ReadRawXmlValue(m, elemType)).ToArray();
                }
                else
                {
                    XElement match = childElements.FirstOrDefault(e => names.Contains(e.Name.LocalName));

                    if (match != null)
                    {
                        consumedElements.Add(match);

                        Type hintedType = elementAttributes
                            .FirstOrDefault(a => (string.IsNullOrWhiteSpace(a.ElementName) ? propName : a.ElementName) == match.Name.LocalName)
                            ?.Type ?? propType;

                        rawValues[propName] = ReadRawXmlValue(match, hintedType);
                    }
                }

                continue;
            }

            if (prop.GetCustomAttribute<XmlAttributeAttribute>() is XmlAttributeAttribute xmlAttribute)
            {
                string attributeName = string.IsNullOrWhiteSpace(xmlAttribute.AttributeName) ? propName : xmlAttribute.AttributeName;
                XAttribute attr = attributes.FirstOrDefault(a => a.Name.LocalName == attributeName);

                if (attr != null)
                {
                    consumedAttributes.Add(attr);
                    rawValues[propName] = attr.Value;
                }

                continue;
            }

            XElement defaultElement = childElements.FirstOrDefault(e => e.Name.LocalName == propName);
            if (defaultElement != null)
            {
                consumedElements.Add(defaultElement);
                rawValues[propName] = ReadRawXmlValue(defaultElement, propType);
            }
        }

        PropertyStore ps = new(defs, _parser, rawValues);

        object instance;
        ConstructorInfo storeCtor = targetType.GetConstructor([typeof(PropertyStore)]);

        if (storeCtor != null)
            instance = storeCtor.Invoke([ps]);
        else
            instance = Activator.CreateInstance(targetType);

        foreach (PropertyInfo prop in props.Where(p => p.CanWrite))
        {
            if (!rawValues.ContainsKey(prop.Name))
                continue;

            try
            {
                prop.SetValue(instance, ps.Get(prop.Name));
            }
            catch { }
        }

        return (instance, false);
    }

    private static Type ResolveConcreteType(Type hintType, XElement element)
    {
        if (hintType == null)
            return typeof(object);

        if (!hintType.IsAbstract && !hintType.IsInterface && hintType != typeof(object))
            return hintType;

        string elementName = element.Name.LocalName;
        IEnumerable<Type> candidates;

        if (hintType == typeof(object))
        {
            candidates = typeof(PropertyStore).Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface);
        }
        else
        {
            candidates = hintType.Assembly
                .GetTypes()
                .Where(t => hintType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
        }

        Type match = candidates.FirstOrDefault(t =>
        {
            XmlRootAttribute root = t.GetCustomAttribute<XmlRootAttribute>();
            string rootName = root?.ElementName;

            if (!string.IsNullOrWhiteSpace(rootName) && rootName == elementName)
                return true;

            return t.Name == elementName;
        });

        return match ?? hintType;
    }

    private static object ReadRawXmlValue(XElement element, Type expectedType)
    {
        if (expectedType == typeof(string) || expectedType.IsPrimitive || expectedType.IsEnum)
            return element.Value;

        if (expectedType == typeof(XmlElement))
            return ToXmlElement(element);

        return element;
    }

    private static object ConvertAnyElements(List<XElement> elements, Type targetType)
    {
        List<XmlElement> xmlElements = elements.Select(ToXmlElement).ToList();

        if (targetType.IsArray)
            return xmlElements.ToArray();

        return xmlElements;
    }

    private static object ConvertAnyAttributes(List<XAttribute> attributes, Type targetType)
    {
        List<XmlAttribute> xmlAttributes = attributes.Select(ToXmlAttribute).ToList();

        if (targetType.IsArray)
            return xmlAttributes.ToArray();

        return xmlAttributes;
    }

    private static XmlElement ToXmlElement(XElement element)
    {
        XmlDocument doc = new();
        using XmlReader reader = element.CreateReader();
        XmlNode node = doc.ReadNode(reader);
        return (XmlElement)node;
    }

    private static XmlAttribute ToXmlAttribute(XAttribute attribute)
    {
        XmlDocument doc = new();
        XmlAttribute xmlAttribute = doc.CreateAttribute(attribute.Name.LocalName);
        xmlAttribute.Value = attribute.Value;
        return xmlAttribute;
    }

    private (object Result, bool CanBeCached) Evaluate(object raw, Type hintType)
    {
        if (raw == null)
            return (raw, true);

        Type type = raw.GetType();

        if (raw is string str)
        {
            return _parser.Expand(str);
        }

        if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
        {
            Type elemType = GetElementType(type);
            List<object> items = [];
            bool canBeCached = true;

            foreach (object element in (IEnumerable)raw)
            {
                (object val, bool cached) = Evaluate(element, elemType);
                items.Add(val);
                canBeCached &= cached;
            }

            return (ConvertCollectionType(items, type, elemType), canBeCached);
        }

        if (raw is XElement elem)
        {
            return DeserializeObject(elem, hintType);
        }

        return (raw, false);
    }

    private static object ValidateAndConvert(string name, object raw, Type type)
    {
        try
        {
            if (IsCollectionType(raw?.GetType()) && IsCollectionType(type))
                return ConvertCollectionType(raw, type, GetElementType(type));

            if (raw == null || !raw.GetType().IsAssignableTo(typeof(IConvertible)))
                return raw; // Hope and pray

            if (raw is string enumField && type.IsEnum)
                return Enum.Parse(type, enumField);

            return Convert.ChangeType(raw, type);
        }
        catch (Exception ex)
        {
            throw new MalformedPropertyValueException(name, ex);
        }
    }

    /// <summary>
    /// Evaluates a property.
    /// </summary>
    /// <param name="key">The key of the property to evaluate.</param>
    /// <returns>The value of the requested property.</returns>
    public object Get(string key)
    {
        Property prop = GetPropertyDef(key);

        if (_instantiatedProperties.TryGetValue(key, out object cached))
            return cached;

        if (_uninstantiatedProperties.TryGetValue(key, out object pval))
        {
            (object value, bool canBeCached) = Evaluate(pval, prop.Type);
            
            if (prop != null)
            {
                value = ValidateAndConvert(key, value, prop.Type);

                if (canBeCached || prop.CanBeCached)
                {
                    if (_instantiatedProperties.TryAdd(key, value))
                        _uninstantiatedProperties.Remove(key);
                }
            }

            return value;
        }

        if (prop != null)
        {
            object defaultVal = prop.Default;

            if (defaultVal == null && prop.Type.IsValueType)
                return Activator.CreateInstance(prop.Type);

            return defaultVal;
        }

        return null;
    }

    /// <summary>
    /// Sets the value of a property.
    /// </summary>
    /// <param name="key">The name of the property to set.</param>
    /// <param name="value">The value to set the property represented by <paramref name="key"/> to.</param>
    public void Set(string key, object value)
    {
        if (GetPropertyDef(key) is null)
            AddPropertyDef(new(key, value?.GetType() ?? typeof(object)));

        _uninstantiatedProperties.Remove(key);
        _instantiatedProperties.Remove(key);

        // Overridden values are always treated as cacheable
        _instantiatedProperties.Add(key, value);
    }
}