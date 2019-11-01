using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "property")]
    public class TMXProperty
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string StringValue { get; set; }

        [XmlIgnore()]
        public int IntValue
        {
            get => int.Parse(StringValue);
        }

        [XmlIgnore()]
        public bool BoolValue
        {
            get => StringValue == "true";
        }

        [XmlIgnore()]
        public TMXColor ColorValue
        {
            get => TMXColor.FromString(StringValue);
        }

        [XmlIgnore()]
        public float FloatValue
        {
            get => float.Parse(StringValue);
        }
    }
}
