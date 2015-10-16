using System.IO;

namespace InspectorSwfTime.Swf.Tags
{
    public sealed class GenericTag : SwfTag
    {
        public byte[] Data { get; private set; }

        internal GenericTag(ushort code, ulong length, SwfBitStream swfStream)
            : base(code, length)
        {
            Data = swfStream.GetBytes((uint)length);
        }

        public SwfBitStream GetBitStreamFromData()
        {
            return new SwfBitStream(new MemoryStream(Data, false));
        }
    }
}
