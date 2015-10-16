
using System;

namespace InspectorSwfTime.Swf
{
    public class SwfRect
    {
        public long XMin { get; protected set; }
        public long XMax { get; protected set; }
        public long YMin { get; protected set; }
        public long YMax { get; protected set; }

        public long Top { get { return (long)Math.Floor(YMin / 20D); } }
        public long Left { get { return (long)Math.Floor(XMin / 20D); } }
        public long Right { get { return (long)Math.Ceiling(XMax / 20D); } }
        public long Bottom { get { return (long)Math.Ceiling(YMax / 20D); } }
        public long Height { get { return Bottom - Top; } }
        public long Width { get { return Right - Left; } }

        public SwfRect(SwfBitStream swfStream)
        {
            var fieldLength = (uint)swfStream.GetUnsignedBitValue(5);
            XMin = swfStream.GetSignedBitValue(fieldLength);
            XMax = swfStream.GetSignedBitValue(fieldLength);
            YMin = swfStream.GetSignedBitValue(fieldLength);
            YMax = swfStream.GetSignedBitValue(fieldLength);
        }
    }
}
