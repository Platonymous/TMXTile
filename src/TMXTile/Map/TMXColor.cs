
using System;
using System.Xml.Serialization;

namespace TMXTile
{
    public class TMXColor
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public byte A { get; set; } = 255;

        public static TMXColor FromString(string value)
        {
            if (value == null || value.Length < 7 || !value.StartsWith("#"))
                return null;

            if (value.Length == 7)
            {
                byte r = (byte)(Convert.ToUInt32(value.Substring(1, 2), 16));
                byte g = (byte)(Convert.ToUInt32(value.Substring(3, 2), 16));
                byte b = (byte)(Convert.ToUInt32(value.Substring(5, 2), 16));
                return new TMXColor() { R = r, G = g, B = b };
            }else if(value.Length == 9)
            {
                byte a = (byte)(Convert.ToUInt32(value.Substring(1, 2), 16));
                byte r = (byte)(Convert.ToUInt32(value.Substring(3, 2), 16));
                byte g = (byte)(Convert.ToUInt32(value.Substring(5, 2), 16));
                byte b = (byte)(Convert.ToUInt32(value.Substring(7, 2), 16));

                return new TMXColor() { R = r, G = g, B = b, A = a };
            }

            return null;
        }

        public override string ToString()
        {
            return "#"+ (A != 255 ? A.ToString("X2") : "") + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }
        
    }
}
