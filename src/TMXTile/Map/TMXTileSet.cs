using System.Collections.Generic;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "tileset")]
    public class TMXTileset
    {
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
    }
}
