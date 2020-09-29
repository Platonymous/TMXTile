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

        public static ICompressionHelper CompressionHelper { get; set; } = new CompressionHelper();
        public TMXMap Parse(Stream stream, string path = "")
        {
            TMXMap parsedMap = null;
            CurrentEncoding = DataEncodingType.XML;
            if (path == "" && stream is FileStream fs)
                path = fs.Name;

            CurrentDirectory = Path.GetDirectoryName(path);

            if (Serializer.Deserialize(stream) is TMXMap map)
                parsedMap = map;

            return parsedMap;
        }

        public TMXMap Parse(XmlReader reader)
        {
            TMXMap parsedMap = null;

            CurrentEncoding = DataEncodingType.XML;
            if (Serializer.Deserialize(reader) is TMXMap map)
                parsedMap = map;

            return parsedMap;
        }

        public TMXMap Parse(string path)
        {
            CurrentEncoding = DataEncodingType.XML;
            using (Stream stream = new FileStream(path, FileMode.Open))
                return Parse(stream, path);
        }

        public void Export(TMXMap map, Stream stream, DataEncodingType dataEncodingType = DataEncodingType.XML)
        {
            if (dataEncodingType == DataEncodingType.CSV)
                PrepareCSVFormatting(map);
            
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            CurrentEncoding = dataEncodingType;
            Serializer.Serialize(stream, map, ns);
        }

        public void Export(TMXMap map, string path, DataEncodingType dataEncodingType = DataEncodingType.XML)
        {
            using (Stream stream = new FileStream(path, FileMode.Create))
                Export(map, stream, dataEncodingType);
        }

        public void Export(TMXMap map, XmlWriter writer, DataEncodingType dataEncodingType = DataEncodingType.XML)
        {
            if (dataEncodingType == DataEncodingType.CSV)
                PrepareCSVFormatting(map);
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            CurrentEncoding = dataEncodingType;
            Serializer.Serialize(writer, map, ns);
        }

        internal void PrepareCSVFormatting(TMXMap map)
        {
            foreach(TMXLayer layer in map.Layers)
                layer.Data.LayerWidth = (int) layer.Width;
        }

    }
   
}
