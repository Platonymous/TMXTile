using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TMXTile
{
    public enum DataEncodingType{
        XML,
        CSV,
        Base64,
        GZip,
        ZLib,
        ZStd
        
    }
    public class TMXParser
    {
        private XmlSerializer Serializer { get; } = new XmlSerializer(typeof(TMXMap));
        internal static DataEncodingType CurrentEncoding {get;set;} = DataEncodingType.XML;
        internal static string CurrentDirectory { get; set; } = "";
        public TMXMap Parse(Stream stream, string path = "")
        {
            CurrentEncoding = DataEncodingType.XML;

            if (path == "" && stream is FileStream fs)
                path = fs.Name;

            CurrentDirectory = Path.GetDirectoryName(path);

            if (Serializer.Deserialize(stream) is TMXMap map)
                return map;

            return null;
        }

        public TMXMap Parse(XmlReader reader)
        {
            CurrentEncoding = DataEncodingType.XML;
            if (Serializer.Deserialize(reader) is TMXMap map)
                return map;

            return null;
        }

        public TMXMap Parse(string path)
        {
            CurrentEncoding = DataEncodingType.XML;
            using (Stream stream = new FileStream(path, FileMode.Open))
                return Parse(stream, path);
        }

        public void Export(TMXMap map, Stream stream, DataEncodingType dataEncodingType = DataEncodingType.XML)
        {
            CurrentEncoding = dataEncodingType;
            Serializer.Serialize(stream, map);
        }

        public void Export(TMXMap map, string path, DataEncodingType dataEncodingType = DataEncodingType.XML)
        {
            using (Stream stream = new FileStream(path, FileMode.Create))
                Export(map, stream, dataEncodingType);
        }

        public void Export(TMXMap map, XmlWriter writer, DataEncodingType dataEncodingType = DataEncodingType.XML)
        {
            CurrentEncoding = dataEncodingType;
            Serializer.Serialize(writer, map);
        }

    }
   
}
