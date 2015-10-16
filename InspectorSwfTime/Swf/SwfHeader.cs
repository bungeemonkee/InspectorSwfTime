using System;
using System.IO;

namespace InspectorSwfTime.Swf
{
    public class SwfHeader
    {
        public SwfHeaderSignature Signature { get; protected set; }
        public uint Version { get; protected set; }
        public uint FileLength { get; protected set; }
        public SwfRect FrameSize { get; protected set; }
        public ushort FrameRate { get; protected set; }
        public ushort FrameCount { get; protected set; }

        public double FramesPerSecond { get { return FrameRate * 8.8D; } }
        public uint LengthInSeconds { get { return (uint)Math.Ceiling(FrameCount / FramesPerSecond); } }

        internal SwfHeader(Stream swfData)
        {
            var header = new byte[8];
            if (swfData.Read(header, 0, 8) != 8)
            {
                throw new SwfParseException("Unable to read enough bytes (8) to parse the Swf header.");
            }

            switch (header[0])
            {
                case (byte)SwfHeaderSignature.FWS:
                    Signature = SwfHeaderSignature.FWS;
                    break;
                case (byte)SwfHeaderSignature.CWS:
                    Signature = SwfHeaderSignature.CWS;
                    break;
                case (byte)SwfHeaderSignature.ZWS:
                    Signature = SwfHeaderSignature.ZWS;
                    break;
                default:
                    throw new SwfParseException(string.Format("Unable to parse Swf signature, unexpected first byte: 0x{0:x2}", header[0]));
            }

            if (header[1] != 0x57)
            {
                throw new SwfParseException(string.Format("Unable to parse Swf signature, unexpected second byte: 0x{0:x2}", header[1]));
            }

            if (header[2] != 0x53)
            {
                throw new SwfParseException(string.Format("Unable to parse Swf signature, unexpected third byte: 0x{0:x2}", header[2]));
            }

            Version = header[3];
            FileLength = BitConverter.ToUInt32(header, 4);
        }

        internal void FinishParsing(SwfBitStream swfStream)
        {
            FrameSize = new SwfRect(swfStream);
            FrameRate = swfStream.GetUInt16();
            FrameCount = swfStream.GetUInt16();
        }

    }
}
