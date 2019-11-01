using System;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "tile")]
    public class TMXTileSetTile
    {
        [XmlArrayItem("property", typeof(TMXProperty))]
        [XmlArray("properties")]
        public TMXProperty[] Properties { get; set; }

        [XmlArrayItem("frame", typeof(TMXFrame))]
        [XmlArray("animation")]
        public TMXFrame[] Animations { get; set; }

        [XmlElement(ElementName = "image")]
        public virtual TMXImage Image { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }
    }

}
