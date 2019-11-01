
using System;
using System.Xml.Serialization;

namespace TMXTile
{
    public class TMXColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static TMXColor FromString(string value)
        {
            if (value == null)
                return null;

            byte r = (byte)(Convert.ToUInt32(value.Substring(1, 2), 16));
            byte g = (byte)(Convert.ToUInt32(value.Substring(3, 2), 16));
            byte b = (byte)(Convert.ToUInt32(value.Substring(5, 2), 16));
            return new TMXColor() { R = r, G = g, B = b };
        }

        public override string ToString()
        {
            return "#" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }
        
    }
}
