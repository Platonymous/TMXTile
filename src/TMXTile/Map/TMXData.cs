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
        [XmlIgnore]
        public int LayerWidth { get; set; } = -1;

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

        /// <summary>This property is for (de)serialization; code should use the normalized <see cref="Tiles"/> instead.</summary>
        [XmlElement(ElementName = "tile")]
        public List<TMXTile> XmlTiles
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
                // This field only applies in XML mode, but on Android it will be called for other encodings anyway with an empty list.
                if (TMXParser.CurrentEncoding == DataEncodingType.XML)
                    Tiles = value;
            }
        }

        /// <summary>This property is for (de)serialization; code should use the normalized <see cref="Tiles"/> instead.</summary>
        [XmlText]
        public string BodyTiles
        {
            get => encode();
            set => decode(value);
        }

        [XmlIgnore]
        public List<TMXTile> Tiles { get; private set; } = new List<TMXTile>();

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
                data = TMXParser.CompressionHelper.Decompress(DataEncodingType.GZip, data);


            if (TMXParser.CurrentEncoding == DataEncodingType.ZLib)
                data = TMXParser.CompressionHelper.Decompress(DataEncodingType.ZLib, data);


            if (TMXParser.CurrentEncoding == DataEncodingType.ZStd)
                data = TMXParser.CompressionHelper.Decompress(DataEncodingType.ZStd, data);

            for (int i = 0; i < data.Length; i += 4)
                Tiles.Add(new TMXTile() { Gid = BitConverter.ToUInt32(data, i) });
        }
        private string encode()
        {
            if (TMXParser.CurrentEncoding == DataEncodingType.XML || Tiles == null || Tiles.Count == 0)
                return null;

            if (TMXParser.CurrentEncoding == DataEncodingType.CSV)
            {
                string csv = Environment.NewLine;
                for (int i = 0, c = Tiles.Count; i < c; i++)
                {
                    csv += Tiles[i].Gid.ToString();
                    if (i != c - 1)
                        csv += ",";
                    if ((i + 1) % LayerWidth == 0)
                        csv += Environment.NewLine;
                }

                return csv;

                return string.Join(",", Tiles.Select<TMXTile, string>(t => t.Gid.ToString()));
            }
            List<byte> byteList = new List<byte>();

            foreach (TMXTile tile in Tiles)
                byteList.AddRange(BitConverter.GetBytes(tile.Gid));

            byte[] data = byteList.ToArray();

            if (TMXParser.CurrentEncoding == DataEncodingType.GZip)
                data = TMXParser.CompressionHelper.Compress(DataEncodingType.GZip, data);

            if (TMXParser.CurrentEncoding == DataEncodingType.ZLib)
                data = TMXParser.CompressionHelper.Compress(DataEncodingType.ZLib, data);


            if (TMXParser.CurrentEncoding == DataEncodingType.ZStd)
                data = TMXParser.CompressionHelper.Compress(DataEncodingType.ZStd, data);

            return Convert.ToBase64String(data.ToArray());
        }

    }

}
