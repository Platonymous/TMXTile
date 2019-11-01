using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "object")]
    public class TMXObject
    {
        [XmlArrayItem("property", typeof(TMXProperty))]
        [XmlArray("properties")]
        public TMXProperty[] Properties { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "x")]
        public float X { get; set; }
        [XmlAttribute(AttributeName = "y")]
        public float Y { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public float Width { get; set; }
        [XmlAttribute(AttributeName = "height")]
        public float Height { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "rotation")]
        public float Rotation { get; set; }
    }
}
