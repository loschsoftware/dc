using System.IO;
using System.Xml.Serialization;

namespace LoschScript.Text.FragmentStore;

/// <summary>
/// Provides functionality for serializing and deserializing <see cref="FileFragment"/> objects.
/// </summary>
public static class FragmentSerializer
{
    /// <summary>
    /// Serializes the specified fragment list as XML.
    /// </summary>
    /// <param name="filePath">The file path of the serialized file.</param>
    /// <param name="flist">The instance to serialize.</param>
    public static void Serialize(string filePath, FileFragment flist)
    {
        using StreamWriter sw = new(filePath);
        XmlSerializer xmls = new(typeof(FileFragment));

        xmls.Serialize(sw, flist);
    }

    /// <summary>
    /// Deserializes the specified XML file as a fragment list.
    /// </summary>
    /// <param name="filePath">The path to the serialized XML file.</param>
    /// <returns>Returns an instance of <see cref="FileFragment"/> corresponding to the serialized XML file.</returns>
    public static FileFragment Deserialize(string filePath)
    {
        using StreamReader sr = new(filePath);
        XmlSerializer xmls = new(typeof(FileFragment));

        return (FileFragment)xmls.Deserialize(sr);
    }
}