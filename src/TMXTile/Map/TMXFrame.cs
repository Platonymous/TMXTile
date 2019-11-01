
using System;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "frame")]
    public class TMXFrame
    {
        [XmlAttribute(AttributeName = "tileid")]
        public uint TileId { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public uint Duration { get; set; }
    }
}
