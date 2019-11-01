using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "data")]
    public class TMXData
    {
        [XmlAttribute(AttributeName = "encoding")]
        public string Encoding { 
            get{
                switch(TMXParser.CurrentEncoding){
                    case DataEncodingType.XML: return null;
                    case DataEncodingType.CSV: return "csv";
                    default: return "base64";
                }
            }
            set{
                switch(value){
                    case null: TMXParser.CurrentEncoding = DataEncodingType.XML;break;
                    case "csv": TMXParser.CurrentEncoding = DataEncodingType.CSV; break;
                    default: TMXParser.CurrentEncoding = DataEncodingType.Base64; break;
                }
            }
        }

        [XmlAttribute(AttributeName = "compression")]
        public string Compression {
            get
            {
                if (TMXParser.CurrentEncoding == DataEncodingType.GZip)
                    return "gzip";
                else if (TMXParser.CurrentEncoding == DataEncodingType.ZLib)
                    return "zlib";

                return null;
            }
            set
            {
                if (value != null && value != "")
                {
                    if (value == "zlib")
                        TMXParser.CurrentEncoding = DataEncodingType.ZLib;
                    else if (value == "gzip")
                        TMXParser.CurrentEncoding = DataEncodingType.GZip;
                }
            }
         }

        [XmlElement(ElementName = "chunk")]
        public List<TMXChunk> Chunks { get; set; }

        [XmlElement(ElementName = "tile")]
        public List<TMXTile> RawTiles
        {
            get
            {
                if (TMXParser.CurrentEncoding == DataEncodingType.XML)
                    return Tiles;
                else
                    return null;
            }
            set
            {
                Tiles = new List<TMXTile>();

                foreach (TMXTile tile in value)
                    Tiles.Add(tile);
            }
        }

        [XmlIgnore]
        public List<TMXTile> Tiles { get; set; }

        [XmlText()]
        public string Raw
        {
            get => encode();
            set => decode(value);
        }

        private void decode(string dataString)
        {
            if (TMXParser.CurrentEncoding == DataEncodingType.XML || dataString == null || dataString == "")
                return;

            Tiles = new List<TMXTile>();

            if (TMXParser.CurrentEncoding == DataEncodingType.CSV)
            {
                Tiles.AddRange(dataString.Split(',').Select<string, TMXTile>(s => new TMXTile() { Gid = uint.Parse(s) }));
                return;
            }

            byte[] data = Convert.FromBase64String(dataString);

            if (TMXParser.CurrentEncoding == DataEncodingType.GZip)
                using (MemoryStream decompressed = new MemoryStream())
                using (MemoryStream compressed = new MemoryStream(data))
                using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Decompress))
                {
                    gzip.CopyTo(decompressed);
                    data = decompressed.ToArray();
                }

            if (TMXParser.CurrentEncoding == DataEncodingType.ZLib)
                throw (new InvalidDataException("ZLib compression is not supported."));

            for (int i = 0; i < data.Length; i += 4)
                Tiles.Add(new TMXTile() { Gid = BitConverter.ToUInt32(data, i) });
        }
        private string encode()
        {
            if (TMXParser.CurrentEncoding == DataEncodingType.XML || Tiles == null || Tiles.Count == 0)
                return null;

            if (TMXParser.CurrentEncoding == DataEncodingType.CSV)
                return string.Join(",", Tiles.Select<TMXTile, string>(t => t.Gid.ToString()));

            List<byte> byteList = new List<byte>();

            foreach (TMXTile tile in Tiles)
                byteList.AddRange(BitConverter.GetBytes(tile.Gid));

            byte[] data = byteList.ToArray();

            if (TMXParser.CurrentEncoding == DataEncodingType.GZip)
                using (MemoryStream decompressed = new MemoryStream(data))
                using (MemoryStream compressed = new MemoryStream())
                using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Compress))
                {
                    decompressed.CopyTo(gzip);
                    gzip.Close();
                    data = compressed.ToArray();
                }

            if (TMXParser.CurrentEncoding == DataEncodingType.ZLib)
                throw (new InvalidDataException("ZLib compression is not supported."));

            return Convert.ToBase64String(data.ToArray());
        }

    }

}
