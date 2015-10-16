using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using InspectorSwfTime.Swf.Tags;

namespace InspectorSwfTime.Swf
{
    public class SwfInfo
    {
        public SwfHeader Header { get; protected set; }
        public SwfTag[] Tags { get; protected set; }

        public SwfInfo(Stream swfStream)
        {
            if (!BitConverter.IsLittleEndian)
            {
                throw new SwfParseException("Big endian systems are not (currently) supported.");
            }

            if (!swfStream.CanRead)
            {
                throw new SwfParseException("Unable to read from stream.");
            }

            // parse the initial header
            Header = new SwfHeader(swfStream);

            // add any necessary deompression
            Stream decompressed;
            switch (Header.Signature)
            {
                case SwfHeaderSignature.FWS:
                    decompressed = swfStream;
                    break;
                case SwfHeaderSignature.CWS:
                    decompressed = new ZlibStream(swfStream, CompressionMode.Decompress);
                    break;
                case SwfHeaderSignature.ZWS:
                    throw new SwfParseException("LZMA compression is not (yet) supported.");
                default:
                    throw new SwfParseException("Unsuppored compression type: " + Header.Signature);
            }

            using (var swfBitStream = new SwfBitStream(decompressed))
            {
                // finish parsing the header - why can't the whole header be uncompressed?
                Header.FinishParsing(swfBitStream);

                // parse out all the tags
                var tags = new List<SwfTag>();
                SwfTag tag;
                while (swfBitStream.BytesRead < Header.FileLength)
                {
                    tag = SwfTag.ParseTag(swfBitStream);
                    if (tag.Code != 0)
                    {
                        tags.Add(tag);
                    }
                    else
                    {
                        break;
                    }
                }
                Tags = tags.ToArray();
            }
        }
    }
}
