using System.Xml;

namespace Dassie.Configuration.Analysis;

public static class XmlLocationService
{
    public class Location
    {
        public static Location Invalid => new(-1, -1, -1);

        public Location(int row, int column, int length)
        {
            Row = row;
            Column = column;
            Length = length;
        }

        public int Row { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }
    }

    public static Location GetElementLocation(string filePath, string elementName)
    {
        try
        {
            using XmlReader reader = new XmlTextReader(filePath);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == elementName)
                {
                    int row = ((IXmlLineInfo)reader).LineNumber;
                    int col = ((IXmlLineInfo)reader).LinePosition;
                    int length = reader.Name.Length;

                    return new(row, col, length);
                }
            }
        }
        catch
        {
            return Location.Invalid;
        }

        return Location.Invalid;
    }
}