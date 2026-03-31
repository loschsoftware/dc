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
/// Represents an XML-backed configuration object.
/// </summary>
public abstract class ConfigObject
{
    /// <summary>
    /// The <see cref="PropertyStore"/> associated with the configuration object.
    /// </summary>
    internal PropertyStore Store { get; }

    private IEnumerable<XObject> GetNodeFor(PropertyInfo prop, Property meta)
    {
        static object ValueOrNode(object val)
        {
            if (val is ConfigObject obj)
                return obj.Node;

            return val;
        }

        bool isExplicit = prop.GetCustomAttribute<ExplicitAttribute>() != null;
        object defaultVal = meta?.Default ?? prop.GetCustomAttribute<DefaultValueAttribute>()?.Value;

        object val = prop.GetValue(this);

        if (val == null || (!isExplicit && val.Equals(defaultVal)))
            return [];

        val ??= defaultVal;

        if (prop.GetCustomAttribute<XmlElementAttribute>() is XmlElementAttribute xmlElem)
        {
            string elemName = prop.Name;
            if (!string.IsNullOrEmpty(xmlElem.ElementName))
                elemName = xmlElem.ElementName;

            return [new XElement(elemName, ValueOrNode(val))];
        }

        if (prop.GetCustomAttribute<XmlAttributeAttribute>() is XmlAttributeAttribute xmlAttrib)
        {
            string attribName = prop.Name;
            if (!string.IsNullOrEmpty(xmlAttrib.AttributeName))
                attribName = xmlAttrib.AttributeName;

            return [new XAttribute(attribName, val)];
        }

        if (prop.GetCustomAttribute<XmlTextAttribute>() is XmlTextAttribute)
            return [new XText(val.ToString())];

        if (prop.GetCustomAttribute<XmlArrayAttribute>() is XmlArrayAttribute xmlArray)
        {
            XElement arrayElem = new(xmlArray.ElementName);

            foreach (object elem in (IEnumerable)val)
                arrayElem.Add(elem is ConfigObject elemObj ? elemObj.Node : new XElement(elem.GetType().Name, elem));

            return [arrayElem];
        }

        if (prop.GetCustomAttribute<XmlAnyElementAttribute>() is XmlAnyElementAttribute xmlAnyElement)
        {
            List<XElement> elems = [];

            foreach (XmlNode node in (IEnumerable)val)
            {
                using XmlNodeReader reader = new(node);
                elems.Add(XElement.Load(reader));
            }

            return elems;
        }

        if (prop.GetCustomAttribute<XmlAnyAttributeAttribute>() is XmlAnyAttributeAttribute xmlAnyAttribute)
        {
            List<XAttribute> attribs = [];

            foreach (XmlAttribute attrib in (IEnumerable)val)
                attribs.Add(new(attrib.Name, attrib.Value));

            return attribs;
        }

        return [];
    }

    /// <summary>
    /// Generates an XML node representing this configuration object.
    /// </summary>
    public virtual XNode Node
    {
        get
        {
            Type type = GetType();

            string elemName = type.Name;
            if (type.GetCustomAttribute<XmlRootAttribute>() is XmlRootAttribute xmlRoot && !string.IsNullOrEmpty(xmlRoot.ElementName))
                elemName = xmlRoot.ElementName;

            XElement elem = new(elemName);

            foreach (PropertyInfo prop in type.GetProperties().Where(p => p.Name != nameof(Node) && p.GetIndexParameters().Length == 0))
            {
                Property meta = Store.Properties.FirstOrDefault(p => p.Name == prop.Name);
                foreach (XObject obj in GetNodeFor(prop, meta))
                    elem.Add(obj);
            }

            return elem;
        }
    }

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
        => (T)(Store.Get(name) ?? default(T));

    /// <summary>
    /// Sets the specified property.
    /// </summary>
    /// <param name="name">The name of the property to set.</param>
    /// <param name="value">The value to set the property represented by <paramref name="name"/> to.</param>
    protected void Set(string name, object value)
        => Store.Set(name, value);
}