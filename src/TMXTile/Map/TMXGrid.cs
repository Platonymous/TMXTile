using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "grid")]
    public class TMXGrid
    {
        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }
    }
}
