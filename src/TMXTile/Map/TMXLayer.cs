using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "layer")]
    public class TMXLayer
    {
        [XmlArrayItem("property", typeof(TMXProperty))]
        [XmlArray("properties")]
        public TMXProperty[] Properties { get; set; }

        [XmlElement(ElementName = "data")]
        public TMXData Data { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public float Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public float Height { get; set; }

        [XmlIgnore()]
        public bool Visible { get; set; } = true;

        [XmlAttribute(AttributeName = "visible")]
        internal int VisibleAsInt {
            get => (Visible? 1 : 0);
            set => Visible = value == 1;
        }

        [XmlAttribute(AttributeName = "opacity")]
        public float Opacity { get; set; } = 1.0f;

        [XmlAttribute(AttributeName = "offsetx")]
        public float Offsetx { get; set; }

        [XmlAttribute(AttributeName = "offsety")]
        public float Offsety { get; set; }
    }
}
