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
                    case DataEncodingType.Base64: return "base64";
                    default: return "base64";
                }
            }
            set{
                switch(value){
                    case null: TMXParser.CurrentEncoding = DataEncodingType.XML;break;
                    case "csv": TMXParser.CurrentEncoding = DataEncodingType.CSV; break;
                    case "base64": TMXParser.CurrentEncoding = DataEncodingType.Base64; break;
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
                else if (TMXParser.CurrentEncoding == DataEncodingType.ZStd)
                    return "zstd";

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
                    else if (value == "zstd")
                        TMXParser.CurrentEncoding = DataEncodingType.ZStd;
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
            set => Tiles = value;
        }

        [XmlIgnore]
        public List<TMXTile> Tiles { get; set; }

        [XmlText()]
        public string Raw
        {
            get => encode();
            set => decode(value);
        }

        private void copyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
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
            {
                byte[] stripped = new byte[data.Length - 6];
                for (int i = 2; i < data.Length - 4; i++)
                    stripped[i-2] = data[i];

                using (MemoryStream decompressed = new MemoryStream())
                using (MemoryStream compressed = new MemoryStream(stripped))
                using (DeflateStream zlib = new DeflateStream(compressed, CompressionMode.Decompress))
                {
                    zlib.CopyTo(decompressed);
                    data = decompressed.ToArray();
                }
            }

            if (TMXParser.CurrentEncoding == DataEncodingType.ZStd)
                throw (new InvalidDataException("ZStandard compression is not supported."));

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


            if (TMXParser.CurrentEncoding == DataEncodingType.ZStd)
                throw (new InvalidDataException("ZStandard compression is not supported."));

            return Convert.ToBase64String(data.ToArray());
        }

    }

}
