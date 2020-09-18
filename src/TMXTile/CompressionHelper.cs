using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXTile
{
    public interface ICompressionHelper
    {
        byte[] Decompress(DataEncodingType type, byte[] data);

        byte[] Compress(DataEncodingType type, byte[] data);
    }

    internal class CompressionHelper : ICompressionHelper
    {
        public byte[] Decompress(DataEncodingType type, byte[] data)
        {
            byte[] decompressedData = new byte[0];

            if (type ==  DataEncodingType.GZip)
                using (MemoryStream decompressed = new MemoryStream())
                using (MemoryStream compressed = new MemoryStream(data))
                using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Decompress))
                {
                    gzip.CopyTo(decompressed);
                    decompressedData = decompressed.ToArray();
                }
            else if (type == DataEncodingType.ZLib)
            {
                byte[] stripped = new byte[data.Length - 6];
                for (int i = 2; i < data.Length - 4; i++)
                    stripped[i - 2] = data[i];

                using (MemoryStream decompressed = new MemoryStream())
                using (MemoryStream compressed = new MemoryStream(stripped))
                using (DeflateStream zlib = new DeflateStream(compressed, CompressionMode.Decompress))
                {
                    zlib.CopyTo(decompressed);
                    decompressedData = decompressed.ToArray();
                }
            }
            else if (type == DataEncodingType.ZStd)
                throw (new InvalidDataException("ZStandard compression is not supported."));

            return decompressedData;
        }

        public byte[] Compress(DataEncodingType type, byte[] data)
        {
            byte[] compressedData = new byte[0];
            
            if (type == DataEncodingType.GZip)
                using (MemoryStream decompressed = new MemoryStream(data))
                using (MemoryStream compressed = new MemoryStream())
                using (GZipStream gzip = new GZipStream(compressed, CompressionMode.Compress))
                {
                    decompressed.CopyTo(gzip);
                    gzip.Close();
                    compressedData = compressed.ToArray();
                }
            else if (type == DataEncodingType.ZLib)
                throw (new InvalidDataException("ZLib compression is not supported."));
            else if (type == DataEncodingType.ZStd)
                throw (new InvalidDataException("ZStandard compression is not supported."));

            return compressedData;
        }
    }
}
