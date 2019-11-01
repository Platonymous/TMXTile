using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "image")]
    public class TMXImage
    {
        [XmlAttribute(AttributeName = "source")]
        public virtual string Source { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }
    }

}
