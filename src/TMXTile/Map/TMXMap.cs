using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace TMXTile
{
    [XmlRoot(ElementName = "map")]
    public class TMXMap
    {
        [XmlArrayItem("property", typeof(TMXProperty))]
        [XmlArray("properties")]
        public TMXProperty[] Properties { get; set; }

        [XmlElement(ElementName = "tileset")]
        public List<TMXTileset> Tilesets { get; set; }

        [XmlElement(ElementName = "layer")]
        public List<TMXLayer> Layers { get; set; }

        [XmlElement(ElementName = "imagelayer")]
        public List<TMXImageLayer> ImageLayers { get; set; }

        [XmlElement(ElementName = "objectgroup")]
        public List<TMXObjectgroup> Objectgroups { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "tiledversion")]
        public string Tiledversion { get; set; }

        [XmlAttribute(AttributeName = "orientation")]
        public string Orientation { get; set; }

        [XmlAttribute(AttributeName = "renderorder")]
        public string Renderorder { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

        [XmlAttribute(AttributeName = "tilewidth")]
        public int Tilewidth { get; set; }

        [XmlAttribute(AttributeName = "tileheight")]
        public int Tileheight { get; set; }

        [XmlIgnore()]
        internal bool Infinite { get; set; }

        [XmlAttribute(AttributeName = "infinite")]
        public int InfiniteAsInt {
            get => (Infinite ? 1 : 0);
            set => Infinite = value == 1;
        }

        [XmlIgnore()]
        public TMXColor Backgroundcolor { get; set; }

        [XmlAttribute(AttributeName = "backgroundcolor")]
        public string BackgroundcolorString
        {
            get => Backgroundcolor?.ToString();
            set => Backgroundcolor = TMXColor.FromString(value);
        }

        [XmlAttribute(AttributeName = "nextlayerid")]
        public int Nextlayerid { get; set; }

        [XmlAttribute(AttributeName = "nextobjectid")]
        public int Nextobjectid { get; set; }
    }
}