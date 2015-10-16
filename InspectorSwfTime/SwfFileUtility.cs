using System;
using System.IO;
using Ionic.Zlib;

namespace InspectorSwfTime
{
    public static class SwfFileUtility
    {
        public static void CopyDecompressed(string inputPath, string outputPath)
        {
            if (inputPath.ToLowerInvariant() == outputPath.ToLowerInvariant())
            {
                throw new Exception("The input and output files must be different.");
            }

            using (var swfStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
            {
                const int headerSize = 8;

                var read = 0;
                var header = new byte[headerSize];
                do
                {
                    read = swfStream.Read(header, read, headerSize - read);
                } while (read < headerSize);
                switch (header[0])
                {
                    case 0x46:
                        // already uncompressed - just copy the file
                        using (var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            outStream.Write(header, 0, headerSize);
                            swfStream.CopyTo(outStream);
                        }
                        break;
                    case 0x43:
                        using (var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            header[0] = 0x46;
                            outStream.Write(header, 0, headerSize);
                            using (var decompressed = new ZlibStream(swfStream, CompressionMode.Decompress))
                            {
                                decompressed.CopyTo(outStream);
                            }
                        }
                        break;
                    case 0x5A:
                        throw new Exception("LZMA compression is not supported (yet).");
                    default:
                        throw new Exception(string.Format("Unexpected compression type: 0x{0:x2}", header[0]));
                }
            }
        }
    }
}
