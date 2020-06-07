using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "chunk")]
    public class TMXChunk
    {
        [XmlAttribute(AttributeName = "x")]
        public int X { get; set; }

        [XmlAttribute(AttributeName = "y")]
        public int Y { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

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
