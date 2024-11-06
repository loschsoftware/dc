using Dassie.Configuration;
using Dassie.Errors;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Dassie.Validation;

/// <summary>
/// Provides functionality for validating project files.
/// </summary>
internal static class ConfigValidation
{
    static ConfigValidation()
    {
        foreach (PropertyInfo prop in typeof(DassieConfig).GetProperties())
        {
            XmlElementAttribute attribute = prop.GetCustomAttribute<XmlElementAttribute>();
            if (attribute != null)
            {
                if (!string.IsNullOrEmpty(attribute.ElementName))
                    _validProperties.Add(attribute.ElementName);
                else
                    _validProperties.Add(prop.Name);
            }

            XmlArrayAttribute arrayAttribute = prop.GetCustomAttribute<XmlArrayAttribute>();
            if (arrayAttribute != null)
            {
                if (!string.IsNullOrEmpty(arrayAttribute.ElementName))
                    _validProperties.Add(arrayAttribute.ElementName);
                else
                    _validProperties.Add(prop.Name);
            }
        }
    }

    private static List<string> _validProperties = [];

    /// <summary>
    /// Checks if the specified project config file is valid.
    /// </summary>
    /// <param name="path">The path to the config file.</param>
    /// <returns>A list of errors in the specified project file.</returns>
    public static List<ErrorInfo> Validate(string path)
    {
        List<ErrorInfo> errors = [];
        XDocument doc = null;

        try
        {
            doc = XDocument.Load(path, LoadOptions.SetLineInfo);
        }
        catch (XmlException xmlEx)
        {
            EmitErrorMessage(
                0, 0, 0,
                DS0001_SyntaxError,
                $"Malformed document: {xmlEx.Message}",
                ProjectConfigurationFileName);

            return errors;
        }

        foreach (XElement element in doc.Element(XName.Get("DassieConfig")).Elements())
        {
            if (!_validProperties.Contains(element.Name.LocalName))
            {
                errors.Add(new ErrorInfo()
                {
                    CodePosition = (((IXmlLineInfo)element).LineNumber, ((IXmlLineInfo)element).LinePosition),
                    Length = element.Name.LocalName.Length + 2,
                    ErrorCode = DS0089_InvalidDSConfigProperty,
                    Severity = Severity.Warning,
                    ErrorMessage = $"Invalid property '{element.Name.LocalName}'.",
                    File = ProjectConfigurationFileName
                });
            }
        }

        return errors;
    }
}