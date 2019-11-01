using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "tile")]
    public class TMXTile
    {
        [XmlAttribute(AttributeName = "gid")]
        public uint Gid { get; set; } = 0;
    }
}
