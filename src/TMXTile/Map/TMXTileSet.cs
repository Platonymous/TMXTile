using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "tileset")]
    public class TMXTileset
    {
        [XmlIgnore]
        private XmlSerializer Serializer { get; } = new XmlSerializer(typeof(TMXTileset));

        [XmlElement(ElementName = "image")]
        public TMXImage Image { get; set; }
        [XmlAttribute(AttributeName = "firstgid")]
        public ulong Firstgid { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "tilewidth")]
        public int Tilewidth { get; set; }
        [XmlAttribute(AttributeName = "tileheight")]
        public int Tileheight { get; set; }
        [XmlAttribute(AttributeName = "tilecount")]
        public int Tilecount { get; set; }
        [XmlAttribute(AttributeName = "columns")]
        public int Columns { get; set; }
        [XmlElement(ElementName = "grid")]
        public TMXGrid Grid { get; set; }
        [XmlElement(ElementName = "tile")]
        public List<TMXTileSetTile> Tiles { get; set; }
        [XmlArrayItem("property", typeof(TMXProperty))]
        [XmlArray("properties")]
        public TMXProperty[] Properties { get; set; }

        [XmlAttribute(AttributeName = "source")]
        public string Source
        {
            get
            {
                return null;
            }
            set
            {
                using (Stream stream = new FileStream(Path.Combine(TMXParser.CurrentDirectory, value), FileMode.Open))
                    if (Serializer.Deserialize(stream) is TMXTileset ts)
                    {
                        Image = ts.Image;
                        Name = ts.Name;
                        Tilewidth = ts.Tilewidth;
                        Tileheight = ts.Tileheight;
                        Tilecount = ts.Tilecount;
                        Columns = ts.Columns;
                        Grid = ts.Grid;
                        Tiles = ts.Tiles;
                        Properties = ts.Properties;
                    }

            }
        }
    }
}
